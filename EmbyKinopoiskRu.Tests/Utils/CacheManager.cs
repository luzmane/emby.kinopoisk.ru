using EasyCaching.Core;
using EasyCaching.Disk;

using Microsoft.Extensions.DependencyInjection;

namespace EmbyKinopoiskRu.Tests.Utils;

public static class CacheManager
{
    private const string CACHE_NAME = "http";
    private const string SERIALIZER_NAME = "json";


    private static readonly IEasyCachingProvider? HttpCache = new ServiceCollection()
            .AddEasyCaching(option =>
                _ = option
                    .WithJson(jsonSerializerSettingsConfigure: x => x.TypeNameHandling = Newtonsoft.Json.TypeNameHandling.None,
                        SERIALIZER_NAME)
                    .UseDisk(cfg =>
                        {
                            cfg.DBConfig = new DiskDbOptions { BasePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Cache") };
                            cfg.SerializerName = SERIALIZER_NAME;
                        }, CACHE_NAME))
            .BuildServiceProvider()
            .GetService<IEasyCachingProviderFactory>()?
            .GetCachingProvider(CACHE_NAME);

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
