using System.Net;

using EmbyKinopoiskRu.Configuration;
using EmbyKinopoiskRu.Provider.RemoteMetadata;

using FluentAssertions;

using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Providers;

namespace EmbyKinopoiskRu.Tests.KinopoiskApiUnofficial;

[Collection("Sequential")]
public class KpSeriesProviderTest : BaseTest
{
    private static readonly NLog.ILogger Logger = NLog.LogManager.GetLogger(nameof(KpSeriesProviderTest));

    private readonly KpSeriesProvider _kpSeriesProvider;


    #region Test configs
    public KpSeriesProviderTest() : base(Logger)
    {
        _pluginConfiguration.Token = GetKinopoiskUnofficialToken();
        _pluginConfiguration.ApiType = PluginConfiguration.KinopoiskAPIUnofficialTech;

        ConfigLibraryManager();

        ConfigXmlSerializer();

        _kpSeriesProvider = new(_httpClient, _logManager.Object);
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
    public async void KpSeriesProvider_ForCodeCoverage()
    {
        Logger.Info($"Start '{nameof(KpSeriesProvider_ForCodeCoverage)}'");

        _kpSeriesProvider.Name.Should().NotBeNull("name is hardcoded");

        HttpResponseInfo response = await _kpSeriesProvider.GetImageResponse("https://www.google.com", CancellationToken.None);
        response.StatusCode.Should().Be(HttpStatusCode.OK, "this is status code of the response to google.com");

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
            .Returns("UN_KpSeriesProvider_GetMetadata_Provider_Kp");

        var seriesInfo = new SeriesInfo()
        {
            ProviderIds = new(new() { { Plugin.PluginKey, "452973" } })
        };
        using var cancellationTokenSource = new CancellationTokenSource();
        MetadataResult<Series> result = await _kpSeriesProvider.GetMetadata(seriesInfo, cancellationTokenSource.Token);

        result.HasMetadata.Should().BeTrue("that mean the item was found");
        Series series = result.Item;
        series.Should().NotBeNull("that mean the series was found");
        series.CommunityRating.Should().BeGreaterThan(8, "such value received from API");
        series.ExternalId.Should().Be("452973", "KP id of requested item");
        series.Genres.Should().HaveCount(6);
        series.IsFolder.Should().BeTrue("series organized as a folder");
        series.Name.Should().Be("Гуррен-Лаганн", "this is the series's name");
        series.OriginalTitle.Should().Be("Tengen toppa gurren lagann", "this is the original name of the series");
        series.Overview.Should().Be("Сотни лет люди живут в глубоких пещерах, в постоянном страхе перед землетрясениями и обвалами. В одной из таких подземных деревень живет мальчик Симон и его духовный наставник — парень Камина. Камина верит, что наверху есть другой мир, без стен и потолков, его мечта — попасть туда.\n\nНо мечты остаются пустыми фантазиями, пока в один прекрасный день Симон случайно не находит сверло, оказавшееся ключом от странного железного лица в толще земли. В этот же день потолок пещеры рушится. Так начинается приключение Симона, Камины и их компаньонов в новом мире под открытым небом огромной вселенной.", "this is series's Overview");
        series.ProductionLocations.Should().ContainSingle();
        series.ProductionYear.Should().Be(2007, "this is series ProductionYear");
        series.GetProviderId(Plugin.PluginKey).Should().Be("452973", "id of the requested item");
        series.GetProviderId(MetadataProviders.Imdb).Should().Be("tt0948103", "IMDB id of the requested item");
        series.RemoteTrailers.Length.Should().Be(2, "the series has RemoteTrailers");
        series.SortName.Should().Be(series.Name, "SortName should be equal to Name");
        series.Studios.Should().BeEmpty("the series doesn't have Studios");
        series.Tagline.Should().Be("Pierce through tragedy and fight toward victory, Gurren Lagann!", "this is a Tagline of the series");

        result.People.Should().HaveCount(71);
        PersonInfo? person = result.People.FirstOrDefault(p => "Марина Иноуэ".Equals(p.Name, StringComparison.Ordinal));
        person.Should().NotBeNull("that mean the person was found");
        person.GetProviderId(Plugin.PluginKey).Should().Be("1202776", "id of the requested item");
        person!.Role.Should().Be("Yoko Littner, озвучка", "this is person's Role");
        person.Name.Should().Be("Марина Иноуэ", "this is the person's name");
        person.ImageUrl.Should().NotBeNullOrWhiteSpace("person image exists");

        _logManager.Verify(lm => lm.GetLogger(It.IsAny<string>()), Times.Exactly(4));
        _applicationPaths.VerifyGet(ap => ap.PluginConfigurationsPath, Times.Once());
        _xmlSerializer.Verify(xs => xs.DeserializeFromFile(typeof(PluginConfiguration), "UN_KpSeriesProvider_GetMetadata_Provider_Kp/EmbyKinopoiskRu.xml"), Times.Once());
        _localizationManager.Verify(lm => lm.RemoveDiacritics("Гуррен-Лаганн"), Times.Once());
        VerifyNoOtherCalls();

        Logger.Info($"Finished '{nameof(KpSeriesProvider_GetMetadata_Provider_Kp)}'");
    }

    [Fact]
    public async void KpSeriesProvider_GetMetadata_NameAndYear()
    {
        Logger.Info($"Start '{nameof(KpSeriesProvider_GetMetadata_NameAndYear)}'");

        _ = _applicationPaths
            .SetupGet(m => m.PluginConfigurationsPath)
            .Returns("UN_KpSeriesProvider_GetMetadata_NameAndYear");

        var seriesInfo = new SeriesInfo()
        {
            Name = "Гуррен-Лаганн",
            Year = 2007
        };
        using var cancellationTokenSource = new CancellationTokenSource();
        MetadataResult<Series> result = await _kpSeriesProvider.GetMetadata(seriesInfo, cancellationTokenSource.Token);

        result.HasMetadata.Should().BeTrue("that mean the item was found");
        Series series = result.Item;
        series.Should().NotBeNull("that mean the series was found");
        series.CommunityRating.Should().BeGreaterThan(8, "such value received from API");
        series.ExternalId.Should().Be("452973", "KP id of requested item");
        series.Genres.Should().HaveCount(6);
        series.IsFolder.Should().BeTrue("series organized as a folder");
        series.Name.Should().Be("Гуррен-Лаганн", "this is the series's name");
        series.OriginalTitle.Should().Be("Tengen toppa gurren lagann", "this is the original name of the series");
        series.Overview.Should().Be("Сотни лет люди живут в глубоких пещерах, в постоянном страхе перед землетрясениями и обвалами. В одной из таких подземных деревень живет мальчик Симон и его духовный наставник — парень Камина. Камина верит, что наверху есть другой мир, без стен и потолков, его мечта — попасть туда.\n\nНо мечты остаются пустыми фантазиями, пока в один прекрасный день Симон случайно не находит сверло, оказавшееся ключом от странного железного лица в толще земли. В этот же день потолок пещеры рушится. Так начинается приключение Симона, Камины и их компаньонов в новом мире под открытым небом огромной вселенной.", "this is series's Overview");
        series.ProductionLocations.Should().ContainSingle();
        series.ProductionYear.Should().Be(2007, "this is series ProductionYear");
        series.GetProviderId(Plugin.PluginKey).Should().Be("452973", "id of the requested item");
        series.GetProviderId(MetadataProviders.Imdb).Should().Be("tt0948103", "IMDB id of the requested item");
        series.RemoteTrailers.Length.Should().Be(2, "the series has RemoteTrailers");
        series.SortName.Should().Be(series.Name, "SortName should be equal to Name");
        series.Studios.Should().BeEmpty("the series doesn't have Studios");
        series.Tagline.Should().Be("Pierce through tragedy and fight toward victory, Gurren Lagann!", "this is a Tagline of the series");

        result.People.Should().HaveCount(71);
        PersonInfo? person = result.People.FirstOrDefault(p => "Марина Иноуэ".Equals(p.Name, StringComparison.Ordinal));
        person.Should().NotBeNull("that mean the person was found");
        person.GetProviderId(Plugin.PluginKey).Should().Be("1202776", "id of the requested item");
        person!.Role.Should().Be("Yoko Littner, озвучка", "this is person's Role");
        person.Name.Should().Be("Марина Иноуэ", "this is the person's name");
        person.ImageUrl.Should().NotBeNullOrWhiteSpace("person image exists");

        _logManager.Verify(lm => lm.GetLogger(It.IsAny<string>()), Times.Exactly(4));
        _applicationPaths.VerifyGet(ap => ap.PluginConfigurationsPath, Times.Once());
        _xmlSerializer.Verify(xs => xs.DeserializeFromFile(typeof(PluginConfiguration), "UN_KpSeriesProvider_GetMetadata_NameAndYear/EmbyKinopoiskRu.xml"), Times.Once());
        _localizationManager.Verify(lm => lm.RemoveDiacritics("Гуррен-Лаганн"), Times.Once());
        VerifyNoOtherCalls();

        Logger.Info($"Finished '{nameof(KpSeriesProvider_GetMetadata_NameAndYear)}'");
    }

    [Fact]
    public async void KpSeriesProvider_GetSearchResults_Provider_Kp()
    {
        Logger.Info($"Start '{nameof(KpSeriesProvider_GetSearchResults_Provider_Kp)}'");

        _ = _applicationPaths
            .SetupGet(m => m.PluginConfigurationsPath)
            .Returns("UN_KpSeriesProvider_GetSearchResults_Provider_Kp");

        var seriesInfo = new SeriesInfo()
        {
            ProviderIds = new(new() { { Plugin.PluginKey, "452973" } })
        };
        using var cancellationTokenSource = new CancellationTokenSource();
        IEnumerable<RemoteSearchResult> result = await _kpSeriesProvider.GetSearchResults(seriesInfo, cancellationTokenSource.Token);
        result.Should().ContainSingle();
        RemoteSearchResult series = result.First();
        series.Should().NotBeNull("that mean the series was found");
        series.GetProviderId(Plugin.PluginKey).Should().Be("452973", "id of the requested item");
        series.GetProviderId(MetadataProviders.Imdb).Should().Be("tt0948103", "IMDB id of the requested item");
        series.Name.Should().Be("Гуррен-Лаганн", "this is the name of the person");
        series.ImageUrl.Should().NotBeNullOrWhiteSpace("series image exists");
        series.ProductionYear.Should().Be(2007, "this is series ProductionYear");
        series.SearchProviderName.Should().Be(Plugin.PluginKey, "this is series's SearchProviderName");
        series.Overview.Should().Be("Сотни лет люди живут в глубоких пещерах, в постоянном страхе перед землетрясениями и обвалами. В одной из таких подземных деревень живет мальчик Симон и его духовный наставник — парень Камина. Камина верит, что наверху есть другой мир, без стен и потолков, его мечта — попасть туда.\n\nНо мечты остаются пустыми фантазиями, пока в один прекрасный день Симон случайно не находит сверло, оказавшееся ключом от странного железного лица в толще земли. В этот же день потолок пещеры рушится. Так начинается приключение Симона, Камины и их компаньонов в новом мире под открытым небом огромной вселенной.", "this is series's Overview");

        _logManager.Verify(lm => lm.GetLogger(It.IsAny<string>()), Times.Exactly(4));
        _applicationPaths.VerifyGet(ap => ap.PluginConfigurationsPath, Times.Once());
        _xmlSerializer.Verify(xs => xs.DeserializeFromFile(typeof(PluginConfiguration), "UN_KpSeriesProvider_GetSearchResults_Provider_Kp/EmbyKinopoiskRu.xml"), Times.Once());
        VerifyNoOtherCalls();

        Logger.Info($"Finished '{nameof(KpSeriesProvider_GetSearchResults_Provider_Kp)}'");
    }

    [Fact]
    public async void KpSeriesProvider_GetSearchResults_NameAndYear()
    {
        Logger.Info($"Start '{nameof(KpSeriesProvider_GetSearchResults_NameAndYear)}'");

        _ = _applicationPaths
            .SetupGet(m => m.PluginConfigurationsPath)
            .Returns("UN_KpSeriesProvider_GetSearchResults_NameAndYear");

        var seriesInfo = new SeriesInfo()
        {
            Name = "Гуррен-Лаганн",
            Year = 2007
        };
        using var cancellationTokenSource = new CancellationTokenSource();
        IEnumerable<RemoteSearchResult> result = await _kpSeriesProvider.GetSearchResults(seriesInfo, cancellationTokenSource.Token);
        result.Should().ContainSingle();
        RemoteSearchResult series = result.First();
        series.Should().NotBeNull("that mean the series was found");
        series.GetProviderId(Plugin.PluginKey).Should().Be("452973", "id of the requested item");
        series.GetProviderId(MetadataProviders.Imdb).Should().Be("tt0948103", "IMDB id of the requested item");
        series.Name.Should().Be("Гуррен-Лаганн", "this is the name of the person");
        series.ImageUrl.Should().NotBeNullOrWhiteSpace("series image exists");
        series.ProductionYear.Should().Be(2007, "this is series ProductionYear");
        series.SearchProviderName.Should().Be(Plugin.PluginKey, "this is series's SearchProviderName");

        _logManager.Verify(lm => lm.GetLogger(It.IsAny<string>()), Times.Exactly(4));
        _applicationPaths.VerifyGet(ap => ap.PluginConfigurationsPath, Times.Once());
        _xmlSerializer.Verify(xs => xs.DeserializeFromFile(typeof(PluginConfiguration), "UN_KpSeriesProvider_GetSearchResults_NameAndYear/EmbyKinopoiskRu.xml"), Times.Once());
        VerifyNoOtherCalls();

        Logger.Info($"Finished '{nameof(KpSeriesProvider_GetSearchResults_NameAndYear)}'");
    }

}
