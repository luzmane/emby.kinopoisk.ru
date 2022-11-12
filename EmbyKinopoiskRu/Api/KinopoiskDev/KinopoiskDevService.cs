using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EmbyKinopoiskRu.Api.KinopoiskDev.Model;
using EmbyKinopoiskRu.Api.KinopoiskDev.Model.Movie;
using EmbyKinopoiskRu.Api.KinopoiskDev.Model.Person;
using EmbyKinopoiskRu.Api.KinopoiskDev.Model.Season;
using EmbyKinopoiskRu.Helper;
using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Providers;
using MediaBrowser.Model.Serialization;

namespace EmbyKinopoiskRu.Api.KinopoiskDev
{
    public class KinopoiskDevService : IKinopoiskRuService
    {
        private readonly ILogger _log;
        private readonly KinopoiskDevApi _api;

        internal KinopoiskDevService(ILogManager logManager, IHttpClient httpClient, IJsonSerializer jsonSerializer)
        {
            _log = logManager.GetLogger(GetType().Name);
            _api = new KinopoiskDevApi(_log, httpClient, jsonSerializer);
        }

        #region MovieProvider
        public async Task<MetadataResult<Movie>> GetMetadata(MovieInfo info, CancellationToken cancellationToken)
        {
            MetadataResult<Movie> result = new()
            {
                ResultLanguage = "ru"
            };

            if (string.IsNullOrWhiteSpace(Plugin.Instance?.Configuration.GetToken()))
            {
                _log.Warn($"The Token for {Plugin.PluginName} is empty");
                return result;
            }

            if (info.HasProviderId(Plugin.PluginName))
            {
                string movieId = info.GetProviderId(Plugin.PluginName);
                if (!string.IsNullOrWhiteSpace(movieId))
                {
                    _log.Info($"Searching movie by movie id '{movieId}'");
                    KpMovie? movie = await _api.GetMovieById(movieId, cancellationToken);
                    if (movie != null)
                    {
                        result.Item = CreateMovieFromKpMovie(movie);
                        await UpdatePersonsList(result, movie.Persons, cancellationToken);
                        result.HasMetadata = true;
                        return result;
                    }
                    _log.Info($"Movie by movie id '{movieId}' not found");
                }
            }

            _log.Info($"Searching movie by name {info.Name}");
            KpSearchResult<KpMovie> movies = await _api.GetMoviesByMetadata(info.Name, info.Year, info.IsAutomated, cancellationToken);
            if (movies.Docs.Count != 1)
            {
                _log.Error($"Found {movies.Docs.Count} movies, skipping movie update");
                return result;
            }
            result.Item = CreateMovieFromKpMovie(movies.Docs[0]);
            await UpdatePersonsList(result, movies.Docs[0].Persons, cancellationToken);
            result.HasMetadata = true;
            return result;
        }
        public async Task<IEnumerable<RemoteSearchResult>> GetSearchResults(MovieInfo searchInfo, CancellationToken cancellationToken)
        {
            List<RemoteSearchResult> result = new();

            if (string.IsNullOrWhiteSpace(Plugin.Instance?.Configuration.GetToken()))
            {
                _log.Warn($"The Token for {Plugin.PluginName} is empty");
                return result;
            }

            if (searchInfo.HasProviderId(Plugin.PluginName))
            {
                string movieId = searchInfo.GetProviderId(Plugin.PluginName);
                if (!string.IsNullOrWhiteSpace(movieId))
                {
                    _log.Info($"Searching movie by id '{movieId}'");
                    KpMovie? movie = await _api.GetMovieById(movieId, cancellationToken);
                    if (movie != null)
                    {
                        string imageUrl = (movie.Poster?.PreviewUrl ?? movie.Poster?.Url) ?? string.Empty;
                        RemoteSearchResult item = new()
                        {
                            Name = movie.Name,
                            ImageUrl = imageUrl,
                            SearchProviderName = Plugin.PluginName,
                            ProductionYear = movie.Year,
                            Overview = movie.Description,
                        };
                        item.SetProviderId(Plugin.PluginName, movieId);
                        result.Add(item);
                        return result;
                    }
                    _log.Info($"Movie by id '{movieId}' not found");
                }
            }

            _log.Info($"Searching movies by available metadata");
            KpSearchResult<KpMovie> movies = await _api.GetMoviesByMetadata(searchInfo.Name, searchInfo.Year, searchInfo.IsAutomated, cancellationToken);
            foreach (KpMovie movie in movies.Docs)
            {
                string imageUrl = (movie.Poster?.PreviewUrl ?? movie.Poster?.Url) ?? string.Empty;
                RemoteSearchResult item = new()
                {
                    Name = movie.Name,
                    ImageUrl = imageUrl,
                    SearchProviderName = Plugin.PluginName,
                    ProductionYear = movie.Year,
                    Overview = movie.Description,
                };
                item.SetProviderId(Plugin.PluginName, movie.Id.ToString(CultureInfo.InvariantCulture));
                result.Add(item);
            }
            _log.Info($"By name '{searchInfo.Name}' found {result.Count} movies");
            return result;
        }
        private Movie CreateMovieFromKpMovie(KpMovie movie)
        {
            _log.Info($"Movie '{movie.Name}' with {Plugin.PluginName} id '{movie.Id}' found");

            string movieId = movie.Id.ToString(CultureInfo.InvariantCulture);
            Movie toReturn = new()
            {
                CommunityRating = movie.Rating?.Kp,
                CriticRating = movie.Rating?.FilmCritics * 10,
                EndDate = GetEndDate(movie.ReleaseYears),
                ExternalId = movieId,
                Name = movie.Name,
                OfficialRating = movie.RatingMpaa,
                OriginalTitle = movie.AlternativeName,
                Overview = movie.Description,
                PremiereDate = KpHelper.GetPremierDate(movie.Premiere),
                ProductionLocations = movie.Countries?.Select(i => i.Name).ToArray(),
                ProductionYear = movie.Year,
                Size = movie.MovieLength,
                SortName = !string.IsNullOrWhiteSpace(movie.EnName) ? movie.EnName : movie.Name,
                Tagline = movie.Slogan
            };

            toReturn.SetProviderId(Plugin.PluginName, movieId);

            if (!string.IsNullOrWhiteSpace(movie.ExternalId?.Imdb))
            {
                toReturn.ProviderIds.Add(MetadataProviders.Imdb.ToString(), movie.ExternalId.Imdb);
            }

            if (!string.IsNullOrWhiteSpace(movie.ExternalId?.Tmdb))
            {
                toReturn.ProviderIds.Add(MetadataProviders.Tmdb.ToString(), movie.ExternalId.Tmdb);
            }

            IEnumerable<string?>? genres = movie.Genres?.Select(i => i.Name).AsEnumerable();
            if (genres != null)
            {
                toReturn.SetGenres(genres);
            }

            IEnumerable<string?>? studios = movie.ProductionCompanies?.Select(i => i.Name).AsEnumerable();
            if (studios != null)
            {
                toReturn.SetStudios(studios);
            }

            movie.Videos?.Teasers?
                .Select(i => i.Url)
                .Where(i => !string.IsNullOrWhiteSpace(i))
                .ToList()
                .ForEach(j => toReturn.AddTrailerUrl(j));
            movie.Videos?.Trailers?
                .Select(i => i.Url)
                .Where(i => !string.IsNullOrWhiteSpace(i))
                .ToList()
                .ForEach(j => toReturn.AddTrailerUrl(j));

            return toReturn;
        }
        #endregion

