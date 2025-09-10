using System.Net;

using EmbyKinopoiskRu.Configuration;
using EmbyKinopoiskRu.Provider.RemoteImage;

using FluentAssertions;

using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Entities.Audio;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Providers;

namespace EmbyKinopoiskRu.Tests.KinopoiskDev;

public class KpImageProviderTest : BaseTest
{
    private static readonly NLog.Logger Logger = NLog.LogManager.GetLogger(nameof(KpImageProviderTest));

    private readonly KpImageProvider _kpImageProvider;

    public KpImageProviderTest() : base(Logger)
    {
        _pluginConfiguration.Token = GetKinopoiskDevToken();

        ConfigLibraryManager();

        _kpImageProvider = new KpImageProvider(_httpClient);
    }

    [Fact]
    public async Task KpImageProviderTest_GetImages_ByProviderId()
    {
        Logger.Info($"Started '{nameof(KpImageProviderTest_GetImages_ByProviderId)}'");

        using var cancellationTokenSource = new CancellationTokenSource();
        Movie item = new()
        {
            Name = "Побег из Шоушенка",
            OriginalTitle = "The Shawshank Redemption",
            ProductionYear = 1994,
            ProviderIds = new ProviderIdDictionary(new Dictionary<string, string>
            {
                { Plugin.PluginKey, "326" }
            })
        };
        LibraryOptions options = new();

        _ = _applicationPaths
            .SetupGet(m => m.PluginConfigurationsPath)
            .Returns(nameof(KpImageProviderTest_GetImages_ByProviderId));

        IEnumerable<RemoteImageInfo> imageInfos = await _kpImageProvider.GetImages(item, options, cancellationTokenSource.Token);

        imageInfos.Should().HaveCount(3);

        RemoteImageInfo? primary = imageInfos.FirstOrDefault(i => i.Type == ImageType.Primary);
        primary.Should().NotBeNull();
        primary!.DisplayLanguage.Should().Be("RU");
        primary.Language.Should().Be("ru");
        primary.ProviderName.Should().Be("KinopoiskRu");
        primary.Url.Should().NotBeNullOrWhiteSpace();
        primary.ThumbnailUrl.Should().NotBeNullOrWhiteSpace();

        RemoteImageInfo? backdrop = imageInfos.FirstOrDefault(i => i.Type == ImageType.Backdrop);
        backdrop.Should().NotBeNull();
        backdrop!.DisplayLanguage.Should().Be("RU");
        backdrop.Language.Should().Be("ru");
        backdrop.ProviderName.Should().Be("KinopoiskRu");
        backdrop.Url.Should().NotBeNullOrWhiteSpace();
        backdrop.ThumbnailUrl.Should().NotBeNullOrWhiteSpace();

        RemoteImageInfo? logo = imageInfos.FirstOrDefault(i => i.Type == ImageType.Logo);
        logo.Should().NotBeNull("that mean the logo was found");
        logo!.DisplayLanguage.Should().Be("RU");
        logo.Language.Should().Be("ru");
        logo.ProviderName.Should().Be("KinopoiskRu");
        logo.Url.Should().NotBeNullOrWhiteSpace();

        _logManager.Verify(lm => lm.GetLogger(It.IsAny<string>()), Times.Exactly(3));
        _applicationPaths.VerifyGet(ap => ap.PluginConfigurationsPath, Times.Once());
        _xmlSerializer.Verify(xs => xs.DeserializeFromFile(typeof(PluginConfiguration), $"{nameof(KpImageProviderTest_GetImages_ByProviderId)}/EmbyKinopoiskRu.xml"), Times.Once());
        VerifyNoOtherCalls();

        Logger.Info($"Finished '{nameof(KpImageProviderTest_GetImages_ByProviderId)}'");
    }

