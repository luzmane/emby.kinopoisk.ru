using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Model.Providers;

namespace EmbyKinopoiskRu.Api
{
    internal interface IKinopoiskRuService
    {
        Task<MetadataResult<Movie>> GetMetadataAsync(MovieInfo info, CancellationToken cancellationToken);
        Task<IEnumerable<RemoteSearchResult>> GetSearchResultsAsync(MovieInfo searchInfo, CancellationToken cancellationToken);
        Task<List<Movie>> GetMoviesByOriginalNameAndYearAsync(string name, int? year, CancellationToken cancellationToken);

        Task<MetadataResult<Series>> GetMetadataAsync(SeriesInfo info, CancellationToken cancellationToken);
        Task<IEnumerable<RemoteSearchResult>> GetSearchResultsAsync(SeriesInfo searchInfo, CancellationToken cancellationToken);

        Task<MetadataResult<Episode>> GetMetadataAsync(EpisodeInfo info, CancellationToken cancellationToken);

        Task<MetadataResult<Person>> GetMetadataAsync(PersonLookupInfo info, CancellationToken cancellationToken);
        Task<IEnumerable<RemoteSearchResult>> GetSearchResultsAsync(PersonLookupInfo searchInfo, CancellationToken cancellationToken);

        Task<IEnumerable<RemoteImageInfo>> GetImagesAsync(BaseItem item, LibraryOptions libraryOptions, CancellationToken cancellationToken);

        Task<List<BaseItem>> GetTop250CollectionAsync(CancellationToken cancellationToken);
        Task<ApiResult<Dictionary<string, long>>> GetKpIdByAnotherIdAsync(string externalIdType, IEnumerable<string> idList, CancellationToken cancellationToken);
    }
}
