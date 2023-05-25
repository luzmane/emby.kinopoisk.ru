using System.Text.Json;

using EmbyKinopoiskRu.Api.KinopoiskApiUnofficial.Model;
using EmbyKinopoiskRu.Api.KinopoiskApiUnofficial.Model.Film;
using EmbyKinopoiskRu.Api.KinopoiskApiUnofficial.Model.Person;
using EmbyKinopoiskRu.Api.KinopoiskApiUnofficial.Model.Season;

namespace EmbyKinopoiskRu.Tests.Utils;

public class KinopoiskUnofficialApiRequests : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public KinopoiskUnofficialApiRequests()
    {
        _httpClient = new();
        _httpClient.DefaultRequestHeaders.Add("X-API-KEY", "0f162131-81c1-4979-b46c-3eea4263fb11");

        _jsonOptions = new() { PropertyNameCaseInsensitive = true };
    }

    [Fact(Skip = "not in use")]
    public async Task GetFilm()
    {
        var request = $"https://kinopoiskapiunofficial.tech/api/v2.2/films/326";
        using HttpResponseMessage responseMessage = await _httpClient.GetAsync(new Uri(request)).ConfigureAwait(false);
        _ = responseMessage.EnsureSuccessStatusCode();
        var response = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
        KpFilm? film = JsonSerializer.Deserialize<KpFilm>(response, _jsonOptions);
        Assert.NotNull(film);
        Assert.Equal(1, film!.Countries?.Count);
        Assert.Equal("https://avatars.mds.yandex.net/get-ott/1652588/2a00000186aca5e13ea6cec11d584ac5455b/orig", film.CoverUrl);
        Assert.Equal("Бухгалтер Энди Дюфрейн обвинён в убийстве собственной жены и её любовника. Оказавшись в тюрьме под названием Шоушенк, он сталкивается с жестокостью и беззаконием, царящими по обе стороны решётки. Каждый, кто попадает в эти стены, становится их рабом до конца жизни. Но Энди, обладающий живым умом и доброй душой, находит подход как к заключённым, так и к охранникам, добиваясь их особого к себе расположения.", film.Description);
        Assert.Equal(142, film.FilmLength);
        Assert.Equal(1, film.Genres?.Count);
        Assert.Equal("tt0111161", film.ImdbId);
        Assert.Equal(326, film.KinopoiskId);
        Assert.Equal("https://avatars.mds.yandex.net/get-ott/1648503/2a000001705c8bf514c033f1019473a4caae/orig", film.LogoUrl);
        Assert.Equal("The Shawshank Redemption", film.NameOriginal);
        Assert.Equal("Побег из Шоушенка", film.NameRu);
        Assert.Equal("https://kinopoiskapiunofficial.tech/images/posters/kp/326.jpg", film.PosterUrl);
        Assert.Equal("https://kinopoiskapiunofficial.tech/images/posters/kp_small/326.jpg", film.PosterUrlPreview);
        Assert.Equal("r", film.RatingMpaa);
        Assert.Equal("Страх - это кандалы. Надежда - это свобода", film.Slogan);
        Assert.Equal(1994, film.Year);
        Assert.True(0 < film.RatingKinopoisk);
    }

    [Fact(Skip = "not in use")]
    public async Task GetSeasons()
    {
        var request = $"https://kinopoiskapiunofficial.tech/api/v2.2/films/77044/seasons";
        using HttpResponseMessage responseMessage = await _httpClient.GetAsync(new Uri(request)).ConfigureAwait(false);
        _ = responseMessage.EnsureSuccessStatusCode();
        var response = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
        KpSearchResult<KpSeason>? seasons = JsonSerializer.Deserialize<KpSearchResult<KpSeason>>(response, _jsonOptions);
        Assert.NotNull(seasons);
        Assert.Equal(10, seasons!.Items.Count);
        KpSeason kpSeason = seasons.Items[0];
        Assert.Equal(24, kpSeason.Episodes.Count);
        KpEpisode kpEpisode = kpSeason.Episodes[23];
        Assert.Equal(24, kpEpisode.EpisodeNumber);
        Assert.Equal(1, kpEpisode.SeasonNumber);
        Assert.Equal("Эпизод, где Рейчел понимает", kpEpisode.NameRu);
        Assert.Equal("The One Where Rachel Finds Out", kpEpisode.NameEn);
        Assert.Equal("1995-05-18", kpEpisode.ReleaseDate);
    }

    [Fact(Skip = "not in use")]
    public async Task GetVideos()
    {
        var request = $"https://kinopoiskapiunofficial.tech/api/v2.2/films/77044/videos";
        using HttpResponseMessage responseMessage = await _httpClient.GetAsync(new Uri(request)).ConfigureAwait(false);
        _ = responseMessage.EnsureSuccessStatusCode();
        var response = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
        KpSearchResult<KpVideo>? videos = JsonSerializer.Deserialize<KpSearchResult<KpVideo>>(response, _jsonOptions);
        Assert.NotNull(videos);
        Assert.Equal(8, videos!.Items.Count);
        KpVideo kpVideo = videos.Items[0];
        Assert.Equal("http://trailers.s3.mds.yandex.net/video_original/160983-05ae2e39521817fce4e34a89968f3808.mp4", kpVideo.Url);
    }

    [Fact(Skip = "not in use")]
    public async Task GetStaff()
    {
        var request = $"https://kinopoiskapiunofficial.tech/api/v1/staff/7987";
        using HttpResponseMessage responseMessage = await _httpClient.GetAsync(new Uri(request)).ConfigureAwait(false);
        _ = responseMessage.EnsureSuccessStatusCode();
        var response = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
        KpPerson? kpPerson = JsonSerializer.Deserialize<KpPerson>(response, _jsonOptions);
        Assert.NotNull(kpPerson);
        Assert.Equal("1958-10-16", kpPerson!.Birthday);
        Assert.Equal("Уэст-Ковина, штат Калифорния, США", kpPerson.BirthPlace);
        Assert.Null(kpPerson.Death);
        Assert.Null(kpPerson.DeathPlace);
        Assert.Equal(4, kpPerson.Facts?.Count);
        Assert.Equal("Tim Robbins", kpPerson.NameEn);
        Assert.Equal("Тим Роббинс", kpPerson.NameRu);
        Assert.Equal(7987, kpPerson.PersonId);
        Assert.Equal("https://kinopoiskapiunofficial.tech/images/actor_posters/kp/7987.jpg", kpPerson.PosterUrl);
    }

    [Fact(Skip = "not in use")]
    public async Task GetFilmsStaff()
    {
        var request = $"https://kinopoiskapiunofficial.tech/api/v1/staff?filmId=326";
        using HttpResponseMessage responseMessage = await _httpClient.GetAsync(new Uri(request)).ConfigureAwait(false);
        _ = responseMessage.EnsureSuccessStatusCode();
        var response = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
        List<KpFilmStaff>? filmStaffList = JsonSerializer.Deserialize<List<KpFilmStaff>>(response, _jsonOptions);
        Assert.NotNull(filmStaffList);
        Assert.Equal(90, filmStaffList!.Count);
        KpFilmStaff filmStaff = filmStaffList[1];
        Assert.Equal("Andy Dufresne", filmStaff.Description);
        Assert.Equal("Tim Robbins", filmStaff.NameEn);
        Assert.Equal("Тим Роббинс", filmStaff.NameRu);
        Assert.Equal("https://kinopoiskapiunofficial.tech/images/actor_posters/kp/7987.jpg", filmStaff.PosterUrl);
        Assert.Equal("ACTOR", filmStaff.ProfessionKey);
        Assert.Equal(7987, filmStaff.StaffId);
    }

    [Fact(Skip = "not in use")]
    public async Task SearchFilmByName()
    {
        var request = $"https://kinopoiskapiunofficial.tech/api/v2.2/films?keyword=100 шагов";
        using HttpResponseMessage responseMessage = await _httpClient.GetAsync(new Uri(request)).ConfigureAwait(false);
        _ = responseMessage.EnsureSuccessStatusCode();
        var response = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
        KpSearchResult<KpFilm>? filmSearchResult = JsonSerializer.Deserialize<KpSearchResult<KpFilm>>(response, _jsonOptions);
        Assert.NotNull(filmSearchResult);
        Assert.Equal(3, filmSearchResult!.Items.Count);
        KpFilm film = filmSearchResult.Items.First(f => f.KinopoiskId == 933277);
        Assert.NotNull(film);
        Assert.Equal(933277, film!.KinopoiskId);
        Assert.Equal("tt3904078", film.ImdbId);
        Assert.Equal("100 Things to Do Before High School", film.NameOriginal);
        Assert.Equal("100 шагов: Успеть до старших классов", film.NameRu);
        Assert.Equal(1, film.Countries?.Count);
        Assert.Equal(2, film.Genres?.Count);
        Assert.True(0 < film.RatingKinopoisk);
        Assert.Equal(2014, film.Year);
        Assert.Equal("https://kinopoiskapiunofficial.tech/images/posters/kp/933277.jpg", film.PosterUrl);
        Assert.Equal("https://kinopoiskapiunofficial.tech/images/posters/kp_small/933277.jpg", film.PosterUrlPreview);
    }

    [Fact(Skip = "not in use")]
    public async Task SearchFilmByNameAndYear()
    {
        var request = $"https://kinopoiskapiunofficial.tech/api/v2.2/films?keyword=100 шагов&yearFrom=2014&yearTo=2014";
        using HttpResponseMessage responseMessage = await _httpClient.GetAsync(new Uri(request)).ConfigureAwait(false);
        _ = responseMessage.EnsureSuccessStatusCode();
        var response = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
        KpSearchResult<KpFilm>? filmSearchResult = JsonSerializer.Deserialize<KpSearchResult<KpFilm>>(response, _jsonOptions);
        Assert.NotNull(filmSearchResult);
        Assert.True(1 == filmSearchResult!.Items.Count);
        KpFilm film = filmSearchResult.Items.First(f => f.KinopoiskId == 933277);
        Assert.NotNull(film);
        Assert.Equal(933277, film!.KinopoiskId);
        Assert.Equal("tt3904078", film.ImdbId);
        Assert.Equal("100 Things to Do Before High School", film.NameOriginal);
        Assert.Equal("100 шагов: Успеть до старших классов", film.NameRu);
        Assert.Equal(1, film.Countries?.Count);
        Assert.Equal(2, film.Genres?.Count);
        Assert.True(0 < film.RatingKinopoisk);
        Assert.Equal(2014, film.Year);
        Assert.Equal("https://kinopoiskapiunofficial.tech/images/posters/kp/933277.jpg", film.PosterUrl);
        Assert.Equal("https://kinopoiskapiunofficial.tech/images/posters/kp_small/933277.jpg", film.PosterUrlPreview);
    }

    [Fact(Skip = "not in use")]
    public async Task SearchStaffByName()
    {
        var request = $"https://kinopoiskapiunofficial.tech/api/v1/persons?name=Тим Роббинс";
        using HttpResponseMessage responseMessage = await _httpClient.GetAsync(new Uri(request)).ConfigureAwait(false);
        _ = responseMessage.EnsureSuccessStatusCode();
        var response = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
        KpSearchResult<KpStaff>? staffSearchResult = JsonSerializer.Deserialize<KpSearchResult<KpStaff>>(response, _jsonOptions);
        Assert.NotNull(staffSearchResult);
        Assert.Equal(4, staffSearchResult!.Items.Count);
        KpStaff staff = staffSearchResult.Items.First(f => f.KinopoiskId == 7987);
        Assert.NotNull(staff);
        Assert.Equal(7987, staff!.KinopoiskId);
        Assert.Equal("Tim Robbins", staff.NameEn);
        Assert.Equal("Тим Роббинс", staff.NameRu);
        Assert.Equal("https://kinopoiskapiunofficial.tech/images/actor_posters/kp/7987.jpg", staff.PosterUrl);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposeAll)
    {
        _httpClient.Dispose();
    }

}
