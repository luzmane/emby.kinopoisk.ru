using System.Net;

using EmbyKinopoiskRu.Api.KinopoiskDev;
using EmbyKinopoiskRu.Configuration;
using EmbyKinopoiskRu.Provider.RemoteMetadata;

using FluentAssertions;

using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Providers;

namespace EmbyKinopoiskRu.Tests.KinopoiskDev;

[Collection("Sequential")]
public class KpSeriesProviderTest : BaseTest
{
    private static readonly NLog.ILogger Logger = NLog.LogManager.GetLogger(nameof(KpSeriesProviderTest));

    private readonly KpSeriesProvider _kpSeriesProvider;


    #region Test configs

    public KpSeriesProviderTest() : base(Logger)
    {
        _pluginConfiguration.Token = GetKinopoiskDevToken();

        ConfigLibraryManager();

        ConfigXmlSerializer();

        _kpSeriesProvider = new KpSeriesProvider(_httpClient, _logManager.Object);
    }

    #endregion

    [Fact]
    public async void KpSeriesProvider_ForCodeCoverage()
    {
        Logger.Info($"Start '{nameof(KpSeriesProvider_ForCodeCoverage)}'");

        _kpSeriesProvider.Name.Should().NotBeNull();

        HttpResponseInfo response = await _kpSeriesProvider.GetImageResponse("https://www.google.com", CancellationToken.None);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        _logManager.Verify(lm => lm.GetLogger("KpSeriesProvider"), Times.Once());
        _logManager.Verify(lm => lm.GetLogger("KinopoiskRu"), Times.Once());
        VerifyNoOtherCalls();

        Logger.Info($"Finished '{nameof(KpSeriesProvider_ForCodeCoverage)}'");
    }

    [Fact]
    public async void KpSeriesProvider_GetMetadata_Provider_Kp()
    {
        Logger.Info($"Start '{nameof(KpSeriesProvider_GetMetadata_Provider_Kp)}'");

        _ = _applicationPaths
            .SetupGet(m => m.PluginConfigurationsPath)
            .Returns("KpSeriesProvider_GetMetadata_Provider_Kp");

        var seriesInfo = new SeriesInfo
        {
            ProviderIds = new ProviderIdDictionary(new Dictionary<string, string>
            {
                { Plugin.PluginKey, "502838" }
            })
        };
        using var cancellationTokenSource = new CancellationTokenSource();
        MetadataResult<Series> result = await _kpSeriesProvider.GetMetadata(seriesInfo, cancellationTokenSource.Token);

        result.HasMetadata.Should().BeTrue();
        VerifySeries502838(result.Item);

        result.People.Should().HaveCount(24);
        VerifyPersonInfo34549(result.People.FirstOrDefault(p => "Бенедикт Камбербэтч".Equals(p.Name, StringComparison.Ordinal)));

        _logManager.Verify(lm => lm.GetLogger(It.IsAny<string>()), Times.Exactly(4));
        _applicationPaths.VerifyGet(ap => ap.PluginConfigurationsPath, Times.Once());
        _xmlSerializer.Verify(xs => xs.DeserializeFromFile(typeof(PluginConfiguration), "KpSeriesProvider_GetMetadata_Provider_Kp/EmbyKinopoiskRu.xml"), Times.Once());
        _localizationManager.Verify(lm => lm.RemoveDiacritics("Шерлок"), Times.Once());
        VerifyNoOtherCalls();

        Logger.Info($"Finished '{nameof(KpSeriesProvider_GetMetadata_Provider_Kp)}'");
    }

    [Fact]
    public async void KpSeriesProvider_GetMetadata_Provider_Imdb()
    {
        Logger.Info($"Start '{nameof(KpSeriesProvider_GetMetadata_Provider_Imdb)}'");

        _ = _applicationPaths
            .SetupGet(m => m.PluginConfigurationsPath)
            .Returns("KpSeriesProvider_GetMetadata_Provider_Imdb");

        var seriesInfo = new SeriesInfo
        {
            ProviderIds = new ProviderIdDictionary(new Dictionary<string, string>
            {
                { MetadataProviders.Imdb.ToString(), "tt1475582" }
            })
        };
        using var cancellationTokenSource = new CancellationTokenSource();
        MetadataResult<Series> result = await _kpSeriesProvider.GetMetadata(seriesInfo, cancellationTokenSource.Token);

        result.HasMetadata.Should().BeTrue();
        VerifySeries502838(result.Item);

        result.People.Should().HaveCount(24);
        VerifyPersonInfo34549(result.People.FirstOrDefault(p => "Бенедикт Камбербэтч".Equals(p.Name, StringComparison.Ordinal)));

        _logManager.Verify(lm => lm.GetLogger(It.IsAny<string>()), Times.Exactly(4));
        _applicationPaths.VerifyGet(ap => ap.PluginConfigurationsPath, Times.Once());
        _xmlSerializer.Verify(xs => xs.DeserializeFromFile(typeof(PluginConfiguration), "KpSeriesProvider_GetMetadata_Provider_Imdb/EmbyKinopoiskRu.xml"), Times.Once());
        _localizationManager.Verify(lm => lm.RemoveDiacritics("Шерлок"), Times.Once());
        VerifyNoOtherCalls();

        Logger.Info($"Finished '{nameof(KpSeriesProvider_GetMetadata_Provider_Imdb)}'");
    }

