using System.Text.Json;

using FluentAssertions;

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
        kpMovie.Should().NotBeNull();
        kpMovie!.AlternativeName.Should().Be("The Green Mile");
        kpMovie.Backdrop?.Url.Should().Be("https://avatars.mds.yandex.net/get-ott/224348/2a00000169e39ef77f588ccdfe574dae8227/orig");
        kpMovie.Backdrop?.PreviewUrl.Should().Be("https://avatars.mds.yandex.net/get-ott/224348/2a00000169e39ef77f588ccdfe574dae8227/x1000");
        kpMovie.Countries?.Count.Should().Be(1);
        kpMovie.Description.Should().Be("Пол Эджкомб — начальник блока смертников в тюрьме «Холодная гора», каждый из узников которого однажды проходит «зеленую милю» по пути к месту казни. Пол повидал много заключённых и надзирателей за время работы. Однако гигант Джон Коффи, обвинённый в страшном преступлении, стал одним из самых необычных обитателей блока.");
        kpMovie.ExternalId?.Imdb.Should().Be("tt0120689");
        kpMovie.ExternalId?.Tmdb.Should().Be(497);
        kpMovie.Genres?.Count.Should().Be(3);
        kpMovie.Id.Should().Be(435);
        kpMovie.Logo?.Url.Should().Be("https://avatars.mds.yandex.net/get-ott/239697/2a0000016f12f1eb8870b609ee94313774b2/orig");
        kpMovie.MovieLength.Should().Be(189);
        kpMovie.Name.Should().Be("Зеленая миля");
        kpMovie.Persons?.Count.Should().Be(87);
        kpMovie.Poster?.Url.Should().Be("https://st.kp.yandex.net/images/film_big/435.jpg");
        kpMovie.Poster?.PreviewUrl.Should().Be("https://st.kp.yandex.net/images/film_iphone/iphone360_435.jpg");
        kpMovie.Premiere?.World.Should().Be("1999-12-06T00:00:00.000Z");
        kpMovie.ProductionCompanies?.Count.Should().Be(4);
        kpMovie.Rating?.Kp.Should().NotBeNull("");
        kpMovie.RatingMpaa.Should().Be("r");
        kpMovie.Slogan.Should().Be("Пол Эджкомб не верил в чудеса. Пока не столкнулся с одним из них");
        kpMovie.Videos?.Teasers.Count.Should().Be(0);
        kpMovie.Videos?.Trailers.Count.Should().Be(2);
        kpMovie.Year.Should().Be(1999);
        kpMovie.Facts?.Count.Should().Be(21);
        kpMovie.SequelsAndPrequels?.Count.Should().Be(0);
        kpMovie.Top250.Should().Be(1);
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
        searchResultMovie.Should().NotBeNull("");
        searchResultMovie!.Docs.Should().ContainSingle();
        KpMovie kpMovie = searchResultMovie!.Docs[0];
        kpMovie.AlternativeName.Should().Be("Harry Potter and the Sorcerer's Stone");
        kpMovie.Backdrop?.Url.Should().Be("https://avatars.mds.yandex.net/get-ott/223007/2a0000016fb3ac87014aae4f0c64329f64e0/orig");
        kpMovie.Backdrop?.PreviewUrl.Should().Be("https://avatars.mds.yandex.net/get-ott/223007/2a0000016fb3ac87014aae4f0c64329f64e0/x1000");
        kpMovie.Countries?.Count.Should().Be(2);
        kpMovie.Description.Should().Be("Жизнь десятилетнего Гарри Поттера нельзя назвать сладкой: родители умерли, едва ему исполнился год, а от дяди и тёти, взявших сироту на воспитание, достаются лишь тычки да подзатыльники. Но в одиннадцатый день рождения Гарри всё меняется. Странный гость, неожиданно появившийся на пороге, приносит письмо, из которого мальчик узнаёт, что на самом деле он - волшебник и зачислен в школу магии под названием Хогвартс. А уже через пару недель Гарри будет мчаться в поезде Хогвартс-экспресс навстречу новой жизни, где его ждут невероятные приключения, верные друзья и самое главное — ключ к разгадке тайны смерти его родителей.");
        kpMovie.ExternalId?.Imdb.Should().Be("tt0241527");
        kpMovie.ExternalId?.Tmdb.Should().Be(671);
        kpMovie.Facts?.Count.Should().Be(52);
        kpMovie.Genres?.Count.Should().Be(3);
        kpMovie.Id.Should().Be(689);
        kpMovie.Logo?.Url.Should().Be("https://avatars.mds.yandex.net/get-ott/223007/2a0000017e127a46aa2122ff48cb306de98b/orig");
        kpMovie.MovieLength.Should().Be(152);
        kpMovie.Name.Should().Be("Гарри Поттер и философский камень");
        kpMovie.Persons?.Count.Should().Be(173);
        kpMovie.Poster?.Url.Should().Be("https://st.kp.yandex.net/images/film_big/689.jpg");
        kpMovie.Poster?.PreviewUrl.Should().Be("https://st.kp.yandex.net/images/film_iphone/iphone360_689.jpg");
        kpMovie.Premiere?.World.Should().Be("2001-11-04T00:00:00.000Z");
        kpMovie.ProductionCompanies?.Count.Should().Be(3);
        kpMovie.Rating?.Kp.Should().NotBeNull("");
        kpMovie.RatingMpaa.Should().Be("pg");
        kpMovie.SequelsAndPrequels?.Count.Should().Be(8);
        kpMovie.Slogan.Should().Be("Путешествие в твою мечту");
        kpMovie.Videos!.Teasers.Should().BeEmpty("");
        kpMovie.Videos!.Trailers.Count.Should().Be(20);
        kpMovie.Year.Should().Be(2001);
        kpMovie.Top250.Should().BeGreaterThan(0);
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
        searchResultMovie.Should().NotBeNull("");
        searchResultMovie!.Docs.Count.Should().Be(2);

        KpMovie kpMovie = searchResultMovie!.Docs.First(i => i.Id == 689);
        kpMovie.AlternativeName.Should().Be("Harry Potter and the Sorcerer's Stone");
        kpMovie.Backdrop?.Url.Should().Be("https://avatars.mds.yandex.net/get-ott/223007/2a0000016fb3ac87014aae4f0c64329f64e0/orig");
        kpMovie.Backdrop?.PreviewUrl.Should().Be("https://avatars.mds.yandex.net/get-ott/223007/2a0000016fb3ac87014aae4f0c64329f64e0/x1000");
        kpMovie.Countries?.Count.Should().Be(2);
        kpMovie.Description.Should().Be("Жизнь десятилетнего Гарри Поттера нельзя назвать сладкой: родители умерли, едва ему исполнился год, а от дяди и тёти, взявших сироту на воспитание, достаются лишь тычки да подзатыльники. Но в одиннадцатый день рождения Гарри всё меняется. Странный гость, неожиданно появившийся на пороге, приносит письмо, из которого мальчик узнаёт, что на самом деле он - волшебник и зачислен в школу магии под названием Хогвартс. А уже через пару недель Гарри будет мчаться в поезде Хогвартс-экспресс навстречу новой жизни, где его ждут невероятные приключения, верные друзья и самое главное — ключ к разгадке тайны смерти его родителей.");
        kpMovie.ExternalId?.Imdb.Should().Be("tt0241527");
        kpMovie.ExternalId?.Tmdb.Should().Be(671);
        kpMovie.Facts?.Count.Should().Be(52);
        kpMovie.Genres?.Count.Should().Be(3);
        kpMovie.Id.Should().Be(689);
        kpMovie.Logo?.Url.Should().Be("https://avatars.mds.yandex.net/get-ott/223007/2a0000017e127a46aa2122ff48cb306de98b/orig");
        kpMovie.MovieLength.Should().Be(152);
        kpMovie.Name.Should().Be("Гарри Поттер и философский камень");
        kpMovie.Persons?.Count.Should().Be(173);
        kpMovie.Poster?.Url.Should().Be("https://st.kp.yandex.net/images/film_big/689.jpg");
        kpMovie.Poster?.PreviewUrl.Should().Be("https://st.kp.yandex.net/images/film_iphone/iphone360_689.jpg");
        kpMovie.Premiere?.World.Should().Be("2001-11-04T00:00:00.000Z");
        kpMovie.ProductionCompanies?.Count.Should().Be(3);
        kpMovie.Rating?.Kp.Should().NotBeNull("");
        kpMovie.RatingMpaa.Should().Be("pg");
        kpMovie.SequelsAndPrequels.Count.Should().Be(8);
        kpMovie.Slogan.Should().Be("Путешествие в твою мечту");
        kpMovie.Videos!.Teasers.Should().BeEmpty("");
        kpMovie.Videos!.Trailers.Count.Should().Be(20);
        kpMovie.Year.Should().Be(2001);
        kpMovie.Top250.Should().BeGreaterThan(0);

        kpMovie = searchResultMovie!.Docs.First(i => i.Id == 435);
        kpMovie.Should().NotBeNull("");
        kpMovie!.AlternativeName.Should().Be("The Green Mile");
        kpMovie.Backdrop?.Url.Should().Be("https://avatars.mds.yandex.net/get-ott/224348/2a00000169e39ef77f588ccdfe574dae8227/orig");
        kpMovie.Backdrop?.PreviewUrl.Should().Be("https://avatars.mds.yandex.net/get-ott/224348/2a00000169e39ef77f588ccdfe574dae8227/x1000");
        kpMovie.Countries?.Count.Should().Be(1);
        kpMovie.Description.Should().Be("Пол Эджкомб — начальник блока смертников в тюрьме «Холодная гора», каждый из узников которого однажды проходит «зеленую милю» по пути к месту казни. Пол повидал много заключённых и надзирателей за время работы. Однако гигант Джон Коффи, обвинённый в страшном преступлении, стал одним из самых необычных обитателей блока.");
        kpMovie.ExternalId?.Imdb.Should().Be("tt0120689");
        kpMovie.ExternalId?.Tmdb.Should().Be(497);
        kpMovie.Genres?.Count.Should().Be(3);
        kpMovie.Id.Should().Be(435);
        kpMovie.Logo?.Url.Should().Be("https://avatars.mds.yandex.net/get-ott/239697/2a0000016f12f1eb8870b609ee94313774b2/orig");
        kpMovie.MovieLength.Should().Be(189);
        kpMovie.Name.Should().Be("Зеленая миля");
        kpMovie.Persons?.Count.Should().Be(87);
        kpMovie.Poster?.Url.Should().Be("https://st.kp.yandex.net/images/film_big/435.jpg");
        kpMovie.Poster?.PreviewUrl.Should().Be("https://st.kp.yandex.net/images/film_iphone/iphone360_435.jpg");
        kpMovie.Premiere?.World.Should().Be("1999-12-06T00:00:00.000Z");
        kpMovie.ProductionCompanies?.Count.Should().Be(4);
        kpMovie.Rating?.Kp.Should().NotBeNull("");
        kpMovie.RatingMpaa.Should().Be("r");
        kpMovie.Slogan.Should().Be("Пол Эджкомб не верил в чудеса. Пока не столкнулся с одним из них");
        kpMovie.Videos?.Teasers.Count.Should().Be(0);
        kpMovie.Videos?.Trailers.Count.Should().Be(2);
        kpMovie.Year.Should().Be(1999);
        kpMovie.Facts?.Count.Should().Be(21);
        kpMovie.SequelsAndPrequels.Should().BeEmpty("");
        kpMovie.Top250.Should().Be(1);
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
        searchResultMovie.Should().NotBeNull("");
        searchResultMovie!.Docs.Should().ContainSingle();
        KpMovie kpMovie = searchResultMovie!.Docs[0];
        kpMovie.AlternativeName.Should().Be("Harry Potter and the Sorcerer's Stone");
        kpMovie.Backdrop?.Url.Should().Be("https://avatars.mds.yandex.net/get-ott/223007/2a0000016fb3ac87014aae4f0c64329f64e0/orig");
        kpMovie.Backdrop?.PreviewUrl.Should().Be("https://avatars.mds.yandex.net/get-ott/223007/2a0000016fb3ac87014aae4f0c64329f64e0/x1000");
        kpMovie.Countries?.Count.Should().Be(2);
        kpMovie.Description.Should().Be("Жизнь десятилетнего Гарри Поттера нельзя назвать сладкой: родители умерли, едва ему исполнился год, а от дяди и тёти, взявших сироту на воспитание, достаются лишь тычки да подзатыльники. Но в одиннадцатый день рождения Гарри всё меняется. Странный гость, неожиданно появившийся на пороге, приносит письмо, из которого мальчик узнаёт, что на самом деле он - волшебник и зачислен в школу магии под названием Хогвартс. А уже через пару недель Гарри будет мчаться в поезде Хогвартс-экспресс навстречу новой жизни, где его ждут невероятные приключения, верные друзья и самое главное — ключ к разгадке тайны смерти его родителей.");
        kpMovie.ExternalId?.Imdb.Should().Be("tt0241527");
        kpMovie.ExternalId?.Tmdb.Should().Be(671);
        kpMovie.Facts?.Count.Should().Be(52);
        kpMovie.Genres?.Count.Should().Be(3);
        kpMovie.Id.Should().Be(689);
        kpMovie.Logo?.Url.Should().Be("https://avatars.mds.yandex.net/get-ott/223007/2a0000017e127a46aa2122ff48cb306de98b/orig");
        kpMovie.MovieLength.Should().Be(152);
        kpMovie.Name.Should().Be("Гарри Поттер и философский камень");
        kpMovie.Persons?.Count.Should().Be(173);
        kpMovie.Poster?.Url.Should().Be("https://st.kp.yandex.net/images/film_big/689.jpg");
        kpMovie.Poster?.PreviewUrl.Should().Be("https://st.kp.yandex.net/images/film_iphone/iphone360_689.jpg");
        kpMovie.Premiere?.World.Should().Be("2001-11-04T00:00:00.000Z");
        kpMovie.ProductionCompanies?.Count.Should().Be(3);
        kpMovie.Rating?.Kp.Should().NotBeNull("");
        kpMovie.RatingMpaa.Should().Be("pg");
        kpMovie.SequelsAndPrequels?.Count.Should().Be(8);
        kpMovie.Slogan.Should().Be("Путешествие в твою мечту");
        kpMovie.Videos!.Teasers.Should().BeEmpty("");
        kpMovie.Videos!.Trailers.Count.Should().Be(20);
        kpMovie.Year.Should().Be(2001);
        kpMovie.Top250.Should().BeGreaterThan(0);
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
        kpMovie.Should().NotBeNull("");
        kpMovie!.Docs.Count.Should().BeGreaterThanOrEqualTo(490);
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
        searchResultMovie.Should().NotBeNull("");
        searchResultMovie!.Docs.Count.Should().Be(2);

        KpMovie kpMovie = searchResultMovie!.Docs.First(i => i.Id == 689);
        kpMovie.AlternativeName.Should().Be("Harry Potter and the Sorcerer's Stone");
        kpMovie.ExternalId?.Imdb.Should().Be("tt0241527");
        kpMovie.Id.Should().Be(689);
        kpMovie.Name.Should().Be("Гарри Поттер и философский камень");

        kpMovie = searchResultMovie!.Docs.First(i => i.Id == 435);
        kpMovie.Should().NotBeNull("");
        kpMovie!.AlternativeName.Should().Be("The Green Mile");
        kpMovie.ExternalId?.Imdb.Should().Be("tt0120689");
        kpMovie.Id.Should().Be(435);
        kpMovie.Name.Should().Be("Зеленая миля");
    }

    [Fact(Skip = "not in use")]
    public async Task GetPersonById()
    {
        var request = $"https://api.kinopoisk.dev/v1/person/7987";
        using HttpResponseMessage responseMessage = await _httpClient.GetAsync(new Uri(request)).ConfigureAwait(false);
        _ = responseMessage.EnsureSuccessStatusCode();
        var response = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
        KpPerson? kpPerson = JsonSerializer.Deserialize<KpPerson>(response, _jsonOptions);
        kpPerson.Should().NotBeNull("");
        kpPerson!.Birthday.Should().Be("1958-10-16T00:00:00.000Z");
        kpPerson.BirthPlace?.Count.Should().Be(3);
        kpPerson.Death.Should().BeNull("person still alive");
        kpPerson.DeathPlace?.Count.Should().Be(0);
        kpPerson.Facts?.Count.Should().Be(4);
        kpPerson.EnName.Should().Be("Tim Robbins");
        kpPerson.Id.Should().Be(7987);
        kpPerson.Movies?.Count.Should().Be(241);
        kpPerson.Name.Should().Be("Тим Роббинс");
        kpPerson.Photo.Should().Be("https://avatars.mds.yandex.net/get-kinopoisk-image/1777765/598f49ce-05ff-4e33-885e-a7f0225f854d/orig");
    }

    [Fact(Skip = "not in use")]
    public async Task GetPersonByName()
    {
        var request = $"https://api.kinopoisk.dev/v1/person?name=Тим Роббинс";
        using HttpResponseMessage responseMessage = await _httpClient.GetAsync(new Uri(request)).ConfigureAwait(false);
        _ = responseMessage.EnsureSuccessStatusCode();
        var response = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
        KpSearchResult<KpPerson>? searchResultKpPerson = JsonSerializer.Deserialize<KpSearchResult<KpPerson>>(response, _jsonOptions);
        searchResultKpPerson.Should().NotBeNull("");
        searchResultKpPerson!.Docs.Count.Should().Be(2);
        KpPerson kpPerson = searchResultKpPerson.Docs[0];
        kpPerson.Id.Should().Be(7987);
        kpPerson.Name.Should().Be("Тим Роббинс");
        kpPerson.Photo.Should().Be("https://avatars.mds.yandex.net/get-kinopoisk-image/1777765/598f49ce-05ff-4e33-885e-a7f0225f854d/orig");
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
        searchResultKpPerson.Should().NotBeNull("");
        searchResultKpPerson!.Docs.Count.Should().Be(112);
        KpPerson? kpPerson = searchResultKpPerson.Docs.FirstOrDefault(i => i.Id == 7987);
        kpPerson.Should().NotBeNull("");
        kpPerson!.Id.Should().Be(7987);
        kpPerson.Name.Should().Be("Тим Роббинс");
        kpPerson.Movies?.Count.Should().BeGreaterThanOrEqualTo(241);
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
        searchResultKpSeason.Should().NotBeNull("");
        searchResultKpSeason!.Docs.Count.Should().Be(10);
        KpSeason? kpSeason = searchResultKpSeason.Docs.FirstOrDefault(i => i.Number == 1);
        kpSeason.Should().NotBeNull("");
        kpSeason!.MovieId.Should().Be(77044);
        kpSeason.Episodes?.Count.Should().Be(24);
        KpEpisode? kpEpisode = kpSeason.Episodes?.FirstOrDefault(i => i.Number == 1);
        kpEpisode.Should().NotBeNull("");
        kpEpisode!.Date.Should().Be("1994-09-22");
        kpEpisode.EnName.Should().Be("The One Where Monica Gets a Roommate");
        kpEpisode.Name.Should().Be("Эпизод, где Моника берёт новую соседку");
        kpEpisode.Description.Should().BeNull("there is no description loaded from API");
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
