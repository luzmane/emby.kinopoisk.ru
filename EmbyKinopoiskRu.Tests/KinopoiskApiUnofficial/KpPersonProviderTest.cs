using System.Net;

using EmbyKinopoiskRu.Configuration;
using EmbyKinopoiskRu.Provider.RemoteMetadata;

using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Providers;

namespace EmbyKinopoiskRu.Tests.KinopoiskApiUnofficial;

[Collection("Sequential")]
public class KpPersonProviderTest : BaseTest
{
    private static readonly NLog.ILogger Logger = NLog.LogManager.GetLogger(nameof(KpPersonProviderTest));

    private readonly KpPersonProvider _kpPersonProvider;


    #region Test configs
    public KpPersonProviderTest() : base(Logger)
    {
        _pluginConfiguration.Token = GetKinopoiskUnofficialToken();
        _pluginConfiguration.ApiType = PluginConfiguration.KinopoiskAPIUnofficialTech;

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
            .Returns("UN_KpPersonProvider_GetMetadata_Provider_Kp");

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
        Assert.Equal("В возрасте 6 лет Чан был отдан в школу пекинской оперы в Гонконге.\nДважды попадал в Книгу рекордов Гиннесса за «наибольшее число трюков в карьере» и «самое большое число упоминаний в титрах одного фильма». Так, в фильме «Доспехи Бога 3: Миссия Зодиак» Джеки был задействован на 15 должностях (актёр, режиссёр, сценарист, продюсер, оператор, осветитель, постановщик трюков, исполнитель песни, ответственный за питание съёмочной группы и др.), тем самым побив рекорд Роберта Родригеса.\nНастоящее имя - Чань Кунсан (Chan Kongsang), что означает «Чан, рожденный в Гонконге».\nЕго родители — Чарльз Чан и Ли-Ли Чан — бежали в Гонконг с континента во время гражданской войны, а в 1960 году перебрались в Австралию.\nЧан делает самостоятельно большую часть трюков, а также иногда дублирует других актёров; он неоднократно получал травмы (во время финальных титров в его фильмах обычно демонстрируются неудавшиеся дубли), поэтому Чан внесён в чёрные списки страховых компаний по всему миру.\nУ Чана есть дочь Этта Нг (род. 19 ноября 1999) от внебрачной связи с актрисой Элейн Нг.", person.Overview);
        Assert.NotNull(person.PremiereDate);
        Assert.Equal(new DateTime(1954, 04, 07), person.PremiereDate.Value.DateTime, new DateTimeEqualityComparer());
        var place = Assert.Single(person.ProductionLocations);
        Assert.Equal("29855", person.GetProviderId(Plugin.PluginKey));
        Assert.Equal(person.Name, person.SortName);

        _logManager.Verify(lm => lm.GetLogger(It.IsAny<string>()), Times.Exactly(4));
        _applicationPaths.VerifyGet(ap => ap.PluginConfigurationsPath, Times.Once());
        _xmlSerializer.Verify(xs => xs.DeserializeFromFile(typeof(PluginConfiguration), "UN_KpPersonProvider_GetMetadata_Provider_Kp/EmbyKinopoiskRu.xml"), Times.Once());
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
            .Returns("UN_KpPersonProvider_GetMetadata_ByName");

        var personInfo = new PersonLookupInfo()
        {
            Name = "Милла Йовович"
        };
        using var cancellationTokenSource = new CancellationTokenSource();
        MetadataResult<Person> result = await _kpPersonProvider.GetMetadata(personInfo, cancellationTokenSource.Token);

        Assert.True(result.HasMetadata);
        Person person = result.Item;
        Assert.NotNull(person);
        Assert.Equal("Милла Йовович", person.Name);
        Assert.Equal("Milla Jovovich", person.OriginalTitle);
        Assert.Equal("В своём твиттере Йовович рассказала, что по рождению она является православной христианкой, но может ходить «в любую церковь, где будут присутствовать любовь и одухотворение».\nХотя наиболее распространенным в мире вариантом произношения имени и фамилии актрисы является «Mil-ah Jo-vo-vich» (Мил-а Джо-во-вич), правильный вариант - «Mee-lah Yoh-ve-vich» (Ми-ла Йо-вэ-вич).\nПервый брак Миллы с актёром Шоном Эндрюсом был аннулирован матерью актрисы, поскольку Милле было на тот момент всего 16 лет. Брачный союз, заключенный в Лас-Вегасе, продлился с октября по ноябрь 1992 года.\nВ 1994 году Милла выпустила музыкальный компакт-диск «Божественная комедия» (The Divine Comedy).\nСвободно говорит на русском, сербском и французском языках.\nОтец Миллы - сербский врач-педиатр, который переехал из Белграда в СССР и там женился на ее матери.\nМилла - левша.\nМама Миллы - советская актриса Галина Логинова, снимавшаяся вместе с Константином Райкиным и Татьяной Веденеевой.", person.Overview);
        Assert.NotNull(person.PremiereDate);
        Assert.Equal(new DateTime(1975, 12, 17), person.PremiereDate.Value.DateTime, new DateTimeEqualityComparer());
        var place = Assert.Single(person.ProductionLocations);
        Assert.Equal("24507", person.GetProviderId(Plugin.PluginKey));
        Assert.Equal(person.Name, person.SortName);

        _logManager.Verify(lm => lm.GetLogger(It.IsAny<string>()), Times.Exactly(4));
        _applicationPaths.VerifyGet(ap => ap.PluginConfigurationsPath, Times.Once());
        _xmlSerializer.Verify(xs => xs.DeserializeFromFile(typeof(PluginConfiguration), "UN_KpPersonProvider_GetMetadata_ByName/EmbyKinopoiskRu.xml"), Times.Once());
        _localizationManager.Verify(lm => lm.RemoveDiacritics("Милла Йовович"), Times.Once());
        VerifyNoOtherCalls();

        Logger.Info($"Finish '{nameof(KpPersonProvider_GetMetadata_ByName)}'");
    }