        #region SeriesProvider
        public async Task<MetadataResult<Series>> GetMetadata(SeriesInfo info, CancellationToken cancellationToken)
        {
            MetadataResult<Series> result = new()
            {
                ResultLanguage = "ru"
            };

            if (string.IsNullOrWhiteSpace(Plugin.Instance?.Configuration.GetToken()))
            {
                _log.Warn($"The Token for {Plugin.PluginName} is empty");
                return result;
            }

            if (info.HasProviderId(Plugin.PluginName))
            {
                string seriesId = info.GetProviderId(Plugin.PluginName);
                if (!string.IsNullOrWhiteSpace(seriesId))
                {
                    _log.Info($"Fetching series by series id '{seriesId}'");
                    KpMovie? item = await _api.GetMovieById(seriesId, cancellationToken);
                    if (item != null)
                    {
                        result.Item = CreateSeriesFromKpMovie(item);
                        await UpdatePersonsList(result, item.Persons, cancellationToken);
                        result.HasMetadata = true;
                        return result;
                    }
                    _log.Info($"Series by series id '{seriesId}' not found");
                }
            }

            _log.Info($"Searching series by name {info.Name}");
            KpSearchResult<KpMovie> series = await _api.GetMoviesByMetadata(info.Name, info.Year, info.IsAutomated, cancellationToken);
            if (series.Docs.Count != 1)
            {
                _log.Error($"Found {series.Docs.Count} series, skipping series update");
                return result;
            }
            result.Item = CreateSeriesFromKpMovie(series.Docs[0]);
            await UpdatePersonsList(result, series.Docs[0].Persons, cancellationToken);
            result.HasMetadata = true;
            return result;
        }
        public async Task<IEnumerable<RemoteSearchResult>> GetSearchResults(SeriesInfo searchInfo, CancellationToken cancellationToken)
        {
            List<RemoteSearchResult> result = new();

            if (string.IsNullOrWhiteSpace(Plugin.Instance?.Configuration.GetToken()))
            {
                _log.Warn($"The Token for {Plugin.PluginName} is empty");
                return result;
            }

            if (searchInfo.HasProviderId(Plugin.PluginName))
            {
                string seriesId = searchInfo.GetProviderId(Plugin.PluginName);
                if (!string.IsNullOrWhiteSpace(seriesId))
                {
                    _log.Info($"Searching series by id '{seriesId}'");
                    KpMovie? series = await _api.GetMovieById(seriesId, cancellationToken);
                    if (series != null)
                    {
                        string imageUrl = (series.Poster?.PreviewUrl ?? series.Poster?.Url) ?? string.Empty;
                        RemoteSearchResult item = new()
                        {
                            Name = series.Name,
                            ImageUrl = imageUrl,
                            SearchProviderName = Plugin.PluginName,
                            ProductionYear = series.Year,
                            Overview = series.Description,
                        };
                        item.SetProviderId(Plugin.PluginName, seriesId);
                        result.Add(item);
                        return result;
                    }
                    _log.Info($"Series by id '{seriesId}' not found");
                }
            }

            _log.Info($"Searching series by available metadata");
            KpSearchResult<KpMovie> seriesResult = await _api.GetMoviesByMetadata(searchInfo.Name, searchInfo.Year, searchInfo.IsAutomated, cancellationToken);
            foreach (KpMovie series in seriesResult.Docs)
            {
                string imageUrl = (series.Poster?.PreviewUrl ?? series.Poster?.Url) ?? string.Empty;
                RemoteSearchResult item = new()
                {
                    Name = series.Name,
                    ImageUrl = imageUrl,
                    SearchProviderName = Plugin.PluginName,
                    ProductionYear = series.Year,
                    Overview = series.Description,
                };
                item.SetProviderId(Plugin.PluginName, series.Id.ToString(CultureInfo.InvariantCulture));
                result.Add(item);
            }
            _log.Info($"By name '{searchInfo.Name}' found {result.Count} series");
            return result;
        }
        private Series CreateSeriesFromKpMovie(KpMovie series)
        {
            _log.Info($"Series '{series.Name}' with KinopoiskId '{series.Id}' found");

            string seriesId = series.Id.ToString(CultureInfo.InvariantCulture);
            Series toReturn = new()
            {
                CommunityRating = series.Rating?.Kp,
                CriticRating = series.Rating?.FilmCritics * 10,
                EndDate = GetEndDate(series.ReleaseYears),
                ExternalId = seriesId,
                Name = series.Name,
                OfficialRating = series.RatingMpaa,
                OriginalTitle = series.AlternativeName,
                Overview = series.Description,
                PremiereDate = KpHelper.GetPremierDate(series.Premiere),
                ProductionLocations = series.Countries?.Select(i => i.Name).ToArray(),
                ProductionYear = series.Year,
                Size = series.MovieLength,
                SortName = !string.IsNullOrWhiteSpace(series.EnName) ? series.EnName : series.Name,
                Tagline = series.Slogan
            };

            toReturn.SetProviderId(Plugin.PluginName, seriesId);

            if (!string.IsNullOrWhiteSpace(series.ExternalId?.Imdb))
            {
                toReturn.ProviderIds.Add(MetadataProviders.Imdb.ToString(), series.ExternalId.Imdb);
            }

            if (!string.IsNullOrWhiteSpace(series.ExternalId?.Tmdb))
            {
                toReturn.ProviderIds.Add(MetadataProviders.Tmdb.ToString(), series.ExternalId.Tmdb);
            }

            IEnumerable<string?>? genres = series.Genres?.Select(i => i.Name).AsEnumerable();
            if (genres != null)
            {
                toReturn.SetGenres(genres);
            }

            IEnumerable<string?>? studios = series.ProductionCompanies?.Select(i => i.Name).AsEnumerable();
            if (studios != null)
            {
                toReturn.SetStudios(studios);
            }

            series.Videos?.Teasers?
                .Select(i => i.Url)
                .Where(i => !string.IsNullOrWhiteSpace(i))
                .ToList()
                .ForEach(j => toReturn.AddTrailerUrl(j));
            series.Videos?.Trailers?
                .Select(i => i.Url)
                .Where(i => !string.IsNullOrWhiteSpace(i))
                .ToList()
                .ForEach(j => toReturn.AddTrailerUrl(j));
            return toReturn;
        }
        #endregion

