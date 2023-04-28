using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using EmbyKinopoiskRu.Api.KinopoiskDev;
using EmbyKinopoiskRu.Api.KinopoiskDev.Model;
using EmbyKinopoiskRu.Api.KinopoiskDev.Model.Movie;
using EmbyKinopoiskRu.ScheduledTasks.Model;

using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Querying;
using MediaBrowser.Model.Serialization;

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
        internal static async Task<CollectionFolder> InsureCollectionLibraryFolder(ILibraryManager libraryManager, ILogger logger)
        {
            List<CollectionFolder> folders = FindCollectionFolders(libraryManager);
            CollectionFolder toReturn = folders.FirstOrDefault(f => "Collections".Equals(f.Name, StringComparison.Ordinal));
            if (toReturn != null)
            {
                return toReturn;
            }
            logger.Info($"Creating 'Collections' virtual folder");
            var options = new LibraryOptions()
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
            if (!choosenItems.Any())
            {
                logger.Info("Provided list is empty");
                return new List<BaseItem>();
            }
            QueryResult<BaseItem> collectionItems = libraryManager.QueryItems(new InternalItemsQuery()
            {
                IncludeItemTypes = new[] { CollectionType.Movies.ToString(), CollectionType.TvShows.ToString() },
                Recursive = false,
                IsVirtualItem = false,
                AnyProviderIdEquals = choosenItems.Select(id => new KeyValuePair<string, string>(Plugin.PluginKey, id.ToString(CultureInfo.InvariantCulture))).ToList()
            });
            var toReturn = collectionItems.Items.ToList();
            logger.Info($"Internal objects with Kinopoisk ids: {toReturn.Count}");

            if (toReturn.Count != choosenItems.Count)
            {
                logger.Info("Searching for another objects with another metadata providers");
                foreach (BaseItem item in collectionItems.Items)
                {
                    if (long.TryParse(item.GetProviderId(Plugin.PluginKey), out var id))
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
                logger.Info($"Found {imdbList.Count} IMDB ids");
                QueryResult<BaseItem> imdbCollectionItems = imdbList.Any()
                    ? libraryManager.QueryItems(new InternalItemsQuery()
                    {
                        IncludeItemTypes = new[] { CollectionType.Movies.ToString(), CollectionType.TvShows.ToString() },
                        Recursive = false,
                        IsVirtualItem = false,
                        AnyProviderIdEquals = imdbList
                            .Select(id => new KeyValuePair<string, string>(MetadataProviders.Imdb.ToString(), id))
                            .ToList()
                    })
                    : new QueryResult<BaseItem>();
                logger.Info($"Found {imdbCollectionItems.Items.Length} internal IMDB objects");

                var tmdbList = kpExternalIdist
                    .Where(e => e.Tmdb != null && e.Tmdb > 0)
                    .Select(e => e.Tmdb.ToString())
                    .Cast<string>()
                    .ToList();
                logger.Info($"Found {tmdbList.Count} TMDB ids");
                QueryResult<BaseItem> tmdbCollectionItems = tmdbList.Any()
                    ? libraryManager.QueryItems(new InternalItemsQuery()
                    {
                        IncludeItemTypes = new[] { CollectionType.Movies.ToString(), CollectionType.TvShows.ToString() },
                        Recursive = false,
                        IsVirtualItem = false,
                        AnyProviderIdEquals = tmdbList
                            .Select(id => new KeyValuePair<string, string>(MetadataProviders.Tmdb.ToString(), id))
                            .ToList()
                    })
                    : new QueryResult<BaseItem>();
                logger.Info($"Found {tmdbCollectionItems.Items.Length} internal TMDB objects");

                var list = new List<BaseItem>(imdbCollectionItems.Items);
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
        internal static BoxSet SearchExistingCollection(List<long> internalIds, ILibraryManager libraryManager, ILogger logger)
        {
            logger.Info($"Search existing collections for item with ids: '{string.Join(" ,", internalIds)}'");
            var boxsetList = new List<KeyValuePair<BoxSet, int>>();
            QueryResult<BaseItem> collections = libraryManager.QueryItems(new InternalItemsQuery()
            {
                IncludeItemTypes = new[] { "boxset" },
                ListItemIds = internalIds.ToArray()
            });
            foreach (BoxSet collection in collections.Items.Cast<BoxSet>())
            {
                QueryResult<BaseItem> collectionItems = libraryManager.QueryItems(new InternalItemsQuery()
                {
                    IncludeItemTypes = new[] { CollectionType.Movies.ToString(), CollectionType.TvShows.ToString() },
                    Recursive = false,
                    IsVirtualItem = false,
                    CollectionIds = new long[] { collection.InternalId }
                });
                var count = collectionItems.Items.Select(i => internalIds.Contains(i.InternalId)).Count();
                if (collectionItems.TotalRecordCount <= internalIds.Count && collectionItems.TotalRecordCount == count)
                {
                    boxsetList.Add(new KeyValuePair<BoxSet, int>(collection, count));
                }
            }
            logger.Info($"Found {boxsetList.Count} potential collections with names: '{string.Join("', '", boxsetList.Select(p => p.Key.Name))}'");
            KeyValuePair<BoxSet, int> boxsetTmp = boxsetList
                    .OrderByDescending(boxset => boxset.Value)
                    .FirstOrDefault();

            return boxsetTmp.Value == 0 ? null : boxsetTmp.Key;
        }
        internal static Dictionary<string, string> GetAvailableTransactionsForTasks(string key)
        {
            var basePath = Plugin.Instance.GetType().Namespace + $".i18n.ScheduledTasks.{key}.";
            return Assembly.GetExecutingAssembly().GetManifestResourceNames()
                .Where(i => i.StartsWith(basePath, StringComparison.OrdinalIgnoreCase))
                .Select(i =>
                    new TranslationInfo()
                    {
                        Locale = Path.GetFileNameWithoutExtension(i.Substring(basePath.Length)),
                        EmbeddedResourcePath = i
                    })
                .ToDictionary(i => i.Locale, j => j.EmbeddedResourcePath);
        }
        internal static TaskTranslation GetTaskTranslation(
            Dictionary<string, TaskTranslation> translations,
            IServerConfigurationManager serverConfigurationManager,
            IJsonSerializer jsonSerializer,
            Dictionary<string, string> availableTranslations)
        {
            if (!translations.TryGetValue(serverConfigurationManager.Configuration.UICulture, out TaskTranslation translation))
            {
                if (!availableTranslations.TryGetValue(serverConfigurationManager.Configuration.UICulture, out var resourcePath))
                {
                    resourcePath = availableTranslations["en-US"];
                }
                using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourcePath))
                {
                    translation = jsonSerializer.DeserializeFromStream<TaskTranslation>(stream);
                }
                if (translation != null)
                {
                    lock (translations)
                    {
                        translations.Add(serverConfigurationManager.Configuration.UICulture, translation);
                    }
                }
            }
            return translation;
        }
    }
}
