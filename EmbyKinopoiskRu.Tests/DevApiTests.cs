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
            Assert.AreEqual(kpMovie!.AlternativeName, "The Shawshank Redemption");
            Assert.AreEqual(kpMovie.Backdrop?.Url, "https://avatars.mds.yandex.net/get-ott/1672343/2a0000016b03d1f5365474a90d26998e2a9f/orig");
            Assert.AreEqual(kpMovie.Backdrop?.PreviewUrl, "https://avatars.mds.yandex.net/get-ott/1672343/2a0000016b03d1f5365474a90d26998e2a9f/orig");
            Assert.AreEqual(kpMovie.Countries.Count, 1);
            Assert.AreEqual(kpMovie.Description, "Бухгалтер Энди Дюфрейн обвинён в убийстве собственной жены и её любовника. Оказавшись в тюрьме под названием Шоушенк, он сталкивается с жестокостью и беззаконием, царящими по обе стороны решётки. Каждый, кто попадает в эти стены, становится их рабом до конца жизни. Но Энди, обладающий живым умом и доброй душой, находит подход как к заключённым, так и к охранникам, добиваясь их особого к себе расположения.");
            Assert.IsNull(kpMovie.EnName);
            Assert.AreEqual(kpMovie.ExternalId?.Imdb, "tt0111161");
            Assert.AreEqual(kpMovie.Genres.Count, 1);
            Assert.AreEqual(kpMovie.Id, 326);
            Assert.AreEqual(kpMovie.Logo?.Url, "https://avatars.mds.yandex.net/get-ott/1648503/2a000001705c8bf514c033f1019473a4caae/orig");
            Assert.AreEqual(kpMovie.MovieLength, 142);
            Assert.AreEqual(kpMovie.Name, "Побег из Шоушенка");
            Assert.AreEqual(kpMovie.Persons.Count, 112);
            Assert.AreEqual(kpMovie.Poster?.Url, "https://st.kp.yandex.net/images/film_big/326.jpg");
            Assert.AreEqual(kpMovie.Poster?.PreviewUrl, "https://st.kp.yandex.net/images/film_iphone/iphone360_326.jpg");
            Assert.AreEqual(kpMovie.Premiere?.World, "1994-09-10T00:00:00.000Z");
            Assert.AreEqual(kpMovie.ProductionCompanies.Count, 1);
            Assert.IsNotNull(kpMovie.Rating?.Kp);
            Assert.IsNotNull(kpMovie.Rating?.FilmCritics);
            Assert.AreEqual(kpMovie.RatingMpaa, "r");
            Assert.AreEqual(kpMovie.Slogan, "Страх - это кандалы. Надежда - это свобода");
            Assert.AreEqual(kpMovie.Status, "Выпущен");
            Assert.AreEqual(kpMovie.Type, "movie");
            Assert.IsNotNull(kpMovie.Videos);
            Assert.AreEqual(kpMovie.Videos!.Teasers.Count, 0);
            Assert.AreEqual(kpMovie.Videos!.Trailers.Count, 0);
            Assert.AreEqual(kpMovie.Year, 1994);
        }

        [Test]
        public async Task GetMoviesByMetadataStrictNameYear()
        {
            string request = $"https://api.kinopoisk.dev/movie?token={_token}";
            request += "&limit=50";
            request += "&selectFields=externalId logo poster rating movieLength id type name description year alternativeName enName backdrop countries genres persons premiere productionCompanies ratingMpaa slogan";
            request += "&isStrict=true";
            request += "&field=name&search=Побег из Шоушенка";
            request += "&field=year&search=1994";
            using HttpResponseMessage responseMessage = await httpClient.GetAsync(request);
            _ = responseMessage.EnsureSuccessStatusCode();
            string response = await responseMessage.Content.ReadAsStringAsync();
            KpSearchResult<KpMovie>? searchResultMovie = JsonSerializer.Deserialize<KpSearchResult<KpMovie>>(response, jsonOptions);
            Assert.NotNull(searchResultMovie);
            Assert.AreEqual(searchResultMovie!.Docs.Count, 1);
            KpMovie kpMovie = searchResultMovie!.Docs[0];
            Assert.AreEqual(kpMovie.AlternativeName, "The Shawshank Redemption");
            Assert.AreEqual(kpMovie.Backdrop?.Url, "https://avatars.mds.yandex.net/get-ott/1672343/2a0000016b03d1f5365474a90d26998e2a9f/orig");
            Assert.AreEqual(kpMovie.Backdrop?.PreviewUrl, "https://avatars.mds.yandex.net/get-ott/1672343/2a0000016b03d1f5365474a90d26998e2a9f/orig");
            Assert.AreEqual(kpMovie.Countries.Count, 1);
            Assert.AreEqual(kpMovie.Description, "Бухгалтер Энди Дюфрейн обвинён в убийстве собственной жены и её любовника. Оказавшись в тюрьме под названием Шоушенк, он сталкивается с жестокостью и беззаконием, царящими по обе стороны решётки. Каждый, кто попадает в эти стены, становится их рабом до конца жизни. Но Энди, обладающий живым умом и доброй душой, находит подход как к заключённым, так и к охранникам, добиваясь их особого к себе расположения.");
            Assert.IsNull(kpMovie.EnName);
            Assert.AreEqual(kpMovie.ExternalId?.Imdb, "tt0111161");
            Assert.AreEqual(kpMovie.Genres.Count, 1);
            Assert.AreEqual(kpMovie.Id, 326);
            Assert.AreEqual(kpMovie.Logo?.Url, "https://avatars.mds.yandex.net/get-ott/1648503/2a000001705c8bf514c033f1019473a4caae/orig");
            Assert.AreEqual(kpMovie.MovieLength, 142);
            Assert.AreEqual(kpMovie.Name, "Побег из Шоушенка");
            Assert.AreEqual(kpMovie.Persons.Count, 112);
            Assert.AreEqual(kpMovie.Poster?.Url, "https://st.kp.yandex.net/images/film_big/326.jpg");
            Assert.AreEqual(kpMovie.Poster?.PreviewUrl, "https://st.kp.yandex.net/images/film_iphone/iphone360_326.jpg");
            Assert.AreEqual(kpMovie.Premiere?.World, "1994-09-10T00:00:00.000Z");
            Assert.AreEqual(kpMovie.ProductionCompanies.Count, 1);
            Assert.IsNotNull(kpMovie.Rating?.Kp);
            Assert.IsNotNull(kpMovie.Rating?.FilmCritics);
            Assert.AreEqual(kpMovie.RatingMpaa, "r");
            Assert.AreEqual(kpMovie.Slogan, "Страх - это кандалы. Надежда - это свобода");
            Assert.AreEqual(kpMovie.Type, "movie");
            Assert.AreEqual(kpMovie.Year, 1994);
        }

        [Test]
        public async Task GetMoviesByMetadataStrictName()
        {
            string request = $"https://api.kinopoisk.dev/movie?token={_token}";
            request += "&limit=50";
            request += "&selectFields=externalId logo poster rating movieLength id type name description year alternativeName enName backdrop countries genres persons premiere productionCompanies ratingMpaa slogan";
            request += "&isStrict=true";
            request += "&field=name&search=Побег из Шоушенка";
            using HttpResponseMessage responseMessage = await httpClient.GetAsync(request);
            _ = responseMessage.EnsureSuccessStatusCode();
            string response = await responseMessage.Content.ReadAsStringAsync();
            KpSearchResult<KpMovie>? searchResultMovie = JsonSerializer.Deserialize<KpSearchResult<KpMovie>>(response, jsonOptions);
            Assert.NotNull(searchResultMovie);
            Assert.AreEqual(searchResultMovie!.Docs.Count, 1);
            KpMovie kpMovie = searchResultMovie!.Docs[0];
            Assert.AreEqual(kpMovie.AlternativeName, "The Shawshank Redemption");
            Assert.AreEqual(kpMovie.Backdrop?.Url, "https://avatars.mds.yandex.net/get-ott/1672343/2a0000016b03d1f5365474a90d26998e2a9f/orig");
            Assert.AreEqual(kpMovie.Backdrop?.PreviewUrl, "https://avatars.mds.yandex.net/get-ott/1672343/2a0000016b03d1f5365474a90d26998e2a9f/orig");
            Assert.AreEqual(kpMovie.Countries.Count, 1);
            Assert.AreEqual(kpMovie.Description, "Бухгалтер Энди Дюфрейн обвинён в убийстве собственной жены и её любовника. Оказавшись в тюрьме под названием Шоушенк, он сталкивается с жестокостью и беззаконием, царящими по обе стороны решётки. Каждый, кто попадает в эти стены, становится их рабом до конца жизни. Но Энди, обладающий живым умом и доброй душой, находит подход как к заключённым, так и к охранникам, добиваясь их особого к себе расположения.");
            Assert.IsNull(kpMovie.EnName);
            Assert.AreEqual(kpMovie.ExternalId?.Imdb, "tt0111161");
            Assert.AreEqual(kpMovie.Genres.Count, 1);
            Assert.AreEqual(kpMovie.Id, 326);
            Assert.AreEqual(kpMovie.Logo?.Url, "https://avatars.mds.yandex.net/get-ott/1648503/2a000001705c8bf514c033f1019473a4caae/orig");
            Assert.AreEqual(kpMovie.MovieLength, 142);
            Assert.AreEqual(kpMovie.Name, "Побег из Шоушенка");
            Assert.AreEqual(kpMovie.Persons.Count, 112);
            Assert.AreEqual(kpMovie.Poster?.Url, "https://st.kp.yandex.net/images/film_big/326.jpg");
            Assert.AreEqual(kpMovie.Poster?.PreviewUrl, "https://st.kp.yandex.net/images/film_iphone/iphone360_326.jpg");
            Assert.AreEqual(kpMovie.Premiere?.World, "1994-09-10T00:00:00.000Z");
            Assert.AreEqual(kpMovie.ProductionCompanies.Count, 1);
            Assert.IsNotNull(kpMovie.Rating?.Kp);
            Assert.IsNotNull(kpMovie.Rating?.FilmCritics);
            Assert.AreEqual(kpMovie.RatingMpaa, "r");
            Assert.AreEqual(kpMovie.Slogan, "Страх - это кандалы. Надежда - это свобода");
            Assert.AreEqual(kpMovie.Type, "movie");
            Assert.AreEqual(kpMovie.Year, 1994);
        }

        [Test]
        public async Task GetMoviesByMetadataNotStrictNameYear()
        {
            string request = $"https://api.kinopoisk.dev/movie?token={_token}";
            request += "&limit=50";
            request += "&selectFields=externalId logo poster rating movieLength id type name description year alternativeName enName backdrop countries genres persons premiere productionCompanies ratingMpaa slogan";
            request += "&isStrict=false";
            request += "&field=name&search=Побег из Шоушенка";
            request += "&field=year&search=1994";
            using HttpResponseMessage responseMessage = await httpClient.GetAsync(request);
            _ = responseMessage.EnsureSuccessStatusCode();
            string response = await responseMessage.Content.ReadAsStringAsync();
            KpSearchResult<KpMovie>? searchResultMovie = JsonSerializer.Deserialize<KpSearchResult<KpMovie>>(response, jsonOptions);
            Assert.NotNull(searchResultMovie);
            Assert.AreEqual(searchResultMovie!.Docs.Count, 1);
            KpMovie kpMovie = searchResultMovie!.Docs[0];
            Assert.AreEqual(kpMovie.AlternativeName, "The Shawshank Redemption");
            Assert.AreEqual(kpMovie.Backdrop?.Url, "https://avatars.mds.yandex.net/get-ott/1672343/2a0000016b03d1f5365474a90d26998e2a9f/orig");
            Assert.AreEqual(kpMovie.Backdrop?.PreviewUrl, "https://avatars.mds.yandex.net/get-ott/1672343/2a0000016b03d1f5365474a90d26998e2a9f/orig");
            Assert.AreEqual(kpMovie.Countries.Count, 1);
            Assert.AreEqual(kpMovie.Description, "Бухгалтер Энди Дюфрейн обвинён в убийстве собственной жены и её любовника. Оказавшись в тюрьме под названием Шоушенк, он сталкивается с жестокостью и беззаконием, царящими по обе стороны решётки. Каждый, кто попадает в эти стены, становится их рабом до конца жизни. Но Энди, обладающий живым умом и доброй душой, находит подход как к заключённым, так и к охранникам, добиваясь их особого к себе расположения.");
            Assert.IsNull(kpMovie.EnName);
            Assert.AreEqual(kpMovie.ExternalId?.Imdb, "tt0111161");
            Assert.AreEqual(kpMovie.Genres.Count, 1);
            Assert.AreEqual(kpMovie.Id, 326);
            Assert.AreEqual(kpMovie.Logo?.Url, "https://avatars.mds.yandex.net/get-ott/1648503/2a000001705c8bf514c033f1019473a4caae/orig");
            Assert.AreEqual(kpMovie.MovieLength, 142);
            Assert.AreEqual(kpMovie.Name, "Побег из Шоушенка");
            Assert.AreEqual(kpMovie.Persons.Count, 112);
            Assert.AreEqual(kpMovie.Poster?.Url, "https://st.kp.yandex.net/images/film_big/326.jpg");
            Assert.AreEqual(kpMovie.Poster?.PreviewUrl, "https://st.kp.yandex.net/images/film_iphone/iphone360_326.jpg");
            Assert.AreEqual(kpMovie.Premiere?.World, "1994-09-10T00:00:00.000Z");
            Assert.AreEqual(kpMovie.ProductionCompanies.Count, 1);
            Assert.IsNotNull(kpMovie.Rating?.Kp);
            Assert.IsNotNull(kpMovie.Rating?.FilmCritics);
            Assert.AreEqual(kpMovie.RatingMpaa, "r");
            Assert.AreEqual(kpMovie.Slogan, "Страх - это кандалы. Надежда - это свобода");
            Assert.AreEqual(kpMovie.Type, "movie");
            Assert.AreEqual(kpMovie.Year, 1994);
        }

        [Test]
        public async Task GetMoviesByMetadataNotStrictName()
        {
            string request = $"https://api.kinopoisk.dev/movie?token={_token}";
            request += "&limit=50";
            request += "&selectFields=externalId logo poster rating movieLength id type name description year alternativeName enName backdrop countries genres persons premiere productionCompanies ratingMpaa slogan";
            request += "&isStrict=false";
            request += "&field=name&search=Побег из Шоушенка";
            using HttpResponseMessage responseMessage = await httpClient.GetAsync(request);
            _ = responseMessage.EnsureSuccessStatusCode();
            string response = await responseMessage.Content.ReadAsStringAsync();
            KpSearchResult<KpMovie>? searchResultMovie = JsonSerializer.Deserialize<KpSearchResult<KpMovie>>(response, jsonOptions);
            Assert.NotNull(searchResultMovie);
            Assert.AreEqual(searchResultMovie!.Docs.Count, 1);
            KpMovie kpMovie = searchResultMovie!.Docs[0];
            Assert.AreEqual(kpMovie.AlternativeName, "The Shawshank Redemption");
            Assert.AreEqual(kpMovie.Backdrop?.Url, "https://avatars.mds.yandex.net/get-ott/1672343/2a0000016b03d1f5365474a90d26998e2a9f/orig");
            Assert.AreEqual(kpMovie.Backdrop?.PreviewUrl, "https://avatars.mds.yandex.net/get-ott/1672343/2a0000016b03d1f5365474a90d26998e2a9f/orig");
            Assert.AreEqual(kpMovie.Countries.Count, 1);
            Assert.AreEqual(kpMovie.Description, "Бухгалтер Энди Дюфрейн обвинён в убийстве собственной жены и её любовника. Оказавшись в тюрьме под названием Шоушенк, он сталкивается с жестокостью и беззаконием, царящими по обе стороны решётки. Каждый, кто попадает в эти стены, становится их рабом до конца жизни. Но Энди, обладающий живым умом и доброй душой, находит подход как к заключённым, так и к охранникам, добиваясь их особого к себе расположения.");
            Assert.IsNull(kpMovie.EnName);
            Assert.AreEqual(kpMovie.ExternalId?.Imdb, "tt0111161");
            Assert.AreEqual(kpMovie.Genres.Count, 1);
            Assert.AreEqual(kpMovie.Id, 326);
            Assert.AreEqual(kpMovie.Logo?.Url, "https://avatars.mds.yandex.net/get-ott/1648503/2a000001705c8bf514c033f1019473a4caae/orig");
            Assert.AreEqual(kpMovie.MovieLength, 142);
            Assert.AreEqual(kpMovie.Name, "Побег из Шоушенка");
            Assert.AreEqual(kpMovie.Persons.Count, 112);
            Assert.AreEqual(kpMovie.Poster?.Url, "https://st.kp.yandex.net/images/film_big/326.jpg");
            Assert.AreEqual(kpMovie.Poster?.PreviewUrl, "https://st.kp.yandex.net/images/film_iphone/iphone360_326.jpg");
            Assert.AreEqual(kpMovie.Premiere?.World, "1994-09-10T00:00:00.000Z");
            Assert.AreEqual(kpMovie.ProductionCompanies.Count, 1);
            Assert.IsNotNull(kpMovie.Rating?.Kp);
            Assert.IsNotNull(kpMovie.Rating?.FilmCritics);
            Assert.AreEqual(kpMovie.RatingMpaa, "r");
            Assert.AreEqual(kpMovie.Slogan, "Страх - это кандалы. Надежда - это свобода");
            Assert.AreEqual(kpMovie.Type, "movie");
            Assert.AreEqual(kpMovie.Year, 1994);
        }

        [Test]
        public async Task GetMoviesByMovieDetailsAlternativeNameYear()
        {
            string request = $"https://api.kinopoisk.dev/movie?token={_token}";
            request += "&limit=50";
            request += "&field=alternativeName&search=The Shawshank Redemption";
            request += "&field=year&search=1994";
            using HttpResponseMessage responseMessage = await httpClient.GetAsync(request);
            _ = responseMessage.EnsureSuccessStatusCode();
            string response = await responseMessage.Content.ReadAsStringAsync();
            KpSearchResult<KpMovie>? searchResultMovie = JsonSerializer.Deserialize<KpSearchResult<KpMovie>>(response, jsonOptions);
            Assert.NotNull(searchResultMovie);
            Assert.AreEqual(searchResultMovie!.Docs.Count, 1);
            KpMovie kpMovie = searchResultMovie!.Docs[0];
            Assert.AreEqual(kpMovie.AlternativeName, "The Shawshank Redemption");
            Assert.AreEqual(kpMovie.Backdrop?.Url, "https://avatars.mds.yandex.net/get-ott/1672343/2a0000016b03d1f5365474a90d26998e2a9f/orig");
            Assert.AreEqual(kpMovie.Backdrop?.PreviewUrl, "https://avatars.mds.yandex.net/get-ott/1672343/2a0000016b03d1f5365474a90d26998e2a9f/orig");
            Assert.AreEqual(kpMovie.Countries.Count, 1);
            Assert.AreEqual(kpMovie.Description, "Бухгалтер Энди Дюфрейн обвинён в убийстве собственной жены и её любовника. Оказавшись в тюрьме под названием Шоушенк, он сталкивается с жестокостью и беззаконием, царящими по обе стороны решётки. Каждый, кто попадает в эти стены, становится их рабом до конца жизни. Но Энди, обладающий живым умом и доброй душой, находит подход как к заключённым, так и к охранникам, добиваясь их особого к себе расположения.");
            Assert.IsNull(kpMovie.EnName);
            Assert.AreEqual(kpMovie.ExternalId?.Imdb, "tt0111161");
            Assert.AreEqual(kpMovie.Genres.Count, 1);
            Assert.AreEqual(kpMovie.Id, 326);
            Assert.AreEqual(kpMovie.Logo?.Url, "https://avatars.mds.yandex.net/get-ott/1648503/2a000001705c8bf514c033f1019473a4caae/orig");
            Assert.AreEqual(kpMovie.MovieLength, 142);
            Assert.AreEqual(kpMovie.Name, "Побег из Шоушенка");
            Assert.AreEqual(kpMovie.Persons.Count, 112);
            Assert.AreEqual(kpMovie.Poster?.Url, "https://st.kp.yandex.net/images/film_big/326.jpg");
            Assert.AreEqual(kpMovie.Poster?.PreviewUrl, "https://st.kp.yandex.net/images/film_iphone/iphone360_326.jpg");
            Assert.AreEqual(kpMovie.Premiere?.World, "1994-09-10T00:00:00.000Z");
            Assert.AreEqual(kpMovie.ProductionCompanies.Count, 1);
            Assert.IsNotNull(kpMovie.Rating?.Kp);
            Assert.IsNotNull(kpMovie.Rating?.FilmCritics);
            Assert.AreEqual(kpMovie.RatingMpaa, "r");
            Assert.AreEqual(kpMovie.Slogan, "Страх - это кандалы. Надежда - это свобода");
            Assert.AreEqual(kpMovie.Type, "movie");
            Assert.AreEqual(kpMovie.Year, 1994);
        }

        [Test]
        public async Task GetMoviesByMovieDetailsAlternativeName()
        {
            string request = $"https://api.kinopoisk.dev/movie?token={_token}";
            request += "&limit=50";
            request += "&field=alternativeName&search=The Shawshank Redemption";
            using HttpResponseMessage responseMessage = await httpClient.GetAsync(request);
            _ = responseMessage.EnsureSuccessStatusCode();
            string response = await responseMessage.Content.ReadAsStringAsync();
            KpSearchResult<KpMovie>? searchResultMovie = JsonSerializer.Deserialize<KpSearchResult<KpMovie>>(response, jsonOptions);
            Assert.NotNull(searchResultMovie);
            Assert.AreEqual(searchResultMovie!.Docs.Count, 1);
            KpMovie kpMovie = searchResultMovie!.Docs[0];
            Assert.AreEqual(kpMovie.AlternativeName, "The Shawshank Redemption");
            Assert.AreEqual(kpMovie.Backdrop?.Url, "https://avatars.mds.yandex.net/get-ott/1672343/2a0000016b03d1f5365474a90d26998e2a9f/orig");
            Assert.AreEqual(kpMovie.Backdrop?.PreviewUrl, "https://avatars.mds.yandex.net/get-ott/1672343/2a0000016b03d1f5365474a90d26998e2a9f/orig");
            Assert.AreEqual(kpMovie.Countries.Count, 1);
            Assert.AreEqual(kpMovie.Description, "Бухгалтер Энди Дюфрейн обвинён в убийстве собственной жены и её любовника. Оказавшись в тюрьме под названием Шоушенк, он сталкивается с жестокостью и беззаконием, царящими по обе стороны решётки. Каждый, кто попадает в эти стены, становится их рабом до конца жизни. Но Энди, обладающий живым умом и доброй душой, находит подход как к заключённым, так и к охранникам, добиваясь их особого к себе расположения.");
            Assert.IsNull(kpMovie.EnName);
            Assert.AreEqual(kpMovie.ExternalId?.Imdb, "tt0111161");
            Assert.AreEqual(kpMovie.Genres.Count, 1);
            Assert.AreEqual(kpMovie.Id, 326);
            Assert.AreEqual(kpMovie.Logo?.Url, "https://avatars.mds.yandex.net/get-ott/1648503/2a000001705c8bf514c033f1019473a4caae/orig");
            Assert.AreEqual(kpMovie.MovieLength, 142);
            Assert.AreEqual(kpMovie.Name, "Побег из Шоушенка");
            Assert.AreEqual(kpMovie.Persons.Count, 112);
            Assert.AreEqual(kpMovie.Poster?.Url, "https://st.kp.yandex.net/images/film_big/326.jpg");
            Assert.AreEqual(kpMovie.Poster?.PreviewUrl, "https://st.kp.yandex.net/images/film_iphone/iphone360_326.jpg");
            Assert.AreEqual(kpMovie.Premiere?.World, "1994-09-10T00:00:00.000Z");
            Assert.AreEqual(kpMovie.ProductionCompanies.Count, 1);
            Assert.IsNotNull(kpMovie.Rating?.Kp);
            Assert.IsNotNull(kpMovie.Rating?.FilmCritics);
            Assert.AreEqual(kpMovie.RatingMpaa, "r");
            Assert.AreEqual(kpMovie.Slogan, "Страх - это кандалы. Надежда - это свобода");
            Assert.AreEqual(kpMovie.Type, "movie");
            Assert.AreEqual(kpMovie.Year, 1994);
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
            Assert.AreEqual(kpPerson!.BirthPlace.Count, 0);
            Assert.AreEqual(kpPerson.Birthday, "1958-10-16T00:00:00.000Z");
            Assert.AreEqual(kpPerson.Facts.Count, 4);
            Assert.AreEqual(kpPerson.Id, 7987);
            Assert.AreEqual(kpPerson.Movies.Count, 241);
            Assert.AreEqual(kpPerson.Name, "Тим Роббинс");
            Assert.AreEqual(kpPerson.Photo, "https://st.kp.yandex.net/images/actor_iphone/iphone360_7987.jpg");
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
            Assert.AreEqual(searchResultKpPerson!.Docs.Count, 1);
            KpPerson kpPerson = searchResultKpPerson.Docs[0];
            Assert.AreEqual(kpPerson.Id, 7987);
            Assert.AreEqual(kpPerson.Name, "Тим Роббинс");
            Assert.AreEqual(kpPerson.Photo, "https://st.kp.yandex.net/images/actor_iphone/iphone360_7987.jpg");
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
            Assert.AreEqual(searchResultKpPerson!.Docs.Count, 112);
            KpPerson? kpPerson = searchResultKpPerson.Docs.FirstOrDefault(i => i.Id == 1929007);
            Assert.NotNull(kpPerson);
            Assert.AreEqual(kpPerson!.Id, 1929007);
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
            Assert.AreEqual(searchResultKpSeason!.Docs.Count, 10);
            KpSeason? kpSeason = searchResultKpSeason.Docs.FirstOrDefault(i => i.Number == 1);
            Assert.NotNull(kpSeason);
            Assert.AreEqual(kpSeason!.MovieId, 77044);
            Assert.AreEqual(kpSeason.Episodes.Count, 24);
            KpEpisode? kpEpisode = kpSeason.Episodes.FirstOrDefault(i => i.Number == 1);
            Assert.NotNull(kpEpisode);
            Assert.AreEqual(kpEpisode!.Date, "1994-09-22T00:00:00.000Z");
            Assert.AreEqual(kpEpisode.EnName, "The One Where Monica Gets a Roommate");
            Assert.AreEqual(kpEpisode.Name, "Эпизод, где Моника берёт новую соседку");
        }

    }
}