        #region EpisodeProvider
        public async Task<MetadataResult<Episode>> GetMetadata(EpisodeInfo info, CancellationToken cancellationToken)
        {
            MetadataResult<Episode> result = new()
            {
                ResultLanguage = "ru"
            };

            if (string.IsNullOrWhiteSpace(Plugin.Instance?.Configuration.GetToken()))
            {
                _log.Warn($"The Token for {Plugin.PluginName} is empty");
                return result;
            }

            string? seriesId = info.GetSeriesProviderId(Plugin.PluginName);
            if (string.IsNullOrWhiteSpace(seriesId))
            {
                _log.Debug($"SeriesProviderId not exists for {Plugin.PluginName}, checking ProviderId");
                seriesId = info.GetProviderId(Plugin.PluginName);
            }
            if (string.IsNullOrWhiteSpace(seriesId))
            {
                _log.Info($"The episode doesn't have series id for {Plugin.PluginName}");
                return result;
            }
            if (info.IndexNumber == null || info.ParentIndexNumber == null)
            {
                _log.Warn($"Not enough parameters. Season index '{info.ParentIndexNumber}', episode index '{info.IndexNumber}'");
                return result;
            }

            _log.Info($"Searching episode by series id '{seriesId}', season index '{info.ParentIndexNumber}' and episode index '{info.IndexNumber}'");
            KpSearchResult<KpSeason>? item = await _api.GetEpisodesBySeriesId(seriesId, cancellationToken);
            if (item == null)
            {
                _log.Info($"Episodes by series id '{seriesId}' not found");
                return result;
            }
            KpSeason? kpSeason = item.Docs.FirstOrDefault(s => s.Number == info.ParentIndexNumber);
            if (kpSeason == null)
            {
                _log.Info($"Season with index '{info.ParentIndexNumber}' not found");
                return result;
            }
            KpEpisode? kpEpisode = kpSeason.Episodes.FirstOrDefault(e => e.Number == info.IndexNumber);
            if (kpEpisode == null)
            {
                _log.Info($"Episode with index '{info.IndexNumber}' not found");
                return result;
            }
            _ = DateTimeOffset.TryParseExact(
                kpEpisode.Date,
                "yyyy-MM-dd'T'HH:mm:ss.fffZ",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateTimeOffset premiereDate);
            result.Item = new()
            {
                Name = kpEpisode.Name,
                OriginalTitle = kpEpisode.EnName,
                Overview = kpEpisode.Description,
                IndexNumber = info.IndexNumber,
                ParentIndexNumber = info.ParentIndexNumber,
                PremiereDate = premiereDate,
            };
            result.HasMetadata = true;
            _log.Info($"Episode {info.IndexNumber} of season {info.ParentIndexNumber} of series {seriesId} updated");
            return result;
        }
        #endregion

