using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using EmbyKinopoiskRu.Helper;
using EmbyKinopoiskRu.ScheduledTasks.Model;

using MediaBrowser.Controller.Collections;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Extensions;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Querying;
using MediaBrowser.Model.Serialization;
using MediaBrowser.Model.Tasks;

namespace EmbyKinopoiskRu.ScheduledTasks
{
    public abstract class CreateTop250Base
    {
        protected ILogger Log { get; private set; }
        protected ILibraryManager LibraryManager { get; private set; }
        protected ICollectionManager CollectionManager { get; private set; }
        public string Key { get; private set; }
        protected Plugin Plugin { get; private set; }
        private readonly IServerConfigurationManager _serverConfigurationManager;
        private readonly IJsonSerializer _jsonSerializer;
        private readonly Dictionary<string, TaskTranslation> _translations = new Dictionary<string, TaskTranslation>();
        private readonly Dictionary<string, string> _availableTranslations = new Dictionary<string, string>();
        private readonly string _libraryType;
        private readonly string _itemType;


        protected CreateTop250Base(
            ILibraryManager libraryManager,
            ICollectionManager collectionManager,
            ILogger log,
            IJsonSerializer jsonSerializer,
            IServerConfigurationManager serverConfigurationManager,
            string libraryType,
            string itemType,
            string key)
        {
            Log = log;
            CollectionManager = collectionManager;
            LibraryManager = libraryManager;
            Key = key;
            _libraryType = libraryType;
            _itemType = itemType;
            _jsonSerializer = jsonSerializer;
            _serverConfigurationManager = serverConfigurationManager;
            if (Plugin.Instance == null)
            {
                throw new NullReferenceException($"Plugin '{Plugin.PluginName}' instance is null");
            }
            Plugin = Plugin.Instance;

            _availableTranslations = EmbyHelper.GetAvailableTransactions($"ScheduledTasks.{Key}");
        }

        public virtual IEnumerable<TaskTriggerInfo> GetDefaultTriggers()
        {
            return Array.Empty<TaskTriggerInfo>();
        }

        private async Task UpdateLibrary(List<BaseItem> itemsList, string collectionName)
        {
            if (!itemsList.Any())
            {
                return;
            }
            Log.Info($"Adding {itemsList.Count} items to '{collectionName}'");
            Log.Info($"Check if '{collectionName}' already exists");
            QueryResult<BaseItem> existingCollectionResult = LibraryManager.QueryItems(new InternalItemsQuery()
            {
                IncludeItemTypes = new[] { "BoxSet" },
                Recursive = false,
                Name = collectionName
            });
            Log.Info($"Found {existingCollectionResult.TotalRecordCount} collections: '{string.Join("', '", existingCollectionResult.Items.Select(m => m.Name))}'");
            if (existingCollectionResult.TotalRecordCount == 1)
            {
                var existingCollection = (BoxSet)existingCollectionResult.Items[0];
                Log.Info($"Updating collection with name '{existingCollection.Name}' with following internal ids: '{string.Join(", ", itemsList.Select(m => m.InternalId))}'");
                foreach (BaseItem item in itemsList)
                {
                    if (item.AddCollection(existingCollection))
                    {
                        Log.Info($"Adding '{item.Name}' (internalId '{item.InternalId}') to collection '{existingCollection.Name}'");
                        item.UpdateToRepository(ItemUpdateType.MetadataEdit);
                    }
                    else
                    {
                        Log.Info($"'{item.Name}' (internalId '{item.InternalId}') already in the collection '{existingCollection.Name}'");
                    }
                }
            }
            else if (existingCollectionResult.TotalRecordCount == 0)
            {
                Log.Info($"Creating '{collectionName}' collection with following items: '{string.Join("', '", itemsList.Select(m => m.Name))}'");
                CollectionFolder rootCollectionFolder = await EmbyHelper.InsureCollectionLibraryFolder(LibraryManager, Log);
                if (rootCollectionFolder == null)
                {
                    Log.Info($"The virtual folder 'Collections' was not found nor created. {collectionName} will not be created");
                }
                else
                {
                    _ = await CollectionManager.CreateCollection(new CollectionCreationOptions()
                    {
                        IsLocked = false,
                        Name = collectionName,
                        ParentId = rootCollectionFolder.InternalId,
                        ItemIdList = itemsList.Select(m => m.InternalId).ToArray()
                    });
                    Log.Info("The collection created");
                }
            }
            else
            {
                Log.Warn($"Found more collections than expected {existingCollectionResult.TotalRecordCount}, please open a github issue");
            }
        }

