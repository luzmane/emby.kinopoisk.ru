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
    public class KinopoiskRuServiceFactory
    {
        private static readonly Dictionary<string, IKinopoiskRuService> _kinopoiskServiciesDictionary = new();
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
            if (PluginConfiguration.KINOPOISKDEV.Equals(Plugin.Instance?.Configuration.ApiType, StringComparison.Ordinal))
            {
                _log!.Info($"Fetching {PluginConfiguration.KINOPOISKDEV} service");
                if (!_kinopoiskServiciesDictionary.TryGetValue("KinopoiskDev", out IKinopoiskRuService? result))
                {
                    result = new KinopoiskDevService(_logManager!, _httpClient!, _jsonSerializer!, _activityManager!);
                    _kinopoiskServiciesDictionary.Add("KinopoiskDev", result);
                }
                return result;
            }
            if (PluginConfiguration.KINOPOISKAPIUNOFFICIALTECH.Equals(Plugin.Instance?.Configuration.ApiType, StringComparison.Ordinal))
            {
                _log!.Info($"Fetching {PluginConfiguration.KINOPOISKAPIUNOFFICIALTECH} service");
                if (!_kinopoiskServiciesDictionary.TryGetValue("KinopoiskUnofficial", out IKinopoiskRuService? result))
                {
                    result = new KinopoiskUnofficialService(_logManager!, _httpClient!, _jsonSerializer!, _activityManager!);
                    _kinopoiskServiciesDictionary.Add("KinopoiskUnofficial", result);
                }
                return result;
            }
#pragma warning disable CA2201
            throw new Exception($"Unable to recognize provided API type '{Plugin.Instance?.Configuration.ApiType}'");
#pragma warning restore CA2201
        }

    }
}
