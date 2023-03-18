using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Querying;
using MediaBrowser.Model.Tasks;

namespace EmbyKinopoiskRu.ScheduledTasks
{
    public class CreateKinopoiskIdTask : IScheduledTask, IConfigurableScheduledTask
    {
        private readonly ILogger _log;
        private static bool _isScanRunning;
        private static readonly object ScanLock = new();

        private readonly ILibraryManager _libraryManager;
        private Plugin Plugin { get; set; }
        public string Name => "Add KinopoiskId based on IMDB, TMDB";
        public string Key => "KinopoiskFromOther";
        public string Description => "Add KinopoiskId searching them by IMDB and TMDB ids. Support kinopoisk.dev only";
        public string Category => "KinopoiskRu Plugin";
        public bool IsHidden => false;
        public bool IsEnabled => false;
        public bool IsLogged => true;

        public CreateKinopoiskIdTask(ILogManager logManager, ILibraryManager libraryManager)
        {
            _log = logManager.GetLogger(GetType().Name);
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
                _log.Info("Searching for movies/series without KP id, but with IMDB or TMDB");
                QueryResult<BaseItem> itemsToUpdateResult = _libraryManager.QueryItems(new InternalItemsQuery()
                {
                    MissingAnyProviderId = new[] { Plugin.PluginName },
                    HasAnyProviderId = new[] { MetadataProviders.Imdb.ToString(), MetadataProviders.Tmdb.ToString() },
                });
                _log.Info($"Found {itemsToUpdateResult.TotalRecordCount} items");
                progress.Report(10d);

                if (itemsToUpdateResult.TotalRecordCount == 0)
                {
                    _log.Info("Nothing to update. Exiting");
                }
                else
                {
                    var imdbList = itemsToUpdateResult.Items
                        .Where(m => m.HasProviderId(MetadataProviders.Imdb.ToString()))
                        .Select(m => m.GetProviderId(MetadataProviders.Imdb.ToString()))
                        .ToList();
                    _log.Info($"Requesting KP ID for {imdbList.Count} items by IMDB ID");
                    Dictionary<string, long> imdbMovieIds = await Plugin.GetKinopoiskService().GetKpIdByAnotherId(MetadataProviders.Imdb.ToString(), imdbList, cancellationToken);
                    _log.Info($"Found {imdbMovieIds.Count} KP IDs by IMDB IDs");
                    itemsToUpdateResult.Items
                        .Where(m => m.HasProviderId(MetadataProviders.Imdb.ToString()))
                        .Where(m => imdbMovieIds.ContainsKey(m.GetProviderId(MetadataProviders.Imdb.ToString())))
                        .ToList()
                        .ForEach(m =>
                        {
                            m.SetProviderId(Plugin.PluginName, imdbMovieIds[m.GetProviderId(MetadataProviders.Imdb.ToString())].ToString(CultureInfo.InvariantCulture));
                            m.UpdateToRepository(ItemUpdateType.MetadataEdit);
                        });
                    progress.Report(55d);
                    _log.Info("KP by IMDB updated");

                    var tmdbList = itemsToUpdateResult.Items
                        .Where(m => !m.HasProviderId(MetadataProviders.Imdb.ToString()))
                        .Where(m => m.HasProviderId(MetadataProviders.Tmdb.ToString()))
                        .Select(m => m.GetProviderId(MetadataProviders.Tmdb.ToString()))
                        .ToList();
                    _log.Info($"Requesting KP ID for {tmdbList.Count} items by TMDB ID without IMDB ID");
                    Dictionary<string, long> tmdbMovieIds = await Plugin.GetKinopoiskService().GetKpIdByAnotherId(MetadataProviders.Tmdb.ToString(), tmdbList, cancellationToken);
                    _log.Info($"Found {tmdbMovieIds.Count} KP IDs by TMDB IDs");
                    itemsToUpdateResult.Items
                        .Where(m => !m.HasProviderId(MetadataProviders.Imdb.ToString()))
                        .Where(m => m.HasProviderId(MetadataProviders.Tmdb.ToString()))
                        .Where(m => tmdbMovieIds.ContainsKey(m.GetProviderId(MetadataProviders.Tmdb.ToString())))
                        .ToList()
                        .ForEach(m =>
                        {
                            m.SetProviderId(Plugin.PluginName, tmdbMovieIds[m.GetProviderId(MetadataProviders.Tmdb.ToString())].ToString(CultureInfo.InvariantCulture));
                            m.UpdateToRepository(ItemUpdateType.MetadataEdit);
                        });
                    progress.Report(100d);
                    _log.Info("KP by TMDB updated");
                }
            }
            finally
            {
                _isScanRunning = false;
                _log.Info("Task finished");
            }
        }
    }
}
