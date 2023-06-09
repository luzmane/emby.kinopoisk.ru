using System.Net;

using EmbyKinopoiskRu.Configuration;
using EmbyKinopoiskRu.Provider.RemoteMetadata;

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

        Assert.NotNull(_kpSeriesProvider.Name);

        HttpResponseInfo response = await _kpSeriesProvider.GetImageResponse("https://www.google.com", CancellationToken.None);
        Assert.True(response.StatusCode == HttpStatusCode.OK);

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

        Series series = result.Item;
        Assert.NotNull(series);
        Assert.True(result.HasMetadata);
        Assert.True(8 < series.CommunityRating);
        Assert.Equal("452973", series.ExternalId);
        Assert.Equal(6, series.Genres.Length);
        Assert.True(series.IsFolder);
        Assert.Equal("Гуррен-Лаганн", series.Name);
        Assert.Equal("Tengen toppa gurren lagann", series.OriginalTitle);
        Assert.Equal("Сотни лет люди живут в глубоких пещерах, в постоянном страхе перед землетрясениями и обвалами. В одной из таких подземных деревень живет мальчик Симон и его духовный наставник — парень Камина. Камина верит, что наверху есть другой мир, без стен и потолков, его мечта — попасть туда.\n\nНо мечты остаются пустыми фантазиями, пока в один прекрасный день Симон случайно не находит сверло, оказавшееся ключом от странного железного лица в толще земли. В этот же день потолок пещеры рушится. Так начинается приключение Симона, Камины и их компаньонов в новом мире под открытым небом огромной вселенной.", series.Overview);
        _ = Assert.Single(series.ProductionLocations);
        Assert.Equal(2007, series.ProductionYear);
        Assert.Equal("452973", series.GetProviderId(Plugin.PluginKey));
        Assert.Equal("tt0948103", series.GetProviderId(MetadataProviders.Imdb));
        Assert.Equal(2, series.RemoteTrailers.Length);
        Assert.Equal(series.Name, series.SortName);
        Assert.Empty(series.Studios);
        Assert.Equal("Pierce through tragedy and fight toward victory, Gurren Lagann!", series.Tagline);

        Assert.Equal(71, result.People.Count);
        PersonInfo? person = result.People.FirstOrDefault(p => "Марина Иноуэ".Equals(p.Name, StringComparison.Ordinal));
        Assert.NotNull(person);
        Assert.Equal("1202776", person.GetProviderId(Plugin.PluginKey));
        Assert.Equal("Yoko Littner, озвучка", person.Role);
        Assert.Equal("Марина Иноуэ", person.Name);
        Assert.True(!string.IsNullOrWhiteSpace(person.ImageUrl));

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

        Series series = result.Item;
        Assert.NotNull(series);
        Assert.True(result.HasMetadata);
        Assert.True(8 < series.CommunityRating);
        Assert.Equal("452973", series.ExternalId);
        Assert.Equal(6, series.Genres.Length);
        Assert.True(series.IsFolder);
        Assert.Equal("Гуррен-Лаганн", series.Name);
        Assert.Equal("Tengen toppa gurren lagann", series.OriginalTitle);
        Assert.Equal("Сотни лет люди живут в глубоких пещерах, в постоянном страхе перед землетрясениями и обвалами. В одной из таких подземных деревень живет мальчик Симон и его духовный наставник — парень Камина. Камина верит, что наверху есть другой мир, без стен и потолков, его мечта — попасть туда.\n\nНо мечты остаются пустыми фантазиями, пока в один прекрасный день Симон случайно не находит сверло, оказавшееся ключом от странного железного лица в толще земли. В этот же день потолок пещеры рушится. Так начинается приключение Симона, Камины и их компаньонов в новом мире под открытым небом огромной вселенной.", series.Overview);
        _ = Assert.Single(series.ProductionLocations);
        Assert.Equal(2007, series.ProductionYear);
        Assert.Equal("452973", series.GetProviderId(Plugin.PluginKey));
        Assert.Equal("tt0948103", series.GetProviderId(MetadataProviders.Imdb));
        Assert.Equal(2, series.RemoteTrailers.Length);
        Assert.Equal(series.Name, series.SortName);
        Assert.Empty(series.Studios);
        Assert.Equal("Pierce through tragedy and fight toward victory, Gurren Lagann!", series.Tagline);

        Assert.Equal(71, result.People.Count);
        PersonInfo? person = result.People.FirstOrDefault(p => "Марина Иноуэ".Equals(p.Name, StringComparison.Ordinal));
        Assert.NotNull(person);
        Assert.Equal("1202776", person.GetProviderId(Plugin.PluginKey));
        Assert.Equal("Yoko Littner, озвучка", person.Role);
        Assert.Equal("Марина Иноуэ", person.Name);
        Assert.True(!string.IsNullOrWhiteSpace(person.ImageUrl));

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

        RemoteSearchResult searchResult = Assert.Single(result);
        Assert.NotNull(searchResult);
        Assert.Equal("452973", searchResult.GetProviderId(Plugin.PluginKey));
        Assert.Equal("tt0948103", searchResult.GetProviderId(MetadataProviders.Imdb));
        Assert.Equal("Гуррен-Лаганн", searchResult.Name);
        Assert.True(!string.IsNullOrWhiteSpace(searchResult.ImageUrl));
        Assert.Equal(2007, searchResult.ProductionYear);
        Assert.Equal(Plugin.PluginKey, searchResult.SearchProviderName);
        Assert.Equal("Сотни лет люди живут в глубоких пещерах, в постоянном страхе перед землетрясениями и обвалами. В одной из таких подземных деревень живет мальчик Симон и его духовный наставник — парень Камина. Камина верит, что наверху есть другой мир, без стен и потолков, его мечта — попасть туда.\n\nНо мечты остаются пустыми фантазиями, пока в один прекрасный день Симон случайно не находит сверло, оказавшееся ключом от странного железного лица в толще земли. В этот же день потолок пещеры рушится. Так начинается приключение Симона, Камины и их компаньонов в новом мире под открытым небом огромной вселенной.", searchResult.Overview);

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

        RemoteSearchResult searchResult = Assert.Single(result);
        Assert.NotNull(searchResult);
        Assert.Equal("452973", searchResult.GetProviderId(Plugin.PluginKey));
        Assert.Equal("tt0948103", searchResult.GetProviderId(MetadataProviders.Imdb));
        Assert.Equal("Гуррен-Лаганн", searchResult.Name);
        Assert.True(!string.IsNullOrWhiteSpace(searchResult.ImageUrl));
        Assert.Equal(2007, searchResult.ProductionYear);
        Assert.Equal(Plugin.PluginKey, searchResult.SearchProviderName);

        _logManager.Verify(lm => lm.GetLogger(It.IsAny<string>()), Times.Exactly(4));
        _applicationPaths.VerifyGet(ap => ap.PluginConfigurationsPath, Times.Once());
        _xmlSerializer.Verify(xs => xs.DeserializeFromFile(typeof(PluginConfiguration), "UN_KpSeriesProvider_GetSearchResults_NameAndYear/EmbyKinopoiskRu.xml"), Times.Once());
        VerifyNoOtherCalls();

        Logger.Info($"Finished '{nameof(KpSeriesProvider_GetSearchResults_NameAndYear)}'");
    }

}