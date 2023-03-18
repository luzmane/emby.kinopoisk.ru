using System;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

using EmbyKinopoiskRu.Api.KinopoiskDev.Model;
using EmbyKinopoiskRu.Api.KinopoiskDev.Model.Movie;
using EmbyKinopoiskRu.Api.KinopoiskDev.Model.Person;
using EmbyKinopoiskRu.Api.KinopoiskDev.Model.Season;

using NUnit.Framework;

namespace EmbyKinopoiskRu.Tests
{
    /// <summary>
    /// Swagger documentation: 
    ///     https://api.kinopoisk.dev/v1/documentation-json
    ///     https://api.kinopoisk.dev/v1/documentation-yaml
    /// </summary>
    [TestFixture]
    public class DevApiTests
    {
        private const string Token = "8DA0EV2-KTP4A5Q-G67QP3K-S2VFBX7";
        private static readonly HttpClient HttpClient = new();
        private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

        [OneTimeSetUp]
        public void Init()
        {
            HttpClient.DefaultRequestHeaders.Add("X-API-KEY", Token);
        }


        [Test]
        public async Task GetMovieById()
        {
            var request = new Uri("https://api.kinopoisk.dev/v1/movie/435");
            using HttpResponseMessage responseMessage = await HttpClient.GetAsync(request).ConfigureAwait(false);
            _ = responseMessage.EnsureSuccessStatusCode();
            var response = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
            KpMovie? kpMovie = JsonSerializer.Deserialize<KpMovie>(response, JsonOptions);
            Assert.NotNull(kpMovie);
            Assert.AreEqual("The Green Mile", kpMovie!.AlternativeName);
            Assert.AreEqual("https://avatars.mds.yandex.net/get-ott/224348/2a00000169e39ef77f588ccdfe574dae8227/orig", kpMovie.Backdrop?.Url);
            Assert.AreEqual("https://avatars.mds.yandex.net/get-ott/224348/2a00000169e39ef77f588ccdfe574dae8227/x1000", kpMovie.Backdrop?.PreviewUrl);
            Assert.AreEqual(1, kpMovie.Countries?.Count);
            Assert.AreEqual("Пол Эджкомб — начальник блока смертников в тюрьме «Холодная гора», каждый из узников которого однажды проходит «зеленую милю» по пути к месту казни. Пол повидал много заключённых и надзирателей за время работы. Однако гигант Джон Коффи, обвинённый в страшном преступлении, стал одним из самых необычных обитателей блока.", kpMovie.Description);
            Assert.AreEqual("tt0120689", kpMovie.ExternalId?.Imdb);
            Assert.AreEqual(497, kpMovie.ExternalId?.Tmdb);
            Assert.AreEqual(3, kpMovie.Genres?.Count);
            Assert.AreEqual(435, kpMovie.Id);
            Assert.AreEqual("https://avatars.mds.yandex.net/get-ott/239697/2a0000016f12f1eb8870b609ee94313774b2/orig", kpMovie.Logo?.Url);
            Assert.AreEqual(189, kpMovie.MovieLength);
            Assert.AreEqual("Зеленая миля", kpMovie.Name);
            Assert.AreEqual(87, kpMovie.Persons?.Count);
            Assert.AreEqual("https://st.kp.yandex.net/images/film_big/435.jpg", kpMovie.Poster?.Url);
            Assert.AreEqual("https://st.kp.yandex.net/images/film_iphone/iphone360_435.jpg", kpMovie.Poster?.PreviewUrl);
            Assert.AreEqual("1999-12-06T00:00:00.000Z", kpMovie.Premiere?.World);
            Assert.AreEqual(4, kpMovie.ProductionCompanies?.Count);
            Assert.IsNotNull(kpMovie.Rating?.Kp);
            Assert.IsNotNull(kpMovie.Rating?.FilmCritics);
            Assert.AreEqual("r", kpMovie.RatingMpaa);
            Assert.AreEqual("Пол Эджкомб не верил в чудеса. Пока не столкнулся с одним из них", kpMovie.Slogan);
            Assert.AreEqual(0, kpMovie.Videos?.Teasers.Count);
            Assert.AreEqual(2, kpMovie.Videos?.Trailers.Count);
            Assert.AreEqual(1999, kpMovie.Year);
            Assert.AreEqual(21, kpMovie.Facts?.Count);
            Assert.AreEqual(0, kpMovie.SequelsAndPrequels?.Count);
            Assert.AreEqual(1, kpMovie.Top250);
        }