    [Fact]
    public async void KpPersonProvider_GetMetadata_ByEnName()
    {
        Logger.Info($"Start '{nameof(KpPersonProvider_GetMetadata_ByEnName)}'");

        _ = _applicationPaths
            .SetupGet(m => m.PluginConfigurationsPath)
            .Returns("UN_KpPersonProvider_GetMetadata_ByEnName");

        var personInfo = new PersonLookupInfo()
        {
            Name = "Milla Jovovich"
        };
        using var cancellationTokenSource = new CancellationTokenSource();
        MetadataResult<Person> result = await _kpPersonProvider.GetMetadata(personInfo, cancellationTokenSource.Token);

        Assert.True(result.HasMetadata);
        Person person = result.Item;
        Assert.NotNull(person);
        Assert.Equal("Милла Йовович", person.Name);
        Assert.Equal("Milla Jovovich", person.OriginalTitle);
        Assert.Equal("В своём твиттере Йовович рассказала, что по рождению она является православной христианкой, но может ходить «в любую церковь, где будут присутствовать любовь и одухотворение».\nХотя наиболее распространенным в мире вариантом произношения имени и фамилии актрисы является «Mil-ah Jo-vo-vich» (Мил-а Джо-во-вич), правильный вариант - «Mee-lah Yoh-ve-vich» (Ми-ла Йо-вэ-вич).\nПервый брак Миллы с актёром Шоном Эндрюсом был аннулирован матерью актрисы, поскольку Милле было на тот момент всего 16 лет. Брачный союз, заключенный в Лас-Вегасе, продлился с октября по ноябрь 1992 года.\nВ 1994 году Милла выпустила музыкальный компакт-диск «Божественная комедия» (The Divine Comedy).\nСвободно говорит на русском, сербском и французском языках.\nОтец Миллы - сербский врач-педиатр, который переехал из Белграда в СССР и там женился на ее матери.\nМилла - левша.\nМама Миллы - советская актриса Галина Логинова, снимавшаяся вместе с Константином Райкиным и Татьяной Веденеевой.", person.Overview);
        Assert.NotNull(person.PremiereDate);
        Assert.Equal(new DateTime(1975, 12, 17), person.PremiereDate.Value.DateTime, new DateTimeEqualityComparer());
        var place = Assert.Single(person.ProductionLocations);
        Assert.Equal("24507", person.GetProviderId(Plugin.PluginKey));
        Assert.Equal(person.Name, person.SortName);

        _logManager.Verify(lm => lm.GetLogger(It.IsAny<string>()), Times.Exactly(4));
        _applicationPaths.VerifyGet(ap => ap.PluginConfigurationsPath, Times.Once());
        _xmlSerializer.Verify(xs => xs.DeserializeFromFile(typeof(PluginConfiguration), "UN_KpPersonProvider_GetMetadata_ByEnName/EmbyKinopoiskRu.xml"), Times.Once());
        _localizationManager.Verify(lm => lm.RemoveDiacritics("Милла Йовович"), Times.Once());
        VerifyNoOtherCalls();

        Logger.Info($"Finish '{nameof(KpPersonProvider_GetMetadata_ByEnName)}'");
    }