        protected TaskTranslation GetTranslation()
        {
            return EmbyHelper.GetTaskTranslation(_translations, _serverConfigurationManager, _jsonSerializer, _availableTranslations);
        }

        protected async Task Execute(
            IProgress<double> progress,
            string baseCollectionName,
            Func<CancellationToken, Task<List<BaseItem>>> getTop250Collection,
            CancellationToken cancellationToken)
        {
            Log.Info("Fetch top 250 list from API");
            List<BaseItem> videos = await getTop250Collection(cancellationToken);
            if (videos.Count == 0)
            {
                Log.Info("Top 250 list was not fetched from API");
                return;
            }
            Log.Info($"Received {videos.Count} items from API");

            Log.Info($"Get all {_libraryType} libraries");
            QueryResult<BaseItem> librariesResult = LibraryManager.QueryItems(new InternalItemsQuery()
            {
                IncludeItemTypes = new[] { nameof(CollectionFolder) },
                HasPath = true,
                IsFolder = true,
                IsVirtualItem = false
            });
            var libraries = librariesResult.Items
                .Cast<CollectionFolder>()
                .Where(b => _libraryType.EqualsIgnoreCase(b.CollectionType))
                .ToList();
            Log.Info($"Found {libraries.Count} libraries: '{string.Join(", ", libraries.Select(i => i.Name))}'");

            var p = 10d;
            progress.Report(p);

            foreach (CollectionFolder library in libraries)
            {
                await ProcessLibrary(library, videos, baseCollectionName);
                p += 90d / libraries.Count;
                progress.Report(p);
            }
        }

        private async Task ProcessLibrary(CollectionFolder library, List<BaseItem> items, string baseCollectionName)
        {
            Log.Info($"Searching relevant {_libraryType} in '{library.Name}' library");
            var top250ProviderIdList = items
                .SelectMany(m =>
                {
                    var toReturn = new List<KeyValuePair<string, string>>()
                    {
                        new KeyValuePair<string, string>(Plugin.PluginKey, m.GetProviderId(Plugin.PluginKey))
                    };
                    if (m.HasProviderId(MetadataProviders.Imdb.ToString()))
                    {
                        toReturn.Add(new KeyValuePair<string, string>(MetadataProviders.Imdb.ToString(), m.GetProviderId(MetadataProviders.Imdb.ToString())));
                    }
                    if (m.HasProviderId(MetadataProviders.Tmdb.ToString()))
                    {
                        toReturn.Add(new KeyValuePair<string, string>(MetadataProviders.Tmdb.ToString(), m.GetProviderId(MetadataProviders.Tmdb.ToString())));
                    }
                    return toReturn;
                })
                .ToList();
            QueryResult<BaseItem> videosInLibraryQueryResult = top250ProviderIdList.Any()
                ? LibraryManager.QueryItems(new InternalItemsQuery()
                {
                    IncludeItemTypes = new[] { _itemType },
                    AnyProviderIdEquals = top250ProviderIdList,
                    Recursive = false,
                    IsVirtualItem = false,
                    ParentIds = new long[] { library.InternalId },
                })
                : new QueryResult<BaseItem>();
            var videosInLibrary = videosInLibraryQueryResult.Items
                            .Where(i => i.LocationType == LocationType.FileSystem)
                            .Where(i => i.Path != null)
                            .ToList();
            Log.Info($"Found {videosInLibrary.Count} {_itemType} in '{library.Name}' library");

            await UpdateLibrary(videosInLibrary, $"{baseCollectionName} ({library.Name})");

            Log.Info($"Finished with library '{library.Name}'");
        }
    }
}
