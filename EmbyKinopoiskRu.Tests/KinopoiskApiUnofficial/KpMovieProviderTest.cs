using System.Net;

using EmbyKinopoiskRu.Configuration;
using EmbyKinopoiskRu.Provider.RemoteMetadata;

using FluentAssertions;

using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Providers;


namespace EmbyKinopoiskRu.Tests.KinopoiskApiUnofficial;

public class KpMovieProviderTest : BaseTest
{
    private static readonly NLog.ILogger Logger = NLog.LogManager.GetLogger(nameof(KpMovieProviderTest));

    private readonly KpMovieProvider _kpMovieProvider;


    #region Test configs

    public KpMovieProviderTest() : base(Logger)
    {
        _pluginConfiguration.Token = GetKinopoiskUnofficialToken();
        _pluginConfiguration.ApiType = PluginConfiguration.KinopoiskApiUnofficialTech;

        ConfigLibraryManager();

        _kpMovieProvider = new KpMovieProvider(_httpClient, _logManager.Object);
    }

    #endregion

    [Fact]
    public async Task UN_KpMovieProvider_ForCodeCoverage()
    {
        Logger.Info($"Start '{nameof(UN_KpMovieProvider_ForCodeCoverage)}'");

        _kpMovieProvider.Name.Should().NotBeNull();

        _kpMovieProvider.Features.Should().NotBeEmpty();

        HttpResponseInfo response = await _kpMovieProvider.GetImageResponse("https://www.google.com", CancellationToken.None);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        _logManager.Verify(lm => lm.GetLogger("KpMovieProvider"), Times.Once());
        _logManager.Verify(lm => lm.GetLogger("KinopoiskRu"), Times.Once());
        VerifyNoOtherCalls();

        Logger.Info($"Finished '{nameof(UN_KpMovieProvider_ForCodeCoverage)}'");
    }

    [Fact]
    public async Task UN_KpMovieProvider_GetMetadata_Provider_Kp()
    {
        Logger.Info($"Start '{nameof(UN_KpMovieProvider_GetMetadata_Provider_Kp)}'");

        var movieInfo = new MovieInfo
        {
            ProviderIds = new ProviderIdDictionary(new Dictionary<string, string>
            {
                { Plugin.PluginKey, "326" }
            })
        };

        _ = _applicationPaths
            .SetupGet(m => m.PluginConfigurationsPath)
            .Returns(nameof(UN_KpMovieProvider_GetMetadata_Provider_Kp));

        using var cancellationTokenSource = new CancellationTokenSource();
        MetadataResult<Movie> result = await _kpMovieProvider.GetMetadata(movieInfo, cancellationTokenSource.Token);

        result.HasMetadata.Should().BeTrue();
        Movie movie = result.Item;
        movie.Should().NotBeNull();
        movie.GetProviderId(Plugin.PluginKey).Should().Be("326");
        movie.GetProviderId(MetadataProviders.Imdb).Should().Be("tt0111161");
        movie.MediaType.Should().Be("Video");
        movie.CommunityRating.Should().BeGreaterThan(5);
        movie.ExternalId.Should().Be("326");
        movie.Genres.Should().ContainSingle();
        movie.Genres[0].Should().Be("драма");
        movie.Name.Should().Be("Побег из Шоушенка");
        movie.OfficialRating.Should().Be("r");
        movie.OriginalTitle.Should().Be("The Shawshank Redemption");
        movie.Overview.Should().Be("Бухгалтер Энди Дюфрейн обвинён в убийстве собственной жены и её любовника. Оказавшись в тюрьме под названием Шоушенк, он сталкивается с жестокостью и беззаконием, царящими по обе стороны решётки. Каждый, кто попадает в эти стены, становится их рабом до конца жизни. Но Энди, обладающий живым умом и доброй душой, находит подход как к заключённым, так и к охранникам, добиваясь их особого к себе расположения.");
        movie.ProductionYear.Should().Be(1994);
        movie.Size.Should().Be(142);
        movie.SortName.Should().Be(movie.Name);
        movie.Tagline.Should().Be("Страх - это кандалы. Надежда - это свобода");

        result.People.Should().HaveCount(80);
        PersonInfo? person = result.People.FirstOrDefault(p => "Тим Роббинс".Equals(p.Name, StringComparison.Ordinal));
        person.Should().NotBeNull();
        person.GetProviderId(Plugin.PluginKey).Should().Be("7987");
        person!.Role.Should().Be("Andy Dufresne");
        person.Name.Should().Be("Тим Роббинс");
        person.ImageUrl.Should().NotBeNullOrWhiteSpace();

        _logManager.Verify(lm => lm.GetLogger(It.IsAny<string>()), Times.Exactly(4));
        _applicationPaths.VerifyGet(ap => ap.PluginConfigurationsPath, Times.Once());
        _xmlSerializer.Verify(xs => xs.DeserializeFromFile(typeof(PluginConfiguration), $"{nameof(UN_KpMovieProvider_GetMetadata_Provider_Kp)}/EmbyKinopoiskRu.xml"), Times.Once());
        _localizationManager.Verify(lm => lm.RemoveDiacritics("Побег из Шоушенка"), Times.Once());
        VerifyNoOtherCalls();

        Logger.Info($"Finished '{nameof(UN_KpMovieProvider_GetMetadata_Provider_Kp)}'");
    }

