using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using EmbyKinopoiskRu.Api;
using EmbyKinopoiskRu.Api.KinopoiskApiUnofficial;
using EmbyKinopoiskRu.Helper;
using EmbyKinopoiskRu.ScheduledTasks.Model;

using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Querying;
using MediaBrowser.Model.Serialization;
using MediaBrowser.Model.Tasks;

namespace EmbyKinopoiskRu.ScheduledTasks
{
    public class CreateKinopoiskIdTask : IScheduledTask, IConfigurableScheduledTask
    {
        private readonly ILogger _log;
        private static bool _isScanRunning;
        private static readonly object ScanLock = new object();
        private const int CHUNK_SIZE = 150;

        private readonly ILibraryManager _libraryManager;
        private readonly IServerConfigurationManager _serverConfigurationManager;
        private readonly IJsonSerializer _jsonSerializer;
        private readonly Dictionary<string, TaskTranslation> _translations = new Dictionary<string, TaskTranslation>();
        private readonly Dictionary<string, string> _availableTranslations = new Dictionary<string, string>();
        private Plugin Plugin { get; set; }
        public string Name => GetTranslation().Name;
        public string Key => "KinopoiskFromOther";
        public string Description => GetTranslation().Description;
        public string Category => GetTranslation().Category;
        public bool IsHidden => false;
        public bool IsEnabled => false;
        public bool IsLogged => true;

        public CreateKinopoiskIdTask(
            ILogManager logManager,
            ILibraryManager libraryManager,
            IServerConfigurationManager serverConfigurationManager,
            IJsonSerializer jsonSerializer)
        {
            _log = logManager.GetLogger(GetType().Name);
            _libraryManager = libraryManager;
            _serverConfigurationManager = serverConfigurationManager;
            _jsonSerializer = jsonSerializer;

            if (Plugin.Instance == null)
            {
                throw new NullReferenceException($"Plugin '{Plugin.PluginName}' instance is null");
            }
            Plugin = Plugin.Instance;

            _availableTranslations = EmbyHelper.GetAvailableTransactionsForTasks(Key);
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
                IKinopoiskRuService kinopoiskRuService = Plugin.GetKinopoiskService();
                if (kinopoiskRuService is KinopoiskUnofficialService)
                {
                    _log.Error("KinopoiskUnofficial doesn't support this task. Exit");
                    return;
                }

                _log.Info("Searching for movies/series without KP id, but with IMDB or TMDB");
                QueryResult<BaseItem> itemsToUpdateResult = _libraryManager.QueryItems(new InternalItemsQuery()
                {
                    IncludeItemTypes = new[] { "movie", "tvshow" },
                    Recursive = false,
                    IsVirtualItem = false,
                    MissingAnyProviderId = new[] { Plugin.PluginKey },
                    HasAnyProviderId = new[] { MetadataProviders.Imdb.ToString(), MetadataProviders.Tmdb.ToString() },
                });
                var names = itemsToUpdateResult.TotalRecordCount > 0 ?
                    ": '" + string.Join("','", itemsToUpdateResult.Items.Select(i => i.Name)) + "'"
                    : string.Empty;
                _log.Info($"Found {itemsToUpdateResult.TotalRecordCount} items{names}");
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
                    _log.Info("Finishing update Kinopoisk IDs by Imdb ID");
                    progress.Report(50d);

                    var tmdbList = itemsToUpdateResult.Items
                        .Where(m => !m.HasProviderId(MetadataProviders.Imdb.ToString()))
                        .Where(m => m.HasProviderId(MetadataProviders.Tmdb.ToString()))
                        .Where(m => !string.IsNullOrWhiteSpace(m.GetProviderId(MetadataProviders.Tmdb.ToString())))
                        .ToList();
                    await UpdateItemsByProviderId(tmdbList, MetadataProviders.Tmdb.ToString(), cancellationToken);
                    _log.Info("Finishing update Kinopoisk IDs by Tmdb ID");
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
                        item.SetProviderId(Plugin.PluginKey, fetchedIds.Item[id].ToString(CultureInfo.InvariantCulture));
                        item.UpdateToRepository(ItemUpdateType.MetadataEdit);
                    }
                    else
                    {
                        _log.Info($"Unable to find Kp id for {item.Name} ({providerId}:{id})");
                    }
                }
            }
        }
        private TaskTranslation GetTranslation()
        {
            return EmbyHelper.GetTaskTranslation(_translations, _serverConfigurationManager, _jsonSerializer, _availableTranslations);
        }
    }
}
