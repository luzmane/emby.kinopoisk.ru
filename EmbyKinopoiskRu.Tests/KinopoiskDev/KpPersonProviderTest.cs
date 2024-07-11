using System.Net;

using EmbyKinopoiskRu.Configuration;
using EmbyKinopoiskRu.Provider.RemoteMetadata;

using FluentAssertions;

using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Providers;

namespace EmbyKinopoiskRu.Tests.KinopoiskDev;

[Collection("Sequential")]
public class KpPersonProviderTest : BaseTest
{
    private static readonly NLog.ILogger Logger = NLog.LogManager.GetLogger(nameof(KpPersonProviderTest));

    private readonly KpPersonProvider _kpPersonProvider;


    #region Test configs

    public KpPersonProviderTest() : base(Logger)
    {
        _pluginConfiguration.Token = GetKinopoiskDevToken();

        ConfigLibraryManager();

        ConfigXmlSerializer();

        _kpPersonProvider = new KpPersonProvider(_httpClient, _logManager.Object);
    }

    #endregion

    [Fact]
    public async void KpPersonProvider_ForCodeCoverage()
    {
        Logger.Info($"Start '{nameof(KpPersonProvider_ForCodeCoverage)}'");

        _kpPersonProvider.Name.Should().NotBeNull();

        HttpResponseInfo response = await _kpPersonProvider.GetImageResponse("https://www.google.com", CancellationToken.None);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        _logManager.Verify(lm => lm.GetLogger("KpPersonProvider"), Times.Once());
        _logManager.Verify(lm => lm.GetLogger("KinopoiskRu"), Times.Once());

        VerifyNoOtherCalls();

        Logger.Info($"Finished '{nameof(KpPersonProvider_ForCodeCoverage)}'");
    }

    [Fact]
    public async void KpPersonProvider_GetMetadata_Provider_Kp()
    {
        Logger.Info($"Start '{nameof(KpPersonProvider_GetMetadata_Provider_Kp)}'");

        _ = _applicationPaths
            .SetupGet(m => m.PluginConfigurationsPath)
            .Returns(nameof(KpPersonProvider_GetMetadata_Provider_Kp));

        var personInfo = new PersonLookupInfo();
        personInfo.SetProviderId(Plugin.PluginKey, "29855");
        using var cancellationTokenSource = new CancellationTokenSource();
        MetadataResult<Person> result = await _kpPersonProvider.GetMetadata(personInfo, cancellationTokenSource.Token);

        result.HasMetadata.Should().BeTrue();
        VerifyPerson29855(result.Item);

        _logManager.Verify(lm => lm.GetLogger(It.IsAny<string>()), Times.Exactly(4));
        _applicationPaths.VerifyGet(ap => ap.PluginConfigurationsPath, Times.Once());
        _xmlSerializer.Verify(xs => xs.DeserializeFromFile(typeof(PluginConfiguration), $"{nameof(KpPersonProvider_GetMetadata_Provider_Kp)}/EmbyKinopoiskRu.xml"), Times.Once());
        _localizationManager.Verify(lm => lm.RemoveDiacritics("Джеки Чан"), Times.Once());
        VerifyNoOtherCalls();

        Logger.Info($"Finish '{nameof(KpPersonProvider_GetMetadata_Provider_Kp)}'");
    }

    [Fact]
    public async void KpPersonProvider_GetMetadata_ByName()
    {
        Logger.Info($"Start '{nameof(KpPersonProvider_GetMetadata_ByName)}'");

        _ = _applicationPaths
            .SetupGet(m => m.PluginConfigurationsPath)
            .Returns(nameof(KpPersonProvider_GetMetadata_ByName));

        var personInfo = new PersonLookupInfo
        {
            Name = "Джеки Чан"
        };
        using var cancellationTokenSource = new CancellationTokenSource();
        MetadataResult<Person> result = await _kpPersonProvider.GetMetadata(personInfo, cancellationTokenSource.Token);

        result.HasMetadata.Should().BeTrue();
        VerifyPerson29855(result.Item);

        _logManager.Verify(lm => lm.GetLogger(It.IsAny<string>()), Times.Exactly(4));
        _applicationPaths.VerifyGet(ap => ap.PluginConfigurationsPath, Times.Once());
        _xmlSerializer.Verify(xs => xs.DeserializeFromFile(typeof(PluginConfiguration), $"{nameof(KpPersonProvider_GetMetadata_ByName)}/EmbyKinopoiskRu.xml"), Times.Once());
        _localizationManager.Verify(lm => lm.RemoveDiacritics("Джеки Чан"), Times.Once());
        VerifyNoOtherCalls();

        Logger.Info($"Finish '{nameof(KpPersonProvider_GetMetadata_ByName)}'");
    }