    [Fact]
    public async Task UN_KpMovieProvider_GetMetadata_Provider_Imdb()
    {
        Logger.Info($"Start '{nameof(UN_KpMovieProvider_GetMetadata_Provider_Imdb)}'");

        var movieInfo = new MovieInfo
        {
            ProviderIds = new ProviderIdDictionary(new Dictionary<string, string>
            {
                { MetadataProviders.Imdb.ToString(), "tt0111161" }
            })
        };

        _ = _applicationPaths
            .SetupGet(m => m.PluginConfigurationsPath)
            .Returns(nameof(UN_KpMovieProvider_GetMetadata_Provider_Imdb));

        using var cancellationTokenSource = new CancellationTokenSource();
        MetadataResult<Movie> result = await _kpMovieProvider.GetMetadata(movieInfo, cancellationTokenSource.Token);

        result.HasMetadata.Should().BeTrue();
        Movie movie = result.Item;
        movie.Should().NotBeNull();
        movie.GetProviderId(Plugin.PluginKey).Should().Be("326");
        movie.GetProviderId(MetadataProviders.Imdb).Should().Be("tt0111161");
        movie.MediaType.Should().Be("Video");
        movie.CommunityRating.Should().BeGreaterThan(5);
        movie.ExternalId.Should().Be("326");
        movie.Genres.Should().ContainSingle();
        movie.Genres[0].Should().Be("драма");
        movie.Name.Should().Be("Побег из Шоушенка");
        movie.OfficialRating.Should().Be("r");
        movie.OriginalTitle.Should().Be("The Shawshank Redemption");
        movie.Overview.Should().Be("Бухгалтер Энди Дюфрейн обвинён в убийстве собственной жены и её любовника. Оказавшись в тюрьме под названием Шоушенк, он сталкивается с жестокостью и беззаконием, царящими по обе стороны решётки. Каждый, кто попадает в эти стены, становится их рабом до конца жизни. Но Энди, обладающий живым умом и доброй душой, находит подход как к заключённым, так и к охранникам, добиваясь их особого к себе расположения.");
        movie.ProductionYear.Should().Be(1994);
        movie.Size.Should().Be(142);
        movie.SortName.Should().Be(movie.Name);
        movie.Tagline.Should().Be("Страх - это кандалы. Надежда - это свобода");

        result.People.Should().HaveCount(80);
        PersonInfo? person = result.People.FirstOrDefault(p => "Тим Роббинс".Equals(p.Name, StringComparison.Ordinal));
        person.Should().NotBeNull();
        person.GetProviderId(Plugin.PluginKey).Should().Be("7987");
        person!.Role.Should().Be("Andy Dufresne");
        person.Name.Should().Be("Тим Роббинс");
        person.ImageUrl.Should().NotBeNullOrWhiteSpace();

        _logManager.Verify(lm => lm.GetLogger(It.IsAny<string>()), Times.Exactly(4));
        _applicationPaths.VerifyGet(ap => ap.PluginConfigurationsPath, Times.Once());
        _xmlSerializer.Verify(xs => xs.DeserializeFromFile(typeof(PluginConfiguration), $"{nameof(UN_KpMovieProvider_GetMetadata_Provider_Imdb)}/EmbyKinopoiskRu.xml"), Times.Once());
        _localizationManager.Verify(lm => lm.RemoveDiacritics("Побег из Шоушенка"), Times.Once());
        VerifyNoOtherCalls();

        Logger.Info($"Finished '{nameof(UN_KpMovieProvider_GetMetadata_Provider_Imdb)}'");
    }

