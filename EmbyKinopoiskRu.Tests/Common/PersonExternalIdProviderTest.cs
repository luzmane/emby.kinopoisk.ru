using EmbyKinopoiskRu.Provider.ExternalId;

using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.TV;

using NLog;

namespace EmbyKinopoiskRu.Tests.Common;

public class PersonExternalIdProviderTest
{
    private readonly ILogger _logger = LogManager.GetLogger(nameof(PersonExternalIdProviderTest));

    [Fact]
    public void PersonExternalIdProvider_ForCodeCoverage()
    {
        _logger.Info($"Start '{nameof(PersonExternalIdProvider_ForCodeCoverage)}'");

        PersonExternalIdProvider personExternalIdProvider = new();

        Assert.Equal(Plugin.PluginName, personExternalIdProvider.Name);
        Assert.Equal(Plugin.PluginKey, personExternalIdProvider.Key);
        Assert.Equal("https://www.kinopoisk.ru/name/{0}/", personExternalIdProvider.UrlFormatString);

        Assert.True(personExternalIdProvider.Supports(new Person()));
        Assert.False(personExternalIdProvider.Supports(new Series()));

        _logger.Info($"Finished '{nameof(PersonExternalIdProvider_ForCodeCoverage)}'");
    }

}