    [Fact]
    public async void KpPersonProvider_GetMetadata_ByEnName()
    {
        Logger.Info($"Start '{nameof(KpPersonProvider_GetMetadata_ByEnName)}'");

        _ = _applicationPaths
            .SetupGet(m => m.PluginConfigurationsPath)
            .Returns(nameof(KpPersonProvider_GetMetadata_ByEnName));

        var personInfo = new PersonLookupInfo
        {
            Name = "Jackie Chan"
        };
        using var cancellationTokenSource = new CancellationTokenSource();
        MetadataResult<Person> result = await _kpPersonProvider.GetMetadata(personInfo, cancellationTokenSource.Token);

        result.HasMetadata.Should().BeTrue();
        VerifyPerson29855(result.Item);

        _logManager.Verify(lm => lm.GetLogger(It.IsAny<string>()), Times.Exactly(4));
        _applicationPaths.VerifyGet(ap => ap.PluginConfigurationsPath, Times.Once());
        _xmlSerializer.Verify(xs => xs.DeserializeFromFile(typeof(PluginConfiguration), $"{nameof(KpPersonProvider_GetMetadata_ByEnName)}/EmbyKinopoiskRu.xml"), Times.Once());
        _localizationManager.Verify(lm => lm.RemoveDiacritics("Джеки Чан"), Times.Once());
        VerifyNoOtherCalls();

        Logger.Info($"Finish '{nameof(KpPersonProvider_GetMetadata_ByEnName)}'");
    }

    [Fact]
    public async void KpPersonProvider_GetSearchResults_Provider_Kp()
    {
        Logger.Info($"Start '{nameof(KpPersonProvider_GetSearchResults_Provider_Kp)}'");

        _ = _applicationPaths
            .SetupGet(m => m.PluginConfigurationsPath)
            .Returns(nameof(KpPersonProvider_GetSearchResults_Provider_Kp));

        var personInfo = new PersonLookupInfo
        {
            ProviderIds = new ProviderIdDictionary(new Dictionary<string, string>
            {
                { Plugin.PluginKey, "29855" }
            })
        };
        using var cancellationTokenSource = new CancellationTokenSource();
        IEnumerable<RemoteSearchResult> result = await _kpPersonProvider.GetSearchResults(personInfo, cancellationTokenSource.Token);
        result.Should().ContainSingle();
        VerifyRemoteSearchResult29855(result.First());

        _logManager.Verify(lm => lm.GetLogger(It.IsAny<string>()), Times.Exactly(4));
        _applicationPaths.VerifyGet(ap => ap.PluginConfigurationsPath, Times.Once());
        _xmlSerializer.Verify(xs => xs.DeserializeFromFile(typeof(PluginConfiguration), $"{nameof(KpPersonProvider_GetSearchResults_Provider_Kp)}/EmbyKinopoiskRu.xml"), Times.Once());
        VerifyNoOtherCalls();

        Logger.Info($"Finish '{nameof(KpPersonProvider_GetSearchResults_Provider_Kp)}'");
    }

    [Fact]
    public async void KpPersonProvider_GetSearchResults_ByName()
    {
        Logger.Info($"Start '{nameof(KpPersonProvider_GetSearchResults_ByName)}'");

        _ = _applicationPaths
            .SetupGet(m => m.PluginConfigurationsPath)
            .Returns(nameof(KpPersonProvider_GetSearchResults_ByName));

        var personInfo = new PersonLookupInfo
        {
            Name = "Джеки Чан"
        };
        using var cancellationTokenSource = new CancellationTokenSource();
        IEnumerable<RemoteSearchResult> result = await _kpPersonProvider.GetSearchResults(personInfo, cancellationTokenSource.Token);
        result.Should().ContainSingle();
        VerifyRemoteSearchResult29855(result.First());

        _logManager.Verify(lm => lm.GetLogger(It.IsAny<string>()), Times.Exactly(4));
        _applicationPaths.VerifyGet(ap => ap.PluginConfigurationsPath, Times.Once());
        _xmlSerializer.Verify(xs => xs.DeserializeFromFile(typeof(PluginConfiguration), $"{nameof(KpPersonProvider_GetSearchResults_ByName)}/EmbyKinopoiskRu.xml"), Times.Once());
        VerifyNoOtherCalls();

        Logger.Info($"Finish '{nameof(KpPersonProvider_GetSearchResults_ByName)}'");
    }

    [Fact]
    public async void KpPersonProvider_GetSearchResults_ByEnName()
    {
        Logger.Info($"Start '{nameof(KpPersonProvider_GetSearchResults_ByEnName)}'");

        _ = _applicationPaths
            .SetupGet(m => m.PluginConfigurationsPath)
            .Returns(nameof(KpPersonProvider_GetSearchResults_ByEnName));

        var personInfo = new PersonLookupInfo
        {
            Name = "Jackie Chan"
        };
        using var cancellationTokenSource = new CancellationTokenSource();
        IEnumerable<RemoteSearchResult> result = await _kpPersonProvider.GetSearchResults(personInfo, cancellationTokenSource.Token);
        result.Should().ContainSingle();
        VerifyRemoteSearchResult29855(result.First());

        _logManager.Verify(lm => lm.GetLogger(It.IsAny<string>()), Times.Exactly(4));
        _applicationPaths.VerifyGet(ap => ap.PluginConfigurationsPath, Times.Once());
        _xmlSerializer.Verify(xs => xs.DeserializeFromFile(typeof(PluginConfiguration), $"{nameof(KpPersonProvider_GetSearchResults_ByEnName)}/EmbyKinopoiskRu.xml"), Times.Once());
        VerifyNoOtherCalls();

        Logger.Info($"Finish '{nameof(KpPersonProvider_GetSearchResults_ByEnName)}'");
    }
}
