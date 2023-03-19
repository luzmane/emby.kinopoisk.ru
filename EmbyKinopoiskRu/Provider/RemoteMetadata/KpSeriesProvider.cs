using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Providers;

namespace EmbyKinopoiskRu.Provider.RemoteMetadata
{
    public class KpSeriesProvider : IRemoteMetadataProvider<Series, SeriesInfo>
    {
        private readonly IHttpClient _httpClient;
        public string Name => Plugin.PluginName;

        public KpSeriesProvider(IHttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public Task<HttpResponseInfo> GetImageResponse(string url, CancellationToken cancellationToken)
        {
            return _httpClient.GetResponse(new HttpRequestOptions
            {
                CancellationToken = cancellationToken,
                Url = url,
                BufferContent = false
            });
        }

        public async Task<MetadataResult<Series>> GetMetadata(SeriesInfo info, CancellationToken cancellationToken)
        {
            return await Plugin.Instance!.GetKinopoiskService().GetMetadata(info, cancellationToken);
        }

        public async Task<IEnumerable<RemoteSearchResult>> GetSearchResults(SeriesInfo searchInfo, CancellationToken cancellationToken)
        {
            return await Plugin.Instance!.GetKinopoiskService().GetSearchResults(searchInfo, cancellationToken);
        }
    }
}
