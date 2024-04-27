using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using EmbyKinopoiskRu.Configuration;
using EmbyKinopoiskRu.Helper;

using MediaBrowser.Controller.Configuration;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Serialization;
using MediaBrowser.Model.Tasks;

namespace EmbyKinopoiskRu.ScheduledTasks
{
    /// <summary>
    /// Clean downloaded intros
    /// </summary>
    public class IntrosCleanerTask : BaseTask, IScheduledTask, IConfigurableScheduledTask
    {
        private const string TaskKey = "KinopoiskCleanIntros";

        /// <inheritdoc />
        public string Name => GetTranslation().Name;

        /// <inheritdoc />
        public string Key => TaskKey;

        /// <inheritdoc />
        public string Description => GetTranslation().Description;

        /// <inheritdoc />
        public string Category => GetTranslation().Category;

        /// <inheritdoc />
        public bool IsHidden => false;

        /// <inheritdoc />
        public bool IsEnabled => true;

        /// <inheritdoc />
        public bool IsLogged => true;

        private readonly ILogger _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="jsonSerializer"></param>
        /// <param name="logManager"></param>
        /// <param name="serverConfigurationManager"></param>
        public IntrosCleanerTask(
            IJsonSerializer jsonSerializer,
            ILogManager logManager,
            IServerConfigurationManager serverConfigurationManager)
            : base(TaskKey, jsonSerializer, serverConfigurationManager)
        {
            _logger = logManager.GetLogger(GetType().Name);
        }

        /// <inheritdoc />
        public IEnumerable<TaskTriggerInfo> GetDefaultTriggers()
        {
            return Array.Empty<TaskTriggerInfo>();
        }

        /// <inheritdoc />
        public async Task Execute(CancellationToken cancellationToken, IProgress<double> progress)
        {
            _logger.Info("Kinopoisk intros cleanup started");
            if (!PluginConfiguration.KinopoiskDev.Equals(Plugin.Instance.Configuration.ApiType))
            {
                _logger.Warn("Kinopoisk.dev API is required for the cleanup and trailer collection creation. Exit");
                return;
            }

            var basePath = Plugin.Instance.Configuration.IntrosPath;
            if (string.IsNullOrWhiteSpace(basePath) || !Directory.Exists(basePath))
            {
                _logger.Error($"Intros path from the plugin configuration is invalid: '{basePath}'. Exit");
                return;
            }

            var selectedCollections = Plugin.Instance.Configuration.CollectionsList
                .Where(c => c.IsEnable)
                .Select(c => c.Id)
                .ToList();
            var oneProgress = 100.0 / (selectedCollections.Count + 1);
            double currentProgress = 0;

            // Remove not selected collections
            _logger.Info("Remove not selected collections");
            var localCollections = Directory.GetDirectories(basePath);
            List<string> foldersToRemove = localCollections
                .Where(l => !selectedCollections.Contains(Path.GetFileName(l)))
                .ToList();
            if (localCollections.Length == foldersToRemove.Count)
            {
                _logger.Warn("Looks like you want to delete all collections. Please do this manually. Exit");
                return;
            }

            foldersToRemove.ForEach(RemoveDirectory);
            _logger.Info($"Removed {foldersToRemove.Count} collections");
            currentProgress += oneProgress;
            progress.Report(currentProgress);
            if (cancellationToken.IsCancellationRequested)
            {
                _logger.Info("Cancellation was requested. Exit");
                return;
            }

            // Remove trailers not in collections
            _logger.Info("Clean selected collections from extra videos");
            foreach (var collection in selectedCollections)
            {
                _logger.Info($"Clearing '{collection}'");
                var idList = (await Plugin.Instance.GetKinopoiskService().GetTrailersFromCollectionAsync(collection, cancellationToken))
                    .Select(t => t.Url)
                    .Where(u => !string.IsNullOrWhiteSpace(u) && u.Contains("you"))
                    .Select(YtHelper.GetYoutubeId)
                    .ToList();

                if (idList.Count == 0)
                {
                    _logger.Warn($"Looks like a problem with API - no trailers were found for the collection '{collection}'. Skip collection check");
                    currentProgress += oneProgress;
                    progress.Report(currentProgress);
                    if (cancellationToken.IsCancellationRequested)
                    {
                        _logger.Info("Cancellation was requested. Exit");
                        return;
                    }

                    continue;
                }

                var collectionFolder = Path.Combine(basePath, collection);
                var files = Directory.GetFiles(collectionFolder);
                var cnt = 0;
                foreach (var file in files)
                {
                    var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file);
                    if (!idList.Contains(fileNameWithoutExtension))
                    {
                        RemoveFile(collectionFolder, fileNameWithoutExtension);
                        cnt++;
                    }
                }

                _logger.Info($"Removed {cnt} files from collection '{collection}'");

                currentProgress += oneProgress;
                progress.Report(currentProgress);
                if (cancellationToken.IsCancellationRequested)
                {
                    _logger.Info("Cancellation was requested. Exit");
                    return;
                }
            }
        }

        private void RemoveFile(string folderPath, string fileNameWithoutExtension)
        {
            _logger.Info($"Remove '{Path.Combine(folderPath, fileNameWithoutExtension + ".*")}'");
            var files = Directory.GetFiles(folderPath, fileNameWithoutExtension);
            try
            {
                foreach (var file in files)
                {
                    File.Delete(file);
                }
            }
            catch (Exception ex)
            {
                _logger.ErrorException($"Unable to delete file '{Path.Combine(folderPath, fileNameWithoutExtension + ".*")}' due to: {ex.Message}", ex);
            }
        }

        private void RemoveDirectory(string localCollection)
        {
            _logger.Info($"Remove '{localCollection}'");
            try
            {
                Directory.Delete(localCollection, true);
            }
            catch (Exception ex)
            {
                _logger.ErrorException($"Unable to delete directory due to: {ex.Message}", ex);
            }
        }
    }
}
