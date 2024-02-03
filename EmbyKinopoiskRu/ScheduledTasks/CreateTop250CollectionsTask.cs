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
    public class CreateTop250CollectionsTask// : IScheduledTask, IConfigurableScheduledTask
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
        public string Key => "KinopoiskTop250";


        /// <summary>
        /// Initializes a new instance of the <see cref="CreateTop250CollectionsTask"/> class.
        /// </summary>
        /// <param name="logManager">Instance of the <see cref="ILogManager"/> interface.</param>
        /// <param name="libraryManager">Instance of the <see cref="ILibraryManager"/> interface.</param>
        /// <param name="collectionManager">Instance of the <see cref="ICollectionManager"/> interface.</param>
        /// <param name="jsonSerializer">Instance of the <see cref="IJsonSerializer"/> interface.</param>
        /// <param name="serverConfigurationManager">Instance of the <see cref="IServerConfigurationManager"/> interface.</param>
        public CreateTop250CollectionsTask(
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
                _logger.Info("Fetch top 250 list from API");
                List<BaseItem> videos = await _plugin.GetKinopoiskService().GetTop250CollectionAsync(cancellationToken);
                if (videos.Count == 0)
                {
                    _logger.Info("Top 250 list was not fetched from API");
                    return;
                }
                _logger.Info($"Received {videos.Count} items from API");

                _logger.Info($"Get all movies libraries");
                QueryResult<BaseItem> librariesResult = _libraryManager.QueryItems(new InternalItemsQuery
                {
                    IncludeItemTypes = new[] { nameof(CollectionFolder) },
                    HasPath = true,
                    IsFolder = true,
                    IsVirtualItem = false
                });
                var libraries = librariesResult.Items
                    .Cast<CollectionFolder>()
                    .Where(b => "movies".EqualsIgnoreCase(b.CollectionType))
                    .ToList();
                _logger.Info($"Found {libraries.Count} libraries: '{string.Join(", ", libraries.Select(i => i.Name))}'");

                var p = 10d;
                progress.Report(p);

                foreach (CollectionFolder library in libraries)
                {
                    await ProcessLibraryAsync(library, videos, _plugin.Configuration.GetCurrentTop250CollectionName());
                    p += 90d / libraries.Count;
                    progress.Report(p);
                }
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

        private async Task UpdateLibraryAsync(List<BaseItem> itemsList, string collectionName)
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
                    _logger.Info($"The virtual folder 'Collections' was not found nor created. {collectionName} will not be created");
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
                    _logger.Info("The collection created");
                }
            }
            else
            {
                _logger.Warn($"Found more collections than expected {existingCollectionResult.TotalRecordCount}, please open a github issue");
            }
        }

        private TaskTranslation GetTranslation()
        {
            return EmbyHelper.GetTaskTranslation(_translations, _serverConfigurationManager, _jsonSerializer, _availableTranslations);
        }

        private async Task ProcessLibraryAsync(CollectionFolder library, List<BaseItem> items, string baseCollectionName)
        {
            _logger.Info($"Searching relevant movies in '{library.Name}' library");
            var top250ProviderIdList = items
                .SelectMany(m =>
                {
                    var toReturn = new List<KeyValuePair<string, string>>
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
                ? _libraryManager.QueryItems(new InternalItemsQuery
                {
                    IncludeItemTypes = new[] { nameof(Movie) },
                    AnyProviderIdEquals = top250ProviderIdList,
                    Recursive = false,
                    IsVirtualItem = false,
                    ParentIds = new[] { library.InternalId },
                })
                : new QueryResult<BaseItem>();
            var videosInLibrary = videosInLibraryQueryResult.Items
                            .Where(i => i.LocationType == LocationType.FileSystem)
                            .Where(i => i.Path != null)
                            .ToList();
            _logger.Info($"Found {videosInLibrary.Count} {nameof(Movie)} in '{library.Name}' library");

            string collectionName = _plugin.Configuration.NeedToCreateTop250InOneLib() ?
                $"{baseCollectionName}" :
                $"{baseCollectionName} ({library.Name})";
            await UpdateLibraryAsync(videosInLibrary, collectionName);

            _logger.Info($"Finished with library '{library.Name}'");
        }

    }
}