        #region PersonProvider
        public async Task<MetadataResult<Person>> GetMetadata(PersonLookupInfo info, CancellationToken cancellationToken)
        {
            MetadataResult<Person> result = new()
            {
                ResultLanguage = "ru"
            };

            if (string.IsNullOrWhiteSpace(Plugin.Instance?.Configuration.GetToken()))
            {
                _log.Warn($"The Token for {Plugin.PluginName} is empty");
                return result;
            }

            if (info.HasProviderId(Plugin.PluginName))
            {
                string personId = info.ProviderIds[Plugin.PluginName];
                if (!string.IsNullOrWhiteSpace(personId))
                {
                    _log.Info($"Fetching person by person id '{personId}'");
                    KpPerson? person = await _api.GetPersonById(personId, cancellationToken);
                    if (person != null)
                    {
                        result.Item = CreatePersonFromKpPerson(person);
                        result.HasMetadata = true;
                        return result;
                    };
                    _log.Info($"Person by person id '{personId}' not found");
                }
            }

            _log.Info($"Searching person by name {info.Name}");
            KpSearchResult<KpPerson> persons = await _api.GetPersonsByName(info.Name, cancellationToken);
            if (persons.Docs.Count != 1)
            {
                _log.Error($"Found {persons.Docs.Count} persons, skipping person update");
                return result;
            }
            result.Item = CreatePersonFromKpPerson(persons.Docs[0]);
            result.HasMetadata = true;
            return result;
        }
        public async Task<IEnumerable<RemoteSearchResult>> GetSearchResults(PersonLookupInfo searchInfo, CancellationToken cancellationToken)
        {
            List<RemoteSearchResult> result = new();

            if (string.IsNullOrWhiteSpace(Plugin.Instance?.Configuration.GetToken()))
            {
                _log.Warn($"The Token for {Plugin.PluginName} is empty");
                return result;
            }

            if (searchInfo.HasProviderId(Plugin.PluginName))
            {
                string personId = searchInfo.ProviderIds[Plugin.PluginName];
                if (!string.IsNullOrWhiteSpace(personId))
                {
                    _log.Info($"Searching person by id '{personId}'");
                    KpPerson? person = await _api.GetPersonById(personId, cancellationToken);
                    if (person != null)
                    {
                        RemoteSearchResult item = new()
                        {
                            Name = person.Name,
                            ImageUrl = person.Photo,
                        };
                        item.SetProviderId(Plugin.PluginName, personId);
                        result.Add(item);
                        return result;
                    }
                    _log.Info($"Person by id '{personId}' not found");
                }
            }

            _log.Info($"Searching person by name {searchInfo.Name}");
            KpSearchResult<KpPerson> persons = await _api.GetPersonsByName(searchInfo.Name, cancellationToken);
            foreach (KpPerson person in persons.Docs)
            {
                RemoteSearchResult item = new()
                {
                    Name = person.Name,
                    ImageUrl = person.Photo,
                };
                item.SetProviderId(Plugin.PluginName, person.Id.ToString(CultureInfo.InvariantCulture));
                result.Add(item);
            }
            _log.Info($"By name '{searchInfo.Name}' found {result.Count} persons");
            return result;
        }

