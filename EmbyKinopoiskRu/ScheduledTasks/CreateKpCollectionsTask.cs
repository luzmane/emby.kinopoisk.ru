using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using EmbyKinopoiskRu.Configuration;
using EmbyKinopoiskRu.Helper;

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
    /// <summary>
    /// Task to create Kinopoisk collections
    /// </summary>
    public class CreateKpCollectionsTask : BaseTask, IScheduledTask, IConfigurableScheduledTask
    {
        private static bool s_isScanRunning;
        private static readonly object ScanLock = new object();
        private const string TaskKey = "KinopoiskCollections";

        private readonly ILogger _logger;
        private readonly ILibraryManager _libraryManager;
        private readonly ICollectionManager _collectionManager;

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
            : base(TaskKey, jsonSerializer, serverConfigurationManager)
        {
            _logger = logManager.GetLogger(GetType().Name);
            _collectionManager = collectionManager;
            _libraryManager = libraryManager;
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

        private async Task CreateKpCollectionsAsync(CancellationToken cancellationToken, IProgress<double> progress)
        {
            List<CollectionItem> collections = Plugin.Instance.Configuration.CollectionsList.Where(x => x.IsEnable).ToList();
            _logger.Info($"{collections.Count} collections were chosen in configuration: '{string.Join("', '", collections.Select(i => i.Name))}'");
            if (!collections.Any())
            {
                _logger.Info("No collections were chosen");
                return;
            }

            List<CollectionFolder> libraries = GetLibraries();
            if (libraries.Count == 0)
            {
                _logger.Info("No libraries having movies, tvshows or mixed as a ContextType found");
                return;
            }

            _logger.Info($"Found {libraries.Count} libraries: '{string.Join("', '", libraries.Select(i => i.Name))}'");
            var librariesIds = libraries.Select(l => l.InternalId).ToArray();
            var p = 5d;
            progress.Report(p);
            var inc = 95d / collections.Count;

            foreach (CollectionItem collection in collections)
            {
                _logger.Info($"Processing '{collection.Name}', Id '{collection.Id}' collection");
                List<BaseItem> videos = await Plugin.Instance.GetKinopoiskService().GetCollectionItemsAsync(collection.Id, cancellationToken);
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
            _logger.Info("Get all movies, tvshows or mixed libraries");
            return EmbyHelper.GetLibraries(_libraryManager)
                .Where(l =>
                    {
                        _logger.Info($"Found '{l.Name}' -> CollectionType: '{l.CollectionType}', IsFolder: '{l.IsFolder}', IsVirtualItem: '{l.IsVirtualItem}'");
                        return l.IsFolder
                               && !l.IsVirtualItem
                               && (l.CollectionType == null
                                   || "movies".EqualsIgnoreCase(l.CollectionType)
                                   || "tvshows".EqualsIgnoreCase(l.CollectionType));
                    }
                )
                .ToList();
        }

        private async Task ProcessCollectionAsync(IEnumerable<BaseItem> items, string collectionName, long[] librariesIds)
        {
            List<KeyValuePair<string, string>> providerIdList = items
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
            _logger.Info($"Fetched {providerIdList.Count} provider IDs for '{collectionName}'");
            List<BaseItem> videosInLibrary = FetchInternalIds(providerIdList, librariesIds);
            _logger.Info($"Found {videosInLibrary.Count} internal items for '{collectionName}'");

            await UpdateCollectionAsync(videosInLibrary, collectionName);
            _logger.Info($"Finished creation of '{collectionName}' collection");
        }

        private List<BaseItem> FetchInternalIds(IReadOnlyCollection<KeyValuePair<string, string>> providerIdList, long[] librariesIds)
        {
            const int requestSize = 100;
            var toReturn = new List<BaseItem>();
            if (!providerIdList.Any())
            {
                _logger.Info("No provider ID found for collection");
                return toReturn;
            }

            var times = Math.Ceiling((double)providerIdList.Count / requestSize);
            for (var page = 0; page < times; page++)
            {
                BaseItem[] queryResult = EmbyHelper.GetPagedMovieSeriesByProviderIdsAndLibraryIds(
                    _libraryManager,
                    providerIdList,
                    librariesIds,
                    page,
                    requestSize);
                if (queryResult.Length > 0)
                {
                    toReturn.AddRange(queryResult.Where(j => j.LocationType == LocationType.FileSystem));
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
            QueryResult<BaseItem> existingCollectionResult = EmbyHelper.GetCollectionByName(_libraryManager, collectionName);
            _logger.Info($"Found {existingCollectionResult.TotalRecordCount} collections: '{string.Join("', '", existingCollectionResult.Items.Select(m => m.Name))}'");
            switch (existingCollectionResult.TotalRecordCount)
            {
                case 0:
                    _logger.Info($"Creating '{collectionName}' collection with following items: '{string.Join("', '", itemsList.Select(m => m.Name))}'");
                    CollectionFolder rootCollectionFolder = EmbyHelper.InsureCollectionLibraryFolderAsync(_libraryManager, _logger);
                    if (rootCollectionFolder == null)
                    {
                        _logger.Info($"The virtual folder 'Collections' was not found nor created. '{collectionName}' will not be created");
                    }
                    else
                    {
                        var currentCollection = await _collectionManager.CreateCollection(new CollectionCreationOptions
                        {
                            IsLocked = false,
                            Name = collectionName,
                            ParentId = rootCollectionFolder.InternalId,
                            ItemIdList = itemsList.Select(m => m.InternalId).ToArray()
                        });

                        // Update collection sort name
                        currentCollection.SortName = $"\"{collectionName}\"";
                        currentCollection.UpdateToRepository(ItemUpdateType.MetadataEdit);

                        _logger.Info($"The collection '{collectionName}' created");
                    }

                    break;
                case 1:
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

                    break;
                default:
                    _logger.Warn($"Found more collections than expected {existingCollectionResult.TotalRecordCount}, please open a github issue");
                    break;
            }
        }
    }
}