        [Test]
        public async Task GetMoviesByMovieDetailsNameYear()
        {
            var request = $"https://api.kinopoisk.dev/v1/movie?";
            request += "&limit=50";
            request += "&selectFields=alternativeName backdrop countries description enName externalId genres id logo movieLength name persons poster premiere productionCompanies rating ratingMpaa slogan videos year sequelsAndPrequels top250 facts releaseYears seasonsInfo";
            request += "&name=Гарри Поттер и философский камень";
            request += "&year=2001"; // 689
            using HttpResponseMessage responseMessage = await HttpClient.GetAsync(new Uri(request)).ConfigureAwait(false);
            _ = responseMessage.EnsureSuccessStatusCode();
            var response = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
            KpSearchResult<KpMovie>? searchResultMovie = JsonSerializer.Deserialize<KpSearchResult<KpMovie>>(response, JsonOptions);
            Assert.NotNull(searchResultMovie);
            Assert.AreEqual(1, searchResultMovie!.Docs.Count);
            KpMovie kpMovie = searchResultMovie!.Docs[0];
            Assert.AreEqual("Harry Potter and the Sorcerer's Stone", kpMovie.AlternativeName);
            Assert.AreEqual("https://avatars.mds.yandex.net/get-ott/223007/2a0000016fb3ac87014aae4f0c64329f64e0/orig", kpMovie.Backdrop?.Url);
            Assert.AreEqual("https://avatars.mds.yandex.net/get-ott/223007/2a0000016fb3ac87014aae4f0c64329f64e0/x1000", kpMovie.Backdrop?.PreviewUrl);
            Assert.AreEqual(2, kpMovie.Countries?.Count);
            Assert.AreEqual("Жизнь десятилетнего Гарри Поттера нельзя назвать сладкой: родители умерли, едва ему исполнился год, а от дяди и тёти, взявших сироту на воспитание, достаются лишь тычки да подзатыльники. Но в одиннадцатый день рождения Гарри всё меняется. Странный гость, неожиданно появившийся на пороге, приносит письмо, из которого мальчик узнаёт, что на самом деле он - волшебник и зачислен в школу магии под названием Хогвартс. А уже через пару недель Гарри будет мчаться в поезде Хогвартс-экспресс навстречу новой жизни, где его ждут невероятные приключения, верные друзья и самое главное — ключ к разгадке тайны смерти его родителей.", kpMovie.Description);
            Assert.AreEqual("tt0241527", kpMovie.ExternalId?.Imdb);
            Assert.AreEqual(671, kpMovie.ExternalId?.Tmdb);
            Assert.AreEqual(52, kpMovie.Facts?.Count);
            Assert.AreEqual(3, kpMovie.Genres?.Count);
            Assert.AreEqual(689, kpMovie.Id);
            Assert.AreEqual("https://avatars.mds.yandex.net/get-ott/223007/2a0000017e127a46aa2122ff48cb306de98b/orig", kpMovie.Logo?.Url);
            Assert.AreEqual(152, kpMovie.MovieLength);
            Assert.AreEqual("Гарри Поттер и философский камень", kpMovie.Name);
            Assert.AreEqual(173, kpMovie.Persons?.Count);
            Assert.AreEqual("https://st.kp.yandex.net/images/film_big/689.jpg", kpMovie.Poster?.Url);
            Assert.AreEqual("https://st.kp.yandex.net/images/film_iphone/iphone360_689.jpg", kpMovie.Poster?.PreviewUrl);
            Assert.AreEqual("2001-11-04T00:00:00.000Z", kpMovie.Premiere?.World);
            Assert.AreEqual(3, kpMovie.ProductionCompanies?.Count);
            Assert.IsNotNull(kpMovie.Rating?.Kp);
            Assert.IsNotNull(kpMovie.Rating?.FilmCritics);
            Assert.AreEqual("pg", kpMovie.RatingMpaa);
            Assert.AreEqual(8, kpMovie.SequelsAndPrequels?.Count);
            Assert.AreEqual("Путешествие в твою мечту", kpMovie.Slogan);
            Assert.AreEqual(0, kpMovie.Videos!.Teasers.Count);
            Assert.AreEqual(20, kpMovie.Videos!.Trailers.Count);
            Assert.AreEqual(2001, kpMovie.Year);
            Assert.Less(0, kpMovie.Top250);
        }

