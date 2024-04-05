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
using MediaBrowser.Controller.Notifications;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Activity;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Providers;
using MediaBrowser.Model.Serialization;


namespace EmbyKinopoiskRu.Api.KinopoiskDev
{
    internal sealed class KinopoiskDevService : IKinopoiskRuService
    {
        private static PluginConfiguration PluginConfig => Plugin.Instance.Configuration;
        private readonly ILogger _log;
        private readonly KinopoiskDevApi _api;
        private readonly ILibraryManager _libraryManager;
        private readonly ICollectionManager _collectionManager;

        private static readonly List<KpMovieType?> MovieTypes = new List<KpMovieType?> { KpMovieType.Anime, KpMovieType.Movie, KpMovieType.Cartoon };

        internal KinopoiskDevService(
            ILogManager logManager
            , IHttpClient httpClient
            , IJsonSerializer jsonSerializer
            , IActivityManager activityManager
            , ILibraryManager libraryManager
            , ICollectionManager collectionManager
            , INotificationManager notificationManager)
        {
            _log = logManager.GetLogger(GetType().Name);
            _api = new KinopoiskDevApi(logManager, httpClient, jsonSerializer, activityManager, notificationManager);
            _libraryManager = libraryManager;
            _collectionManager = collectionManager;
        }

        #region MovieProvider

        public async Task<MetadataResult<Movie>> GetMetadataAsync(MovieInfo info, CancellationToken cancellationToken)
        {
            var result = new MetadataResult<Movie> { ResultLanguage = "ru" };

            if (string.IsNullOrWhiteSpace(PluginConfig.GetCurrentToken()))
            {
                _log.Warn($"The Token for {Plugin.PluginName} is empty");
                return result;
            }

            KpMovie movie = await GetKpMovieByProviderIdAsync(info, cancellationToken);
            if (movie != null)
            {
                _log.Info($"Movie found by provider ID, Kinopoisk ID: '{movie.Id}'");
                result.Item = await CreateMovieFromKpMovieAsync(movie, cancellationToken);
                UpdatePersonsList(result, movie.Persons);
                result.HasMetadata = true;
                return result;
            }

            _log.Info("Movie was not found by provider IDs or provider IDs not exist");

            var name = KpHelper.CleanName(info.Name);
            _log.Info($"Searching movie by name '{name}' and year '{info.Year}'");
            KpSearchResult<KpMovie> movies = await _api.GetMoviesByMovieDetailsAsync(name, info.Year, cancellationToken);
            List<KpMovie> relevantMovies = FilterIrrelevantItems(movies.Docs, name, info.Year, name, true);
            _log.Info($"{relevantMovies.Count} movies left after filtering");
            if (relevantMovies.Count == 1)
            {
                movie = relevantMovies[0];
            }
            else if (relevantMovies.Count > 1)
            {
                movie = relevantMovies
                    .Where(m => m.Rating?.Kp != null)
                    .OrderByDescending(m => m.Rating.Kp)
                    .FirstOrDefault();
                // all films without KP rating
                movie = movie ?? relevantMovies[0];
            }

            if (movie == null)
            {
                return result;
            }

            KpMovie film = await _api.GetMovieByIdAsync(movie.Id.ToString(), cancellationToken);
            if (film != null)
            {
                result.Item = await CreateMovieFromKpMovieAsync(film, cancellationToken);
                UpdatePersonsList(result, film.Persons);
                result.HasMetadata = true;
            }
            else
            {
                _log.Warn($"Unable to fetch info about valid Kinopoisk ID: '{movie.Id}'");
            }

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

            KpMovie movie = await GetKpMovieByProviderIdAsync(searchInfo, cancellationToken);
            if (movie != null)
            {
                _log.Info($"Movie found by provider ID, Kinopoisk ID: '{movie.Id}'");
                result.Add(CreateRemoteSearchResultFromKpMovie(movie));
                return result;
            }

            _log.Info("Movie was not found by provider IDs or provider IDs not exist");

            var name = KpHelper.CleanName(searchInfo.Name);
            _log.Info($"Searching movie by name '{name}' and year '{searchInfo.Year}'");
            KpSearchResult<KpMovie> movies = await _api.GetMoviesByMovieDetailsAsync(name, searchInfo.Year, cancellationToken);
            result.AddRange(movies.Docs.Select(CreateRemoteSearchResultFromKpMovie));

            _log.Info($"By name '{name}' found {result.Count} movies");
            return result;
        }

        private static RemoteSearchResult CreateRemoteSearchResultFromKpMovie(KpMovie movie)
        {
            var item = new RemoteSearchResult
            {
                Name = movie.Name,
                ImageUrl = (movie.Poster?.PreviewUrl ?? movie.Poster?.Url) ?? string.Empty,
                SearchProviderName = Plugin.PluginKey,
                ProductionYear = movie.Year,
                Overview = movie.Description
            };
            item.SetProviderId(Plugin.PluginKey, movie.Id.ToString());
            if (!string.IsNullOrWhiteSpace(movie.ExternalId?.Imdb))
            {
                item.ProviderIds.Add(MetadataProviders.Imdb.ToString(), movie.ExternalId.Imdb);
            }

            if (movie.ExternalId?.Tmdb != null)
            {
                item.ProviderIds.Add(MetadataProviders.Tmdb.ToString(), movie.ExternalId.Tmdb.ToString());
            }

            return item;
        }

