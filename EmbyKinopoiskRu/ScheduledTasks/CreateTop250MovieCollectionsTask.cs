using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using EmbyKinopoiskRu.Helper;

using MediaBrowser.Controller.Collections;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Querying;
using MediaBrowser.Model.Tasks;

namespace EmbyKinopoiskRu.ScheduledTasks
{
    public class CreateTop250MovieCollectionsTask : IScheduledTask, IConfigurableScheduledTask
    {
        private static bool _isScanRunning;
        private static readonly object ScanLock = new();

        private readonly ILogger _log;
        private readonly ILibraryManager _libraryManager;
        private readonly ICollectionManager _collectionManager;
        private Plugin Plugin { get; set; }

        public string Name => "Create Top250 Movie collection from Kinopoisk";
        public string Key => "KinopoiskTop250Movie";
        public string Description => "Create movie collection based on top 250 list from Kinopoisk.ru. Support kinopoisk.dev only";
        public string Category => "KinopoiskRu Plugin";
        public bool IsHidden => false;
        public bool IsEnabled => false;
        public bool IsLogged => true;

        public CreateTop250MovieCollectionsTask(ILogManager logManager, ILibraryManager libraryManager, ICollectionManager collectionManager)
        {
            _log = logManager.GetLogger(GetType().Name);
            _collectionManager = collectionManager;
            _libraryManager = libraryManager;
            if (Plugin.Instance == null)
            {
                throw new NullReferenceException($"Plugin '{Plugin.PluginName}' instance is null");
            }
            Plugin = Plugin.Instance;
        }

        public IEnumerable<TaskTriggerInfo> GetDefaultTriggers()
        {
            return Array.Empty<TaskTriggerInfo>();
        }

        public async Task Execute(CancellationToken cancellationToken, IProgress<double> progress)
        {
            _log.Info("Task started");
            if (_isScanRunning)
            {
                _log.Info("Another task is running, exiting");
                _log.Info("Task finished");
                return;
            }
            lock (ScanLock)
            {
                if (_isScanRunning)
                {
                    _log.Info("Another task is running, exiting");
                    _log.Info("Task finished");
                    return;
                }
                _isScanRunning = true;
            }
            try
            {
                _log.Info("Fetch top 250 list from API");
                List<Movie> movies = await Plugin.GetKinopoiskService().GetTop250MovieCollection(cancellationToken);
                if (movies.Count == 0)
                {
                    _log.Info("Top 250 film list was not fetched from API");
                    return;
                }
                _log.Info($"Received {movies.Count} items from API");

                _log.Info("Get all libraries");
                QueryResult<BaseItem> librariesResult = _libraryManager.QueryItems(new InternalItemsQuery
                {
                    IncludeItemTypes = new[] { "CollectionFolder" },
                    Recursive = false,
                });
                var libraries = librariesResult.Items
                    .Where(b => b.Name != "Collections")
                    .Cast<CollectionFolder>()
                    .ToList();
                _log.Info($"Found {libraries.Count} libraries: '{string.Join(", ", libraries.Select(i => i.Name))}'");

                var p = 10d;
                progress.Report(p);

                foreach (CollectionFolder library in libraries)
                {
                    await ProcessLibrary(library, movies);
                    p += 90d / libraries.Count;
                    progress.Report(p);
                }
            }
            finally
            {
                _isScanRunning = false;
                _log.Info("Task finished");
            }
        }

        private async Task ProcessLibrary(BaseItem library, List<Movie> movies)
        {
            _log.Info($"Processing '{library.Name}' library");

            _log.Info("Search movies in the library");
            var anyProviderIdEquals = movies
                .SelectMany(m =>
                {
                    List<KeyValuePair<string, string>> toReturn = new()
                    {
                        new KeyValuePair<string, string>(Plugin.PluginName, m.GetProviderId(Plugin.PluginName))
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
            QueryResult<BaseItem> moviesInLibraryQueryResult = _libraryManager.QueryItems(new InternalItemsQuery
            {
                IncludeItemTypes = new[] { "movie" },
                AnyProviderIdEquals = anyProviderIdEquals,
                Recursive = false,
                IsVirtualItem = false,
                ParentIds = new long[] { library.InternalId },
            });
            var moviesInLibrary = moviesInLibraryQueryResult.Items
                            .Where(i => i.LocationType == LocationType.FileSystem && i.MediaType == "Video")
                            .Where(i => i.Path != null && !i.IsVirtualItem)
                            .Where(i => i.GetTopParent() != null && i.Parent.GetParent() is not BoxSet)
                            .ToList();
            _log.Info($"Found {moviesInLibrary.Count} movies in '{library.Name}' library");

            _log.Info($"Check if '{Plugin.Configuration.GetCurrentTop250MovieCollectionName()}' already exists");
            QueryResult<BaseItem> existingCollectionResult = _libraryManager.QueryItems(new InternalItemsQuery
            {
                IncludeItemTypes = new[] { "BoxSet" },
                Recursive = false,
                Name = Plugin.Configuration.GetCurrentTop250MovieCollectionName()
            });
            _log.Info($"Found {existingCollectionResult.TotalRecordCount} collections: '{string.Join("', '", existingCollectionResult.Items.Select(m => m.Name))}'");
            if (existingCollectionResult.TotalRecordCount == 1)
            {
                var existingCollection = (BoxSet)existingCollectionResult.Items[0];
                _log.Info($"Updating collection with name '{existingCollection.Name}' with following internal ids: '{string.Join(", ", moviesInLibrary.Select(m => m.InternalId))}'");
                foreach (BaseItem item in moviesInLibrary)
                {
                    if (item.AddCollection(existingCollection))
                    {
                        _log.Info($"Adding '{item.Name}' (internalId '{item.InternalId}') to collection '{existingCollection.Name}'");
                        item.UpdateToRepository(ItemUpdateType.MetadataEdit);
                    }
                    else
                    {
                        _log.Warn($"'{item.Name}' already in the collection '{existingCollection.Name}'");
                    }
                }
            }
            else
            {
                _log.Info("Create new collection");
                CollectionFolder? rootCollectionFolder = await EmbyHelper.InsureCollectionLibraryFolder(_libraryManager, _log);
                if (rootCollectionFolder == null)
                {
                    _log.Info($"The virtual folder 'Collections' was not found nor created");
                }
                else
                {
                    _ = await _collectionManager.CreateCollection(new CollectionCreationOptions()
                    {
                        IsLocked = false,
                        Name = Plugin.Configuration.GetCurrentTop250MovieCollectionName(),
                        ParentId = rootCollectionFolder.InternalId,
                        ItemIdList = moviesInLibrary.Select(m => m.InternalId).ToArray()
                    });
                    _log.Info("The collection created");
                }
            }
            _log.Info($"Finished with library '{library.Name}'");
        }
    }
}
