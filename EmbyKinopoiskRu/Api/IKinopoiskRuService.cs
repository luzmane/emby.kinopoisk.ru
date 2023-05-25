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
    public interface IKinopoiskRuService
    {
        Task<MetadataResult<Movie>> GetMetadata(MovieInfo info, CancellationToken cancellationToken);
        Task<IEnumerable<RemoteSearchResult>> GetSearchResults(MovieInfo searchInfo, CancellationToken cancellationToken);
        Task<List<Movie>> GetMoviesByOriginalNameAndYear(string name, int? year, CancellationToken cancellationToken);

        Task<MetadataResult<Series>> GetMetadata(SeriesInfo info, CancellationToken cancellationToken);
        Task<IEnumerable<RemoteSearchResult>> GetSearchResults(SeriesInfo searchInfo, CancellationToken cancellationToken);

        Task<MetadataResult<Episode>> GetMetadata(EpisodeInfo info, CancellationToken cancellationToken);

        Task<MetadataResult<Person>> GetMetadata(PersonLookupInfo info, CancellationToken cancellationToken);
        Task<IEnumerable<RemoteSearchResult>> GetSearchResults(PersonLookupInfo searchInfo, CancellationToken cancellationToken);

        Task<IEnumerable<RemoteImageInfo>> GetImages(BaseItem item, LibraryOptions libraryOptions, CancellationToken cancellationToken);

        Task<List<BaseItem>> GetTop250MovieCollection(CancellationToken cancellationToken);
        Task<List<BaseItem>> GetTop250SeriesCollection(CancellationToken cancellationToken);
        Task<ApiResult<Dictionary<string, long>>> GetKpIdByAnotherId(string externalIdType, IEnumerable<string> idList, CancellationToken cancellationToken);
    }
}