        private async Task<Movie> CreateMovieFromKpMovieAsync(KpMovie movie, CancellationToken cancellationToken)
        {
            _log.Info($"Movie '{movie.Name}' with {Plugin.PluginName} id '{movie.Id}' found");

            var movieId = movie.Id.ToString(CultureInfo.InvariantCulture);
            var toReturn = new Movie
            {
                CommunityRating = movie.Rating?.Kp,
                ExternalId = movieId,
                Name = movie.Name,
                OfficialRating = movie.RatingMpaa,
                OriginalTitle = movie.AlternativeName,
                Overview = PrepareOverview(movie),
                PremiereDate = KpHelper.GetPremierDate(movie.Premiere),
                ProductionLocations = movie.Countries?.Select(i => i.Name).ToArray(),
                ProductionYear = movie.Year,
                Size = movie.MovieLength ?? 0,
                SortName =
                    string.IsNullOrWhiteSpace(movie.Name)
                        ? string.IsNullOrWhiteSpace(movie.AlternativeName)
                            ? string.Empty
                            : movie.AlternativeName
                        : movie.Name,
                Tagline = movie.Slogan
            };

            toReturn.SetProviderId(Plugin.PluginKey, movieId);

            if (!string.IsNullOrWhiteSpace(movie.ExternalId?.Imdb))
            {
                toReturn.ProviderIds.Add(MetadataProviders.Imdb.ToString(), movie.ExternalId.Imdb);
            }

            if (movie.ExternalId?.Tmdb != null)
            {
                toReturn.ProviderIds.Add(MetadataProviders.Tmdb.ToString(), movie.ExternalId.Tmdb.ToString());
            }

            IEnumerable<string> genres = movie.Genres?.Select(i => i.Name);
            if (genres != null)
            {
                toReturn.SetGenres(genres);
            }

            IEnumerable<string> studios = movie.ProductionCompanies?.Select(i => i.Name);
            if (studios != null)
            {
                toReturn.SetStudios(studios);
            }

            movie.Videos?.Teasers?
                .Where(i => !string.IsNullOrWhiteSpace(i.Url) && i.Url.Contains(Constants.Youtube))
                .Select(i => i.Url
                    .Replace(Constants.YoutubeEmbed, Constants.YoutubeWatch)
                    .Replace(Constants.YoutubeV, Constants.YoutubeWatch))
                .Reverse()
                .ToList()
                .ForEach(j => toReturn.AddTrailerUrl(j));
            movie.Videos?.Trailers?
                .Where(i => !string.IsNullOrWhiteSpace(i.Url) && i.Url.Contains(Constants.Youtube))
                .Select(i => i.Url
                    .Replace(Constants.YoutubeEmbed, Constants.YoutubeWatch)
                    .Replace(Constants.YoutubeV, Constants.YoutubeWatch))
                .Reverse()
                .ToList()
                .ForEach(j => toReturn.AddTrailerUrl(j));

            if (PluginConfig.NeedToCreateSequenceCollection() && movie.SequelsAndPrequels != null && movie.SequelsAndPrequels.Any())
            {
                await AddMovieToCollectionAsync(toReturn, movie, cancellationToken);
            }

            return toReturn;
        }

        #endregion

        #region SeriesProvider

        public async Task<MetadataResult<Series>> GetMetadataAsync(SeriesInfo info, CancellationToken cancellationToken)
        {
            var result = new MetadataResult<Series> { ResultLanguage = "ru" };

            if (string.IsNullOrWhiteSpace(PluginConfig.GetCurrentToken()))
            {
                _log.Warn($"The Token for {Plugin.PluginName} is empty");
                return result;
            }

            KpMovie item = await GetKpMovieByProviderIdAsync(info, cancellationToken);
            if (item != null)
            {
                _log.Info($"Series found by provider ID, Kinopoisk ID: '{item.Id}'");
                result.Item = await CreateSeriesFromKpMovieAsync(item, cancellationToken);
                UpdatePersonsList(result, item.Persons);
                result.HasMetadata = true;
                return result;
            }

            _log.Info("Series was not found by provider ID");

            var name = KpHelper.CleanName(info.Name);
            _log.Info($"Searching series by name '{name}' and year '{info.Year}'");
            KpSearchResult<KpMovie> series = await _api.GetMoviesByMovieDetailsAsync(name, info.Year, cancellationToken);
            List<KpMovie> relevantSeries = FilterIrrelevantItems(series.Docs, name, info.Year, name, false);
            _log.Info($"{relevantSeries.Count} series left after filtering");
            if (!relevantSeries.Any())
            {
                _log.Error("Found 0 series, skipping movie update");
                return result;
            }

            KpMovie serial = await _api.GetMovieByIdAsync(relevantSeries[0].Id.ToString(), cancellationToken);
            if (serial != null)
            {
                result.Item = await CreateSeriesFromKpMovieAsync(serial, cancellationToken);
                UpdatePersonsList(result, serial.Persons);
                result.HasMetadata = true;
            }
            else
            {
                _log.Warn($"Unable to fetch info about valid Kinopoisk ID: '{relevantSeries[0].Id}'");
            }

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

            KpMovie series = await GetKpMovieByProviderIdAsync(searchInfo, cancellationToken);
            if (series != null)
            {
                _log.Info($"Series found by provider ID, Kinopoisk ID: '{series.Id}'");
                result.Add(CreateSeriesRemoteSearchResult(series));
                return result;
            }

            _log.Info("Series was not found by provider ID");

            var name = KpHelper.CleanName(searchInfo.Name);
            _log.Info($"Searching series by name '{name}' and year '{searchInfo.Year}'");
            KpSearchResult<KpMovie> seriesResult = await _api.GetMoviesByMovieDetailsAsync(name, searchInfo.Year, cancellationToken);
            result.AddRange(seriesResult.Docs.Select(CreateSeriesRemoteSearchResult));

            _log.Info($"By name '{name}' found {result.Count} series");
            return result;
        }

