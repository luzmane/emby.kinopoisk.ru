using System.Net;
using System.Text;

using EasyCaching.Core;

using EmbyKinopoiskRu.Tests.Utils;

using MediaBrowser.Common.Net;

namespace EmbyKinopoiskRu.Tests.EmbyMock;

public class EmbyHttpClient : IHttpClient
{
    private const string USER_AGENT = "Mozilla/5.0 (X11; Linux x86_64; rv:109.0) Gecko/20100101 Firefox/111.0";

    private static readonly HttpClient Client = new();

    public HttpResponseInfo ReturnResponse { get; set; }

    public async Task<HttpResponseInfo> GetResponse(MediaBrowser.Common.Net.HttpRequestOptions options)
    {
        if (ReturnResponse != null)
        {
            return ReturnResponse;
        }

        if (options == null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        CacheValue<string> cachedResponse = CacheManager.GetFromCache(options.Url);
        if (cachedResponse.HasValue)
        {
            return new HttpResponseInfo()
            {
                Content = new MemoryStream(Encoding.UTF8.GetBytes(cachedResponse.Value)),
                StatusCode = HttpStatusCode.OK
            };
        }

        using var msg = new HttpRequestMessage(HttpMethod.Get, options.Url);
        msg.Headers.Add("User-Agent", USER_AGENT);
        foreach (KeyValuePair<string, string> item in options.RequestHeaders)
        {
            msg.Headers.Add(item.Key, item.Value);
        }
        HttpResponseMessage res = await Client.SendAsync(msg);
        if (res.StatusCode == HttpStatusCode.OK)
        {
            CacheManager.AddToCache(options.Url, await res.Content.ReadAsStringAsync());
        }
        return new HttpResponseInfo()
        {
            Content = await res.Content.ReadAsStreamAsync(),
            StatusCode = res.StatusCode
        };
    }

    public Task<Stream> Get(MediaBrowser.Common.Net.HttpRequestOptions options)
    {
        throw new NotImplementedException();
    }
    public IDisposable GetConnectionContext(MediaBrowser.Common.Net.HttpRequestOptions options)
    {
        throw new NotImplementedException();
    }
    public Task<string> GetTempFile(MediaBrowser.Common.Net.HttpRequestOptions options)
    {
        throw new NotImplementedException();
    }
    public Task<HttpResponseInfo> GetTempFileResponse(MediaBrowser.Common.Net.HttpRequestOptions options)
    {
        throw new NotImplementedException();
    }
    public Task<HttpResponseInfo> Post(MediaBrowser.Common.Net.HttpRequestOptions options)
    {
        throw new NotImplementedException();
    }
    public Task<HttpResponseInfo> SendAsync(MediaBrowser.Common.Net.HttpRequestOptions options, string httpMethod)
    {
        throw new NotImplementedException();
    }
}
