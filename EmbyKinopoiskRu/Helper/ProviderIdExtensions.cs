

using MediaBrowser.Controller.Providers;

namespace EmbyKinopoiskRu.Helper
{
    public static class ProviderIdExtensions
    {
        public static bool HasSeriesProviderId(this EpisodeInfo instance, string provider)
        {
            return !string.IsNullOrEmpty(instance.GetSeriesProviderId(provider));
        }

        public static string? GetSeriesProviderId(this EpisodeInfo instance, string name)
        {
            if (instance.SeriesProviderIds == null)
            {
                return null;
            }
            _ = instance.SeriesProviderIds.TryGetValue(name, out var value);
            return value;
        }
    }
}
