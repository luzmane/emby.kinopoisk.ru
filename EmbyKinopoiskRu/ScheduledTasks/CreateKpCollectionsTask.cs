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
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Extensions;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Querying;
using MediaBrowser.Model.Serialization;
using MediaBrowser.Model.Tasks;

namespace EmbyKinopoiskRu.ScheduledTasks
{
    /// <inheritdoc />
    public class CreateKpCollectionsTask : IScheduledTask, IConfigurableScheduledTask
    {
        private static bool s_isScanRunning;
        private static readonly object ScanLock = new object();

        private readonly Dictionary<string, TaskTranslation> _translations = new Dictionary<string, TaskTranslation>();
        private readonly Dictionary<string, string> _availableTranslations = new Dictionary<string, string>();
        private readonly IServerConfigurationManager _serverConfigurationManager;
        private readonly IJsonSerializer _jsonSerializer;
        private readonly ILogger _logger;
        private readonly ILibraryManager _libraryManager;
        private readonly ICollectionManager _collectionManager;
        private readonly Plugin _plugin;

        /// <inheritdoc />
        public string Name => GetTranslation().Name;

        /// <inheritdoc />
        public string Description => GetTranslation().Description;

        /// <inheritdoc />
        public string Category => GetTranslation().Category;

        /// <inheritdoc />
        public bool IsHidden => false;

        /// <inheritdoc />
        public bool IsEnabled => false;

        /// <inheritdoc />
        public bool IsLogged => true;

        /// <inheritdoc />
        public string Key => "KinopoiskCollections";


        /// <summary>
        /// Initializes a new instance of the <see cref="CreateKpCollectionsTask"/> class.
        /// </summary>
        /// <param name="logManager">Instance of the <see cref="ILogManager"/> interface.</param>
        /// <param name="libraryManager">Instance of the <see cref="ILibraryManager"/> interface.</param>
        /// <param name="collectionManager">Instance of the <see cref="ICollectionManager"/> interface.</param>
        /// <param name="jsonSerializer">Instance of the <see cref="IJsonSerializer"/> interface.</param>
        /// <param name="serverConfigurationManager">Instance of the <see cref="IServerConfigurationManager"/> interface.</param>
        public CreateKpCollectionsTask(
                  ILogManager logManager,
                  ILibraryManager libraryManager,
                  ICollectionManager collectionManager,
                  IJsonSerializer jsonSerializer,
                  IServerConfigurationManager serverConfigurationManager)
        {
            _logger = logManager.GetLogger(GetType().Name);
            if (Plugin.Instance == null)
            {
                throw new NullReferenceException($"Plugin '{Plugin.PluginName}' instance is null");
            }
            _plugin = Plugin.Instance;
            _collectionManager = collectionManager;
            _libraryManager = libraryManager;
            _jsonSerializer = jsonSerializer;
            _serverConfigurationManager = serverConfigurationManager;
            _availableTranslations = EmbyHelper.GetAvailableTransactions($"ScheduledTasks.{Key}");
        }

        /// <inheritdoc />
        public async Task Execute(CancellationToken cancellationToken, IProgress<double> progress)
        {
            _logger.Info("Task started");
            if (s_isScanRunning)
            {
                _logger.Info("Another task is running, exiting");
                _logger.Info("Task finished");
                return;
            }
            lock (ScanLock)
            {
                if (s_isScanRunning)
                {
                    _logger.Info("Another task is running, exiting");
                    _logger.Info("Task finished");
                    return;
                }
                s_isScanRunning = true;
            }
            try
            {
                await CreateKpCollectionsAsync(cancellationToken, progress);
            }
            finally
            {
                s_isScanRunning = false;
                _logger.Info("Task finished");
            }
        }

        /// <summary>
        /// Gets default triggers
        /// </summary>
        public IEnumerable<TaskTriggerInfo> GetDefaultTriggers()
        {
            return Array.Empty<TaskTriggerInfo>();
        }

