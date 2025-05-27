using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Providers;

namespace EmbyKinopoiskRu.Provider.RemoteMetadata
{
    /// <inheritdoc />
    public class KpSeriesProvider : IRemoteMetadataProvider<Series, SeriesInfo>, IHasMetadataFeatures, IHasSupportedExternalIdentifiers
    {
        private readonly IHttpClient _httpClient;
        private readonly ILogger _log;

        /// <inheritdoc />
        public string Name => Plugin.PluginName;

        /// <inheritdoc />
        public MetadataFeatures[] Features => FeaturesArray;

        private static readonly MetadataFeatures[] FeaturesArray = { MetadataFeatures.Collections, MetadataFeatures.Adult, MetadataFeatures.RequiredSetup };

        /// <summary>
        /// Initializes a new instance of the <see cref="KpSeriesProvider"/> class.
        /// </summary>
        /// <param name="httpClient">Instance of the <see cref="IHttpClient"/> interface.</param>
        /// <param name="logManager">Instance of the <see cref="ILogManager"/> interface.</param>
        public KpSeriesProvider(IHttpClient httpClient, ILogManager logManager)
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
        public async Task<MetadataResult<Series>> GetMetadata(SeriesInfo info, CancellationToken cancellationToken)
        {
            _log.Info($"GetMetadata by SeriesInfo:'{info.Name}', '{info.Year}'");
            return await Plugin.Instance.GetKinopoiskService().GetMetadataAsync(info, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<RemoteSearchResult>> GetSearchResults(SeriesInfo searchInfo, CancellationToken cancellationToken)
        {
            _log.Info($"GetSearchResults by SeriesInfo:'{searchInfo.Name}', '{searchInfo.Year}'");
            return await Plugin.Instance.GetKinopoiskService().GetSearchResultsAsync(searchInfo, cancellationToken);
        }

        /// <inheritdoc />
        public string[] GetSupportedExternalIdentifiers()
        {
            return new string[1]
            {
                Plugin.PluginKey
            };
        }

    }
}
