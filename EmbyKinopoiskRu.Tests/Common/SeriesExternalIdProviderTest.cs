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

        seriesExternalIdProvider.Name.Should().Be(Plugin.PluginName, "has value of Plugin.PluginName");
        seriesExternalIdProvider.Key.Should().Be(Plugin.PluginKey, "has value of Plugin.PluginKey");
        seriesExternalIdProvider.UrlFormatString.Should().Be("https://www.kinopoisk.ru/series/{0}/", "this is constant");

        seriesExternalIdProvider.Supports(new Series()).Should().BeTrue("this provider supports only Series");
        seriesExternalIdProvider.Supports(new Movie()).Should().BeFalse("this provider supports only Series");

        _logger.Info($"Finished '{nameof(SeriesExternalIdProvider_ForCodeCoverage)}'");
    }

}