        [Test]
        public async Task GetMoviesByMovieIds()
        {
            var request = $"https://api.kinopoisk.dev/v1/movie?";
            request += "&limit=50";
            request += "&selectFields=alternativeName backdrop countries description enName externalId genres id logo movieLength name persons poster premiere productionCompanies rating ratingMpaa slogan videos year sequelsAndPrequels top250 facts releaseYears seasonsInfo";
            request += "&id=689&id=435";
            using HttpResponseMessage responseMessage = await HttpClient.GetAsync(new Uri(request)).ConfigureAwait(false);
            _ = responseMessage.EnsureSuccessStatusCode();
            var response = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
            KpSearchResult<KpMovie>? searchResultMovie = JsonSerializer.Deserialize<KpSearchResult<KpMovie>>(response, JsonOptions);
            Assert.NotNull(searchResultMovie);
            Assert.AreEqual(2, searchResultMovie!.Docs.Count);

            KpMovie kpMovie = searchResultMovie!.Docs.First(i => i.Id == 689);
            Assert.AreEqual("Harry Potter and the Sorcerer's Stone", kpMovie.AlternativeName);
            Assert.AreEqual("https://avatars.mds.yandex.net/get-ott/223007/2a0000016fb3ac87014aae4f0c64329f64e0/orig", kpMovie.Backdrop?.Url);
            Assert.AreEqual("https://avatars.mds.yandex.net/get-ott/223007/2a0000016fb3ac87014aae4f0c64329f64e0/x1000", kpMovie.Backdrop?.PreviewUrl);
            Assert.AreEqual(2, kpMovie.Countries?.Count);
            Assert.AreEqual("Жизнь десятилетнего Гарри Поттера нельзя назвать сладкой: родители умерли, едва ему исполнился год, а от дяди и тёти, взявших сироту на воспитание, достаются лишь тычки да подзатыльники. Но в одиннадцатый день рождения Гарри всё меняется. Странный гость, неожиданно появившийся на пороге, приносит письмо, из которого мальчик узнаёт, что на самом деле он - волшебник и зачислен в школу магии под названием Хогвартс. А уже через пару недель Гарри будет мчаться в поезде Хогвартс-экспресс навстречу новой жизни, где его ждут невероятные приключения, верные друзья и самое главное — ключ к разгадке тайны смерти его родителей.", kpMovie.Description);
            Assert.AreEqual("tt0241527", kpMovie.ExternalId?.Imdb);
            Assert.AreEqual(671, kpMovie.ExternalId?.Tmdb);
            Assert.AreEqual(52, kpMovie.Facts?.Count);
            Assert.AreEqual(3, kpMovie.Genres?.Count);
            Assert.AreEqual(689, kpMovie.Id);
            Assert.AreEqual("https://avatars.mds.yandex.net/get-ott/223007/2a0000017e127a46aa2122ff48cb306de98b/orig", kpMovie.Logo?.Url);
            Assert.AreEqual(152, kpMovie.MovieLength);
            Assert.AreEqual("Гарри Поттер и философский камень", kpMovie.Name);
            Assert.AreEqual(173, kpMovie.Persons?.Count);
            Assert.AreEqual("https://st.kp.yandex.net/images/film_big/689.jpg", kpMovie.Poster?.Url);
            Assert.AreEqual("https://st.kp.yandex.net/images/film_iphone/iphone360_689.jpg", kpMovie.Poster?.PreviewUrl);
            Assert.AreEqual("2001-11-04T00:00:00.000Z", kpMovie.Premiere?.World);
            Assert.AreEqual(3, kpMovie.ProductionCompanies?.Count);
            Assert.IsNotNull(kpMovie.Rating?.Kp);
            Assert.IsNotNull(kpMovie.Rating?.FilmCritics);
            Assert.AreEqual("pg", kpMovie.RatingMpaa);
            Assert.AreEqual(8, kpMovie.SequelsAndPrequels.Count);
            Assert.AreEqual("Путешествие в твою мечту", kpMovie.Slogan);
            Assert.AreEqual(0, kpMovie.Videos!.Teasers.Count);
            Assert.AreEqual(20, kpMovie.Videos!.Trailers.Count);
            Assert.AreEqual(2001, kpMovie.Year);
            Assert.Less(0, kpMovie.Top250);

            kpMovie = searchResultMovie!.Docs.First(i => i.Id == 435);
            Assert.NotNull(kpMovie);
            Assert.AreEqual("The Green Mile", kpMovie!.AlternativeName);
            Assert.AreEqual("https://avatars.mds.yandex.net/get-ott/224348/2a00000169e39ef77f588ccdfe574dae8227/orig", kpMovie.Backdrop?.Url);
            Assert.AreEqual("https://avatars.mds.yandex.net/get-ott/224348/2a00000169e39ef77f588ccdfe574dae8227/x1000", kpMovie.Backdrop?.PreviewUrl);
            Assert.AreEqual(1, kpMovie.Countries?.Count);
            Assert.AreEqual("Пол Эджкомб — начальник блока смертников в тюрьме «Холодная гора», каждый из узников которого однажды проходит «зеленую милю» по пути к месту казни. Пол повидал много заключённых и надзирателей за время работы. Однако гигант Джон Коффи, обвинённый в страшном преступлении, стал одним из самых необычных обитателей блока.", kpMovie.Description);
            Assert.AreEqual("tt0120689", kpMovie.ExternalId?.Imdb);
            Assert.AreEqual(497, kpMovie.ExternalId?.Tmdb);
            Assert.AreEqual(3, kpMovie.Genres?.Count);
            Assert.AreEqual(435, kpMovie.Id);
            Assert.AreEqual("https://avatars.mds.yandex.net/get-ott/239697/2a0000016f12f1eb8870b609ee94313774b2/orig", kpMovie.Logo?.Url);
            Assert.AreEqual(189, kpMovie.MovieLength);
            Assert.AreEqual("Зеленая миля", kpMovie.Name);
            Assert.AreEqual(87, kpMovie.Persons?.Count);
            Assert.AreEqual("https://st.kp.yandex.net/images/film_big/435.jpg", kpMovie.Poster?.Url);
            Assert.AreEqual("https://st.kp.yandex.net/images/film_iphone/iphone360_435.jpg", kpMovie.Poster?.PreviewUrl);
            Assert.AreEqual("1999-12-06T00:00:00.000Z", kpMovie.Premiere?.World);
            Assert.AreEqual(4, kpMovie.ProductionCompanies?.Count);
            Assert.IsNotNull(kpMovie.Rating?.Kp);
            Assert.IsNotNull(kpMovie.Rating?.FilmCritics);
            Assert.AreEqual("r", kpMovie.RatingMpaa);
            Assert.AreEqual("Пол Эджкомб не верил в чудеса. Пока не столкнулся с одним из них", kpMovie.Slogan);
            Assert.AreEqual(0, kpMovie.Videos?.Teasers.Count);
            Assert.AreEqual(2, kpMovie.Videos?.Trailers.Count);
            Assert.AreEqual(1999, kpMovie.Year);
            Assert.AreEqual(21, kpMovie.Facts?.Count);
            Assert.AreEqual(0, kpMovie.SequelsAndPrequels.Count);
            Assert.AreEqual(1, kpMovie.Top250);
        }

