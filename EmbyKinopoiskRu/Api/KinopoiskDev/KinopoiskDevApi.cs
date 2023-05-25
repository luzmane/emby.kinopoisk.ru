using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using EmbyKinopoiskRu.Api.KinopoiskDev.Model;
using EmbyKinopoiskRu.Api.KinopoiskDev.Model.Movie;
using EmbyKinopoiskRu.Api.KinopoiskDev.Model.Person;
using EmbyKinopoiskRu.Api.KinopoiskDev.Model.Season;
using EmbyKinopoiskRu.Helper;

using MediaBrowser.Common.Net;
using MediaBrowser.Model.Activity;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Net;
using MediaBrowser.Model.Serialization;

namespace EmbyKinopoiskRu.Api.KinopoiskDev
{
    internal class KinopoiskDevApi
    {
        private readonly IHttpClient _httpClient;
        private readonly ILogger _log;
        private readonly IJsonSerializer _jsonSerializer;
        private readonly IActivityManager _activityManager;

        internal KinopoiskDevApi(
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

        internal async Task<KpMovie> GetMovieById(string movieId, CancellationToken cancellationToken)
        {
            var json = await SendRequest($"https://api.kinopoisk.dev/v1.3/movie/{movieId}", cancellationToken);
            return _jsonSerializer.DeserializeFromString<KpMovie>(json);
        }
        internal async Task<KpSearchResult<KpMovie>> GetMoviesByIds(List<string> movieIdList, CancellationToken cancellationToken)
        {
            if (!movieIdList.Any())
            {
                _log.Info("GetMoviesByIds - received empty id's list");
                return new KpSearchResult<KpMovie>();
            }
            var url = new StringBuilder($"https://api.kinopoisk.dev/v1.3/movie?")
                .Append($"id={string.Join("&id=", movieIdList)}")
                .Append($"&limit={movieIdList.Count}")
                .Append("&selectFields=alternativeName backdrop countries description enName externalId genres id logo movieLength name persons poster premiere productionCompanies rating ratingMpaa slogan videos year sequelsAndPrequels top250 facts releaseYears seasonsInfo")
                .ToString();
            var json = await SendRequest(url, cancellationToken);
            return _jsonSerializer.DeserializeFromString<KpSearchResult<KpMovie>>(json);
        }
        internal async Task<KpSearchResult<KpMovie>> GetMoviesByMovieDetails(string name, int? year, CancellationToken cancellationToken)
        {
            return await GetMoviesByMovieDetails(name, name, year, cancellationToken);
        }
        internal async Task<KpSearchResult<KpMovie>> GetMoviesByMovieDetails(string name, string alternativeName, int? year, CancellationToken cancellationToken)
        {
            var hasName = !string.IsNullOrWhiteSpace(name);
            var hasYear = year != null && year > 1000;
            var hasAlternativeName = !string.IsNullOrWhiteSpace(alternativeName);
            var url = "https://api.kinopoisk.dev/v1.3/movie?limit=50";
            var selectFields = "&selectFields=alternativeName backdrop countries description enName externalId genres id logo movieLength name persons poster premiere productionCompanies rating ratingMpaa slogan videos year sequelsAndPrequels top250 facts releaseYears seasonsInfo";
            var namePart = $"&name={name}";
            var alternativeNamePart = $"&alternativeName={alternativeName}";
            var yearPart = $"&year={year}";

            if (hasName && hasYear)
            {
                var request = url + namePart + yearPart + selectFields;
                var json = await SendRequest(request, cancellationToken);
                KpSearchResult<KpMovie> toReturn = _jsonSerializer.DeserializeFromString<KpSearchResult<KpMovie>>(json);
                if (toReturn != null && toReturn.Docs.Count > 0)
                {
                    _log.Info($"Found {toReturn.Docs.Count} items");
                    return toReturn;
                }
            }

            if (hasName)
            {
                var request = url + namePart + selectFields;
                var json = await SendRequest(request, cancellationToken);
                KpSearchResult<KpMovie> toReturn = _jsonSerializer.DeserializeFromString<KpSearchResult<KpMovie>>(json);
                if (toReturn != null && toReturn.Docs.Count > 0)
                {
                    _log.Info($"Found {toReturn.Docs.Count} items");
                    return toReturn;
                }
            }

            if (hasAlternativeName && hasYear)
            {
                var request = url + alternativeNamePart + yearPart + selectFields;
                var json = await SendRequest(request, cancellationToken);
                KpSearchResult<KpMovie> toReturn = _jsonSerializer.DeserializeFromString<KpSearchResult<KpMovie>>(json);
                if (toReturn != null && toReturn.Docs.Count > 0)
                {
                    _log.Info($"Found {toReturn.Docs.Count} items");
                    return toReturn;
                }
            }

            if (hasAlternativeName)
            {
                var request = url + alternativeNamePart + selectFields;
                var json = await SendRequest(request, cancellationToken);
                KpSearchResult<KpMovie> toReturn = _jsonSerializer.DeserializeFromString<KpSearchResult<KpMovie>>(json);
                if (toReturn != null && toReturn.Docs.Count > 0)
                {
                    _log.Info($"Found {toReturn.Docs.Count} items");
                    return toReturn;
                }
            }
            return new KpSearchResult<KpMovie>();
        }
        internal async Task<KpSearchResult<KpMovie>> GetTop250Collection(CancellationToken cancellationToken)
        {
            var request = $"https://api.kinopoisk.dev/v1.3/movie?";
            request += "limit=1000&top250=!null";
            request += "&selectFields=alternativeName externalId id name top250 typeNumber";
            var json = await SendRequest(request, cancellationToken);
            return _jsonSerializer.DeserializeFromString<KpSearchResult<KpMovie>>(json);
        }

        internal async Task<KpPerson> GetPersonById(string personId, CancellationToken cancellationToken)
        {
            var json = await SendRequest($"https://api.kinopoisk.dev/v1/person/{personId}", cancellationToken);
            return _jsonSerializer.DeserializeFromString<KpPerson>(json);
        }
        internal async Task<KpSearchResult<KpPerson>> GetPersonsByName(string name, CancellationToken cancellationToken)
        {
            var url = $"https://api.kinopoisk.dev/v1/person?";
            url += "selectFields=id name enName photo birthday death birthPlace deathPlace facts";
            var namePart = $"&name={name}";
            var enNamePart = $"&enName={name}";

            var json = await SendRequest(url + namePart, cancellationToken);
            KpSearchResult<KpPerson> toReturn = _jsonSerializer.DeserializeFromString<KpSearchResult<KpPerson>>(json);
            if (toReturn != null && toReturn.Docs.Count > 0)
            {
                _log.Info($"Found {toReturn.Docs.Count} persons by '{name}'");
                return toReturn;
            }

            json = await SendRequest(url + enNamePart, cancellationToken);
            toReturn = _jsonSerializer.DeserializeFromString<KpSearchResult<KpPerson>>(json);
            if (toReturn != null && toReturn.Docs.Count > 0)
            {
                _log.Info($"Found {toReturn.Docs.Count} persons by '{enNamePart}'");
                return toReturn;
            }
            return new KpSearchResult<KpPerson>();
        }
        internal async Task<KpSearchResult<KpPerson>> GetPersonsByMovieId(string movieId, CancellationToken cancellationToken)
        {
            var url = new StringBuilder("https://api.kinopoisk.dev/v1/person?")
                 .Append($"movies.id={movieId}")
                 .Append("&selectFields=id movies&limit=1000")
                 .ToString();
            var json = await SendRequest(url, cancellationToken);
            KpSearchResult<KpPerson> toReturn = _jsonSerializer.DeserializeFromString<KpSearchResult<KpPerson>>(json);
            if (toReturn != null && toReturn.Docs.Count > 0)
            {
                _log.Info($"Found {toReturn.Docs.Count} persons");
                return toReturn;
            }
            return new KpSearchResult<KpPerson>();
        }

        internal async Task<KpSearchResult<KpSeason>> GetEpisodesBySeriesId(string seriesId, CancellationToken cancellationToken)
        {
            var url = $"https://api.kinopoisk.dev/v1/season?movieId={seriesId}&limit=50";
            var json = await SendRequest(url, cancellationToken);
            return _jsonSerializer.DeserializeFromString<KpSearchResult<KpSeason>>(json);
        }

        internal async Task<KpSearchResult<KpMovie>> GetKpIdByAnotherId(string externalIdType, IEnumerable<string> idList, CancellationToken cancellationToken)
        {
            if (!idList.Any())
            {
                _log.Info("Received ids list is empty");
                return new KpSearchResult<KpMovie>();
            }
            var request = $"https://api.kinopoisk.dev/v1.3/movie?";
            request += $"selectFields=externalId.{externalIdType.ToLowerInvariant()} id&limit=1000";
            var delimeter = $"&externalId.{externalIdType.ToLowerInvariant()}=";
            request += $"{delimeter}{string.Join(delimeter, idList)}";
            var json = await SendRequest(request, cancellationToken);
            var hasError = json.Length == 0;
            return hasError
                ? new KpSearchResult<KpMovie>() { HasError = true }
                : _jsonSerializer.DeserializeFromString<KpSearchResult<KpMovie>>(json);
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
                            case 403:
                                msg = "Request limit exceeded (either daily or total) for current token";
                                _log.Warn(msg);
                                AddToActivityLog(msg, "Request limit exceeded");
                                // await EmbyHelper.SendNotification(_notificationManager, msg, cancellationToken);
                                return string.Empty;
                            default:
                                KpErrorResponse error = _jsonSerializer.DeserializeFromString<KpErrorResponse>(result);
                                _log.Error($"Received '{response.StatusCode}' from API: Error-'{error.Error}', Message-'{error.Message}'");
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
                    case 403:
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
