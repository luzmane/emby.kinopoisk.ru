using System.Net;

using EmbyKinopoiskRu.Configuration;
using EmbyKinopoiskRu.Provider.RemoteMetadata;

using FluentAssertions;

using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;

namespace EmbyKinopoiskRu.Tests.KinopoiskApiUnofficial;

[Collection("Sequential")]
public class KpEpisodeProviderTest : BaseTest
{
    private static readonly NLog.ILogger Logger = NLog.LogManager.GetLogger(nameof(KpEpisodeProviderTest));

    private readonly KpEpisodeProvider _kpEpisodeProvider;


    #region Test configs
    public KpEpisodeProviderTest() : base(Logger)
    {
        _pluginConfiguration.Token = GetKinopoiskUnofficialToken();
        _pluginConfiguration.ApiType = PluginConfiguration.KinopoiskAPIUnofficialTech;

        ConfigLibraryManager();

        ConfigXmlSerializer();

        _kpEpisodeProvider = new(_httpClient, _logManager.Object);
    }
    protected override void ConfigLibraryManager()
    {
        base.ConfigLibraryManager();
    }
    protected override void ConfigXmlSerializer()
    {
        base.ConfigXmlSerializer();
    }

    #endregion

    [Fact]
    public async void KpEpisodeProvider_ForCodeCoverage()
    {
        Logger.Info($"Start '{nameof(KpEpisodeProvider_ForCodeCoverage)}'");

        _kpEpisodeProvider.Name.Should().NotBeNullOrWhiteSpace("this is a name of the provider");

        HttpResponseInfo response = await _kpEpisodeProvider.GetImageResponse("https://www.google.com", CancellationToken.None);
        response.StatusCode.Should().Be(HttpStatusCode.OK, "this is status code of the response to google.com");

        _logManager.Verify(lm => lm.GetLogger("KpEpisodeProvider"), Times.Once());
        _logManager.Verify(lm => lm.GetLogger("KinopoiskRu"), Times.Once());

        VerifyNoOtherCalls();

        Logger.Info($"Finished '{nameof(KpEpisodeProvider_ForCodeCoverage)}'");
    }

    [Fact]
    public async void KpEpisodeProvider_GetMetadata_ProviderIds()
    {
        Logger.Info($"Start '{nameof(KpEpisodeProvider_GetMetadata_ProviderIds)}'");

        var episodeInfo = new EpisodeInfo()
        {
            SeriesProviderIds = null,
            IndexNumber = 2,
            ParentIndexNumber = 1,
            ProviderIds = new(new() { { Plugin.PluginKey, "452973" } })
        };

        _ = _applicationPaths
            .SetupGet(m => m.PluginConfigurationsPath)
            .Returns("UN_KpEpisodeProvider_GetMetadata_ProviderIds");

        using var cancellationTokenSource = new CancellationTokenSource();
        MetadataResult<Episode> result = await _kpEpisodeProvider.GetMetadata(episodeInfo, cancellationTokenSource.Token);

        result.HasMetadata.Should().BeTrue("that mean the item was found");
        Episode episode = result.Item;
        episode.Should().NotBeNull("that mean the episode was found");
        episode.IndexNumber.Should().Be(2, "requested second episode");
        episode.MediaType.Should().Be("Video", "this is video");
        episode.Name.Should().Be("А я сказал — оседлаю!", "this is the name of the episode");
        episode.OriginalTitle.Should().Be("I Said I'm Gonna Pilot That Thing!!", "this is the original name of the episode");
        episode.ParentIndexNumber.Should().Be(1, "requested first season");
        episode.PremiereDate.Should().NotBeNull("episode premier date should have a date");
        episode.PremiereDate!.Value.DateTime.Should().HaveYear(2007).And.HaveMonth(4).And.HaveDay(8);
        episode.SortName.Should().Be(episode.Name, "Emby sorts episode by name");

        _logManager.Verify(lm => lm.GetLogger(It.IsAny<string>()), Times.Exactly(4));
        _applicationPaths.VerifyGet(ap => ap.PluginConfigurationsPath, Times.Once());
        _xmlSerializer.Verify(xs => xs.DeserializeFromFile(typeof(PluginConfiguration), "UN_KpEpisodeProvider_GetMetadata_ProviderIds/EmbyKinopoiskRu.xml"), Times.Once());
        _localizationManager.Verify(lm => lm.RemoveDiacritics("А я сказал — оседлаю!"), Times.Once());
        _serverConfigurationManager.VerifyGet(scm => scm.Configuration, Times.Once());
        VerifyNoOtherCalls();

        Logger.Info($"Finish '{nameof(KpEpisodeProvider_GetMetadata_ProviderIds)}'");
    }

    [Fact]
    public async void KpEpisodeProvider_GetMetadata_SeriesProviderIds()
    {
        Logger.Info($"Start '{nameof(KpEpisodeProvider_GetMetadata_SeriesProviderIds)}'");

        var episodeInfo = new EpisodeInfo()
        {
            IndexNumber = 2,
            ParentIndexNumber = 1,
            SeriesProviderIds = new() { { Plugin.PluginKey, "452973" } }
        };

        _ = _applicationPaths
            .SetupGet(m => m.PluginConfigurationsPath)
            .Returns("UN_KpEpisodeProvider_GetMetadata_SeriesProviderIds");

        using var cancellationTokenSource = new CancellationTokenSource();
        MetadataResult<Episode> result = await _kpEpisodeProvider.GetMetadata(episodeInfo, cancellationTokenSource.Token);

        result.HasMetadata.Should().BeTrue("that mean the item was found");
        Episode episode = result.Item;
        episode.Should().NotBeNull("that mean the episode was found");
        episode.IndexNumber.Should().Be(2, "requested second episode");
        episode.MediaType.Should().Be("Video", "this is video");
        episode.Name.Should().Be("А я сказал — оседлаю!", "this is the name of the episode");
        episode.OriginalTitle.Should().Be("I Said I'm Gonna Pilot That Thing!!", "this is the original name of the episode");
        episode.ParentIndexNumber.Should().Be(1, "requested first season");
        episode.PremiereDate.Should().NotBeNull("episode premier date should have a date");
        episode.PremiereDate!.Value.DateTime.Should().HaveYear(2007).And.HaveMonth(4).And.HaveDay(8);
        episode.SortName.Should().Be(episode.Name, "SortName should be equal to Name");

        _logManager.Verify(lm => lm.GetLogger(It.IsAny<string>()), Times.Exactly(4));
        _applicationPaths.VerifyGet(ap => ap.PluginConfigurationsPath, Times.Once());
        _xmlSerializer.Verify(xs => xs.DeserializeFromFile(typeof(PluginConfiguration), "UN_KpEpisodeProvider_GetMetadata_SeriesProviderIds/EmbyKinopoiskRu.xml"), Times.Once());
        _localizationManager.Verify(lm => lm.RemoveDiacritics("А я сказал — оседлаю!"), Times.Once());
        _serverConfigurationManager.VerifyGet(scm => scm.Configuration, Times.Once());
        VerifyNoOtherCalls();

        Logger.Info($"Finish '{nameof(KpEpisodeProvider_GetMetadata_SeriesProviderIds)}'");
    }

}
