using System;
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
    /// <inheritdoc />
    public class KpEpisodeProvider : IRemoteMetadataProvider<Episode, EpisodeInfo>
    {
        private readonly IHttpClient _httpClient;
        private readonly ILogger _log;

        /// <inheritdoc />
        public string Name => Plugin.PluginName;

        /// <summary>
        /// Initializes a new instance of the <see cref="KpEpisodeProvider"/> class.
        /// </summary>
        /// <param name="httpClient">Instance of the <see cref="IHttpClient"/> interface.</param>
        /// <param name="logManager">Instance of the <see cref="ILogManager"/> interface.</param>
        public KpEpisodeProvider(IHttpClient httpClient, ILogManager logManager)
        {
            _httpClient = httpClient;
            _log = logManager.GetLogger(GetType().Name);
        }

        /// <inheritdoc />
        public Task<HttpResponseInfo> GetImageResponse(string url, CancellationToken cancellationToken)
        {
            return _httpClient.GetResponse(new HttpRequestOptions
            {
                CancellationToken = cancellationToken,
                Url = url,
                BufferContent = false
            });
        }

        /// <inheritdoc />
        public async Task<MetadataResult<Episode>> GetMetadata(EpisodeInfo info, CancellationToken cancellationToken)
        {
            _log.Info($"GetMetadata by EpisodeInfo:'{info.Name}', '{info.Year}'");
            return await Plugin.Instance.GetKinopoiskService().GetMetadataAsync(info, cancellationToken);
        }

        /// <inheritdoc />
        public Task<IEnumerable<RemoteSearchResult>> GetSearchResults(EpisodeInfo searchInfo, CancellationToken cancellationToken)
        {
            _log.Info($"GetSearchResults by EpisodeInfo:'{searchInfo.Name}', '{searchInfo.Year}'");
            throw new NotSupportedException();
        }
    }
}
