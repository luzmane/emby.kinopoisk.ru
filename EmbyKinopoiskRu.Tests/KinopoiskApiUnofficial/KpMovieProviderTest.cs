using System.Net;

using EmbyKinopoiskRu.Configuration;
using EmbyKinopoiskRu.Provider.RemoteMetadata;

using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Providers;


namespace EmbyKinopoiskRu.Tests.KinopoiskApiUnofficial;

[Collection("Sequential")]
public class KpMovieProviderTest : BaseTest
{
    private static readonly NLog.ILogger Logger = NLog.LogManager.GetLogger(nameof(KpMovieProviderTest));

    private readonly KpMovieProvider _kpMovieProvider;


    #region Test configs
    public KpMovieProviderTest() : base(Logger)
    {
        _pluginConfiguration.Token = GetKinopoiskUnofficialToken();
        _pluginConfiguration.ApiType = PluginConfiguration.KinopoiskAPIUnofficialTech;

        ConfigLibraryManager();

        ConfigXmlSerializer();

        _kpMovieProvider = new(_httpClient, _logManager.Object);
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
    public async void KpMovieProvider_ForCodeCoverage()
    {
        Logger.Info($"Start '{nameof(KpMovieProvider_ForCodeCoverage)}'");

        Assert.NotNull(_kpMovieProvider.Name);

        Assert.NotEmpty(_kpMovieProvider.Features);

        HttpResponseInfo response = await _kpMovieProvider.GetImageResponse("https://www.google.com", CancellationToken.None);
        Assert.True(response.StatusCode == HttpStatusCode.OK);

        _logManager.Verify(lm => lm.GetLogger("KpMovieProvider"), Times.Once());
        _logManager.Verify(lm => lm.GetLogger("KinopoiskRu"), Times.Once());
        VerifyNoOtherCalls();

        Logger.Info($"Finished '{nameof(KpMovieProvider_ForCodeCoverage)}'");
    }

    [Fact]
    public async void KpMovieProvider_GetMetadata_Provider_Kp()
    {
        Logger.Info($"Start '{nameof(KpMovieProvider_GetMetadata_Provider_Kp)}'");

        var movieInfo = new MovieInfo()
        {
            ProviderIds = new(new() { { Plugin.PluginKey, "326" } })
        };

        _ = _applicationPaths
            .SetupGet(m => m.PluginConfigurationsPath)
            .Returns("UN_KpMovieProvider_GetMetadata_Provider_Kp");

        using var cancellationTokenSource = new CancellationTokenSource();
        MetadataResult<Movie> result = await _kpMovieProvider.GetMetadata(movieInfo, cancellationTokenSource.Token);

        Movie movie = result.Item;
        Assert.NotNull(movie);
        Assert.True(result.HasMetadata);
        Assert.Equal("326", movie.GetProviderId(Plugin.PluginKey));
        Assert.Equal("tt0111161", movie.GetProviderId(MetadataProviders.Imdb));
        Assert.Equal("Video", movie.MediaType);
        Assert.True(5 < movie.CommunityRating);
        Assert.Equal("326", movie.ExternalId);
        _ = Assert.Single(movie.Genres);
        Assert.Equal("драма", movie.Genres[0]);
        Assert.Equal("Побег из Шоушенка", movie.Name);
        Assert.Equal("r", movie.OfficialRating);
        Assert.Equal("The Shawshank Redemption", movie.OriginalTitle);
        Assert.Equal("Бухгалтер Энди Дюфрейн обвинён в убийстве собственной жены и её любовника. Оказавшись в тюрьме под названием Шоушенк, он сталкивается с жестокостью и беззаконием, царящими по обе стороны решётки. Каждый, кто попадает в эти стены, становится их рабом до конца жизни. Но Энди, обладающий живым умом и доброй душой, находит подход как к заключённым, так и к охранникам, добиваясь их особого к себе расположения.", movie.Overview);
        Assert.Equal(1994, movie.ProductionYear);
        Assert.Equal(142, movie.Size);
        Assert.Equal(movie.Name, movie.SortName);
        Assert.Equal("Страх - это кандалы. Надежда - это свобода", movie.Tagline);

        Assert.Equal(80, result.People.Count);
        PersonInfo? person = result.People.FirstOrDefault(p => "Тим Роббинс".Equals(p.Name, StringComparison.Ordinal));
        Assert.NotNull(person);
        Assert.Equal("7987", person.GetProviderId(Plugin.PluginKey));
        Assert.Equal("Andy Dufresne", person.Role);
        Assert.Equal("Тим Роббинс", person.Name);
        Assert.True(!string.IsNullOrWhiteSpace(person.ImageUrl));

        _logManager.Verify(lm => lm.GetLogger(It.IsAny<string>()), Times.Exactly(4));
        _applicationPaths.VerifyGet(ap => ap.PluginConfigurationsPath, Times.Once());
        _xmlSerializer.Verify(xs => xs.DeserializeFromFile(typeof(PluginConfiguration), "UN_KpMovieProvider_GetMetadata_Provider_Kp/EmbyKinopoiskRu.xml"), Times.Once());
        _localizationManager.Verify(lm => lm.RemoveDiacritics("Побег из Шоушенка"), Times.Once());
        VerifyNoOtherCalls();

        Logger.Info($"Finished '{nameof(KpMovieProvider_GetMetadata_Provider_Kp)}'");
    }

    [Fact]
    public async void KpMovieProvider_GetMetadata_Provider_Imdb()
    {
        Logger.Info($"Start '{nameof(KpMovieProvider_GetMetadata_Provider_Imdb)}'");

        var movieInfo = new MovieInfo()
        {
            ProviderIds = new(new() { { MetadataProviders.Imdb.ToString(), "tt0111161" } })
        };

        _ = _applicationPaths
            .SetupGet(m => m.PluginConfigurationsPath)
            .Returns("UN_KpMovieProvider_GetMetadata_Provider_Imdb");

        using var cancellationTokenSource = new CancellationTokenSource();
        MetadataResult<Movie> result = await _kpMovieProvider.GetMetadata(movieInfo, cancellationTokenSource.Token);

        Movie movie = result.Item;
        Assert.NotNull(movie);
        Assert.True(result.HasMetadata);
        Assert.Equal("326", movie.GetProviderId(Plugin.PluginKey));
        Assert.Equal("tt0111161", movie.GetProviderId(MetadataProviders.Imdb));
        Assert.Equal("Video", movie.MediaType);
        Assert.True(5 < movie.CommunityRating);
        Assert.Equal("326", movie.ExternalId);
        _ = Assert.Single(movie.Genres);
        Assert.Equal("драма", movie.Genres[0]);
        Assert.Equal("Побег из Шоушенка", movie.Name);
        Assert.Equal("r", movie.OfficialRating);
        Assert.Equal("The Shawshank Redemption", movie.OriginalTitle);
        Assert.Equal("Бухгалтер Энди Дюфрейн обвинён в убийстве собственной жены и её любовника. Оказавшись в тюрьме под названием Шоушенк, он сталкивается с жестокостью и беззаконием, царящими по обе стороны решётки. Каждый, кто попадает в эти стены, становится их рабом до конца жизни. Но Энди, обладающий живым умом и доброй душой, находит подход как к заключённым, так и к охранникам, добиваясь их особого к себе расположения.", movie.Overview);
        Assert.Equal(1994, movie.ProductionYear);
        Assert.Equal(142, movie.Size);
        Assert.Equal(movie.Name, movie.SortName);
        Assert.Equal("Страх - это кандалы. Надежда - это свобода", movie.Tagline);

        Assert.Equal(80, result.People.Count);
        PersonInfo? person = result.People.FirstOrDefault(p => "Тим Роббинс".Equals(p.Name, StringComparison.Ordinal));
        Assert.NotNull(person);
        Assert.Equal("7987", person.GetProviderId(Plugin.PluginKey));
        Assert.Equal("Andy Dufresne", person.Role);
        Assert.Equal("Тим Роббинс", person.Name);
        Assert.True(!string.IsNullOrWhiteSpace(person.ImageUrl));

        _logManager.Verify(lm => lm.GetLogger(It.IsAny<string>()), Times.Exactly(4));
        _applicationPaths.VerifyGet(ap => ap.PluginConfigurationsPath, Times.Once());
        _xmlSerializer.Verify(xs => xs.DeserializeFromFile(typeof(PluginConfiguration), "UN_KpMovieProvider_GetMetadata_Provider_Imdb/EmbyKinopoiskRu.xml"), Times.Once());
        _localizationManager.Verify(lm => lm.RemoveDiacritics("Побег из Шоушенка"), Times.Once());
        VerifyNoOtherCalls();

        Logger.Info($"Finished '{nameof(KpMovieProvider_GetMetadata_Provider_Imdb)}'");
    }

    [Fact]
    public async void KpMovieProvider_GetMetadata_NameAndYear()
    {
        Logger.Info($"Start '{nameof(KpMovieProvider_GetMetadata_NameAndYear)}'");

        var movieInfo = new MovieInfo()
        {
            Name = "Побег из Шоушенка",
            Year = 1994
        };

        _ = _applicationPaths
            .SetupGet(m => m.PluginConfigurationsPath)
            .Returns("UN_KpMovieProvider_GetMetadata_NameAndYear");

        using var cancellationTokenSource = new CancellationTokenSource();
        MetadataResult<Movie> result = await _kpMovieProvider.GetMetadata(movieInfo, cancellationTokenSource.Token);

        Movie movie = result.Item;
        Assert.NotNull(movie);
        Assert.True(result.HasMetadata);
        Assert.Equal("326", movie.GetProviderId(Plugin.PluginKey));
        Assert.Equal("tt0111161", movie.GetProviderId(MetadataProviders.Imdb));
        Assert.Equal("Video", movie.MediaType);
        Assert.True(5 < movie.CommunityRating);
        Assert.Equal("326", movie.ExternalId);
        _ = Assert.Single(movie.Genres);
        Assert.Equal("драма", movie.Genres[0]);
        Assert.Equal("Побег из Шоушенка", movie.Name);
        Assert.Equal("r", movie.OfficialRating);
        Assert.Equal("The Shawshank Redemption", movie.OriginalTitle);
        Assert.Equal("Бухгалтер Энди Дюфрейн обвинён в убийстве собственной жены и её любовника. Оказавшись в тюрьме под названием Шоушенк, он сталкивается с жестокостью и беззаконием, царящими по обе стороны решётки. Каждый, кто попадает в эти стены, становится их рабом до конца жизни. Но Энди, обладающий живым умом и доброй душой, находит подход как к заключённым, так и к охранникам, добиваясь их особого к себе расположения.", movie.Overview);
        Assert.Equal(1994, movie.ProductionYear);
        Assert.Equal(142, movie.Size);
        Assert.Equal(movie.Name, movie.SortName);
        Assert.Equal("Страх - это кандалы. Надежда - это свобода", movie.Tagline);

        Assert.Equal(80, result.People.Count);
        PersonInfo? person = result.People.FirstOrDefault(p => "Тим Роббинс".Equals(p.Name, StringComparison.Ordinal));
        Assert.NotNull(person);
        Assert.Equal("7987", person.GetProviderId(Plugin.PluginKey));
        Assert.Equal("Andy Dufresne", person.Role);
        Assert.Equal("Тим Роббинс", person.Name);
        Assert.True(!string.IsNullOrWhiteSpace(person.ImageUrl));

        _logManager.Verify(lm => lm.GetLogger(It.IsAny<string>()), Times.Exactly(4));
        _applicationPaths.VerifyGet(ap => ap.PluginConfigurationsPath, Times.Once());
        _xmlSerializer.Verify(xs => xs.DeserializeFromFile(typeof(PluginConfiguration), "UN_KpMovieProvider_GetMetadata_NameAndYear/EmbyKinopoiskRu.xml"), Times.Once());
        _localizationManager.Verify(lm => lm.RemoveDiacritics("Побег из Шоушенка"), Times.Once());
        VerifyNoOtherCalls();

        Logger.Info($"Finished '{nameof(KpMovieProvider_GetMetadata_NameAndYear)}'");
    }

    [Fact]
    public async void KpMovieProvider_GetSearchResults_Provider_Kp()
    {
        Logger.Info($"Start '{nameof(KpMovieProvider_GetSearchResults_Provider_Kp)}'");

        _ = _applicationPaths
            .SetupGet(m => m.PluginConfigurationsPath)
            .Returns("UN_KpMovieProvider_GetSearchResults_Provider_Kp");

        var movieInfo = new MovieInfo()
        {
            ProviderIds = new(new() { { Plugin.PluginKey, "326" } })
        };
        using var cancellationTokenSource = new CancellationTokenSource();
        IEnumerable<RemoteSearchResult> result = await _kpMovieProvider.GetSearchResults(movieInfo, cancellationTokenSource.Token);

        RemoteSearchResult searchResult = Assert.Single(result);
        Assert.NotNull(searchResult);
        Assert.Equal("326", searchResult.GetProviderId(Plugin.PluginKey));
        Assert.Equal("Побег из Шоушенка", searchResult.Name);
        Assert.True(!string.IsNullOrWhiteSpace(searchResult.ImageUrl));
        Assert.Equal(1994, searchResult.ProductionYear);
        Assert.Equal(Plugin.PluginKey, searchResult.SearchProviderName);
        Assert.Equal("Бухгалтер Энди Дюфрейн обвинён в убийстве собственной жены и её любовника. Оказавшись в тюрьме под названием Шоушенк, он сталкивается с жестокостью и беззаконием, царящими по обе стороны решётки. Каждый, кто попадает в эти стены, становится их рабом до конца жизни. Но Энди, обладающий живым умом и доброй душой, находит подход как к заключённым, так и к охранникам, добиваясь их особого к себе расположения.", searchResult.Overview);

        _logManager.Verify(lm => lm.GetLogger(It.IsAny<string>()), Times.Exactly(4));
        _applicationPaths.VerifyGet(ap => ap.PluginConfigurationsPath, Times.Once());
        _xmlSerializer.Verify(xs => xs.DeserializeFromFile(typeof(PluginConfiguration), "UN_KpMovieProvider_GetSearchResults_Provider_Kp/EmbyKinopoiskRu.xml"), Times.Once());
        VerifyNoOtherCalls();

        Logger.Info($"Finished '{nameof(KpMovieProvider_GetSearchResults_Provider_Kp)}'");
    }

    [Fact]
    public async void KpMovieProvider_GetSearchResults_Provider_Imdb()
    {
        Logger.Info($"Start '{nameof(KpMovieProvider_GetSearchResults_Provider_Imdb)}'");

        _ = _applicationPaths
            .SetupGet(m => m.PluginConfigurationsPath)
            .Returns("UN_KpMovieProvider_GetSearchResults_Provider_Imdb");

        var movieInfo = new MovieInfo()
        {
            ProviderIds = new(new() { { Plugin.PluginKey, "326" } })
        };
        using var cancellationTokenSource = new CancellationTokenSource();
        IEnumerable<RemoteSearchResult> result = await _kpMovieProvider.GetSearchResults(movieInfo, cancellationTokenSource.Token);

        RemoteSearchResult searchResult = Assert.Single(result);
        Assert.NotNull(searchResult);
        Assert.Equal("326", searchResult.GetProviderId(Plugin.PluginKey));
        Assert.Equal("Побег из Шоушенка", searchResult.Name);
        Assert.True(!string.IsNullOrWhiteSpace(searchResult.ImageUrl));
        Assert.Equal(1994, searchResult.ProductionYear);
        Assert.Equal(Plugin.PluginKey, searchResult.SearchProviderName);
        Assert.Equal("Бухгалтер Энди Дюфрейн обвинён в убийстве собственной жены и её любовника. Оказавшись в тюрьме под названием Шоушенк, он сталкивается с жестокостью и беззаконием, царящими по обе стороны решётки. Каждый, кто попадает в эти стены, становится их рабом до конца жизни. Но Энди, обладающий живым умом и доброй душой, находит подход как к заключённым, так и к охранникам, добиваясь их особого к себе расположения.", searchResult.Overview);

        _logManager.Verify(lm => lm.GetLogger(It.IsAny<string>()), Times.Exactly(4));
        _applicationPaths.VerifyGet(ap => ap.PluginConfigurationsPath, Times.Once());
        _xmlSerializer.Verify(xs => xs.DeserializeFromFile(typeof(PluginConfiguration), "UN_KpMovieProvider_GetSearchResults_Provider_Imdb/EmbyKinopoiskRu.xml"), Times.Once());
        VerifyNoOtherCalls();

        Logger.Info($"Finished '{nameof(KpMovieProvider_GetSearchResults_Provider_Imdb)}'");
    }

    [Fact]
    public async void KpMovieProvider_GetSearchResults_NameAndYear()
    {
        Logger.Info($"Start '{nameof(KpMovieProvider_GetSearchResults_NameAndYear)}'");

        _ = _applicationPaths
            .SetupGet(m => m.PluginConfigurationsPath)
            .Returns("UN_KpMovieProvider_GetSearchResults_NameAndYear");

        var movieInfo = new MovieInfo()
        {
            Name = "Побег из Шоушенка",
            Year = 1994
        };
        using var cancellationTokenSource = new CancellationTokenSource();
        IEnumerable<RemoteSearchResult> result = await _kpMovieProvider.GetSearchResults(movieInfo, cancellationTokenSource.Token);

        RemoteSearchResult searchResult = Assert.Single(result);
        Assert.NotNull(searchResult);
        Assert.Equal("326", searchResult.GetProviderId(Plugin.PluginKey));
        Assert.Equal("Побег из Шоушенка", searchResult.Name);
        Assert.True(!string.IsNullOrWhiteSpace(searchResult.ImageUrl));
        Assert.Equal(1994, searchResult.ProductionYear);
        Assert.Equal(Plugin.PluginKey, searchResult.SearchProviderName);

        _logManager.Verify(lm => lm.GetLogger(It.IsAny<string>()), Times.Exactly(4));
        _applicationPaths.VerifyGet(ap => ap.PluginConfigurationsPath, Times.Once());
        _xmlSerializer.Verify(xs => xs.DeserializeFromFile(typeof(PluginConfiguration), "UN_KpMovieProvider_GetSearchResults_NameAndYear/EmbyKinopoiskRu.xml"), Times.Once());
        VerifyNoOtherCalls();

        Logger.Info($"Finished '{nameof(KpMovieProvider_GetSearchResults_NameAndYear)}'");
    }

}
