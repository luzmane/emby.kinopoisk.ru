using System.Collections.Generic;
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
        private static readonly HttpClient httpClient = new();
        private static readonly JsonSerializerOptions jsonOptions = new() { PropertyNameCaseInsensitive = true };

        [OneTimeSetUp]
        public void UnofficialApiTestsSetUp()
        {
            httpClient.DefaultRequestHeaders.Add("X-API-KEY", "0f162131-81c1-4979-b46c-3eea4263fb11");
        }

        [Test]
        public async Task GetFilm()
        {
            string request = $"https://kinopoiskapiunofficial.tech/api/v2.2/films/326";
            using HttpResponseMessage responseMessage = await httpClient.GetAsync(request);
            _ = responseMessage.EnsureSuccessStatusCode();
            string response = await responseMessage.Content.ReadAsStringAsync();
            KpFilm? film = JsonSerializer.Deserialize<KpFilm>(response, jsonOptions);
            Assert.NotNull(film);
            Assert.AreEqual(film!.Countries?.Count, 1);
            Assert.AreEqual(film.CoverUrl, "https://avatars.mds.yandex.net/get-ott/1672343/2a0000016b03d1f5365474a90d26998e2a9f/orig");
            Assert.AreEqual(film.Description, "Бухгалтер Энди Дюфрейн обвинён в убийстве собственной жены и её любовника. Оказавшись в тюрьме под названием Шоушенк, он сталкивается с жестокостью и беззаконием, царящими по обе стороны решётки. Каждый, кто попадает в эти стены, становится их рабом до конца жизни. Но Энди, обладающий живым умом и доброй душой, находит подход как к заключённым, так и к охранникам, добиваясь их особого к себе расположения.");
            Assert.AreEqual(film.FilmLength, 142);
            Assert.AreEqual(film.Genres?.Count, 1);
            Assert.AreEqual(film.ImdbId, "tt0111161");
            Assert.AreEqual(film.KinopoiskId, 326);
            Assert.AreEqual(film.LogoUrl, "https://avatars.mds.yandex.net/get-ott/1648503/2a000001705c8bf514c033f1019473a4caae/orig");
            Assert.AreEqual(film.NameOriginal, "The Shawshank Redemption");
            Assert.AreEqual(film.NameRu, "Побег из Шоушенка");
            Assert.AreEqual(film.PosterUrl, "https://kinopoiskapiunofficial.tech/images/posters/kp/326.jpg");
            Assert.AreEqual(film.PosterUrlPreview, "https://kinopoiskapiunofficial.tech/images/posters/kp_small/326.jpg");
            Assert.AreEqual(film.RatingMpaa, "r");
            Assert.AreEqual(film.Slogan, "Страх - это кандалы. Надежда - это свобода");
            Assert.AreEqual(film.Year, 1994);
        }

        [Test]
        public async Task GetSeasons()
        {
            string request = $"https://kinopoiskapiunofficial.tech/api/v2.2/films/77044/seasons";
            using HttpResponseMessage responseMessage = await httpClient.GetAsync(request);
            _ = responseMessage.EnsureSuccessStatusCode();
            string response = await responseMessage.Content.ReadAsStringAsync();
            KpSearchResult<KpSeason>? seasons = JsonSerializer.Deserialize<KpSearchResult<KpSeason>>(response, jsonOptions);
            Assert.NotNull(seasons);
            Assert.AreEqual(seasons!.Items.Count, 10);
            KpSeason kpSeason = seasons.Items[0];
            Assert.AreEqual(kpSeason.Episodes.Count, 24);
            KpEpisode kpEpisode = kpSeason.Episodes[23];
            Assert.AreEqual(kpEpisode.EpisodeNumber, 24);
            Assert.AreEqual(kpEpisode.SeasonNumber, 1);
            Assert.AreEqual(kpEpisode.NameRu, "Эпизод, где Рейчел понимает");
            Assert.AreEqual(kpEpisode.NameEn, "The One Where Rachel Finds Out");
            Assert.AreEqual(kpEpisode.ReleaseDate, "1995-05-18");
        }

        [Test]
        public async Task GetVideos()
        {
            string request = $"https://kinopoiskapiunofficial.tech/api/v2.2/films/77044/videos";
            using HttpResponseMessage responseMessage = await httpClient.GetAsync(request);
            _ = responseMessage.EnsureSuccessStatusCode();
            string response = await responseMessage.Content.ReadAsStringAsync();
            KpSearchResult<KpVideo>? videos = JsonSerializer.Deserialize<KpSearchResult<KpVideo>>(response, jsonOptions);
            Assert.NotNull(videos);
            Assert.AreEqual(videos!.Items.Count, 8);
            KpVideo kpVideo = videos.Items[0];
            Assert.AreEqual(kpVideo.Url, "http://trailers.s3.mds.yandex.net/video_original/160983-05ae2e39521817fce4e34a89968f3808.mp4");
            Assert.AreEqual(kpVideo.Name, "Тизер (русский язык)");
            Assert.AreEqual(kpVideo.Site, "UNKNOWN");
        }

        [Test]
        public async Task GetStaff()
        {
            string request = $"https://kinopoiskapiunofficial.tech/api/v1/staff/7987";
            using HttpResponseMessage responseMessage = await httpClient.GetAsync(request);
            _ = responseMessage.EnsureSuccessStatusCode();
            string response = await responseMessage.Content.ReadAsStringAsync();
            KpPerson? kpPerson = JsonSerializer.Deserialize<KpPerson>(response, jsonOptions);
            Assert.NotNull(kpPerson);
            Assert.AreEqual(kpPerson!.Birthday, "1958-10-16");
            Assert.AreEqual(kpPerson.BirthPlace, "Уэст-Ковина, штат Калифорния, США");
            Assert.IsNull(kpPerson.Death);
            Assert.IsNull(kpPerson.DeathPlace);
            Assert.AreEqual(kpPerson.Facts.Count, 4);
            Assert.AreEqual(kpPerson.NameEn, "Tim Robbins");
            Assert.AreEqual(kpPerson.NameRu, "Тим Роббинс");
            Assert.AreEqual(kpPerson.PersonId, 7987);
            Assert.AreEqual(kpPerson.PosterUrl, "https://kinopoiskapiunofficial.tech/images/actor_posters/kp/7987.jpg");
        }

        [Test]
        public async Task GetFilmsStaff()
        {
            string request = $"https://kinopoiskapiunofficial.tech/api/v1/staff?filmId=326";
            using HttpResponseMessage responseMessage = await httpClient.GetAsync(request);
            _ = responseMessage.EnsureSuccessStatusCode();
            string response = await responseMessage.Content.ReadAsStringAsync();
            List<KpFilmStaff>? filmStaffList = JsonSerializer.Deserialize<List<KpFilmStaff>>(response, jsonOptions);
            Assert.NotNull(filmStaffList);
            Assert.AreEqual(filmStaffList!.Count, 89);
            KpFilmStaff filmStaff = filmStaffList[1];
            Assert.AreEqual(filmStaff.Description, "Andy Dufresne");
            Assert.AreEqual(filmStaff.NameEn, "Tim Robbins");
            Assert.AreEqual(filmStaff.NameRu, "Тим Роббинс");
            Assert.AreEqual(filmStaff.PosterUrl, "https://kinopoiskapiunofficial.tech/images/actor_posters/kp/7987.jpg");
            Assert.AreEqual(filmStaff.ProfessionKey, "ACTOR");
            Assert.AreEqual(filmStaff.StaffId, 7987);
        }

        [Test]
        public async Task SearchFilmByName()
        {
            string request = $"https://kinopoiskapiunofficial.tech/api/v2.2/films?keyword=100 шагов";
            using HttpResponseMessage responseMessage = await httpClient.GetAsync(request);
            _ = responseMessage.EnsureSuccessStatusCode();
            string response = await responseMessage.Content.ReadAsStringAsync();
            KpSearchResult<KpFilm>? filmSearchResult = JsonSerializer.Deserialize<KpSearchResult<KpFilm>>(response, jsonOptions);
            Assert.NotNull(filmSearchResult);
            Assert.AreEqual(filmSearchResult!.Items.Count, 3);
            KpFilm film = filmSearchResult.Items[2];
            Assert.NotNull(film);
            Assert.AreEqual(film!.KinopoiskId, 933277);
            Assert.AreEqual(film.ImdbId, "tt3904078");
            Assert.AreEqual(film.NameOriginal, "100 Things to Do Before High School");
            Assert.AreEqual(film.NameRu, "100 шагов: Успеть до старших классов");
            Assert.AreEqual(film.Countries?.Count, 1);
            Assert.AreEqual(film.Genres?.Count, 2);
            Assert.IsNull(film.RatingKinopoisk);
            Assert.AreEqual(film.Year, 2014);
            Assert.AreEqual(film.PosterUrl, "https://kinopoiskapiunofficial.tech/images/posters/kp/933277.jpg");
            Assert.AreEqual(film.PosterUrlPreview, "https://kinopoiskapiunofficial.tech/images/posters/kp_small/933277.jpg");
        }

        [Test]
        public async Task SearchStaffByName()
        {
            string request = $"https://kinopoiskapiunofficial.tech/api/v1/persons?name=Тим Роббинс";
            using HttpResponseMessage responseMessage = await httpClient.GetAsync(request);
            _ = responseMessage.EnsureSuccessStatusCode();
            string response = await responseMessage.Content.ReadAsStringAsync();
            KpSearchResult<KpStaff>? staffSearchResult = JsonSerializer.Deserialize<KpSearchResult<KpStaff>>(response, jsonOptions);
            Assert.NotNull(staffSearchResult);
            Assert.AreEqual(staffSearchResult!.Items.Count, 4);
            KpStaff staff = staffSearchResult.Items[0];
            Assert.AreEqual(staff!.KinopoiskId, 7987);
            Assert.AreEqual(staff.NameEn, "Tim Robbins");
            Assert.AreEqual(staff.NameRu, "Тим Роббинс");
            Assert.AreEqual(staff.PosterUrl, "https://kinopoiskapiunofficial.tech/images/actor_posters/kp/7987.jpg");
        }
    }
}
