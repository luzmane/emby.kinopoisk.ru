using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Emby.Notifications;

using EmbyKinopoiskRu.Api.KinopoiskApiUnofficial.Model;
using EmbyKinopoiskRu.Api.KinopoiskApiUnofficial.Model.Film;
using EmbyKinopoiskRu.Api.KinopoiskApiUnofficial.Model.Person;
using EmbyKinopoiskRu.Api.KinopoiskApiUnofficial.Model.Season;

using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Notifications;
using MediaBrowser.Model.Activity;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Net;
using MediaBrowser.Model.Serialization;

namespace EmbyKinopoiskRu.Api.KinopoiskApiUnofficial
{
    internal class KinopoiskUnofficialApi
    {
        private readonly IHttpClient _httpClient;
        private readonly ILogger _log;
        private readonly IJsonSerializer _jsonSerializer;
        private readonly IActivityManager _activityManager;
        private readonly INotificationManager _notificationManager;

        internal KinopoiskUnofficialApi(
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

        internal async Task<KpFilm> GetFilmByIdAsync(string movieId, CancellationToken cancellationToken)
        {
            var url = $"https://kinopoiskapiunofficial.tech/api/v2.2/films/{movieId}";
            var response = await SendRequestAsync(url, cancellationToken);
            return string.IsNullOrEmpty(response) ? null : _jsonSerializer.DeserializeFromString<KpFilm>(response);
        }

        internal async Task<KpSearchResult<KpFilm>> GetFilmsByNameAndYearAsync(string name, int? year, CancellationToken cancellationToken)
        {
            var hasName = !string.IsNullOrWhiteSpace(name);
            var hasYear = year > 1000;
            const string url = "https://kinopoiskapiunofficial.tech/api/v2.2/films";
            var namePart = $"?keyword={name}";
            var yearPart = $"&yearFrom={year}&yearTo={year}";

            if (hasYear && hasName)
            {
                var request = url + namePart + yearPart;
                var response = await SendRequestAsync(request, cancellationToken);
                var toReturn = _jsonSerializer.DeserializeFromString<KpSearchResult<KpFilm>>(response);
                if (toReturn != null && toReturn.Items.Count > 0)
                {
                    _log.Info($"Found {toReturn.Items.Count} movies for name '{name}' year '{year}'");
                    return toReturn;
                }
            }

            if (hasName)
            {
                var request = url + namePart;
                var response = await SendRequestAsync(request, cancellationToken);
                var toReturn = _jsonSerializer.DeserializeFromString<KpSearchResult<KpFilm>>(response);
                if (toReturn != null && toReturn.Items.Count > 0)
                {
                    _log.Info($"Found {toReturn.Items.Count} movies for name '{name}' year '{year}'");
                    return toReturn;
                }
            }

            _log.Info($"Nothing found for name '{name}' year '{year}'");
            return new KpSearchResult<KpFilm>
            {
                HasError = true
            };
        }

        internal async Task<List<KpFilmStaff>> GetStaffByFilmIdAsync(string movieId, CancellationToken cancellationToken)
        {
            var url = $"https://kinopoiskapiunofficial.tech/api/v1/staff?filmId={movieId}";
            var response = await SendRequestAsync(url, cancellationToken);
            return string.IsNullOrEmpty(response) ? new List<KpFilmStaff>() : _jsonSerializer.DeserializeFromString<List<KpFilmStaff>>(response);
        }

        internal async Task<KpSearchResult<KpVideo>> GetVideosByFilmIdAsync(string movieId, CancellationToken cancellationToken)
        {
            var url = $"https://kinopoiskapiunofficial.tech/api/v2.2/films/{movieId}/videos";
            var response = await SendRequestAsync(url, cancellationToken);
            return _jsonSerializer.DeserializeFromString<KpSearchResult<KpVideo>>(response)
                   ?? new KpSearchResult<KpVideo>
                   {
                       HasError = true
                   };
        }

        internal async Task<KpSearchResult<KpSeason>> GetEpisodesBySeriesIdAsync(string seriesId, CancellationToken cancellationToken)
        {
            var url = $"https://kinopoiskapiunofficial.tech/api/v2.2/films/{seriesId}/seasons";
            var response = await SendRequestAsync(url, cancellationToken);
            return _jsonSerializer.DeserializeFromString<KpSearchResult<KpSeason>>(response)
                   ?? new KpSearchResult<KpSeason>
                   {
                       HasError = true
                   };
        }

        internal async Task<KpPerson> GetPersonByIdAsync(string personId, CancellationToken cancellationToken)
        {
            var url = $"https://kinopoiskapiunofficial.tech/api/v1/staff/{personId}";
            var response = await SendRequestAsync(url, cancellationToken);
            return string.IsNullOrEmpty(response) ? null : _jsonSerializer.DeserializeFromString<KpPerson>(response);
        }

        internal async Task<KpSearchResult<KpStaff>> GetPersonsByNameAsync(string name, CancellationToken cancellationToken)
        {
            var url = $"https://kinopoiskapiunofficial.tech/api/v1/persons?name={name}";
            var response = await SendRequestAsync(url, cancellationToken);
            return _jsonSerializer.DeserializeFromString<KpSearchResult<KpStaff>>(response)
                   ?? new KpSearchResult<KpStaff>
                   {
                       HasError = true
                   };
        }

        internal async Task<KpSearchResult<KpFilm>> GetFilmByImdbIdAsync(string imdbMovieId, CancellationToken cancellationToken)
        {
            var hasImdb = !string.IsNullOrWhiteSpace(imdbMovieId);
            var request = $"https://kinopoiskapiunofficial.tech/api/v2.2/films?imdbId={imdbMovieId}";

            if (hasImdb)
            {
                var response = await SendRequestAsync(request, cancellationToken);
                return _jsonSerializer.DeserializeFromString<KpSearchResult<KpFilm>>(response)
                       ?? new KpSearchResult<KpFilm>
                       {
                           HasError = true
                       };
            }

            _log.Warn("IMDB ID was not provided");
            return new KpSearchResult<KpFilm>
            {
                HasError = true
            };
        }

        internal async Task<KpSearchResult<KpFilm>> GetCollectionItemsAsync(string collectionId, int page, CancellationToken cancellationToken)
        {
            var request = $"https://kinopoiskapiunofficial.tech/api/v2.2/films/collections?page={page}&type={collectionId}";
            var response = await SendRequestAsync(request, cancellationToken);
            return _jsonSerializer.DeserializeFromString<KpSearchResult<KpFilm>>(response)
                   ?? new KpSearchResult<KpFilm>
                   {
                       HasError = true
                   };
        }

        private async Task<string> SendRequestAsync(string url, CancellationToken cancellationToken)
        {
            _log.Info($"Sending request to {url}");
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
                LogResponse = true,
                CacheLength = TimeSpan.FromHours(12),
                CacheMode = CacheMode.Unconditional,
                TimeoutMs = 180000,
                DecompressionMethod = CompressionMethod.Gzip,
                EnableHttpCompression = true,
                EnableDefaultUserAgent = true
            };
            options.RequestHeaders.Add("X-API-KEY", token);
            options.Sanitation.SanitizeDefaultParams = false;
            try
            {
                using (HttpResponseInfo response = await _httpClient.GetResponse(options))
                {
                    using (var reader = new StreamReader(response.Content))
                    {
                        var result = await reader.ReadToEndAsync();
                        switch ((int)response.StatusCode)
                        {
                            case int n when n >= 200 && n < 300:
                                _log.Info($"Received response: '{result}'");
                                return result;
                            case 401:
                                var msg = $"Token is invalid: '{token}'";
                                _log.Error(msg);
                                NotifyUser(msg, "Token is invalid");
                                return string.Empty;
                            case 402:
                                msg = "Request limit exceeded (either daily or total) for current token";
                                _log.Warn(msg);
                                NotifyUser(msg, "Request limit exceeded");
                                return string.Empty;
                            case 404:
                                _log.Info($"Data not found for URL: {url}");
                                return string.Empty;
                            case 429:
                                _log.Info("Too many requests per second. Waiting 2 sec");
                                await Task.Delay(2000, cancellationToken);
                                return await SendRequestAsync(url, cancellationToken);
                            default:
                                msg = $"Received '{response.StatusCode}' from API: '{result}'";
                                _log.Error(msg);
                                NotifyUser(msg, "API request issue");
                                return string.Empty;
                        }
                    }
                }
            }
            catch (HttpException ex)
            {
                switch ((int)ex.StatusCode)
                {
                    case 401:
                        var msg = $"Token is invalid: '{token}'";
                        _log.Error(msg);
                        NotifyUser(msg, "Token is invalid");
                        break;
                    case 402:
                        msg = "Request limit exceeded (either daily or total) for current token";
                        _log.Warn(msg);
                        NotifyUser(msg, "Request limit exceeded");
                        break;
                    default:
                        msg = $"Received '{ex.StatusCode}' from API: Message-'{ex.Message}'";
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
