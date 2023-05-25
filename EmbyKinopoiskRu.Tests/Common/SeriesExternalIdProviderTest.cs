using EmbyKinopoiskRu.Provider.ExternalId;

using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Entities.TV;

using NLog;

namespace EmbyKinopoiskRu.Tests.Common;

public class SeriesExternalIdProviderTest
{
    private readonly ILogger _logger = LogManager.GetLogger(nameof(SeriesExternalIdProviderTest));

    [Fact]
    public void SeriesExternalIdProvider_ForCodeCoverage()
    {
        _logger.Info($"Start '{nameof(SeriesExternalIdProvider_ForCodeCoverage)}'");

        SeriesExternalIdProvider seriesExternalIdProvider = new();

        Assert.Equal(Plugin.PluginName, seriesExternalIdProvider.Name);
        Assert.Equal(Plugin.PluginKey, seriesExternalIdProvider.Key);
        Assert.Equal("https://www.kinopoisk.ru/series/{0}/", seriesExternalIdProvider.UrlFormatString);

        Assert.False(seriesExternalIdProvider.Supports(new Movie()));
        Assert.True(seriesExternalIdProvider.Supports(new Series()));

        _logger.Info($"Finished '{nameof(SeriesExternalIdProvider_ForCodeCoverage)}'");
    }

}
