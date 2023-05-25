using System.Net;

using EmbyKinopoiskRu.Configuration;
using EmbyKinopoiskRu.Provider.RemoteImage;

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
        _pluginConfiguration.Token = KINOPOISK_UNOFFICIAL_TOKEN;
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

        Assert.True(3 == imageInfos.Count());

        RemoteImageInfo? primary = imageInfos.FirstOrDefault(i => i.Type == ImageType.Primary);
        Assert.NotNull(primary);
        Assert.Equal("RU", primary.DisplayLanguage);
        Assert.Equal("ru", primary.Language);
        Assert.Equal("KinopoiskRu", primary.ProviderName);
        Assert.False(string.IsNullOrWhiteSpace(primary.Url));
        Assert.False(string.IsNullOrWhiteSpace(primary.ThumbnailUrl));

        RemoteImageInfo? backdrop = imageInfos.FirstOrDefault(i => i.Type == ImageType.Backdrop);
        Assert.NotNull(backdrop);
        Assert.Equal("RU", backdrop.DisplayLanguage);
        Assert.Equal("ru", backdrop.Language);
        Assert.Equal("KinopoiskRu", backdrop.ProviderName);
        Assert.False(string.IsNullOrWhiteSpace(backdrop.Url));
        Assert.True(string.IsNullOrWhiteSpace(backdrop.ThumbnailUrl));

        RemoteImageInfo? logo = imageInfos.FirstOrDefault(i => i.Type == ImageType.Logo);
        Assert.NotNull(logo);
        Assert.Equal("RU", logo.DisplayLanguage);
        Assert.Equal("ru", logo.Language);
        Assert.Equal("KinopoiskRu", logo.ProviderName);
        Assert.False(string.IsNullOrWhiteSpace(logo.Url));

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

        Assert.True(3 == imageInfos.Count());

        RemoteImageInfo? primary = imageInfos.FirstOrDefault(i => i.Type == ImageType.Primary);
        Assert.NotNull(primary);
        Assert.Equal("RU", primary.DisplayLanguage);
        Assert.Equal("ru", primary.Language);
        Assert.Equal("KinopoiskRu", primary.ProviderName);
        Assert.False(string.IsNullOrWhiteSpace(primary.Url));
        Assert.False(string.IsNullOrWhiteSpace(primary.ThumbnailUrl));

        RemoteImageInfo? backdrop = imageInfos.FirstOrDefault(i => i.Type == ImageType.Backdrop);
        Assert.NotNull(backdrop);
        Assert.Equal("RU", backdrop.DisplayLanguage);
        Assert.Equal("ru", backdrop.Language);
        Assert.Equal("KinopoiskRu", backdrop.ProviderName);
        Assert.False(string.IsNullOrWhiteSpace(backdrop.Url));
        Assert.True(string.IsNullOrWhiteSpace(backdrop.ThumbnailUrl));

        RemoteImageInfo? logo = imageInfos.FirstOrDefault(i => i.Type == ImageType.Logo);
        Assert.NotNull(logo);
        Assert.Equal("RU", logo.DisplayLanguage);
        Assert.Equal("ru", logo.Language);
        Assert.Equal("KinopoiskRu", logo.ProviderName);
        Assert.False(string.IsNullOrWhiteSpace(logo.Url));

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

        Assert.True(3 == imageInfos.Count());

        RemoteImageInfo? primary = imageInfos.FirstOrDefault(i => i.Type == ImageType.Primary);
        Assert.NotNull(primary);
        Assert.Equal("RU", primary.DisplayLanguage);
        Assert.Equal("ru", primary.Language);
        Assert.Equal("KinopoiskRu", primary.ProviderName);
        Assert.False(string.IsNullOrWhiteSpace(primary.Url));
        Assert.False(string.IsNullOrWhiteSpace(primary.ThumbnailUrl));

        RemoteImageInfo? backdrop = imageInfos.FirstOrDefault(i => i.Type == ImageType.Backdrop);
        Assert.NotNull(backdrop);
        Assert.Equal("RU", backdrop.DisplayLanguage);
        Assert.Equal("ru", backdrop.Language);
        Assert.Equal("KinopoiskRu", backdrop.ProviderName);
        Assert.False(string.IsNullOrWhiteSpace(backdrop.Url));
        Assert.True(string.IsNullOrWhiteSpace(backdrop.ThumbnailUrl));

        RemoteImageInfo? logo = imageInfos.FirstOrDefault(i => i.Type == ImageType.Logo);
        Assert.NotNull(logo);
        Assert.Equal("RU", logo.DisplayLanguage);
        Assert.Equal("ru", logo.Language);
        Assert.Equal("KinopoiskRu", logo.ProviderName);
        Assert.False(string.IsNullOrWhiteSpace(logo.Url));

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

        Assert.Empty(imageInfos);

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

        Assert.True(_kpImageProvider.Supports(new Movie()));
        Assert.True(_kpImageProvider.Supports(new Series()));
        Assert.False(_kpImageProvider.Supports(new Audio()));

        IEnumerable<ImageType> supportedImages = _kpImageProvider.GetSupportedImages(new Movie());
        Assert.True(supportedImages.Count() == 3);

        HttpResponseInfo response = await _kpImageProvider.GetImageResponse("https://www.google.com", CancellationToken.None);
        Assert.True(response.StatusCode == HttpStatusCode.OK);

        Assert.NotNull(_kpImageProvider.Name);

        _logManager.Verify(lm => lm.GetLogger("KinopoiskRu"), Times.Once());
        VerifyNoOtherCalls();

        Logger.Info($"Finished '{nameof(KpImageProviderTest_ForCodeCoverage)}'");
    }

}
