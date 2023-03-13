using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using EmbyKinopoiskRu.Api.KinopoiskApiUnofficial.Model;
using EmbyKinopoiskRu.Api.KinopoiskApiUnofficial.Model.Film;
using EmbyKinopoiskRu.Api.KinopoiskApiUnofficial.Model.Person;
using EmbyKinopoiskRu.Api.KinopoiskApiUnofficial.Model.Season;
using EmbyKinopoiskRu.Helper;

using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Activity;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Providers;
using MediaBrowser.Model.Serialization;

namespace EmbyKinopoiskRu.Api.KinopoiskApiUnofficial
{
    public class KinopoiskUnofficialService : IKinopoiskRuService
    {
        private readonly ILogger _log;
        private readonly KinopoiskUnofficialApi _api;

        public KinopoiskUnofficialService(
            ILogManager logManager
            , IHttpClient httpClient
            , IJsonSerializer jsonSerializer
            , IActivityManager activityManager)
        {
            _log = logManager.GetLogger(GetType().Name);
            _api = new KinopoiskUnofficialApi(_log, httpClient, jsonSerializer, activityManager);
        }

        #region MovieProvider
        public async Task<MetadataResult<Movie>> GetMetadata(MovieInfo info, CancellationToken cancellationToken)
        {
            MetadataResult<Movie> result = new()
            {
                ResultLanguage = "ru"
            };

            if (string.IsNullOrWhiteSpace(Plugin.Instance?.Configuration.GetCurrentToken()))
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
                    KpFilm? movie = await _api.GetFilmById(movieId, cancellationToken);
                    if (movie != null)
                    {
                        await CreateMovie(result, movie, cancellationToken);
                        return result;
                    }
                    _log.Info($"Movie by movie id '{movieId}' not found");
                }
            }

