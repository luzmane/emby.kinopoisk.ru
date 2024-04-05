using System.Text.Json;

using FluentAssertions;

using EmbyKinopoiskRu.Api;
using EmbyKinopoiskRu.Api.KinopoiskDev.Model;
using EmbyKinopoiskRu.Api.KinopoiskDev.Model.Movie;
using EmbyKinopoiskRu.Api.KinopoiskDev.Model.Person;
using EmbyKinopoiskRu.Api.KinopoiskDev.Model.Season;

using NLog;

namespace EmbyKinopoiskRu.Tests.KinopoiskDev;

/// <summary>
/// Swagger documentation:
///     https://api.kinopoisk.dev/v1/documentation-json
///     https://api.kinopoisk.dev/v1/documentation-yaml
///     https://api.kinopoisk.dev/v1/documentation#/
/// </summary>
public class ApiTests : IDisposable
{
    private const string KinopoiskDevToken = "8DA0EV2-KTP4A5Q-G67QP3K-S2VFBX7";
    private const int RequestLimit = 250;

    // doesn't have "productionCompanies" in the result
    private static readonly IList<string> MovieUniversalSelectFields = new List<string>
    {
        "alternativeName",
        "backdrop",
        "countries",
        "description",
        "enName",
        "externalId",
        "genres",
        "id",
        "logo",
        "movieLength",
        "name",
        "persons",
        "poster",
        "premiere",
        "rating",
        "ratingMpaa",
        "slogan",
        "videos",
        "year",
        "sequelsAndPrequels",
        "top250",
        "facts",
        "releaseYears",
        "seasonsInfo",
        "lists"
    }.AsReadOnly();

    private static readonly IList<string> PersonUniversalSelectFields = new List<string>
    {
        "birthday",
        "birthPlace",
        "death",
        "deathPlace",
        "enName",
        "facts",
        "id",
        "movies",
        "name",
        "photo"
    }.AsReadOnly();

    private static readonly IList<string> SeasonUniversalSelectFields = new List<string>
    {
        "airDate",
        "description",
        "episodes",
        "episodesCount",
        "movieId",
        "name",
        "number",
        "poster"
    }.AsReadOnly();

    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly ILogger _logger;