        private static RemoteSearchResult CreateSeriesRemoteSearchResult(KpMovie series)
        {
            var item = new RemoteSearchResult
            {
                Name = series.Name,
                ImageUrl = (series.Poster?.PreviewUrl ?? series.Poster?.Url) ?? string.Empty,
                SearchProviderName = Plugin.PluginKey,
                ProductionYear = series.Year,
                Overview = series.Description
            };
            item.SetProviderId(Plugin.PluginKey, series.Id.ToString());
            if (!string.IsNullOrWhiteSpace(series.ExternalId?.Imdb))
            {
                item.ProviderIds.Add(MetadataProviders.Imdb.ToString(), series.ExternalId.Imdb);
            }

            if (series.ExternalId?.Tmdb != null)
            {
                item.ProviderIds.Add(MetadataProviders.Tmdb.ToString(), series.ExternalId.Tmdb.ToString());
            }

            return item;
        }

        private async Task<Series> CreateSeriesFromKpMovieAsync(KpMovie series, CancellationToken cancellationToken)
        {
            _log.Info($"Series '{series.Name}' with KinopoiskId '{series.Id}' found");

            var seriesId = series.Id.ToString(CultureInfo.InvariantCulture);
            var toReturn = new Series
            {
                CommunityRating = series.Rating?.Kp,
                EndDate = GetEndDate(series.ReleaseYears),
                ExternalId = seriesId,
                Name = series.Name,
                OfficialRating = series.RatingMpaa,
                OriginalTitle = series.AlternativeName,
                Overview = PrepareOverview(series),
                PremiereDate = KpHelper.GetPremierDate(series.Premiere),
                ProductionLocations = series.Countries?.Select(i => i.Name).ToArray(),
                ProductionYear = series.Year,
                Size = series.MovieLength ?? 0,
                SortName =
                    string.IsNullOrWhiteSpace(series.Name)
                        ? string.IsNullOrWhiteSpace(series.AlternativeName)
                            ? string.Empty
                            : series.AlternativeName
                        : series.Name,
                Tagline = series.Slogan
            };

            toReturn.SetProviderId(Plugin.PluginKey, seriesId);

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
                .Where(i => !string.IsNullOrWhiteSpace(i.Url) && i.Url.Contains(Constants.Youtube))
                .Select(i => i.Url
                    .Replace(Constants.YoutubeEmbed, Constants.YoutubeWatch)
                    .Replace(Constants.YoutubeV, Constants.YoutubeWatch))
                .Reverse()
                .ToList()
                .ForEach(j => toReturn.AddTrailerUrl(j));
            series.Videos?.Trailers?
                .Where(i => !string.IsNullOrWhiteSpace(i.Url) && i.Url.Contains(Constants.Youtube))
                .Select(i => i.Url
                    .Replace(Constants.YoutubeEmbed, Constants.YoutubeWatch)
                    .Replace(Constants.YoutubeV, Constants.YoutubeWatch))
                .Reverse()
                .ToList()
                .ForEach(j => toReturn.AddTrailerUrl(j));

            if (PluginConfig.NeedToCreateSequenceCollection() && series.SequelsAndPrequels != null && series.SequelsAndPrequels.Any())
            {
                await AddMovieToCollectionAsync(toReturn, series, cancellationToken);
            }

            return toReturn;
        }

        #endregion

        #region EpisodeProvider

