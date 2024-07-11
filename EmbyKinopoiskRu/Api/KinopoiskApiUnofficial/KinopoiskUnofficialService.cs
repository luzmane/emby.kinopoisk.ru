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
using MediaBrowser.Controller.Notifications;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Activity;
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
        private static PluginConfiguration PluginConfig => Plugin.Instance.Configuration;

        private static readonly List<KpLists> KpCollections = new List<KpLists>
        {
            new KpLists
            {
                Slug = "the_closest_releases",
                Name = "Ближайшие премьеры",
                Category = "Онлайн-кинотеатр"
            },
            new KpLists
            {
                Slug = "theme_comics",
                Name = "Лучшие фильмы, основанные на комиксах",
                Category = "Фильмы"
            },
            new KpLists
            {
                Slug = "theme_catastrophe",
                Name = "Фильмы-катастрофы",
                Category = "Фильмы"
            },
            new KpLists
            {
                Slug = "hd-family",
                Name = "Смотрим всей семьей",
                Category = "Онлайн-кинотеатр"
            },
            new KpLists
            {
                Slug = "theme_kids_animation",
                Name = "Мультфильмы для самых маленьких",
                Category = "Фильмы"
            },
            new KpLists
            {
                Slug = "theme_love",
                Name = "Фильмы про любовь и страсть: список лучших романтических фильмов",
                Category = "Фильмы"
            },
            new KpLists
            {
                Slug = "oscar_winners_2021",
                Name = "«Оскар-2021»: победители",
                Category = "Премии"
            },
            new KpLists
            {
                Slug = "series-top250",
                Name = "250 лучших сериалов",
                Category = "Сериалы"
            },
            new KpLists
            {
                Slug = "top250",
                Name = "250 лучших фильмов",
                Category = "Фильмы"
            },
            new KpLists
            {
                Slug = "popular-series",
                Name = "Популярные сериалы",
                Category = "Сериалы"
            },
            new KpLists
            {
                Slug = "popular-films",
                Name = "Популярные фильмы",
                Category = "Фильмы"
            },
            new KpLists
            {
                Slug = "theme_vampire",
                Name = "Фильмы про вампиров",
                Category = "Фильмы"
            },
            new KpLists
            {
                Slug = "theme_zombie",
                Name = "Фильмы про зомби: список лучших фильмов про живых мертвецов",
                Category = "Фильмы"
            }
        };

        private static readonly Dictionary<string, string> CollectionSlugMap = new Dictionary<string, string>
        {
            { "the_closest_releases", "CLOSES_RELEASES" },
            { "theme_comics", "COMICS_THEME" },
            { "theme_catastrophe", "CATASTROPHE_THEME" },
            { "hd-family", "FAMILY" },
            { "theme_kids_animation", "KIDS_ANIMATION_THEME" },
            { "theme_love", "LOVE_THEME" },
            { "oscar_winners_2021", "OSKAR_WINNERS_2021" },
            { "series-top250", "TOP_250_TV_SHOWS" },
            { "top250", "TOP_250_MOVIES" },
            { "popular-series", "TOP_POPULAR_ALL" },
            { "popular-films", "TOP_POPULAR_MOVIES" },
            { "theme_vampire", "VAMPIRE_THEME" },
            { "theme_zombie", "ZOMBIE_THEME" }
        };

        private static readonly List<string> MovieTypes = new List<string>
        {
            "FILM",
            "VIDEO"
        };

        internal KinopoiskUnofficialService(
            ILogManager logManager
            , IHttpClient httpClient
            , IJsonSerializer jsonSerializer
            , IActivityManager activityManager
            , INotificationManager notificationManager)
        {
            _log = logManager.GetLogger(GetType().Name);
            _api = new KinopoiskUnofficialApi(logManager, httpClient, jsonSerializer, activityManager, notificationManager);
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

            _log.Info("Movie not found by provider IDs or no provider IDs exist");

            // no name cleanup - search 'as is', otherwise doesn't work
            var name = info.Name?.Trim();
            _log.Info($"Searching movies by name '{name}' and year '{info.Year}'");
            KpSearchResult<KpFilm> movies = await _api.GetFilmsByNameAndYearAsync(info.Name, info.Year, cancellationToken);
            List<KpFilm> relevantMovies = FilterIrrelevantItems(movies.Items, name, info.Year, true);
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
                    Overview = movie.Description
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

            _log.Info("Movie not found by provider IDs or no provider IDs exist");

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
                    Overview = m.Description
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
            if (result.HasMetadata && !videosList.HasError && videosList.Items.Count > 0)
            {
                videosList.Items
                    .Where(i => !string.IsNullOrWhiteSpace(i.Url) && i.Url.Contains(Constants.Youtube))
                    .Select(i => i.Url
                        .Replace(Constants.YoutubeEmbed, Constants.YoutubeWatch)
                        .Replace(Constants.YoutubeV, Constants.YoutubeWatch))
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

        public async Task<IEnumerable<RemoteImageInfo>> GetImagesAsync(BaseItem item, CancellationToken cancellationToken)
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
            List<KpFilm> relevantMovies = FilterIrrelevantItems(movies.Items, item.Name, item.ProductionYear, true);
            if (relevantMovies.Count != 1)
            {
                _log.Error($"Found {relevantMovies.Count} movies, skipping image update");
                return result;
            }

            KpFilm film = await _api.GetFilmByIdAsync(relevantMovies[0].KinopoiskId.ToString(), cancellationToken);
            UpdateRemoteImageInfoList(film, result);
            return result;
        }

        private void UpdateRemoteImageInfoList(KpFilm movie, ICollection<RemoteImageInfo> result)
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

            _log.Info("Series was not found by provider ID");

            // no name cleanup - search 'as is', otherwise doesn't work
            _log.Info($"Searching series by name '{info.Name}' and year '{info.Year}'");
            KpSearchResult<KpFilm> series = await _api.GetFilmsByNameAndYearAsync(info.Name, info.Year, cancellationToken);
            List<KpFilm> relevantSeries = FilterIrrelevantItems(series.Items, info.Name, info.Year, false);
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
                    Overview = series.Description
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

            _log.Info("Series not found by provider ID");

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
                    Overview = s.Description
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
            if (result.HasMetadata && !videosList.HasError && videosList.Items.Count > 0)
            {
                videosList.Items
                    .Where(i => !string.IsNullOrWhiteSpace(i.Url) && i.Url.Contains(Constants.Youtube))
                    .Select(i => i.Url
                        .Replace(Constants.YoutubeEmbed, Constants.YoutubeWatch)
                        .Replace(Constants.YoutubeV, Constants.YoutubeWatch))
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
                Tagline = series.Slogan
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
            if (item.HasError)
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
                PremiereDate = premiereDate
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
            if (persons.HasError)
            {
                _log.Info($"Error searching person by name '{info.Name}'. Nothing found");
                return result;
            }

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
            if (persons.HasError)
            {
                _log.Info($"Error searching person by name '{searchInfo.Name}'. Nothing found");
                return result;
            }

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
                    new Dictionary<string, string>
                    {
                        { Plugin.PluginKey, person.PersonId.ToString() }
                    })
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
                        Role = staff.Description
                    };
                    person.SetProviderId(Plugin.PluginKey, staff.StaffId.ToString(CultureInfo.InvariantCulture));

                    result.AddPerson(person);
                }
            }

            _log.Info($"Added {result.People.Count} persons to the movie with id '{result.Item.GetProviderId(Plugin.PluginKey)}'");
        }

        private List<KpFilm> FilterIrrelevantItems(List<KpFilm> list, string name, int? year, bool isMovie)
        {
            _log.Info("Filtering out irrelevant items");
            if (list.Count > 1)
            {
                IEnumerable<KpFilm> toReturn = list
                    .Where(m => (!string.IsNullOrWhiteSpace(name)
                                 && KpHelper.CleanName(m.NameRu) == KpHelper.CleanName(name))
                                || KpHelper.CleanName(m.NameOriginal) == KpHelper.CleanName(name))
                    .Where(m => year == null || m.Year == year);
                if (!toReturn.Any())
                {
                    return list;
                }

                if (toReturn.Count() == 1)
                {
                    return toReturn.ToList();
                }

                IEnumerable<KpFilm> tmp = toReturn
                    .Where(m => isMovie ? "FILM".Equals(m.Type) : !"FILM".Equals(m.Type));
                return tmp.Any() ? tmp.ToList() : toReturn.ToList();
            }

            return list;
        }

        private List<KpStaff> FilterPersonsByName(string name, List<KpStaff> list)
        {
            _log.Info("Filtering out irrelevant persons");
            if (list.Count <= 1)
            {
                return list;
            }

            List<KpStaff> toReturn = list
                .Where(m => (!string.IsNullOrWhiteSpace(name)
                             && KpHelper.CleanName(m.NameRu) == KpHelper.CleanName(name))
                            || KpHelper.CleanName(m.NameEn) == KpHelper.CleanName(name))
                .ToList();
            return toReturn.Any() ? toReturn : list;
        }

        private async Task<KpFilm> GetKpFilmByProviderIdAsync(IHasProviderIds info, CancellationToken cancellationToken)
        {
            if (info.HasProviderId(Plugin.PluginKey) && !string.IsNullOrWhiteSpace(info.GetProviderId(Plugin.PluginKey)))
            {
                var movieId = info.GetProviderId(Plugin.PluginKey);
                _log.Info($"Searching movie by movie id '{movieId}'");
                return await _api.GetFilmByIdAsync(movieId, cancellationToken);
            }

            if (!info.HasProviderId(MetadataProviders.Imdb) || string.IsNullOrWhiteSpace(info.GetProviderId(MetadataProviders.Imdb)))
            {
                return null;
            }

            var imdbMovieId = info.GetProviderId(MetadataProviders.Imdb);
            _log.Info($"Searching Kp movie by IMDB '{imdbMovieId}'");
            KpSearchResult<KpFilm> kpSearchResult = await _api.GetFilmByImdbIdAsync(imdbMovieId, cancellationToken);
            if (kpSearchResult.Items.Count == 1)
            {
                return await _api.GetFilmByIdAsync(kpSearchResult.Items[0].KinopoiskId.ToString(), cancellationToken);
            }

            _log.Info($"Nothing was found by IMDB '{imdbMovieId}'. Skip search by IMDB ID");
            return null;
        }

        #endregion

        #region Scheduled Tasks

        public async Task<List<BaseItem>> GetCollectionItemsAsync(string collectionId, CancellationToken cancellationToken)
        {
            _log.Info($"Get collection items for '{collectionId}'");
            var toReturn = new List<BaseItem>();
            if (!CollectionSlugMap.TryGetValue(collectionId, out var slug))
            {
                _log.Error($"Unable to map '{collectionId}' to collection id of kinopoiskapiunofficial.tech");
                return toReturn;
            }

            _log.Info($"'{collectionId}' mapped to '{slug}'");

            List<KpFilm> movies = await GetAllCollectionItemsAsync(slug, cancellationToken);
            movies.ForEach(m =>
            {
                BaseItem item;
                if (MovieTypes.Contains(m.Type))
                {
                    item = new Movie
                    {
                        Name = m.NameRu,
                        OriginalTitle = m.NameOriginal
                    };
                    item.SetProviderId(Plugin.PluginKey, m.KinopoiskId.ToString(CultureInfo.InvariantCulture));
                    item.SetProviderId(MetadataProviders.Imdb.ToString(), m.ImdbId);
                }
                else
                {
                    item = new Series
                    {
                        Name = m.NameRu,
                        OriginalTitle = m.NameOriginal
                    };
                    item.SetProviderId(Plugin.PluginKey, m.KinopoiskId.ToString(CultureInfo.InvariantCulture));
                    item.SetProviderId(MetadataProviders.Imdb.ToString(), m.ImdbId);
                }

                toReturn.Add(item);
            });
            _log.Info($"Return {toReturn.Count} items for collection '{collectionId}'");
            return toReturn;
        }

        public async Task<ApiResult<Dictionary<string, long>>> GetKpIdByAnotherIdAsync(string externalIdType, IEnumerable<string> idList, CancellationToken cancellationToken)
        {
            if (!"imdb".Equals(externalIdType, StringComparison.InvariantCultureIgnoreCase))
            {
                _log.Info($"Not supported provider: '{externalIdType}'");
                return new ApiResult<Dictionary<string, long>>(new Dictionary<string, long>())
                {
                    HasError = true
                };
            }

            _log.Info($"Search Kinopoisk ID for {idList.Count()} items by {externalIdType} provider");
            var toReturn = new ApiResult<Dictionary<string, long>>(new Dictionary<string, long>());
            int errorCount = 0;
            const int errorThreshold = 10;
            foreach (var imdb in idList)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    _log.Info("Cancellation was requested. Return founded items");
                    break;
                }

                if (errorCount > errorThreshold)
                {
                    _log.Warn("Too much errors during KP IDs search by IMDB. Return founded items");
                    break;
                }

                KpSearchResult<KpFilm> movies = await _api.GetKpIdByImdbAsync(imdb, cancellationToken);
                if (movies.HasError || movies.Items.Count == 0)
                {
                    _log.Info($"Failed to get Kinopoisk ID by IMDB '{externalIdType}'");
                    errorCount++;
                    continue;
                }

                toReturn.Item.Add(movies.Items[0].ImdbId, movies.Items[0].KinopoiskId);
            }

            return toReturn;
        }

        public Task<List<KpLists>> GetKpCollectionsAsync(CancellationToken cancellationToken)
        {
            _log.Info("KinopoiskUnofficial doesn't have method to fetch collection, so list is hardcoded");
            return Task.FromResult(KpCollections);
        }

        private async Task<List<KpFilm>> GetAllCollectionItemsAsync(string collectionId, CancellationToken cancellationToken)
        {
            _log.Info($"Get all collection items for '{collectionId}'");
            var movies = new List<KpFilm>();
            KpSearchResult<KpFilm> tmp = await _api.GetCollectionItemsAsync(collectionId, 1, cancellationToken);
            if (tmp.HasError)
            {
                _log.Error($"Failed to fetch items list from API for collection '{collectionId}'");
                return movies;
            }

            if (tmp.Items.Count == 0)
            {
                _log.Info($"No items found for collection '{collectionId}'");
                return movies;
            }

            movies.AddRange(tmp.Items);
            for (var i = 2; i <= tmp.TotalPages; i++)
            {
                _log.Info($"Requesting page {i} of {tmp.TotalPages} pages ({movies.Count} of {tmp.Total} items) for collection '{collectionId}'");
                tmp = await _api.GetCollectionItemsAsync(collectionId, i, cancellationToken);
                if (tmp.HasError)
                {
                    _log.Warn($"Failed to fetch page {i} for collection '{collectionId}");
                    continue;
                }

                movies.AddRange(tmp.Items);
                _log.Info($"Fetched page {i} of {tmp.TotalPages} pages ({movies.Count} of {tmp.Total} items) for collection '{collectionId}'");
            }

            return movies;
        }

        public async Task<List<KpTrailer>> GetTrailersFromCollectionAsync(string collectionId, CancellationToken cancellationToken)
        {
            _log.Info($"Get trailers for '{collectionId}'");
            var toReturn = new HashSet<KpTrailer>(new KpTrailerComparer());
            if (!CollectionSlugMap.TryGetValue(collectionId, out var slug))
            {
                _log.Error($"Unable to map '{collectionId}' to collection id of kinopoiskapiunofficial.tech");
                return toReturn.ToList();
            }

            _log.Info($"'{collectionId}' mapped to '{slug}'");
            List<KpFilm> movies = await GetAllCollectionItemsAsync(slug, cancellationToken);
            foreach (var movie in movies)
            {
                ProviderIdDictionary providerIdDictionary = new ProviderIdDictionary
                {
                    { Plugin.PluginKey, movie.KinopoiskId.ToString() }
                };
                if (!string.IsNullOrWhiteSpace(movie.ImdbId))
                {
                    providerIdDictionary.Add(MetadataProviders.Imdb.ToString(), movie.ImdbId);
                }

                var tmp = await _api.GetVideosByFilmIdAsync(movie.KinopoiskId.ToString(), cancellationToken);
                if (tmp.HasError)
                {
                    _log.Error($"Failed to fetch trailers from API for videoId '{movie.KinopoiskId}'");
                }
                else
                {
                    _log.Debug($"Video with Id '{movie.KinopoiskId}' has '{tmp.Items.Count}' trailers");
                    tmp.Items.ForEach(t => toReturn.Add(new KpTrailer
                    {
                        ImageUrl = string.IsNullOrWhiteSpace(movie.PosterUrlPreview) ? movie.PosterUrl : movie.PosterUrlPreview,
                        VideoName = movie.NameRu,
                        TrailerName = t.Name,
                        Overview = movie.Description,
                        ProviderIds = providerIdDictionary,
                        Url = t.Url,
                        PremierDate = movie.Year > 1000 && movie.Year < 3000
                            ? new DateTimeOffset((int)movie.Year, 1, 1, 0, 0, 0, TimeSpan.Zero)
                            : (DateTimeOffset?)null
                    }));
                }
            }

            _log.Info($"Return {toReturn.Count} items for collection '{collectionId}'");

            return toReturn.ToList();
        }

        #endregion
    }
}
