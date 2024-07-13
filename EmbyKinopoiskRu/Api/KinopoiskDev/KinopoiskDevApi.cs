using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Emby.Notifications;

using EmbyKinopoiskRu.Api.KinopoiskDev.Model;
using EmbyKinopoiskRu.Api.KinopoiskDev.Model.Movie;
using EmbyKinopoiskRu.Api.KinopoiskDev.Model.Person;
using EmbyKinopoiskRu.Api.KinopoiskDev.Model.Season;
using EmbyKinopoiskRu.Helper;

using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Notifications;
using MediaBrowser.Model.Activity;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Net;
using MediaBrowser.Model.Serialization;

namespace EmbyKinopoiskRu.Api.KinopoiskDev
{
    internal class KinopoiskDevApi
    {
        internal const int ApiResponseLimit = 20;

        private static readonly IList<string> FullSeasonPropertiesList = new List<string>
        {
            "airDate",
            "description",
            "episodes",
            "episodesCount",
            "movieId",
            "name",
            "number",
            "poster"
        }.AsReadOnly();

        private static readonly IList<string> MoviePropertiesList = new List<string>
        {
            "alternativeName",
            "backdrop",
            "countries",
            "description",
            "enName",
            "externalId",
            "genres",
            "id",
            "logo",
            "movieLength",
            "name",
            "persons",
            "poster",
            "premiere",
            "rating",
            "ratingMpaa",
            "slogan",
            "videos",
            "year",
            "sequelsAndPrequels",
            "top250",
            "facts",
            "releaseYears",
            "seasonsInfo"
        }.AsReadOnly();

        private static readonly IList<string> TrailerPropertiesList = new List<string>
        {
            "alternativeName",
            "externalId",
            "id",
            "name",
            "typeNumber",
            "videos",
            "premiere",
            "description",
            "poster",
        }.AsReadOnly();

        private readonly IHttpClient _httpClient;
        private readonly ILogger _log;
        private readonly IJsonSerializer _jsonSerializer;
        private readonly IActivityManager _activityManager;
        private readonly INotificationManager _notificationManager;

        internal KinopoiskDevApi(
            ILogManager logManager
            , IHttpClient httpClient
            , IJsonSerializer jsonSerializer
            , IActivityManager activityManager
            , INotificationManager notificationManager)
        {
            _httpClient = httpClient;
            _log = logManager.GetLogger(GetType().Name);
            _jsonSerializer = jsonSerializer;
            _activityManager = activityManager;
            _notificationManager = notificationManager;
        }

        internal async Task<KpMovie> GetMovieByIdAsync(string movieId, CancellationToken cancellationToken)
        {
            var json = await SendRequestAsync($"https://api.kinopoisk.dev/v1.4/movie/{movieId}", cancellationToken);
            return _jsonSerializer.DeserializeFromString<KpMovie>(json);
        }

        internal async Task<KpSearchResult<KpMovie>> GetMoviesByIdsAsync(IEnumerable<string> movieIdList, CancellationToken cancellationToken)
        {
            if (!movieIdList.Any())
            {
                _log.Info("GetMoviesByIds - received empty id's list");
                return new KpSearchResult<KpMovie>();
            }

            var url = new StringBuilder($"https://api.kinopoisk.dev/v1.4/movie?limit={ApiResponseLimit}")
                .Append($"&id={string.Join("&id=", movieIdList)}")
                .Append($"&selectFields={string.Join("&selectFields=", MoviePropertiesList)}")
                .ToString();
            var json = await SendRequestAsync(url, cancellationToken);
            return _jsonSerializer.DeserializeFromString<KpSearchResult<KpMovie>>(json);
        }

        internal async Task<KpSearchResult<KpMovie>> GetMoviesByMovieDetailsAsync(string name, int? year, CancellationToken cancellationToken)
        {
            return await GetMoviesByMovieDetailsAsync(name, name, year, cancellationToken);
        }

