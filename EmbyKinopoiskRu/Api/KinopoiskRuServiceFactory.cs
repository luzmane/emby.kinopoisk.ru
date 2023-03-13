using System;
using System.Collections.Generic;

using EmbyKinopoiskRu.Api.KinopoiskApiUnofficial;
using EmbyKinopoiskRu.Api.KinopoiskDev;
using EmbyKinopoiskRu.Configuration;

using MediaBrowser.Common.Net;
using MediaBrowser.Model.Activity;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Serialization;

namespace EmbyKinopoiskRu.Api
{
    public static class KinopoiskRuServiceFactory
    {
        private static readonly Dictionary<string, IKinopoiskRuService> KinopoiskServiciesDictionary = new();
        private static IHttpClient? _httpClient;
        private static IJsonSerializer? _jsonSerializer;
        private static ILogManager? _logManager;
        private static ILogger? _log;
        private static IActivityManager? _activityManager;


        public static void Init(
            ILogManager logManager
            , IHttpClient httpClient
            , IJsonSerializer jsonSerializer
            , IActivityManager activityManager
            )
        {
            _httpClient = httpClient;
            _jsonSerializer = jsonSerializer;
            _logManager = logManager;
            _activityManager = activityManager;
            _log = _logManager.GetLogger("KinopoiskRuServiceFactory");
        }
        public static IKinopoiskRuService GetService()
        {
            if (PluginConfiguration.KinopoiskDev.Equals(Plugin.Instance?.Configuration.ApiType, StringComparison.Ordinal))
            {
                _log!.Info($"Fetching {PluginConfiguration.KinopoiskDev} service");
                if (!KinopoiskServiciesDictionary.TryGetValue("KinopoiskDev", out IKinopoiskRuService? result))
                {
                    result = new KinopoiskDevService(_logManager!, _httpClient!, _jsonSerializer!, _activityManager!);
                    KinopoiskServiciesDictionary.Add("KinopoiskDev", result);
                }
                return result;
            }
            if (PluginConfiguration.KinopoiskAPIUnofficialTech.Equals(Plugin.Instance?.Configuration.ApiType, StringComparison.Ordinal))
            {
                _log!.Info($"Fetching {PluginConfiguration.KinopoiskAPIUnofficialTech} service");
                if (!KinopoiskServiciesDictionary.TryGetValue("KinopoiskUnofficial", out IKinopoiskRuService? result))
                {
                    result = new KinopoiskUnofficialService(_logManager!, _httpClient!, _jsonSerializer!, _activityManager!);
                    KinopoiskServiciesDictionary.Add("KinopoiskUnofficial", result);
                }
                return result;
            }
            throw new Exception($"Unable to recognize provided API type '{Plugin.Instance?.Configuration.ApiType}'");
        }

    }
}