        private Person CreatePersonFromKpPerson(KpPerson person)
        {
            _log.Info($"Person '{person.Name}' with KinopoiskId '{person.Id}' found");

            Person toReturn = new()
            {
                Name = person.Name,
                SortName = person.Name,
            };
            if (DateTimeOffset.TryParse(person.Birthday, out DateTimeOffset birthDay))
            {
                toReturn.PremiereDate = birthDay;
            }
            if (DateTimeOffset.TryParse(person.Death, out DateTimeOffset deathDay))
            {
                toReturn.EndDate = deathDay;
            }
            string?[]? birthPlace = person.BirthPlace?.Select(i => i.Value).ToArray();
            if (birthPlace != null && birthPlace.Length > 0 && !string.IsNullOrWhiteSpace(birthPlace[0]))
            {
                toReturn.ProductionLocations = new string[] { string.Join(", ", birthPlace) };
            }
            string?[]? facts = person.Facts?.Select(i => i.Value).ToArray();
            if (facts != null && facts.Length > 0 && !string.IsNullOrWhiteSpace(facts[0]))
            {
                toReturn.Overview = string.Join('\n', facts);
            }
            return toReturn;
        }
        #endregion

        #region MovieImageProvider
        public async Task<IEnumerable<RemoteImageInfo>> GetImages(BaseItem item, LibraryOptions libraryOptions, CancellationToken cancellationToken)
        {
            List<RemoteImageInfo> result = new();

            if (string.IsNullOrWhiteSpace(Plugin.Instance?.Configuration.GetToken()))
            {
                _log.Warn($"The Token for {Plugin.PluginName} is empty");
                return result;
            }

            if (item.HasProviderId(Plugin.PluginName))
            {
                string movieId = item.ProviderIds[Plugin.PluginName];
                if (!string.IsNullOrWhiteSpace(movieId))
                {
                    _log.Info($"Searching images by movie id '{movieId}'");
                    KpMovie? movie = await _api.GetMovieById(movieId, cancellationToken);
                    if (movie != null)
                    {
                        UpdateRemoteImageInfoList(movie, result);
                        return result;
                    };
                    _log.Info($"Images by movie id '{movieId}' not found");
                }
            }

            _log.Info($"Searching images by available metadata");
            KpSearchResult<KpMovie> movies = await _api.GetMoviesByMovieDetails(item.Name, item.OriginalTitle, item.ProductionYear, cancellationToken);
            if (movies.Docs.Count != 1)
            {
                _log.Error($"Found {movies.Docs.Count} movies, skipping image update");
                return result;
            }
            UpdateRemoteImageInfoList(movies.Docs[0], result);
            return result;
        }
        private void UpdateRemoteImageInfoList(KpMovie movie, List<RemoteImageInfo> toReturn)
        {
            if (!string.IsNullOrWhiteSpace(movie.Backdrop?.Url))
            {
                toReturn.Add(new RemoteImageInfo()
                {
                    ProviderName = Plugin.PluginName,
                    Url = movie.Backdrop.Url,
                    ThumbnailUrl = movie.Backdrop.PreviewUrl,
                    Language = "ru",
                    DisplayLanguage = "RU",
                    Type = ImageType.Backdrop
                });
            }
            if (!string.IsNullOrWhiteSpace(movie.Poster?.Url))
            {
                toReturn.Add(new RemoteImageInfo()
                {
                    ProviderName = Plugin.PluginName,
                    Url = movie.Poster.Url,
                    ThumbnailUrl = movie.Poster.PreviewUrl,
                    Language = "ru",
                    DisplayLanguage = "RU",
                    Type = ImageType.Primary
                });
            }
            if (!string.IsNullOrWhiteSpace(movie.Logo?.Url))
            {
                toReturn.Add(new RemoteImageInfo()
                {
                    ProviderName = Plugin.PluginName,
                    Url = movie.Logo.Url,
                    ThumbnailUrl = movie.Logo.PreviewUrl,
                    Language = "ru",
                    DisplayLanguage = "RU",
                    Type = ImageType.Logo
                });
            }
            _log.Info($"By movie id '{movie.Id}' found '{string.Join(", ", toReturn.Select(i => i.Type).ToList())}' image types");
        }
        #endregion