        [Test]
        public async Task GetMoviesByMovieDetailsAlternativeNameYear()
        {
            var request = $"https://api.kinopoisk.dev/v1/movie?";
            request += "&limit=50";
            request += "&selectFields=alternativeName backdrop countries description enName externalId genres id logo movieLength name persons poster premiere productionCompanies rating ratingMpaa slogan videos year sequelsAndPrequels top250 facts releaseYears seasonsInfo";
            request += "&alternativeName=Harry Potter and the Sorcerer's Stone";
            request += "&year=2001"; // 689
            using HttpResponseMessage responseMessage = await HttpClient.GetAsync(new Uri(request)).ConfigureAwait(false);
            _ = responseMessage.EnsureSuccessStatusCode();
            var response = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
            KpSearchResult<KpMovie>? searchResultMovie = JsonSerializer.Deserialize<KpSearchResult<KpMovie>>(response, JsonOptions);
            Assert.NotNull(searchResultMovie);
            Assert.AreEqual(1, searchResultMovie!.Docs.Count);
            KpMovie kpMovie = searchResultMovie!.Docs[0];
            Assert.AreEqual("Harry Potter and the Sorcerer's Stone", kpMovie.AlternativeName);
            Assert.AreEqual("https://avatars.mds.yandex.net/get-ott/223007/2a0000016fb3ac87014aae4f0c64329f64e0/orig", kpMovie.Backdrop?.Url);
            Assert.AreEqual("https://avatars.mds.yandex.net/get-ott/223007/2a0000016fb3ac87014aae4f0c64329f64e0/x1000", kpMovie.Backdrop?.PreviewUrl);
            Assert.AreEqual(2, kpMovie.Countries?.Count);
            Assert.AreEqual("Жизнь десятилетнего Гарри Поттера нельзя назвать сладкой: родители умерли, едва ему исполнился год, а от дяди и тёти, взявших сироту на воспитание, достаются лишь тычки да подзатыльники. Но в одиннадцатый день рождения Гарри всё меняется. Странный гость, неожиданно появившийся на пороге, приносит письмо, из которого мальчик узнаёт, что на самом деле он - волшебник и зачислен в школу магии под названием Хогвартс. А уже через пару недель Гарри будет мчаться в поезде Хогвартс-экспресс навстречу новой жизни, где его ждут невероятные приключения, верные друзья и самое главное — ключ к разгадке тайны смерти его родителей.", kpMovie.Description);
            Assert.AreEqual("tt0241527", kpMovie.ExternalId?.Imdb);
            Assert.AreEqual(671, kpMovie.ExternalId?.Tmdb);
            Assert.AreEqual(52, kpMovie.Facts?.Count);
            Assert.AreEqual(3, kpMovie.Genres?.Count);
            Assert.AreEqual(689, kpMovie.Id);
            Assert.AreEqual("https://avatars.mds.yandex.net/get-ott/223007/2a0000017e127a46aa2122ff48cb306de98b/orig", kpMovie.Logo?.Url);
            Assert.AreEqual(152, kpMovie.MovieLength);
            Assert.AreEqual("Гарри Поттер и философский камень", kpMovie.Name);
            Assert.AreEqual(173, kpMovie.Persons?.Count);
            Assert.AreEqual("https://st.kp.yandex.net/images/film_big/689.jpg", kpMovie.Poster?.Url);
            Assert.AreEqual("https://st.kp.yandex.net/images/film_iphone/iphone360_689.jpg", kpMovie.Poster?.PreviewUrl);
            Assert.AreEqual("2001-11-04T00:00:00.000Z", kpMovie.Premiere?.World);
            Assert.AreEqual(3, kpMovie.ProductionCompanies?.Count);
            Assert.IsNotNull(kpMovie.Rating?.Kp);
            Assert.IsNotNull(kpMovie.Rating?.FilmCritics);
            Assert.AreEqual("pg", kpMovie.RatingMpaa);
            Assert.AreEqual(8, kpMovie.SequelsAndPrequels?.Count);
            Assert.AreEqual("Путешествие в твою мечту", kpMovie.Slogan);
            Assert.AreEqual(0, kpMovie.Videos!.Teasers.Count);
            Assert.AreEqual(20, kpMovie.Videos!.Trailers.Count);
            Assert.AreEqual(2001, kpMovie.Year);
            Assert.Less(0, kpMovie.Top250);
        }