        internal async Task<KpSearchResult<KpMovie>> GetMoviesByMovieDetailsAsync(string name, string alternativeName, int? year, CancellationToken cancellationToken)
        {
            var url = $"https://api.kinopoisk.dev/v1.4/movie/search?limit={ApiResponseLimit}";
            if (!string.IsNullOrWhiteSpace(name))
            {
                var query = KpHelper.MultiWhitespace.Replace($"&query={name} {year}", "");
                var request = url + query;
                var json = await SendRequestAsync(request, cancellationToken);
                var toReturn = _jsonSerializer.DeserializeFromString<KpSearchResult<KpMovie>>(json);
                if (toReturn != null && toReturn.Docs.Count > 0)
                {
                    _log.Info($"Found {toReturn.Docs.Count} items for name '{name}' year '{year}'");
                    return toReturn;
                }
            }

            if (!string.IsNullOrWhiteSpace(alternativeName) && !string.Equals(name, alternativeName, StringComparison.InvariantCultureIgnoreCase))
            {
                var query = KpHelper.MultiWhitespace.Replace($"&query={alternativeName} {year}", "");
                var request = url + query;
                var json = await SendRequestAsync(request, cancellationToken);
                var toReturn = _jsonSerializer.DeserializeFromString<KpSearchResult<KpMovie>>(json);
                if (toReturn != null)
                {
                    _log.Info($"Found {toReturn.Docs.Count} items for alternativeName '{alternativeName}' year '{year}'");
                    return toReturn;
                }
            }

            _log.Info($"Nothing found for name '{name}' alternativeName '{alternativeName}' year '{year}'");
            return new KpSearchResult<KpMovie>();
        }

        internal async Task<KpSearchResult<KpMovie>> GetCollectionItemsAsync(string collectionId, int page, CancellationToken cancellationToken)
        {
            var request = new StringBuilder($"https://api.kinopoisk.dev/v1.4/movie?limit=250&page={page}&lists={collectionId}")
                .Append($"&selectFields={string.Join("&selectFields=", TrailerPropertiesList)}")
                .ToString();
            var json = await SendRequestAsync(request, cancellationToken);
            var hasError = json.Length == 0;
            return hasError
                ? new KpSearchResult<KpMovie>
                {
                    HasError = true
                }
                : _jsonSerializer.DeserializeFromString<KpSearchResult<KpMovie>>(json);
        }

        internal async Task<KpPerson> GetPersonByIdAsync(string personId, CancellationToken cancellationToken)
        {
            var json = await SendRequestAsync($"https://api.kinopoisk.dev/v1.4/person/{personId}", cancellationToken);
            return _jsonSerializer.DeserializeFromString<KpPerson>(json);
        }

        internal async Task<KpSearchResult<KpPerson>> GetPersonsByNameAsync(string name, CancellationToken cancellationToken)
        {
            var url = $"https://api.kinopoisk.dev/v1.4/person/search?limit={ApiResponseLimit}&query={name}";
            var json = await SendRequestAsync(url, cancellationToken);
            var toReturn = _jsonSerializer.DeserializeFromString<KpSearchResult<KpPerson>>(json);
            if (toReturn == null)
            {
                return new KpSearchResult<KpPerson>();
            }

            _log.Info($"Found {toReturn.Docs.Count} persons by '{name}'");
            return toReturn;
        }

        internal async Task<KpSearchResult<KpSeason>> GetSeasonBySeriesIdAsync(string seriesId, int parentIndexNumber, CancellationToken cancellationToken)
        {
            var url = new StringBuilder($"https://api.kinopoisk.dev/v1.4/season?limit={ApiResponseLimit}")
                .Append($"&selectFields={string.Join("&selectFields=", FullSeasonPropertiesList)}")
                .Append($"&movieId={seriesId}&number={parentIndexNumber}")
                .ToString();
            var json = await SendRequestAsync(url, cancellationToken);
            return _jsonSerializer.DeserializeFromString<KpSearchResult<KpSeason>>(json);
        }

        internal async Task<KpSearchResult<KpMovie>> GetKpIdByAnotherIdAsync(string externalIdType, IEnumerable<string> idList, CancellationToken cancellationToken)
        {
            if (!idList.Any())
            {
                _log.Info("Received ids list is empty");
                return new KpSearchResult<KpMovie>();
            }

            externalIdType = externalIdType.ToLowerInvariant();
            var request = $"https://api.kinopoisk.dev/v1.4/movie?limit={ApiResponseLimit}";
            request += "&selectFields=externalId&selectFields=id";
            var delimiter = $"&externalId.{externalIdType}=";
            request += $"{delimiter}{string.Join(delimiter, idList)}";
            var json = await SendRequestAsync(request, cancellationToken);
            var hasError = json.Length == 0;
            return hasError
                ? new KpSearchResult<KpMovie>
                {
                    HasError = true
                }
                : _jsonSerializer.DeserializeFromString<KpSearchResult<KpMovie>>(json);
        }

        internal async Task<KpSearchResult<KpLists>> GetKpCollectionsAsync(CancellationToken cancellationToken)
        {
            var request = "https://api.kinopoisk.dev/v1.4/list?limit=100";
            request += "&selectFields=name&selectFields=slug&selectFields=moviesCount&selectFields=cover&selectFields=category";
            var json = await SendRequestAsync(request, cancellationToken);
            var hasError = json.Length == 0;
            return hasError
                ? new KpSearchResult<KpLists>
                {
                    HasError = true
                }
                : _jsonSerializer.DeserializeFromString<KpSearchResult<KpLists>>(json);
        }

