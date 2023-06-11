using System;
using System.Threading;
using System.Threading.Tasks;

using MediaBrowser.Controller.Collections;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Serialization;
using MediaBrowser.Model.Tasks;

namespace EmbyKinopoiskRu.ScheduledTasks
{
    public class CreateTop250SeriesCollectionsTask : CreateTop250Base, IScheduledTask, IConfigurableScheduledTask
    {
        private static bool _isScanRunning;
        private static readonly object ScanLock = new object();

        public string Name => GetTranslation().Name;
        public string Description => GetTranslation().Description;
        public string Category => GetTranslation().Category;
        public bool IsHidden => false;
        public bool IsEnabled => false;
        public bool IsLogged => true;

        public CreateTop250SeriesCollectionsTask(
            ILogManager logManager,
            ILibraryManager libraryManager,
            ICollectionManager collectionManager,
            IJsonSerializer jsonSerializer,
            IServerConfigurationManager serverConfigurationManager)
            : base(
                libraryManager,
                collectionManager,
                logManager.GetLogger(nameof(CreateTop250SeriesCollectionsTask)),
                jsonSerializer,
                serverConfigurationManager,
                "tvshows",
                nameof(Series),
                "KinopoiskTop250Series")
        {
        }

        public async Task Execute(CancellationToken cancellationToken, IProgress<double> progress)
        {
            Log.Info("Task started");
            if (_isScanRunning)
            {
                Log.Info("Another task Kinopoisk Collection creation is running, exiting");
                Log.Info("Task finished");
                return;
            }
            lock (ScanLock)
            {
                if (_isScanRunning)
                {
                    Log.Info("Another task Kinopoisk Collection creation is running, exiting");
                    Log.Info("Task finished");
                    return;
                }
                _isScanRunning = true;
            }
            try
            {
                await Execute(
                    progress,
                    Plugin.Configuration.GetCurrentTop250SeriesCollectionName(),
                    Plugin.GetKinopoiskService().GetTop250SeriesCollection,
                    cancellationToken);
            }
            finally
            {
                _isScanRunning = false;
                Log.Info("Task finished");
            }
        }

    }
}