        public async Task<MetadataResult<Episode>> GetMetadataAsync(EpisodeInfo info, CancellationToken cancellationToken)
        {
            var result = new MetadataResult<Episode> { ResultLanguage = "ru" };

            if (string.IsNullOrWhiteSpace(PluginConfig.GetCurrentToken()))
            {
                _log.Warn($"The Token for {Plugin.PluginName} is empty");
                return result;
            }

            var seriesId = info.GetSeriesProviderId(Plugin.PluginKey);
            if (string.IsNullOrWhiteSpace(seriesId))
            {
                _log.Info($"SeriesProviderId not exists for {Plugin.PluginName}, checking ProviderId");
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
            KpSearchResult<KpSeason> item = await _api.GetSeasonBySeriesIdAsync(seriesId, (int)info.ParentIndexNumber, cancellationToken);
            KpSeason kpSeason = item?.Docs.FirstOrDefault(s => s.Number == info.ParentIndexNumber);
            if (kpSeason == null)
            {
                _log.Info($"Season with index '{info.ParentIndexNumber}' not found for series with id '{seriesId}'");
                return result;
            }

            KpEpisode kpEpisode = kpSeason.Episodes?.FirstOrDefault(e => e.Number == info.IndexNumber);
            if (kpEpisode == null)
            {
                _log.Info($"Episode with index '{info.IndexNumber}' not found in season '{info.ParentIndexNumber}' and series '{seriesId}'");
                return result;
            }

            DateTimeOffset? premiereDate = null;
            if (DateTimeOffset.TryParseExact(kpEpisode.AirDate, "yyyy-MM-dd'T'HH:mm:ss.fffZ", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTimeOffset tmp))
            {
                premiereDate = tmp;
            }

            result.Item = new Episode
            {
                Name = kpEpisode.Name,
                OriginalTitle = kpEpisode.EnName,
                Overview = kpEpisode.Description,
                IndexNumber = info.IndexNumber,
                ParentIndexNumber = info.ParentIndexNumber,
                PremiereDate = premiereDate
            };
            result.HasMetadata = true;
            _log.Info($"Episode {info.IndexNumber} of season {info.ParentIndexNumber} of series {seriesId} found");
            return result;
        }

        public async Task<IEnumerable<RemoteSearchResult>> GetSearchResultsAsync(EpisodeInfo searchInfo, CancellationToken cancellationToken)
        {
            var result = new List<RemoteSearchResult>();

            if (string.IsNullOrWhiteSpace(PluginConfig.GetCurrentToken()))
            {
                _log.Warn($"The Token for {Plugin.PluginName} is empty");
                return result;
            }

            if (!VerifyEpisodeInfoInput(searchInfo))
            {
                return Enumerable.Empty<RemoteSearchResult>();
            }

            var seriesId = searchInfo.GetSeriesProviderId(Plugin.PluginKey);
            if (string.IsNullOrWhiteSpace(seriesId))
            {
                _log.Info($"SeriesProviderId not exists for {Plugin.PluginName}, checking ProviderId");
                seriesId = searchInfo.GetProviderId(Plugin.PluginKey);
            }

            _log.Info($"Searching episode by SeriesId '{seriesId}', season number '{searchInfo.ParentIndexNumber}' and episode number '{searchInfo.IndexNumber}'");
            KpSearchResult<KpSeason> kpSearchResult = await _api.GetSeasonBySeriesIdAsync(seriesId, searchInfo.ParentIndexNumber.Value, cancellationToken);
            if (kpSearchResult == null || kpSearchResult.HasError || !kpSearchResult.Docs.Any())
            {
                _log.Info($"Nothing found for SeriesId '{seriesId}', season number '{searchInfo.ParentIndexNumber}'");
                return Enumerable.Empty<RemoteSearchResult>();
            }

            KpSeason season = kpSearchResult.Docs[0];
            KpEpisode episode = season.Episodes.FirstOrDefault(x => x.Number == searchInfo.IndexNumber);
            if (episode == null)
            {
                _log.Info($"Season '{searchInfo.ParentIndexNumber}' doesn't have episode '{searchInfo.IndexNumber}'");
                return Enumerable.Empty<RemoteSearchResult>();
            }

            var item = new RemoteSearchResult
            {
                Name = episode.Name,
                ImageUrl = (episode.Still?.PreviewUrl ?? episode.Still?.Url) ?? string.Empty,
                IndexNumber = episode.Number,
                ParentIndexNumber = searchInfo.ParentIndexNumber,
                SearchProviderName = Plugin.PluginKey,
                Overview = episode.Description
            };
            item.SetProviderId(Plugin.PluginKey, seriesId);
            if (DateTimeOffset.TryParseExact(
                    episode.AirDate,
                    KpHelper.PremierDateFormat,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out DateTimeOffset premierDate))
            {
                item.PremiereDate = premierDate;
                item.ProductionYear = premierDate.Year;
            }

            result.Add(item);
            _log.Info($"Found metadata for seriesId '{seriesId}', season number '{searchInfo.ParentIndexNumber}' and episode number '{searchInfo.IndexNumber}'");
            return result;
        }

        private bool VerifyEpisodeInfoInput(EpisodeInfo episodeInfo)
        {
            var toReturn = true;
            if (episodeInfo.ParentIndexNumber == null || episodeInfo.ParentIndexNumber < 0)
            {
                _log.Error($"Unable to search for episode - season number is '{episodeInfo.ParentIndexNumber}'");
                toReturn = false;
            }

            if (episodeInfo.IndexNumber == null || episodeInfo.IndexNumber < 0)
            {
                _log.Error($"Unable to search for episode - episode number is '{episodeInfo.IndexNumber}'");
                toReturn = false;
            }

            var seriesId = episodeInfo.GetSeriesProviderId(Plugin.PluginKey);
            if (string.IsNullOrWhiteSpace(seriesId))
            {
                _log.Info($"SeriesProviderId not exists for {Plugin.PluginName}, checking ProviderId");
                seriesId = episodeInfo.GetProviderId(Plugin.PluginKey);
            }

            if (!string.IsNullOrWhiteSpace(seriesId))
            {
                return toReturn;
            }

            _log.Error("Unable to search for episode - SeriesProviderIds and ProviderIds numbers aren't exist");
            return false;
        }

        #endregion

        #region PersonProvider

        public async Task<MetadataResult<Person>> GetMetadataAsync(PersonLookupInfo info, CancellationToken cancellationToken)
        {
            var result = new MetadataResult<Person> { ResultLanguage = "ru" };

            if (string.IsNullOrWhiteSpace(PluginConfig.GetCurrentToken()))
            {
                _log.Warn($"The Token for {Plugin.PluginName} is empty");
                return result;
            }

            KpPerson person;
            if (info.ProviderIds.TryGetValue(Plugin.PluginKey, out var personId))
            {
                _log.Info($"Fetching person by person id '{personId}'");
                person = await _api.GetPersonByIdAsync(personId, cancellationToken);
                if (person != null)
                {
                    result.Item = CreatePersonFromKpPerson(person);
                    result.HasMetadata = true;
                    return result;
                }

                _log.Info($"Person by person id '{personId}' not found");
            }

            _log.Info($"Searching person by name {info.Name}");
            KpSearchResult<KpPerson> persons = await _api.GetPersonsByNameAsync(info.Name, cancellationToken);
            List<KpPerson> relevantPersons = FilterIrrelevantPersons(persons.Docs, info.Name);
            if (!relevantPersons.Any())
            {
                _log.Error("Found 0 persons, skipping movie update");
                return result;
            }

            person = await _api.GetPersonByIdAsync(relevantPersons[0].Id.ToString(), cancellationToken);
            if (person != null)
            {
                result.Item = CreatePersonFromKpPerson(person);
                result.HasMetadata = true;
            }
            else
            {
                _log.Warn($"Unable to fetch person by valid Kinopoisk id: '{relevantPersons[0].Id}'");
            }

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
                    result.Add(CreateRemoteSearchResultFromKpPerson(person));
                    return result;
                }

                _log.Info($"Person by id '{personId}' not found");
            }

