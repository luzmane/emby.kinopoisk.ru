using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using EmbyKinopoiskRu.Api.KinopoiskApiUnofficial.Model;
using EmbyKinopoiskRu.Api.KinopoiskApiUnofficial.Model.Film;
using EmbyKinopoiskRu.Api.KinopoiskApiUnofficial.Model.Person;
using EmbyKinopoiskRu.Api.KinopoiskApiUnofficial.Model.Season;
using EmbyKinopoiskRu.Helper;

using MediaBrowser.Common.Net;
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

        internal KinopoiskUnofficialApi(
            ILogManager logManager
            , IHttpClient httpClient
            , IJsonSerializer jsonSerializer
            , IActivityManager activityManager)
        {
            _httpClient = httpClient;
            _log = logManager.GetLogger(GetType().Name);
            _jsonSerializer = jsonSerializer;
            _activityManager = activityManager;
        }

        internal async Task<KpFilm> GetFilmById(string movieId, CancellationToken cancellationToken)
        {
            var url = $"https://kinopoiskapiunofficial.tech/api/v2.2/films/{movieId}";
            var response = await SendRequest(url, cancellationToken);
            return string.IsNullOrEmpty(response) ? null : _jsonSerializer.DeserializeFromString<KpFilm>(response);
        }
        internal async Task<KpSearchResult<KpFilm>> GetFilmsByNameAndYear(string name, int? year, CancellationToken cancellationToken)
        {
            var hasName = !string.IsNullOrWhiteSpace(name);
            var hasYear = year != null && year > 1000;
            var url = "https://kinopoiskapiunofficial.tech/api/v2.2/films";
            var namePart = $"?keyword={name}";
            var yearPart = $"&yearFrom={year}&yearTo={year}";

            if (hasYear && hasName)
            {
                var request = url + namePart + yearPart;
                var response = await SendRequest(request, cancellationToken);
                KpSearchResult<KpFilm> toReturn = _jsonSerializer.DeserializeFromString<KpSearchResult<KpFilm>>(response);
                if (toReturn != null && toReturn.Items.Count > 0)
                {
                    _log.Info($"Found {toReturn.Items.Count} movies");
                    return toReturn;
                }
            }

            if (hasName)
            {
                var request = url + namePart;
                var response = await SendRequest(request, cancellationToken);
                KpSearchResult<KpFilm> toReturn = _jsonSerializer.DeserializeFromString<KpSearchResult<KpFilm>>(response);
                if (toReturn != null && toReturn.Items.Count > 0)
                {
                    _log.Info($"Found {toReturn.Items.Count} movies");
                    return toReturn;
                }
            }
            return new KpSearchResult<KpFilm>();
        }
        internal async Task<List<KpFilmStaff>> GetStaffByFilmId(string movieId, CancellationToken cancellationToken)
        {
            var url = $"https://kinopoiskapiunofficial.tech/api/v1/staff?filmId={movieId}";
            var response = await SendRequest(url, cancellationToken);
            return string.IsNullOrEmpty(response) ? new List<KpFilmStaff>() : _jsonSerializer.DeserializeFromString<List<KpFilmStaff>>(response);
        }
        internal async Task<KpSearchResult<KpVideo>> GetVideosByFilmId(string movieId, CancellationToken cancellationToken)
        {
            var url = $"https://kinopoiskapiunofficial.tech/api/v2.2/films/{movieId}/videos";
            var response = await SendRequest(url, cancellationToken);
            return string.IsNullOrEmpty(response) ? new KpSearchResult<KpVideo>() : _jsonSerializer.DeserializeFromString<KpSearchResult<KpVideo>>(response);
        }
        internal async Task<KpSearchResult<KpSeason>> GetEpisodesBySeriesId(string seriesId, CancellationToken cancellationToken)
        {
            var url = $"https://kinopoiskapiunofficial.tech/api/v2.2/films/{seriesId}/seasons";
            var response = await SendRequest(url, cancellationToken);
            return string.IsNullOrEmpty(response) ? null : _jsonSerializer.DeserializeFromString<KpSearchResult<KpSeason>>(response);
        }
        internal async Task<KpPerson> GetPersonById(string personId, CancellationToken cancellationToken)
        {
            var url = $"https://kinopoiskapiunofficial.tech/api/v1/staff/{personId}";
            var response = await SendRequest(url, cancellationToken);
            return string.IsNullOrEmpty(response) ? null : _jsonSerializer.DeserializeFromString<KpPerson>(response);
        }
        internal async Task<KpSearchResult<KpStaff>> GetPersonsByName(string name, CancellationToken cancellationToken)
        {
            var url = $"https://kinopoiskapiunofficial.tech/api/v1/persons?name={name}";
            var response = await SendRequest(url, cancellationToken);
            return string.IsNullOrEmpty(response) ? new KpSearchResult<KpStaff>() : _jsonSerializer.DeserializeFromString<KpSearchResult<KpStaff>>(response);
        }

        private async Task<string> SendRequest(string url, CancellationToken cancellationToken)
        {
            _log.Info($"Sending request to {url}");
            var token = Plugin.Instance?.Configuration.GetCurrentToken();
            if (string.IsNullOrWhiteSpace(token))
            {
                _log.Error("The token is empty. Skip request");
                return string.Empty;
            }
            var options = new HttpRequestOptions()
            {
                CancellationToken = cancellationToken,
                Url = url,
                LogResponse = true,
                CacheLength = TimeSpan.FromHours(12),
                CacheMode = CacheMode.Unconditional,
                TimeoutMs = 180000,
                DecompressionMethod = CompressionMethod.Gzip,
                EnableHttpCompression = true,
                EnableDefaultUserAgent = true,
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
                                AddToActivityLog(msg, "Token is invalid");
                                // await EmbyHelper.SendNotification(_notificationManager, msg, cancellationToken);
                                return string.Empty;
                            case 402:
                                msg = "Request limit exceeded (either daily or total) for current token";
                                _log.Warn(msg);
                                AddToActivityLog(msg, "Request limit exceeded");
                                // await EmbyHelper.SendNotification(_notificationManager, msg, cancellationToken);
                                return string.Empty;
                            case 404:
                                _log.Info($"Data not found for URL: {url}");
                                return string.Empty;
                            case 429:
                                _log.Info("Too many requests per second. Waiting 2 sec");
                                await Task.Delay(2000, cancellationToken);
                                return await SendRequest(url, cancellationToken);
                            default:
                                _log.Error($"Received '{response.StatusCode}' from API: {result}");
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
                        AddToActivityLog(msg, "Token is invalid");
                        // await EmbyHelper.SendNotification(_notificationManager, msg, cancellationToken);
                        break;
                    case 402:
                        msg = "Request limit exceeded (either daily or total) for current token";
                        _log.Warn(msg);
                        AddToActivityLog(msg, "Request limit exceeded");
                        // await EmbyHelper.SendNotification(_notificationManager, msg, cancellationToken);
                        break;
                    default:
                        _log.Error($"Received '{ex.StatusCode}' from API: Message-'{ex.Message}'", ex);
                        break;
                }
                return string.Empty;
            }
            catch (Exception ex)
            {
                _log.ErrorException($"Unable to fetch data from URL '{url}' due to {ex.Message}", ex);
                return string.Empty;
            }
        }

        private void AddToActivityLog(string overview, string shortOverview)
        {
            KpHelper.AddToActivityLog(_activityManager, overview, shortOverview);
        }

    }
}
