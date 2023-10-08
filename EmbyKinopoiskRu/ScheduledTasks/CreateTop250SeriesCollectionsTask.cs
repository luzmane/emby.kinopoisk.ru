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
    /// <inheritdoc />
    public class CreateTop250SeriesCollectionsTask : CreateTop250Base, IScheduledTask, IConfigurableScheduledTask
    {
        private static bool _isScanRunning;
        private static readonly object ScanLock = new object();

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

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateTop250SeriesCollectionsTask"/> class.
        /// </summary>
        /// <param name="logManager">Instance of the <see cref="ILogManager"/> interface.</param>
        /// <param name="libraryManager">Instance of the <see cref="ILibraryManager"/> interface.</param>
        /// <param name="collectionManager">Instance of the <see cref="ICollectionManager"/> interface.</param>
        /// <param name="jsonSerializer">Instance of the <see cref="IJsonSerializer"/> interface.</param>
        /// <param name="serverConfigurationManager">Instance of the <see cref="IServerConfigurationManager"/> interface.</param>
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

        /// <inheritdoc />
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
