using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Providers;

namespace EmbyKinopoiskRu.Provider.RemoteMetadata
{
    /// <inheritdoc />
    public class KpMovieProvider : IRemoteMetadataProvider<Movie, MovieInfo>, IHasMetadataFeatures
    {
        private readonly IHttpClient _httpClient;
        private readonly ILogger _log;

        /// <inheritdoc />
        public string Name => Plugin.PluginName;

        /// <inheritdoc />
        public MetadataFeatures[] Features => FEATURES;
        private static readonly MetadataFeatures[] FEATURES = new[] { MetadataFeatures.Collections, MetadataFeatures.Adult, MetadataFeatures.RequiredSetup };

        /// <summary>
        /// Initializes a new instance of the <see cref="KpMovieProvider"/> class.
        /// </summary>
        /// <param name="httpClient">Instance of the <see cref="IHttpClient"/> interface.</param>
        /// <param name="logManager">Instance of the <see cref="ILogManager"/> interface.</param>
        public KpMovieProvider(IHttpClient httpClient, ILogManager logManager)
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
        public async Task<MetadataResult<Movie>> GetMetadata(MovieInfo info, CancellationToken cancellationToken)
        {
            _log.Info($"GetMetadata by MovieInfo:'{info.Name}', '{info.Year}'");
            return await Plugin.Instance.GetKinopoiskService().GetMetadataAsync(info, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<RemoteSearchResult>> GetSearchResults(MovieInfo searchInfo, CancellationToken cancellationToken)
        {
            _log.Info($"GetSearchResults by MovieInfo:'{searchInfo.Name}', '{searchInfo.Year}'");
            return await Plugin.Instance.GetKinopoiskService().GetSearchResultsAsync(searchInfo, cancellationToken);
        }
    }
}
