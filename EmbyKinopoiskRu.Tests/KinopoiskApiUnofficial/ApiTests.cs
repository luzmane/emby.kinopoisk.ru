using System.Text.Json;

using FluentAssertions;

using EmbyKinopoiskRu.Api.KinopoiskApiUnofficial.Model;
using EmbyKinopoiskRu.Api.KinopoiskApiUnofficial.Model.Film;
using EmbyKinopoiskRu.Api.KinopoiskApiUnofficial.Model.Person;
using EmbyKinopoiskRu.Api.KinopoiskApiUnofficial.Model.Season;
using NLog;

namespace EmbyKinopoiskRu.Tests.KinopoiskApiUnofficial;

public class ApiTests : IDisposable
{
    private const string KINOPOISK_UNOFFICIAL_TOKEN = "0f162131-81c1-4979-b46c-3eea4263fb11";
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly ILogger _logger;

    public ApiTests()
    {
        _logger = LogManager.GetCurrentClassLogger();

        _httpClient = new();
        _httpClient.DefaultRequestHeaders.Add("X-API-KEY", GetKinopoiskUnofficialToken());

        _jsonOptions = new() { PropertyNameCaseInsensitive = true };
    }

    #region Tests

    [Fact]
    public async Task GetFilmById()
    {
        var request = $"https://kinopoiskapiunofficial.tech/api/v2.2/films/326";
        using HttpResponseMessage responseMessage = await _httpClient.GetAsync(new Uri(request));
        _ = responseMessage.EnsureSuccessStatusCode();
        var response = await responseMessage.Content.ReadAsStringAsync();
        KpFilm? film = JsonSerializer.Deserialize<KpFilm>(response, _jsonOptions);

        VerifyKpFilm326(film);
    }

    [Fact]
    public async Task SearchFilmByName()
    {
        var request = $"https://kinopoiskapiunofficial.tech/api/v2.2/films?keyword=Побег из Шоушенка";
        using HttpResponseMessage responseMessage = await _httpClient.GetAsync(new Uri(request));
        _ = responseMessage.EnsureSuccessStatusCode();
        var response = await responseMessage.Content.ReadAsStringAsync();
        KpSearchResult<KpFilm>? filmSearchResult = JsonSerializer.Deserialize<KpSearchResult<KpFilm>>(response, _jsonOptions);
        filmSearchResult.Should().NotBeNull();
        filmSearchResult!.Items.Should().ContainSingle();

        VerifyKpFilm326(filmSearchResult.Items.First(x => x.KinopoiskId == 326), true);
    }

    [Fact]
    public async Task SearchFilmByNameAndYear()
    {
        var request = $"https://kinopoiskapiunofficial.tech/api/v2.2/films?keyword=Побег из Шоушенка&yearFrom=1994&yearTo=1994";
        using HttpResponseMessage responseMessage = await _httpClient.GetAsync(new Uri(request));
        _ = responseMessage.EnsureSuccessStatusCode();
        var response = await responseMessage.Content.ReadAsStringAsync();
        KpSearchResult<KpFilm>? filmSearchResult = JsonSerializer.Deserialize<KpSearchResult<KpFilm>>(response, _jsonOptions);
        filmSearchResult.Should().NotBeNull();
        filmSearchResult!.Items.Should().ContainSingle();

        VerifyKpFilm326(filmSearchResult.Items.First(x => x.KinopoiskId == 326), true);
    }

    [Fact]
    public async Task GetSeasons()
    {
        var request = $"https://kinopoiskapiunofficial.tech/api/v2.2/films/77044/seasons";
        using HttpResponseMessage responseMessage = await _httpClient.GetAsync(new Uri(request));
        _ = responseMessage.EnsureSuccessStatusCode();
        var response = await responseMessage.Content.ReadAsStringAsync();
        KpSearchResult<KpSeason>? seasons = JsonSerializer.Deserialize<KpSearchResult<KpSeason>>(response, _jsonOptions);
        seasons.Should().NotBeNull();
        seasons!.Items.Count.Should().Be(10);
        KpSeason kpSeason = seasons.Items[0];
        kpSeason.Episodes.Count.Should().Be(24);
        KpEpisode kpEpisode = kpSeason.Episodes[23];
        kpEpisode.EpisodeNumber.Should().Be(24);
        kpEpisode.SeasonNumber.Should().Be(1);
        kpEpisode.NameRu.Should().Be("Эпизод, где Рейчел понимает");
        kpEpisode.NameEn.Should().Be("The One Where Rachel Finds Out");
        kpEpisode.ReleaseDate.Should().Be("1995-05-18");
    }

