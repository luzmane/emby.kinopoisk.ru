using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Providers;

namespace EmbyKinopoiskRu.Provider.RemoteMetadata
{
    public class KpPersonProvider : IRemoteMetadataProvider<Person, PersonLookupInfo>
    {
        private readonly IHttpClient _httpClient;
        private readonly ILogger _log;
        public string Name => Plugin.PluginName;

        public KpPersonProvider(IHttpClient httpClient, ILogManager logManager)
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
        public async Task<MetadataResult<Person>> GetMetadata(PersonLookupInfo info, CancellationToken cancellationToken)
        {
            _log.Info($"GetMetadata by PersonLookupInfo:'{info.Name}'");
            return await Plugin.Instance.GetKinopoiskService().GetMetadata(info, cancellationToken);
        }
        public async Task<IEnumerable<RemoteSearchResult>> GetSearchResults(PersonLookupInfo searchInfo, CancellationToken cancellationToken)
        {
            _log.Info($"GetSearchResults by PersonLookupInfo:'{searchInfo.Name}'");
            return await Plugin.Instance.GetKinopoiskService().GetSearchResults(searchInfo, cancellationToken);
        }
    }
}