        private TaskTranslation GetTranslation()
        {
            return EmbyHelper.GetTaskTranslation(_translations, _serverConfigurationManager, _jsonSerializer, _availableTranslations);
        }
        private async Task CreateKpCollectionsAsync(CancellationToken cancellationToken, IProgress<double> progress)
        {
            var collections = _plugin.Configuration.CollectionsList.Where(x => x.IsEnable).ToList();
            _logger.Info($"{collections.Count} collections were chosen in configuration: '{string.Join("', '", collections.Select(i => i.Name))}'");
            if (!collections.Any())
            {
                _logger.Info($"No collections were chosen");
                return;
            }

            var libraries = GetLibraries();
            var librariesIds = libraries.Select(l => l.InternalId).ToArray();
            var p = 5d;
            progress.Report(p);
            var inc = 95d / collections.Count;

            foreach (var collection in collections)
            {
                _logger.Info($"Processing '{collection.Name}', Id '{collection.Id}' collection");
                List<BaseItem> videos = await _plugin.GetKinopoiskService().GetCollectionItemsAsync(collection.Id, cancellationToken);
                if (videos.Count == 0)
                {
                    _logger.Warn($"No items were found for '{collection.Name}', Id '{collection.Id}'");
                    p += inc;
                    progress.Report(p);
                    continue;
                }
                _logger.Info($"Received {videos.Count} items from API");

                await ProcessCollectionAsync(videos, collection.Name, librariesIds);

                p += inc;
                progress.Report(p);
            }
        }
        private List<CollectionFolder> GetLibraries()
        {
            _logger.Info($"Get all movie, tvshow or mixed libraries");
            QueryResult<BaseItem> librariesResult = _libraryManager.QueryItems(new InternalItemsQuery
            {
                IncludeItemTypes = new[] { nameof(CollectionFolder) },
                HasPath = true,
                IsFolder = true,
                IsVirtualItem = false
            });
            var libraries = librariesResult.Items
                .Cast<CollectionFolder>()
                .Where(b =>
                    b.CollectionType == null
                    || "movies".EqualsIgnoreCase(b.CollectionType)
                    || "tvshows".EqualsIgnoreCase(b.CollectionType)
                )
                .ToList();
            _logger.Info($"Found {libraries.Count} libraries: '{string.Join("', '", libraries.Select(i => i.Name))}'");
            return libraries;
        }
        private async Task ProcessCollectionAsync(List<BaseItem> items, string collectionName, long[] librariesIds)
        {
            var providerIdList = items
                .SelectMany(m =>
                {
                    var toReturn = new List<KeyValuePair<string, string>>
                    {
                        new KeyValuePair<string, string>(Plugin.PluginKey, m.GetProviderId(Plugin.PluginKey))
                    };
                    if (m.HasProviderId(MetadataProviders.Imdb))
                    {
                        toReturn.Add(new KeyValuePair<string, string>(MetadataProviders.Imdb.ToString(), m.GetProviderId(MetadataProviders.Imdb)));
                    }
                    if (m.HasProviderId(MetadataProviders.Tmdb))
                    {
                        toReturn.Add(new KeyValuePair<string, string>(MetadataProviders.Tmdb.ToString(), m.GetProviderId(MetadataProviders.Tmdb)));
                    }
                    return toReturn;
                })
                .ToList();
            var videosInLibrary = FetchInternalIds(providerIdList, librariesIds);

            _logger.Info($"Found {videosInLibrary.Count} internal items for '{collectionName}'");

            await UpdateCollectionAsync(videosInLibrary, collectionName);
            _logger.Info($"Finished creation of '{collectionName}' collection");
        }
        private List<BaseItem> FetchInternalIds(List<KeyValuePair<string, string>> providerIdList, long[] librariesIds)
        {
            const int requestSize = 100;
            List<BaseItem> toReturn = new List<BaseItem>();
            if (providerIdList.Any())
            {
                var times = Math.Ceiling((double)providerIdList.Count / requestSize);
                for (int i = 0; i <= times; i++)
                {
                    var queryResult = _libraryManager.QueryItems(new InternalItemsQuery
                    {
                        IncludeItemTypes = new[] { nameof(Movie), nameof(Series) },
                        AnyProviderIdEquals = providerIdList.Skip(i * requestSize).Take(requestSize).ToList(),
                        Recursive = false,
                        IsVirtualItem = false,
                        ParentIds = librariesIds,
                        HasPath = true,
                    });
                    if (queryResult != null && queryResult.Items.Length > 0)
                    {
                        toReturn.AddRange(queryResult.Items.Where(j => j.LocationType == LocationType.FileSystem));
                    }
                }
            }
            return toReturn;
        }
        private async Task UpdateCollectionAsync(List<BaseItem> itemsList, string collectionName)
        {
            if (!itemsList.Any())
            {
                return;
            }
            _logger.Info($"Adding {itemsList.Count} items to '{collectionName}'");
            _logger.Info($"Check if '{collectionName}' already exists");
            QueryResult<BaseItem> existingCollectionResult = _libraryManager.QueryItems(new InternalItemsQuery
            {
                IncludeItemTypes = new[] { "BoxSet" },
                Recursive = false,
                Name = collectionName
            });
            _logger.Info($"Found {existingCollectionResult.TotalRecordCount} collections: '{string.Join("', '", existingCollectionResult.Items.Select(m => m.Name))}'");
            if (existingCollectionResult.TotalRecordCount == 1)
            {
                var existingCollection = (BoxSet)existingCollectionResult.Items[0];
                _logger.Info($"Updating collection with name '{existingCollection.Name}' with following internal ids: '{string.Join(", ", itemsList.Select(m => m.InternalId))}'");
                foreach (BaseItem item in itemsList)
                {
                    if (item.AddCollection(existingCollection))
                    {
                        _logger.Info($"Adding '{item.Name}' (internalId '{item.InternalId}') to collection '{existingCollection.Name}'");
                        item.UpdateToRepository(ItemUpdateType.MetadataEdit);
                    }
                    else
                    {
                        _logger.Info($"'{item.Name}' (internalId '{item.InternalId}') already in the collection '{existingCollection.Name}'");
                    }
                }
            }
            else if (existingCollectionResult.TotalRecordCount == 0)
            {
                _logger.Info($"Creating '{collectionName}' collection with following items: '{string.Join("', '", itemsList.Select(m => m.Name))}'");
                CollectionFolder rootCollectionFolder = await EmbyHelper.InsureCollectionLibraryFolderAsync(_libraryManager, _logger);
                if (rootCollectionFolder == null)
                {
                    _logger.Info($"The virtual folder 'Collections' was not found nor created. '{collectionName}' will not be created");
                }
                else
                {
                    _ = await _collectionManager.CreateCollection(new CollectionCreationOptions
                    {
                        IsLocked = false,
                        Name = collectionName,
                        ParentId = rootCollectionFolder.InternalId,
                        ItemIdList = itemsList.Select(m => m.InternalId).ToArray()
                    });
                    _logger.Info($"The collection '{collectionName}'created");
                }
            }
            else
            {
                _logger.Warn($"Found more collections than expected {existingCollectionResult.TotalRecordCount}, please open a github issue");
            }
        }
    }
}
