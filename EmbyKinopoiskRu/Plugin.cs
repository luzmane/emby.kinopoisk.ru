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
using MediaBrowser.Model.Activity;
using MediaBrowser.Model.Drawing;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;

namespace EmbyKinopoiskRu
{
    public class Plugin : BasePlugin<PluginConfiguration>, IHasThumbImage, IHasWebPages, IHasTranslations
    {
        public const string PluginKey = "KinopoiskRu";
        public const string PluginName = "Кинопоиск";

        public static Plugin Instance { get; private set; }

        private readonly Dictionary<string, IKinopoiskRuService> _kinopoiskServiciesDictionary = new Dictionary<string, IKinopoiskRuService>();
        private readonly IHttpClient _httpClient;
        private readonly IJsonSerializer _jsonSerializer;
        private readonly ILogManager _logManager;
        private readonly ILogger _log;
        private readonly IActivityManager _activityManager;
        private readonly ILibraryManager _libraryManager;
        private readonly ICollectionManager _collectionManager;

        public override string Name => PluginName;
        public override string Description => "Fetch metadata from Kinopoisk.ru";
        public ImageFormat ThumbImageFormat => ImageFormat.Png;

        public Plugin(
            IApplicationPaths applicationPaths,
            IXmlSerializer xmlSerializer,
            ILogManager logManager,
            IHttpClient httpClient,
            IJsonSerializer jsonSerializer,
            IActivityManager activityManager,
            ILibraryManager libraryManager,
            ICollectionManager collectionManager)
            : base(applicationPaths, xmlSerializer)
        {
            Instance = this;
            SetId(new Guid("0417364b-5a93-4ad0-a5f0-b8756957cf80"));

            _httpClient = httpClient;
            _jsonSerializer = jsonSerializer;
            _logManager = logManager;
            _activityManager = activityManager;
            _log = _logManager.GetLogger(PluginKey);
            _libraryManager = libraryManager;
            _collectionManager = collectionManager;
        }
        public Stream GetThumbImage()
        {
            Type type = GetType();
            return type.Assembly.GetManifestResourceStream(type.Namespace + ".thumb.png");
        }
        public IEnumerable<PluginPageInfo> GetPages()
        {
            return new[]
            {
                new PluginPageInfo
                {
                    Name = "kinopoiskru",
                    EmbeddedResourcePath = GetType().Namespace + ".Configuration.kinopoiskru.html",
                },
                new PluginPageInfo
                {
                    Name = "kinopoiskrujs",
                    EmbeddedResourcePath = GetType().Namespace + ".Configuration.kinopoiskru.js"
                }
            };
        }
        public TranslationInfo[] GetTranslations()
        {
            var basePath = GetType().Namespace + ".i18n.Configuration.";
            return GetType().Assembly.GetManifestResourceNames()
                .Where(i => i.StartsWith(basePath, StringComparison.OrdinalIgnoreCase))
                .Select(i =>
                    new TranslationInfo()
                    {
                        Locale = Path.GetFileNameWithoutExtension(i.Substring(basePath.Length)),
                        EmbeddedResourcePath = i
                    })
                .ToArray();
        }
        public IKinopoiskRuService GetKinopoiskService()
        {
            if (PluginConfiguration.KinopoiskDev.Equals(Configuration.ApiType, StringComparison.Ordinal))
            {
                _log.Info($"Fetching {PluginConfiguration.KinopoiskDev} service");
                if (!_kinopoiskServiciesDictionary.TryGetValue("KinopoiskDev", out IKinopoiskRuService result))
                {
                    result = new KinopoiskDevService(_logManager, _httpClient, _jsonSerializer, _activityManager, _libraryManager, _collectionManager);
                    _kinopoiskServiciesDictionary.Add("KinopoiskDev", result);
                }
                return result;
            }
            if (PluginConfiguration.KinopoiskAPIUnofficialTech.Equals(Configuration.ApiType, StringComparison.Ordinal))
            {
                _log.Info($"Fetching {PluginConfiguration.KinopoiskAPIUnofficialTech} service");
                if (!_kinopoiskServiciesDictionary.TryGetValue("KinopoiskUnofficial", out IKinopoiskRuService result))
                {
                    result = new KinopoiskUnofficialService(_logManager, _httpClient, _jsonSerializer, _activityManager);
                    _kinopoiskServiciesDictionary.Add("KinopoiskUnofficial", result);
                }
                return result;
            }
            throw new Exception($"Unable to recognize provided API type '{Configuration.ApiType}'");
        }
    }
}