    [Fact]
    public async void KpPersonProvider_GetSearchResults_Provider_Kp()
    {
        Logger.Info($"Start '{nameof(KpPersonProvider_GetSearchResults_Provider_Kp)}'");

        _ = _applicationPaths
            .SetupGet(m => m.PluginConfigurationsPath)
            .Returns("UN_KpPersonProvider_GetSearchResults_Provider_Kp");

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
        _xmlSerializer.Verify(xs => xs.DeserializeFromFile(typeof(PluginConfiguration), "UN_KpPersonProvider_GetSearchResults_Provider_Kp/EmbyKinopoiskRu.xml"), Times.Once());
        VerifyNoOtherCalls();

        Logger.Info($"Finish '{nameof(KpPersonProvider_GetSearchResults_Provider_Kp)}'");
    }

    [Fact]
    public async void KpPersonProvider_GetSearchResults_ByName()
    {
        Logger.Info($"Start '{nameof(KpPersonProvider_GetSearchResults_ByName)}'");

        _ = _applicationPaths
            .SetupGet(m => m.PluginConfigurationsPath)
            .Returns("UN_KpPersonProvider_GetSearchResults_ByName");

        var personInfo = new PersonLookupInfo()
        {
            Name = "Джеки Чан"
        };
        using var cancellationTokenSource = new CancellationTokenSource();
        IEnumerable<RemoteSearchResult> result = await _kpPersonProvider.GetSearchResults(personInfo, cancellationTokenSource.Token);

        Assert.NotEmpty(result);
        RemoteSearchResult? searchResult = result
            .FirstOrDefault(
                r => "29855".Equals(r.ProviderIds[Plugin.PluginKey], StringComparison.Ordinal));
        Assert.NotNull(searchResult);
        Assert.Equal("29855", searchResult.GetProviderId(Plugin.PluginKey));
        Assert.Equal("Джеки Чан", searchResult.Name);
        Assert.True(!string.IsNullOrWhiteSpace(searchResult.ImageUrl));
        Assert.Equal(Plugin.PluginKey, searchResult.SearchProviderName);

        _logManager.Verify(lm => lm.GetLogger(It.IsAny<string>()), Times.Exactly(4));
        _applicationPaths.VerifyGet(ap => ap.PluginConfigurationsPath, Times.Once());
        _xmlSerializer.Verify(xs => xs.DeserializeFromFile(typeof(PluginConfiguration), "UN_KpPersonProvider_GetSearchResults_ByName/EmbyKinopoiskRu.xml"), Times.Once());
        VerifyNoOtherCalls();

        Logger.Info($"Finish '{nameof(KpPersonProvider_GetSearchResults_ByName)}'");
    }

    [Fact]
    public async void KpPersonProvider_GetSearchResults_ByEnName()
    {
        Logger.Info($"Start '{nameof(KpPersonProvider_GetSearchResults_ByEnName)}'");

        _ = _applicationPaths
            .SetupGet(m => m.PluginConfigurationsPath)
            .Returns("UN_KpPersonProvider_GetSearchResults_ByEnName");

        var personInfo = new PersonLookupInfo()
        {
            Name = "Jackie Chan"
        };
        using var cancellationTokenSource = new CancellationTokenSource();
        IEnumerable<RemoteSearchResult> result = await _kpPersonProvider.GetSearchResults(personInfo, cancellationTokenSource.Token);

        Assert.NotEmpty(result);
        RemoteSearchResult? searchResult = result
            .FirstOrDefault(
                r => "29855".Equals(r.ProviderIds[Plugin.PluginKey], StringComparison.Ordinal));
        Assert.NotNull(searchResult);
        Assert.Equal("29855", searchResult.GetProviderId(Plugin.PluginKey));
        Assert.Equal("Джеки Чан", searchResult.Name);
        Assert.True(!string.IsNullOrWhiteSpace(searchResult.ImageUrl));
        Assert.Equal(Plugin.PluginKey, searchResult.SearchProviderName);

        _logManager.Verify(lm => lm.GetLogger(It.IsAny<string>()), Times.Exactly(4));
        _applicationPaths.VerifyGet(ap => ap.PluginConfigurationsPath, Times.Once());
        _xmlSerializer.Verify(xs => xs.DeserializeFromFile(typeof(PluginConfiguration), "UN_KpPersonProvider_GetSearchResults_ByEnName/EmbyKinopoiskRu.xml"), Times.Once());
        VerifyNoOtherCalls();

        Logger.Info($"Finish '{nameof(KpPersonProvider_GetSearchResults_ByEnName)}'");
    }

}