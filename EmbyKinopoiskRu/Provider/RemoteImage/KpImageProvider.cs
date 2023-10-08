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
    /// <inheritdoc />
    public class KpImageProvider : IRemoteImageProvider
    {
        private readonly IHttpClient _httpClient;

        /// <inheritdoc />
        public string Name => Plugin.PluginName;

        /// <summary>
        /// Initializes a new instance of the <see cref="KpImageProvider"/> class.
        /// </summary>
        /// <param name="httpClient">Instance of the <see cref="IHttpClient"/> interface.</param>
        public KpImageProvider(IHttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        /// <inheritdoc />
        public bool Supports(BaseItem item)
        {
            return item is Movie || item is Series;
        }

        /// <inheritdoc />
        public IEnumerable<ImageType> GetSupportedImages(BaseItem item)
        {
            return new List<ImageType>
            {
                ImageType.Primary,
                ImageType.Backdrop,
                ImageType.Logo,
            };
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
        public async Task<IEnumerable<RemoteImageInfo>> GetImages(BaseItem item, LibraryOptions libraryOptions, CancellationToken cancellationToken)
        {
            return await Plugin.Instance.GetKinopoiskService().GetImages(item, libraryOptions, cancellationToken);
        }
    }
}