        [Test]
        public async Task GetMoviesByTop250()
        {
            var request = $"https://api.kinopoisk.dev/v1/movie?";
            request += "selectFields=alternativeName externalId id name top250 typeNumber";
            request += "&limit=1000";
            request += "&top250=!null";
            using HttpResponseMessage responseMessage = await HttpClient.GetAsync(new Uri(request)).ConfigureAwait(false);
            _ = responseMessage.EnsureSuccessStatusCode();
            var response = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
            KpSearchResult<KpMovie>? kpMovie = JsonSerializer.Deserialize<KpSearchResult<KpMovie>>(response, JsonOptions);
            Assert.IsNotNull(kpMovie);
            Assert.LessOrEqual(499, kpMovie!.Docs.Count);
        }

        [Test]
        public async Task GetMoviesByExternalIds()
        {
            var request = $"https://api.kinopoisk.dev/v1/movie?";
            request += "selectFields=alternativeName externalId.imdb id name&limit=1000";
            request += "&externalId.imdb=tt0241527";
            request += "&externalId.imdb=tt0120689";
            using HttpResponseMessage responseMessage = await HttpClient.GetAsync(new Uri(request)).ConfigureAwait(false);
            _ = responseMessage.EnsureSuccessStatusCode();
            var response = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
            KpSearchResult<KpMovie>? searchResultMovie = JsonSerializer.Deserialize<KpSearchResult<KpMovie>>(response, JsonOptions);
            Assert.IsNotNull(searchResultMovie);
            Assert.AreEqual(2, searchResultMovie!.Docs.Count);

            KpMovie kpMovie = searchResultMovie!.Docs.First(i => i.Id == 689);
            Assert.AreEqual("Harry Potter and the Sorcerer's Stone", kpMovie.AlternativeName);
            Assert.AreEqual("tt0241527", kpMovie.ExternalId?.Imdb);
            Assert.AreEqual(689, kpMovie.Id);
            Assert.AreEqual("Гарри Поттер и философский камень", kpMovie.Name);

            kpMovie = searchResultMovie!.Docs.First(i => i.Id == 435);
            Assert.NotNull(kpMovie);
            Assert.AreEqual("The Green Mile", kpMovie!.AlternativeName);
            Assert.AreEqual("tt0120689", kpMovie.ExternalId?.Imdb);
            Assert.AreEqual(435, kpMovie.Id);
            Assert.AreEqual("Зеленая миля", kpMovie.Name);
        }