        private async Task<string> SendRequestAsync(string url, CancellationToken cancellationToken)
        {
            _log.Debug($"Sending request to {url}");
            var token = Plugin.Instance?.Configuration.GetCurrentToken();
            if (string.IsNullOrWhiteSpace(token))
            {
                _log.Error("The token is empty. Skip request");
                return string.Empty;
            }

            var options = new HttpRequestOptions
            {
                CancellationToken = cancellationToken,
                Url = url,
                CacheLength = TimeSpan.FromHours(12),
                CacheMode = CacheMode.Unconditional,
                TimeoutMs = 30_000,
                DecompressionMethod = CompressionMethod.Gzip,
                EnableHttpCompression = true,
                EnableDefaultUserAgent = true
            };
            options.RequestHeaders.Add("X-API-KEY", token);
            options.RequestHeaders.Add("accept", "application/json");
            options.Sanitation.SanitizeDefaultParams = false;

            HttpResponseInfo response = null;
            try
            {
                using (response = await _httpClient.GetResponse(options))
                {
                    using (var reader = new StreamReader(response.Content))
                    {
                        var result = await reader.ReadToEndAsync();
                        switch ((int)response.StatusCode)
                        {
                            case int n when n >= 200 && n < 300:
                                _log.Debug($"Received response: '{result}'");
                                return result;
                            case 400:
                                var error = _jsonSerializer.DeserializeFromString<KpErrorResponse>(result);
                                var msg = $"{error.Error}: {error.Message.FirstOrDefault()} for URL: '{url}'";
                                _log.Error(msg);
                                NotifyUser(msg, $"{error.Error}");
                                return string.Empty;
                            case 401:
                                msg = $"Token is invalid: '{token}'";
                                _log.Error(msg);
                                NotifyUser(msg, "Token is invalid");
                                return string.Empty;
                            case 403:
                                msg = "Request limit exceeded (either daily or total) for current token";
                                _log.Warn(msg);
                                NotifyUser(msg, "Request limit exceeded");
                                return string.Empty;
                            default:
                                error = _jsonSerializer.DeserializeFromString<KpErrorResponse>(result);
                                msg = $"Received '{response.StatusCode}' from API";
                                if (error == null)
                                {
                                    msg += $": '{result}' for URL: '{url}'";
                                }
                                else
                                {
                                    msg += $" - Error:'{error.Error}', Message:'{error.Message.FirstOrDefault()}' for URL: '{url}'";
                                }
                                _log.Error(msg);
                                NotifyUser(msg, "API request issue");
                                return string.Empty;
                        }
                    }
                }
            }
            catch (HttpException ex)
            {
                var content = string.Empty;
                if (response?.Content != null)
                {
                    using (var reader = new StreamReader(response.Content))
                    {
                        content = await reader.ReadToEndAsync();
                    }
                }

                switch ((int?)ex.StatusCode)
                {
                    case 401:
                        var msg = $"Token is invalid: '{token}'.{(string.IsNullOrWhiteSpace(content) ? string.Empty : " Message: '" + content + "'")}";
                        _log.Error(msg);
                        NotifyUser(msg, "Token is invalid");
                        break;
                    case 403:
                        msg = $"Request limit exceeded (either daily or total) for current token.{(string.IsNullOrWhiteSpace(content) ? string.Empty : " Message: '" + content + "'.")} For '{url}'";
                        _log.Warn(msg);
                        NotifyUser(msg, "Request limit exceeded");
                        break;
                    default:
                        msg = $"Received '{ex.StatusCode}' from API: '{(string.IsNullOrWhiteSpace(content) ? ex.Message : content)}'";
                        _log.Error(msg, ex);
                        NotifyUser(msg, "General error");
                        break;
                }

                return string.Empty;
            }
            catch (Exception ex)
            {
                var msg = $"Unable to fetch data from URL '{url}' due to {ex.Message}";
                _log.ErrorException(msg, ex);
                NotifyUser(msg, "General error");
                return string.Empty;
            }
        }

        private void NotifyUser(string overview, string shortOverview)
        {
            _activityManager.Create(new ActivityLogEntry
            {
                Name = Plugin.PluginKey,
                Type = "PluginError",
                Overview = overview,
                ShortOverview = shortOverview,
                Severity = LogSeverity.Error
            });

            _notificationManager.SendNotification(new NotificationRequest
            {
                Description = overview,
                Title = shortOverview
            });
        }
    }
}
