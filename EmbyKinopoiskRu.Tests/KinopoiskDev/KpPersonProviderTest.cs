using System.Globalization;
using System.Net;

using EmbyKinopoiskRu.Configuration;
using EmbyKinopoiskRu.Provider.RemoteMetadata;

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
        _pluginConfiguration.Token = KINOPOISK_DEV_TOKEN;

        ConfigLibraryManager();

        ConfigXmlSerializer();

        _kpPersonProvider = new(_httpClient, _logManager.Object);
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
    public async void KpPersonProvider_ForCodeCoverage()
    {
        Logger.Info($"Start '{nameof(KpPersonProvider_ForCodeCoverage)}'");

        Assert.NotNull(_kpPersonProvider.Name);

        HttpResponseInfo response = await _kpPersonProvider.GetImageResponse("https://www.google.com", CancellationToken.None);
        Assert.True(response.StatusCode == HttpStatusCode.OK);

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
            .Returns("KpPersonProvider_GetMetadata_Provider_Kp");

        var personInfo = new PersonLookupInfo()
        {
            ProviderIds = new(new() { { Plugin.PluginKey, "29855" } })
        };
        using var cancellationTokenSource = new CancellationTokenSource();
        MetadataResult<Person> result = await _kpPersonProvider.GetMetadata(personInfo, cancellationTokenSource.Token);

        Assert.True(result.HasMetadata);
        Person person = result.Item;
        Assert.NotNull(person);
        Assert.Equal("Джеки Чан", person.Name);
        Assert.Equal("Настоящее имя — Чань Кунсан (Chan Kongsang), что означает &#171;Чан, рожденный в Гонконге&#187;.\nЧан делает самостоятельно большую часть трюков, а также иногда дублирует других актёров; он неоднократно получал травмы (во время финальных титров в его фильмах обычно демонстрируются неудавшиеся дубли), поэтому Чан внесён в чёрные списки страховых компаний по всему миру.\nУ Чана есть дочь Этта Нг (род. 19 ноября 1999) от внебрачной связи с актрисой Элейн Нг.\nЕго родители — Чарльз Чан и Ли-Ли Чан — бежали в Гонконг с континента во время гражданской войны, а в 1960 году перебрались в Австралию.\nВ возрасте 6 лет Чан был отдан в школу пекинской оперы в Гонконге.\nДважды попадал в Книгу рекордов Гиннесса за «наибольшее число трюков в карьере» и «самое большое число упоминаний в титрах одного фильма». Так, в фильме «<a href=\"/film/654749/\" class=\"all\">Доспехи Бога 3: Миссия Зодиак</a>» Джеки был задействован на 15 должностях (актёр, режиссёр, сценарист, продюсер, оператор, осветитель, постановщик трюков, исполнитель песни, ответственный за питание съёмочной группы и др.), тем самым побив рекорд <a href=\"/name/30966/\" class=\"all\">Роберта Родригеса</a>.", person.Overview);
        Assert.Equal(DateTime.Parse("1954-04-07T00:00:00.0000000+00:00", DateTimeFormatInfo.InvariantInfo), person.PremiereDate);
        var place = Assert.Single(person.ProductionLocations);
        Assert.Equal("29855", person.GetProviderId(Plugin.PluginKey));
        Assert.Equal(person.Name, person.SortName);

        _logManager.Verify(lm => lm.GetLogger(It.IsAny<string>()), Times.Exactly(4));
        _applicationPaths.VerifyGet(ap => ap.PluginConfigurationsPath, Times.Once());
        _xmlSerializer.Verify(xs => xs.DeserializeFromFile(typeof(PluginConfiguration), "KpPersonProvider_GetMetadata_Provider_Kp/EmbyKinopoiskRu.xml"), Times.Once());
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
            .Returns("KpPersonProvider_GetMetadata_ByName");

        var personInfo = new PersonLookupInfo()
        {
            Name = "Джеки Чан"
        };
        using var cancellationTokenSource = new CancellationTokenSource();
        MetadataResult<Person> result = await _kpPersonProvider.GetMetadata(personInfo, cancellationTokenSource.Token);

        Assert.True(result.HasMetadata);
        Person person = result.Item;
        Assert.NotNull(person);
        Assert.Equal("Джеки Чан", person.Name);
        Assert.Equal("Jackie Chan", person.OriginalTitle);
        Assert.Equal("Настоящее имя — Чань Кунсан (Chan Kongsang), что означает &#171;Чан, рожденный в Гонконге&#187;.\nЧан делает самостоятельно большую часть трюков, а также иногда дублирует других актёров; он неоднократно получал травмы (во время финальных титров в его фильмах обычно демонстрируются неудавшиеся дубли), поэтому Чан внесён в чёрные списки страховых компаний по всему миру.\nУ Чана есть дочь Этта Нг (род. 19 ноября 1999) от внебрачной связи с актрисой Элейн Нг.\nЕго родители — Чарльз Чан и Ли-Ли Чан — бежали в Гонконг с континента во время гражданской войны, а в 1960 году перебрались в Австралию.\nВ возрасте 6 лет Чан был отдан в школу пекинской оперы в Гонконге.\nДважды попадал в Книгу рекордов Гиннесса за «наибольшее число трюков в карьере» и «самое большое число упоминаний в титрах одного фильма». Так, в фильме «<a href=\"/film/654749/\" class=\"all\">Доспехи Бога 3: Миссия Зодиак</a>» Джеки был задействован на 15 должностях (актёр, режиссёр, сценарист, продюсер, оператор, осветитель, постановщик трюков, исполнитель песни, ответственный за питание съёмочной группы и др.), тем самым побив рекорд <a href=\"/name/30966/\" class=\"all\">Роберта Родригеса</a>.", person.Overview);
        Assert.Equal(DateTime.Parse("1954-04-07T00:00:00.0000000+00:00", DateTimeFormatInfo.InvariantInfo), person.PremiereDate);
        var place = Assert.Single(person.ProductionLocations);
        Assert.Equal("29855", person.GetProviderId(Plugin.PluginKey));
        Assert.Equal(person.Name, person.SortName);

        _logManager.Verify(lm => lm.GetLogger(It.IsAny<string>()), Times.Exactly(4));
        _applicationPaths.VerifyGet(ap => ap.PluginConfigurationsPath, Times.Once());
        _xmlSerializer.Verify(xs => xs.DeserializeFromFile(typeof(PluginConfiguration), "KpPersonProvider_GetMetadata_ByName/EmbyKinopoiskRu.xml"), Times.Once());
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
            .Returns("KpPersonProvider_GetMetadata_ByEnName");

        var personInfo = new PersonLookupInfo()
        {
            Name = "Jackie Chan"
        };
        using var cancellationTokenSource = new CancellationTokenSource();
        MetadataResult<Person> result = await _kpPersonProvider.GetMetadata(personInfo, cancellationTokenSource.Token);

        Assert.True(result.HasMetadata);
        Person person = result.Item;
        Assert.NotNull(person);
        Assert.Equal("Джеки Чан", person.Name);
        Assert.Equal("Jackie Chan", person.OriginalTitle);
        Assert.Equal("Настоящее имя — Чань Кунсан (Chan Kongsang), что означает &#171;Чан, рожденный в Гонконге&#187;.\nЧан делает самостоятельно большую часть трюков, а также иногда дублирует других актёров; он неоднократно получал травмы (во время финальных титров в его фильмах обычно демонстрируются неудавшиеся дубли), поэтому Чан внесён в чёрные списки страховых компаний по всему миру.\nУ Чана есть дочь Этта Нг (род. 19 ноября 1999) от внебрачной связи с актрисой Элейн Нг.\nЕго родители — Чарльз Чан и Ли-Ли Чан — бежали в Гонконг с континента во время гражданской войны, а в 1960 году перебрались в Австралию.\nВ возрасте 6 лет Чан был отдан в школу пекинской оперы в Гонконге.\nДважды попадал в Книгу рекордов Гиннесса за «наибольшее число трюков в карьере» и «самое большое число упоминаний в титрах одного фильма». Так, в фильме «<a href=\"/film/654749/\" class=\"all\">Доспехи Бога 3: Миссия Зодиак</a>» Джеки был задействован на 15 должностях (актёр, режиссёр, сценарист, продюсер, оператор, осветитель, постановщик трюков, исполнитель песни, ответственный за питание съёмочной группы и др.), тем самым побив рекорд <a href=\"/name/30966/\" class=\"all\">Роберта Родригеса</a>.", person.Overview);
        Assert.Equal(DateTime.Parse("1954-04-07T00:00:00.0000000+00:00", DateTimeFormatInfo.InvariantInfo), person.PremiereDate);
        var place = Assert.Single(person.ProductionLocations);
        Assert.Equal("29855", person.GetProviderId(Plugin.PluginKey));
        Assert.Equal(person.Name, person.SortName);

        _logManager.Verify(lm => lm.GetLogger(It.IsAny<string>()), Times.Exactly(4));
        _applicationPaths.VerifyGet(ap => ap.PluginConfigurationsPath, Times.Once());
        _xmlSerializer.Verify(xs => xs.DeserializeFromFile(typeof(PluginConfiguration), "KpPersonProvider_GetMetadata_ByEnName/EmbyKinopoiskRu.xml"), Times.Once());
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
            .Returns("KpPersonProvider_GetSearchResults_Provider_Kp");

        var personInfo = new PersonLookupInfo()
        {
            ProviderIds = new(new() { { Plugin.PluginKey, "29855" } })
        };
        using var cancellationTokenSource = new CancellationTokenSource();
        IEnumerable<RemoteSearchResult> result = await _kpPersonProvider.GetSearchResults(personInfo, cancellationTokenSource.Token);

        RemoteSearchResult searchResult = Assert.Single(result);
        Assert.NotNull(searchResult);
        Assert.Equal("29855", searchResult.GetProviderId(Plugin.PluginKey));
        Assert.Equal("Джеки Чан", searchResult.Name);
        Assert.True(!string.IsNullOrWhiteSpace(searchResult.ImageUrl));
        Assert.Equal(Plugin.PluginKey, searchResult.SearchProviderName);

        _logManager.Verify(lm => lm.GetLogger(It.IsAny<string>()), Times.Exactly(4));
        _applicationPaths.VerifyGet(ap => ap.PluginConfigurationsPath, Times.Once());
        _xmlSerializer.Verify(xs => xs.DeserializeFromFile(typeof(PluginConfiguration), "KpPersonProvider_GetSearchResults_Provider_Kp/EmbyKinopoiskRu.xml"), Times.Once());
        VerifyNoOtherCalls();

        Logger.Info($"Finish '{nameof(KpPersonProvider_GetSearchResults_Provider_Kp)}'");
    }

    [Fact]
    public async void KpPersonProvider_GetSearchResults_ByName()
    {
        Logger.Info($"Start '{nameof(KpPersonProvider_GetSearchResults_ByName)}'");

        _ = _applicationPaths
            .SetupGet(m => m.PluginConfigurationsPath)
            .Returns("KpPersonProvider_GetSearchResults_ByName");

        var personInfo = new PersonLookupInfo()
        {
            Name = "Джеки Чан"
        };
        using var cancellationTokenSource = new CancellationTokenSource();
        IEnumerable<RemoteSearchResult> result = await _kpPersonProvider.GetSearchResults(personInfo, cancellationTokenSource.Token);

        RemoteSearchResult searchResult = Assert.Single(result);
        Assert.NotNull(searchResult);
        Assert.Equal("29855", searchResult.GetProviderId(Plugin.PluginKey));
        Assert.Equal("Джеки Чан", searchResult.Name);
        Assert.True(!string.IsNullOrWhiteSpace(searchResult.ImageUrl));

        _logManager.Verify(lm => lm.GetLogger(It.IsAny<string>()), Times.Exactly(4));
        _applicationPaths.VerifyGet(ap => ap.PluginConfigurationsPath, Times.Once());
        _xmlSerializer.Verify(xs => xs.DeserializeFromFile(typeof(PluginConfiguration), "KpPersonProvider_GetSearchResults_ByName/EmbyKinopoiskRu.xml"), Times.Once());
        VerifyNoOtherCalls();

        Logger.Info($"Finish '{nameof(KpPersonProvider_GetSearchResults_ByName)}'");
    }

    [Fact]
    public async void KpPersonProvider_GetSearchResults_ByEnName()
    {
        Logger.Info($"Start '{nameof(KpPersonProvider_GetSearchResults_ByEnName)}'");

        _ = _applicationPaths
            .SetupGet(m => m.PluginConfigurationsPath)
            .Returns("KpPersonProvider_GetSearchResults_ByEnName");

        var personInfo = new PersonLookupInfo()
        {
            Name = "Jackie Chan"
        };
        using var cancellationTokenSource = new CancellationTokenSource();
        IEnumerable<RemoteSearchResult> result = await _kpPersonProvider.GetSearchResults(personInfo, cancellationTokenSource.Token);

        RemoteSearchResult searchResult = Assert.Single(result);
        Assert.NotNull(searchResult);
        Assert.Equal("29855", searchResult.GetProviderId(Plugin.PluginKey));
        Assert.Equal("Джеки Чан", searchResult.Name);
        Assert.True(!string.IsNullOrWhiteSpace(searchResult.ImageUrl));
        Assert.Equal(Plugin.PluginKey, searchResult.SearchProviderName);

        _logManager.Verify(lm => lm.GetLogger(It.IsAny<string>()), Times.Exactly(4));
        _applicationPaths.VerifyGet(ap => ap.PluginConfigurationsPath, Times.Once());
        _xmlSerializer.Verify(xs => xs.DeserializeFromFile(typeof(PluginConfiguration), "KpPersonProvider_GetSearchResults_ByEnName/EmbyKinopoiskRu.xml"), Times.Once());
        VerifyNoOtherCalls();

        Logger.Info($"Finish '{nameof(KpPersonProvider_GetSearchResults_ByEnName)}'");
    }



}