    [Fact]
    public async Task UN_KpMovieProvider_GetMetadata_NameAndYear()
    {
        Logger.Info($"Start '{nameof(UN_KpMovieProvider_GetMetadata_NameAndYear)}'");

        var movieInfo = new MovieInfo
        {
            Name = "Побег из Шоушенка",
            Year = 1994
        };

        _ = _applicationPaths
            .SetupGet(m => m.PluginConfigurationsPath)
            .Returns(nameof(UN_KpMovieProvider_GetMetadata_NameAndYear));

        using var cancellationTokenSource = new CancellationTokenSource();
        MetadataResult<Movie> result = await _kpMovieProvider.GetMetadata(movieInfo, cancellationTokenSource.Token);

        result.HasMetadata.Should().BeTrue();
        Movie movie = result.Item;
        movie.Should().NotBeNull();
        movie.GetProviderId(Plugin.PluginKey).Should().Be("326");
        movie.GetProviderId(MetadataProviders.Imdb).Should().Be("tt0111161");
        movie.MediaType.Should().Be("Video");
        movie.CommunityRating.Should().BeGreaterThan(5);
        movie.ExternalId.Should().Be("326");
        movie.Genres.Should().ContainSingle();
        movie.Genres[0].Should().Be("драма");
        movie.Name.Should().Be("Побег из Шоушенка");
        movie.OfficialRating.Should().Be("r");
        movie.OriginalTitle.Should().Be("The Shawshank Redemption");
        movie.Overview.Should().Be("Бухгалтер Энди Дюфрейн обвинён в убийстве собственной жены и её любовника. Оказавшись в тюрьме под названием Шоушенк, он сталкивается с жестокостью и беззаконием, царящими по обе стороны решётки. Каждый, кто попадает в эти стены, становится их рабом до конца жизни. Но Энди, обладающий живым умом и доброй душой, находит подход как к заключённым, так и к охранникам, добиваясь их особого к себе расположения.");
        movie.ProductionYear.Should().Be(1994);
        movie.Size.Should().Be(142);
        movie.SortName.Should().Be(movie.Name);
        movie.Tagline.Should().Be("Страх - это кандалы. Надежда - это свобода");

        result.People.Should().HaveCount(80);
        PersonInfo? person = result.People.FirstOrDefault(p => "Тим Роббинс".Equals(p.Name, StringComparison.Ordinal));
        person.Should().NotBeNull();
        person.GetProviderId(Plugin.PluginKey).Should().Be("7987");
        person!.Role.Should().Be("Andy Dufresne");
        person.Name.Should().Be("Тим Роббинс");
        person.ImageUrl.Should().NotBeNullOrWhiteSpace();

        _logManager.Verify(lm => lm.GetLogger(It.IsAny<string>()), Times.Exactly(4));
        _applicationPaths.VerifyGet(ap => ap.PluginConfigurationsPath, Times.Once());
        _xmlSerializer.Verify(xs => xs.DeserializeFromFile(typeof(PluginConfiguration), $"{nameof(UN_KpMovieProvider_GetMetadata_NameAndYear)}/EmbyKinopoiskRu.xml"), Times.Once());
        _localizationManager.Verify(lm => lm.RemoveDiacritics("Побег из Шоушенка"), Times.Once());
        VerifyNoOtherCalls();

        Logger.Info($"Finished '{nameof(UN_KpMovieProvider_GetMetadata_NameAndYear)}'");
    }

    [Fact]
    public async Task UN_KpMovieProvider_GetSearchResults_Provider_Kp()
    {
        Logger.Info($"Start '{nameof(UN_KpMovieProvider_GetSearchResults_Provider_Kp)}'");

        _ = _applicationPaths
            .SetupGet(m => m.PluginConfigurationsPath)
            .Returns(nameof(UN_KpMovieProvider_GetSearchResults_Provider_Kp));

        var movieInfo = new MovieInfo
        {
            ProviderIds = new ProviderIdDictionary(new Dictionary<string, string>
            {
                { Plugin.PluginKey, "326" }
            })
        };
        using var cancellationTokenSource = new CancellationTokenSource();
        IEnumerable<RemoteSearchResult> result = await _kpMovieProvider.GetSearchResults(movieInfo, cancellationTokenSource.Token);
        result.Should().ContainSingle();
        RemoteSearchResult movie = result.First();
        movie.Should().NotBeNull();
        movie.GetProviderId(Plugin.PluginKey).Should().Be("326");
        movie.Name.Should().Be("Побег из Шоушенка");
        movie.ImageUrl.Should().NotBeNullOrWhiteSpace("movie image exists");
        movie.ProductionYear.Should().Be(1994);
        movie.SearchProviderName.Should().Be(Plugin.PluginKey, "this is movie's SearchProviderName");
        movie.Overview.Should().Be("Бухгалтер Энди Дюфрейн обвинён в убийстве собственной жены и её любовника. Оказавшись в тюрьме под названием Шоушенк, он сталкивается с жестокостью и беззаконием, царящими по обе стороны решётки. Каждый, кто попадает в эти стены, становится их рабом до конца жизни. Но Энди, обладающий живым умом и доброй душой, находит подход как к заключённым, так и к охранникам, добиваясь их особого к себе расположения.");

        _logManager.Verify(lm => lm.GetLogger(It.IsAny<string>()), Times.Exactly(4));
        _applicationPaths.VerifyGet(ap => ap.PluginConfigurationsPath, Times.Once());
        _xmlSerializer.Verify(xs => xs.DeserializeFromFile(typeof(PluginConfiguration), $"{nameof(UN_KpMovieProvider_GetSearchResults_Provider_Kp)}/EmbyKinopoiskRu.xml"), Times.Once());
        VerifyNoOtherCalls();

        Logger.Info($"Finished '{nameof(UN_KpMovieProvider_GetSearchResults_Provider_Kp)}'");
    }

