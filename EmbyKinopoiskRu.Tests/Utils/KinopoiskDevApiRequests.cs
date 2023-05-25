using System.Text.Json;

using EmbyKinopoiskRu.Api.KinopoiskDev.Model;
using EmbyKinopoiskRu.Api.KinopoiskDev.Model.Movie;
using EmbyKinopoiskRu.Api.KinopoiskDev.Model.Person;
using EmbyKinopoiskRu.Api.KinopoiskDev.Model.Season;

namespace EmbyKinopoiskRu.Tests.Utils;

/// <summary>
/// Swagger documentation: 
///     https://api.kinopoisk.dev/v1/documentation-json
///     https://api.kinopoisk.dev/v1/documentation-yaml
/// </summary>
public class KinopoiskDevApiRequests : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public KinopoiskDevApiRequests()
    {
        _httpClient = new();
        _httpClient.DefaultRequestHeaders.Add("X-API-KEY", "8DA0EV2-KTP4A5Q-G67QP3K-S2VFBX7");

        _jsonOptions = new() { PropertyNameCaseInsensitive = true };
    }


    [Fact(Skip = "not in use")]
    public async Task GetMovieById()
    {
        var request = new Uri("https://api.kinopoisk.dev/v1.3/movie/689");
        using HttpResponseMessage responseMessage = await _httpClient.GetAsync(request).ConfigureAwait(false);
        _ = responseMessage.EnsureSuccessStatusCode();
        var response = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
        KpMovie? kpMovie = JsonSerializer.Deserialize<KpMovie>(response, _jsonOptions);
        Assert.NotNull(kpMovie);
        Assert.Equal("The Green Mile", kpMovie!.AlternativeName);
        Assert.Equal("https://avatars.mds.yandex.net/get-ott/224348/2a00000169e39ef77f588ccdfe574dae8227/orig", kpMovie.Backdrop?.Url);
        Assert.Equal("https://avatars.mds.yandex.net/get-ott/224348/2a00000169e39ef77f588ccdfe574dae8227/x1000", kpMovie.Backdrop?.PreviewUrl);
        Assert.Equal(1, kpMovie.Countries?.Count);
        Assert.Equal("Пол Эджкомб — начальник блока смертников в тюрьме «Холодная гора», каждый из узников которого однажды проходит «зеленую милю» по пути к месту казни. Пол повидал много заключённых и надзирателей за время работы. Однако гигант Джон Коффи, обвинённый в страшном преступлении, стал одним из самых необычных обитателей блока.", kpMovie.Description);
        Assert.Equal("tt0120689", kpMovie.ExternalId?.Imdb);
        Assert.Equal(497, kpMovie.ExternalId?.Tmdb);
        Assert.Equal(3, kpMovie.Genres?.Count);
        Assert.Equal(435, kpMovie.Id);
        Assert.Equal("https://avatars.mds.yandex.net/get-ott/239697/2a0000016f12f1eb8870b609ee94313774b2/orig", kpMovie.Logo?.Url);
        Assert.Equal(189, kpMovie.MovieLength);
        Assert.Equal("Зеленая миля", kpMovie.Name);
        Assert.Equal(87, kpMovie.Persons?.Count);
        Assert.Equal("https://st.kp.yandex.net/images/film_big/435.jpg", kpMovie.Poster?.Url);
        Assert.Equal("https://st.kp.yandex.net/images/film_iphone/iphone360_435.jpg", kpMovie.Poster?.PreviewUrl);
        Assert.Equal("1999-12-06T00:00:00.000Z", kpMovie.Premiere?.World);
        Assert.Equal(4, kpMovie.ProductionCompanies?.Count);
        Assert.NotNull(kpMovie.Rating?.Kp);
        Assert.Equal("r", kpMovie.RatingMpaa);
        Assert.Equal("Пол Эджкомб не верил в чудеса. Пока не столкнулся с одним из них", kpMovie.Slogan);
        Assert.Equal(0, kpMovie.Videos?.Teasers.Count);
        Assert.Equal(2, kpMovie.Videos?.Trailers.Count);
        Assert.Equal(1999, kpMovie.Year);
        Assert.Equal(21, kpMovie.Facts?.Count);
        Assert.Equal(0, kpMovie.SequelsAndPrequels?.Count);
        Assert.Equal(1, kpMovie.Top250);
    }

    [Fact(Skip = "not in use")]
    public async Task GetMoviesByMovieDetailsNameYear()
    {
        var request = $"https://api.kinopoisk.dev/v1.3/movie?";
        request += "&limit=50";
        request += "&selectFields=alternativeName backdrop countries description enName externalId genres id logo movieLength name persons poster premiere productionCompanies rating ratingMpaa slogan videos year sequelsAndPrequels top250 facts releaseYears seasonsInfo";
        request += "&name=Гарри Поттер и философский камень";
        request += "&year=2001"; // 689
        using HttpResponseMessage responseMessage = await _httpClient.GetAsync(new Uri(request)).ConfigureAwait(false);
        _ = responseMessage.EnsureSuccessStatusCode();
        var response = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
        KpSearchResult<KpMovie>? searchResultMovie = JsonSerializer.Deserialize<KpSearchResult<KpMovie>>(response, _jsonOptions);
        Assert.NotNull(searchResultMovie);
        Assert.Single(searchResultMovie!.Docs);
        KpMovie kpMovie = searchResultMovie!.Docs[0];
        Assert.Equal("Harry Potter and the Sorcerer's Stone", kpMovie.AlternativeName);
        Assert.Equal("https://avatars.mds.yandex.net/get-ott/223007/2a0000016fb3ac87014aae4f0c64329f64e0/orig", kpMovie.Backdrop?.Url);
        Assert.Equal("https://avatars.mds.yandex.net/get-ott/223007/2a0000016fb3ac87014aae4f0c64329f64e0/x1000", kpMovie.Backdrop?.PreviewUrl);
        Assert.Equal(2, kpMovie.Countries?.Count);
        Assert.Equal("Жизнь десятилетнего Гарри Поттера нельзя назвать сладкой: родители умерли, едва ему исполнился год, а от дяди и тёти, взявших сироту на воспитание, достаются лишь тычки да подзатыльники. Но в одиннадцатый день рождения Гарри всё меняется. Странный гость, неожиданно появившийся на пороге, приносит письмо, из которого мальчик узнаёт, что на самом деле он - волшебник и зачислен в школу магии под названием Хогвартс. А уже через пару недель Гарри будет мчаться в поезде Хогвартс-экспресс навстречу новой жизни, где его ждут невероятные приключения, верные друзья и самое главное — ключ к разгадке тайны смерти его родителей.", kpMovie.Description);
        Assert.Equal("tt0241527", kpMovie.ExternalId?.Imdb);
        Assert.Equal(671, kpMovie.ExternalId?.Tmdb);
        Assert.Equal(52, kpMovie.Facts?.Count);
        Assert.Equal(3, kpMovie.Genres?.Count);
        Assert.Equal(689, kpMovie.Id);
        Assert.Equal("https://avatars.mds.yandex.net/get-ott/223007/2a0000017e127a46aa2122ff48cb306de98b/orig", kpMovie.Logo?.Url);
        Assert.Equal(152, kpMovie.MovieLength);
        Assert.Equal("Гарри Поттер и философский камень", kpMovie.Name);
        Assert.Equal(173, kpMovie.Persons?.Count);
        Assert.Equal("https://st.kp.yandex.net/images/film_big/689.jpg", kpMovie.Poster?.Url);
        Assert.Equal("https://st.kp.yandex.net/images/film_iphone/iphone360_689.jpg", kpMovie.Poster?.PreviewUrl);
        Assert.Equal("2001-11-04T00:00:00.000Z", kpMovie.Premiere?.World);
        Assert.Equal(3, kpMovie.ProductionCompanies?.Count);
        Assert.NotNull(kpMovie.Rating?.Kp);
        Assert.Equal("pg", kpMovie.RatingMpaa);
        Assert.Equal(8, kpMovie.SequelsAndPrequels?.Count);
        Assert.Equal("Путешествие в твою мечту", kpMovie.Slogan);
        Assert.Empty(kpMovie.Videos!.Teasers);
        Assert.Equal(20, kpMovie.Videos!.Trailers.Count);
        Assert.Equal(2001, kpMovie.Year);
        Assert.True(0 < kpMovie.Top250);
    }

    [Fact(Skip = "not in use")]
    public async Task GetMoviesByMovieIds()
    {
        var request = $"https://api.kinopoisk.dev/v1.3/movie?";
        request += "&limit=50";
        request += "&selectFields=alternativeName backdrop countries description enName externalId genres id logo movieLength name persons poster premiere productionCompanies rating ratingMpaa slogan videos year sequelsAndPrequels top250 facts releaseYears seasonsInfo";
        request += "&id=689&id=435";
        using HttpResponseMessage responseMessage = await _httpClient.GetAsync(new Uri(request)).ConfigureAwait(false);
        _ = responseMessage.EnsureSuccessStatusCode();
        var response = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
        KpSearchResult<KpMovie>? searchResultMovie = JsonSerializer.Deserialize<KpSearchResult<KpMovie>>(response, _jsonOptions);
        Assert.NotNull(searchResultMovie);
        Assert.Equal(2, searchResultMovie!.Docs.Count);

        KpMovie kpMovie = searchResultMovie!.Docs.First(i => i.Id == 689);
        Assert.Equal("Harry Potter and the Sorcerer's Stone", kpMovie.AlternativeName);
        Assert.Equal("https://avatars.mds.yandex.net/get-ott/223007/2a0000016fb3ac87014aae4f0c64329f64e0/orig", kpMovie.Backdrop?.Url);
        Assert.Equal("https://avatars.mds.yandex.net/get-ott/223007/2a0000016fb3ac87014aae4f0c64329f64e0/x1000", kpMovie.Backdrop?.PreviewUrl);
        Assert.Equal(2, kpMovie.Countries?.Count);
        Assert.Equal("Жизнь десятилетнего Гарри Поттера нельзя назвать сладкой: родители умерли, едва ему исполнился год, а от дяди и тёти, взявших сироту на воспитание, достаются лишь тычки да подзатыльники. Но в одиннадцатый день рождения Гарри всё меняется. Странный гость, неожиданно появившийся на пороге, приносит письмо, из которого мальчик узнаёт, что на самом деле он - волшебник и зачислен в школу магии под названием Хогвартс. А уже через пару недель Гарри будет мчаться в поезде Хогвартс-экспресс навстречу новой жизни, где его ждут невероятные приключения, верные друзья и самое главное — ключ к разгадке тайны смерти его родителей.", kpMovie.Description);
        Assert.Equal("tt0241527", kpMovie.ExternalId?.Imdb);
        Assert.Equal(671, kpMovie.ExternalId?.Tmdb);
        Assert.Equal(52, kpMovie.Facts?.Count);
        Assert.Equal(3, kpMovie.Genres?.Count);
        Assert.Equal(689, kpMovie.Id);
        Assert.Equal("https://avatars.mds.yandex.net/get-ott/223007/2a0000017e127a46aa2122ff48cb306de98b/orig", kpMovie.Logo?.Url);
        Assert.Equal(152, kpMovie.MovieLength);
        Assert.Equal("Гарри Поттер и философский камень", kpMovie.Name);
        Assert.Equal(173, kpMovie.Persons?.Count);
        Assert.Equal("https://st.kp.yandex.net/images/film_big/689.jpg", kpMovie.Poster?.Url);
        Assert.Equal("https://st.kp.yandex.net/images/film_iphone/iphone360_689.jpg", kpMovie.Poster?.PreviewUrl);
        Assert.Equal("2001-11-04T00:00:00.000Z", kpMovie.Premiere?.World);
        Assert.Equal(3, kpMovie.ProductionCompanies?.Count);
        Assert.NotNull(kpMovie.Rating?.Kp);
        Assert.Equal("pg", kpMovie.RatingMpaa);
        Assert.Equal(8, kpMovie.SequelsAndPrequels.Count);
        Assert.Equal("Путешествие в твою мечту", kpMovie.Slogan);
        Assert.Empty(kpMovie.Videos!.Teasers);
        Assert.Equal(20, kpMovie.Videos!.Trailers.Count);
        Assert.Equal(2001, kpMovie.Year);
        Assert.True(0 < kpMovie.Top250);

        kpMovie = searchResultMovie!.Docs.First(i => i.Id == 435);
        Assert.NotNull(kpMovie);
        Assert.Equal("The Green Mile", kpMovie!.AlternativeName);
        Assert.Equal("https://avatars.mds.yandex.net/get-ott/224348/2a00000169e39ef77f588ccdfe574dae8227/orig", kpMovie.Backdrop?.Url);
        Assert.Equal("https://avatars.mds.yandex.net/get-ott/224348/2a00000169e39ef77f588ccdfe574dae8227/x1000", kpMovie.Backdrop?.PreviewUrl);
        Assert.Equal(1, kpMovie.Countries?.Count);
        Assert.Equal("Пол Эджкомб — начальник блока смертников в тюрьме «Холодная гора», каждый из узников которого однажды проходит «зеленую милю» по пути к месту казни. Пол повидал много заключённых и надзирателей за время работы. Однако гигант Джон Коффи, обвинённый в страшном преступлении, стал одним из самых необычных обитателей блока.", kpMovie.Description);
        Assert.Equal("tt0120689", kpMovie.ExternalId?.Imdb);
        Assert.Equal(497, kpMovie.ExternalId?.Tmdb);
        Assert.Equal(3, kpMovie.Genres?.Count);
        Assert.Equal(435, kpMovie.Id);
        Assert.Equal("https://avatars.mds.yandex.net/get-ott/239697/2a0000016f12f1eb8870b609ee94313774b2/orig", kpMovie.Logo?.Url);
        Assert.Equal(189, kpMovie.MovieLength);
        Assert.Equal("Зеленая миля", kpMovie.Name);
        Assert.Equal(87, kpMovie.Persons?.Count);
        Assert.Equal("https://st.kp.yandex.net/images/film_big/435.jpg", kpMovie.Poster?.Url);
        Assert.Equal("https://st.kp.yandex.net/images/film_iphone/iphone360_435.jpg", kpMovie.Poster?.PreviewUrl);
        Assert.Equal("1999-12-06T00:00:00.000Z", kpMovie.Premiere?.World);
        Assert.Equal(4, kpMovie.ProductionCompanies?.Count);
        Assert.NotNull(kpMovie.Rating?.Kp);
        Assert.Equal("r", kpMovie.RatingMpaa);
        Assert.Equal("Пол Эджкомб не верил в чудеса. Пока не столкнулся с одним из них", kpMovie.Slogan);
        Assert.Equal(0, kpMovie.Videos?.Teasers.Count);
        Assert.Equal(2, kpMovie.Videos?.Trailers.Count);
        Assert.Equal(1999, kpMovie.Year);
        Assert.Equal(21, kpMovie.Facts?.Count);
        Assert.Empty(kpMovie.SequelsAndPrequels);
        Assert.Equal(1, kpMovie.Top250);
    }

    [Fact(Skip = "not in use")]
    public async Task GetMoviesByMovieDetailsAlternativeNameYear()
    {
        var request = $"https://api.kinopoisk.dev/v1/movie?";
        request += "&limit=50";
        request += "&selectFields=alternativeName backdrop countries description enName externalId genres id logo movieLength name persons poster premiere productionCompanies rating ratingMpaa slogan videos year sequelsAndPrequels top250 facts releaseYears seasonsInfo";
        request += "&alternativeName=Harry Potter and the Sorcerer's Stone";
        request += "&year=2001"; // 689
        using HttpResponseMessage responseMessage = await _httpClient.GetAsync(new Uri(request)).ConfigureAwait(false);
        _ = responseMessage.EnsureSuccessStatusCode();
        var response = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
        KpSearchResult<KpMovie>? searchResultMovie = JsonSerializer.Deserialize<KpSearchResult<KpMovie>>(response, _jsonOptions);
        Assert.NotNull(searchResultMovie);
        Assert.Single(searchResultMovie!.Docs);
        KpMovie kpMovie = searchResultMovie!.Docs[0];
        Assert.Equal("Harry Potter and the Sorcerer's Stone", kpMovie.AlternativeName);
        Assert.Equal("https://avatars.mds.yandex.net/get-ott/223007/2a0000016fb3ac87014aae4f0c64329f64e0/orig", kpMovie.Backdrop?.Url);
        Assert.Equal("https://avatars.mds.yandex.net/get-ott/223007/2a0000016fb3ac87014aae4f0c64329f64e0/x1000", kpMovie.Backdrop?.PreviewUrl);
        Assert.Equal(2, kpMovie.Countries?.Count);
        Assert.Equal("Жизнь десятилетнего Гарри Поттера нельзя назвать сладкой: родители умерли, едва ему исполнился год, а от дяди и тёти, взявших сироту на воспитание, достаются лишь тычки да подзатыльники. Но в одиннадцатый день рождения Гарри всё меняется. Странный гость, неожиданно появившийся на пороге, приносит письмо, из которого мальчик узнаёт, что на самом деле он - волшебник и зачислен в школу магии под названием Хогвартс. А уже через пару недель Гарри будет мчаться в поезде Хогвартс-экспресс навстречу новой жизни, где его ждут невероятные приключения, верные друзья и самое главное — ключ к разгадке тайны смерти его родителей.", kpMovie.Description);
        Assert.Equal("tt0241527", kpMovie.ExternalId?.Imdb);
        Assert.Equal(671, kpMovie.ExternalId?.Tmdb);
        Assert.Equal(52, kpMovie.Facts?.Count);
        Assert.Equal(3, kpMovie.Genres?.Count);
        Assert.Equal(689, kpMovie.Id);
        Assert.Equal("https://avatars.mds.yandex.net/get-ott/223007/2a0000017e127a46aa2122ff48cb306de98b/orig", kpMovie.Logo?.Url);
        Assert.Equal(152, kpMovie.MovieLength);
        Assert.Equal("Гарри Поттер и философский камень", kpMovie.Name);
        Assert.Equal(173, kpMovie.Persons?.Count);
        Assert.Equal("https://st.kp.yandex.net/images/film_big/689.jpg", kpMovie.Poster?.Url);
        Assert.Equal("https://st.kp.yandex.net/images/film_iphone/iphone360_689.jpg", kpMovie.Poster?.PreviewUrl);
        Assert.Equal("2001-11-04T00:00:00.000Z", kpMovie.Premiere?.World);
        Assert.Equal(3, kpMovie.ProductionCompanies?.Count);
        Assert.NotNull(kpMovie.Rating?.Kp);
        Assert.Equal("pg", kpMovie.RatingMpaa);
        Assert.Equal(8, kpMovie.SequelsAndPrequels?.Count);
        Assert.Equal("Путешествие в твою мечту", kpMovie.Slogan);
        Assert.Empty(kpMovie.Videos!.Teasers);
        Assert.Equal(20, kpMovie.Videos!.Trailers.Count);
        Assert.Equal(2001, kpMovie.Year);
        Assert.True(0 < kpMovie.Top250);
    }

    [Fact(Skip = "not in use")]
    public async Task GetMoviesByTop250()
    {
        var request = $"https://api.kinopoisk.dev/v1.3/movie?";
        request += "selectFields=alternativeName externalId id name top250 typeNumber";
        request += "&limit=1000";
        request += "&top250=!null";
        using HttpResponseMessage responseMessage = await _httpClient.GetAsync(new Uri(request)).ConfigureAwait(false);
        _ = responseMessage.EnsureSuccessStatusCode();
        var response = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
        KpSearchResult<KpMovie>? kpMovie = JsonSerializer.Deserialize<KpSearchResult<KpMovie>>(response, _jsonOptions);
        Assert.NotNull(kpMovie);
        Assert.True(490 <= kpMovie!.Docs.Count);
    }

    [Fact(Skip = "not in use")]
    public async Task GetMoviesByExternalIds()
    {
        var request = $"https://api.kinopoisk.dev/v1.3/movie?";
        request += "selectFields=alternativeName externalId.imdb id name&limit=1000";
        request += "&externalId.imdb=tt0241527";
        request += "&externalId.imdb=tt0120689";
        using HttpResponseMessage responseMessage = await _httpClient.GetAsync(new Uri(request)).ConfigureAwait(false);
        _ = responseMessage.EnsureSuccessStatusCode();
        var response = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
        KpSearchResult<KpMovie>? searchResultMovie = JsonSerializer.Deserialize<KpSearchResult<KpMovie>>(response, _jsonOptions);
        Assert.NotNull(searchResultMovie);
        Assert.Equal(2, searchResultMovie!.Docs.Count);

        KpMovie kpMovie = searchResultMovie!.Docs.First(i => i.Id == 689);
        Assert.Equal("Harry Potter and the Sorcerer's Stone", kpMovie.AlternativeName);
        Assert.Equal("tt0241527", kpMovie.ExternalId?.Imdb);
        Assert.Equal(689, kpMovie.Id);
        Assert.Equal("Гарри Поттер и философский камень", kpMovie.Name);

        kpMovie = searchResultMovie!.Docs.First(i => i.Id == 435);
        Assert.NotNull(kpMovie);
        Assert.Equal("The Green Mile", kpMovie!.AlternativeName);
        Assert.Equal("tt0120689", kpMovie.ExternalId?.Imdb);
        Assert.Equal(435, kpMovie.Id);
        Assert.Equal("Зеленая миля", kpMovie.Name);
    }

    [Fact(Skip = "not in use")]
    public async Task GetPersonById()
    {
        var request = $"https://api.kinopoisk.dev/v1/person/7987";
        using HttpResponseMessage responseMessage = await _httpClient.GetAsync(new Uri(request)).ConfigureAwait(false);
        _ = responseMessage.EnsureSuccessStatusCode();
        var response = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
        KpPerson? kpPerson = JsonSerializer.Deserialize<KpPerson>(response, _jsonOptions);
        Assert.NotNull(kpPerson);
        Assert.Equal("1958-10-16T00:00:00.000Z", kpPerson!.Birthday);
        Assert.Equal(3, kpPerson.BirthPlace?.Count);
        Assert.Null(kpPerson.Death);
        Assert.Equal(0, kpPerson.DeathPlace?.Count);
        Assert.Equal(4, kpPerson.Facts?.Count);
        Assert.Equal("Tim Robbins", kpPerson.EnName);
        Assert.Equal(7987, kpPerson.Id);
        Assert.Equal(241, kpPerson.Movies?.Count);
        Assert.Equal("Тим Роббинс", kpPerson.Name);
        Assert.Equal("https://avatars.mds.yandex.net/get-kinopoisk-image/1777765/598f49ce-05ff-4e33-885e-a7f0225f854d/orig", kpPerson.Photo);
    }

    [Fact(Skip = "not in use")]
    public async Task GetPersonByName()
    {
        var request = $"https://api.kinopoisk.dev/v1/person?name=Тим Роббинс";
        using HttpResponseMessage responseMessage = await _httpClient.GetAsync(new Uri(request)).ConfigureAwait(false);
        _ = responseMessage.EnsureSuccessStatusCode();
        var response = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
        KpSearchResult<KpPerson>? searchResultKpPerson = JsonSerializer.Deserialize<KpSearchResult<KpPerson>>(response, _jsonOptions);
        Assert.NotNull(searchResultKpPerson);
        Assert.Equal(2, searchResultKpPerson!.Docs.Count);
        KpPerson kpPerson = searchResultKpPerson.Docs[0];
        Assert.Equal(7987, kpPerson.Id);
        Assert.Equal("Тим Роббинс", kpPerson.Name);
        Assert.Equal("https://avatars.mds.yandex.net/get-kinopoisk-image/1777765/598f49ce-05ff-4e33-885e-a7f0225f854d/orig", kpPerson.Photo);
    }

    [Fact(Skip = "not in use")]
    public async Task GetPersonByMovieId()
    {
        var request = $"https://api.kinopoisk.dev/v1/person?";
        request += "&movies.id=326";
        request += "&selectFields=id movies name";
        request += "&limit=1000";
        using HttpResponseMessage responseMessage = await _httpClient.GetAsync(new Uri(request)).ConfigureAwait(false);
        _ = responseMessage.EnsureSuccessStatusCode();
        var response = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
        KpSearchResult<KpPerson>? searchResultKpPerson = JsonSerializer.Deserialize<KpSearchResult<KpPerson>>(response, _jsonOptions);
        Assert.NotNull(searchResultKpPerson);
        Assert.Equal(112, searchResultKpPerson!.Docs.Count);
        KpPerson? kpPerson = searchResultKpPerson.Docs.FirstOrDefault(i => i.Id == 7987);
        Assert.NotNull(kpPerson);
        Assert.Equal(7987, kpPerson!.Id);
        Assert.Equal("Тим Роббинс", kpPerson.Name);
        Assert.True(241 <= kpPerson.Movies?.Count);
    }

    [Fact(Skip = "not in use")]
    public async Task GetEpisodesBySeriesId()
    {
        var request = $"https://api.kinopoisk.dev/v1/season?";
        request += "movieId=77044";
        request += "&limit=50";
        using HttpResponseMessage responseMessage = await _httpClient.GetAsync(new Uri(request)).ConfigureAwait(false);
        _ = responseMessage.EnsureSuccessStatusCode();
        var response = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
        KpSearchResult<KpSeason>? searchResultKpSeason = JsonSerializer.Deserialize<KpSearchResult<KpSeason>>(response, _jsonOptions);
        Assert.NotNull(searchResultKpSeason);
        Assert.Equal(10, searchResultKpSeason!.Docs.Count);
        KpSeason? kpSeason = searchResultKpSeason.Docs.FirstOrDefault(i => i.Number == 1);
        Assert.NotNull(kpSeason);
        Assert.Equal(77044, kpSeason!.MovieId);
        Assert.Equal(24, kpSeason.Episodes?.Count);
        KpEpisode? kpEpisode = kpSeason.Episodes?.FirstOrDefault(i => i.Number == 1);
        Assert.NotNull(kpEpisode);
        Assert.Equal("1994-09-22", kpEpisode!.Date);
        Assert.Equal("The One Where Monica Gets a Roommate", kpEpisode.EnName);
        Assert.Equal("Эпизод, где Моника берёт новую соседку", kpEpisode.Name);
        Assert.Null(kpEpisode.Description);
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
