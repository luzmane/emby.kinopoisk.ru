using EmbyKinopoiskRu.Provider.ExternalId;

using FluentAssertions;

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

        seriesExternalIdProvider.Name.Should().Be(Plugin.PluginName);
        seriesExternalIdProvider.Key.Should().Be(Plugin.PluginKey);
        seriesExternalIdProvider.UrlFormatString.Should().Be("https://www.kinopoisk.ru/series/{0}/");

        seriesExternalIdProvider.Supports(new Series()).Should().BeTrue();
        seriesExternalIdProvider.Supports(new Movie()).Should().BeFalse();

        _logger.Info($"Finished '{nameof(SeriesExternalIdProvider_ForCodeCoverage)}'");
    }

}