    [Fact]
    public async Task KpImageProviderTest_GetImages_ByOriginalTitle()
    {
        Logger.Info($"Started '{nameof(KpImageProviderTest_GetImages_ByOriginalTitle)}'");

        using var cancellationTokenSource = new CancellationTokenSource();
        Movie item = new()
        {
            Name = string.Empty,
            OriginalTitle = "The Shawshank Redemption"
        };
        LibraryOptions options = new();

        _ = _applicationPaths
            .SetupGet(m => m.PluginConfigurationsPath)
            .Returns(nameof(KpImageProviderTest_GetImages_ByOriginalTitle));

        IEnumerable<RemoteImageInfo> imageInfos = await _kpImageProvider.GetImages(item, options, cancellationTokenSource.Token);

        imageInfos.Should().HaveCount(3);

        RemoteImageInfo? primary = imageInfos.FirstOrDefault(i => i.Type == ImageType.Primary);
        primary.Should().NotBeNull();
        primary!.DisplayLanguage.Should().Be("RU");
        primary.Language.Should().Be("ru");
        primary.ProviderName.Should().Be("KinopoiskRu");
        primary.Url.Should().NotBeNullOrWhiteSpace();
        primary.ThumbnailUrl.Should().NotBeNullOrWhiteSpace();

        RemoteImageInfo? backdrop = imageInfos.FirstOrDefault(i => i.Type == ImageType.Backdrop);
        backdrop.Should().NotBeNull();
        backdrop!.DisplayLanguage.Should().Be("RU");
        backdrop.Language.Should().Be("ru");
        backdrop.ProviderName.Should().Be("KinopoiskRu");
        backdrop.Url.Should().NotBeNullOrWhiteSpace();
        backdrop.ThumbnailUrl.Should().NotBeNullOrWhiteSpace();

        RemoteImageInfo? logo = imageInfos.FirstOrDefault(i => i.Type == ImageType.Logo);
        logo.Should().NotBeNull("that mean the logo was found");
        logo!.DisplayLanguage.Should().Be("RU");
        logo.Language.Should().Be("ru");
        logo.ProviderName.Should().Be("KinopoiskRu");
        logo.Url.Should().NotBeNullOrWhiteSpace();

        _logManager.Verify(lm => lm.GetLogger(It.IsAny<string>()), Times.Exactly(3));
        _applicationPaths.VerifyGet(ap => ap.PluginConfigurationsPath, Times.Once());
        _xmlSerializer.Verify(xs => xs.DeserializeFromFile(typeof(PluginConfiguration), $"{nameof(KpImageProviderTest_GetImages_ByOriginalTitle)}/EmbyKinopoiskRu.xml"), Times.Once());
        VerifyNoOtherCalls();

        Logger.Info($"Finished '{nameof(KpImageProviderTest_GetImages_ByOriginalTitle)}'");
    }

