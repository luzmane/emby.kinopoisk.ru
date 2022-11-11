using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EmbyKinopoiskRu.Api.KinopoiskDev.Model;
using EmbyKinopoiskRu.Api.KinopoiskDev.Model.Movie;
using EmbyKinopoiskRu.Api.KinopoiskDev.Model.Person;
using EmbyKinopoiskRu.Api.KinopoiskDev.Model.Season;
using MediaBrowser.Common.Net;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Serialization;

namespace EmbyKinopoiskRu.Api.KinopoiskDev
{
    internal class KinopoiskDevApi
    {
        private readonly IHttpClient _httpClient;
        private readonly ILogger _log;
        private readonly IJsonSerializer _jsonSerializer;

        internal KinopoiskDevApi(ILogger log, IHttpClient httpClient, IJsonSerializer jsonSerializer)
        {
            _httpClient = httpClient;
            _log = log;
            _jsonSerializer = jsonSerializer;
        }

        internal async Task<KpMovie?> GetMovieById(string movieId, CancellationToken cancellationToken)
        {
            string? token = Plugin.Instance?.Configuration.GetToken();
            if (string.IsNullOrWhiteSpace(token))
            {
                _log.Error("The token is empty. Skip request");
                return null;
            }

            string url = new StringBuilder($"https://api.kinopoisk.dev/movie?token={token}")
                .Append(CultureInfo.InvariantCulture, $"&field=id&search={movieId}")
                .ToString();
            string? json = await SendRequest(url, cancellationToken);
            return _jsonSerializer.DeserializeFromString<KpMovie>(json);
        }
        internal async Task<KpSearchResult<KpMovie>> GetMoviesByMetadata(string? name, int? year, bool isStrict, CancellationToken cancellationToken)
        {
            string? token = Plugin.Instance?.Configuration.GetToken();
            if (string.IsNullOrWhiteSpace(token))
            {
                _log.Error("The token is empty. Skip request");
                return new KpSearchResult<KpMovie>();
            }

            bool hasName = !string.IsNullOrWhiteSpace(name);
            bool hasYear = year is not null and > 1000;
            string url = new StringBuilder($"https://api.kinopoisk.dev/movie?token={token}")
                .Append("&limit=50")
                .Append(CultureInfo.InvariantCulture, $"&isStrict={isStrict}")
                .Append("&selectFields=externalId logo poster rating movieLength id type name description year alternativeName enName backdrop countries genres persons premiere productionCompanies ratingMpaa slogan")
                .ToString();
            string namePart = hasName ? $"&field=name&search={name}" : string.Empty;
            string yearPart = hasYear ? $"&field=year&search={year}" : string.Empty;

            if (hasName && hasYear)
            {
                string request = url + namePart + yearPart;
                string? json = await SendRequest(request, cancellationToken);
                KpSearchResult<KpMovie>? toReturn = _jsonSerializer.DeserializeFromString<KpSearchResult<KpMovie>>(json);
                if (toReturn != null && toReturn.Docs.Count > 0)
                {
                    _log.Info($"Found {toReturn.Docs.Count} movies");
                    return toReturn;
                }
            }

            if (hasName)
            {
                string request = url + namePart;
                string? json = await SendRequest(request, cancellationToken);
                KpSearchResult<KpMovie>? toReturn = _jsonSerializer.DeserializeFromString<KpSearchResult<KpMovie>>(json);
                if (toReturn != null && toReturn.Docs.Count > 0)
                {
                    _log.Info($"Found {toReturn.Docs.Count} movies");
                    return toReturn;
                }
            }

            return new KpSearchResult<KpMovie>();
        }
        internal async Task<KpSearchResult<KpMovie>> GetMoviesByMovieDetails(string? name, string? alternativeName, int? year, CancellationToken cancellationToken)
        {
            string? token = Plugin.Instance?.Configuration.GetToken();
            if (string.IsNullOrWhiteSpace(token))
            {
                _log.Error("The token is empty. Skip request");
                return new KpSearchResult<KpMovie>();
            }

            bool hasName = !string.IsNullOrWhiteSpace(name);
            bool hasYear = year is not null and > 1000;
            bool hasAlternativeName = !string.IsNullOrWhiteSpace(alternativeName);
            string url = new StringBuilder($"https://api.kinopoisk.dev/movie?token={token}")
                .Append($"&limit=50")
                .ToString();
            string namePart = hasName ? $"&field=name&search={name}" : string.Empty;
            string alternativeNamePart = hasAlternativeName ? $"&field=alternativeName&search={alternativeName}" : string.Empty;
            string yearPart = hasYear ? $"&field=year&search={year}" : string.Empty;

            if (hasName && hasYear)
            {
                string request = url + namePart + yearPart;
                string? json = await SendRequest(request, cancellationToken);
                KpSearchResult<KpMovie>? toReturn = _jsonSerializer.DeserializeFromString<KpSearchResult<KpMovie>>(json);
                if (toReturn != null && toReturn.Docs.Count > 0)
                {
                    _log.Info($"Found {toReturn.Docs.Count} movies");
                    return toReturn;
                }
            }

            if (hasName)
            {
                string request = url + namePart;
                string? json = await SendRequest(request, cancellationToken);
                KpSearchResult<KpMovie>? toReturn = _jsonSerializer.DeserializeFromString<KpSearchResult<KpMovie>>(json);
                if (toReturn != null && toReturn.Docs.Count > 0)
                {
                    _log.Info($"Found {toReturn.Docs.Count} movies");
                    return toReturn;
                }
            }

            if (hasAlternativeName && hasYear)
            {
                string request = url + alternativeNamePart + yearPart;
                string? json = await SendRequest(request, cancellationToken);
                KpSearchResult<KpMovie>? toReturn = _jsonSerializer.DeserializeFromString<KpSearchResult<KpMovie>>(json);
                if (toReturn != null && toReturn.Docs.Count > 0)
                {
                    _log.Info($"Found {toReturn.Docs.Count} movies");
                    return toReturn;
                }
            }

            if (hasAlternativeName)
            {
                string request = url + alternativeNamePart;
                string? json = await SendRequest(request, cancellationToken);
                KpSearchResult<KpMovie>? toReturn = _jsonSerializer.DeserializeFromString<KpSearchResult<KpMovie>>(json);
                if (toReturn != null && toReturn.Docs.Count > 0)
                {
                    _log.Info($"Found {toReturn.Docs.Count} movies");
                    return toReturn;
                }
            }
            return new KpSearchResult<KpMovie>();
        }

