using System.Globalization;
using System.Net;

using EmbyKinopoiskRu.Configuration;
using EmbyKinopoiskRu.Provider.RemoteMetadata;

using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;

namespace EmbyKinopoiskRu.Tests.KinopoiskDev;

[Collection("Sequential")]
public class KpEpisodeProviderTest : BaseTest
{
    private static readonly NLog.ILogger Logger = NLog.LogManager.GetLogger(nameof(KpEpisodeProviderTest));

    private readonly KpEpisodeProvider _kpEpisodeProvider;


    #region Test configs
    public KpEpisodeProviderTest() : base(Logger)
    {
        _pluginConfiguration.Token = GetKinopoiskDevToken();

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

        Assert.NotNull(_kpEpisodeProvider.Name);

        HttpResponseInfo response = await _kpEpisodeProvider.GetImageResponse("https://www.google.com", CancellationToken.None);
        Assert.True(response.StatusCode == HttpStatusCode.OK);

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
            .Returns("KpEpisodeProvider_GetMetadata_ProviderIds");

        using var cancellationTokenSource = new CancellationTokenSource();
        MetadataResult<Episode> result = await _kpEpisodeProvider.GetMetadata(episodeInfo, cancellationTokenSource.Token);

        Assert.True(result.HasMetadata);
        Episode episode = result.Item;
        Assert.NotNull(episode);
        Assert.Equal(2, episode.IndexNumber);
        Assert.Equal("Video", episode.MediaType);
        Assert.Equal("А я сказал — оседлаю!", episode.Name);
        Assert.Equal("I Said I'm Gonna Pilot That Thing!!", episode.OriginalTitle);
        Assert.Equal(1, episode.ParentIndexNumber);
        Assert.Equal(DateTime.Parse("2007-04-08T00:00:00.0000000+00:00", DateTimeFormatInfo.InvariantInfo), episode.PremiereDate);
        Assert.Equal(episode.Name, episode.SortName);

        _logManager.Verify(lm => lm.GetLogger(It.IsAny<string>()), Times.Exactly(4));
        _applicationPaths.VerifyGet(ap => ap.PluginConfigurationsPath, Times.Once());
        _xmlSerializer.Verify(xs => xs.DeserializeFromFile(typeof(PluginConfiguration), "KpEpisodeProvider_GetMetadata_ProviderIds/EmbyKinopoiskRu.xml"), Times.Once());
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
            .Returns("KpEpisodeProvider_GetMetadata_SeriesProviderIds");

        using var cancellationTokenSource = new CancellationTokenSource();
        MetadataResult<Episode> result = await _kpEpisodeProvider.GetMetadata(episodeInfo, cancellationTokenSource.Token);

        Assert.True(result.HasMetadata);
        Episode episode = result.Item;
        Assert.NotNull(episode);
        Assert.Equal(2, episode.IndexNumber);
        Assert.Equal("Video", episode.MediaType);
        Assert.Equal("А я сказал — оседлаю!", episode.Name);
        Assert.Equal("I Said I'm Gonna Pilot That Thing!!", episode.OriginalTitle);
        Assert.Equal(1, episode.ParentIndexNumber);
        Assert.Equal(DateTime.Parse("2007-04-08T00:00:00.0000000+00:00", DateTimeFormatInfo.InvariantInfo), episode.PremiereDate);
        Assert.Equal(episode.Name, episode.SortName);

        _logManager.Verify(lm => lm.GetLogger(It.IsAny<string>()), Times.Exactly(4));
        _applicationPaths.VerifyGet(ap => ap.PluginConfigurationsPath, Times.Once());
        _xmlSerializer.Verify(xs => xs.DeserializeFromFile(typeof(PluginConfiguration), "KpEpisodeProvider_GetMetadata_SeriesProviderIds/EmbyKinopoiskRu.xml"), Times.Once());
        _localizationManager.Verify(lm => lm.RemoveDiacritics("А я сказал — оседлаю!"), Times.Once());
        _serverConfigurationManager.VerifyGet(scm => scm.Configuration, Times.Once());
        VerifyNoOtherCalls();

        Logger.Info($"Finish '{nameof(KpEpisodeProvider_GetMetadata_SeriesProviderIds)}'");
    }

}
