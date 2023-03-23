using System;
using System.Collections.Generic;
using System.Globalization;
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
            ILogger log
            , IHttpClient httpClient
            , IJsonSerializer jsonSerializer
            , IActivityManager activityManager)
        {
            _httpClient = httpClient;
            _log = log;
            _jsonSerializer = jsonSerializer;
            _activityManager = activityManager;
        }

        internal async Task<KpMovie?> GetMovieById(string movieId, CancellationToken cancellationToken)
        {
            var json = await SendRequest($"https://api.kinopoisk.dev/v1/movie/{movieId}", cancellationToken);
            return _jsonSerializer.DeserializeFromString<KpMovie>(json);
        }
        internal async Task<KpSearchResult<KpMovie>> GetMoviesByIds(List<string> movieIdList, CancellationToken cancellationToken)
        {
            var url = new StringBuilder($"https://api.kinopoisk.dev/v1/movie?")
                .Append(CultureInfo.InvariantCulture, $"limit={movieIdList.Count}")
                .Append("&selectFields=alternativeName backdrop countries description enName externalId genres id logo movieLength name persons poster premiere productionCompanies rating ratingMpaa slogan videos year sequelsAndPrequels top250 facts releaseYears seasonsInfo")
                .Append(CultureInfo.InvariantCulture, $"&id={string.Join("&id=", movieIdList)}")
                .ToString();
            var json = await SendRequest(url, cancellationToken);
            return _jsonSerializer.DeserializeFromString<KpSearchResult<KpMovie>>(json);
        }
        internal async Task<KpSearchResult<KpMovie>> GetMoviesByMovieDetails(string? name, int? year, CancellationToken cancellationToken)
        {
            return await GetMoviesByMovieDetails(name, name, year, cancellationToken);
        }
        internal async Task<KpSearchResult<KpMovie>> GetMoviesByMovieDetails(string? name, string? alternativeName, int? year, CancellationToken cancellationToken)
        {
            var hasName = !string.IsNullOrWhiteSpace(name);
            var hasYear = year is not null and > 1000;
            var hasAlternativeName = !string.IsNullOrWhiteSpace(alternativeName);
            var url = new StringBuilder($"https://api.kinopoisk.dev/v1/movie?")
                .Append($"limit=50")
                .Append("&selectFields=alternativeName backdrop countries description enName externalId genres id logo movieLength name persons poster premiere productionCompanies rating ratingMpaa slogan videos year sequelsAndPrequels top250 facts releaseYears seasonsInfo")
                .ToString();
            var namePart = hasName ? $"&name={name}" : string.Empty;
            var alternativeNamePart = hasAlternativeName ? $"&alternativeName={alternativeName}" : string.Empty;
            var yearPart = hasYear ? $"&year={year}" : string.Empty;

            if (hasName && hasYear)
            {
                var request = url + namePart + yearPart;
                var json = await SendRequest(request, cancellationToken);
                KpSearchResult<KpMovie>? toReturn = _jsonSerializer.DeserializeFromString<KpSearchResult<KpMovie>>(json);
                if (toReturn != null && toReturn.Docs.Count > 0)
                {
                    _log.Info($"Found {toReturn.Docs.Count} movies");
                    return toReturn;
                }
            }

            if (hasName)
            {
                var request = url + namePart;
                var json = await SendRequest(request, cancellationToken);
                KpSearchResult<KpMovie>? toReturn = _jsonSerializer.DeserializeFromString<KpSearchResult<KpMovie>>(json);
                if (toReturn != null && toReturn.Docs.Count > 0)
                {
                    _log.Info($"Found {toReturn.Docs.Count} movies");
                    return toReturn;
                }
            }

            if (hasAlternativeName && hasYear)
            {
                var request = url + alternativeNamePart + yearPart;
                var json = await SendRequest(request, cancellationToken);
                KpSearchResult<KpMovie>? toReturn = _jsonSerializer.DeserializeFromString<KpSearchResult<KpMovie>>(json);
                if (toReturn != null && toReturn.Docs.Count > 0)
                {
                    _log.Info($"Found {toReturn.Docs.Count} movies");
                    return toReturn;
                }
            }

            if (hasAlternativeName)
            {
                var request = url + alternativeNamePart;
                var json = await SendRequest(request, cancellationToken);
                KpSearchResult<KpMovie>? toReturn = _jsonSerializer.DeserializeFromString<KpSearchResult<KpMovie>>(json);
                if (toReturn != null && toReturn.Docs.Count > 0)
                {
                    _log.Info($"Found {toReturn.Docs.Count} movies");
                    return toReturn;
                }
            }
            return new KpSearchResult<KpMovie>();
        }
        internal async Task<KpSearchResult<KpMovie>> GetTop250Collection(CancellationToken cancellationToken)
        {
            var request = $"https://api.kinopoisk.dev/v1/movie?";
            request += "selectFields=alternativeName externalId id name top250 typeNumber";
            request += "&limit=1000&top250=!null";
            var json = await SendRequest(request, cancellationToken);
            return _jsonSerializer.DeserializeFromString<KpSearchResult<KpMovie>>(json);
        }

        internal async Task<KpPerson?> GetPersonById(string personId, CancellationToken cancellationToken)
        {
            var json = await SendRequest($"https://api.kinopoisk.dev/v1/person/{personId}", cancellationToken);
            return _jsonSerializer.DeserializeFromString<KpPerson>(json);
        }
        internal async Task<KpSearchResult<KpPerson>> GetPersonsByName(string name, CancellationToken cancellationToken)
        {
            var url = $"https://api.kinopoisk.dev/v1/person";
            var namePart = $"&name={name}";
            var enNamePart = $"&enName={name}";

            var json = await SendRequest(url + namePart, cancellationToken);
            KpSearchResult<KpPerson>? toReturn = _jsonSerializer.DeserializeFromString<KpSearchResult<KpPerson>>(json);
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
                 .Append(CultureInfo.InvariantCulture, $"&movies.id={movieId}")
                 .Append("&selectFields=id movies")
                 .Append("&limit=1000")
                 .ToString();
            var json = await SendRequest(url, cancellationToken);
            KpSearchResult<KpPerson>? toReturn = _jsonSerializer.DeserializeFromString<KpSearchResult<KpPerson>>(json);
            if (toReturn != null && toReturn.Docs.Count > 0)
            {
                _log.Info($"Found {toReturn.Docs.Count} persons");
                return toReturn;
            }
            return new KpSearchResult<KpPerson>();
        }

        internal async Task<KpSearchResult<KpSeason>?> GetEpisodesBySeriesId(string seriesId, CancellationToken cancellationToken)
        {
            var url = new StringBuilder("https://api.kinopoisk.dev/v1/season?")
                .Append(CultureInfo.InvariantCulture, $"movieId={seriesId}")
                .Append($"&limit=50")
                .ToString();
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
            var request = $"https://api.kinopoisk.dev/v1/movie?";
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
            HttpRequestOptions options = new()
            {
                CancellationToken = cancellationToken,
                Url = url,
                LogResponse = true,
                CacheLength = TimeSpan.FromHours(12),
                CacheMode = CacheMode.Unconditional,
                TimeoutMs = 180000
            };
            options.RequestHeaders.Add("X-API-KEY", token);
            options.Sanitation.SanitizeDefaultParams = false;
            try
            {
                using HttpResponseInfo response = await _httpClient.GetResponse(options);
                using StreamReader reader = new(response.Content);
                var result = await reader.ReadToEndAsync();
                switch ((int)response.StatusCode)
                {
                    case int n when n is >= 200 and < 300:
                        _log.Info($"Received response: '{result}'");
                        return result;
                    case 401:
                        _log.Error($"Token is invalid: '{token}'");
                        AddToActivityLog($"Token '{token}' is invalid", "Token is invalid");
                        return string.Empty;
                    case 403:
                        _log.Warn("Request limit exceeded (either daily or total)");
                        AddToActivityLog("Request limit exceeded (either daily or total)", "Request limit exceeded");
                        return string.Empty;
                    default:
                        KpErrorResponse error = _jsonSerializer.DeserializeFromString<KpErrorResponse>(result);
                        _log.Error($"Received '{response.StatusCode}' from API: Error-'{error.Error}', Message-'{error.Message}'");
                        return string.Empty;
                }
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
