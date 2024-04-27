using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Providers;

namespace EmbyKinopoiskRu.Api
{
    internal interface IKinopoiskRuService
    {
        Task<MetadataResult<Movie>> GetMetadataAsync(MovieInfo info, CancellationToken cancellationToken);
        Task<IEnumerable<RemoteSearchResult>> GetSearchResultsAsync(MovieInfo searchInfo, CancellationToken cancellationToken);

        Task<MetadataResult<Series>> GetMetadataAsync(SeriesInfo info, CancellationToken cancellationToken);
        Task<IEnumerable<RemoteSearchResult>> GetSearchResultsAsync(SeriesInfo searchInfo, CancellationToken cancellationToken);

        Task<MetadataResult<Episode>> GetMetadataAsync(EpisodeInfo info, CancellationToken cancellationToken);
        Task<IEnumerable<RemoteSearchResult>> GetSearchResultsAsync(EpisodeInfo searchInfo, CancellationToken cancellationToken);

        Task<MetadataResult<Person>> GetMetadataAsync(PersonLookupInfo info, CancellationToken cancellationToken);
        Task<IEnumerable<RemoteSearchResult>> GetSearchResultsAsync(PersonLookupInfo searchInfo, CancellationToken cancellationToken);

        Task<IEnumerable<RemoteImageInfo>> GetImagesAsync(BaseItem item, CancellationToken cancellationToken);

        Task<List<KpLists>> GetKpCollectionsAsync(CancellationToken cancellationToken);
        Task<List<BaseItem>> GetCollectionItemsAsync(string collectionId, CancellationToken cancellationToken);
        Task<ApiResult<Dictionary<string, long>>> GetKpIdByAnotherIdAsync(string externalIdType, IEnumerable<string> idList, CancellationToken cancellationToken);
        Task<List<KpTrailer>> GetTrailersFromCollectionAsync(string collectionId, CancellationToken cancellationToken);
    }
}