        internal async Task<KpPerson?> GetPersonById(string personId, CancellationToken cancellationToken)
        {
            string? token = Plugin.Instance?.Configuration.GetToken();
            if (string.IsNullOrWhiteSpace(token))
            {
                _log.Error("The token is empty. Skip request");
                return null;
            }

            string request = $"https://api.kinopoisk.dev/person?token={token}&field=id&search={personId}";
            string? json = await SendRequest(request, cancellationToken);
            return _jsonSerializer.DeserializeFromString<KpPerson>(json);
        }
        internal async Task<KpSearchResult<KpPerson>> GetPersonsByName(string name, CancellationToken cancellationToken)
        {
            string? token = Plugin.Instance?.Configuration.GetToken();
            if (string.IsNullOrWhiteSpace(token))
            {
                _log.Error("The token is empty. Skip request");
                return new KpSearchResult<KpPerson>();
            }

            string url = $"https://api.kinopoisk.dev/movie?token={token}&field=name&search={Uri.EscapeDataString(name)}";
            string? json = await SendRequest(url, cancellationToken);
            KpSearchResult<KpPerson>? toReturn = _jsonSerializer.DeserializeFromString<KpSearchResult<KpPerson>>(json);
            if (toReturn != null && toReturn.Docs.Count > 0)
            {
                _log.Info($"Found {toReturn.Docs.Count} persons");
                return toReturn;
            }
            return new KpSearchResult<KpPerson>();
        }
        internal async Task<KpSearchResult<KpPerson>> GetPersonsByMovieId(string movieId, CancellationToken cancellationToken)
        {
            string? token = Plugin.Instance?.Configuration.GetToken();
            if (string.IsNullOrWhiteSpace(token))
            {
                _log.Error("The token is empty. Skip request");
                return new KpSearchResult<KpPerson>();
            }

            string url = new StringBuilder($"https://api.kinopoisk.dev/person?token={token}")
                 .Append(CultureInfo.InvariantCulture, $"&field=movies.id&search={movieId}")
                 .Append("&selectFields=id movies")
                 .Append("&limit=1000")
                 .ToString();
            string? json = await SendRequest(url, cancellationToken);
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
            string? token = Plugin.Instance?.Configuration.GetToken();
            if (string.IsNullOrWhiteSpace(token))
            {
                _log.Error("The token is empty. Skip request");
                return null;
            }
            string url = new StringBuilder($"https://api.kinopoisk.dev/season?token={token}")
                .Append(CultureInfo.InvariantCulture, $"&field=movieId&search={seriesId}")
                .Append($"&limit=50")
                .ToString();
            string? json = await SendRequest(url, cancellationToken);
            return _jsonSerializer.DeserializeFromString<KpSearchResult<KpSeason>>(json);
        }

        private async Task<string?> SendRequest(string url, CancellationToken cancellationToken)
        {
            _log.Info($"Sending request to {url}");
            HttpRequestOptions options = new()
            {
                CancellationToken = cancellationToken,
                Url = url,
                LogResponse = true,
                CacheLength = TimeSpan.FromHours(12),
                CacheMode = CacheMode.Unconditional,
                TimeoutMs = 180000
            };
            options.Sanitation.SanitizeDefaultParams = false;
            try
            {
                using HttpResponseInfo response = await _httpClient.SendAsync(options, "GET");
                if (((int)response.StatusCode) is >= 200 and < 300)
                {
                    using StreamReader reader = new(response.Content);
                    string result = await reader.ReadToEndAsync();
                    _log.Info($"Received response: '{result}'");
                    return result;
                }
                else
                {
                    _log.Error($"Received '{response.StatusCode}' from API");
                    return null;
                }
            }
            catch (Exception ex)
            {
                _log.ErrorException($"Unable to fetch data from URL '{url}' due to {ex.Message}", ex);
                return null;
            }
        }
    }
}