        #region Common
        private static DateTimeOffset? GetEndDate(List<KpYearRange>? releaseYears)
        {
            if (releaseYears == null || releaseYears.Count == 0)
            {
                return null;
            }
            int max = 0;
            releaseYears
                .Where(i => i.End != null)
                .ToList()
                .ForEach(i => max = Math.Max((int)max, (int)i.End!));

            return DateTimeOffset.TryParseExact(
                            max.ToString(CultureInfo.InvariantCulture),
                            "yyyy",
                            CultureInfo.InvariantCulture,
                            DateTimeStyles.None,
                            out DateTimeOffset result)
                ? result
                : null;
        }
        private async Task UpdatePersonsList<T>(MetadataResult<T> result, List<KpMovie.KpPerson>? persons, CancellationToken cancellationToken)
            where T : BaseItem
        {
            if (persons == null)
            {
                _log.Warn($"Received persons list is null for series with id '{result.Item.GetProviderId(Plugin.PluginName)}'");
                return;
            }

            _log.Info($"Updating persons list of the series with id '{result.Item.GetProviderId(Plugin.PluginName)}'");
            string seriesId = result.Item.GetProviderId(Plugin.PluginName);
            string movieName = result.Item.Name;
            KpSearchResult<KpPerson> personsBySeriesId = await _api.GetPersonsByMovieId(seriesId, cancellationToken);
            personsBySeriesId.Docs
                .ForEach(a =>
                    a.Movies.RemoveAll(b =>
                        b.Id.ToString(CultureInfo.InvariantCulture) != seriesId
                            || string.IsNullOrWhiteSpace(b.Description)
                        ));

            Dictionary<long, string?> idRoleDictionary = personsBySeriesId.Docs
                .ToDictionary(
                    c => c.Id,
                    c => c.Movies.FirstOrDefault()?.Description);

            string seriesName = result.Item.Name;
            foreach (KpMovie.KpPerson kpPerson in persons)
            {
                PersonType? personType = KpHelper.GetPersonType(kpPerson.EnProfession);
                string? name = string.IsNullOrWhiteSpace(kpPerson.Name) ? kpPerson.EnName : kpPerson.Name;
                if (string.IsNullOrWhiteSpace(name))
                {
                    _log.Warn($"Skip adding staff with id '{kpPerson.Id.ToString(CultureInfo.InvariantCulture)}' as nameless to '{movieName}'");
                }
                else if (personType == null)
                {
                    _log.Warn($"Skip adding {name} as '{kpPerson.EnProfession}' to {seriesName}");
                }
                else
                {
                    _log.Debug($"Adding {name} as '{personType}' to {seriesName}");
                    _ = idRoleDictionary.TryGetValue(kpPerson.Id, out string? role);
                    PersonInfo person = new()
                    {
                        Name = name,
                        ImageUrl = kpPerson.Photo,
                        Type = (PersonType)personType,
                        Role = role,
                    };
                    person.SetProviderId(Plugin.PluginName, kpPerson.Id.ToString(CultureInfo.InvariantCulture));

                    result.AddPerson(person);
                }
            }
            _log.Info($"Added {result.People.Count} persons to the series with id '{result.Item.GetProviderId(Plugin.PluginName)}'");
        }
        #endregion
    }
}
