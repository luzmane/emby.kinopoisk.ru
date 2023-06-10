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

namespace EmbyKinopoiskRu.Tests.KinopoiskApiUnofficial;

[Collection("Sequential")]
public class KpImageProviderTest : BaseTest
{
    private static readonly NLog.ILogger Logger = NLog.LogManager.GetLogger(nameof(KpImageProviderTest));

    private readonly KpImageProvider _kpImageProvider;

    public KpImageProviderTest() : base(Logger)
    {
        _pluginConfiguration.Token = GetKinopoiskUnofficialToken();
        _pluginConfiguration.ApiType = PluginConfiguration.KinopoiskAPIUnofficialTech;

        ConfigLibraryManager();

        ConfigXmlSerializer();

        _kpImageProvider = new KpImageProvider(_httpClient);
    }

    [Fact]
    public async void KpImageProviderTest_GetImages_ByProviderId()
    {
        Logger.Info($"Started '{nameof(KpImageProviderTest_GetImages_ByProviderId)}'");

        using var cancellationTokenSource = new CancellationTokenSource();
        Movie item = new()
        {
            Name = "Побег из Шоушенка",
            OriginalTitle = "The Shawshank Redemption",
            ProductionYear = 1994,
            ProviderIds = new(new() { { Plugin.PluginKey, "326" } })
        };
        LibraryOptions options = new();

        _ = _applicationPaths
            .SetupGet(m => m.PluginConfigurationsPath)
            .Returns("UN_KpImageProviderTest_GetImages_ByProviderId");

        IEnumerable<RemoteImageInfo> imageInfos = await _kpImageProvider.GetImages(item, options, cancellationTokenSource.Token);

        imageInfos.Should().HaveCount(3);

        RemoteImageInfo? primary = imageInfos.FirstOrDefault(i => i.Type == ImageType.Primary);
        primary.Should().NotBeNull("that mean the primary image was found");
        primary!.DisplayLanguage.Should().Be("RU", "config of the library");
        primary.Language.Should().Be("ru", "config of the library");
        primary.ProviderName.Should().Be("KinopoiskRu", "this is used provider");
        primary.Url.Should().NotBeNullOrWhiteSpace("Url should not be empty");
        primary.ThumbnailUrl.Should().NotBeNullOrWhiteSpace("ThumbnailUrl should not be empty");

        RemoteImageInfo? backdrop = imageInfos.FirstOrDefault(i => i.Type == ImageType.Backdrop);
        backdrop.Should().NotBeNull("that mean the backdrop image was found");
        backdrop!.DisplayLanguage.Should().Be("RU", "config of the library");
        backdrop.Language.Should().Be("ru", "config of the library");
        backdrop.ProviderName.Should().Be("KinopoiskRu", "this is used provider");
        backdrop.Url.Should().NotBeNullOrWhiteSpace("Url should not be empty");
        backdrop.ThumbnailUrl.Should().BeNullOrWhiteSpace("ThumbnailUrl should be empty in that case");

        RemoteImageInfo? logo = imageInfos.FirstOrDefault(i => i.Type == ImageType.Logo);
        logo.Should().NotBeNull("that mean the logo was found");
        logo!.DisplayLanguage.Should().Be("RU", "config of the library");
        logo.Language.Should().Be("ru", "config of the library");
        logo.ProviderName.Should().Be("KinopoiskRu", "this is used provider");
        logo.Url.Should().NotBeNullOrWhiteSpace("Url should not be empty");

        _logManager.Verify(lm => lm.GetLogger(It.IsAny<string>()), Times.Exactly(3));
        _applicationPaths.VerifyGet(ap => ap.PluginConfigurationsPath, Times.Once());
        _xmlSerializer.Verify(xs => xs.DeserializeFromFile(typeof(PluginConfiguration), "UN_KpImageProviderTest_GetImages_ByProviderId/EmbyKinopoiskRu.xml"), Times.Once());
        VerifyNoOtherCalls();

        Logger.Info($"Finished '{nameof(KpImageProviderTest_GetImages_ByProviderId)}'");
    }