    [Fact]
    public async void KpSeriesProvider_GetMetadata_Provider_Tmdb()
    {
        Logger.Info($"Start '{nameof(KpSeriesProvider_GetMetadata_Provider_Tmdb)}'");

        _ = _applicationPaths
            .SetupGet(m => m.PluginConfigurationsPath)
            .Returns("KpSeriesProvider_GetMetadata_Provider_Tmdb");

        var seriesInfo = new SeriesInfo
        {
            ProviderIds = new ProviderIdDictionary(new Dictionary<string, string>
            {
                { MetadataProviders.Tmdb.ToString(), "19885" }
            })
        };
        using var cancellationTokenSource = new CancellationTokenSource();
        MetadataResult<Series> result = await _kpSeriesProvider.GetMetadata(seriesInfo, cancellationTokenSource.Token);

        result.HasMetadata.Should().BeTrue();
        VerifySeries502838(result.Item);

        result.People.Should().HaveCount(24);
        VerifyPersonInfo34549(result.People.FirstOrDefault(p => "Бенедикт Камбербэтч".Equals(p.Name, StringComparison.Ordinal)));

        _logManager.Verify(lm => lm.GetLogger(It.IsAny<string>()), Times.Exactly(4));
        _applicationPaths.VerifyGet(ap => ap.PluginConfigurationsPath, Times.Once());
        _xmlSerializer.Verify(xs => xs.DeserializeFromFile(typeof(PluginConfiguration), "KpSeriesProvider_GetMetadata_Provider_Tmdb/EmbyKinopoiskRu.xml"), Times.Once());
        _localizationManager.Verify(lm => lm.RemoveDiacritics("Шерлок"), Times.Once());
        VerifyNoOtherCalls();

        Logger.Info($"Finished '{nameof(KpSeriesProvider_GetMetadata_Provider_Tmdb)}'");
    }

    [Fact]
    public async void KpSeriesProvider_GetMetadata_NameAndYear()
    {
        Logger.Info($"Start '{nameof(KpSeriesProvider_GetMetadata_NameAndYear)}'");

        _ = _applicationPaths
            .SetupGet(m => m.PluginConfigurationsPath)
            .Returns("KpSeriesProvider_GetMetadata_NameAndYear");

        var seriesInfo = new SeriesInfo
        {
            Name = "Шерлок",
            Year = 2010
        };
        using var cancellationTokenSource = new CancellationTokenSource();
        MetadataResult<Series> result = await _kpSeriesProvider.GetMetadata(seriesInfo, cancellationTokenSource.Token);

        result.HasMetadata.Should().BeTrue();
        VerifySeries502838(result.Item);

        result.People.Should().HaveCount(24);
        VerifyPersonInfo34549(result.People.FirstOrDefault(p => "Бенедикт Камбербэтч".Equals(p.Name, StringComparison.Ordinal)));

        _logManager.Verify(lm => lm.GetLogger(It.IsAny<string>()), Times.Exactly(4));
        _applicationPaths.VerifyGet(ap => ap.PluginConfigurationsPath, Times.Once());
        _xmlSerializer.Verify(xs => xs.DeserializeFromFile(typeof(PluginConfiguration), "KpSeriesProvider_GetMetadata_NameAndYear/EmbyKinopoiskRu.xml"), Times.Once());
        _localizationManager.Verify(lm => lm.RemoveDiacritics("Шерлок"), Times.Once());
        VerifyNoOtherCalls();

        Logger.Info($"Finished '{nameof(KpSeriesProvider_GetMetadata_NameAndYear)}'");
    }