    [Fact]
    public async Task GetVideos()
    {
        var request = $"https://kinopoiskapiunofficial.tech/api/v2.2/films/77044/videos";
        using HttpResponseMessage responseMessage = await _httpClient.GetAsync(new Uri(request));
        _ = responseMessage.EnsureSuccessStatusCode();
        var response = await responseMessage.Content.ReadAsStringAsync();
        KpSearchResult<KpVideo>? videos = JsonSerializer.Deserialize<KpSearchResult<KpVideo>>(response, _jsonOptions);
        videos.Should().NotBeNull();
        videos!.Items.Count.Should().Be(8);
        KpVideo kpVideo = videos.Items[0];
        kpVideo.Url.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task GetStaffById()
    {
        var request = $"https://kinopoiskapiunofficial.tech/api/v1/staff/7987";
        using HttpResponseMessage responseMessage = await _httpClient.GetAsync(new Uri(request));
        _ = responseMessage.EnsureSuccessStatusCode();
        var response = await responseMessage.Content.ReadAsStringAsync();
        KpPerson? kpPerson = JsonSerializer.Deserialize<KpPerson>(response, _jsonOptions);
        kpPerson.Should().NotBeNull();
        kpPerson!.Birthday.Should().Be("1958-10-16");
        kpPerson.BirthPlace.Should().Be("Уэст-Ковина, Калифорния, США");
        kpPerson.Death.Should().BeNull();
        kpPerson.DeathPlace.Should().BeNull();
        kpPerson.Facts?.Count.Should().Be(4);
        kpPerson.NameEn.Should().Be("Tim Robbins");
        kpPerson.NameRu.Should().Be("Тим Роббинс");
        kpPerson.PersonId.Should().Be(7987);
        kpPerson.PosterUrl.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task GetStaffByFilmId()
    {
        var request = $"https://kinopoiskapiunofficial.tech/api/v1/staff?filmId=326";
        using HttpResponseMessage responseMessage = await _httpClient.GetAsync(new Uri(request));
        _ = responseMessage.EnsureSuccessStatusCode();
        var response = await responseMessage.Content.ReadAsStringAsync();
        List<KpFilmStaff>? filmStaffList = JsonSerializer.Deserialize<List<KpFilmStaff>>(response, _jsonOptions);
        filmStaffList.Should().NotBeNull();
        filmStaffList!.Count.Should().Be(90);
        KpFilmStaff filmStaff = filmStaffList[1];
        filmStaff.Description.Should().Be("Andy Dufresne");
        filmStaff.NameEn.Should().Be("Tim Robbins");
        filmStaff.NameRu.Should().Be("Тим Роббинс");
        filmStaff.PosterUrl.Should().NotBeNullOrWhiteSpace();
        filmStaff.ProfessionKey.Should().Be("ACTOR");
        filmStaff.StaffId.Should().Be(7987);
    }

    [Fact]
    public async Task SearchStaffByName()
    {
        var request = $"https://kinopoiskapiunofficial.tech/api/v1/persons?name=Тим Роббинс";
        using HttpResponseMessage responseMessage = await _httpClient.GetAsync(new Uri(request));
        _ = responseMessage.EnsureSuccessStatusCode();
        var response = await responseMessage.Content.ReadAsStringAsync();
        KpSearchResult<KpStaff>? staffSearchResult = JsonSerializer.Deserialize<KpSearchResult<KpStaff>>(response, _jsonOptions);
        staffSearchResult.Should().NotBeNull();
        staffSearchResult!.Items.Count.Should().Be(4);
        KpStaff staff = staffSearchResult.Items.First(f => f.KinopoiskId == 7987);
        staff.Should().NotBeNull();
        staff!.KinopoiskId.Should().Be(7987);
        staff.NameEn.Should().Be("Tim Robbins");
        staff.NameRu.Should().Be("Тим Роббинс");
        staff.PosterUrl.Should().NotBeNullOrWhiteSpace();
    }

    #endregion

    #region Verify

    private static void VerifyKpFilm326(KpFilm? film, bool isSearch = false)
    {
        film.Should().NotBeNull();
        film!.Countries.Should().ContainSingle();
        film.Genres.Should().ContainSingle();
        film.ImdbId.Should().Be("tt0111161");
        film.KinopoiskId.Should().Be(326);
        film.NameOriginal.Should().Be("The Shawshank Redemption");
        film.NameRu.Should().Be("Побег из Шоушенка");
        film.PosterUrl.Should().NotBeNullOrWhiteSpace();
        film.PosterUrlPreview.Should().NotBeNullOrWhiteSpace();
        film.RatingKinopoisk.Should().BeGreaterThan(9);
        film.Year.Should().Be(1994);
        if (isSearch)
        {
            film.CoverUrl.Should().BeNull();
            film.Description.Should().BeNull();
            film.FilmLength.Should().BeNull();
            film.LogoUrl.Should().BeNull();
            film.RatingMpaa.Should().BeNull();
            film.Slogan.Should().BeNull();
        }
        else
        {
            film.CoverUrl.Should().NotBeNullOrWhiteSpace();
            film.Description.Should().Be("Бухгалтер Энди Дюфрейн обвинён в убийстве собственной жены и её любовника. Оказавшись в тюрьме под названием Шоушенк, он сталкивается с жестокостью и беззаконием, царящими по обе стороны решётки. Каждый, кто попадает в эти стены, становится их рабом до конца жизни. Но Энди, обладающий живым умом и доброй душой, находит подход как к заключённым, так и к охранникам, добиваясь их особого к себе расположения.");
            film.FilmLength.Should().Be(142);
            film.LogoUrl.Should().NotBeNullOrWhiteSpace();
            film.RatingMpaa.Should().Be("r");
            film.Slogan.Should().Be("Страх - это кандалы. Надежда - это свобода");
        }
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

    protected string GetKinopoiskUnofficialToken()
    {
        var token = Environment.GetEnvironmentVariable("KINOPOISK_UNOFFICIAL_TOKEN");
        _logger.Info($"Env token length is: {(token != null ? token.Length : 0)}");
        return string.IsNullOrWhiteSpace(token) ? KINOPOISK_UNOFFICIAL_TOKEN : token;
    }

    #endregion

}
