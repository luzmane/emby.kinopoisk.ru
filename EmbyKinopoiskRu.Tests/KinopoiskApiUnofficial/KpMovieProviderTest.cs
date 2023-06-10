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

        _kpMovieProvider.Name.Should().NotBeNull("name is hardcoded");

        _kpMovieProvider.Features.Should().NotBeEmpty();

        HttpResponseInfo response = await _kpMovieProvider.GetImageResponse("https://www.google.com", CancellationToken.None);
        response.StatusCode.Should().Be(HttpStatusCode.OK, "this is status code of the response to google.com");

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

        result.HasMetadata.Should().BeTrue("that mean the item was found");
        Movie movie = result.Item;
        movie.Should().NotBeNull("that mean the movie was found");
        movie.GetProviderId(Plugin.PluginKey).Should().Be("326", "id of the requested item");
        movie.GetProviderId(MetadataProviders.Imdb).Should().Be("tt0111161", "IMDB id of the requested item");
        movie.MediaType.Should().Be("Video", "this is video");
        movie.CommunityRating.Should().BeGreaterThan(5, "such value received from API");
        movie.ExternalId.Should().Be("326", "KP id of requested item");
        movie.Genres.Should().ContainSingle();
        movie.Genres[0].Should().Be("драма", "the film has only this genre");
        movie.Name.Should().Be("Побег из Шоушенка", "this is the name of the movie");
        movie.OfficialRating.Should().Be("r", "this is film's OfficialRating");
        movie.OriginalTitle.Should().Be("The Shawshank Redemption", "this is the original name of the movie");
        movie.Overview.Should().Be("Бухгалтер Энди Дюфрейн обвинён в убийстве собственной жены и её любовника. Оказавшись в тюрьме под названием Шоушенк, он сталкивается с жестокостью и беззаконием, царящими по обе стороны решётки. Каждый, кто попадает в эти стены, становится их рабом до конца жизни. Но Энди, обладающий живым умом и доброй душой, находит подход как к заключённым, так и к охранникам, добиваясь их особого к себе расположения.", "this is film's Overview");
        movie.ProductionYear.Should().Be(1994, "this is movie ProductionYear");
        movie.Size.Should().Be(142, "this is movie Size");
        movie.SortName.Should().Be(movie.Name, "SortName should be equal to Name");
        movie.Tagline.Should().Be("Страх - это кандалы. Надежда - это свобода", "this is a Tagline of the movie");

        result.People.Should().HaveCount(80);
        PersonInfo? person = result.People.FirstOrDefault(p => "Тим Роббинс".Equals(p.Name, StringComparison.Ordinal));
        person.Should().NotBeNull("that mean the person was found");
        person.GetProviderId(Plugin.PluginKey).Should().Be("7987", "id of the requested item");
        person!.Role.Should().Be("Andy Dufresne", "this is person's Role");
        person.Name.Should().Be("Тим Роббинс", "this is the person's name");
        person.ImageUrl.Should().NotBeNullOrWhiteSpace("person image exists");

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

        result.HasMetadata.Should().BeTrue("that mean the item was found");
        Movie movie = result.Item;
        movie.Should().NotBeNull("that mean the movie was found");
        movie.GetProviderId(Plugin.PluginKey).Should().Be("326", "id of the requested item");
        movie.GetProviderId(MetadataProviders.Imdb).Should().Be("tt0111161", "IMDB id of the requested item");
        movie.MediaType.Should().Be("Video", "this is video");
        movie.CommunityRating.Should().BeGreaterThan(5, "such value received from API");
        movie.ExternalId.Should().Be("326", "KP id of requested item");
        movie.Genres.Should().ContainSingle();
        movie.Genres[0].Should().Be("драма", "the film has only this genre");
        movie.Name.Should().Be("Побег из Шоушенка", "this is the name of the movie");
        movie.OfficialRating.Should().Be("r", "this is film's OfficialRating");
        movie.OriginalTitle.Should().Be("The Shawshank Redemption", "this is the original name of the movie");
        movie.Overview.Should().Be("Бухгалтер Энди Дюфрейн обвинён в убийстве собственной жены и её любовника. Оказавшись в тюрьме под названием Шоушенк, он сталкивается с жестокостью и беззаконием, царящими по обе стороны решётки. Каждый, кто попадает в эти стены, становится их рабом до конца жизни. Но Энди, обладающий живым умом и доброй душой, находит подход как к заключённым, так и к охранникам, добиваясь их особого к себе расположения.", "this is film's Overview");
        movie.ProductionYear.Should().Be(1994, "this is movie ProductionYear");
        movie.Size.Should().Be(142, "this is movie Size");
        movie.SortName.Should().Be(movie.Name, "SortName should be equal to Name");
        movie.Tagline.Should().Be("Страх - это кандалы. Надежда - это свобода", "this is a Tagline of the movie");

        result.People.Should().HaveCount(80);
        PersonInfo? person = result.People.FirstOrDefault(p => "Тим Роббинс".Equals(p.Name, StringComparison.Ordinal));
        person.Should().NotBeNull("that mean the person was found");
        person.GetProviderId(Plugin.PluginKey).Should().Be("7987", "id of the requested item");
        person!.Role.Should().Be("Andy Dufresne", "this is person's Role");
        person.Name.Should().Be("Тим Роббинс", "this is the person's name");
        person.ImageUrl.Should().NotBeNullOrWhiteSpace("person image exists");

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

        result.HasMetadata.Should().BeTrue("that mean the item was found");
        Movie movie = result.Item;
        movie.Should().NotBeNull("that mean the movie was found");
        movie.GetProviderId(Plugin.PluginKey).Should().Be("326", "id of the requested item");
        movie.GetProviderId(MetadataProviders.Imdb).Should().Be("tt0111161", "IMDB id of the requested item");
        movie.MediaType.Should().Be("Video", "this is video");
        movie.CommunityRating.Should().BeGreaterThan(5, "such value received from API");
        movie.ExternalId.Should().Be("326", "KP id of requested item");
        movie.Genres.Should().ContainSingle();
        movie.Genres[0].Should().Be("драма", "the film has only this genre");
        movie.Name.Should().Be("Побег из Шоушенка", "this is the name of the movie");
        movie.OfficialRating.Should().Be("r", "this is film's OfficialRating");
        movie.OriginalTitle.Should().Be("The Shawshank Redemption", "this is the original name of the movie");
        movie.Overview.Should().Be("Бухгалтер Энди Дюфрейн обвинён в убийстве собственной жены и её любовника. Оказавшись в тюрьме под названием Шоушенк, он сталкивается с жестокостью и беззаконием, царящими по обе стороны решётки. Каждый, кто попадает в эти стены, становится их рабом до конца жизни. Но Энди, обладающий живым умом и доброй душой, находит подход как к заключённым, так и к охранникам, добиваясь их особого к себе расположения.", "this is film's Overview");
        movie.ProductionYear.Should().Be(1994, "this is movie ProductionYear");
        movie.Size.Should().Be(142, "this is movie Size");
        movie.SortName.Should().Be(movie.Name, "SortName should be equal to Name");
        movie.Tagline.Should().Be("Страх - это кандалы. Надежда - это свобода", "this is a Tagline of the movie");

        result.People.Should().HaveCount(80);
        PersonInfo? person = result.People.FirstOrDefault(p => "Тим Роббинс".Equals(p.Name, StringComparison.Ordinal));
        person.Should().NotBeNull("that mean the person was found");
        person.GetProviderId(Plugin.PluginKey).Should().Be("7987", "id of the requested item");
        person!.Role.Should().Be("Andy Dufresne", "this is person's Role");
        person.Name.Should().Be("Тим Роббинс", "this is the person's name");
        person.ImageUrl.Should().NotBeNullOrWhiteSpace("person image exists");

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
        result.Should().ContainSingle();
        RemoteSearchResult movie = result.First();
        movie.Should().NotBeNull("that mean the movie was found");
        movie.GetProviderId(Plugin.PluginKey).Should().Be("326", "id of the requested item");
        movie.Name.Should().Be("Побег из Шоушенка", "this is the name of the movie");
        movie.ImageUrl.Should().NotBeNullOrWhiteSpace("movie image exists");
        movie.ProductionYear.Should().Be(1994, "this is movie ProductionYear");
        movie.SearchProviderName.Should().Be(Plugin.PluginKey, "this is movie's SearchProviderName");
        movie.Overview.Should().Be("Бухгалтер Энди Дюфрейн обвинён в убийстве собственной жены и её любовника. Оказавшись в тюрьме под названием Шоушенк, он сталкивается с жестокостью и беззаконием, царящими по обе стороны решётки. Каждый, кто попадает в эти стены, становится их рабом до конца жизни. Но Энди, обладающий живым умом и доброй душой, находит подход как к заключённым, так и к охранникам, добиваясь их особого к себе расположения.", "this is film's Overview");

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
        result.Should().ContainSingle();
        RemoteSearchResult movie = result.First();
        movie.Should().NotBeNull("that mean the movie was found");
        movie.GetProviderId(Plugin.PluginKey).Should().Be("326", "id of the requested item");
        movie.Name.Should().Be("Побег из Шоушенка", "this is the name of the movie");
        movie.ImageUrl.Should().NotBeNullOrWhiteSpace("movie image exists");
        movie.ProductionYear.Should().Be(1994, "this is movie ProductionYear");
        movie.SearchProviderName.Should().Be(Plugin.PluginKey, "this is movie's SearchProviderName");
        movie.Overview.Should().Be("Бухгалтер Энди Дюфрейн обвинён в убийстве собственной жены и её любовника. Оказавшись в тюрьме под названием Шоушенк, он сталкивается с жестокостью и беззаконием, царящими по обе стороны решётки. Каждый, кто попадает в эти стены, становится их рабом до конца жизни. Но Энди, обладающий живым умом и доброй душой, находит подход как к заключённым, так и к охранникам, добиваясь их особого к себе расположения.", "this is film's Overview");

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
        result.Should().ContainSingle();
        RemoteSearchResult movie = result.First();
        movie.Should().NotBeNull("that mean the movie was found");
        movie.GetProviderId(Plugin.PluginKey).Should().Be("326", "id of the requested item");
        movie.Name.Should().Be("Побег из Шоушенка", "this is the name of the movie");
        movie.ImageUrl.Should().NotBeNullOrWhiteSpace("movie image exists");
        movie.ProductionYear.Should().Be(1994, "this is movie ProductionYear");
        movie.SearchProviderName.Should().Be(Plugin.PluginKey, "this is movie's SearchProviderName");

        _logManager.Verify(lm => lm.GetLogger(It.IsAny<string>()), Times.Exactly(4));
        _applicationPaths.VerifyGet(ap => ap.PluginConfigurationsPath, Times.Once());
        _xmlSerializer.Verify(xs => xs.DeserializeFromFile(typeof(PluginConfiguration), "UN_KpMovieProvider_GetSearchResults_NameAndYear/EmbyKinopoiskRu.xml"), Times.Once());
        VerifyNoOtherCalls();

        Logger.Info($"Finished '{nameof(KpMovieProvider_GetSearchResults_NameAndYear)}'");
    }

}
