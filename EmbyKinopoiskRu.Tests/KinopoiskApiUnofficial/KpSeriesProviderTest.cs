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

public class KpSeriesProviderTest : BaseTest
{
    private static readonly NLog.ILogger Logger = NLog.LogManager.GetLogger(nameof(KpSeriesProviderTest));

    private readonly KpSeriesProvider _kpSeriesProvider;


    #region Test configs

    public KpSeriesProviderTest() : base(Logger)
    {
        _pluginConfiguration.Token = GetKinopoiskUnofficialToken();
        _pluginConfiguration.ApiType = PluginConfiguration.KinopoiskApiUnofficialTech;

        ConfigLibraryManager();

        ConfigXmlSerializer();

        _kpSeriesProvider = new KpSeriesProvider(_httpClient, _logManager.Object);
    }

    #endregion

    [Fact]
    public async Task UN_KpSeriesProvider_ForCodeCoverage()
    {
        Logger.Info($"Start '{nameof(UN_KpSeriesProvider_ForCodeCoverage)}'");

        _kpSeriesProvider.Name.Should().NotBeNull();

        HttpResponseInfo response = await _kpSeriesProvider.GetImageResponse("https://www.google.com", CancellationToken.None);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        _logManager.Verify(lm => lm.GetLogger("KpSeriesProvider"), Times.Once());
        _logManager.Verify(lm => lm.GetLogger("KinopoiskRu"), Times.Once());
        VerifyNoOtherCalls();

        Logger.Info($"Finished '{nameof(UN_KpSeriesProvider_ForCodeCoverage)}'");
    }

    [Fact]
    public async Task UN_KpSeriesProvider_GetMetadata_Provider_Kp()
    {
        Logger.Info($"Start '{nameof(UN_KpSeriesProvider_GetMetadata_Provider_Kp)}'");

        _ = _applicationPaths
            .SetupGet(m => m.PluginConfigurationsPath)
            .Returns(nameof(UN_KpSeriesProvider_GetMetadata_Provider_Kp));

        var seriesInfo = new SeriesInfo
        {
            ProviderIds = new ProviderIdDictionary(new Dictionary<string, string>
            {
                { Plugin.PluginKey, "452973" }
            })
        };
        using var cancellationTokenSource = new CancellationTokenSource();
        MetadataResult<Series> result = await _kpSeriesProvider.GetMetadata(seriesInfo, cancellationTokenSource.Token);

        result.HasMetadata.Should().BeTrue();
        Series series = result.Item;
        series.Should().NotBeNull();
        series.CommunityRating.Should().BeGreaterThan(8);
        series.ExternalId.Should().Be("452973");
        series.Genres.Should().HaveCount(6);
        series.IsFolder.Should().BeTrue();
        series.Name.Should().Be("Гуррен-Лаганн");
        series.OriginalTitle.Should().Be("Tengen toppa gurren lagann");
        series.Overview.Should().Be("Сотни лет люди живут в глубоких пещерах, в постоянном страхе перед землетрясениями и обвалами. В одной из таких подземных деревень живет мальчик Симон и его духовный наставник — парень Камина. Камина верит, что наверху есть другой мир, без стен и потолков, его мечта — попасть туда.\n\nНо мечты остаются пустыми фантазиями, пока в один прекрасный день Симон случайно не находит сверло, оказавшееся ключом от странного железного лица в толще земли. В этот же день потолок пещеры рушится. Так начинается приключение Симона, Камины и их компаньонов в новом мире под открытым небом огромной вселенной.");
        series.ProductionLocations.Should().ContainSingle();
        series.ProductionYear.Should().Be(2007);
        series.GetProviderId(Plugin.PluginKey).Should().Be("452973");
        series.GetProviderId(MetadataProviders.Imdb).Should().Be("tt0948103");
        series.RemoteTrailers.Length.Should().Be(2);
        series.SortName.Should().Be(series.Name);
        series.Studios.Should().BeEmpty();
        series.Tagline.Should().Be("Pierce through tragedy and fight toward victory, Gurren Lagann!");

        result.People.Should().HaveCount(71);
        PersonInfo? person = result.People.FirstOrDefault(p => "Марина Иноуэ".Equals(p.Name, StringComparison.Ordinal));
        person.Should().NotBeNull();
        person.GetProviderId(Plugin.PluginKey).Should().Be("1202776");
        person!.Role.Should().Be("Yoko Littner, озвучка");
        person.Name.Should().Be("Марина Иноуэ");
        person.ImageUrl.Should().NotBeNullOrWhiteSpace();

        _logManager.Verify(lm => lm.GetLogger(It.IsAny<string>()), Times.Exactly(4));
        _applicationPaths.VerifyGet(ap => ap.PluginConfigurationsPath, Times.Once());
        _xmlSerializer.Verify(xs => xs.DeserializeFromFile(typeof(PluginConfiguration), $"{nameof(UN_KpSeriesProvider_GetMetadata_Provider_Kp)}/EmbyKinopoiskRu.xml"), Times.Once());
        _localizationManager.Verify(lm => lm.RemoveDiacritics("Гуррен-Лаганн"), Times.Once());
        VerifyNoOtherCalls();

        Logger.Info($"Finished '{nameof(UN_KpSeriesProvider_GetMetadata_Provider_Kp)}'");
    }