            _log.Info($"Searching person by name '{searchInfo.Name}'");
            KpSearchResult<KpPerson> persons = await _api.GetPersonsByNameAsync(searchInfo.Name, cancellationToken);
            persons.Docs = FilterIrrelevantPersons(persons.Docs, searchInfo.Name);
            persons.Docs.ForEach(x => result.Add(CreateRemoteSearchResultFromKpPerson(x)));
            _log.Info($"By name '{searchInfo.Name}' found {result.Count} persons");
            return result;
        }

        private Person CreatePersonFromKpPerson(KpPerson person)
        {
            _log.Info($"Person '{person.Name}' with KinopoiskId '{person.Id}' found");

            var toReturn = new Person { Name = person.Name, SortName = person.Name, OriginalTitle = person.EnName };
            toReturn.ProviderIds.Add(Plugin.PluginKey, person.Id.ToString());
            if (DateTimeOffset.TryParse(person.Birthday, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTimeOffset birthDay))
            {
                toReturn.PremiereDate = birthDay;
            }

            if (DateTimeOffset.TryParse(person.Death, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTimeOffset deathDay))
            {
                toReturn.EndDate = deathDay;
            }

            var birthPlace = person.BirthPlace?.Select(i => i.Value).ToArray();
            if (birthPlace != null && birthPlace.Length > 0 && !string.IsNullOrWhiteSpace(birthPlace[0]))
            {
                toReturn.ProductionLocations = new[] { string.Join(", ", birthPlace) };
            }

            var facts = person.Facts?.Select(i => i.Value).ToArray();
            if (facts != null && facts.Length > 0 && !string.IsNullOrWhiteSpace(facts[0]))
            {
                toReturn.Overview = string.Join("\n", facts);
            }

            return toReturn;
        }

        private List<KpPerson> FilterIrrelevantPersons(List<KpPerson> list, string name)
        {
            _log.Info("Filtering out irrelevant persons");
            if (list.Count <= 1)
            {
                return list;
            }

            List<KpPerson> toReturn = list
                .Where(m =>
                    KpHelper.CleanName(m.Name) == KpHelper.CleanName(name)
                    || KpHelper.CleanName(m.EnName) == KpHelper.CleanName(name))
                .ToList();
            if (toReturn.Count > 1)
            {
                toReturn = toReturn
                    .Where(m => !string.IsNullOrWhiteSpace(m.Photo))
                    .ToList();
            }

            return toReturn.Any() ? toReturn : list;
        }

        private static RemoteSearchResult CreateRemoteSearchResultFromKpPerson(KpPerson person)
        {
            var item = new RemoteSearchResult { Name = person.Name, ImageUrl = person.Photo, SearchProviderName = Plugin.PluginKey };
            item.SetProviderId(Plugin.PluginKey, person.Id.ToString());
            if (DateTimeOffset.TryParseExact(
                    person.Birthday,
                    KpHelper.PremierDateFormat,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out DateTimeOffset birthday))
            {
                item.PremiereDate = birthday;
            }

            return item;
        }

        #endregion

        #region MovieImageProvider

        public async Task<IEnumerable<RemoteImageInfo>> GetImagesAsync(BaseItem item, CancellationToken cancellationToken)
        {
            var result = new List<RemoteImageInfo>();

            if (string.IsNullOrWhiteSpace(PluginConfig.GetCurrentToken()))
            {
                _log.Warn($"The Token for {Plugin.PluginName} is empty");
                return result;
            }

            KpMovie movie;
            if (item.ProviderIds.TryGetValue(Plugin.PluginKey, out var movieId))
            {
                _log.Info($"Searching images by movie id '{movieId}'");
                movie = await _api.GetMovieByIdAsync(movieId, cancellationToken);
                if (movie != null)
                {
                    UpdateRemoteImageInfoList(movie, result);
                    return result;
                }

                _log.Info($"Images by movie id '{movieId}' not found");
            }

            var name = KpHelper.CleanName(item.Name);
            var originalTitle = KpHelper.CleanName(item.OriginalTitle);
            _log.Info($"Searching images by name: '{name}', originalTitle: '{originalTitle}', productionYear: '{item.ProductionYear}'");
            KpSearchResult<KpMovie> movies = await _api.GetMoviesByMovieDetailsAsync(name, originalTitle, item.ProductionYear, cancellationToken);
            _log.Info("Filtering out irrelevant films");
            List<KpMovie> filteredItems = FilterIrrelevantItems(movies.Docs, name, item.ProductionYear, originalTitle, true);
            movie = filteredItems.FirstOrDefault();
            if (movie == null)
            {
                _log.Error("Nothing were found");
                return result;
            }

            UpdateRemoteImageInfoList(movie, result);
            return result;
        }

