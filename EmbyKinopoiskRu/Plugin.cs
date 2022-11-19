using System;
using System.Collections.Generic;
using System.IO;
using EmbyKinopoiskRu.Api;
using EmbyKinopoiskRu.Configuration;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Net;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Activity;
using MediaBrowser.Model.Drawing;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;

namespace EmbyKinopoiskRu
{
    public class Plugin : BasePlugin<PluginConfiguration>, IHasThumbImage, IHasWebPages
    {
        public static readonly string PluginName = "KinopoiskRu";
        public static Plugin? Instance { get; private set; }

        public override string Name => PluginName;
        public override string Description => "Fetch metadata from Kinopoisk.ru";
        public ImageFormat ThumbImageFormat => ImageFormat.Png;

        public Plugin(
            IApplicationPaths applicationPaths,
            IXmlSerializer xmlSerializer,
            ILogManager logManager,
            IHttpClient httpClient,
            IJsonSerializer jsonSerializer,
            IActivityManager activityManager
            )
            : base(applicationPaths, xmlSerializer)
        {
            Instance = this;
            SetId(new Guid("0417364b-5a93-4ad0-a5f0-b8756957cf80"));
            KinopoiskRuServiceFactory.Init(logManager, httpClient, jsonSerializer, activityManager);
        }
        public Stream GetThumbImage()
        {
            Type type = GetType();
            return type.Assembly.GetManifestResourceStream(type.Namespace + ".thumb.png")!;
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
    }
}