    [Fact]
    public async Task UN_KpSeriesProvider_GetMetadata_NameAndYear()
    {
        Logger.Info($"Start '{nameof(UN_KpSeriesProvider_GetMetadata_NameAndYear)}'");

        _ = _applicationPaths
            .SetupGet(m => m.PluginConfigurationsPath)
            .Returns(nameof(UN_KpSeriesProvider_GetMetadata_NameAndYear));

        var seriesInfo = new SeriesInfo
        {
            Name = "Гуррен-Лаганн",
            Year = 2007
        };
        using var cancellationTokenSource = new CancellationTokenSource();
        MetadataResult<Series> result = await _kpSeriesProvider.GetMetadata(seriesInfo, cancellationTokenSource.Token);

        result.HasMetadata.Should().BeTrue();
        Series series = result.Item;
        series.Should().NotBeNull();
        series.CommunityRating.Should().BeGreaterThan(8);
        series.ExternalId.Should().Be("452973");
        series.Genres.Should().HaveCount(6);
        series.IsFolder.Should().BeTrue();
        series.Name.Should().Be("Гуррен-Лаганн");
        series.OriginalTitle.Should().Be("Tengen toppa gurren lagann");
        series.Overview.Should().Be("Сотни лет люди живут в глубоких пещерах, в постоянном страхе перед землетрясениями и обвалами. В одной из таких подземных деревень живет мальчик Симон и его духовный наставник — парень Камина. Камина верит, что наверху есть другой мир, без стен и потолков, его мечта — попасть туда.\n\nНо мечты остаются пустыми фантазиями, пока в один прекрасный день Симон случайно не находит сверло, оказавшееся ключом от странного железного лица в толще земли. В этот же день потолок пещеры рушится. Так начинается приключение Симона, Камины и их компаньонов в новом мире под открытым небом огромной вселенной.");
        series.ProductionLocations.Should().ContainSingle();
        series.ProductionYear.Should().Be(2007);
        series.GetProviderId(Plugin.PluginKey).Should().Be("452973");
        series.GetProviderId(MetadataProviders.Imdb).Should().Be("tt0948103");
        series.RemoteTrailers.Length.Should().Be(2);
        series.SortName.Should().Be(series.Name);
        series.Studios.Should().BeEmpty();
        series.Tagline.Should().Be("Pierce through tragedy and fight toward victory, Gurren Lagann!");

        result.People.Should().HaveCount(71);
        PersonInfo? person = result.People.FirstOrDefault(p => "Марина Иноуэ".Equals(p.Name, StringComparison.Ordinal));
        person.Should().NotBeNull();
        person.GetProviderId(Plugin.PluginKey).Should().Be("1202776");
        person!.Role.Should().Be("Yoko Littner, озвучка");
        person.Name.Should().Be("Марина Иноуэ");
        person.ImageUrl.Should().NotBeNullOrWhiteSpace();

        _logManager.Verify(lm => lm.GetLogger(It.IsAny<string>()), Times.Exactly(4));
        _applicationPaths.VerifyGet(ap => ap.PluginConfigurationsPath, Times.Once());
        _xmlSerializer.Verify(xs => xs.DeserializeFromFile(typeof(PluginConfiguration), $"{nameof(UN_KpSeriesProvider_GetMetadata_NameAndYear)}/EmbyKinopoiskRu.xml"), Times.Once());
        _localizationManager.Verify(lm => lm.RemoveDiacritics("Гуррен-Лаганн"), Times.Once());
        VerifyNoOtherCalls();

        Logger.Info($"Finished '{nameof(UN_KpSeriesProvider_GetMetadata_NameAndYear)}'");
    }