    [Fact]
    public async Task KpImageProviderTest_GetImages_ByOriginalTitleAndYear()
    {
        Logger.Info($"Started '{nameof(KpImageProviderTest_GetImages_ByOriginalTitleAndYear)}'");

        using var cancellationTokenSource = new CancellationTokenSource();
        Movie item = new()
        {
            Name = string.Empty,
            OriginalTitle = "The Shawshank Redemption",
            ProductionYear = 1994
        };
        LibraryOptions options = new();

        _ = _applicationPaths
            .SetupGet(m => m.PluginConfigurationsPath)
            .Returns(nameof(KpImageProviderTest_GetImages_ByOriginalTitleAndYear));

        IEnumerable<RemoteImageInfo> imageInfos = await _kpImageProvider.GetImages(item, options, cancellationTokenSource.Token);

        imageInfos.Should().HaveCount(3);

        RemoteImageInfo? primary = imageInfos.FirstOrDefault(i => i.Type == ImageType.Primary);
        primary.Should().NotBeNull();
        primary!.DisplayLanguage.Should().Be("RU");
        primary.Language.Should().Be("ru");
        primary.ProviderName.Should().Be("KinopoiskRu");
        primary.Url.Should().NotBeNullOrWhiteSpace();
        primary.ThumbnailUrl.Should().NotBeNullOrWhiteSpace();

        RemoteImageInfo? backdrop = imageInfos.FirstOrDefault(i => i.Type == ImageType.Backdrop);
        backdrop.Should().NotBeNull();
        backdrop!.DisplayLanguage.Should().Be("RU");
        backdrop.Language.Should().Be("ru");
        backdrop.ProviderName.Should().Be("KinopoiskRu");
        backdrop.Url.Should().NotBeNullOrWhiteSpace();
        backdrop.ThumbnailUrl.Should().NotBeNullOrWhiteSpace();

        RemoteImageInfo? logo = imageInfos.FirstOrDefault(i => i.Type == ImageType.Logo);
        logo.Should().NotBeNull("that mean the logo was found");
        logo!.DisplayLanguage.Should().Be("RU");
        logo.Language.Should().Be("ru");
        logo.ProviderName.Should().Be("KinopoiskRu");
        logo.Url.Should().NotBeNullOrWhiteSpace();

        _logManager.Verify(lm => lm.GetLogger(It.IsAny<string>()), Times.Exactly(3));
        _applicationPaths.VerifyGet(ap => ap.PluginConfigurationsPath, Times.Once());
        _xmlSerializer.Verify(xs => xs.DeserializeFromFile(typeof(PluginConfiguration), $"{nameof(KpImageProviderTest_GetImages_ByOriginalTitleAndYear)}/EmbyKinopoiskRu.xml"), Times.Once());
        VerifyNoOtherCalls();

        Logger.Info($"Finished '{nameof(KpImageProviderTest_GetImages_ByOriginalTitleAndYear)}'");
    }

    [Fact]
    public async Task KpImageProviderTest_GetImages_NothingFound()
    {
        Logger.Info($"Started '{nameof(KpImageProviderTest_GetImages_NothingFound)}'");

        using var cancellationTokenSource = new CancellationTokenSource();
        Movie item = new()
        {
            Name = string.Empty,
            OriginalTitle = "Shawshank Redemption"
        };
        LibraryOptions options = new();

        _ = _applicationPaths
            .SetupGet(m => m.PluginConfigurationsPath)
            .Returns(nameof(KpImageProviderTest_GetImages_NothingFound));

        IEnumerable<RemoteImageInfo> imageInfos = await _kpImageProvider.GetImages(item, options, cancellationTokenSource.Token);

        imageInfos.Should().BeEmpty();

        _logManager.Verify(lm => lm.GetLogger(It.IsAny<string>()), Times.Exactly(3));
        _applicationPaths.VerifyGet(ap => ap.PluginConfigurationsPath, Times.Once());
        _xmlSerializer.Verify(xs => xs.DeserializeFromFile(typeof(PluginConfiguration), $"{nameof(KpImageProviderTest_GetImages_NothingFound)}/EmbyKinopoiskRu.xml"), Times.Once());
        VerifyNoOtherCalls();

        Logger.Info($"Finished '{nameof(KpImageProviderTest_GetImages_NothingFound)}'");
    }

    [Fact]
    public async Task KpImageProviderTest_ForCodeCoverage()
    {
        Logger.Info($"Started '{nameof(KpImageProviderTest_ForCodeCoverage)}'");

        _kpImageProvider.Supports(new Movie()).Should().BeTrue();
        _kpImageProvider.Supports(new Series()).Should().BeTrue();
        _kpImageProvider.Supports(new Audio()).Should().BeFalse();

        _kpImageProvider.GetSupportedImages(new Movie()).Should().HaveCount(3, "that plugin config");

        HttpResponseInfo response = await _kpImageProvider.GetImageResponse("https://www.google.com", CancellationToken.None);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        _kpImageProvider.Name.Should().Be(Plugin.PluginName, "provider config");

        _logManager.Verify(lm => lm.GetLogger("KinopoiskRu"), Times.Once());
        VerifyNoOtherCalls();

        Logger.Info($"Finished '{nameof(KpImageProviderTest_ForCodeCoverage)}'");
    }
}