    [Fact]
    public async void KpSeriesProvider_GetSearchResults_Provider_Kp()
    {
        Logger.Info($"Start '{nameof(KpSeriesProvider_GetSearchResults_Provider_Kp)}'");

        _ = _applicationPaths
            .SetupGet(m => m.PluginConfigurationsPath)
            .Returns("KpSeriesProvider_GetSearchResults_Provider_Kp");

        var seriesInfo = new SeriesInfo
        {
            ProviderIds = new ProviderIdDictionary(new Dictionary<string, string>
            {
                { Plugin.PluginKey, "502838" }
            })
        };
        using var cancellationTokenSource = new CancellationTokenSource();
        IEnumerable<RemoteSearchResult> result = await _kpSeriesProvider.GetSearchResults(seriesInfo, cancellationTokenSource.Token);
        result.Should().ContainSingle();
        VerifyRemoteSearchResult502838(result.First());

        _logManager.Verify(lm => lm.GetLogger(It.IsAny<string>()), Times.Exactly(4));
        _applicationPaths.VerifyGet(ap => ap.PluginConfigurationsPath, Times.Once());
        _xmlSerializer.Verify(xs => xs.DeserializeFromFile(typeof(PluginConfiguration), "KpSeriesProvider_GetSearchResults_Provider_Kp/EmbyKinopoiskRu.xml"), Times.Once());
        VerifyNoOtherCalls();

        Logger.Info($"Finished '{nameof(KpSeriesProvider_GetSearchResults_Provider_Kp)}'");
    }

    [Fact]
    public async void KpSeriesProvider_GetSearchResults_Provider_Imdb()
    {
        Logger.Info($"Start '{nameof(KpSeriesProvider_GetSearchResults_Provider_Imdb)}'");

        _ = _applicationPaths
            .SetupGet(m => m.PluginConfigurationsPath)
            .Returns("KpSeriesProvider_GetSearchResults_Provider_Imdb");

        var seriesInfo = new SeriesInfo
        {
            ProviderIds = new ProviderIdDictionary(new Dictionary<string, string>
            {
                { MetadataProviders.Imdb.ToString(), "tt1475582" }
            })
        };
        using var cancellationTokenSource = new CancellationTokenSource();
        IEnumerable<RemoteSearchResult> result = await _kpSeriesProvider.GetSearchResults(seriesInfo, cancellationTokenSource.Token);
        result.Should().ContainSingle();
        VerifyRemoteSearchResult502838(result.First());

        _logManager.Verify(lm => lm.GetLogger(It.IsAny<string>()), Times.Exactly(4));
        _applicationPaths.VerifyGet(ap => ap.PluginConfigurationsPath, Times.Once());
        _xmlSerializer.Verify(xs => xs.DeserializeFromFile(typeof(PluginConfiguration), "KpSeriesProvider_GetSearchResults_Provider_Imdb/EmbyKinopoiskRu.xml"), Times.Once());
        VerifyNoOtherCalls();

        Logger.Info($"Finished '{nameof(KpSeriesProvider_GetSearchResults_Provider_Imdb)}'");
    }

    [Fact]
    public async void KpSeriesProvider_GetSearchResults_NameAndYear()
    {
        Logger.Info($"Start '{nameof(KpSeriesProvider_GetSearchResults_NameAndYear)}'");

        _ = _applicationPaths
            .SetupGet(m => m.PluginConfigurationsPath)
            .Returns("KpSeriesProvider_GetSearchResults_NameAndYear");

        var seriesInfo = new SeriesInfo
        {
            Name = "Шерлок",
            Year = 2010
        };
        using var cancellationTokenSource = new CancellationTokenSource();
        IEnumerable<RemoteSearchResult> result = await _kpSeriesProvider.GetSearchResults(seriesInfo, cancellationTokenSource.Token);
        result.Should().HaveCountLessThanOrEqualTo(KinopoiskDevApi.ApiResponseLimit);
        VerifyRemoteSearchResult502838(result.First(x => x.ProviderIds[Plugin.PluginKey] == "502838"));

        _logManager.Verify(lm => lm.GetLogger(It.IsAny<string>()), Times.Exactly(4));
        _applicationPaths.VerifyGet(ap => ap.PluginConfigurationsPath, Times.Once());
        _xmlSerializer.Verify(xs => xs.DeserializeFromFile(typeof(PluginConfiguration), "KpSeriesProvider_GetSearchResults_NameAndYear/EmbyKinopoiskRu.xml"), Times.Once());
        VerifyNoOtherCalls();

        Logger.Info($"Finished '{nameof(KpSeriesProvider_GetSearchResults_NameAndYear)}'");
    }
}
