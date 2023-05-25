using System;
using System.Threading;
using System.Threading.Tasks;

using MediaBrowser.Controller.Collections;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Serialization;
using MediaBrowser.Model.Tasks;

namespace EmbyKinopoiskRu.ScheduledTasks
{
    public class CreateTop250MovieCollectionsTask : CreateTop250Base, IScheduledTask, IConfigurableScheduledTask
    {
        private static bool _isScanRunning;
        private static readonly object ScanLock = new object();

        public string Name => GetTranslation().Name;
        public string Description => GetTranslation().Description;
        public string Category => GetTranslation().Category;
        public bool IsHidden => false;
        public bool IsEnabled => false;
        public bool IsLogged => true;

        public CreateTop250MovieCollectionsTask(
            ILogManager logManager,
            ILibraryManager libraryManager,
            ICollectionManager collectionManager,
            IJsonSerializer jsonSerializer,
            IServerConfigurationManager serverConfigurationManager)
            : base(
                libraryManager,
                collectionManager,
                logManager.GetLogger("CreateTop250MovieCollectionsTask"),
                jsonSerializer,
                serverConfigurationManager,
                "movies",
                nameof(Movie),
                "KinopoiskTop250Movie")
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
                await Execute(
                    cancellationToken,
                    progress,
                    Plugin.Configuration.GetCurrentTop250MovieCollectionName(),
                    Plugin.GetKinopoiskService().GetTop250MovieCollection);
            }
            finally
            {
                _isScanRunning = false;
                Log.Info("Task finished");
            }
        }

    }
}