    [Fact]
    public async Task UN_KpMovieProvider_GetSearchResults_Provider_Imdb()
    {
        Logger.Info($"Start '{nameof(UN_KpMovieProvider_GetSearchResults_Provider_Imdb)}'");

        _ = _applicationPaths
            .SetupGet(m => m.PluginConfigurationsPath)
            .Returns(nameof(UN_KpMovieProvider_GetSearchResults_Provider_Imdb));

        var movieInfo = new MovieInfo
        {
            ProviderIds = new ProviderIdDictionary(new Dictionary<string, string>
            {
                { Plugin.PluginKey, "326" }
            })
        };
        using var cancellationTokenSource = new CancellationTokenSource();
        IEnumerable<RemoteSearchResult> result = await _kpMovieProvider.GetSearchResults(movieInfo, cancellationTokenSource.Token);
        result.Should().ContainSingle();
        RemoteSearchResult movie = result.First();
        movie.Should().NotBeNull();
        movie.GetProviderId(Plugin.PluginKey).Should().Be("326");
        movie.Name.Should().Be("Побег из Шоушенка");
        movie.ImageUrl.Should().NotBeNullOrWhiteSpace("movie image exists");
        movie.ProductionYear.Should().Be(1994);
        movie.SearchProviderName.Should().Be(Plugin.PluginKey, "this is movie's SearchProviderName");
        movie.Overview.Should().Be("Бухгалтер Энди Дюфрейн обвинён в убийстве собственной жены и её любовника. Оказавшись в тюрьме под названием Шоушенк, он сталкивается с жестокостью и беззаконием, царящими по обе стороны решётки. Каждый, кто попадает в эти стены, становится их рабом до конца жизни. Но Энди, обладающий живым умом и доброй душой, находит подход как к заключённым, так и к охранникам, добиваясь их особого к себе расположения.");

        _logManager.Verify(lm => lm.GetLogger(It.IsAny<string>()), Times.Exactly(4));
        _applicationPaths.VerifyGet(ap => ap.PluginConfigurationsPath, Times.Once());
        _xmlSerializer.Verify(xs => xs.DeserializeFromFile(typeof(PluginConfiguration), $"{nameof(UN_KpMovieProvider_GetSearchResults_Provider_Imdb)}/EmbyKinopoiskRu.xml"), Times.Once());
        VerifyNoOtherCalls();

        Logger.Info($"Finished '{nameof(UN_KpMovieProvider_GetSearchResults_Provider_Imdb)}'");
    }

    [Fact]
    public async Task UN_KpMovieProvider_GetSearchResults_NameAndYear()
    {
        Logger.Info($"Start '{nameof(UN_KpMovieProvider_GetSearchResults_NameAndYear)}'");

        _ = _applicationPaths
            .SetupGet(m => m.PluginConfigurationsPath)
            .Returns(nameof(UN_KpMovieProvider_GetSearchResults_NameAndYear));

        var movieInfo = new MovieInfo
        {
            Name = "Побег из Шоушенка",
            Year = 1994
        };
        using var cancellationTokenSource = new CancellationTokenSource();
        IEnumerable<RemoteSearchResult> result = await _kpMovieProvider.GetSearchResults(movieInfo, cancellationTokenSource.Token);
        result.Should().ContainSingle();
        RemoteSearchResult movie = result.First();
        movie.Should().NotBeNull();
        movie.GetProviderId(Plugin.PluginKey).Should().Be("326");
        movie.Name.Should().Be("Побег из Шоушенка");
        movie.ImageUrl.Should().NotBeNullOrWhiteSpace("movie image exists");
        movie.ProductionYear.Should().Be(1994);
        movie.SearchProviderName.Should().Be(Plugin.PluginKey, "this is movie's SearchProviderName");

        _logManager.Verify(lm => lm.GetLogger(It.IsAny<string>()), Times.Exactly(4));
        _applicationPaths.VerifyGet(ap => ap.PluginConfigurationsPath, Times.Once());
        _xmlSerializer.Verify(xs => xs.DeserializeFromFile(typeof(PluginConfiguration), $"{nameof(UN_KpMovieProvider_GetSearchResults_NameAndYear)}/EmbyKinopoiskRu.xml"), Times.Once());
        VerifyNoOtherCalls();

        Logger.Info($"Finished '{nameof(UN_KpMovieProvider_GetSearchResults_NameAndYear)}'");
    }
}
