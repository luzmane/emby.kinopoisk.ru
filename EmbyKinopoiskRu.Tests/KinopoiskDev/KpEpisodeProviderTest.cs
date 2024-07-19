using System.Net;

using EmbyKinopoiskRu.Configuration;
using EmbyKinopoiskRu.Provider.RemoteMetadata;

using FluentAssertions;

using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Providers;

namespace EmbyKinopoiskRu.Tests.KinopoiskDev;

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

        _kpEpisodeProvider = new KpEpisodeProvider(_httpClient, _logManager.Object);
    }

    #endregion

    [Fact]
    public async Task KpEpisodeProvider_ForCodeCoverage()
    {
        Logger.Info($"Start '{nameof(KpEpisodeProvider_ForCodeCoverage)}'");

        _kpEpisodeProvider.Name.Should().NotBeNull();

        HttpResponseInfo response = await _kpEpisodeProvider.GetImageResponse("https://www.google.com", CancellationToken.None);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        _logManager.Verify(lm => lm.GetLogger("KpEpisodeProvider"), Times.Once());
        _logManager.Verify(lm => lm.GetLogger("KinopoiskRu"), Times.Once());

        VerifyNoOtherCalls();

        Logger.Info($"Finished '{nameof(KpEpisodeProvider_ForCodeCoverage)}'");
    }

    [Fact]
    public async Task KpEpisodeProvider_GetMetadata_ProviderIds()
    {
        Logger.Info($"Start '{nameof(KpEpisodeProvider_GetMetadata_ProviderIds)}'");

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
            .Returns(nameof(KpEpisodeProvider_GetMetadata_ProviderIds));

        using var cancellationTokenSource = new CancellationTokenSource();
        MetadataResult<Episode> result = await _kpEpisodeProvider.GetMetadata(episodeInfo, cancellationTokenSource.Token);

        result.HasMetadata.Should().BeTrue();
        VerifyEpisode_452973_1_2(result.Item);

        _logManager.Verify(lm => lm.GetLogger(It.IsAny<string>()), Times.Exactly(4));
        _applicationPaths.VerifyGet(ap => ap.PluginConfigurationsPath, Times.Once());
        _xmlSerializer.Verify(xs => xs.DeserializeFromFile(typeof(PluginConfiguration), $"{nameof(KpEpisodeProvider_GetMetadata_ProviderIds)}/EmbyKinopoiskRu.xml"), Times.Once());
        _localizationManager.Verify(lm => lm.RemoveDiacritics("А я сказал — оседлаю!"), Times.Once());
        _serverConfigurationManager.VerifyGet(scm => scm.Configuration, Times.Once());
        VerifyNoOtherCalls();

        Logger.Info($"Finish '{nameof(KpEpisodeProvider_GetMetadata_ProviderIds)}'");
    }

    [Fact]
    public async Task KpEpisodeProvider_GetMetadata_SeriesProviderIds()
    {
        Logger.Info($"Start '{nameof(KpEpisodeProvider_GetMetadata_SeriesProviderIds)}'");

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
            .Returns(nameof(KpEpisodeProvider_GetMetadata_SeriesProviderIds));

        using var cancellationTokenSource = new CancellationTokenSource();
        MetadataResult<Episode> result = await _kpEpisodeProvider.GetMetadata(episodeInfo, cancellationTokenSource.Token);

        result.HasMetadata.Should().BeTrue();
        VerifyEpisode_452973_1_2(result.Item);

        _logManager.Verify(lm => lm.GetLogger(It.IsAny<string>()), Times.Exactly(4));
        _applicationPaths.VerifyGet(ap => ap.PluginConfigurationsPath, Times.Once());
        _xmlSerializer.Verify(xs => xs.DeserializeFromFile(typeof(PluginConfiguration), $"{nameof(KpEpisodeProvider_GetMetadata_SeriesProviderIds)}/EmbyKinopoiskRu.xml"), Times.Once());
        _localizationManager.Verify(lm => lm.RemoveDiacritics("А я сказал — оседлаю!"), Times.Once());
        _serverConfigurationManager.VerifyGet(scm => scm.Configuration, Times.Once());
        VerifyNoOtherCalls();

        Logger.Info($"Finish '{nameof(KpEpisodeProvider_GetMetadata_SeriesProviderIds)}'");
    }

    [Fact]
    public async Task KpEpisodeProvider_GetSearchResults_Provider_Kp()
    {
        Logger.Info($"Start '{nameof(KpEpisodeProvider_GetSearchResults_Provider_Kp)}'");

        _ = _applicationPaths
            .SetupGet(m => m.PluginConfigurationsPath)
            .Returns(nameof(KpEpisodeProvider_GetSearchResults_Provider_Kp));

        var personInfo = new EpisodeInfo
        {
            IndexNumber = 2,
            ParentIndexNumber = 1,
            SeriesProviderIds = new Dictionary<string, string>
            {
                { Plugin.PluginKey, "452973" }
            }
        };
        using var cancellationTokenSource = new CancellationTokenSource();
        IEnumerable<RemoteSearchResult> result = await _kpEpisodeProvider.GetSearchResults(personInfo, cancellationTokenSource.Token);
        result.Should().ContainSingle();
        VerifyRemoteSearchResult_452973_1_2(result.First());

        _logManager.Verify(lm => lm.GetLogger(It.IsAny<string>()), Times.Exactly(4));
        _applicationPaths.VerifyGet(ap => ap.PluginConfigurationsPath, Times.Once());
        _xmlSerializer.Verify(xs => xs.DeserializeFromFile(typeof(PluginConfiguration), $"{nameof(KpEpisodeProvider_GetSearchResults_Provider_Kp)}/EmbyKinopoiskRu.xml"), Times.Once());
        VerifyNoOtherCalls();

        Logger.Info($"Finish '{nameof(KpEpisodeProvider_GetSearchResults_Provider_Kp)}'");
    }
}
