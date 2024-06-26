using MediaBrowser.Controller.Providers;

namespace EmbyKinopoiskRu.Helper
{
    internal static class ProviderIdExtensions
    {
        internal static string GetSeriesProviderId(this EpisodeInfo instance, string name)
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