    [Fact]
    public async Task UN_KpSeriesProvider_GetSearchResults_Provider_Kp()
    {
        Logger.Info($"Start '{nameof(UN_KpSeriesProvider_GetSearchResults_Provider_Kp)}'");

        _ = _applicationPaths
            .SetupGet(m => m.PluginConfigurationsPath)
            .Returns(nameof(UN_KpSeriesProvider_GetSearchResults_Provider_Kp));

        var seriesInfo = new SeriesInfo
        {
            ProviderIds = new ProviderIdDictionary(new Dictionary<string, string>
            {
                { Plugin.PluginKey, "452973" }
            })
        };
        using var cancellationTokenSource = new CancellationTokenSource();
        IEnumerable<RemoteSearchResult> result = await _kpSeriesProvider.GetSearchResults(seriesInfo, cancellationTokenSource.Token);
        result.Should().ContainSingle();
        RemoteSearchResult series = result.First();
        series.Should().NotBeNull();
        series.GetProviderId(Plugin.PluginKey).Should().Be("452973");
        series.GetProviderId(MetadataProviders.Imdb).Should().Be("tt0948103");
        series.Name.Should().Be("Гуррен-Лаганн");
        series.ImageUrl.Should().NotBeNullOrWhiteSpace("series image exists");
        series.ProductionYear.Should().Be(2007);
        series.SearchProviderName.Should().Be(Plugin.PluginKey, "this is series's SearchProviderName");
        series.Overview.Should().Be("Сотни лет люди живут в глубоких пещерах, в постоянном страхе перед землетрясениями и обвалами. В одной из таких подземных деревень живет мальчик Симон и его духовный наставник — парень Камина. Камина верит, что наверху есть другой мир, без стен и потолков, его мечта — попасть туда.\n\nНо мечты остаются пустыми фантазиями, пока в один прекрасный день Симон случайно не находит сверло, оказавшееся ключом от странного железного лица в толще земли. В этот же день потолок пещеры рушится. Так начинается приключение Симона, Камины и их компаньонов в новом мире под открытым небом огромной вселенной.");

        _logManager.Verify(lm => lm.GetLogger(It.IsAny<string>()), Times.Exactly(4));
        _applicationPaths.VerifyGet(ap => ap.PluginConfigurationsPath, Times.Once());
        _xmlSerializer.Verify(xs => xs.DeserializeFromFile(typeof(PluginConfiguration), $"{nameof(UN_KpSeriesProvider_GetSearchResults_Provider_Kp)}/EmbyKinopoiskRu.xml"), Times.Once());
        VerifyNoOtherCalls();

        Logger.Info($"Finished '{nameof(UN_KpSeriesProvider_GetSearchResults_Provider_Kp)}'");
    }

    [Fact]
    public async Task UN_KpSeriesProvider_GetSearchResults_NameAndYear()
    {
        Logger.Info($"Start '{nameof(UN_KpSeriesProvider_GetSearchResults_NameAndYear)}'");

        _ = _applicationPaths
            .SetupGet(m => m.PluginConfigurationsPath)
            .Returns(nameof(UN_KpSeriesProvider_GetSearchResults_NameAndYear));

        var seriesInfo = new SeriesInfo
        {
            Name = "Гуррен-Лаганн",
            Year = 2007
        };
        using var cancellationTokenSource = new CancellationTokenSource();
        IEnumerable<RemoteSearchResult> result = await _kpSeriesProvider.GetSearchResults(seriesInfo, cancellationTokenSource.Token);
        result.Should().ContainSingle();
        RemoteSearchResult series = result.First();
        series.Should().NotBeNull();
        series.GetProviderId(Plugin.PluginKey).Should().Be("452973");
        series.GetProviderId(MetadataProviders.Imdb).Should().Be("tt0948103");
        series.Name.Should().Be("Гуррен-Лаганн");
        series.ImageUrl.Should().NotBeNullOrWhiteSpace("series image exists");
        series.ProductionYear.Should().Be(2007);
        series.SearchProviderName.Should().Be(Plugin.PluginKey, "this is series's SearchProviderName");

        _logManager.Verify(lm => lm.GetLogger(It.IsAny<string>()), Times.Exactly(4));
        _applicationPaths.VerifyGet(ap => ap.PluginConfigurationsPath, Times.Once());
        _xmlSerializer.Verify(xs => xs.DeserializeFromFile(typeof(PluginConfiguration), $"{nameof(UN_KpSeriesProvider_GetSearchResults_NameAndYear)}/EmbyKinopoiskRu.xml"), Times.Once());
        VerifyNoOtherCalls();

        Logger.Info($"Finished '{nameof(UN_KpSeriesProvider_GetSearchResults_NameAndYear)}'");
    }
}
