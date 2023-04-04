using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using EmbyKinopoiskRu.Api.KinopoiskDev.Model;
using EmbyKinopoiskRu.Api.KinopoiskDev.Model.Movie;
using EmbyKinopoiskRu.Api.KinopoiskDev.Model.Person;
using EmbyKinopoiskRu.Api.KinopoiskDev.Model.Season;
using EmbyKinopoiskRu.Configuration;
using EmbyKinopoiskRu.Helper;

using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Collections;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Activity;
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
        private readonly ILibraryManager _libraryManager;
        private readonly ICollectionManager _collectionManager;
        private PluginConfiguration PluginConfig { get; set; }
        private readonly List<KpMovieType?> _movieTypes = new List<KpMovieType?>() { KpMovieType.Anime, KpMovieType.Movie, KpMovieType.Cartoon };

        internal KinopoiskDevService(
            ILogManager logManager
            , IHttpClient httpClient
            , IJsonSerializer jsonSerializer
            , IActivityManager activityManager
            , ILibraryManager libraryManager
            , ICollectionManager collectionManager)
        {
            _log = logManager.GetLogger(GetType().Name);
            _api = new KinopoiskDevApi(_log, httpClient, jsonSerializer, activityManager);
            _libraryManager = libraryManager;
            _collectionManager = collectionManager;
            if (Plugin.Instance == null)
            {
                throw new NullReferenceException($"Plugin '{Plugin.PluginName}' instance is null");
            }
            PluginConfig = Plugin.Instance.Configuration;
        }

        #region MovieProvider
        public async Task<List<Movie>> GetMoviesByOriginalNameAndYear(string name, int? year, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(PluginConfig.GetCurrentToken()))
            {
                _log.Warn($"The Token for {Plugin.PluginName} is empty");
                return new List<Movie>();
            }

            KpSearchResult<KpMovie> movies = await _api.GetMoviesByMovieDetails(name, year, cancellationToken);
            List<KpMovie> relevantMovies = FilterRelevantItems(movies.Docs, name, year);
            var toReturn = new List<Movie>();
            foreach (KpMovie movie in relevantMovies)
            {
                toReturn.Add(await CreateMovieFromKpMovie(movie, cancellationToken));
            }
            _log.Info($"By alternative name '{name}' found {toReturn.Count} movies");
            return toReturn;
        }
        public async Task<MetadataResult<Movie>> GetMetadata(MovieInfo info, CancellationToken cancellationToken)
        {
            var result = new MetadataResult<Movie>()
            {
                ResultLanguage = "ru"
            };

            if (string.IsNullOrWhiteSpace(PluginConfig.GetCurrentToken()))
            {
                _log.Warn($"The Token for {Plugin.PluginName} is empty");
                return result;
            }

            if (info.HasProviderId(Plugin.PluginName))
            {
                var movieId = info.GetProviderId(Plugin.PluginName);
                if (!string.IsNullOrWhiteSpace(movieId))
                {
                    _log.Info($"Searching movie by movie id '{movieId}'");
                    KpMovie movie = await _api.GetMovieById(movieId, cancellationToken);
                    if (movie != null)
                    {
                        result.Item = await CreateMovieFromKpMovie(movie, cancellationToken);
                        await UpdatePersonsList(result, movie.Persons, cancellationToken);
                        result.HasMetadata = true;
                        return result;
                    }
                    _log.Info($"Movie by movie id '{movieId}' not found");
                }
            }

            _log.Info($"Searching movie by name '{info.Name}' and year '{info.Year}'");
            KpSearchResult<KpMovie> movies = await _api.GetMoviesByMovieDetails(info.Name, info.Year, cancellationToken);
            List<KpMovie> relevantMovies = FilterRelevantItems(movies.Docs, info.Name, info.Year);
            if (relevantMovies.Count != 1)
            {
                _log.Error($"Found {relevantMovies.Count} movies, skipping movie update");
                return result;
            }
            result.Item = await CreateMovieFromKpMovie(relevantMovies[0], cancellationToken);
            await UpdatePersonsList(result, relevantMovies[0].Persons, cancellationToken);
            result.HasMetadata = true;
            return result;
        }
        public async Task<IEnumerable<RemoteSearchResult>> GetSearchResults(MovieInfo searchInfo, CancellationToken cancellationToken)
        {
            var result = new List<RemoteSearchResult>();

            if (string.IsNullOrWhiteSpace(PluginConfig.GetCurrentToken()))
            {
                _log.Warn($"The Token for {Plugin.PluginName} is empty");
                return result;
            }

            if (searchInfo.HasProviderId(Plugin.PluginName))
            {
                var movieId = searchInfo.GetProviderId(Plugin.PluginName);
                if (!string.IsNullOrWhiteSpace(movieId))
                {
                    _log.Info($"Searching movie by id '{movieId}'");
                    KpMovie movie = await _api.GetMovieById(movieId, cancellationToken);
                    if (movie != null)
                    {
                        var imageUrl = (movie.Poster?.PreviewUrl ?? movie.Poster?.Url) ?? string.Empty;
                        var item = new RemoteSearchResult()
                        {
                            Name = movie.Name,
                            ImageUrl = imageUrl,
                            SearchProviderName = Plugin.PluginName,
                            ProductionYear = movie.Year,
                            Overview = PrepareOverview(movie),
                        };
                        item.SetProviderId(Plugin.PluginName, movieId);
                        result.Add(item);
                        return result;
                    }
                    _log.Info($"Movie by id '{movieId}' not found");
                }
            }

            _log.Info($"Searching movies by available metadata");
            KpSearchResult<KpMovie> movies = await _api.GetMoviesByMovieDetails(searchInfo.Name, searchInfo.Year, cancellationToken);
            foreach (KpMovie movie in movies.Docs)
            {
                var imageUrl = (movie.Poster?.PreviewUrl ?? movie.Poster?.Url) ?? string.Empty;
                var item = new RemoteSearchResult()
                {
                    Name = movie.Name,
                    ImageUrl = imageUrl,
                    SearchProviderName = Plugin.PluginName,
                    ProductionYear = movie.Year,
                    Overview = PrepareOverview(movie),
                };
                item.SetProviderId(Plugin.PluginName, movie.Id.ToString(CultureInfo.InvariantCulture));
                result.Add(item);
            }
            _log.Info($"By name '{searchInfo.Name}' found {result.Count} movies");
            return result;
        }
        private async Task<Movie> CreateMovieFromKpMovie(KpMovie movie, CancellationToken cancellationToken)
        {
            _log.Info($"Movie '{movie.Name}' with {Plugin.PluginName} id '{movie.Id}' found");

            var movieId = movie.Id.ToString(CultureInfo.InvariantCulture);
            var toReturn = new Movie()
            {
                CommunityRating = movie.Rating?.Kp,
                CriticRating = movie.Rating?.FilmCritics * 10,
                ExternalId = movieId,
                Name = movie.Name,
                OfficialRating = movie.RatingMpaa,
                OriginalTitle = movie.AlternativeName,
                Overview = PrepareOverview(movie),
                PremiereDate = KpHelper.GetPremierDate(movie.Premiere),
                ProductionLocations = movie.Countries?.Select(i => i.Name).ToArray(),
                ProductionYear = movie.Year,
                Size = movie.MovieLength,
                SortName =
                    string.IsNullOrWhiteSpace(movie.Name) ?
                        string.IsNullOrWhiteSpace(movie.AlternativeName) ?
                            string.Empty
                            : movie.AlternativeName
                        : movie.Name,
                Tagline = movie.Slogan
            };

            toReturn.SetProviderId(Plugin.PluginName, movieId);

            if (!string.IsNullOrWhiteSpace(movie.ExternalId?.Imdb))
            {
                toReturn.ProviderIds.Add(MetadataProviders.Imdb.ToString(), movie.ExternalId.Imdb);
            }

            if (movie.ExternalId?.Tmdb != null)
            {
                toReturn.ProviderIds.Add(MetadataProviders.Tmdb.ToString(), movie.ExternalId.Tmdb.ToString());
            }

            IEnumerable<string> genres = movie.Genres?.Select(i => i.Name).AsEnumerable();
            if (genres != null)
            {
                toReturn.SetGenres(genres);
            }

            IEnumerable<string> studios = movie.ProductionCompanies?.Select(i => i.Name).AsEnumerable();
            if (studios != null)
            {
                toReturn.SetStudios(studios);
            }

            movie.Videos?.Teasers?
                .Where(i => !string.IsNullOrWhiteSpace(i.Url) && i.Url.Contains("youtube"))
                .Select(i => i.Url
                        .Replace("https://www.youtube.com/embed/", "https://www.youtube.com/watch?v=")
                        .Replace("https://www.youtube.com/v/", "https://www.youtube.com/watch?v="))
                .Reverse()
                .ToList()
                .ForEach(j => toReturn.AddTrailerUrl(j));
            movie.Videos?.Trailers?
                .Where(i => !string.IsNullOrWhiteSpace(i.Url) && i.Url.Contains("youtube"))
                .Select(i => i.Url
                        .Replace("https://www.youtube.com/embed/", "https://www.youtube.com/watch?v=")
                        .Replace("https://www.youtube.com/v/", "https://www.youtube.com/watch?v="))
                .Reverse()
                .ToList()
                .ForEach(j => toReturn.AddTrailerUrl(j));

            if (PluginConfig.NeedToCreateCollection() && movie.SequelsAndPrequels.Any())
            {
                await AddMovieToCollection(toReturn, movie, cancellationToken);
            }

            return toReturn;
        }

        #endregion

        #region SeriesProvider
        public async Task<MetadataResult<Series>> GetMetadata(SeriesInfo info, CancellationToken cancellationToken)
        {
            var result = new MetadataResult<Series>()
            {
                ResultLanguage = "ru"
            };

            if (string.IsNullOrWhiteSpace(PluginConfig.GetCurrentToken()))
            {
                _log.Warn($"The Token for {Plugin.PluginName} is empty");
                return result;
            }

            if (info.HasProviderId(Plugin.PluginName))
            {
                var seriesId = info.GetProviderId(Plugin.PluginName);
                if (!string.IsNullOrWhiteSpace(seriesId))
                {
                    _log.Info($"Fetching series by series id '{seriesId}'");
                    KpMovie item = await _api.GetMovieById(seriesId, cancellationToken);
                    if (item != null)
                    {
                        result.Item = await CreateSeriesFromKpMovie(item, cancellationToken);
                        await UpdatePersonsList(result, item.Persons, cancellationToken);
                        result.HasMetadata = true;
                        return result;
                    }
                    _log.Info($"Series by series id '{seriesId}' not found");
                }
            }

            _log.Info($"Searching series by name '{info.Name}' and year '{info.Year}'");
            KpSearchResult<KpMovie> series = await _api.GetMoviesByMovieDetails(info.Name, info.Year, cancellationToken);
            List<KpMovie> relevantSeries = FilterRelevantItems(series.Docs, info.Name, info.Year);
            if (relevantSeries.Count != 1)
            {
                _log.Error($"Found {relevantSeries.Count} series, skipping series update");
                return result;
            }
            result.Item = await CreateSeriesFromKpMovie(relevantSeries[0], cancellationToken);
            await UpdatePersonsList(result, relevantSeries[0].Persons, cancellationToken);
            result.HasMetadata = true;
            return result;
        }
        public async Task<IEnumerable<RemoteSearchResult>> GetSearchResults(SeriesInfo searchInfo, CancellationToken cancellationToken)
        {
            var result = new List<RemoteSearchResult>();

            if (string.IsNullOrWhiteSpace(PluginConfig.GetCurrentToken()))
            {
                _log.Warn($"The Token for {Plugin.PluginName} is empty");
                return result;
            }

            if (searchInfo.HasProviderId(Plugin.PluginName))
            {
                var seriesId = searchInfo.GetProviderId(Plugin.PluginName);
                if (!string.IsNullOrWhiteSpace(seriesId))
                {
                    _log.Info($"Searching series by id '{seriesId}'");
                    KpMovie series = await _api.GetMovieById(seriesId, cancellationToken);
                    if (series != null)
                    {
                        var imageUrl = (series.Poster?.PreviewUrl ?? series.Poster?.Url) ?? string.Empty;
                        var item = new RemoteSearchResult()
                        {
                            Name = series.Name,
                            ImageUrl = imageUrl,
                            SearchProviderName = Plugin.PluginName,
                            ProductionYear = series.Year,
                            Overview = PrepareOverview(series),
                        };
                        item.SetProviderId(Plugin.PluginName, seriesId);
                        result.Add(item);
                        return result;
                    }
                    _log.Info($"Series by id '{seriesId}' not found");
                }
            }

            _log.Info($"Searching series by available metadata");
            KpSearchResult<KpMovie> seriesResult = await _api.GetMoviesByMovieDetails(searchInfo.Name, searchInfo.Year, cancellationToken);
            foreach (KpMovie series in seriesResult.Docs)
            {
                var imageUrl = (series.Poster?.PreviewUrl ?? series.Poster?.Url) ?? string.Empty;
                var item = new RemoteSearchResult()
                {
                    Name = series.Name,
                    ImageUrl = imageUrl,
                    SearchProviderName = Plugin.PluginName,
                    ProductionYear = series.Year,
                    Overview = PrepareOverview(series),
                };
                item.SetProviderId(Plugin.PluginName, series.Id.ToString(CultureInfo.InvariantCulture));
                result.Add(item);
            }
            _log.Info($"By name '{searchInfo.Name}' found {result.Count} series");
            return result;
        }
        private async Task<Series> CreateSeriesFromKpMovie(KpMovie series, CancellationToken cancellationToken)
        {
            _log.Info($"Series '{series.Name}' with KinopoiskId '{series.Id}' found");

            var seriesId = series.Id.ToString(CultureInfo.InvariantCulture);
            var toReturn = new Series()
            {
                CommunityRating = series.Rating?.Kp,
                CriticRating = series.Rating?.FilmCritics * 10,
                EndDate = GetEndDate(series.ReleaseYears),
                ExternalId = seriesId,
                Name = series.Name,
                OfficialRating = series.RatingMpaa,
                OriginalTitle = series.AlternativeName,
                Overview = PrepareOverview(series),
                PremiereDate = KpHelper.GetPremierDate(series.Premiere),
                ProductionLocations = series.Countries?.Select(i => i.Name).ToArray(),
                ProductionYear = series.Year,
                Size = series.MovieLength,
                SortName =
                    string.IsNullOrWhiteSpace(series.Name) ?
                        string.IsNullOrWhiteSpace(series.AlternativeName) ?
                            string.Empty
                            : series.AlternativeName
                        : series.Name,
                Tagline = series.Slogan
            };

            toReturn.SetProviderId(Plugin.PluginName, seriesId);

            if (!string.IsNullOrWhiteSpace(series.ExternalId?.Imdb))
            {
                toReturn.ProviderIds.Add(MetadataProviders.Imdb.ToString(), series.ExternalId.Imdb);
            }

            if (series.ExternalId?.Tmdb != null)
            {
                toReturn.ProviderIds.Add(MetadataProviders.Tmdb.ToString(), series.ExternalId.Tmdb.ToString());
            }

            IEnumerable<string> genres = series.Genres?.Select(i => i.Name).AsEnumerable();
            if (genres != null)
            {
                toReturn.SetGenres(genres);
            }

            IEnumerable<string> studios = series.ProductionCompanies?.Select(i => i.Name).AsEnumerable();
            if (studios != null)
            {
                toReturn.SetStudios(studios);
            }

            series.Videos?.Teasers?
                .Where(i => !string.IsNullOrWhiteSpace(i.Url) && i.Url.Contains("youtube"))
                .Select(i => i.Url
                        .Replace("https://www.youtube.com/embed/", "https://www.youtube.com/watch?v=")
                        .Replace("https://www.youtube.com/v/", "https://www.youtube.com/watch?v="))
                .Reverse()
                .ToList()
                .ForEach(j => toReturn.AddTrailerUrl(j));
            series.Videos?.Trailers?
                .Where(i => !string.IsNullOrWhiteSpace(i.Url) && i.Url.Contains("youtube"))
                .Select(i => i.Url
                        .Replace("https://www.youtube.com/embed/", "https://www.youtube.com/watch?v=")
                        .Replace("https://www.youtube.com/v/", "https://www.youtube.com/watch?v="))
                .Reverse()
                .ToList()
                .ForEach(j => toReturn.AddTrailerUrl(j));

            if (PluginConfig.NeedToCreateCollection() && series.SequelsAndPrequels.Any())
            {
                await AddMovieToCollection(toReturn, series, cancellationToken);
            }

            return toReturn;
        }
        #endregion

        #region EpisodeProvider
        public async Task<MetadataResult<Episode>> GetMetadata(EpisodeInfo info, CancellationToken cancellationToken)
        {
            var result = new MetadataResult<Episode>()
            {
                ResultLanguage = "ru"
            };

            if (string.IsNullOrWhiteSpace(PluginConfig.GetCurrentToken()))
            {
                _log.Warn($"The Token for {Plugin.PluginName} is empty");
                return result;
            }

            var seriesId = info.GetSeriesProviderId(Plugin.PluginName);
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
            KpSearchResult<KpSeason> item = await _api.GetEpisodesBySeriesId(seriesId, cancellationToken);
            if (item == null)
            {
                _log.Info($"Episodes by series id '{seriesId}' not found");
                return result;
            }
            KpSeason kpSeason = item.Docs.FirstOrDefault(s => s.Number == info.ParentIndexNumber);
            if (kpSeason == null)
            {
                _log.Info($"Season with index '{info.ParentIndexNumber}' not found");
                return result;
            }
            KpEpisode kpEpisode = kpSeason.Episodes?.FirstOrDefault(e => e.Number == info.IndexNumber);
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
            result.Item = new Episode()
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
            var result = new MetadataResult<Person>()
            {
                ResultLanguage = "ru"
            };

            if (string.IsNullOrWhiteSpace(PluginConfig.GetCurrentToken()))
            {
                _log.Warn($"The Token for {Plugin.PluginName} is empty");
                return result;
            }

            if (info.HasProviderId(Plugin.PluginName))
            {
                var personId = info.ProviderIds[Plugin.PluginName];
                if (!string.IsNullOrWhiteSpace(personId))
                {
                    _log.Info($"Fetching person by person id '{personId}'");
                    KpPerson person = await _api.GetPersonById(personId, cancellationToken);
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
            var result = new List<RemoteSearchResult>();

            if (string.IsNullOrWhiteSpace(PluginConfig.GetCurrentToken()))
            {
                _log.Warn($"The Token for {Plugin.PluginName} is empty");
                return result;
            }

            if (searchInfo.HasProviderId(Plugin.PluginName))
            {
                var personId = searchInfo.ProviderIds[Plugin.PluginName];
                if (!string.IsNullOrWhiteSpace(personId))
                {
                    _log.Info($"Searching person by id '{personId}'");
                    KpPerson person = await _api.GetPersonById(personId, cancellationToken);
                    if (person != null)
                    {
                        var item = new RemoteSearchResult()
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
                var item = new RemoteSearchResult()
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

            var toReturn = new Person()
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
            var birthPlace = person.BirthPlace?.Select(i => i.Value).ToArray();
            if (birthPlace != null && birthPlace.Length > 0 && !string.IsNullOrWhiteSpace(birthPlace[0]))
            {
                toReturn.ProductionLocations = new string[] { string.Join(", ", birthPlace) };
            }
            var facts = person.Facts?.Select(i => i.Value).ToArray();
            if (facts != null && facts.Length > 0 && !string.IsNullOrWhiteSpace(facts[0]))
            {
                toReturn.Overview = string.Join("\n", facts);
            }
            return toReturn;
        }
        #endregion

        #region MovieImageProvider
        public async Task<IEnumerable<RemoteImageInfo>> GetImages(BaseItem item, LibraryOptions libraryOptions, CancellationToken cancellationToken)
        {
            var result = new List<RemoteImageInfo>();

            if (string.IsNullOrWhiteSpace(PluginConfig.GetCurrentToken()))
            {
                _log.Warn($"The Token for {Plugin.PluginName} is empty");
                return result;
            }

            if (item.HasProviderId(Plugin.PluginName))
            {
                var movieId = item.ProviderIds[Plugin.PluginName];
                if (!string.IsNullOrWhiteSpace(movieId))
                {
                    _log.Info($"Searching images by movie id '{movieId}'");
                    KpMovie movie = await _api.GetMovieById(movieId, cancellationToken);
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
            List<KpMovie> relevantSeries = FilterRelevantItems(movies.Docs, item.Name, item.ProductionYear);
            if (relevantSeries.Count != 1)
            {
                _log.Error($"Found {relevantSeries.Count} movies, skipping image update");
                return result;
            }
            UpdateRemoteImageInfoList(relevantSeries[0], result);
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
        private static DateTimeOffset? GetEndDate(List<KpYearRange> releaseYears)
        {
            if (releaseYears == null || releaseYears.Count == 0)
            {
                return null;
            }
            var max = 0;
            releaseYears
                .Where(i => i.End != null)
                .ToList()
                .ForEach(i => max = Math.Max((int)max, (int)i.End));

            return DateTimeOffset.TryParseExact(
                            max.ToString(CultureInfo.InvariantCulture),
                            "yyyy",
                            CultureInfo.InvariantCulture,
                            DateTimeStyles.None,
                            out DateTimeOffset result)
                ? result
                : (DateTimeOffset?)null;
        }
        private async Task UpdatePersonsList<T>(MetadataResult<T> result, List<KpPersonMovie> persons, CancellationToken cancellationToken)
            where T : BaseItem
        {
            if (persons == null)
            {
                _log.Warn($"Received persons list is null for series with id '{result.Item.GetProviderId(Plugin.PluginName)}'");
                return;
            }

            _log.Info($"Updating persons list of the series with id '{result.Item.GetProviderId(Plugin.PluginName)}'");
            var seriesId = result.Item.GetProviderId(Plugin.PluginName);
            var movieName = result.Item.Name;
            KpSearchResult<KpPerson> personsBySeriesId = await _api.GetPersonsByMovieId(seriesId, cancellationToken);
            personsBySeriesId.Docs
                .ForEach(a =>
                    a.Movies?.RemoveAll(b =>
                        b.Id.ToString(CultureInfo.InvariantCulture) != seriesId
                            || string.IsNullOrWhiteSpace(b.Description)
                        ));

            var idRoleDictionary = personsBySeriesId.Docs
                .ToDictionary(
                    c => c.Id,
                    c => c.Movies?.FirstOrDefault()?.Description);

            var seriesName = result.Item.Name;
            foreach (KpPersonMovie kpPerson in persons)
            {
                PersonType? personType = KpHelper.GetPersonType(kpPerson.EnProfession);
                var name = string.IsNullOrWhiteSpace(kpPerson.Name) ? kpPerson.EnName : kpPerson.Name;
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
                    _ = idRoleDictionary.TryGetValue(kpPerson.Id, out var role);
                    var person = new PersonInfo()
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
        private async Task AddMovieToCollection(BaseItem toReturn, KpMovie movie, CancellationToken cancellationToken)
        {
            _log.Info($"Adding '{toReturn.Name}' to collection");

            CollectionFolder rootCollectionFolder = await EmbyHelper.InsureCollectionLibraryFolder(_libraryManager, _log);
            if (rootCollectionFolder == null)
            {
                _log.Info($"The virtual folder 'Collections' was not found nor created");
                return;
            }

            // Get internalIds of each object in sequence
            var itemsToAdd = movie.SequelsAndPrequels
                .Select(seq => seq.Id)
                .ToList();
            List<BaseItem> internalCollectionItems = await EmbyHelper.GetInternalIds(itemsToAdd, _libraryManager, _log, _api, cancellationToken);
            var internalIdArray = internalCollectionItems
                .Select(item => item.InternalId)
                .ToList();

            BoxSet collection = EmbyHelper.SearchExistingCollection(internalIdArray, _libraryManager, _log);
            if (collection == null && internalCollectionItems.Count > 0)
            {
                var newCollectionName = GetNewCollectionName(movie);
                _log.Info($"Creating '{newCollectionName}' collection with following items: '{string.Join("', '", internalCollectionItems.Select(m => m.Name))}'");
                collection = await _collectionManager.CreateCollection(new CollectionCreationOptions()
                {
                    IsLocked = false,
                    Name = newCollectionName,
                    ParentId = rootCollectionFolder.InternalId,
                    ItemIdList = internalIdArray.ToArray()
                });
                _ = toReturn.AddCollection(collection);
            }
            else if (collection != null && internalCollectionItems.Count > 0)
            {
                _log.Info($"Updating '{collection.Name}' collection with following items: '{string.Join("', '", internalCollectionItems.Select(m => m.Name))}'");
                foreach (BaseItem item in internalCollectionItems)
                {
                    if (item.AddCollection(collection))
                    {
                        _log.Info($"Adding '{item.Name}' to collection '{collection.Name}'");
                        item.UpdateToRepository(ItemUpdateType.MetadataEdit);
                    }
                    else
                    {
                        _log.Info($"'{item.Name}' already in the collection '{collection.Name}'");
                    }
                }
                _ = toReturn.AddCollection(collection);
            }
            else
            {
                _log.Info("No collection created/updated");
            }

            _log.Info("Finished adding to collection");
        }
        private static string GetNewCollectionName(KpMovie movie)
        {
            var itemsList = movie.SequelsAndPrequels
                .Select(s => (s.Id, s.Name))
                .ToList();
            itemsList.Add((movie.Id, movie.Name));
            (var id, var name) = itemsList
                .Where(m => !string.IsNullOrWhiteSpace(m.Name))
                .OrderBy(m => m.Id)
                .FirstOrDefault();
            return name;
        }
        private static string PrepareOverview(KpMovie movie)
        {
            var subj = $"<br/><br/><b>Интересное:</b><ul>";
            var sb = new StringBuilder(subj);
            movie.Facts?
                .Where(f => !f.Spoiler && "FACT".Equals(f.Type, StringComparison.OrdinalIgnoreCase))
                .ToList()
                .ForEach(f => sb.Append("<li>").Append(f.Value).Append("</li>"));
            _ = sb.Append("</ul>");

            return (sb.Length == (subj.Length + "</ul>".Length))
                ? movie.Description
                : sb.Insert(0, movie.Description).ToString();
        }
        private static List<KpMovie> FilterRelevantItems(List<KpMovie> list, string name, int? year)
        {
            if (list.Count > 1)
            {
                return list
                    .Where(m => m.Name == name || m.AlternativeName == name)
                    .Where(m => year == null || m.Year == year)
                    .ToList();
            }
            else
            {
                return list;
            }
        }
        #endregion

        #region Scheduled Tasks
        public async Task<List<Movie>> GetTop250MovieCollection(CancellationToken cancellationToken)
        {
            var toReturn = new List<Movie>();
            KpSearchResult<KpMovie> movies = await _api.GetTop250Collection(cancellationToken);
            movies.Docs
                .Where(m => _movieTypes.Contains(m.TypeNumber))
                .ToList()
                .ForEach(m =>
                {
                    var movie = new Movie()
                    {
                        Name = m.Name,
                        OriginalTitle = m.AlternativeName
                    };
                    movie.SetProviderId(Plugin.PluginName, m.Id.ToString(CultureInfo.InvariantCulture));
                    if (!string.IsNullOrWhiteSpace(m.ExternalId?.Imdb))
                    {
                        movie.SetProviderId(MetadataProviders.Imdb.ToString(), m.ExternalId.Imdb);
                    }
                    if (m.ExternalId?.Tmdb != null && m.ExternalId.Tmdb > 0)
                    {
                        movie.SetProviderId(MetadataProviders.Tmdb.ToString(), m.ExternalId.Tmdb.ToString());
                    }

                    toReturn.Add(movie);
                });
            return toReturn;
        }
        public async Task<List<Series>> GetTop250SeriesCollection(CancellationToken cancellationToken)
        {
            var toReturn = new List<Series>();
            KpSearchResult<KpMovie> movies = await _api.GetTop250Collection(cancellationToken);
            movies.Docs
                .Where(m => !_movieTypes.Contains(m.TypeNumber))
                .ToList()
                .ForEach(m =>
                {
                    var series = new Series()
                    {
                        Name = m.Name,
                        OriginalTitle = m.AlternativeName
                    };
                    series.SetProviderId(Plugin.PluginName, m.Id.ToString(CultureInfo.InvariantCulture));
                    if (!string.IsNullOrWhiteSpace(m.ExternalId?.Imdb))
                    {
                        series.SetProviderId(MetadataProviders.Imdb.ToString(), m.ExternalId.Imdb);
                    }
                    if (m.ExternalId?.Tmdb != null && m.ExternalId.Tmdb > 0)
                    {
                        series.SetProviderId(MetadataProviders.Tmdb.ToString(), m.ExternalId.Tmdb.ToString());
                    }

                    toReturn.Add(series);
                });
            return toReturn;
        }
        public async Task<ApiResult<Dictionary<string, long>>> GetKpIdByAnotherId(string externalIdType, IEnumerable<string> idList, CancellationToken cancellationToken)
        {
            KpSearchResult<KpMovie> movies = await _api.GetKpIdByAnotherId(externalIdType, idList, cancellationToken);
            if (movies.HasError)
            {
                return new ApiResult<Dictionary<string, long>>(new Dictionary<string, long>())
                {
                    HasError = true
                };
            }
            if (MetadataProviders.Imdb.ToString() == externalIdType)
            {
                return new ApiResult<Dictionary<string, long>>(movies.Docs
                    .Where(m => !string.IsNullOrWhiteSpace(m.ExternalId?.Imdb))
                    .ToDictionary(
                        m => m.ExternalId.Imdb,
                        m => m.Id
                    ));
            }
            if (MetadataProviders.Tmdb.ToString() == externalIdType)
            {
                return new ApiResult<Dictionary<string, long>>(movies.Docs
                    .Where(m => m.ExternalId?.Tmdb != null && m.ExternalId.Tmdb > 0)
                    .ToDictionary(
                        m => ((long)m.ExternalId.Tmdb).ToString(CultureInfo.InvariantCulture),
                        m => m.Id
                    ));
            }
            return new ApiResult<Dictionary<string, long>>(new Dictionary<string, long>())
            {
                HasError = true
            };
        }

        #endregion
    }
}
