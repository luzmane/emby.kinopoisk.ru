using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using EmbyKinopoiskRu.Api;

using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Model.Providers;

namespace EmbyKinopoiskRu.Provider.RemoteMetadata
{
    public class KpMovieProvider : IRemoteMetadataProvider<Movie, MovieInfo>, IHasMetadataFeatures
    {
        private readonly IHttpClient _httpClient;

        public string Name => Plugin.PluginName;

        public MetadataFeatures[] Features => FEATURES;
        private static readonly MetadataFeatures[] FEATURES = new[] { MetadataFeatures.Collections, MetadataFeatures.Adult, MetadataFeatures.RequiredSetup };

        public KpMovieProvider(IHttpClient httpClient)
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
        public async Task<MetadataResult<Movie>> GetMetadata(MovieInfo info, CancellationToken cancellationToken)
        {
            return await Plugin.Instance!.GetService().GetMetadata(info, cancellationToken);
        }
        public async Task<IEnumerable<RemoteSearchResult>> GetSearchResults(MovieInfo searchInfo, CancellationToken cancellationToken)
        {
            return await Plugin.Instance!.GetService().GetSearchResults(searchInfo, cancellationToken);
        }
    }
}