    [Fact]
    public async void KpImageProviderTest_GetImages_ByName()
    {
        Logger.Info($"Started '{nameof(KpImageProviderTest_GetImages_ByName)}'");

        using var cancellationTokenSource = new CancellationTokenSource();
        Movie item = new()
        {
            Name = "Побег из Шоушенка",
        };
        LibraryOptions options = new();

        _ = _applicationPaths
            .SetupGet(m => m.PluginConfigurationsPath)
            .Returns("UN_KpImageProviderTest_GetImages_ByName");

        IEnumerable<RemoteImageInfo> imageInfos = await _kpImageProvider.GetImages(item, options, cancellationTokenSource.Token);

        imageInfos.Should().HaveCount(3);

        RemoteImageInfo? primary = imageInfos.FirstOrDefault(i => i.Type == ImageType.Primary);
        primary.Should().NotBeNull("that mean the primary image was found");
        primary!.DisplayLanguage.Should().Be("RU", "config of the library");
        primary.Language.Should().Be("ru", "config of the library");
        primary.ProviderName.Should().Be("KinopoiskRu", "this is used provider");
        primary.Url.Should().NotBeNullOrWhiteSpace("Url should not be empty");
        primary.ThumbnailUrl.Should().NotBeNullOrWhiteSpace("ThumbnailUrl should not be empty");

        RemoteImageInfo? backdrop = imageInfos.FirstOrDefault(i => i.Type == ImageType.Backdrop);
        backdrop.Should().NotBeNull("that mean the backdrop image was found");
        backdrop!.DisplayLanguage.Should().Be("RU", "config of the library");
        backdrop.Language.Should().Be("ru", "config of the library");
        backdrop.ProviderName.Should().Be("KinopoiskRu", "this is used provider");
        backdrop.Url.Should().NotBeNullOrWhiteSpace("Url should not be empty");
        backdrop.ThumbnailUrl.Should().BeNullOrWhiteSpace("ThumbnailUrl should be empty in that case");

        RemoteImageInfo? logo = imageInfos.FirstOrDefault(i => i.Type == ImageType.Logo);
        logo.Should().NotBeNull("that mean the logo was found");
        logo!.DisplayLanguage.Should().Be("RU", "config of the library");
        logo.Language.Should().Be("ru", "config of the library");
        logo.ProviderName.Should().Be("KinopoiskRu", "this is used provider");
        logo.Url.Should().NotBeNullOrWhiteSpace("Url should not be empty");

        _logManager.Verify(lm => lm.GetLogger(It.IsAny<string>()), Times.Exactly(3));
        _applicationPaths.VerifyGet(ap => ap.PluginConfigurationsPath, Times.Once());
        _xmlSerializer.Verify(xs => xs.DeserializeFromFile(typeof(PluginConfiguration), "UN_KpImageProviderTest_GetImages_ByName/EmbyKinopoiskRu.xml"), Times.Once());
        VerifyNoOtherCalls();

        Logger.Info($"Finished '{nameof(KpImageProviderTest_GetImages_ByName)}'");
    }

