using EmbyKinopoiskRu.Provider.ExternalId;

using FluentAssertions;

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

        personExternalIdProvider.Name.Should().Be(Plugin.PluginName);
        personExternalIdProvider.Key.Should().Be(Plugin.PluginKey);
        personExternalIdProvider.UrlFormatString.Should().Be("https://www.kinopoisk.ru/name/{0}/");

        personExternalIdProvider.Supports(new Person()).Should().BeTrue();
        personExternalIdProvider.Supports(new Series()).Should().BeFalse();

        _logger.Info($"Finished '{nameof(PersonExternalIdProvider_ForCodeCoverage)}'");
    }

}
