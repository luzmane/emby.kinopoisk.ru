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
using EmbyKinopoiskRu.Configuration;
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
    internal sealed class KinopoiskUnofficialService : IKinopoiskRuService
    {
        private readonly ILogger _log;
        private readonly KinopoiskUnofficialApi _api;
        private PluginConfiguration PluginConfig { get; set; }

        internal KinopoiskUnofficialService(
            ILogManager logManager
            , IHttpClient httpClient
            , IJsonSerializer jsonSerializer
            , IActivityManager activityManager)
        {
            _log = logManager.GetLogger(GetType().Name);
            _api = new KinopoiskUnofficialApi(logManager, httpClient, jsonSerializer, activityManager);
            if (Plugin.Instance == null)
            {
                throw new NullReferenceException($"Plugin '{Plugin.PluginName}' instance is null");
            }
            PluginConfig = Plugin.Instance.Configuration;
        }

        #region MovieProvider
        public async Task<MetadataResult<Movie>> GetMetadataAsync(MovieInfo info, CancellationToken cancellationToken)
        {
            var result = new MetadataResult<Movie>
            {
                ResultLanguage = "ru"
            };

            if (string.IsNullOrWhiteSpace(PluginConfig.GetCurrentToken()))
            {
                _log.Warn($"The Token for {Plugin.PluginName} is empty");
                return result;
            }

            KpFilm movie = await GetKpFilmByProviderIdAsync(info, cancellationToken);
            if (movie != null)
            {
                _log.Info($"Movie found by provider ID, Kinopoisk ID: '{movie.KinopoiskId}'");
                await CreateMovieAsync(result, movie, cancellationToken);
                return result;
            }
            _log.Info($"Movie not found by provider IDs");

            // no name cleanup - search 'as is', otherwise doesn't work
            _log.Info($"Searching movies by name '{info.Name}' and year '{info.Year}'");
            KpSearchResult<KpFilm> movies = await _api.GetFilmsByNameAndYearAsync(info.Name, info.Year, cancellationToken);
            List<KpFilm> relevantMovies = FilterRelevantItems(movies.Items, info.Name, info.Year);
            if (relevantMovies.Count != 1)
            {
                _log.Error($"Found {relevantMovies.Count} movies, skipping movie update");
                return result;
            }
            KpFilm film = await _api.GetFilmByIdAsync(relevantMovies[0].KinopoiskId.ToString(), cancellationToken);
            await CreateMovieAsync(result, film, cancellationToken);
            return result;
        }
        public async Task<IEnumerable<RemoteSearchResult>> GetSearchResultsAsync(MovieInfo searchInfo, CancellationToken cancellationToken)
        {
            var result = new List<RemoteSearchResult>();

            if (string.IsNullOrWhiteSpace(PluginConfig.GetCurrentToken()))
            {
                _log.Warn($"The Token for {Plugin.PluginName} is empty");
                return result;
            }

            KpFilm movie = await GetKpFilmByProviderIdAsync(searchInfo, cancellationToken);
            if (movie != null)
            {
                var imageUrl = (movie.PosterUrlPreview ?? movie.PosterUrl) ?? string.Empty;
                var item = new RemoteSearchResult
                {
                    Name = movie.NameRu,
                    ImageUrl = imageUrl,
                    SearchProviderName = Plugin.PluginKey,
                    ProductionYear = movie.Year,
                    Overview = movie.Description,
                };
                item.SetProviderId(Plugin.PluginKey, movie.KinopoiskId.ToString());
                if (!string.IsNullOrWhiteSpace(movie.ImdbId))
                {
                    item.SetProviderId(MetadataProviders.Imdb, movie.ImdbId);
                }
                result.Add(item);
                _log.Info($"Found a movie with name '{movie.NameRu}' and id {movie.KinopoiskId}");
                return result;
            }
            _log.Info($"Movie not found by provider IDs");

            // no name cleanup - search 'as is', otherwise doesn't work
            _log.Info($"Searching movies by name '{searchInfo.Name}' and year '{searchInfo.Year}'");
            KpSearchResult<KpFilm> movies = await _api.GetFilmsByNameAndYearAsync(searchInfo.Name, searchInfo.Year, cancellationToken);
            foreach (KpFilm m in movies.Items)
            {
                var imageUrl = (m.PosterUrlPreview ?? m.PosterUrl) ?? string.Empty;
                var item = new RemoteSearchResult
                {
                    Name = m.NameRu,
                    ImageUrl = imageUrl,
                    SearchProviderName = Plugin.PluginKey,
                    ProductionYear = m.Year,
                    Overview = m.Description,
                };
                item.SetProviderId(Plugin.PluginKey, m.KinopoiskId.ToString(CultureInfo.InvariantCulture));
                if (!string.IsNullOrWhiteSpace(m.ImdbId))
                {
                    item.SetProviderId(MetadataProviders.Imdb, m.ImdbId);
                }
                result.Add(item);
            }
            _log.Info($"By name '{searchInfo.Name}' found {result.Count} movies");
            return result;
        }

        private async Task CreateMovieAsync(MetadataResult<Movie> result, KpFilm movie, CancellationToken cancellationToken)
        {
            result.Item = CreateMovieFromKpFilm(movie);
            result.HasMetadata = true;

            var movieId = movie.KinopoiskId.ToString(CultureInfo.InvariantCulture);

            List<KpFilmStaff> staffList = await _api.GetStaffByFilmIdAsync(movieId, cancellationToken);
            if (staffList != null && staffList.Count > 0)
            {
                UpdatePersonsList(result, staffList, movie.NameRu);
            }

            KpSearchResult<KpVideo> videosList = await _api.GetVideosByFilmIdAsync(movieId, cancellationToken);
            if (result.HasMetadata && videosList != null && videosList.Items.Count > 0)
            {
                videosList.Items
                    .Where(i => !string.IsNullOrWhiteSpace(i.Url) && i.Url.Contains(Constants.YOUTUBE))
                    .Select(i => i.Url
                        .Replace(Constants.YOUTUBE_EMBED, Constants.YOUTUBE_WATCH)
                        .Replace(Constants.YOUTUBE_V, Constants.YOUTUBE_WATCH))
                    .Reverse()
                    .ToList()
                    .ForEach(v => result.Item.AddTrailerUrl(v));
            }

        }
        private Movie CreateMovieFromKpFilm(KpFilm movie)
        {
            _log.Info($"Movie '{movie.NameRu}' with {Plugin.PluginName} id '{movie.KinopoiskId}' found");

            var movieId = movie.KinopoiskId.ToString(CultureInfo.InvariantCulture);
            var toReturn = new Movie
            {
                CommunityRating = movie.RatingKinopoisk,
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

            toReturn.SetProviderId(Plugin.PluginKey, movieId);
            if (!string.IsNullOrWhiteSpace(movie.ImdbId))
            {
                toReturn.SetProviderId(MetadataProviders.Imdb, movie.ImdbId);
            }

            if (long.TryParse(movie.FilmLength?.ToString(CultureInfo.InvariantCulture), out var size))
            {
                toReturn.Size = size;
            }

            IEnumerable<string> genres = movie.Genres?
                .Select(i => i.Genre)
                .Where(i => !string.IsNullOrWhiteSpace(i))
                .AsEnumerable();
            if (genres != null)
            {
                toReturn.SetGenres(genres);
            }

            return toReturn;
        }
        #endregion

        #region MovieImagesProvider
        public async Task<IEnumerable<RemoteImageInfo>> GetImagesAsync(BaseItem item, LibraryOptions libraryOptions, CancellationToken cancellationToken)
        {
            var result = new List<RemoteImageInfo>();

            if (string.IsNullOrWhiteSpace(PluginConfig.GetCurrentToken()))
            {
                _log.Warn($"The Token for {Plugin.PluginName} is empty");
                return result;
            }

            if (item.HasProviderId(Plugin.PluginKey))
            {
                var movieId = item.GetProviderId(Plugin.PluginKey);
                if (!string.IsNullOrWhiteSpace(movieId))
                {
                    _log.Info($"Searching movie by movie id '{movieId}'");
                    KpFilm movie = await _api.GetFilmByIdAsync(movieId, cancellationToken);
                    if (movie != null)
                    {
                        UpdateRemoteImageInfoList(movie, result);
                        return result;
                    }
                    _log.Info($"Images by movie id '{movieId}' not found");
                }
            }

            // no name cleanup - search 'as is', otherwise doesn't work
            _log.Info($"Searching movies by name '{item.Name}' and year '{item.ProductionYear}'");
            KpSearchResult<KpFilm> movies = await _api.GetFilmsByNameAndYearAsync(item.Name, item.ProductionYear, cancellationToken);
            List<KpFilm> relevantMovies = FilterRelevantItems(movies.Items, item.Name, item.ProductionYear);
            if (relevantMovies.Count != 1)
            {
                _log.Error($"Found {relevantMovies.Count} movies, skipping image update");
                return result;
            }
            KpFilm film = await _api.GetFilmByIdAsync(relevantMovies[0].KinopoiskId.ToString(), cancellationToken);
            UpdateRemoteImageInfoList(film, result);
            return result;
        }

        private void UpdateRemoteImageInfoList(KpFilm movie, List<RemoteImageInfo> result)
        {
            if (!string.IsNullOrWhiteSpace(movie.CoverUrl))
            {
                result.Add(new RemoteImageInfo
                {
                    ProviderName = Plugin.PluginKey,
                    Url = movie.CoverUrl,
                    Language = "ru",
                    DisplayLanguage = "RU",
                    Type = ImageType.Backdrop
                });
            }
            if (!string.IsNullOrWhiteSpace(movie.PosterUrl))
            {
                result.Add(new RemoteImageInfo
                {
                    ProviderName = Plugin.PluginKey,
                    Url = movie.PosterUrl,
                    ThumbnailUrl = movie.PosterUrlPreview,
                    Language = "ru",
                    DisplayLanguage = "RU",
                    Type = ImageType.Primary
                });
            }
            if (!string.IsNullOrWhiteSpace(movie.LogoUrl))
            {
                result.Add(new RemoteImageInfo
                {
                    ProviderName = Plugin.PluginKey,
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
        public async Task<MetadataResult<Series>> GetMetadataAsync(SeriesInfo info, CancellationToken cancellationToken)
        {
            var result = new MetadataResult<Series>
            {
                ResultLanguage = "ru"
            };

            if (string.IsNullOrWhiteSpace(PluginConfig.GetCurrentToken()))
            {
                _log.Warn($"The Token for {Plugin.PluginName} is empty");
                return result;
            }

            KpFilm movie = await GetKpFilmByProviderIdAsync(info, cancellationToken);
            if (movie != null)
            {
                _log.Info($"Series found by provider ID, Kinopoisk ID: '{movie.KinopoiskId}'");
                await CreateSeriesAsync(result, movie, cancellationToken);
                return result;
            }
            _log.Info($"Series was not found by provider ID");

            // no name cleanup - search 'as is', otherwise doesn't work
            _log.Info($"Searching series by name '{info.Name}' and year '{info.Year}'");
            KpSearchResult<KpFilm> series = await _api.GetFilmsByNameAndYearAsync(info.Name, info.Year, cancellationToken);
            List<KpFilm> relevantSeries = FilterRelevantItems(series.Items, info.Name, info.Year);
            if (relevantSeries.Count != 1)
            {
                _log.Error($"Found {relevantSeries.Count} series, skipping series update");
                return result;
            }
            KpFilm s = await _api.GetFilmByIdAsync(relevantSeries[0].KinopoiskId.ToString(), cancellationToken);
            await CreateSeriesAsync(result, s, cancellationToken);
            return result;
        }
        public async Task<IEnumerable<RemoteSearchResult>> GetSearchResultsAsync(SeriesInfo searchInfo, CancellationToken cancellationToken)
        {
            var result = new List<RemoteSearchResult>();

            if (string.IsNullOrWhiteSpace(PluginConfig.GetCurrentToken()))
            {
                _log.Warn($"The Token for {Plugin.PluginName} is empty");
                return result;
            }

            KpFilm series = await GetKpFilmByProviderIdAsync(searchInfo, cancellationToken);
            if (series != null)
            {
                var imageUrl = (series.PosterUrlPreview ?? series.PosterUrl) ?? string.Empty;
                var item = new RemoteSearchResult
                {
                    Name = series.NameRu,
                    ImageUrl = imageUrl,
                    SearchProviderName = Plugin.PluginKey,
                    ProductionYear = series.Year,
                    Overview = series.Description,
                };
                item.SetProviderId(Plugin.PluginKey, series.KinopoiskId.ToString());
                if (!string.IsNullOrWhiteSpace(series.ImdbId))
                {
                    item.SetProviderId(MetadataProviders.Imdb, series.ImdbId);
                }
                result.Add(item);
                _log.Info($"Found a series with name {series.NameRu} and id {series.KinopoiskId}");
                return result;
            }
            _log.Info($"Series not found by provider ID");

            // no name cleanup - search 'as is', otherwise doesn't work
            _log.Info($"Searching series by name '{searchInfo.Name}' and year '{searchInfo.Year}'");
            KpSearchResult<KpFilm> seriesResult = await _api.GetFilmsByNameAndYearAsync(searchInfo.Name, searchInfo.Year, cancellationToken);
            foreach (KpFilm s in seriesResult.Items)
            {
                var imageUrl = (s.PosterUrlPreview ?? s.PosterUrl) ?? string.Empty;
                var item = new RemoteSearchResult
                {
                    Name = s.NameRu,
                    ImageUrl = imageUrl,
                    SearchProviderName = Plugin.PluginKey,
                    ProductionYear = s.Year,
                    Overview = s.Description,
                };
                item.SetProviderId(Plugin.PluginKey, s.KinopoiskId.ToString(CultureInfo.InvariantCulture));
                if (!string.IsNullOrWhiteSpace(s.ImdbId))
                {
                    item.SetProviderId(MetadataProviders.Imdb, s.ImdbId);
                }
                result.Add(item);
            }
            _log.Info($"By name '{searchInfo.Name}' found {result.Count} series");
            return result;
        }

        private async Task CreateSeriesAsync(MetadataResult<Series> result, KpFilm film, CancellationToken cancellationToken)
        {
            result.Item = CreateSeriesFromKpFilm(film);
            result.HasMetadata = true;

            var seriesId = film.KinopoiskId.ToString(CultureInfo.InvariantCulture);

            List<KpFilmStaff> staffList = await _api.GetStaffByFilmIdAsync(seriesId, cancellationToken);
            if (staffList != null && staffList.Count > 0)
            {
                UpdatePersonsList(result, staffList, film.NameRu);
            }

            KpSearchResult<KpVideo> videosList = await _api.GetVideosByFilmIdAsync(seriesId, cancellationToken);
            if (result.HasMetadata && videosList != null && videosList.Items.Count > 0)
            {
                videosList.Items
                    .Where(i => !string.IsNullOrWhiteSpace(i.Url) && i.Url.Contains(Constants.YOUTUBE))
                    .Select(i => i.Url
                        .Replace(Constants.YOUTUBE_EMBED, Constants.YOUTUBE_WATCH)
                        .Replace(Constants.YOUTUBE_V, Constants.YOUTUBE_WATCH))
                    .Reverse()
                    .ToList()
                    .ForEach(v => result.Item.AddTrailerUrl(v));
            }
        }
        private Series CreateSeriesFromKpFilm(KpFilm series)
        {
            _log.Info($"Series '{series.NameRu}' with {Plugin.PluginName} id '{series.KinopoiskId}' found");

            var seriesId = series.KinopoiskId.ToString(CultureInfo.InvariantCulture);
            var toReturn = new Series
            {
                CommunityRating = series.RatingKinopoisk,
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

            toReturn.SetProviderId(Plugin.PluginKey, seriesId);
            if (!string.IsNullOrWhiteSpace(series.ImdbId))
            {
                toReturn.SetProviderId(MetadataProviders.Imdb, series.ImdbId);
            }

            if (long.TryParse(series.FilmLength?.ToString(CultureInfo.InvariantCulture), out var size))
            {
                toReturn.Size = size;
            }

            IEnumerable<string> genres = series.Genres?
                .Select(i => i.Genre)
                .Where(i => !string.IsNullOrWhiteSpace(i))
                .AsEnumerable();
            if (genres != null)
            {
                toReturn.SetGenres(genres);
            }

            return toReturn;
        }
        #endregion

        #region EpisodeProvider
        public async Task<MetadataResult<Episode>> GetMetadataAsync(EpisodeInfo info, CancellationToken cancellationToken)
        {
            var result = new MetadataResult<Episode>
            {
                ResultLanguage = "ru"
            };

            if (string.IsNullOrWhiteSpace(PluginConfig.GetCurrentToken()))
            {
                _log.Warn($"The Token for {Plugin.PluginName} is empty");
                return result;
            }

            var seriesId = info.GetSeriesProviderId(Plugin.PluginKey);
            if (string.IsNullOrWhiteSpace(seriesId))
            {
                _log.Debug($"SeriesProviderId not exists for {Plugin.PluginName}, checking ProviderId");
                seriesId = info.GetProviderId(Plugin.PluginKey);
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
            KpSearchResult<KpSeason> item = await _api.GetEpisodesBySeriesIdAsync(seriesId, cancellationToken);
            if (item == null)
            {
                _log.Info($"Episodes by series id '{seriesId}' not found");
                return result;
            }
            KpSeason kpSeason = item.Items.FirstOrDefault(s => s.Number == info.ParentIndexNumber);
            if (kpSeason == null)
            {
                _log.Info($"Season with index '{info.ParentIndexNumber}' not found");
                return result;
            }
            KpEpisode kpEpisode = kpSeason.Episodes.FirstOrDefault(e =>
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
            result.Item = new Episode
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
        public Task<IEnumerable<RemoteSearchResult>> GetSearchResultsAsync(EpisodeInfo searchInfo, CancellationToken cancellationToken)
        {
            _log.Info("KinopoiskUnofficial unable to search episodes");
            return Task.FromResult(Enumerable.Empty<RemoteSearchResult>());
        }

        #endregion

        #region PersonProvider
        public async Task<MetadataResult<Person>> GetMetadataAsync(PersonLookupInfo info, CancellationToken cancellationToken)
        {
            var result = new MetadataResult<Person>
            {
                ResultLanguage = "ru"
            };

            if (string.IsNullOrWhiteSpace(PluginConfig.GetCurrentToken()))
            {
                _log.Warn($"The Token for {Plugin.PluginName} is empty");
                return result;
            }

            if (info.ProviderIds.TryGetValue(Plugin.PluginKey, out var personId))
            {
                _log.Info($"Fetching person by person id '{personId}'");
                KpPerson person = await _api.GetPersonByIdAsync(personId, cancellationToken);
                if (person != null)
                {
                    result.Item = CreatePersonFromKpPerson(person);
                    result.HasMetadata = true;
                    return result;
                }
                _log.Info($"Person by person id '{personId}' not found");
            }

            _log.Info($"Searching person by name '{info.Name}'");
            KpSearchResult<KpStaff> persons = await _api.GetPersonsByNameAsync(info.Name, cancellationToken);
            persons.Items = FilterPersonsByName(info.Name, persons.Items);
            if (persons.Items.Count != 1)
            {
                _log.Error($"Found {persons.Items.Count} persons, skipping person update");
                return result;
            }
            KpPerson p = await _api.GetPersonByIdAsync(persons.Items[0].KinopoiskId.ToString(), cancellationToken);
            result.Item = CreatePersonFromKpPerson(p);
            result.HasMetadata = true;
            return result;
        }
        public async Task<IEnumerable<RemoteSearchResult>> GetSearchResultsAsync(PersonLookupInfo searchInfo, CancellationToken cancellationToken)
        {
            var result = new List<RemoteSearchResult>();

            if (string.IsNullOrWhiteSpace(PluginConfig.GetCurrentToken()))
            {
                _log.Warn($"The Token for {Plugin.PluginName} is empty");
                return result;
            }

            if (searchInfo.ProviderIds.TryGetValue(Plugin.PluginKey, out var personId))
            {
                _log.Info($"Searching person by id '{personId}'");
                KpPerson person = await _api.GetPersonByIdAsync(personId, cancellationToken);
                if (person != null)
                {
                    var item = new RemoteSearchResult
                    {
                        Name = person.NameRu,
                        ImageUrl = person.PosterUrl,
                        SearchProviderName = Plugin.PluginKey
                    };
                    item.SetProviderId(Plugin.PluginKey, personId);
                    result.Add(item);
                    return result;
                }
                _log.Info($"Person by id '{personId}' not found");
            }

            _log.Info($"Searching persons by name '{searchInfo.Name}'");
            KpSearchResult<KpStaff> persons = await _api.GetPersonsByNameAsync(searchInfo.Name, cancellationToken);
            foreach (KpStaff person in persons.Items)
            {
                var item = new RemoteSearchResult
                {
                    Name = person.NameRu,
                    ImageUrl = person.PosterUrl,
                    SearchProviderName = Plugin.PluginKey
                };
                item.SetProviderId(Plugin.PluginKey, person.KinopoiskId.ToString(CultureInfo.InvariantCulture));
                result.Add(item);
            }
            _log.Info($"By name '{searchInfo.Name}' found {result.Count} persons");
            return result;
        }

        private Person CreatePersonFromKpPerson(KpPerson person)
        {
            _log.Info($"Person '{person.NameRu}' with KinopoiskId '{person.PersonId}' found");

            var toReturn = new Person
            {
                Name = person.NameRu,
                SortName = person.NameRu,
                OriginalTitle = person.NameEn,
                ProviderIds = new ProviderIdDictionary(
                    new Dictionary<string, string> { { Plugin.PluginKey, person.PersonId.ToString() } })
            };
            if (DateTimeOffset.TryParse(person.Birthday, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTimeOffset birthDay))
            {
                toReturn.PremiereDate = birthDay;
            }
            if (DateTimeOffset.TryParse(person.Death, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTimeOffset deathDay))
            {
                toReturn.EndDate = deathDay;
            }
            if (!string.IsNullOrWhiteSpace(person.BirthPlace))
            {
                toReturn.ProductionLocations = new[] { person.BirthPlace };
            }
            var facts = person.Facts?.ToArray();
            if (facts?.Length > 0)
            {
                toReturn.Overview = string.Join("\n", facts);
            }
            return toReturn;
        }

        #endregion

        #region Common
        private void UpdatePersonsList<T>(MetadataResult<T> result, List<KpFilmStaff> staffList, string movieName)
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
                    var person = new PersonInfo
                    {
                        Name = name,
                        ImageUrl = staff.PosterUrl,
                        Type = (PersonType)personType,
                        Role = staff.Description,
                    };
                    person.SetProviderId(Plugin.PluginKey, staff.StaffId.ToString(CultureInfo.InvariantCulture));

                    result.AddPerson(person);
                }
            }
            _log.Info($"Added {result.People.Count} persons to the movie with id '{result.Item.GetProviderId(Plugin.PluginKey)}'");
        }
        private List<KpFilm> FilterRelevantItems(List<KpFilm> list, string name, int? year)
        {
            _log.Info("Filtering out irrelevant items");
            if (list.Count > 1)
            {
                var toReturn = list
                    .Where(m => (!string.IsNullOrWhiteSpace(name)
                        && (KpHelper.CleanName(m.NameRu) == KpHelper.CleanName(name)))
                            || KpHelper.CleanName(m.NameOriginal) == KpHelper.CleanName(name))
                    .Where(m => year == null || m.Year == year)
                    .ToList();
                return toReturn.Any() ? toReturn : list;
            }
            else
            {
                return list;
            }
        }
        private List<KpStaff> FilterPersonsByName(string name, List<KpStaff> list)
        {
            _log.Info("Filtering out irrelevant persons");
            if (list.Count > 1)
            {
                var toReturn = list
                    .Where(m => (!string.IsNullOrWhiteSpace(name)
                        && (KpHelper.CleanName(m.NameRu) == KpHelper.CleanName(name)))
                            || KpHelper.CleanName(m.NameEn) == KpHelper.CleanName(name))
                    .ToList();
                return toReturn.Any() ? toReturn : list;
            }
            else
            {
                return list;
            }
        }
        private async Task<KpFilm> GetKpFilmByProviderIdAsync(ItemLookupInfo info, CancellationToken cancellationToken)
        {
            if (info.HasProviderId(Plugin.PluginKey) && !string.IsNullOrWhiteSpace(info.GetProviderId(Plugin.PluginKey)))
            {
                var movieId = info.GetProviderId(Plugin.PluginKey);
                _log.Info($"Searching movie by movie id '{movieId}'");
                return await _api.GetFilmByIdAsync(movieId, cancellationToken);
            }

            if (info.HasProviderId(MetadataProviders.Imdb) && !string.IsNullOrWhiteSpace(info.GetProviderId(MetadataProviders.Imdb)))
            {
                var imdbMovieId = info.GetProviderId(MetadataProviders.Imdb);
                _log.Info($"Searching Kp movie by IMDB '{imdbMovieId}'");
                KpSearchResult<KpFilm> kpSearchResult = await _api.GetFilmByImdbIdAsync(imdbMovieId, cancellationToken);
                if (kpSearchResult.Items.Count != 1)
                {
                    _log.Info($"Nothing was found by IMDB '{imdbMovieId}'. Skip search by IMDB ID");
                    return null;
                }
                return await _api.GetFilmByIdAsync(kpSearchResult.Items[0].KinopoiskId.ToString(), cancellationToken);
            }

            return null;
        }

        #endregion

        #region Scheduled Tasks
        public Task<List<BaseItem>> GetCollectionItemsAsync(string collectionId, CancellationToken cancellationToken)
        {
            _log.Info($"KinopoiskUnofficial doesn't have information about items in collection '{collectionId}'");
            return Task.FromResult(new List<BaseItem>());
        }
        public Task<ApiResult<Dictionary<string, long>>> GetKpIdByAnotherIdAsync(string externalIdType, IEnumerable<string> idList, CancellationToken cancellationToken)
        {
            _log.Info("KinopoiskUnofficial unable to search by IMDB nor by TMDB");
            return Task.FromResult(new ApiResult<Dictionary<string, long>>(new Dictionary<string, long>()));
        }
        public Task<List<KpLists>> GetKpCollectionsAsync()
        {
            _log.Info("KinopoiskUnofficial doesn't have information about Kinopoisk collections");
            return Task.FromResult(Array.Empty<KpLists>().ToList());
        }

        #endregion
    }
}