    public ApiTests()
    {
        _logger = LogManager.GetCurrentClassLogger();

        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("X-API-KEY", GetKinopoiskDevToken());

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    #region Tests

    [Fact]
    public async Task GetMovieById()
    {
        var request = new Uri("https://api.kinopoisk.dev/v1.4/movie/435");
        using HttpResponseMessage responseMessage = await _httpClient.GetAsync(request);
        _ = responseMessage.EnsureSuccessStatusCode();
        var response = await responseMessage.Content.ReadAsStringAsync();

        VerifyMovie435(JsonSerializer.Deserialize<KpMovie>(response, _jsonOptions));
    }

    [Fact]
    public async Task GetMovies_Universal_Ids()
    {
        var request = "https://api.kinopoisk.dev/v1.4/movie?";
        request += $"limit={RequestLimit}";
        request += $"&selectFields={string.Join("&selectFields=", MovieUniversalSelectFields)}";
        request += "&id=689&id=435";
        using HttpResponseMessage responseMessage = await _httpClient.GetAsync(new Uri(request));
        _ = responseMessage.EnsureSuccessStatusCode();
        var response = await responseMessage.Content.ReadAsStringAsync();
        var searchResultMovie = JsonSerializer.Deserialize<KpSearchResult<KpMovie>>(response, _jsonOptions);
        searchResultMovie.Should().NotBeNull();
        searchResultMovie!.Docs.Count.Should().Be(2);

        VerifyMovie689(searchResultMovie.Docs.FirstOrDefault(i => i.Id == 689), true);
        VerifyMovie435(searchResultMovie.Docs.FirstOrDefault(i => i.Id == 435), true);
    }

    [Fact]
    public async Task GetMovies_Query_Name_Year()
    {
        var request = "https://api.kinopoisk.dev/v1.4/movie/search?";
        request += $"limit={RequestLimit}";
        request += "&query=Гарри Поттер и философский камень 2001"; // 689
        using HttpResponseMessage responseMessage = await _httpClient.GetAsync(new Uri(request));
        _ = responseMessage.EnsureSuccessStatusCode();
        var response = await responseMessage.Content.ReadAsStringAsync();
        var searchResultMovie = JsonSerializer.Deserialize<KpSearchResult<KpMovie>>(response, _jsonOptions);
        searchResultMovie.Should().NotBeNull();
        searchResultMovie!.Docs.Should().NotBeEmpty();

        VerifyMovie689(searchResultMovie.Docs.FirstOrDefault(x => x.Id == 689), isQuerySearch: true);
    }

    [Fact]
    public async Task GetMovies_Query_AlternativeName_Year()
    {
        var request = "https://api.kinopoisk.dev/v1.4/movie/search?";
        request += $"limit={RequestLimit}";
        request += "&query=Harry Potter and the Sorcerer's Stone 2001"; // 689
        using HttpResponseMessage responseMessage = await _httpClient.GetAsync(new Uri(request));
        _ = responseMessage.EnsureSuccessStatusCode();
        var response = await responseMessage.Content.ReadAsStringAsync();
        var searchResultMovie = JsonSerializer.Deserialize<KpSearchResult<KpMovie>>(response, _jsonOptions);
        searchResultMovie.Should().NotBeNull();
        searchResultMovie!.Docs.Should().NotBeEmpty();

        VerifyMovie689(searchResultMovie.Docs.FirstOrDefault(x => x.Id == 689), isQuerySearch: true);
    }

    [Fact]
    public async Task GetMovies_Universal_List_Top500()
    {
        var request = "https://api.kinopoisk.dev/v1.4/movie?";
        request += $"limit={RequestLimit}";
        request += $"&selectFields={string.Join("&selectFields=", MovieUniversalSelectFields)}";
        request += "&lists=top500";
        using HttpResponseMessage responseMessage = await _httpClient.GetAsync(new Uri(request));
        _ = responseMessage.EnsureSuccessStatusCode();
        var response = await responseMessage.Content.ReadAsStringAsync();
        var kpMovie = JsonSerializer.Deserialize<KpSearchResult<KpMovie>>(response, _jsonOptions);
        kpMovie.Should().NotBeNull();
        kpMovie!.Docs.Count.Should().Be(RequestLimit);
        kpMovie.Pages.Should().Be(2);
    }

    [Fact]
    public async Task GetMovies_Universal_ExternalIds()
    {
        var request = "https://api.kinopoisk.dev/v1.4/movie?";
        request += $"limit={RequestLimit}";
        request += $"&selectFields={string.Join("&selectFields=", MovieUniversalSelectFields)}";
        request += "&externalId.imdb=tt0241527&externalId.imdb=tt0120689";
        using HttpResponseMessage responseMessage = await _httpClient.GetAsync(new Uri(request));
        _ = responseMessage.EnsureSuccessStatusCode();
        var response = await responseMessage.Content.ReadAsStringAsync();
        var searchResultMovie = JsonSerializer.Deserialize<KpSearchResult<KpMovie>>(response, _jsonOptions);
        searchResultMovie.Should().NotBeNull();
        searchResultMovie!.Docs.Count.Should().Be(2);

        VerifyMovie689(searchResultMovie.Docs.FirstOrDefault(i => i.Id == 689), true);
        VerifyMovie435(searchResultMovie.Docs.FirstOrDefault(i => i.Id == 435), true);
    }

    [Fact]
    public async Task GetPersonById()
    {
        const string request = "https://api.kinopoisk.dev/v1.4/person/7987";
        using HttpResponseMessage responseMessage = await _httpClient.GetAsync(new Uri(request));
        _ = responseMessage.EnsureSuccessStatusCode();
        var response = await responseMessage.Content.ReadAsStringAsync();

        VerifyPerson7987(JsonSerializer.Deserialize<KpPerson>(response, _jsonOptions));
    }

    [Fact]
    public async Task GetPersons_Query_Name()
    {
        var request = "https://api.kinopoisk.dev/v1.4/person/search?";
        request += $"limit={RequestLimit}";
        request += "&query=Тим Роббинс";
        using HttpResponseMessage responseMessage = await _httpClient.GetAsync(new Uri(request));
        _ = responseMessage.EnsureSuccessStatusCode();
        var response = await responseMessage.Content.ReadAsStringAsync();
        var searchResultKpPerson = JsonSerializer.Deserialize<KpSearchResult<KpPerson>>(response, _jsonOptions);
        searchResultKpPerson.Should().NotBeNull();
        searchResultKpPerson!.Docs.Count.Should().Be(RequestLimit);

        VerifyPerson7987(searchResultKpPerson.Docs.FirstOrDefault(x => x.Id == 7987), true);
    }

    [Fact]
    public async Task GetPersons_Universal_MoviesId()
    {
        var request = "https://api.kinopoisk.dev/v1.4/person?";
        request += $"limit={RequestLimit}";
        request += $"&selectFields={string.Join("&selectFields=", PersonUniversalSelectFields)}";
        request += "&movies.id=326";
        using HttpResponseMessage responseMessage = await _httpClient.GetAsync(new Uri(request));
        _ = responseMessage.EnsureSuccessStatusCode();
        var response = await responseMessage.Content.ReadAsStringAsync();
        var searchResultKpPerson = JsonSerializer.Deserialize<KpSearchResult<KpPerson>>(response, _jsonOptions);
        searchResultKpPerson.Should().NotBeNull();
        searchResultKpPerson!.Docs.Should().HaveCountGreaterThan(100);

        VerifyPerson7987(searchResultKpPerson.Docs.FirstOrDefault(i => i.Id == 7987));
    }

    [Fact]
    public async Task GetEpisodes_Universal_MovieId()
    {
        var request = "https://api.kinopoisk.dev/v1.4/season?";
        request += $"limit={RequestLimit}";
        request += $"&selectFields={string.Join("&selectFields=", SeasonUniversalSelectFields)}";
        request += "&movieId=77044";
        using HttpResponseMessage responseMessage = await _httpClient.GetAsync(new Uri(request));
        _ = responseMessage.EnsureSuccessStatusCode();
        var response = await responseMessage.Content.ReadAsStringAsync();
        var searchResultKpSeason = JsonSerializer.Deserialize<KpSearchResult<KpSeason>>(response, _jsonOptions);
        searchResultKpSeason.Should().NotBeNull();
        searchResultKpSeason!.Docs.RemoveAll(x => x.EpisodesCount == 0 || x.Number == 0);
        searchResultKpSeason.Docs.Count.Should().Be(10);

        VerifySeries77044Season5(searchResultKpSeason.Docs.FirstOrDefault(i => i.Number == 5));
    }

    [Fact]
    public async Task GetEpisodes_Universal_MovieId_Season()
    {
        var request = "https://api.kinopoisk.dev/v1.4/season?";
        request += $"limit={RequestLimit}";
        request += $"&selectFields={string.Join("&selectFields=", SeasonUniversalSelectFields)}";
        request += "&movieId=77044&number=5";
        using HttpResponseMessage responseMessage = await _httpClient.GetAsync(new Uri(request));
        _ = responseMessage.EnsureSuccessStatusCode();
        var response = await responseMessage.Content.ReadAsStringAsync();
        var searchResultKpSeason = JsonSerializer.Deserialize<KpSearchResult<KpSeason>>(response, _jsonOptions);
        searchResultKpSeason.Should().NotBeNull();
        searchResultKpSeason!.Docs.Count.Should().Be(1);

        VerifySeries77044Season5(searchResultKpSeason.Docs.FirstOrDefault(i => i.Number == 5));
    }

    [Fact]
    public async Task GetAllKinopoiskLists()
    {
        var request = "https://api.kinopoisk.dev/v1.4/list?limit=200";
        request += "&selectFields=name&selectFields=slug&selectFields=moviesCount&selectFields=cover&selectFields=category";
        using HttpResponseMessage responseMessage = await _httpClient.GetAsync(new Uri(request));
        _ = responseMessage.EnsureSuccessStatusCode();
        var response = await responseMessage.Content.ReadAsStringAsync();

        var lists = JsonSerializer.Deserialize<KpSearchResult<KpLists>>(response, _jsonOptions);
        lists.Should().NotBeNull();
        lists!.Docs.Should().NotBeEmpty();
    }

    #endregion

    #region Verify

    private static void VerifyMovie435(KpMovie? kpMovie, bool isUniversalSearch = false, bool isQuerySearch = false)
    {
        kpMovie.Should().NotBeNull();
        kpMovie!.AlternativeName.Should().Be("The Green Mile");
        kpMovie.Backdrop.Should().NotBeNull();
        kpMovie.Backdrop!.Url.Should().NotBeNullOrWhiteSpace();
        kpMovie.Backdrop!.PreviewUrl.Should().NotBeNullOrWhiteSpace();
        kpMovie.Countries.Should().NotBeNull();
        kpMovie.Countries!.Count.Should().Be(1);
        kpMovie.Description.Should().Be("Пол Эджкомб — начальник блока смертников в тюрьме «Холодная гора», каждый из узников которого однажды проходит «зеленую милю» по пути к месту казни. Пол повидал много заключённых и надзирателей за время работы. Однако гигант Джон Коффи, обвинённый в страшном преступлении, стал одним из самых необычных обитателей блока.");
        kpMovie.ExternalId.Should().NotBeNull();
        kpMovie.ExternalId!.Imdb.Should().Be("tt0120689");
        kpMovie.ExternalId!.Tmdb.Should().Be(497);
        if (isQuerySearch)
        {
            kpMovie.Facts.Should().BeNull();
        }
        else
        {
            kpMovie.Facts.Should().NotBeNull();
            kpMovie.Facts.Should().NotBeEmpty();
        }

        kpMovie.Genres.Should().NotBeNull();
        kpMovie.Genres!.Count.Should().Be(3);
        kpMovie.Id.Should().Be(435);
        if (isQuerySearch)
        {
            kpMovie.Lists.Should().BeNull();
        }
        else
        {
            kpMovie.Lists.Should().NotBeNull();
            kpMovie.Lists.Should().NotBeEmpty();
        }

        kpMovie.Logo.Should().NotBeNull();
        kpMovie.Logo!.Url.Should().NotBeNullOrWhiteSpace();
        kpMovie.MovieLength.Should().Be(189);
        kpMovie.Name.Should().Be("Зеленая миля");
        if (isQuerySearch)
        {
            kpMovie.Persons.Should().BeNull();
        }
        else
        {
            kpMovie.Persons.Should().HaveCount(26);
            KpPersonMovie? kpPersonMovie = kpMovie.Persons!.FirstOrDefault(p => p.Id == 9144L);
            kpPersonMovie.Should().NotBeNull();
            VerifyPersonMovie9144(kpPersonMovie!);
        }

        kpMovie.Poster.Should().NotBeNull();
        kpMovie.Poster!.Url.Should().NotBeNullOrWhiteSpace();
        kpMovie.Poster!.PreviewUrl.Should().NotBeNullOrWhiteSpace();
        if (isQuerySearch)
        {
            kpMovie.Premiere.Should().BeNull();
        }
        else
        {
            kpMovie.Premiere.Should().NotBeNull();
            kpMovie.Premiere!.World.Should().Be("1999-12-06T00:00:00.000Z");
        }

        if (isQuerySearch || isUniversalSearch)
        {
            kpMovie.ProductionCompanies.Should().BeNull();
        }
        else
        {
            kpMovie.ProductionCompanies.Should().NotBeNull();
            kpMovie.ProductionCompanies!.Count.Should().Be(4);
        }

        kpMovie.Rating.Should().NotBeNull();
        kpMovie.Rating!.Kp.Should().NotBeNull();
        kpMovie.RatingMpaa.Should().Be("r");
        kpMovie.Top250.Should().NotBeNull();
        if (isQuerySearch)
        {
            kpMovie.SequelsAndPrequels.Should().BeNull();
            kpMovie.Slogan.Should().BeNull();
            kpMovie.Videos.Should().BeNull();
        }
        else
        {
            kpMovie.SequelsAndPrequels.Should().NotBeNull();
            kpMovie.SequelsAndPrequels.Should().BeEmpty();
            kpMovie.Slogan.Should().Be("Пол Эджкомб не верил в чудеса. Пока не столкнулся с одним из них");
            kpMovie.Videos.Should().NotBeNull();
            kpMovie.Videos!.Teasers.Should().BeEmpty();
            kpMovie.Videos!.Trailers.Should().NotBeEmpty();
        }

        kpMovie.Year.Should().Be(1999);
    }

    private static void VerifyMovie689(KpMovie? kpMovie, bool isUniversalSearch = false, bool isQuerySearch = false)
    {
        kpMovie.Should().NotBeNull();
        kpMovie!.AlternativeName.Should().Be("Harry Potter and the Sorcerer's Stone");
        kpMovie.Backdrop.Should().NotBeNull();
        kpMovie.Backdrop.Url.Should().NotBeNullOrWhiteSpace();
        kpMovie.Backdrop.PreviewUrl.Should().NotBeNullOrWhiteSpace();
        kpMovie.Countries.Should().NotBeNull();
        kpMovie.Countries!.Count.Should().Be(2);
        kpMovie.Description.Should().Be("Жизнь десятилетнего Гарри Поттера нельзя назвать сладкой: родители умерли, едва ему исполнился год, а от дяди и тёти, взявших сироту на воспитание, достаются лишь тычки да подзатыльники. Но в одиннадцатый день рождения Гарри всё меняется. Странный гость, неожиданно появившийся на пороге, приносит письмо, из которого мальчик узнаёт, что на самом деле он - волшебник и зачислен в школу магии под названием Хогвартс. А уже через пару недель Гарри будет мчаться в поезде Хогвартс-экспресс навстречу новой жизни, где его ждут невероятные приключения, верные друзья и самое главное — ключ к разгадке тайны смерти его родителей.");
        kpMovie.ExternalId.Should().NotBeNull();
        kpMovie.ExternalId.Imdb.Should().Be("tt0241527");
        kpMovie.ExternalId.Tmdb.Should().Be(671);
        if (isQuerySearch)
        {
            kpMovie.Facts.Should().BeNull();
        }
        else
        {
            kpMovie.Facts.Should().NotBeNull();
            kpMovie.Facts.Should().NotBeEmpty();
        }

        kpMovie.Genres.Should().NotBeNull();
        kpMovie.Genres.Count.Should().Be(3);
        kpMovie.Id.Should().Be(689);
        if (isQuerySearch)
        {
            kpMovie.Lists.Should().BeNull();
        }
        else
        {
            kpMovie.Lists.Should().NotBeNull();
            kpMovie.Lists.Should().NotBeEmpty();
        }

        kpMovie.Logo.Should().NotBeNull();
        kpMovie.Logo!.Url.Should().NotBeNullOrWhiteSpace();
        kpMovie.MovieLength.Should().Be(152);
        kpMovie.Name.Should().Be("Гарри Поттер и философский камень");
        if (isQuerySearch)
        {
            kpMovie.Persons.Should().BeNull();
        }
        else
        {
            kpMovie.Persons.Should().NotBeNull();
            kpMovie.Persons.Count.Should().Be(37);
        }

        kpMovie.Poster.Should().NotBeNull();
        kpMovie.Poster!.Url.Should().NotBeNullOrWhiteSpace();
        kpMovie.Poster!.PreviewUrl.Should().NotBeNullOrWhiteSpace();
        if (isQuerySearch)
        {
            kpMovie.Premiere.Should().BeNull();
        }
        else
        {
            kpMovie.Premiere.Should().NotBeNull();
            kpMovie.Premiere!.World.Should().Be("2001-11-04T00:00:00.000Z");
        }

        if (isQuerySearch || isUniversalSearch)
        {
            kpMovie.ProductionCompanies.Should().BeNull();
        }
        else
        {
            kpMovie.ProductionCompanies.Should().NotBeNull();
            kpMovie.ProductionCompanies!.Count.Should().Be(4);
        }

        kpMovie.Rating.Should().NotBeNull();
        kpMovie.Rating!.Kp.Should().NotBeNull();
        kpMovie.RatingMpaa.Should().Be("pg");
        kpMovie.Top250.Should().NotBeNull();
        if (isQuerySearch)
        {
            kpMovie.SequelsAndPrequels.Should().BeNull();
            kpMovie.Slogan.Should().BeNull();
            kpMovie.Videos.Should().BeNull();
        }
        else
        {
            kpMovie.SequelsAndPrequels.Should().NotBeNull();
            kpMovie.SequelsAndPrequels.Count.Should().Be(8);
            kpMovie.Slogan.Should().Be("Путешествие в твою мечту");
            kpMovie.Videos.Should().NotBeNull();
            kpMovie.Videos!.Teasers.Should().BeEmpty();
            kpMovie.Videos!.Trailers.Should().NotBeEmpty();
        }

        kpMovie.Year.Should().Be(2001);
    }

    private static void VerifyPersonMovie9144(KpPersonMovie? kpPersonMovie)
    {
        kpPersonMovie.Should().NotBeNull();
        kpPersonMovie!.Id.Should().Be(9144);
        kpPersonMovie.Name.Should().Be("Том Хэнкс");
        kpPersonMovie.EnName.Should().Be("Tom Hanks");
        kpPersonMovie.EnProfession.Should().Be("actor");
        kpPersonMovie.Profession.Should().Be("актеры");
        kpPersonMovie.Description.Should().Be("Paul Edgecomb");
        kpPersonMovie.Photo.Should().NotBeNullOrWhiteSpace();
    }

    private static void VerifyPerson7987(KpPerson? kpPerson, bool isQuerySearch = false)
    {
        kpPerson.Should().NotBeNull();
        kpPerson!.Birthday.Should().Be("1958-10-16T00:00:00.000Z");
        kpPerson.Death.Should().BeNullOrEmpty();
        kpPerson.EnName.Should().Be("Tim Robbins");
        kpPerson.Id.Should().Be(7987);
        kpPerson.Name.Should().Be("Тим Роббинс");
        kpPerson.Photo.Should().NotBeNullOrWhiteSpace();
        if (isQuerySearch)
        {
            kpPerson.BirthPlace.Should().NotBeNull();
            kpPerson.BirthPlace.Should().HaveCount(3);
            kpPerson.Facts.Should().BeNull();
            kpPerson.Movies.Should().BeNull();
        }
        else
        {
            kpPerson.DeathPlace.Should().NotBeNull();
            kpPerson.DeathPlace.Should().BeEmpty();
            kpPerson.Facts.Should().NotBeNull();
            kpPerson.Facts.Should().HaveCountGreaterThanOrEqualTo(4);
            kpPerson.Movies.Should().NotBeNull();
            kpPerson.Movies.Should().HaveCountGreaterThanOrEqualTo(233);
        }
    }

    private static void VerifySeries77044Season5(KpSeason? kpSeason)
    {
        kpSeason.Should().NotBeNull();
        kpSeason!.AirDate.Should().Be("1998-09-24T00:00:00.000Z");
        kpSeason.Description.Should().Be("Росс делает глупую ошибку. На своей свадьбе вместо имени своей невесты Емели, он называет имя Рейчел. Свадьба продолжается, но после нее Емели сбегает и просит не преследовать ее. Росс снова впадает в депрессию. Из-за чего у него постоянные нервные срывы и всплески ярости. Моника и Чендлер начинают тайно встречаться, однако делать это в присутствии друзей очень сложно. Тем не менее, их отношения рано или поздно становятся явным. Фиби рожает своему брату Фрэнку тройняшек и одного хочет взять себе. Джо получает роль в хорошем фильме, съемки которого пройдут в Лас-Вегасе. Все друзья отправляются туда и по возвращению двое из них становятся женатыми. Думаете это Чендлер и Моника? Узнаете при просмотре...");
        kpSeason.Episodes.Should().NotBeNull();
        kpSeason.Episodes.Should().HaveCount(23);
        kpSeason.EpisodesCount.Should().Be(24);
        kpSeason.MovieId.Should().Be(77044);
        kpSeason.Name.Should().Be("Сезон 5");
        kpSeason.Number.Should().Be(5);
        kpSeason.Poster.Should().NotBeNull();
        kpSeason.Poster.Url.Should().NotBeNullOrWhiteSpace();
        kpSeason.Poster.PreviewUrl.Should().NotBeNullOrWhiteSpace();

        VerifySeries77044Season5Episode5(kpSeason.Episodes.FirstOrDefault(i => i.Number == 5));
    }

    private static void VerifySeries77044Season5Episode5(KpEpisode? kpEpisode)
    {
        kpEpisode.Should().NotBeNull();
        kpEpisode!.AirDate.Should().Be("1998-10-29T00:00:00.000Z");
        kpEpisode.EnName.Should().Be("The One with the Kips");
        kpEpisode.Description.Should().Be("Чендлер и Моника проводят вместе выходные, но в результате ссорятся. Росс говорит Рэйчел о требовании Эмили. Джо узнает об отношениях между Моникой и Чендлером.");
        kpEpisode.Number.Should().Be(5);
        kpEpisode.Name.Should().Be("Эпизод со старым соседом Кипом");
        kpEpisode.Still.Should().NotBeNull();
        kpEpisode.Still.PreviewUrl.Should().NotBeEmpty();
        kpEpisode.Still.Url.Should().NotBeEmpty();
    }

    #endregion

    #region Utils

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposeAll)
    {
        _httpClient.Dispose();
    }

    private string GetKinopoiskDevToken()
    {
        var token = Environment.GetEnvironmentVariable("KINOPOISK_DEV_TOKEN");
        _logger.Info($"Env token length is: {token?.Length ?? 0}");
        return string.IsNullOrWhiteSpace(token) ? KinopoiskDevToken : token;
    }

    #endregion
}