        [Test]
        public async Task GetPersonById()
        {
            var request = $"https://api.kinopoisk.dev/v1/person/7987";
            using HttpResponseMessage responseMessage = await HttpClient.GetAsync(new Uri(request)).ConfigureAwait(false);
            _ = responseMessage.EnsureSuccessStatusCode();
            var response = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
            KpPerson? kpPerson = JsonSerializer.Deserialize<KpPerson>(response, JsonOptions);
            Assert.NotNull(kpPerson);
            Assert.AreEqual("1958-10-16T00:00:00.000Z", kpPerson!.Birthday);
            Assert.AreEqual(3, kpPerson.BirthPlace?.Count);
            Assert.IsNull(kpPerson.Death);
            Assert.AreEqual(0, kpPerson.DeathPlace?.Count);
            Assert.IsNull(kpPerson.Description);
            Assert.AreEqual(4, kpPerson.Facts?.Count);
            Assert.AreEqual("Tim Robbins", kpPerson.EnName);
            Assert.AreEqual(7987, kpPerson.Id);
            Assert.AreEqual(241, kpPerson.Movies?.Count);
            Assert.AreEqual("Тим Роббинс", kpPerson.Name);
            Assert.AreEqual("https://avatars.mds.yandex.net/get-kinopoisk-image/1777765/598f49ce-05ff-4e33-885e-a7f0225f854d/orig", kpPerson.Photo);
        }

