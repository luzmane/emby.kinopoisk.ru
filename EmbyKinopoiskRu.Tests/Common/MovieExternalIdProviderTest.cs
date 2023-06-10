using EmbyKinopoiskRu.Provider.ExternalId;

using FluentAssertions;

using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Entities.TV;

using NLog;

namespace EmbyKinopoiskRu.Tests.Common;

public class MovieExternalIdProviderTest
{
    private readonly ILogger _logger = LogManager.GetLogger(nameof(MovieExternalIdProviderTest));

    [Fact]
    public void MovieExternalIdProvider_ForCodeCoverage()
    {
        _logger.Info($"Start '{nameof(MovieExternalIdProvider_ForCodeCoverage)}'");

        MovieExternalIdProvider movieExternalIdProvider = new();

        movieExternalIdProvider.Name.Should().Be(Plugin.PluginName, "has value of Plugin.PluginName");
        movieExternalIdProvider.Key.Should().Be(Plugin.PluginKey, "has value of Plugin.PluginKey");
        movieExternalIdProvider.UrlFormatString.Should().Be("https://www.kinopoisk.ru/film/{0}/", "this is constant");

        movieExternalIdProvider.Supports(new Movie()).Should().BeTrue("this provider supports only Movie");
        movieExternalIdProvider.Supports(new Series()).Should().BeFalse("this provider supports only Movie");

        _logger.Info($"Finished '{nameof(MovieExternalIdProvider_ForCodeCoverage)}'");
    }

}
