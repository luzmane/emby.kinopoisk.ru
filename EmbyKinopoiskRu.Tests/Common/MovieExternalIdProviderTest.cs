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

        movieExternalIdProvider.Name.Should().Be(Plugin.PluginName);
        movieExternalIdProvider.Key.Should().Be(Plugin.PluginKey);
        movieExternalIdProvider.UrlFormatString.Should().Be("https://www.kinopoisk.ru/film/{0}/");

        movieExternalIdProvider.Supports(new Movie()).Should().BeTrue();
        movieExternalIdProvider.Supports(new Series()).Should().BeFalse();

        _logger.Info($"Finished '{nameof(MovieExternalIdProvider_ForCodeCoverage)}'");
    }
}
