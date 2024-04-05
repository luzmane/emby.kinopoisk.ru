using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using EmbyKinopoiskRu.Api;
using EmbyKinopoiskRu.Api.KinopoiskApiUnofficial;
using EmbyKinopoiskRu.Api.KinopoiskDev;
using EmbyKinopoiskRu.Configuration;

using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Net;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Controller.Collections;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Notifications;
using MediaBrowser.Model.Activity;
using MediaBrowser.Model.Drawing;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;

namespace EmbyKinopoiskRu
{
    /// <summary>
    /// The main plugin.
    /// </summary>
    public class Plugin : BasePlugin<PluginConfiguration>, IHasThumbImage, IHasWebPages, IHasTranslations
    {
        internal const string PluginKey = "KinopoiskRu";
        internal const string PluginName = "Кинопоиск";
        internal const string PluginGuid = "0417364b-5a93-4ad0-a5f0-b8756957cf80";

        /// <summary>
        /// Gets the current plugin instance.
        /// </summary>
        public static Plugin Instance { get; private set; }

        private readonly Dictionary<string, IKinopoiskRuService> _kinopoiskServicesDictionary = new Dictionary<string, IKinopoiskRuService>();
        private readonly IHttpClient _httpClient;
        internal readonly IJsonSerializer JsonSerializer;
        private readonly ILogManager _logManager;
        private readonly ILogger _log;
        private readonly IActivityManager _activityManager;
        private readonly ILibraryManager _libraryManager;
        private readonly ICollectionManager _collectionManager;
        private readonly INotificationManager _notificationManager;

        /// <inheritdoc />
        public override string Name => PluginName;

        /// <inheritdoc />
        public override string Description => "Fetch metadata from Kinopoisk.ru";

        /// <inheritdoc />
        public ImageFormat ThumbImageFormat => ImageFormat.Png;

        /// <summary>
        /// Initializes a new instance of the <see cref="Plugin"/> class.
        /// </summary>
        /// <param name="applicationPaths">Instance of the <see cref="IApplicationPaths"/> interface.</param>
        /// <param name="xmlSerializer">Instance of the <see cref="IXmlSerializer"/> interface.</param>
        /// <param name="logManager">Instance of the <see cref="ILogManager"/> interface.</param>
        /// <param name="httpClient">Instance of the <see cref="IHttpClient"/> interface.</param>
        /// <param name="jsonSerializer">Instance of the <see cref="IJsonSerializer"/> interface.</param>
        /// <param name="activityManager">Instance of the <see cref="IActivityManager"/> interface.</param>
        /// <param name="libraryManager">Instance of the <see cref="ILibraryManager"/> interface.</param>
        /// <param name="collectionManager">Instance of the <see cref="ICollectionManager"/> interface.</param>
        /// <param name="notificationManager">Instance of the <see cref="INotificationManager"/> interface.</param>
        public Plugin(
            IApplicationPaths applicationPaths,
            IXmlSerializer xmlSerializer,
            ILogManager logManager,
            IHttpClient httpClient,
            IJsonSerializer jsonSerializer,
            IActivityManager activityManager,
            ILibraryManager libraryManager,
            ICollectionManager collectionManager,
            INotificationManager notificationManager)
            : base(applicationPaths, xmlSerializer)
        {
            Instance = this;
            SetId(new Guid(PluginGuid));

            _httpClient = httpClient;
            JsonSerializer = jsonSerializer;
            _logManager = logManager;
            _activityManager = activityManager;
            _log = _logManager.GetLogger(PluginKey);
            _libraryManager = libraryManager;
            _collectionManager = collectionManager;
            _notificationManager = notificationManager;
        }

        /// <inheritdoc />
        public Stream GetThumbImage()
        {
            Type type = GetType();
            return type.Assembly.GetManifestResourceStream(type.Namespace + ".thumb.png");
        }

        /// <inheritdoc />
        public IEnumerable<PluginPageInfo> GetPages()
        {
            return new[] { new PluginPageInfo { Name = "kinopoiskru", EmbeddedResourcePath = GetType().Namespace + ".Configuration.kinopoiskru.html" }, new PluginPageInfo { Name = "kinopoiskrujs", EmbeddedResourcePath = GetType().Namespace + ".Configuration.kinopoiskru.js" } };
        }

        /// <inheritdoc />
        public TranslationInfo[] GetTranslations()
        {
            var basePath = GetType().Namespace + ".i18n.Configuration.";
            return GetType().Assembly.GetManifestResourceNames()
                .Where(i => i.StartsWith(basePath, StringComparison.OrdinalIgnoreCase))
                .Select(i =>
                    new TranslationInfo { Locale = Path.GetFileNameWithoutExtension(i.Substring(basePath.Length)), EmbeddedResourcePath = i })
                .ToArray();
        }

        internal IKinopoiskRuService GetKinopoiskService()
        {
            if (PluginConfiguration.KinopoiskDev.Equals(Configuration.ApiType, StringComparison.Ordinal))
            {
                _log.Info($"Fetching {PluginConfiguration.KinopoiskDev} service");
                if (_kinopoiskServicesDictionary.TryGetValue("KinopoiskDev", out IKinopoiskRuService devService))
                {
                    return devService;
                }

                devService = new KinopoiskDevService(
                    _logManager,
                    _httpClient,
                    JsonSerializer,
                    _activityManager,
                    _libraryManager,
                    _collectionManager,
                    _notificationManager);
                _kinopoiskServicesDictionary.Add("KinopoiskDev", devService);
                return devService;
            }

            if (!PluginConfiguration.KinopoiskApiUnofficialTech.Equals(Configuration.ApiType, StringComparison.Ordinal))
            {
                throw new Exception($"Unable to recognize provided API type '{Configuration.ApiType}'");
            }

            _log.Info($"Fetching {PluginConfiguration.KinopoiskApiUnofficialTech} service");
            if (_kinopoiskServicesDictionary.TryGetValue("KinopoiskUnofficial", out IKinopoiskRuService unOfficialService))
            {
                return unOfficialService;
            }

            unOfficialService = new KinopoiskUnofficialService(
                _logManager,
                _httpClient,
                JsonSerializer,
                _activityManager,
                _notificationManager);
            _kinopoiskServicesDictionary.Add("KinopoiskUnofficial", unOfficialService);
            return unOfficialService;
        }
    }
}