        [Test]
        public async Task GetPersonByName()
        {
            var request = $"https://api.kinopoisk.dev/v1/person?name=Тим Роббинс";
            using HttpResponseMessage responseMessage = await HttpClient.GetAsync(new Uri(request)).ConfigureAwait(false);
            _ = responseMessage.EnsureSuccessStatusCode();
            var response = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
            KpSearchResult<KpPerson>? searchResultKpPerson = JsonSerializer.Deserialize<KpSearchResult<KpPerson>>(response, JsonOptions);
            Assert.NotNull(searchResultKpPerson);
            Assert.AreEqual(2, searchResultKpPerson!.Docs.Count);
            KpPerson kpPerson = searchResultKpPerson.Docs[0];
            Assert.AreEqual(7987, kpPerson.Id);
            Assert.AreEqual("Тим Роббинс", kpPerson.Name);
            Assert.AreEqual("https://avatars.mds.yandex.net/get-kinopoisk-image/1777765/598f49ce-05ff-4e33-885e-a7f0225f854d/orig", kpPerson.Photo);
        }

        [Test]
        public async Task GetPersonByMovieId()
        {
            var request = $"https://api.kinopoisk.dev/v1/person?";
            request += "&movies.id=326";
            request += "&selectFields=id movies name";
            request += "&limit=1000";
            using HttpResponseMessage responseMessage = await HttpClient.GetAsync(new Uri(request)).ConfigureAwait(false);
            _ = responseMessage.EnsureSuccessStatusCode();
            var response = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
            KpSearchResult<KpPerson>? searchResultKpPerson = JsonSerializer.Deserialize<KpSearchResult<KpPerson>>(response, JsonOptions);
            Assert.NotNull(searchResultKpPerson);
            Assert.AreEqual(112, searchResultKpPerson!.Docs.Count);
            KpPerson? kpPerson = searchResultKpPerson.Docs.FirstOrDefault(i => i.Id == 7987);
            Assert.NotNull(kpPerson);
            Assert.AreEqual(7987, kpPerson!.Id);
            Assert.AreEqual("Тим Роббинс", kpPerson.Name);
            Assert.LessOrEqual(241, kpPerson.Movies?.Count);
        }

        [Test]
        public async Task GetEpisodesBySeriesId()
        {
            var request = $"https://api.kinopoisk.dev/v1/season?";
            request += "movieId=77044";
            request += "&limit=50";
            using HttpResponseMessage responseMessage = await HttpClient.GetAsync(new Uri(request)).ConfigureAwait(false);
            _ = responseMessage.EnsureSuccessStatusCode();
            var response = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
            KpSearchResult<KpSeason>? searchResultKpSeason = JsonSerializer.Deserialize<KpSearchResult<KpSeason>>(response, JsonOptions);
            Assert.NotNull(searchResultKpSeason);
            Assert.AreEqual(10, searchResultKpSeason!.Docs.Count);
            KpSeason? kpSeason = searchResultKpSeason.Docs.FirstOrDefault(i => i.Number == 1);
            Assert.NotNull(kpSeason);
            Assert.AreEqual(77044, kpSeason!.MovieId);
            Assert.AreEqual(24, kpSeason.Episodes?.Count);
            KpEpisode? kpEpisode = kpSeason.Episodes?.FirstOrDefault(i => i.Number == 1);
            Assert.NotNull(kpEpisode);
            Assert.AreEqual("1994-09-22", kpEpisode!.Date);
            Assert.AreEqual("The One Where Monica Gets a Roommate", kpEpisode.EnName);
            Assert.AreEqual("Эпизод, где Моника берёт новую соседку", kpEpisode.Name);
            Assert.IsNull(kpEpisode.Description);
        }

    }
}
