using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using MediaBrowser.Controller.Collections;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Extensions;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Querying;
using MediaBrowser.Model.Tasks;

namespace EmbyKinopoiskRu.ScheduledTasks
{
    public class CreateTop250MovieCollectionsTask : CreateTop250Base, IScheduledTask, IConfigurableScheduledTask
    {
        private static bool _isScanRunning;
        private static readonly object ScanLock = new();

        public string Name => "Create Top250 Movie collection from Kinopoisk";
        public string Key => "KinopoiskTop250Movie";
        public string Description => "Create movie collection based on top 250 list from Kinopoisk.ru. Support kinopoisk.dev only";
        public string Category => "KinopoiskRu Plugin";
        public bool IsHidden => false;
        public bool IsEnabled => false;
        public bool IsLogged => true;

        public CreateTop250MovieCollectionsTask(ILogManager logManager, ILibraryManager libraryManager, ICollectionManager collectionManager)
            : base(libraryManager, collectionManager, logManager.GetLogger("CreateTop250MovieCollectionsTask"))
        {
        }

        public async Task Execute(CancellationToken cancellationToken, IProgress<double> progress)
        {
            Log.Info("Task started");
            if (_isScanRunning)
            {
                Log.Info("Another task is running, exiting");
                Log.Info("Task finished");
                return;
            }
            lock (ScanLock)
            {
                if (_isScanRunning)
                {
                    Log.Info("Another task is running, exiting");
                    Log.Info("Task finished");
                    return;
                }
                _isScanRunning = true;
            }
            try
            {
                Log.Info("Fetch top 250 list from API");
                List<Movie> movies = await Plugin.GetKinopoiskService().GetTop250MovieCollection(cancellationToken);
                if (movies.Count == 0)
                {
                    Log.Info("Top 250 film list was not fetched from API");
                    return;
                }
                Log.Info($"Received {movies.Count} items from API");

                Log.Info("Get all libraries");
                QueryResult<BaseItem> librariesResult = LibraryManager.QueryItems(new InternalItemsQuery()
                {
                    IncludeItemTypes = new[] { "CollectionFolder" },
                    Recursive = false,
                });
                var libraries = librariesResult.Items
                    .Cast<CollectionFolder>()
                    .Where(b => "movies".EqualsIgnoreCase(b.CollectionType))
                    .ToList();
                Log.Info($"Found {libraries.Count} libraries: '{string.Join(", ", libraries.Select(i => i.Name))}'");

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
                Log.Info("Task finished");
            }
        }

        private async Task ProcessLibrary(BaseItem library, List<Movie> movies)
        {
            Log.Info($"Processing '{library.Name}' library");

            Log.Info("Search movies in the library");
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
            QueryResult<BaseItem> moviesInLibraryQueryResult = anyProviderIdEquals.Any()
                ? LibraryManager.QueryItems(new InternalItemsQuery()
                {
                    IncludeItemTypes = new[] { "movie" },
                    AnyProviderIdEquals = anyProviderIdEquals,
                    Recursive = false,
                    IsVirtualItem = false,
                    ParentIds = new long[] { library.InternalId },
                })
                : new();
            var moviesInLibrary = moviesInLibraryQueryResult.Items
                            .Where(i => i.LocationType == LocationType.FileSystem && i.MediaType == "Video")
                            .Where(i => i.Path != null && !i.IsVirtualItem)
                            .ToList();
            Log.Info($"Found {moviesInLibrary.Count} movies in '{library.Name}' library");

            await UpdateLibrary(moviesInLibrary, Plugin.Configuration.GetCurrentTop250MovieCollectionName());

            Log.Info($"Finished with library '{library.Name}'");
        }
    }
}
