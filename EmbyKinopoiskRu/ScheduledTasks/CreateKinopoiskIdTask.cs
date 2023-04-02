using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using EmbyKinopoiskRu.Api;

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
        private const int CHUNK_SIZE = 150;

        private readonly ILibraryManager _libraryManager;
        private Plugin Plugin { get; set; }
        public string Name => "Add KinopoiskId based on IMDB, TMDB";
        public string Key => "KinopoiskFromOther";
        public string Description => "Add KinopoiskId searching them by IMDB and TMDB ids. Support kinopoisk.dev only";
        public string Category => Plugin.PluginTaskCategory;
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
                    IncludeItemTypes = new[] { "movie", "tvshow" },
                    Recursive = false,
                    IsVirtualItem = false,
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
                        .Where(m => !string.IsNullOrWhiteSpace(m.GetProviderId(MetadataProviders.Imdb.ToString())))
                        .ToList();
                    await UpdateItemsByProviderId(imdbList, MetadataProviders.Imdb.ToString(), cancellationToken);
                    _log.Info("Updated Imdb provider id");
                    progress.Report(50d);

                    var tmdbList = itemsToUpdateResult.Items
                        .Where(m => !m.HasProviderId(MetadataProviders.Imdb.ToString()))
                        .Where(m => m.HasProviderId(MetadataProviders.Tmdb.ToString()))
                        .Where(m => !string.IsNullOrWhiteSpace(m.GetProviderId(MetadataProviders.Tmdb.ToString())))
                        .ToList();
                    await UpdateItemsByProviderId(tmdbList, MetadataProviders.Tmdb.ToString(), cancellationToken);
                    _log.Info("Updated Tmdb provider id");
                    progress.Report(100d);
                }
            }
            finally
            {
                _isScanRunning = false;
                _log.Info("Task finished");
            }
        }

        private async Task UpdateItemsByProviderId(List<BaseItem> itemsToUpdate, string providerId, CancellationToken cancellationToken)
        {
            _log.Info($"Requesting KP ID for {itemsToUpdate.Count} items by {providerId} ID from API");
            var count = (int)Math.Ceiling((double)itemsToUpdate.Count / CHUNK_SIZE);
            for (var i = 0; i < count; i++)
            {
                IEnumerable<BaseItem> workingList = itemsToUpdate.Skip(CHUNK_SIZE * i).Take(CHUNK_SIZE);
                ApiResult<Dictionary<string, long>> fetchedIds = await Plugin.GetKinopoiskService()
                    .GetKpIdByAnotherId(providerId, workingList.Select(m => m.GetProviderId(providerId)), cancellationToken);
                if (fetchedIds.HasError)
                {
                    var processed = i == 0 ? workingList.Count() : (CHUNK_SIZE * (i - 1)) + workingList.Count();
                    _log.Warn($"Unable to fetch data from API. Processed {processed} of {itemsToUpdate.Count}");
                    break;
                }
                foreach (BaseItem item in workingList)
                {
                    var id = item.GetProviderId(providerId);
                    if (fetchedIds.Item.ContainsKey(id))
                    {
                        item.SetProviderId(Plugin.PluginName, fetchedIds.Item[id].ToString(CultureInfo.InvariantCulture));
                        item.UpdateToRepository(ItemUpdateType.MetadataEdit);
                    }
                    else
                    {
                        _log.Info($"Unable to find Kp id for {item.Name} ({providerId}:{id})");
                    }
                }
            }
        }
    }
}
