using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Providers;

namespace EmbyKinopoiskRu.Provider.RemoteMetadata
{
    public class KpSeriesProvider : IRemoteMetadataProvider<Series, SeriesInfo>
    {
        private readonly IHttpClient _httpClient;
        private readonly ILogger _log;
        public string Name => Plugin.PluginName;

        public KpSeriesProvider(IHttpClient httpClient, ILogManager logManager)
        {
            _httpClient = httpClient;
            _log = logManager.GetLogger(GetType().Name);
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
            _log.Info($"GetMetadata by SeriesInfo:'{info.Name}', '{info.Year}'");
            return await Plugin.Instance.GetKinopoiskService().GetMetadata(info, cancellationToken);
        }

        public async Task<IEnumerable<RemoteSearchResult>> GetSearchResults(SeriesInfo info, CancellationToken cancellationToken)
        {
            _log.Info($"GetSearchResults by SeriesInfo:'{info.Name}', '{info.Year}'");
            return await Plugin.Instance.GetKinopoiskService().GetSearchResults(info, cancellationToken);
        }
    }
}
