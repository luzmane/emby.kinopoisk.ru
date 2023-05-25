using EmbyKinopoiskRu.Provider.ExternalId;

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

        Assert.Equal(Plugin.PluginName, movieExternalIdProvider.Name);
        Assert.Equal(Plugin.PluginKey, movieExternalIdProvider.Key);
        Assert.Equal("https://www.kinopoisk.ru/film/{0}/", movieExternalIdProvider.UrlFormatString);

        Assert.True(movieExternalIdProvider.Supports(new Movie()));
        Assert.False(movieExternalIdProvider.Supports(new Series()));

        _logger.Info($"Finished '{nameof(MovieExternalIdProvider_ForCodeCoverage)}'");
    }

}
