using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Providers;

namespace EmbyKinopoiskRu.Provider.RemoteImage
{
    public class KpImageProvider : IRemoteImageProvider
    {
        private readonly IHttpClient _httpClient;
        public string Name => Plugin.PluginName;

        public KpImageProvider(IHttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public bool Supports(BaseItem item)
        {
            return item is Movie || item is Series;
        }
        public IEnumerable<ImageType> GetSupportedImages(BaseItem item)
        {
            return new List<ImageType>
            {
                ImageType.Primary,
                ImageType.Backdrop,
                ImageType.Logo,
            };
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
        public async Task<IEnumerable<RemoteImageInfo>> GetImages(BaseItem item, LibraryOptions libraryOptions, CancellationToken cancellationToken)
        {
            return await Plugin.Instance.GetKinopoiskService().GetImages(item, libraryOptions, cancellationToken);
        }
    }
}
