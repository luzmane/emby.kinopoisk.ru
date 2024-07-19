using System.Net;

using EmbyKinopoiskRu.Configuration;
using EmbyKinopoiskRu.Provider.RemoteMetadata;

using FluentAssertions;

using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;

namespace EmbyKinopoiskRu.Tests.KinopoiskApiUnofficial;

public class KpEpisodeProviderTest : BaseTest
{
    private static readonly NLog.ILogger Logger = NLog.LogManager.GetLogger(nameof(KpEpisodeProviderTest));

    private readonly KpEpisodeProvider _kpEpisodeProvider;


    #region Test configs

    public KpEpisodeProviderTest() : base(Logger)
    {
        _pluginConfiguration.Token = GetKinopoiskUnofficialToken();
        _pluginConfiguration.ApiType = PluginConfiguration.KinopoiskApiUnofficialTech;

        ConfigLibraryManager();

        ConfigXmlSerializer();

        _kpEpisodeProvider = new KpEpisodeProvider(_httpClient, _logManager.Object);
    }

    #endregion

    [Fact]
    public async Task UN_KpEpisodeProvider_ForCodeCoverage()
    {
        Logger.Info($"Start '{nameof(UN_KpEpisodeProvider_ForCodeCoverage)}'");

        _kpEpisodeProvider.Name.Should().NotBeNullOrWhiteSpace("this is a name of the provider");

        HttpResponseInfo response = await _kpEpisodeProvider.GetImageResponse("https://www.google.com", CancellationToken.None);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        _logManager.Verify(lm => lm.GetLogger("KpEpisodeProvider"), Times.Once());
        _logManager.Verify(lm => lm.GetLogger("KinopoiskRu"), Times.Once());

        VerifyNoOtherCalls();

        Logger.Info($"Finished '{nameof(UN_KpEpisodeProvider_ForCodeCoverage)}'");
    }

    [Fact]
    public async Task UN_KpEpisodeProvider_GetMetadata_ProviderIds()
    {
        Logger.Info($"Start '{nameof(UN_KpEpisodeProvider_GetMetadata_ProviderIds)}'");

        var episodeInfo = new EpisodeInfo
        {
            SeriesProviderIds = null,
            IndexNumber = 2,
            ParentIndexNumber = 1,
            ProviderIds = new ProviderIdDictionary(new Dictionary<string, string>
            {
                { Plugin.PluginKey, "452973" }
            })
        };

        _ = _applicationPaths
            .SetupGet(m => m.PluginConfigurationsPath)
            .Returns(nameof(UN_KpEpisodeProvider_GetMetadata_ProviderIds));

        using var cancellationTokenSource = new CancellationTokenSource();
        MetadataResult<Episode> result = await _kpEpisodeProvider.GetMetadata(episodeInfo, cancellationTokenSource.Token);

        result.HasMetadata.Should().BeTrue();
        Episode episode = result.Item;
        episode.Should().NotBeNull();
        episode.IndexNumber.Should().Be(2);
        episode.MediaType.Should().Be("Video");
        episode.Name.Should().Be("А я сказал — оседлаю!");
        episode.OriginalTitle.Should().Be("I Said I'm Gonna Pilot That Thing!!");
        episode.ParentIndexNumber.Should().Be(1);
        episode.PremiereDate.Should().NotBeNull();
        episode.PremiereDate!.Value.DateTime.Should().HaveYear(2007).And.HaveMonth(4).And.HaveDay(8);
        episode.SortName.Should().Be(episode.Name);

        _logManager.Verify(lm => lm.GetLogger(It.IsAny<string>()), Times.Exactly(4));
        _applicationPaths.VerifyGet(ap => ap.PluginConfigurationsPath, Times.Once());
        _xmlSerializer.Verify(xs => xs.DeserializeFromFile(typeof(PluginConfiguration), $"{nameof(UN_KpEpisodeProvider_GetMetadata_ProviderIds)}/EmbyKinopoiskRu.xml"), Times.Once());
        _localizationManager.Verify(lm => lm.RemoveDiacritics("А я сказал — оседлаю!"), Times.Once());
        _serverConfigurationManager.VerifyGet(scm => scm.Configuration, Times.Once());
        VerifyNoOtherCalls();

        Logger.Info($"Finish '{nameof(UN_KpEpisodeProvider_GetMetadata_ProviderIds)}'");
    }

    [Fact]
    public async Task UN_KpEpisodeProvider_GetMetadata_SeriesProviderIds()
    {
        Logger.Info($"Start '{nameof(UN_KpEpisodeProvider_GetMetadata_SeriesProviderIds)}'");

        var episodeInfo = new EpisodeInfo
        {
            IndexNumber = 2,
            ParentIndexNumber = 1,
            SeriesProviderIds = new Dictionary<string, string>
            {
                { Plugin.PluginKey, "452973" }
            }
        };

        _ = _applicationPaths
            .SetupGet(m => m.PluginConfigurationsPath)
            .Returns(nameof(UN_KpEpisodeProvider_GetMetadata_SeriesProviderIds));

        using var cancellationTokenSource = new CancellationTokenSource();
        MetadataResult<Episode> result = await _kpEpisodeProvider.GetMetadata(episodeInfo, cancellationTokenSource.Token);

        result.HasMetadata.Should().BeTrue();
        Episode episode = result.Item;
        episode.Should().NotBeNull();
        episode.IndexNumber.Should().Be(2);
        episode.MediaType.Should().Be("Video");
        episode.Name.Should().Be("А я сказал — оседлаю!");
        episode.OriginalTitle.Should().Be("I Said I'm Gonna Pilot That Thing!!");
        episode.ParentIndexNumber.Should().Be(1);
        episode.PremiereDate.Should().NotBeNull();
        episode.PremiereDate!.Value.DateTime.Should().HaveYear(2007).And.HaveMonth(4).And.HaveDay(8);
        episode.SortName.Should().Be(episode.Name);

        _logManager.Verify(lm => lm.GetLogger(It.IsAny<string>()), Times.Exactly(4));
        _applicationPaths.VerifyGet(ap => ap.PluginConfigurationsPath, Times.Once());
        _xmlSerializer.Verify(xs => xs.DeserializeFromFile(typeof(PluginConfiguration), $"{nameof(UN_KpEpisodeProvider_GetMetadata_SeriesProviderIds)}/EmbyKinopoiskRu.xml"), Times.Once());
        _localizationManager.Verify(lm => lm.RemoveDiacritics("А я сказал — оседлаю!"), Times.Once());
        _serverConfigurationManager.VerifyGet(scm => scm.Configuration, Times.Once());
        VerifyNoOtherCalls();

        Logger.Info($"Finish '{nameof(UN_KpEpisodeProvider_GetMetadata_SeriesProviderIds)}'");
    }
}
