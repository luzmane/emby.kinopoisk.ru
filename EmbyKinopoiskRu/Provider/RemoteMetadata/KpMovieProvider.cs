using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EmbyKinopoiskRu.Api;
using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Providers;

namespace EmbyKinopoiskRu.Provider.RemoteMetadata
{
    public class KpMovieProvider : IRemoteMetadataProvider<Movie, MovieInfo>
    {
        private readonly IHttpClient _httpClient;

        public string Name => Plugin.PluginName;

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
            return await KinopoiskRuServiceFactory.GetService().GetMetadata(info, cancellationToken);
        }
        public async Task<IEnumerable<RemoteSearchResult>> GetSearchResults(MovieInfo searchInfo, CancellationToken cancellationToken)
        {
            return await KinopoiskRuServiceFactory.GetService().GetSearchResults(searchInfo, cancellationToken);
        }
    }
}
