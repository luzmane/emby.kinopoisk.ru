using EasyCaching.Core;
using EasyCaching.Disk;

using Microsoft.Extensions.DependencyInjection;

namespace EmbyKinopoiskRu.Tests.Utils;

public static class CacheManager
{
    private const string CacheName = "http";
    private const string SerializerName = "json";


    private static readonly IEasyCachingProvider? HttpCache = new ServiceCollection()
        .AddEasyCaching(option =>
            _ = option
                .WithJson(jsonSerializerSettingsConfigure: x => x.TypeNameHandling = Newtonsoft.Json.TypeNameHandling.None,
                    SerializerName)
                .UseDisk(cfg =>
                {
                    cfg.DBConfig = new DiskDbOptions
                    {
                        BasePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Cache")
                    };
                    cfg.SerializerName = SerializerName;
                }, CacheName))
        .BuildServiceProvider()
        .GetService<IEasyCachingProviderFactory>()?
        .GetCachingProvider(CacheName);

    public static void AddToCache(string key, string value)
    {
        if (HttpCache == null)
        {
            throw new FieldAccessException("HttpCache is null");
        }

        HttpCache.Set(key, value, TimeSpan.FromDays(30));
    }

    public static CacheValue<string> GetFromCache(string key)
    {
        if (HttpCache == null)
        {
            throw new FieldAccessException("HttpCache is null");
        }

        return HttpCache.Get<string>(key);
    }
}