    [Fact]
    public async void KpImageProviderTest_GetImages_ByNameAndYear()
    {
        Logger.Info($"Started '{nameof(KpImageProviderTest_GetImages_ByNameAndYear)}'");

        using var cancellationTokenSource = new CancellationTokenSource();
        Movie item = new()
        {
            Name = "Побег из Шоушенка",
            ProductionYear = 1994,
        };
        LibraryOptions options = new();

        _ = _applicationPaths
            .SetupGet(m => m.PluginConfigurationsPath)
            .Returns("UN_KpImageProviderTest_GetImages_ByNameAndYear");

        IEnumerable<RemoteImageInfo> imageInfos = await _kpImageProvider.GetImages(item, options, cancellationTokenSource.Token);

        imageInfos.Should().HaveCount(3);

        RemoteImageInfo? primary = imageInfos.FirstOrDefault(i => i.Type == ImageType.Primary);
        primary.Should().NotBeNull("that mean the primary image was found");
        primary!.DisplayLanguage.Should().Be("RU", "config of the library");
        primary.Language.Should().Be("ru", "config of the library");
        primary.ProviderName.Should().Be("KinopoiskRu", "this is used provider");
        primary.Url.Should().NotBeNullOrWhiteSpace("Url should not be empty");
        primary.ThumbnailUrl.Should().NotBeNullOrWhiteSpace("ThumbnailUrl should not be empty");

        RemoteImageInfo? backdrop = imageInfos.FirstOrDefault(i => i.Type == ImageType.Backdrop);
        backdrop.Should().NotBeNull("that mean the backdrop image was found");
        backdrop!.DisplayLanguage.Should().Be("RU", "config of the library");
        backdrop.Language.Should().Be("ru", "config of the library");
        backdrop.ProviderName.Should().Be("KinopoiskRu", "this is used provider");
        backdrop.Url.Should().NotBeNullOrWhiteSpace("Url should not be empty");
        backdrop.ThumbnailUrl.Should().BeNullOrWhiteSpace("ThumbnailUrl should be empty in that case");

        RemoteImageInfo? logo = imageInfos.FirstOrDefault(i => i.Type == ImageType.Logo);
        logo.Should().NotBeNull("that mean the logo was found");
        logo!.DisplayLanguage.Should().Be("RU", "config of the library");
        logo.Language.Should().Be("ru", "config of the library");
        logo.ProviderName.Should().Be("KinopoiskRu", "this is used provider");
        logo.Url.Should().NotBeNullOrWhiteSpace("Url should not be empty");

        _logManager.Verify(lm => lm.GetLogger(It.IsAny<string>()), Times.Exactly(3));
        _applicationPaths.VerifyGet(ap => ap.PluginConfigurationsPath, Times.Once());
        _xmlSerializer.Verify(xs => xs.DeserializeFromFile(typeof(PluginConfiguration), "UN_KpImageProviderTest_GetImages_ByNameAndYear/EmbyKinopoiskRu.xml"), Times.Once());
        VerifyNoOtherCalls();

        Logger.Info($"Finished '{nameof(KpImageProviderTest_GetImages_ByNameAndYear)}'");
    }

    [Fact]
    public async void KpImageProviderTest_GetImages_NothingFound()
    {
        Logger.Info($"Started '{nameof(KpImageProviderTest_GetImages_NothingFound)}'");

        using var cancellationTokenSource = new CancellationTokenSource();
        Movie item = new()
        {
            Name = "",
        };
        LibraryOptions options = new();

        _ = _applicationPaths
            .SetupGet(m => m.PluginConfigurationsPath)
            .Returns("UN_KpImageProviderTest_GetImages_NothingFound");

        IEnumerable<RemoteImageInfo> imageInfos = await _kpImageProvider.GetImages(item, options, cancellationTokenSource.Token);

        imageInfos.Should().BeEmpty("expecting no image will be found");

        _logManager.Verify(lm => lm.GetLogger(It.IsAny<string>()), Times.Exactly(3));
        _applicationPaths.VerifyGet(ap => ap.PluginConfigurationsPath, Times.Once());
        _xmlSerializer.Verify(xs => xs.DeserializeFromFile(typeof(PluginConfiguration), "UN_KpImageProviderTest_GetImages_NothingFound/EmbyKinopoiskRu.xml"), Times.Once());
        VerifyNoOtherCalls();

        Logger.Info($"Finished '{nameof(KpImageProviderTest_GetImages_NothingFound)}'");
    }

    [Fact]
    public async void KpImageProviderTest_ForCodeCoverage()
    {
        Logger.Info($"Started '{nameof(KpImageProviderTest_ForCodeCoverage)}'");

        _kpImageProvider.Supports(new Movie()).Should().BeTrue("this provider supports only Movie or Series");
        _kpImageProvider.Supports(new Series()).Should().BeTrue("this provider supports only Movie or Series");
        _kpImageProvider.Supports(new Audio()).Should().BeFalse("this provider supports only Movie or Series");

        _kpImageProvider.GetSupportedImages(new Movie()).Should().HaveCount(3, "that plugin config");

        HttpResponseInfo response = await _kpImageProvider.GetImageResponse("https://www.google.com", CancellationToken.None);
        response.StatusCode.Should().Be(HttpStatusCode.OK, "this is status code of the response to google.com");

        _kpImageProvider.Name.Should().Be(Plugin.PluginName, "provider config");

        _logManager.Verify(lm => lm.GetLogger("KinopoiskRu"), Times.Once());
        VerifyNoOtherCalls();

        Logger.Info($"Finished '{nameof(KpImageProviderTest_ForCodeCoverage)}'");
    }

}
