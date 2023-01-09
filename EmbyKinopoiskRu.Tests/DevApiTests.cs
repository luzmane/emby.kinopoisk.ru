using System.IO;
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
    [TestFixture]
    public class DevApiTests
    {
        private static readonly string _token = File.ReadAllLines("kinopoisk.dev.token").FirstOrDefault() ?? string.Empty;
        private static readonly HttpClient httpClient = new();
        private static readonly JsonSerializerOptions jsonOptions = new() { PropertyNameCaseInsensitive = true };


        [Test]
        public async Task GetMovieById()
        {
            string request = $"https://api.kinopoisk.dev/movie?token={_token}&field=id&search=326";
            using HttpResponseMessage responseMessage = await httpClient.GetAsync(request);
            _ = responseMessage.EnsureSuccessStatusCode();
            string response = await responseMessage.Content.ReadAsStringAsync();
            KpMovie? kpMovie = JsonSerializer.Deserialize<KpMovie>(response, jsonOptions);
            Assert.NotNull(kpMovie);
            Assert.AreEqual("The Shawshank Redemption", kpMovie!.AlternativeName);
            Assert.AreEqual("https://avatars.mds.yandex.net/get-ott/1672343/2a0000016b03d1f5365474a90d26998e2a9f/orig", kpMovie.Backdrop?.Url);
            Assert.AreEqual("https://avatars.mds.yandex.net/get-ott/1672343/2a0000016b03d1f5365474a90d26998e2a9f/orig", kpMovie.Backdrop?.PreviewUrl);
            Assert.AreEqual(1, kpMovie.Countries.Count);
            Assert.AreEqual("Бухгалтер Энди Дюфрейн обвинён в убийстве собственной жены и её любовника. Оказавшись в тюрьме под названием Шоушенк, он сталкивается с жестокостью и беззаконием, царящими по обе стороны решётки. Каждый, кто попадает в эти стены, становится их рабом до конца жизни. Но Энди, обладающий живым умом и доброй душой, находит подход как к заключённым, так и к охранникам, добиваясь их особого к себе расположения.", kpMovie.Description);
            Assert.IsNull(kpMovie.EnName);
            Assert.AreEqual("tt0111161", kpMovie.ExternalId?.Imdb);
            Assert.AreEqual(1, kpMovie.Genres.Count);
            Assert.AreEqual(326, kpMovie.Id);
            Assert.AreEqual("https://avatars.mds.yandex.net/get-ott/1648503/2a000001705c8bf514c033f1019473a4caae/orig", kpMovie.Logo?.Url);
            Assert.AreEqual(142, kpMovie.MovieLength);
            Assert.AreEqual("Побег из Шоушенка", kpMovie.Name);
            Assert.AreEqual(112, kpMovie.Persons.Count);
            Assert.AreEqual("https://st.kp.yandex.net/images/film_big/326.jpg", kpMovie.Poster?.Url);
            Assert.AreEqual("https://st.kp.yandex.net/images/film_iphone/iphone360_326.jpg", kpMovie.Poster?.PreviewUrl);
            Assert.AreEqual("1994-09-10T00:00:00.000Z", kpMovie.Premiere?.World);
            Assert.AreEqual(1, kpMovie.ProductionCompanies.Count);
            Assert.IsNotNull(kpMovie.Rating?.Kp);
            Assert.IsNotNull(kpMovie.Rating?.FilmCritics);
            Assert.AreEqual("r", kpMovie.RatingMpaa);
            Assert.AreEqual("Страх - это кандалы. Надежда - это свобода", kpMovie.Slogan);
            Assert.AreEqual("Выпущен", kpMovie.Status);
            Assert.AreEqual("movie", kpMovie.Type);
            Assert.IsNotNull(kpMovie.Videos);
            Assert.AreEqual(0, kpMovie.Videos!.Teasers.Count);
            Assert.AreEqual(4, kpMovie.Videos!.Trailers.Count);
            Assert.AreEqual(1994, kpMovie.Year);
        }

        [Test]
        public async Task GetMoviesByMetadataStrictNameYear()
        {
            string request = $"https://api.kinopoisk.dev/movie?token={_token}";
            request += "&limit=50";
            request += "&selectFields=externalId logo poster rating movieLength id type name description year alternativeName enName backdrop countries genres persons premiere productionCompanies ratingMpaa slogan";
            request += "&field=name&search=Побег из Шоушенка";
            request += "&field=year&search=1994";
            using HttpResponseMessage responseMessage = await httpClient.GetAsync(request);
            _ = responseMessage.EnsureSuccessStatusCode();
            string response = await responseMessage.Content.ReadAsStringAsync();
            KpSearchResult<KpMovie>? searchResultMovie = JsonSerializer.Deserialize<KpSearchResult<KpMovie>>(response, jsonOptions);
            Assert.NotNull(searchResultMovie);
            Assert.AreEqual(1, searchResultMovie!.Docs.Count);
            KpMovie kpMovie = searchResultMovie!.Docs[0];
            Assert.AreEqual("The Shawshank Redemption", kpMovie.AlternativeName);
            Assert.AreEqual("https://avatars.mds.yandex.net/get-ott/1672343/2a0000016b03d1f5365474a90d26998e2a9f/orig", kpMovie.Backdrop?.Url);
            Assert.AreEqual("https://avatars.mds.yandex.net/get-ott/1672343/2a0000016b03d1f5365474a90d26998e2a9f/orig", kpMovie.Backdrop?.PreviewUrl);
            Assert.AreEqual(1, kpMovie.Countries.Count);
            Assert.AreEqual("Бухгалтер Энди Дюфрейн обвинён в убийстве собственной жены и её любовника. Оказавшись в тюрьме под названием Шоушенк, он сталкивается с жестокостью и беззаконием, царящими по обе стороны решётки. Каждый, кто попадает в эти стены, становится их рабом до конца жизни. Но Энди, обладающий живым умом и доброй душой, находит подход как к заключённым, так и к охранникам, добиваясь их особого к себе расположения.", kpMovie.Description);
            Assert.IsNull(kpMovie.EnName);
            Assert.AreEqual("tt0111161", kpMovie.ExternalId?.Imdb);
            Assert.AreEqual(1, kpMovie.Genres.Count);
            Assert.AreEqual(326, kpMovie.Id);
            Assert.AreEqual("https://avatars.mds.yandex.net/get-ott/1648503/2a000001705c8bf514c033f1019473a4caae/orig", kpMovie.Logo?.Url);
            Assert.AreEqual(142, kpMovie.MovieLength);
            Assert.AreEqual("Побег из Шоушенка", kpMovie.Name);
            Assert.AreEqual(112, kpMovie.Persons.Count);
            Assert.AreEqual("https://st.kp.yandex.net/images/film_big/326.jpg", kpMovie.Poster?.Url);
            Assert.AreEqual("https://st.kp.yandex.net/images/film_iphone/iphone360_326.jpg", kpMovie.Poster?.PreviewUrl);
            Assert.AreEqual("1994-09-10T00:00:00.000Z", kpMovie.Premiere?.World);
            Assert.AreEqual(1, kpMovie.ProductionCompanies.Count);
            Assert.IsNotNull(kpMovie.Rating?.Kp);
            Assert.IsNotNull(kpMovie.Rating?.FilmCritics);
            Assert.AreEqual("r", kpMovie.RatingMpaa);
            Assert.AreEqual("Страх - это кандалы. Надежда - это свобода", kpMovie.Slogan);
            Assert.AreEqual("movie", kpMovie.Type);
            Assert.AreEqual(1994, kpMovie.Year);
        }

        [Test]
        public async Task GetMoviesByMetadataStrictName()
        {
            string request = $"https://api.kinopoisk.dev/movie?token={_token}";
            request += "&limit=50";
            request += "&selectFields=externalId logo poster rating movieLength id type name description year alternativeName enName backdrop countries genres persons premiere productionCompanies ratingMpaa slogan";
            request += "&field=name&search=Побег из Шоушенка";
            using HttpResponseMessage responseMessage = await httpClient.GetAsync(request);
            _ = responseMessage.EnsureSuccessStatusCode();
            string response = await responseMessage.Content.ReadAsStringAsync();
            KpSearchResult<KpMovie>? searchResultMovie = JsonSerializer.Deserialize<KpSearchResult<KpMovie>>(response, jsonOptions);
            Assert.NotNull(searchResultMovie);
            Assert.AreEqual(1, searchResultMovie!.Docs.Count);
            KpMovie kpMovie = searchResultMovie!.Docs[0];
            Assert.AreEqual("The Shawshank Redemption", kpMovie.AlternativeName);
            Assert.AreEqual("https://avatars.mds.yandex.net/get-ott/1672343/2a0000016b03d1f5365474a90d26998e2a9f/orig", kpMovie.Backdrop?.Url);
            Assert.AreEqual("https://avatars.mds.yandex.net/get-ott/1672343/2a0000016b03d1f5365474a90d26998e2a9f/orig", kpMovie.Backdrop?.PreviewUrl);
            Assert.AreEqual(1, kpMovie.Countries.Count);
            Assert.AreEqual("Бухгалтер Энди Дюфрейн обвинён в убийстве собственной жены и её любовника. Оказавшись в тюрьме под названием Шоушенк, он сталкивается с жестокостью и беззаконием, царящими по обе стороны решётки. Каждый, кто попадает в эти стены, становится их рабом до конца жизни. Но Энди, обладающий живым умом и доброй душой, находит подход как к заключённым, так и к охранникам, добиваясь их особого к себе расположения.", kpMovie.Description);
            Assert.IsNull(kpMovie.EnName);
            Assert.AreEqual("tt0111161", kpMovie.ExternalId?.Imdb);
            Assert.AreEqual(1, kpMovie.Genres.Count);
            Assert.AreEqual(326, kpMovie.Id);
            Assert.AreEqual("https://avatars.mds.yandex.net/get-ott/1648503/2a000001705c8bf514c033f1019473a4caae/orig", kpMovie.Logo?.Url);
            Assert.AreEqual(142, kpMovie.MovieLength);
            Assert.AreEqual("Побег из Шоушенка", kpMovie.Name);
            Assert.AreEqual(112, kpMovie.Persons.Count);
            Assert.AreEqual("https://st.kp.yandex.net/images/film_big/326.jpg", kpMovie.Poster?.Url);
            Assert.AreEqual("https://st.kp.yandex.net/images/film_iphone/iphone360_326.jpg", kpMovie.Poster?.PreviewUrl);
            Assert.AreEqual("1994-09-10T00:00:00.000Z", kpMovie.Premiere?.World);
            Assert.AreEqual(1, kpMovie.ProductionCompanies.Count);
            Assert.IsNotNull(kpMovie.Rating?.Kp);
            Assert.IsNotNull(kpMovie.Rating?.FilmCritics);
            Assert.AreEqual("r", kpMovie.RatingMpaa);
            Assert.AreEqual("Страх - это кандалы. Надежда - это свобода", kpMovie.Slogan);
            Assert.AreEqual("movie", kpMovie.Type);
            Assert.AreEqual(1994, kpMovie.Year);
        }

        [Test]
        public async Task GetMoviesByMetadataNotStrictNameYear()
        {
            string request = $"https://api.kinopoisk.dev/movie?token={_token}";
            request += "&limit=50";
            request += "&selectFields=externalId logo poster rating movieLength id type name description year alternativeName enName backdrop countries genres persons premiere productionCompanies ratingMpaa slogan";
            request += "&field=name&search=Побег из Шоушенка";
            request += "&field=year&search=1994";
            using HttpResponseMessage responseMessage = await httpClient.GetAsync(request);
            _ = responseMessage.EnsureSuccessStatusCode();
            string response = await responseMessage.Content.ReadAsStringAsync();
            KpSearchResult<KpMovie>? searchResultMovie = JsonSerializer.Deserialize<KpSearchResult<KpMovie>>(response, jsonOptions);
            Assert.NotNull(searchResultMovie);
            Assert.AreEqual(1, searchResultMovie!.Docs.Count);
            KpMovie kpMovie = searchResultMovie!.Docs[0];
            Assert.AreEqual("The Shawshank Redemption", kpMovie.AlternativeName);
            Assert.AreEqual("https://avatars.mds.yandex.net/get-ott/1672343/2a0000016b03d1f5365474a90d26998e2a9f/orig", kpMovie.Backdrop?.Url);
            Assert.AreEqual("https://avatars.mds.yandex.net/get-ott/1672343/2a0000016b03d1f5365474a90d26998e2a9f/orig", kpMovie.Backdrop?.PreviewUrl);
            Assert.AreEqual(1, kpMovie.Countries.Count);
            Assert.AreEqual("Бухгалтер Энди Дюфрейн обвинён в убийстве собственной жены и её любовника. Оказавшись в тюрьме под названием Шоушенк, он сталкивается с жестокостью и беззаконием, царящими по обе стороны решётки. Каждый, кто попадает в эти стены, становится их рабом до конца жизни. Но Энди, обладающий живым умом и доброй душой, находит подход как к заключённым, так и к охранникам, добиваясь их особого к себе расположения.", kpMovie.Description);
            Assert.IsNull(kpMovie.EnName);
            Assert.AreEqual("tt0111161", kpMovie.ExternalId?.Imdb);
            Assert.AreEqual(1, kpMovie.Genres.Count);
            Assert.AreEqual(326, kpMovie.Id);
            Assert.AreEqual("https://avatars.mds.yandex.net/get-ott/1648503/2a000001705c8bf514c033f1019473a4caae/orig", kpMovie.Logo?.Url);
            Assert.AreEqual(142, kpMovie.MovieLength);
            Assert.AreEqual("Побег из Шоушенка", kpMovie.Name);
            Assert.AreEqual(112, kpMovie.Persons.Count);
            Assert.AreEqual("https://st.kp.yandex.net/images/film_big/326.jpg", kpMovie.Poster?.Url);
            Assert.AreEqual("https://st.kp.yandex.net/images/film_iphone/iphone360_326.jpg", kpMovie.Poster?.PreviewUrl);
            Assert.AreEqual("1994-09-10T00:00:00.000Z", kpMovie.Premiere?.World);
            Assert.AreEqual(1, kpMovie.ProductionCompanies.Count);
            Assert.IsNotNull(kpMovie.Rating?.Kp);
            Assert.IsNotNull(kpMovie.Rating?.FilmCritics);
            Assert.AreEqual("r", kpMovie.RatingMpaa);
            Assert.AreEqual("Страх - это кандалы. Надежда - это свобода", kpMovie.Slogan);
            Assert.AreEqual("movie", kpMovie.Type);
            Assert.AreEqual(1994, kpMovie.Year);
        }

        [Test]
        public async Task GetMoviesByMetadataNotStrictName()
        {
            string request = $"https://api.kinopoisk.dev/movie?token={_token}";
            request += "&limit=50";
            request += "&selectFields=externalId logo poster rating movieLength id type name description year alternativeName enName backdrop countries genres persons premiere productionCompanies ratingMpaa slogan";
            request += "&field=name&search=Побег из Шоушенка";
            using HttpResponseMessage responseMessage = await httpClient.GetAsync(request);
            _ = responseMessage.EnsureSuccessStatusCode();
            string response = await responseMessage.Content.ReadAsStringAsync();
            KpSearchResult<KpMovie>? searchResultMovie = JsonSerializer.Deserialize<KpSearchResult<KpMovie>>(response, jsonOptions);
            Assert.NotNull(searchResultMovie);
            Assert.AreEqual(1, searchResultMovie!.Docs.Count);
            KpMovie kpMovie = searchResultMovie!.Docs[0];
            Assert.AreEqual("The Shawshank Redemption", kpMovie.AlternativeName);
            Assert.AreEqual("https://avatars.mds.yandex.net/get-ott/1672343/2a0000016b03d1f5365474a90d26998e2a9f/orig", kpMovie.Backdrop?.Url);
            Assert.AreEqual("https://avatars.mds.yandex.net/get-ott/1672343/2a0000016b03d1f5365474a90d26998e2a9f/orig", kpMovie.Backdrop?.PreviewUrl);
            Assert.AreEqual(1, kpMovie.Countries.Count);
            Assert.AreEqual("Бухгалтер Энди Дюфрейн обвинён в убийстве собственной жены и её любовника. Оказавшись в тюрьме под названием Шоушенк, он сталкивается с жестокостью и беззаконием, царящими по обе стороны решётки. Каждый, кто попадает в эти стены, становится их рабом до конца жизни. Но Энди, обладающий живым умом и доброй душой, находит подход как к заключённым, так и к охранникам, добиваясь их особого к себе расположения.", kpMovie.Description);
            Assert.IsNull(kpMovie.EnName);
            Assert.AreEqual("tt0111161", kpMovie.ExternalId?.Imdb);
            Assert.AreEqual(1, kpMovie.Genres.Count);
            Assert.AreEqual(326, kpMovie.Id);
            Assert.AreEqual("https://avatars.mds.yandex.net/get-ott/1648503/2a000001705c8bf514c033f1019473a4caae/orig", kpMovie.Logo?.Url);
            Assert.AreEqual(142, kpMovie.MovieLength);
            Assert.AreEqual("Побег из Шоушенка", kpMovie.Name);
            Assert.AreEqual(112, kpMovie.Persons.Count);
            Assert.AreEqual("https://st.kp.yandex.net/images/film_big/326.jpg", kpMovie.Poster?.Url);
            Assert.AreEqual("https://st.kp.yandex.net/images/film_iphone/iphone360_326.jpg", kpMovie.Poster?.PreviewUrl);
            Assert.AreEqual("1994-09-10T00:00:00.000Z", kpMovie.Premiere?.World);
            Assert.AreEqual(1, kpMovie.ProductionCompanies.Count);
            Assert.IsNotNull(kpMovie.Rating?.Kp);
            Assert.IsNotNull(kpMovie.Rating?.FilmCritics);
            Assert.AreEqual("r", kpMovie.RatingMpaa);
            Assert.AreEqual("Страх - это кандалы. Надежда - это свобода", kpMovie.Slogan);
            Assert.AreEqual("movie", kpMovie.Type);
            Assert.AreEqual(1994, kpMovie.Year);
        }

        [Test]
        public async Task GetMoviesByMovieDetailsAlternativeNameYear()
        {
            string request = $"https://api.kinopoisk.dev/movie?token={_token}";
            request += "&limit=50";
            request += "&field=alternativeName&search=The Shawshank Redemption";
            request += "&field=year&search=1994";
            request += "&selectFields=externalId logo poster rating movieLength id type name description year alternativeName enName backdrop countries genres persons premiere productionCompanies ratingMpaa slogan";
            using HttpResponseMessage responseMessage = await httpClient.GetAsync(request);
            _ = responseMessage.EnsureSuccessStatusCode();
            string response = await responseMessage.Content.ReadAsStringAsync();
            KpSearchResult<KpMovie>? searchResultMovie = JsonSerializer.Deserialize<KpSearchResult<KpMovie>>(response, jsonOptions);
            Assert.NotNull(searchResultMovie);
            Assert.AreEqual(1, searchResultMovie!.Docs.Count);
            KpMovie kpMovie = searchResultMovie!.Docs[0];
            Assert.AreEqual("The Shawshank Redemption", kpMovie.AlternativeName);
            Assert.AreEqual("https://avatars.mds.yandex.net/get-ott/1672343/2a0000016b03d1f5365474a90d26998e2a9f/orig", kpMovie.Backdrop?.Url);
            Assert.AreEqual("https://avatars.mds.yandex.net/get-ott/1672343/2a0000016b03d1f5365474a90d26998e2a9f/orig", kpMovie.Backdrop?.PreviewUrl);
            Assert.AreEqual(1, kpMovie.Countries.Count);
            Assert.AreEqual("Бухгалтер Энди Дюфрейн обвинён в убийстве собственной жены и её любовника. Оказавшись в тюрьме под названием Шоушенк, он сталкивается с жестокостью и беззаконием, царящими по обе стороны решётки. Каждый, кто попадает в эти стены, становится их рабом до конца жизни. Но Энди, обладающий живым умом и доброй душой, находит подход как к заключённым, так и к охранникам, добиваясь их особого к себе расположения.", kpMovie.Description);
            Assert.IsNull(kpMovie.EnName);
            Assert.AreEqual("tt0111161", kpMovie.ExternalId?.Imdb);
            Assert.AreEqual(1, kpMovie.Genres.Count);
            Assert.AreEqual(326, kpMovie.Id);
            Assert.AreEqual("https://avatars.mds.yandex.net/get-ott/1648503/2a000001705c8bf514c033f1019473a4caae/orig", kpMovie.Logo?.Url);
            Assert.AreEqual(142, kpMovie.MovieLength);
            Assert.AreEqual("Побег из Шоушенка", kpMovie.Name);
            Assert.AreEqual(112, kpMovie.Persons.Count);
            Assert.AreEqual("https://st.kp.yandex.net/images/film_big/326.jpg", kpMovie.Poster?.Url);
            Assert.AreEqual("https://st.kp.yandex.net/images/film_iphone/iphone360_326.jpg", kpMovie.Poster?.PreviewUrl);
            Assert.AreEqual("1994-09-10T00:00:00.000Z", kpMovie.Premiere?.World);
            Assert.AreEqual(1, kpMovie.ProductionCompanies.Count);
            Assert.IsNotNull(kpMovie.Rating?.Kp);
            Assert.IsNotNull(kpMovie.Rating?.FilmCritics);
            Assert.AreEqual("r", kpMovie.RatingMpaa);
            Assert.AreEqual("Страх - это кандалы. Надежда - это свобода", kpMovie.Slogan);
            Assert.AreEqual("movie", kpMovie.Type);
            Assert.AreEqual(1994, kpMovie.Year);
        }

        [Test]
        public async Task GetMoviesByMovieDetailsAlternativeName()
        {
            string request = $"https://api.kinopoisk.dev/movie?token={_token}";
            request += "&limit=50";
            request += "&field=alternativeName&search=The Shawshank Redemption";
            request += "&selectFields=externalId logo poster rating movieLength id type name description year alternativeName enName backdrop countries genres persons premiere productionCompanies ratingMpaa slogan";
            using HttpResponseMessage responseMessage = await httpClient.GetAsync(request);
            _ = responseMessage.EnsureSuccessStatusCode();
            string response = await responseMessage.Content.ReadAsStringAsync();
            KpSearchResult<KpMovie>? searchResultMovie = JsonSerializer.Deserialize<KpSearchResult<KpMovie>>(response, jsonOptions);
            Assert.NotNull(searchResultMovie);
            Assert.AreEqual(2, searchResultMovie!.Docs.Count);
            KpMovie kpMovie = searchResultMovie!.Docs[0];
            Assert.AreEqual("The Shawshank Redemption", kpMovie.AlternativeName);
            Assert.AreEqual("https://avatars.mds.yandex.net/get-ott/1672343/2a0000016b03d1f5365474a90d26998e2a9f/orig", kpMovie.Backdrop?.Url);
            Assert.AreEqual("https://avatars.mds.yandex.net/get-ott/1672343/2a0000016b03d1f5365474a90d26998e2a9f/orig", kpMovie.Backdrop?.PreviewUrl);
            Assert.AreEqual(1, kpMovie.Countries.Count);
            Assert.AreEqual("Бухгалтер Энди Дюфрейн обвинён в убийстве собственной жены и её любовника. Оказавшись в тюрьме под названием Шоушенк, он сталкивается с жестокостью и беззаконием, царящими по обе стороны решётки. Каждый, кто попадает в эти стены, становится их рабом до конца жизни. Но Энди, обладающий живым умом и доброй душой, находит подход как к заключённым, так и к охранникам, добиваясь их особого к себе расположения.", kpMovie.Description);
            Assert.IsNull(kpMovie.EnName);
            Assert.AreEqual("tt0111161", kpMovie.ExternalId?.Imdb);
            Assert.AreEqual(1, kpMovie.Genres.Count);
            Assert.AreEqual(326, kpMovie.Id);
            Assert.AreEqual("https://avatars.mds.yandex.net/get-ott/1648503/2a000001705c8bf514c033f1019473a4caae/orig", kpMovie.Logo?.Url);
            Assert.AreEqual(142, kpMovie.MovieLength);
            Assert.AreEqual("Побег из Шоушенка", kpMovie.Name);
            Assert.AreEqual(112, kpMovie.Persons.Count);
            Assert.AreEqual("https://st.kp.yandex.net/images/film_big/326.jpg", kpMovie.Poster?.Url);
            Assert.AreEqual("https://st.kp.yandex.net/images/film_iphone/iphone360_326.jpg", kpMovie.Poster?.PreviewUrl);
            Assert.AreEqual("1994-09-10T00:00:00.000Z", kpMovie.Premiere?.World);
            Assert.AreEqual(1, kpMovie.ProductionCompanies.Count);
            Assert.IsNotNull(kpMovie.Rating?.Kp);
            Assert.IsNotNull(kpMovie.Rating?.FilmCritics);
            Assert.AreEqual("r", kpMovie.RatingMpaa);
            Assert.AreEqual("Страх - это кандалы. Надежда - это свобода", kpMovie.Slogan);
            Assert.AreEqual("movie", kpMovie.Type);
            Assert.AreEqual(1994, kpMovie.Year);
        }

        [Test]
        public async Task GetPersonById()
        {
            string request = $"https://api.kinopoisk.dev/person?token={_token}&field=id&search=7987";
            using HttpResponseMessage responseMessage = await httpClient.GetAsync(request);
            _ = responseMessage.EnsureSuccessStatusCode();
            string response = await responseMessage.Content.ReadAsStringAsync();
            KpPerson? kpPerson = JsonSerializer.Deserialize<KpPerson>(response, jsonOptions);
            Assert.NotNull(kpPerson);
            Assert.AreEqual(3, kpPerson!.BirthPlace.Count);
            Assert.AreEqual("1958-10-16T00:00:00.000Z", kpPerson.Birthday);
            Assert.AreEqual(4, kpPerson.Facts.Count);
            Assert.AreEqual(7987, kpPerson.Id);
            Assert.AreEqual(241, kpPerson.Movies.Count);
            Assert.AreEqual("Тим Роббинс", kpPerson.Name);
            Assert.AreEqual("https://avatars.mds.yandex.net/get-kinopoisk-image/1777765/598f49ce-05ff-4e33-885e-a7f0225f854d/orig", kpPerson.Photo);
        }

        [Test]
        public async Task GetPersonByName()
        {
            string request = $"https://api.kinopoisk.dev/person?token={_token}&field=name&search=Тим Роббинс";
            using HttpResponseMessage responseMessage = await httpClient.GetAsync(request);
            _ = responseMessage.EnsureSuccessStatusCode();
            string response = await responseMessage.Content.ReadAsStringAsync();
            KpSearchResult<KpPerson>? searchResultKpPerson = JsonSerializer.Deserialize<KpSearchResult<KpPerson>>(response, jsonOptions);
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
            string request = $"https://api.kinopoisk.dev/person?token={_token}";
            request += "&field=movies.id&search=326";
            request += "&selectFields=id movies";
            request += "&limit=1000";
            using HttpResponseMessage responseMessage = await httpClient.GetAsync(request);
            _ = responseMessage.EnsureSuccessStatusCode();
            string response = await responseMessage.Content.ReadAsStringAsync();
            KpSearchResult<KpPerson>? searchResultKpPerson = JsonSerializer.Deserialize<KpSearchResult<KpPerson>>(response, jsonOptions);
            Assert.NotNull(searchResultKpPerson);
            Assert.AreEqual(112, searchResultKpPerson!.Docs.Count);
            KpPerson? kpPerson = searchResultKpPerson.Docs.FirstOrDefault(i => i.Id == 1929007);
            Assert.NotNull(kpPerson);
            Assert.AreEqual(1929007, kpPerson!.Id);
            Assert.GreaterOrEqual(kpPerson.Movies.Count, 757);
        }

        [Test]
        public async Task GetEpisodesBySeriesId()
        {
            string request = $"https://api.kinopoisk.dev/season?token={_token}";
            request += "&field=movieId&search=77044";
            request += "&limit=50";
            using HttpResponseMessage responseMessage = await httpClient.GetAsync(request);
            _ = responseMessage.EnsureSuccessStatusCode();
            string response = await responseMessage.Content.ReadAsStringAsync();
            KpSearchResult<KpSeason>? searchResultKpSeason = JsonSerializer.Deserialize<KpSearchResult<KpSeason>>(response, jsonOptions);
            Assert.NotNull(searchResultKpSeason);
            Assert.AreEqual(10, searchResultKpSeason!.Docs.Count);
            KpSeason? kpSeason = searchResultKpSeason.Docs.FirstOrDefault(i => i.Number == 1);
            Assert.NotNull(kpSeason);
            Assert.AreEqual(77044, kpSeason!.MovieId);
            Assert.AreEqual(24, kpSeason.Episodes.Count);
            KpEpisode? kpEpisode = kpSeason.Episodes.FirstOrDefault(i => i.Number == 1);
            Assert.NotNull(kpEpisode);
            Assert.AreEqual("1994-09-22T00:00:00.000Z", kpEpisode!.Date);
            Assert.AreEqual("The One Where Monica Gets a Roommate", kpEpisode.EnName);
            Assert.AreEqual("Эпизод, где Моника берёт новую соседку", kpEpisode.Name);
        }

    }
}
