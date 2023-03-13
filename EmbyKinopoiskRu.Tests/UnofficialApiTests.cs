using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

using EmbyKinopoiskRu.Api.KinopoiskApiUnofficial.Model;
using EmbyKinopoiskRu.Api.KinopoiskApiUnofficial.Model.Film;
using EmbyKinopoiskRu.Api.KinopoiskApiUnofficial.Model.Person;
using EmbyKinopoiskRu.Api.KinopoiskApiUnofficial.Model.Season;

using NUnit.Framework;

namespace EmbyKinopoiskRu.Tests
{
    [TestFixture]
    public class UnofficialApiTests
    {
        private static readonly HttpClient HttpClient = new();
        private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

        [OneTimeSetUp]
        public void UnofficialApiTestsSetUp()
        {
            HttpClient.DefaultRequestHeaders.Add("X-API-KEY", "0f162131-81c1-4979-b46c-3eea4263fb11");
        }

        [Test]
        public async Task GetFilm()
        {
            var request = $"https://kinopoiskapiunofficial.tech/api/v2.2/films/326";
            using HttpResponseMessage responseMessage = await HttpClient.GetAsync(new Uri(request)).ConfigureAwait(false);
            _ = responseMessage.EnsureSuccessStatusCode();
            var response = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
            KpFilm? film = JsonSerializer.Deserialize<KpFilm>(response, JsonOptions);
            Assert.NotNull(film);
            Assert.AreEqual(1, film!.Countries?.Count);
            Assert.AreEqual("https://avatars.mds.yandex.net/get-ott/1652588/2a00000186aca5e13ea6cec11d584ac5455b/orig", film.CoverUrl);
            Assert.AreEqual("Бухгалтер Энди Дюфрейн обвинён в убийстве собственной жены и её любовника. Оказавшись в тюрьме под названием Шоушенк, он сталкивается с жестокостью и беззаконием, царящими по обе стороны решётки. Каждый, кто попадает в эти стены, становится их рабом до конца жизни. Но Энди, обладающий живым умом и доброй душой, находит подход как к заключённым, так и к охранникам, добиваясь их особого к себе расположения.", film.Description);
            Assert.AreEqual(142, film.FilmLength);
            Assert.AreEqual(1, film.Genres?.Count);
            Assert.AreEqual("tt0111161", film.ImdbId);
            Assert.AreEqual(326, film.KinopoiskId);
            Assert.AreEqual("https://avatars.mds.yandex.net/get-ott/1648503/2a000001705c8bf514c033f1019473a4caae/orig", film.LogoUrl);
            Assert.AreEqual("The Shawshank Redemption", film.NameOriginal);
            Assert.AreEqual("Побег из Шоушенка", film.NameRu);
            Assert.AreEqual("https://kinopoiskapiunofficial.tech/images/posters/kp/326.jpg", film.PosterUrl);
            Assert.AreEqual("https://kinopoiskapiunofficial.tech/images/posters/kp_small/326.jpg", film.PosterUrlPreview);
            Assert.AreEqual("r", film.RatingMpaa);
            Assert.AreEqual("Страх - это кандалы. Надежда - это свобода", film.Slogan);
            Assert.AreEqual(1994, film.Year);
            Assert.Less(0, film.RatingFilmCritics);
            Assert.Less(0, film.RatingKinopoisk);
        }

        [Test]
        public async Task GetSeasons()
        {
            var request = $"https://kinopoiskapiunofficial.tech/api/v2.2/films/77044/seasons";
            using HttpResponseMessage responseMessage = await HttpClient.GetAsync(new Uri(request)).ConfigureAwait(false);
            _ = responseMessage.EnsureSuccessStatusCode();
            var response = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
            KpSearchResult<KpSeason>? seasons = JsonSerializer.Deserialize<KpSearchResult<KpSeason>>(response, JsonOptions);
            Assert.NotNull(seasons);
            Assert.AreEqual(10, seasons!.Items.Count);
            KpSeason kpSeason = seasons.Items[0];
            Assert.AreEqual(24, kpSeason.Episodes.Count);
            KpEpisode kpEpisode = kpSeason.Episodes[23];
            Assert.AreEqual(24, kpEpisode.EpisodeNumber);
            Assert.AreEqual(1, kpEpisode.SeasonNumber);
            Assert.AreEqual("Эпизод, где Рейчел понимает", kpEpisode.NameRu);
            Assert.AreEqual("The One Where Rachel Finds Out", kpEpisode.NameEn);
            Assert.AreEqual("1995-05-18", kpEpisode.ReleaseDate);
        }

        [Test]
        public async Task GetVideos()
        {
            var request = $"https://kinopoiskapiunofficial.tech/api/v2.2/films/77044/videos";
            using HttpResponseMessage responseMessage = await HttpClient.GetAsync(new Uri(request)).ConfigureAwait(false);
            _ = responseMessage.EnsureSuccessStatusCode();
            var response = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
            KpSearchResult<KpVideo>? videos = JsonSerializer.Deserialize<KpSearchResult<KpVideo>>(response, JsonOptions);
            Assert.NotNull(videos);
            Assert.AreEqual(8, videos!.Items.Count);
            KpVideo kpVideo = videos.Items[0];
            Assert.AreEqual("http://trailers.s3.mds.yandex.net/video_original/160983-05ae2e39521817fce4e34a89968f3808.mp4", kpVideo.Url);
            Assert.AreEqual("Тизер (русский язык)", kpVideo.Name);
            Assert.AreEqual("UNKNOWN", kpVideo.Site);
        }