            _log.Info($"Searching movies by name {info.Name}");
            KpSearchResult<KpFilm> movies = await _api.GetFilmsByName(info.Name, cancellationToken);
            if (movies.Items.Count != 1)
            {
                _log.Error($"Found {movies.Items.Count} movies, skipping movie update");
                return result;
            }
            await CreateMovie(result, movies.Items[0], cancellationToken);
            return result;
        }
        public async Task<IEnumerable<RemoteSearchResult>> GetSearchResults(MovieInfo searchInfo, CancellationToken cancellationToken)
        {
            List<RemoteSearchResult> result = new();

            if (string.IsNullOrWhiteSpace(Plugin.Instance?.Configuration.GetCurrentToken()))
            {
                _log.Warn($"The Token for {Plugin.PluginName} is empty");
                return result;
            }

            if (searchInfo.HasProviderId(Plugin.PluginName))
            {
                var movieId = searchInfo.GetProviderId(Plugin.PluginName);
                if (!string.IsNullOrWhiteSpace(movieId))
                {
                    _log.Info($"Searching movie by movie id '{movieId}'");
                    KpFilm? movie = await _api.GetFilmById(movieId, cancellationToken);
                    if (movie != null)
                    {
                        var imageUrl = (movie.PosterUrlPreview ?? movie.PosterUrl) ?? string.Empty;
                        RemoteSearchResult item = new()
                        {
                            Name = movie.NameRu,
                            ImageUrl = imageUrl,
                            SearchProviderName = Plugin.PluginName,
                            ProductionYear = movie.Year,
                            Overview = movie.Description,
                        };
                        item.SetProviderId(Plugin.PluginName, movieId);
                        result.Add(item);
                        _log.Info($"Found a movie with name {movie.NameRu} and id {movie.KinopoiskId}");
                        return result;
                    }
                    _log.Info($"Movie by movie id '{movieId}' not found");
                }
            }

            _log.Info($"Searching movies by name {searchInfo.Name}");
            KpSearchResult<KpFilm> movies = await _api.GetFilmsByName(searchInfo.Name, cancellationToken);
            foreach (KpFilm movie in movies.Items)
            {
                var imageUrl = (movie.PosterUrlPreview ?? movie.PosterUrl) ?? string.Empty;
                RemoteSearchResult item = new()
                {
                    Name = movie.NameRu,
                    ImageUrl = imageUrl,
                    SearchProviderName = Plugin.PluginName,
                    ProductionYear = movie.Year,
                    Overview = movie.Description,
                };
                item.SetProviderId(Plugin.PluginName, movie.KinopoiskId.ToString(CultureInfo.InvariantCulture));
                result.Add(item);
            }
            _log.Info($"By name '{searchInfo.Name}' found {result.Count} movies");
            return result;
        }
        public async Task<List<Movie>> GetMoviesByOriginalNameAndYear(string name, int? year, CancellationToken cancellationToken)
        {
            List<Movie> result = new();

            if (string.IsNullOrWhiteSpace(Plugin.Instance?.Configuration.GetCurrentToken()))
            {
                _log.Warn($"The Token for {Plugin.PluginName} is empty");
                return result;
            }

            KpSearchResult<KpFilm> movies = await _api.GetFilmsByName(name, cancellationToken);
            foreach (KpFilm movie in movies.Items)
            {
                result.Add(CreateMovieFromKpFilm(movie));
            }
            _log.Info($"By keywords '{name}' found {result.Count} movies");
            return result;
        }

        private async Task CreateMovie(MetadataResult<Movie> result, KpFilm movie, CancellationToken cancellationToken)
        {
            result.Item = CreateMovieFromKpFilm(movie);
            result.HasMetadata = true;

            var movieId = movie.KinopoiskId.ToString(CultureInfo.InvariantCulture);

            List<KpFilmStaff> staffList = await _api.GetStaffByFilmId(movieId, cancellationToken);
            if (staffList != null && staffList.Count > 0)
            {
                UpdatePersonsList(result, staffList, movie.NameRu);
            }

            List<KpVideo> videosList = await _api.GetVideosByFilmId(movieId, cancellationToken);
            if (result.HasMetadata && videosList != null && videosList.Count > 0)
            {
                videosList
                    .Where(i => !string.IsNullOrWhiteSpace(i.Url) && i.Url.Contains("youtube", StringComparison.OrdinalIgnoreCase))
                    .Select(i => i.Url!
                        .Replace("https://www.youtube.com/embed/", "https://www.youtube.com/watch?v=", StringComparison.OrdinalIgnoreCase)
                        .Replace("https://www.youtube.com/v/", "https://www.youtube.com/watch?v=", StringComparison.OrdinalIgnoreCase))
                    .Reverse()
                    .ToList()
                    .ForEach(v => result.Item.AddTrailerUrl(v));
            }

        }
        private Movie CreateMovieFromKpFilm(KpFilm movie)
        {
            _log.Info($"Movie '{movie.NameRu}' with {Plugin.PluginName} id '{movie.KinopoiskId}' found");

            var movieId = movie.KinopoiskId.ToString(CultureInfo.InvariantCulture);
            Movie toReturn = new()
            {
                CommunityRating = movie.RatingKinopoisk,
                CriticRating = movie.RatingFilmCritics * 10,
                ExternalId = movieId,
                Name = movie.NameRu,
                OfficialRating = movie.RatingMpaa,
                OriginalTitle = movie.NameOriginal,
                Overview = movie.Description,
                ProductionLocations = movie.Countries?.Select(i => i.Country).ToArray(),
                ProductionYear = movie.Year,
                SortName = string.IsNullOrWhiteSpace(movie.NameRu) ? movie.NameOriginal : movie.NameRu,
                Tagline = movie.Slogan
            };

            toReturn.SetProviderId(Plugin.PluginName, movieId);

            if (long.TryParse(movie.FilmLength?.ToString(CultureInfo.InvariantCulture), out var size))
            {
                toReturn.Size = size;
            }

            if (!string.IsNullOrWhiteSpace(movie.ImdbId))
            {
                toReturn.ProviderIds.Add(MetadataProviders.Imdb.ToString(), movie.ImdbId);
            }

            IEnumerable<string?>? genres = movie.Genres?
                .Select(i => i.Genre)
                .Where(i => !string.IsNullOrWhiteSpace(i))
                .AsEnumerable();
            if (genres != null)
            {
                toReturn.SetGenres(genres);
            }

            // IEnumerable<string?>? studios = movie.ProductionCompanies?.Select(i => i.Name).AsEnumerable();
            // if (studios != null)
            // {
            //     toReturn.SetStudios(studios);
            // }

            return toReturn;
        }
        #endregion

        #region MovieImagesProvider
        public async Task<IEnumerable<RemoteImageInfo>> GetImages(BaseItem item, LibraryOptions libraryOptions, CancellationToken cancellationToken)
        {
            List<RemoteImageInfo> result = new();

            if (string.IsNullOrWhiteSpace(Plugin.Instance?.Configuration.GetCurrentToken()))
            {
                _log.Warn($"The Token for {Plugin.PluginName} is empty");
                return result;
            }

            if (item.HasProviderId(Plugin.PluginName))
            {
                var movieId = item.GetProviderId(Plugin.PluginName);
                if (!string.IsNullOrWhiteSpace(movieId))
                {
                    _log.Info($"Searching movie by movie id '{movieId}'");
                    KpFilm? movie = await _api.GetFilmById(movieId, cancellationToken);
                    if (movie != null)
                    {
                        UpdateRemoteImageInfoList(movie, result);
                        return result;
                    };
                    _log.Info($"Images by movie id '{movieId}' not found");
                }
            }

            _log.Info($"Searching movies by name {item.Name}");
            KpSearchResult<KpFilm> movies = await _api.GetFilmsByName(item.Name, cancellationToken);
            if (movies.Items.Count != 1)
            {
                _log.Error($"Found {movies.Items.Count} movies, skipping image update");
                return result;
            }
            UpdateRemoteImageInfoList(movies.Items[0], result);
            return result;
        }

        private void UpdateRemoteImageInfoList(KpFilm movie, List<RemoteImageInfo> result)
        {
            if (!string.IsNullOrWhiteSpace(movie.CoverUrl))
            {
                result.Add(new RemoteImageInfo()
                {
                    ProviderName = Plugin.PluginName,
                    Url = movie.CoverUrl,
                    Language = "ru",
                    DisplayLanguage = "RU",
                    Type = ImageType.Backdrop
                });
            }
            if (!string.IsNullOrWhiteSpace(movie.PosterUrl))
            {
                result.Add(new RemoteImageInfo()
                {
                    ProviderName = Plugin.PluginName,
                    Url = movie.PosterUrl,
                    ThumbnailUrl = movie.PosterUrlPreview,
                    Language = "ru",
                    DisplayLanguage = "RU",
                    Type = ImageType.Primary
                });
            }
            if (!string.IsNullOrWhiteSpace(movie.LogoUrl))
            {
                result.Add(new RemoteImageInfo()
                {
                    ProviderName = Plugin.PluginName,
                    Url = movie.LogoUrl,
                    Language = "ru",
                    DisplayLanguage = "RU",
                    Type = ImageType.Logo
                });
            }
            _log.Info($"By movie id '{movie.KinopoiskId}' found '{string.Join(", ", result.Select(i => i.Type).ToList())}' image types");
        }
        #endregion

        #region SeriesProvider
        public async Task<MetadataResult<Series>> GetMetadata(SeriesInfo info, CancellationToken cancellationToken)
        {
            MetadataResult<Series> result = new()
            {
                ResultLanguage = "ru"
            };

            if (string.IsNullOrWhiteSpace(Plugin.Instance?.Configuration.GetCurrentToken()))
            {
                _log.Warn($"The Token for {Plugin.PluginName} is empty");
                return result;
            }

            if (info.HasProviderId(Plugin.PluginName))
            {
                var seriesId = info.GetProviderId(Plugin.PluginName);
                if (!string.IsNullOrWhiteSpace(seriesId))
                {
                    _log.Info($"Searching series by movie id '{seriesId}'");
                    KpFilm? item = await _api.GetFilmById(seriesId, cancellationToken);
                    if (item != null)
                    {
                        await CreateSeries(result, item, cancellationToken);
                        return result;
                    }
                    _log.Info($"Series by series id '{seriesId}' not found");
                }
            }

            _log.Info($"Searching series by name {info.Name}");
            KpSearchResult<KpFilm> series = await _api.GetFilmsByName(info.Name, cancellationToken);
            if (series.Items.Count != 1)
            {
                _log.Error($"Found {series.Items.Count} series, skipping series update");
                return result;
            }
            await CreateSeries(result, series.Items[0], cancellationToken);
            return result;
        }
        public async Task<IEnumerable<RemoteSearchResult>> GetSearchResults(SeriesInfo searchInfo, CancellationToken cancellationToken)
        {
            List<RemoteSearchResult> result = new();

            if (string.IsNullOrWhiteSpace(Plugin.Instance?.Configuration.GetCurrentToken()))
            {
                _log.Warn($"The Token for {Plugin.PluginName} is empty");
                return result;
            }

            if (searchInfo.HasProviderId(Plugin.PluginName))
            {
                var seriesId = searchInfo.GetProviderId(Plugin.PluginName);
                if (!string.IsNullOrWhiteSpace(seriesId))
                {
                    _log.Info($"Searching series by series id '{seriesId}'");
                    KpFilm? series = await _api.GetFilmById(seriesId, cancellationToken);
                    if (series != null)
                    {
                        var imageUrl = (series.PosterUrlPreview ?? series.PosterUrl) ?? string.Empty;
                        RemoteSearchResult item = new()
                        {
                            Name = series.NameRu,
                            ImageUrl = imageUrl,
                            SearchProviderName = Plugin.PluginName,
                            ProductionYear = series.Year,
                            Overview = series.Description,
                        };
                        item.SetProviderId(Plugin.PluginName, seriesId);
                        result.Add(item);
                        _log.Info($"Found a series with name {series.NameRu} and id {series.KinopoiskId}");
                        return result;
                    }
                    _log.Info($"Series by series id '{seriesId}' not found");
                }
            }

            _log.Info($"Searching series by name {searchInfo.Name}");
            KpSearchResult<KpFilm> seriesResult = await _api.GetFilmsByName(searchInfo.Name, cancellationToken);
            foreach (KpFilm series in seriesResult.Items)
            {
                var imageUrl = (series.PosterUrlPreview ?? series.PosterUrl) ?? string.Empty;
                RemoteSearchResult item = new()
                {
                    Name = series.NameRu,
                    ImageUrl = imageUrl,
                    SearchProviderName = Plugin.PluginName,
                    ProductionYear = series.Year,
                    Overview = series.Description,
                };
                item.SetProviderId(Plugin.PluginName, series.KinopoiskId.ToString(CultureInfo.InvariantCulture));
                result.Add(item);
            }
            _log.Info($"By name '{searchInfo.Name}' found {result.Count} series");
            return result;
        }

        private async Task CreateSeries(MetadataResult<Series> result, KpFilm film, CancellationToken cancellationToken)
        {
            result.Item = CreateSeriesFromKpFilm(film);
            result.HasMetadata = true;

            var seriesId = film.KinopoiskId.ToString(CultureInfo.InvariantCulture);

            List<KpFilmStaff> staffList = await _api.GetStaffByFilmId(seriesId, cancellationToken);
            if (staffList != null && staffList.Count > 0)
            {
                UpdatePersonsList(result, staffList, film.NameRu);
            }

            List<KpVideo> videosList = await _api.GetVideosByFilmId(seriesId, cancellationToken);
            if (result.HasMetadata && videosList != null && videosList.Count > 0)
            {
                videosList
                    .Where(i => !string.IsNullOrWhiteSpace(i.Url) && i.Url.Contains("youtube", StringComparison.OrdinalIgnoreCase))
                    .Select(i => i.Url!
                        .Replace("https://www.youtube.com/embed/", "https://www.youtube.com/watch?v=", StringComparison.OrdinalIgnoreCase)
                        .Replace("https://www.youtube.com/v/", "https://www.youtube.com/watch?v=", StringComparison.OrdinalIgnoreCase))
                    .Reverse()
                    .ToList()
                    .ForEach(v => result.Item.AddTrailerUrl(v));
            }
        }
        private Series CreateSeriesFromKpFilm(KpFilm series)
        {
            _log.Info($"Series '{series.NameRu}' with {Plugin.PluginName} id '{series.KinopoiskId}' found");

            var seriesId = series.KinopoiskId.ToString(CultureInfo.InvariantCulture);
            Series toReturn = new()
            {
                CommunityRating = series.RatingKinopoisk,
                CriticRating = series.RatingFilmCritics * 10,
                ExternalId = seriesId,
                Name = series.NameRu,
                OfficialRating = series.RatingMpaa,
                OriginalTitle = series.NameOriginal,
                Overview = series.Description,
                ProductionLocations = series.Countries?.Select(i => i.Country).ToArray(),
                ProductionYear = series.Year,
                SortName = string.IsNullOrWhiteSpace(series.NameRu) ? series.NameOriginal : series.NameRu,
                Tagline = series.Slogan,
            };

            toReturn.SetProviderId(Plugin.PluginName, seriesId);

            if (long.TryParse(series.FilmLength?.ToString(CultureInfo.InvariantCulture), out var size))
            {
                toReturn.Size = size;
            }

            if (!string.IsNullOrWhiteSpace(series.ImdbId))
            {
                toReturn.ProviderIds.Add(MetadataProviders.Imdb.ToString(), series.ImdbId);
            }

            IEnumerable<string?>? genres = series.Genres?
                .Select(i => i.Genre)
                .Where(i => !string.IsNullOrWhiteSpace(i))
                .AsEnumerable();
            if (genres != null)
            {
                toReturn.SetGenres(genres);
            }

            // IEnumerable<string?>? studios = series.ProductionCompanies?.Select(i => i.Name).AsEnumerable();
            // if (studios != null)
            // {
            //     toReturn.SetStudios(studios);
            // }

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

            if (string.IsNullOrWhiteSpace(Plugin.Instance?.Configuration.GetCurrentToken()))
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
            KpSearchResult<KpSeason>? item = await _api.GetEpisodesBySeriesId(seriesId, cancellationToken);
            if (item == null)
            {
                _log.Info($"Episodes by series id '{seriesId}' not found");
                return result;
            }
            KpSeason? kpSeason = item.Items.FirstOrDefault(s => s.Number == info.ParentIndexNumber);
            if (kpSeason == null)
            {
                _log.Info($"Season with index '{info.ParentIndexNumber}' not found");
                return result;
            }
            KpEpisode? kpEpisode = kpSeason.Episodes.FirstOrDefault(e =>
                e.EpisodeNumber == info.IndexNumber
                    && e.SeasonNumber == info.ParentIndexNumber);
            if (kpEpisode == null)
            {
                _log.Info($"Episode with index '{info.IndexNumber}' not found");
                return result;
            }
            _ = DateTimeOffset.TryParseExact(
                kpEpisode.ReleaseDate,
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateTimeOffset premiereDate);
            result.Item = new()
            {
                Name = kpEpisode.NameRu,
                OriginalTitle = kpEpisode.NameEn,
                Overview = kpEpisode.Synopsis,
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

            if (string.IsNullOrWhiteSpace(Plugin.Instance?.Configuration.GetCurrentToken()))
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
            if (persons.Items.Count != 1)
            {
                _log.Error($"Found {persons.Items.Count} persons, skipping person update");
                return result;
            }
            result.Item = CreatePersonFromKpPerson(persons.Items[0]);
            result.HasMetadata = true;
            return result;
        }
        public async Task<IEnumerable<RemoteSearchResult>> GetSearchResults(PersonLookupInfo searchInfo, CancellationToken cancellationToken)
        {
            List<RemoteSearchResult> result = new();

            if (string.IsNullOrWhiteSpace(Plugin.Instance?.Configuration.GetCurrentToken()))
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
                    KpPerson? person = await _api.GetPersonById(personId, cancellationToken);
                    if (person != null)
                    {
                        RemoteSearchResult item = new()
                        {
                            Name = person.NameRu,
                            ImageUrl = person.PosterUrl,
                        };
                        item.SetProviderId(Plugin.PluginName, personId);
                        result.Add(item);
                        return result;
                    }
                    _log.Info($"Person by id '{personId}' not found");
                }
            }

            _log.Info($"Searching persons by available metadata");
            KpSearchResult<KpPerson> persons = await _api.GetPersonsByName(searchInfo.Name, cancellationToken);
            foreach (KpPerson person in persons.Items)
            {
                RemoteSearchResult item = new()
                {
                    Name = person.NameRu,
                    ImageUrl = person.PosterUrl,
                };
                item.SetProviderId(Plugin.PluginName, person.PersonId.ToString(CultureInfo.InvariantCulture));
                result.Add(item);
            }
            _log.Info($"By name '{searchInfo.Name}' found {result.Count} persons");
            return result;
        }

        private Person CreatePersonFromKpPerson(KpPerson person)
        {
            _log.Info($"Person '{person.NameRu}' with KinopoiskId '{person.PersonId}' found");

            Person toReturn = new()
            {
                Name = person.NameRu,
                SortName = person.NameRu,
                OriginalTitle = person.NameEn,

            };
            if (DateTimeOffset.TryParse(person.Birthday, out DateTimeOffset birthDay))
            {
                toReturn.PremiereDate = birthDay;
            }
            if (DateTimeOffset.TryParse(person.Death, out DateTimeOffset deathDay))
            {
                toReturn.EndDate = deathDay;
            }
            if (!string.IsNullOrWhiteSpace(person.BirthPlace))
            {
                toReturn.ProductionLocations = new string[] { person.BirthPlace };
            }
            var facts = person.Facts?.ToArray();
            if (facts?.Length > 0)
            {
                toReturn.Overview = string.Join('\n', facts);
            }
            return toReturn;
        }

        #endregion

        #region Common
        private void UpdatePersonsList<T>(MetadataResult<T> result, List<KpFilmStaff> staffList, string? movieName)
            where T : BaseItem
        {
            foreach (KpFilmStaff staff in staffList)
            {
                PersonType? personType = KpHelper.GetPersonType(staff.ProfessionKey);
                var name = string.IsNullOrWhiteSpace(staff.NameRu) ? staff.NameEn : staff.NameRu;
                if (string.IsNullOrWhiteSpace(name))
                {
                    _log.Warn($"Skip adding staff with id '{staff.StaffId.ToString(CultureInfo.InvariantCulture)}' as nameless to '{movieName}'");
                }
                else if (personType == null)
                {
                    _log.Warn($"Skip adding '{name}' as '{staff.ProfessionKey}' to '{movieName}'");
                }
                else
                {
                    _log.Debug($"Adding '{name}' as '{personType}' to '{movieName}'");
                    PersonInfo person = new()
                    {
                        Name = name,
                        ImageUrl = staff.PosterUrl,
                        Type = (PersonType)personType,
                        Role = staff.Description,
                    };
                    person.SetProviderId(Plugin.PluginName, staff.StaffId.ToString(CultureInfo.InvariantCulture));

                    result.AddPerson(person);
                }
            }
            _log.Info($"Added {result.People.Count} persons to the movie with id '{result.Item.GetProviderId(Plugin.PluginName)}'");
        }

        #endregion

    }
}
