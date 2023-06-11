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

        _kpPersonProvider = new(_httpClient, _logManager.Object);
    }

    #endregion

    [Fact]
    public async void KpPersonProvider_ForCodeCoverage()
    {
        Logger.Info($"Start '{nameof(KpPersonProvider_ForCodeCoverage)}'");

        _kpPersonProvider.Name.Should().NotBeNull("name is hardcoded");

        HttpResponseInfo response = await _kpPersonProvider.GetImageResponse("https://www.google.com", CancellationToken.None);
        response.StatusCode.Should().Be(HttpStatusCode.OK, "this is status code of the response to google.com");

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

        result.HasMetadata.Should().BeTrue("that mean the item was found");
        Person person = result.Item;
        person.Should().NotBeNull("that mean the person was found");
        person.Name.Should().Be("Джеки Чан", "this is the person's name");
        person.Overview.Should().Be("Настоящее имя — Чань Кунсан (Chan Kongsang), что означает &#171;Чан, рожденный в Гонконге&#187;.\nЧан делает самостоятельно большую часть трюков, а также иногда дублирует других актёров; он неоднократно получал травмы (во время финальных титров в его фильмах обычно демонстрируются неудавшиеся дубли), поэтому Чан внесён в чёрные списки страховых компаний по всему миру.\nУ Чана есть дочь Этта Нг (род. 19 ноября 1999) от внебрачной связи с актрисой Элейн Нг.\nЕго родители — Чарльз Чан и Ли-Ли Чан — бежали в Гонконг с континента во время гражданской войны, а в 1960 году перебрались в Австралию.\nВ возрасте 6 лет Чан был отдан в школу пекинской оперы в Гонконге.\nДважды попадал в Книгу рекордов Гиннесса за «наибольшее число трюков в карьере» и «самое большое число упоминаний в титрах одного фильма». Так, в фильме «<a href=\"/film/654749/\" class=\"all\">Доспехи Бога 3: Миссия Зодиак</a>» Джеки был задействован на 15 должностях (актёр, режиссёр, сценарист, продюсер, оператор, осветитель, постановщик трюков, исполнитель песни, ответственный за питание съёмочной группы и др.), тем самым побив рекорд <a href=\"/name/30966/\" class=\"all\">Роберта Родригеса</a>.", "this is person's Overview");
        person.PremiereDate.Should().NotBeNull("person should have a birthday date");
        person.PremiereDate!.Value.DateTime.Should().HaveYear(1954).And.HaveMonth(04).And.HaveDay(07);
        person.ProductionLocations.Should().ContainSingle();
        person.GetProviderId(Plugin.PluginKey).Should().Be("29855", "id of the requested item");
        person.SortName.Should().Be(person.Name, "SortName should be equal to Name");

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

        result.HasMetadata.Should().BeTrue("that mean the item was found");
        Person person = result.Item;
        person.Should().NotBeNull("that mean the person was found");
        person.Name.Should().Be("Джеки Чан", "this is the person's name");
        person.OriginalTitle.Should().Be("Jackie Chan", "this is the original name of the person");
        person.Overview.Should().Be("Настоящее имя — Чань Кунсан (Chan Kongsang), что означает &#171;Чан, рожденный в Гонконге&#187;.\nЧан делает самостоятельно большую часть трюков, а также иногда дублирует других актёров; он неоднократно получал травмы (во время финальных титров в его фильмах обычно демонстрируются неудавшиеся дубли), поэтому Чан внесён в чёрные списки страховых компаний по всему миру.\nУ Чана есть дочь Этта Нг (род. 19 ноября 1999) от внебрачной связи с актрисой Элейн Нг.\nЕго родители — Чарльз Чан и Ли-Ли Чан — бежали в Гонконг с континента во время гражданской войны, а в 1960 году перебрались в Австралию.\nВ возрасте 6 лет Чан был отдан в школу пекинской оперы в Гонконге.\nДважды попадал в Книгу рекордов Гиннесса за «наибольшее число трюков в карьере» и «самое большое число упоминаний в титрах одного фильма». Так, в фильме «<a href=\"/film/654749/\" class=\"all\">Доспехи Бога 3: Миссия Зодиак</a>» Джеки был задействован на 15 должностях (актёр, режиссёр, сценарист, продюсер, оператор, осветитель, постановщик трюков, исполнитель песни, ответственный за питание съёмочной группы и др.), тем самым побив рекорд <a href=\"/name/30966/\" class=\"all\">Роберта Родригеса</a>.", "this is person's Overview");
        person.PremiereDate.Should().NotBeNull("person should have a birthday date");
        person.PremiereDate!.Value.DateTime.Should().HaveYear(1954).And.HaveMonth(04).And.HaveDay(07);
        person.ProductionLocations.Should().ContainSingle();
        person.GetProviderId(Plugin.PluginKey).Should().Be("29855", "id of the requested item");
        person.SortName.Should().Be(person.Name, "SortName should be equal to Name");

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

        result.HasMetadata.Should().BeTrue("that mean the item was found");
        Person person = result.Item;
        person.Should().NotBeNull("that mean the person was found");
        person.Name.Should().Be("Джеки Чан", "this is the person's name");
        person.OriginalTitle.Should().Be("Jackie Chan", "this is the original name of the person");
        person.Overview.Should().Be("Настоящее имя — Чань Кунсан (Chan Kongsang), что означает &#171;Чан, рожденный в Гонконге&#187;.\nЧан делает самостоятельно большую часть трюков, а также иногда дублирует других актёров; он неоднократно получал травмы (во время финальных титров в его фильмах обычно демонстрируются неудавшиеся дубли), поэтому Чан внесён в чёрные списки страховых компаний по всему миру.\nУ Чана есть дочь Этта Нг (род. 19 ноября 1999) от внебрачной связи с актрисой Элейн Нг.\nЕго родители — Чарльз Чан и Ли-Ли Чан — бежали в Гонконг с континента во время гражданской войны, а в 1960 году перебрались в Австралию.\nВ возрасте 6 лет Чан был отдан в школу пекинской оперы в Гонконге.\nДважды попадал в Книгу рекордов Гиннесса за «наибольшее число трюков в карьере» и «самое большое число упоминаний в титрах одного фильма». Так, в фильме «<a href=\"/film/654749/\" class=\"all\">Доспехи Бога 3: Миссия Зодиак</a>» Джеки был задействован на 15 должностях (актёр, режиссёр, сценарист, продюсер, оператор, осветитель, постановщик трюков, исполнитель песни, ответственный за питание съёмочной группы и др.), тем самым побив рекорд <a href=\"/name/30966/\" class=\"all\">Роберта Родригеса</a>.", "this is person's Overview");
        person.PremiereDate.Should().NotBeNull("person should have a birthday date");
        person.PremiereDate!.Value.DateTime.Should().HaveYear(1954).And.HaveMonth(04).And.HaveDay(07);
        person.ProductionLocations.Should().ContainSingle();
        person.GetProviderId(Plugin.PluginKey).Should().Be("29855", "id of the requested item");
        person.SortName.Should().Be(person.Name, "SortName should be equal to Name");

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
        result.Should().ContainSingle();
        RemoteSearchResult person = result.First();
        person.Should().NotBeNull("that mean the person was found");
        person.GetProviderId(Plugin.PluginKey).Should().Be("29855", "id of the requested item");
        person.Name.Should().Be("Джеки Чан", "this is the name of the person");
        person.ImageUrl.Should().NotBeNullOrWhiteSpace("person image exists");
        person.SearchProviderName.Should().Be(Plugin.PluginKey, "this is person's SearchProviderName");

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
        result.Should().ContainSingle();
        RemoteSearchResult person = result.First();
        person.Should().NotBeNull("that mean the person was found");
        person.GetProviderId(Plugin.PluginKey).Should().Be("29855", "id of the requested item");
        person.Name.Should().Be("Джеки Чан", "this is the name of the person");
        person.ImageUrl.Should().NotBeNullOrWhiteSpace("person image exists");

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
        result.Should().ContainSingle();
        RemoteSearchResult person = result.First();
        person.Should().NotBeNull("that mean the person was found");
        person.GetProviderId(Plugin.PluginKey).Should().Be("29855", "id of the requested item");
        person.Name.Should().Be("Джеки Чан", "this is the name of the person");
        person.ImageUrl.Should().NotBeNullOrWhiteSpace("person image exists");
        person.SearchProviderName.Should().Be(Plugin.PluginKey, "this is person's SearchProviderName");

        _logManager.Verify(lm => lm.GetLogger(It.IsAny<string>()), Times.Exactly(4));
        _applicationPaths.VerifyGet(ap => ap.PluginConfigurationsPath, Times.Once());
        _xmlSerializer.Verify(xs => xs.DeserializeFromFile(typeof(PluginConfiguration), "KpPersonProvider_GetSearchResults_ByEnName/EmbyKinopoiskRu.xml"), Times.Once());
        VerifyNoOtherCalls();

        Logger.Info($"Finish '{nameof(KpPersonProvider_GetSearchResults_ByEnName)}'");
    }



}