        [Test]
        public async Task GetStaff()
        {
            var request = $"https://kinopoiskapiunofficial.tech/api/v1/staff/7987";
            using HttpResponseMessage responseMessage = await HttpClient.GetAsync(new Uri(request)).ConfigureAwait(false);
            _ = responseMessage.EnsureSuccessStatusCode();
            var response = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
            KpPerson? kpPerson = JsonSerializer.Deserialize<KpPerson>(response, JsonOptions);
            Assert.NotNull(kpPerson);
            Assert.AreEqual("1958-10-16", kpPerson!.Birthday);
            Assert.AreEqual("Уэст-Ковина, штат Калифорния, США", kpPerson.BirthPlace);
            Assert.IsNull(kpPerson.Death);
            Assert.IsNull(kpPerson.DeathPlace);
            Assert.AreEqual(4, kpPerson.Facts?.Count);
            Assert.AreEqual("Tim Robbins", kpPerson.NameEn);
            Assert.AreEqual("Тим Роббинс", kpPerson.NameRu);
            Assert.AreEqual(7987, kpPerson.PersonId);
            Assert.AreEqual("https://kinopoiskapiunofficial.tech/images/actor_posters/kp/7987.jpg", kpPerson.PosterUrl);
        }

        [Test]
        public async Task GetFilmsStaff()
        {
            var request = $"https://kinopoiskapiunofficial.tech/api/v1/staff?filmId=326";
            using HttpResponseMessage responseMessage = await HttpClient.GetAsync(new Uri(request)).ConfigureAwait(false);
            _ = responseMessage.EnsureSuccessStatusCode();
            var response = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
            List<KpFilmStaff>? filmStaffList = JsonSerializer.Deserialize<List<KpFilmStaff>>(response, JsonOptions);
            Assert.NotNull(filmStaffList);
            Assert.AreEqual(89, filmStaffList!.Count);
            KpFilmStaff filmStaff = filmStaffList[1];
            Assert.AreEqual("Andy Dufresne", filmStaff.Description);
            Assert.AreEqual("Tim Robbins", filmStaff.NameEn);
            Assert.AreEqual("Тим Роббинс", filmStaff.NameRu);
            Assert.AreEqual("https://kinopoiskapiunofficial.tech/images/actor_posters/kp/7987.jpg", filmStaff.PosterUrl);
            Assert.AreEqual("ACTOR", filmStaff.ProfessionKey);
            Assert.AreEqual(7987, filmStaff.StaffId);
        }

        [Test]
        public async Task SearchFilmByName()
        {
            var request = $"https://kinopoiskapiunofficial.tech/api/v2.2/films?keyword=100 шагов";
            using HttpResponseMessage responseMessage = await HttpClient.GetAsync(new Uri(request)).ConfigureAwait(false);
            _ = responseMessage.EnsureSuccessStatusCode();
            var response = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
            KpSearchResult<KpFilm>? filmSearchResult = JsonSerializer.Deserialize<KpSearchResult<KpFilm>>(response, JsonOptions);
            Assert.NotNull(filmSearchResult);
            Assert.AreEqual(3, filmSearchResult!.Items.Count);
            KpFilm film = filmSearchResult.Items.First(f => f.KinopoiskId == 933277);
            Assert.NotNull(film);
            Assert.AreEqual(933277, film!.KinopoiskId);
            Assert.AreEqual("tt3904078", film.ImdbId);
            Assert.AreEqual("100 Things to Do Before High School", film.NameOriginal);
            Assert.AreEqual("100 шагов: Успеть до старших классов", film.NameRu);
            Assert.AreEqual(1, film.Countries?.Count);
            Assert.AreEqual(2, film.Genres?.Count);
            Assert.Less(0, film.RatingKinopoisk);
            Assert.AreEqual(2014, film.Year);
            Assert.AreEqual("https://kinopoiskapiunofficial.tech/images/posters/kp/933277.jpg", film.PosterUrl);
            Assert.AreEqual("https://kinopoiskapiunofficial.tech/images/posters/kp_small/933277.jpg", film.PosterUrlPreview);
        }

        [Test]
        public async Task SearchStaffByName()
        {
            var request = $"https://kinopoiskapiunofficial.tech/api/v1/persons?name=Тим Роббинс";
            using HttpResponseMessage responseMessage = await HttpClient.GetAsync(new Uri(request)).ConfigureAwait(false);
            _ = responseMessage.EnsureSuccessStatusCode();
            var response = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
            KpSearchResult<KpStaff>? staffSearchResult = JsonSerializer.Deserialize<KpSearchResult<KpStaff>>(response, JsonOptions);
            Assert.NotNull(staffSearchResult);
            Assert.AreEqual(4, staffSearchResult!.Items.Count);
            KpStaff staff = staffSearchResult.Items.First(f => f.KinopoiskId == 7987);
            Assert.NotNull(staff);
            Assert.AreEqual(7987, staff!.KinopoiskId);
            Assert.AreEqual("Tim Robbins", staff.NameEn);
            Assert.AreEqual("Тим Роббинс", staff.NameRu);
            Assert.AreEqual("https://kinopoiskapiunofficial.tech/images/actor_posters/kp/7987.jpg", staff.PosterUrl);
        }
    }
}