        private void UpdateRemoteImageInfoList(KpMovie movie, ICollection<RemoteImageInfo> toReturn)
        {
            if (!string.IsNullOrWhiteSpace(movie.Backdrop?.Url))
            {
                toReturn.Add(new RemoteImageInfo
                {
                    ProviderName = Plugin.PluginKey,
                    Url = movie.Backdrop.Url,
                    ThumbnailUrl = movie.Backdrop.PreviewUrl,
                    Language = "ru",
                    DisplayLanguage = "RU",
                    Type = ImageType.Backdrop
                });
            }

            if (!string.IsNullOrWhiteSpace(movie.Poster?.Url))
            {
                toReturn.Add(new RemoteImageInfo
                {
                    ProviderName = Plugin.PluginKey,
                    Url = movie.Poster.Url,
                    ThumbnailUrl = movie.Poster.PreviewUrl,
                    Language = "ru",
                    DisplayLanguage = "RU",
                    Type = ImageType.Primary
                });
            }

            if (!string.IsNullOrWhiteSpace(movie.Logo?.Url))
            {
                toReturn.Add(new RemoteImageInfo
                {
                    ProviderName = Plugin.PluginKey,
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

        private static DateTimeOffset? GetEndDate(IReadOnlyCollection<KpYearRange> releaseYears)
        {
            if (releaseYears == null || releaseYears.Count == 0)
            {
                return null;
            }

            var max = int.MaxValue;
            releaseYears
                .Where(i => i.End != null)
                .ToList()
                .ForEach(i => max = Math.Min(max, i.End.Value));

            return DateTimeOffset.TryParseExact(
                max.ToString(CultureInfo.InvariantCulture),
                "yyyy",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateTimeOffset result)
                ? result
                : (DateTimeOffset?)null;
        }

        private void UpdatePersonsList<T>(MetadataResult<T> result, List<KpPersonMovie> persons) where T : BaseItem
        {
            var itemId = result.Item.GetProviderId(Plugin.PluginKey);
            if (persons == null)
            {
                _log.Warn($"Received persons list is null for video with id '{itemId}'");
                return;
            }

            _log.Info($"Updating persons list of the video with id '{itemId}'");
            foreach (KpPersonMovie kpPerson in persons)
            {
                PersonType? personType = KpHelper.GetPersonType(kpPerson.EnProfession);
                var personName = string.IsNullOrWhiteSpace(kpPerson.Name) ? kpPerson.EnName : kpPerson.Name;
                if (string.IsNullOrWhiteSpace(personName))
                {
                    _log.Warn($"Skip adding staff with id '{kpPerson.Id.ToString(CultureInfo.InvariantCulture)}' as nameless to '{result.Item.Name}'");
                }
                else if (personType == null)
                {
                    _log.Warn($"Skip adding '{personName}' as '{kpPerson.EnProfession}' to '{result.Item.Name}'");
                }
                else
                {
                    _log.Debug($"Adding '{personName}' as '{personType}' to '{result.Item.Name}'");
                    var person = new PersonInfo { Name = personName, ImageUrl = kpPerson.Photo, Type = (PersonType)personType, Role = kpPerson.Description };
                    person.SetProviderId(Plugin.PluginKey, kpPerson.Id.ToString(CultureInfo.InvariantCulture));

                    result.AddPerson(person);
                }
            }

            _log.Info($"Added {result.People.Count} persons to the video with id '{itemId}'");
        }

        private async Task AddMovieToCollectionAsync(BaseItem embyItem, KpMovie movie, CancellationToken cancellationToken)
        {
            _log.Info($"Adding '{embyItem.Name}' to collection");
            CollectionFolder rootCollectionFolder = EmbyHelper.InsureCollectionLibraryFolderAsync(_libraryManager, _log);
            if (rootCollectionFolder == null)
            {
                _log.Error("The virtual folder 'Collections' was not found nor created");
                return;
            }

            // Get internalIds of each object in sequence
            KpSearchResult<KpMovie> sequels = await _api.GetMoviesByIdsAsync(movie.SequelsAndPrequels.Select(seq => seq.Id.ToString(CultureInfo.InvariantCulture)), cancellationToken);
            List<KeyValuePair<string, string>> providerIds = sequels.Docs
                .SelectMany(m =>
                {
                    var list = new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>(Plugin.PluginKey, m.Id.ToString(CultureInfo.InvariantCulture)) };
                    if (m.ExternalId.Tmdb != null)
                    {
                        list.Add(new KeyValuePair<string, string>(MetadataProviders.Tmdb.ToString(), m.ExternalId.Tmdb.ToString()));
                    }

                    if (!string.IsNullOrWhiteSpace(m.ExternalId.Imdb))
                    {
                        list.Add(new KeyValuePair<string, string>(MetadataProviders.Imdb.ToString(), m.ExternalId.Imdb));
                    }

                    return list;
                })
                .ToList();
            BaseItem[] internalCollectionItems = EmbyHelper.GetItemsByProviderIds(providerIds, _libraryManager);
            if (internalCollectionItems.Length == 0)
            {
                _log.Info($"No sequels, nor prequels were found in libraries for '{embyItem.Name}'. Skip collection creation");
                return;
            }

            // Choose collection name
            sequels.Docs.Add(movie);
            KpMovie firstMovie = sequels.Docs
                .Where(m => m.Year != null)
                .OrderBy(m => m.Year)
                .FirstOrDefault();
            var collectionName = firstMovie == null ? movie.Name : firstMovie.Name;

            // Create or update collection
            BaseItem currentCollection = EmbyHelper.GetCollectionByName(_libraryManager, collectionName).Items.FirstOrDefault();
            if (currentCollection == null)
            {
                _log.Info($"Creating '{collectionName}' collection with following items: '{string.Join("', '", internalCollectionItems.Select(m => m.Name))}'");
                currentCollection = await _collectionManager.CreateCollection(new CollectionCreationOptions { IsLocked = false, Name = collectionName, ParentId = rootCollectionFolder.InternalId, ItemIdList = internalCollectionItems.Select(i => i.InternalId).ToArray() });
                if (currentCollection == null)
                {
                    _log.Error($"Unable to create new collection '{collectionName}'");
                    return;
                }
            }
            else
            {
                _log.Info($"Updating '{collectionName}' collection with following items: '{string.Join("', '", internalCollectionItems.Select(m => m.Name))}'");
                foreach (BaseItem item in internalCollectionItems)
                {
                    if (item.AddCollection((BoxSet)currentCollection))
                    {
                        _log.Info($"Adding '{item.Name}' to collection '{collectionName}'");
                        item.UpdateToRepository(ItemUpdateType.MetadataEdit);
                    }
                    else
                    {
                        _log.Info($"'{item.Name}' already in the collection '{collectionName}'");
                    }
                }
            }

            _ = embyItem.AddCollection((BoxSet)currentCollection);

            _log.Info("Finished adding to collection");
        }

        private static string PrepareOverview(KpMovie movie)
        {
            const string subj = "<br/><br/><b>Интересное:</b><br/>";
            var sb = new StringBuilder(subj);
            movie.Facts?
                .Where(f => !f.Spoiler && "FACT".Equals(f.Type, StringComparison.OrdinalIgnoreCase))
                .ToList()
                .ForEach(f => sb.Append("&nbsp;&nbsp;&nbsp;&nbsp;* ").Append(f.Value).Append("<br/>"));

            return sb.Length == subj.Length
                ? movie.Description
                : sb.Insert(0, movie.Description).ToString();
        }

        private List<KpMovie> FilterIrrelevantItems(List<KpMovie> list, string name, int? year, string alternativeName, bool isMovie)
        {
            _log.Info("Filtering out irrelevant items");
            if (list.Count <= 1)
            {
                return list;
            }

            IEnumerable<KpMovie> toReturn = list
                .Where(m =>
                    KpHelper.CleanName(m.Name) == KpHelper.CleanName(name)
                    || KpHelper.CleanName(m.Name) == KpHelper.CleanName(alternativeName)
                    || KpHelper.CleanName(m.AlternativeName) == KpHelper.CleanName(name)
                    || KpHelper.CleanName(m.AlternativeName) == KpHelper.CleanName(alternativeName))
                .Where(m => year == null || m.Year == year);
            if (!toReturn.Any())
            {
                return list;
            }

            if (toReturn.Count() == 1)
            {
                return toReturn.ToList();
            }

            IEnumerable<KpMovie> tmp = toReturn
                .Where(m => isMovie ? MovieTypes.Contains(m.TypeNumber) : !MovieTypes.Contains(m.TypeNumber));
            return tmp.Any() ? tmp.ToList() : toReturn.ToList();
        }

        private async Task<KpMovie> GetKpMovieByProviderIdAsync(IHasProviderIds info, CancellationToken cancellationToken)
        {
            if (info.ProviderIds.TryGetValue(Plugin.PluginKey, out var providerId))
            {
                _log.Info($"Searching Kp movie by id '{providerId}'");
                return await _api.GetMovieByIdAsync(providerId, cancellationToken);
            }

            providerId = info.GetProviderId(MetadataProviders.Imdb);
            if (!string.IsNullOrWhiteSpace(providerId))
            {
                _log.Info($"Searching Kp movie by {MetadataProviders.Imdb} id '{providerId}'");
                ApiResult<Dictionary<string, long>> kpDictionary = await GetKpIdByAnotherIdAsync(MetadataProviders.Imdb.ToString(), new List<string> { providerId }, cancellationToken);
                if (kpDictionary.HasError || kpDictionary.Item.Count != 1)
                {
                    _log.Info($"Failed to get Kinopoisk ID by {MetadataProviders.Imdb} ID '{providerId}'");
                }
                else
                {
                    var movieId = kpDictionary.Item[providerId].ToString();
                    _log.Info($"Kinopoisk ID is '{movieId}' for {MetadataProviders.Imdb} ID '{providerId}'");
                    return await _api.GetMovieByIdAsync(movieId, cancellationToken);
                }
            }

            providerId = info.GetProviderId(MetadataProviders.Tmdb);
            if (string.IsNullOrWhiteSpace(providerId))
            {
                return null;
            }

            _log.Info($"Searching Kp movie by {MetadataProviders.Tmdb} id '{providerId}'");
            ApiResult<Dictionary<string, long>> apiResult = await GetKpIdByAnotherIdAsync(MetadataProviders.Tmdb.ToString(), new List<string> { providerId }, cancellationToken);
            if (apiResult.HasError || apiResult.Item.Count != 1)
            {
                _log.Info($"Failed to get Kinopoisk ID by {MetadataProviders.Tmdb} ID '{providerId}'");
            }
            else
            {
                var movieId = apiResult.Item[providerId].ToString();
                _log.Info($"Kinopoisk ID is '{movieId}' for {MetadataProviders.Tmdb} ID '{providerId}'");
                return await _api.GetMovieByIdAsync(movieId, cancellationToken);
            }
            return null;
        }

        #endregion

        #region Scheduled Tasks

        public async Task<List<BaseItem>> GetCollectionItemsAsync(string collectionId, CancellationToken cancellationToken)
        {
            _log.Info($"Get collection items for '{collectionId}'");
            var toReturn = new List<BaseItem>();
            List<KpMovie> movies = await GetAllCollectionItemsAsync(collectionId, cancellationToken);
            movies.ForEach(m =>
            {
                BaseItem item;
                if (MovieTypes.Contains(m.TypeNumber))
                {
                    item = new Movie { Name = m.Name, OriginalTitle = m.AlternativeName };
                    item.SetProviderId(Plugin.PluginKey, m.Id.ToString(CultureInfo.InvariantCulture));
                    item.SetProviderId(MetadataProviders.Imdb.ToString(), m.ExternalId?.Imdb);
                    if (m.ExternalId?.Tmdb != null && m.ExternalId.Tmdb > 0)
                    {
                        item.SetProviderId(MetadataProviders.Tmdb.ToString(), m.ExternalId.Tmdb.ToString());
                    }
                }
                else
                {
                    item = new Series { Name = m.Name, OriginalTitle = m.AlternativeName };
                    item.SetProviderId(Plugin.PluginKey, m.Id.ToString(CultureInfo.InvariantCulture));
                    item.SetProviderId(MetadataProviders.Imdb.ToString(), m.ExternalId?.Imdb);
                    if (m.ExternalId?.Tmdb != null && m.ExternalId.Tmdb > 0)
                    {
                        item.SetProviderId(MetadataProviders.Tmdb.ToString(), m.ExternalId.Tmdb.ToString());
                    }
                }

                toReturn.Add(item);
            });
            _log.Info($"Return {toReturn.Count} items for collection '{collectionId}'");
            return toReturn;
        }

        public async Task<ApiResult<Dictionary<string, long>>> GetKpIdByAnotherIdAsync(string externalIdType, IEnumerable<string> idList, CancellationToken cancellationToken)
        {
            _log.Info($"Search Kinopoisk ID for {idList.Count()} items by {externalIdType} provider");
            KpSearchResult<KpMovie> movies = await _api.GetKpIdByAnotherIdAsync(externalIdType, idList, cancellationToken);
            if (movies.HasError)
            {
                _log.Info($"Failed to get Kinopoisk ID by {externalIdType} provider");
                return new ApiResult<Dictionary<string, long>>(new Dictionary<string, long>()) { HasError = true };
            }

            _log.Info($"Found {movies.Docs.Count} Kinopoisk IDs for {idList.Count()} items by {externalIdType} provider");
            if (MetadataProviders.Imdb.ToString() == externalIdType)
            {
                return new ApiResult<Dictionary<string, long>>(movies.Docs
                    .Where(m => !string.IsNullOrWhiteSpace(m.ExternalId?.Imdb))
                    .Select(m => new KeyValuePair<string, long>(m.ExternalId.Imdb, m.Id))
                    .Distinct(new KeyValuePairComparer())
                    .ToDictionary(
                        m => m.Key,
                        m => m.Value
                    ));
            }

            if (MetadataProviders.Tmdb.ToString() == externalIdType)
            {
                return new ApiResult<Dictionary<string, long>>(movies.Docs
                    .Where(m => m.ExternalId?.Tmdb != null && m.ExternalId.Tmdb > 0)
                    .Select(m => new KeyValuePair<string, long>(((long)m.ExternalId.Tmdb).ToString(CultureInfo.InvariantCulture), m.Id))
                    .Distinct(new KeyValuePairComparer())
                    .ToDictionary(
                        m => m.Key,
                        m => m.Value
                    ));
            }

            _log.Info($"Not supported provider: '{externalIdType}'");
            return new ApiResult<Dictionary<string, long>>(new Dictionary<string, long>()) { HasError = true };
        }

        public async Task<List<KpLists>> GetKpCollectionsAsync()
        {
            _log.Info("Fetch Kinopoisk collections");
            KpSearchResult<KpLists> collections = await _api.GetKpCollectionsAsync(CancellationToken.None);
            if (collections.HasError)
            {
                _log.Info("Failed to fetch Kinopoisk collections");
                return Enumerable.Empty<KpLists>().ToList();
            }

            _log.Info($"Found {collections.Docs.Count} collections");
            return collections.Docs.Where(x => x.MoviesCount > 0).ToList();
        }

        private async Task<List<KpMovie>> GetAllCollectionItemsAsync(string collectionId, CancellationToken cancellationToken)
        {
            var movies = new List<KpMovie>();
            KpSearchResult<KpMovie> tmp = await _api.GetCollectionItemsAsync(collectionId, 1, cancellationToken);
            if (tmp.HasError)
            {
                _log.Error($"Failed to fetch items list from API for collection '{collectionId}'");
                return movies;
            }

            if (tmp.Docs.Count == 0)
            {
                _log.Info($"No items found for collection '{collectionId}'");
                return movies;
            }

            var pages = Math.Ceiling((double)tmp.Total / tmp.Limit);
            movies.AddRange(tmp.Docs);
            for (var i = 2; i <= pages; i++)
            {
                _log.Info($"Fetched page {i} of {pages} pages ({movies.Count} of {tmp.Total} items) for collection '{collectionId}'");
                tmp = await _api.GetCollectionItemsAsync(collectionId, i, cancellationToken);
                if (tmp.HasError)
                {
                    _log.Warn($"Failed to fetch page {i} for collection '{collectionId}");
                    continue;
                }

                movies.AddRange(tmp.Docs);
                _log.Info($"Fetched page {i} of {pages} pages ({movies.Count} of {tmp.Total} items) for collection '{collectionId}'");
            }

            return movies;
        }

        #endregion
    }
}
