using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using EmbyKinopoiskRu.Api.KinopoiskDev;
using EmbyKinopoiskRu.Api.KinopoiskDev.Model;
using EmbyKinopoiskRu.Api.KinopoiskDev.Model.Movie;

using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Querying;

namespace EmbyKinopoiskRu.Helper
{

    internal class EmbyHelper
    {
        internal static List<CollectionFolder> FindCollectionFolders(ILibraryManager libraryManager)
        {
            return libraryManager
                .GetUserRootFolder()
                .GetChildren(new InternalItemsQuery() { IsFolder = true })
                .OfType<CollectionFolder>()
                .Where(i => MemoryExtensions.Equals(i.CollectionType.AsSpan(), CollectionType.BoxSets.Span, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
        internal static async Task<CollectionFolder?> InsureCollectionLibraryFolder(ILibraryManager libraryManager, ILogger logger)
        {
            List<CollectionFolder> folders = FindCollectionFolders(libraryManager);
            CollectionFolder? toReturn = folders.FirstOrDefault(f => "Collections".Equals(f.Name, StringComparison.Ordinal));
            if (toReturn != null)
            {
                return toReturn;
            }
            logger.Info($"Creating 'Collections' virtual folder");
            LibraryOptions options = new()
            {
                EnableRealtimeMonitor = false,
                SaveLocalMetadata = true,
                ContentType = CollectionType.BoxSets.ToString()
            };
            await libraryManager.AddVirtualFolder("Collections", options, true);
            return FindCollectionFolders(libraryManager).FirstOrDefault(f => "Collections".Equals(f.Name, StringComparison.Ordinal));
        }
        internal static async Task<List<BaseItem>> GetInternalIds(
            List<long> choosenItems,
            ILibraryManager libraryManager,
            ILogger logger,
            KinopoiskDevApi api,
            CancellationToken cancellationToken)
        {
            logger.Info($"Get internalIds of each object in sequence");
            QueryResult<BaseItem> collectionItems = libraryManager.QueryItems(new InternalItemsQuery()
            {
                AnyProviderIdEquals = choosenItems.Select(id => new KeyValuePair<string, string>(Plugin.PluginName, id.ToString(CultureInfo.InvariantCulture))).ToList()
            });
            var toReturn = collectionItems.Items.ToList();
            logger.Info($"Internal objects with Kinopoisk ids: {toReturn.Count}");

            if (toReturn.Count != choosenItems.Count)
            {
                logger.Info("Searching for another objects with another metadata providers");
                foreach (BaseItem item in collectionItems.Items)
                {
                    if (long.TryParse(item.GetProviderId(Plugin.PluginName), out var id))
                    {
                        _ = choosenItems.Remove(id);
                    }
                }
                KpSearchResult<KpMovie> kpSearchResult = await api.GetMoviesByIds(choosenItems.Select(i => i.ToString(CultureInfo.InvariantCulture)).ToList(), cancellationToken);
                var kpExternalIdist = kpSearchResult.Docs
                    .Where(m => m.ExternalId != null)
                    .Select(m => m.ExternalId)
                    .Cast<KpExternalId>()
                    .ToList();

                var imdbList = kpExternalIdist
                    .Where(e => !string.IsNullOrWhiteSpace(e.Imdb))
                    .Select(e => e.Imdb)
                    .Cast<string>()
                    .ToList();
                logger.Info($"Have {imdbList.Count} IMDB ids");
                QueryResult<BaseItem> imdbCollectionItems = libraryManager.QueryItems(new InternalItemsQuery()
                {
                    AnyProviderIdEquals = imdbList
                        .Select(id => new KeyValuePair<string, string>(MetadataProviders.Imdb.ToString(), id))
                        .ToList()
                });
                logger.Info($"Found {imdbCollectionItems.Items.Length} internal IMDB objects");

                var tmdbList = kpExternalIdist
                    .Where(e => e.Tmdb is not null and > 0)
                    .Select(e => e.Tmdb.ToString())
                    .Cast<string>()
                    .ToList();
                logger.Info($"Have {tmdbList.Count} TMDB ids");
                QueryResult<BaseItem> tmdbCollectionItems = libraryManager.QueryItems(new InternalItemsQuery()
                {
                    AnyProviderIdEquals = tmdbList
                        .Select(id => new KeyValuePair<string, string>(MetadataProviders.Tmdb.ToString(), id))
                        .ToList()
                });
                logger.Info($"Found {tmdbCollectionItems.Items.Length} internal TMDB objects");

                List<BaseItem> list = new(imdbCollectionItems.Items);
                foreach (BaseItem item in tmdbCollectionItems.Items)
                {
                    if (list.All(i => i.InternalId != item.InternalId))
                    {
                        list.Add(item);
                    }
                }

                logger.Info($"Adding {list.Count} additional internal objects");
                toReturn.AddRange(list);
            }

            logger.Info($"Found {toReturn.Count} internal Ids: {string.Join(", ", toReturn.Select(i => i.InternalId))}");
            return toReturn;
        }
        internal static BoxSet? SearchExistingCollection(List<long> internalIds, ILibraryManager libraryManager, ILogger logger)
        {
            logger.Info($"Search existing collections for item with ids: '{string.Join(" ,", internalIds)}'");
            List<KeyValuePair<BoxSet, int>> boxsetList = new();
            QueryResult<BaseItem> collections = libraryManager.QueryItems(new InternalItemsQuery
            {
                IncludeItemTypes = new[] { "boxset" },
                ListItemIds = internalIds.ToArray()
            });
            foreach (BoxSet collection in collections.Items.Cast<BoxSet>())
            {
                QueryResult<BaseItem> collectionItems = libraryManager.QueryItems(new InternalItemsQuery
                {
                    CollectionIds = new long[] { collection.InternalId }
                });
                var count = collectionItems.Items.Select(i => internalIds.Contains(i.InternalId)).Count();
                if (collectionItems.TotalRecordCount <= internalIds.Count && collectionItems.TotalRecordCount == count)
                {
                    boxsetList.Add(new(collection, count));
                }
            }
            logger.Info($"Found {boxsetList.Count} potential collections with names: '{string.Join("', '", boxsetList.Select(p => p.Key.Name))}'");
            KeyValuePair<BoxSet, int> boxsetTmp = boxsetList
                    .OrderByDescending(boxset => boxset.Value)
                    .FirstOrDefault();

            return boxsetTmp.Value == 0 ? null : boxsetTmp.Key;
        }

    }
}
