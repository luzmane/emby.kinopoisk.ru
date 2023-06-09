using System.Net;

using EmbyKinopoiskRu.Configuration;
using EmbyKinopoiskRu.Provider.RemoteMetadata;

using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Providers;

namespace EmbyKinopoiskRu.Tests.KinopoiskDev;

[Collection("Sequential")]
public class KpMovieProviderTest : BaseTest
{
    private static readonly NLog.ILogger Logger = NLog.LogManager.GetLogger(nameof(KpMovieProviderTest));

    private readonly KpMovieProvider _kpMovieProvider;


    #region Test configs
    public KpMovieProviderTest() : base(Logger)
    {
        _pluginConfiguration.Token = GetKinopoiskDevToken();

        ConfigLibraryManager();

        ConfigXmlSerializer();

        _kpMovieProvider = new(_httpClient, _logManager.Object);
    }
    protected override void ConfigLibraryManager()
    {
        base.ConfigLibraryManager();

    }
    protected override void ConfigXmlSerializer()
    {
        base.ConfigXmlSerializer();

    }

    #endregion

    [Fact]
    public async void KpMovieProvider_ForCodeCoverage()
    {
        Logger.Info($"Start '{nameof(KpMovieProvider_ForCodeCoverage)}'");

        Assert.NotNull(_kpMovieProvider.Name);

        Assert.NotEmpty(_kpMovieProvider.Features);

        HttpResponseInfo response = await _kpMovieProvider.GetImageResponse("https://www.google.com", CancellationToken.None);
        Assert.True(response.StatusCode == HttpStatusCode.OK);

        _logManager.Verify(lm => lm.GetLogger("KpMovieProvider"), Times.Once());
        _logManager.Verify(lm => lm.GetLogger("KinopoiskRu"), Times.Once());
        VerifyNoOtherCalls();

        Logger.Info($"Finished '{nameof(KpMovieProvider_ForCodeCoverage)}'");
    }

    [Fact]
    public async void KpMovieProvider_GetMetadata_Provider_Kp()
    {
        Logger.Info($"Start '{nameof(KpMovieProvider_GetMetadata_Provider_Kp)}'");

        var movieInfo = new MovieInfo()
        {
            ProviderIds = new(new() { { Plugin.PluginKey, "326" } })
        };

        _ = _applicationPaths
            .SetupGet(m => m.PluginConfigurationsPath)
            .Returns("KpMovieProvider_GetMetadata_Provider_Kp");

        using var cancellationTokenSource = new CancellationTokenSource();
        MetadataResult<Movie> result = await _kpMovieProvider.GetMetadata(movieInfo, cancellationTokenSource.Token);

        Movie movie = result.Item;
        Assert.NotNull(movie);
        Assert.True(result.HasMetadata);
        Assert.Equal("326", movie.GetProviderId(Plugin.PluginKey));
        Assert.Equal("tt0111161", movie.GetProviderId(MetadataProviders.Imdb));
        Assert.Equal("278", movie.GetProviderId(MetadataProviders.Tmdb));
        Assert.Equal("Video", movie.MediaType);
        Assert.True(5 < movie.CommunityRating);
        Assert.Equal("326", movie.ExternalId);
        _ = Assert.Single(movie.Genres);
        Assert.Equal("драма", movie.Genres[0]);
        Assert.Equal("Побег из Шоушенка", movie.Name);
        Assert.Equal("r", movie.OfficialRating);
        Assert.Equal("The Shawshank Redemption", movie.OriginalTitle);
        Assert.Equal("Бухгалтер Энди Дюфрейн обвинён в убийстве собственной жены и её любовника. Оказавшись в тюрьме под названием Шоушенк, он сталкивается с жестокостью и беззаконием, царящими по обе стороны решётки. Каждый, кто попадает в эти стены, становится их рабом до конца жизни. Но Энди, обладающий живым умом и доброй душой, находит подход как к заключённым, так и к охранникам, добиваясь их особого к себе расположения.<br/><br/><b>Интересное:</b><br/>&nbsp;&nbsp;&nbsp;&nbsp;* Фильм снят по мотивам повести <a href=\"/name/24263/\" class=\"all\">Стивена Кинга</a> «Рита Хейуорт и спасение из Шоушенка» (Rita Hayworth and Shawshank Redemption), опубликованной в составе сборника «Четыре сезона» (Different Seasons, 1982).<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Съемки проходили в&#160;Мэнсфилдской исправительной колонии в&#160;штате Огайо. Тюрьма находилась в&#160;таком плачевном состоянии, что пришлось приводить её&#160;в&#160;должный вид.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Оригинальная повесть <a href=\"/name/24263/\" class=\"all\">Стивена Кинга</a> была, по словам самого писателя, кульминацией всех его впечатлений от различных тюремных фильмов, которые он смотрел в детстве.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* <a href=\"/name/24263/\" class=\"all\">Стивен Кинг</a> согласился продать права на&#160;свое произведение практически даром, так как с&#160;<a href=\"/name/24262/\" class=\"all\">Фрэнком</a> их&#160;связывает давняя крепкая дружба. Произошло это после того, как Фрэнк довольно успешно экранизировал рассказ Кинга &#171;<a href=\"/film/7429/\" class=\"all\">Женщина в палате</a>&#187;.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Американское общество защиты животных выступило с&#160;критикой в&#160;адрес фильма, в&#160;котором единственным представителем фауны стал ворон старика Брукса. В&#160;картине есть сцена кормления птицы червяком, найденном во&#160;время обеда в&#160;тарелке главного героя фильма. Общество настояло на&#160;том, чтобы была использована уже мертвая личинка, погибшая естественной смертью. После того как такая особь была найдена, сцену отсняли.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Роль Томми Уильямса изначально была написана под <a href=\"/name/25584/\" class=\"all\">Брэда Питта</a>.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Картина, которую смотрят заключенные, — &#171;<a href=\"/film/8299/\" class=\"all\">Гильда</a>&#187;.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Начальник тюрьмы Нортон насвистывает гимн «Eine feste Burg ist unser Gott», название которого переводится примерно так: «Могучая крепость и есть наш бог».<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Фотографии молодого <a href=\"/name/6750/\" class=\"all\">Моргана Фримана</a> на&#160;документах на&#160;самом деле являются фотографиями его сына <a href=\"/name/6767/\" class=\"all\">Альфонсо Фримана</a>, который также снялся в&#160;одном из&#160;эпизодов фильма.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Несмотря на&#160;то, что в&#160;кинотеатрах фильм не&#160;собрал больших денег, он&#160;стал одним из&#160;самых кассовых релизов на&#160;видео, а&#160;впоследствии и&#160;на&#160;DVD.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Фильм посвящён Аллену Грину (Allen Greene) — близкому другу режиссёра. Аллен скончался незадолго до выхода фильма из-за осложнений СПИДа.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Бывший начальник Мэнсфилдской тюрьмы, где проходили натурные съёмки фильма, <a href=\"/name/1104241/\" class=\"all\">Дэннис Бэйкер</a> снялся в роли пожилого заключённого, сидящего в тюремном автобусе позади Томми Уильямса.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Сценарий <a href=\"/name/24262/\" class=\"all\">Фрэнка Дарабонта</a> очень понравился другому режиссёру, успешно экранизировавшему произведения <a href=\"/name/24263/\" class=\"all\">Стивена Кинга</a>, — <a href=\"/name/5899/\" class=\"all\">Робу Райнеру</a>, постановщику &#171;<a href=\"/film/498/\" class=\"all\">Останься со мной</a>&#187; (1986) и &#171;<a href=\"/film/1574/\" class=\"all\">Мизери</a>&#187; (1990). Райнер был так захвачен материалом, что предложил Дарабонту $2,5 млн за права на сценарий и постановку фильма. Дарабонт серьёзно обдумал предложение, но в конечном счёте решил, что для него этот проект — &#171;шанс сделать что-то действительно великое&#187;, и поставил фильм сам.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* <a href=\"/name/5899/\" class=\"all\">Роб Райнер</a> видел в ролях Реда и Энди Дюфрейна соответственно <a href=\"/name/5679/\" class=\"all\">Харрисона Форда</a> и <a href=\"/name/20302/\" class=\"all\">Тома Круза</a>.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Роль Энди Дюфрейна изначально предложили <a href=\"/name/9144/\" class=\"all\">Тому Хэнксу</a>. Он очень заинтересовался, но не смог принять предложение, из-за того что уже был занят в проекте &#171;<a href=\"/film/448/\" class=\"all\">Форрест Гамп</a>&#187; (1994). Впоследствии Том Хэнкс снялся в главной роли в тюремной драме <a href=\"/name/24262/\" class=\"all\">Фрэнка Дарабонта</a> &#171;<a href=\"/film/435/\" class=\"all\">Зеленая миля</a>&#187; (1999), также поставленной по роману <a href=\"/name/24263/\" class=\"all\">Стивена Кинга</a>.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Роль Энди Дюфрейна также предлагали <a href=\"/name/24087/\" class=\"all\">Кевину Костнеру</a>, но актёр отказался от предложения, о чем впоследствии сильно жалел.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* В оригинальной повести <a href=\"/name/24263/\" class=\"all\">Стивена Кинга</a> Ред — ирландец. Несмотря на то, что в экранизации роль Реда сыграл чернокожий <a href=\"/name/6750/\" class=\"all\">Морган Фриман</a>, было решено оставить в фильме реплику Реда «Может быть, потому что я — ирландец», — как удачную шутку.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Несмотря на то что почти все жители города Мэнсфилда изъявили желание принять участие в съёмках массовых сцен фильма, большинство жителей оказались слишком заняты своей работой и не смогли сниматься. Массовку пришлось набирать в местной богадельне, причём некоторые из её обитателей были бывшими заключенными.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Когда на экране показываются крупные планы рук Энди Дюфрейна, когда в начале фильма он заряжает револьвер и когда Энди вырезает своё имя на стене камеры, — это на самом деле руки режиссёра <a href=\"/name/24262/\" class=\"all\">Фрэнка Дарабонта</a>. Эти кадры были сняты в процессе постпроизводства фильма.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Двое из заключённых Шоушенка носят имена Хейвуд и Флойд. Это отсылка к трилогии <a href=\"/name/47956/\" class=\"all\">Артура Ч. Кларка</a> «<a href=\"/film/380/\" class=\"all\">Космическая одиссея</a>», связующим героем которой является доктор Хейвуд Флойд.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Тюремный номер Энди Дюфрейна — 37927.<br/>", movie.Overview);
        Assert.Equal(1994, movie.ProductionYear);
        Assert.True(4 <= movie.RemoteTrailers.Length);
        Assert.Equal(142, movie.Size);
        Assert.Equal(movie.Name, movie.SortName);
        _ = Assert.Single(movie.Studios);
        Assert.Equal("Страх - это кандалы. Надежда - это свобода", movie.Tagline);

        Assert.Equal(80, result.People.Count);
        PersonInfo? person = result.People.FirstOrDefault(p => "Тим Роббинс".Equals(p.Name, StringComparison.Ordinal));
        Assert.NotNull(person);
        Assert.Equal("7987", person.GetProviderId(Plugin.PluginKey));
        Assert.Equal("Andy Dufresne", person.Role);
        Assert.Equal("Тим Роббинс", person.Name);
        Assert.True(!string.IsNullOrWhiteSpace(person.ImageUrl));

        _logManager.Verify(lm => lm.GetLogger(It.IsAny<string>()), Times.Exactly(4));
        _applicationPaths.VerifyGet(ap => ap.PluginConfigurationsPath, Times.Once());
        _xmlSerializer.Verify(xs => xs.DeserializeFromFile(typeof(PluginConfiguration), "KpMovieProvider_GetMetadata_Provider_Kp/EmbyKinopoiskRu.xml"), Times.Once());
        _localizationManager.Verify(lm => lm.RemoveDiacritics("Побег из Шоушенка"), Times.Once());
        VerifyNoOtherCalls();

        Logger.Info($"Finished '{nameof(KpMovieProvider_GetMetadata_Provider_Kp)}'");
    }

    [Fact]
    public async void KpMovieProvider_GetMetadata_Provider_Imdb()
    {
        Logger.Info($"Start '{nameof(KpMovieProvider_GetMetadata_Provider_Imdb)}'");

        var movieInfo = new MovieInfo()
        {
            ProviderIds = new(new() { { MetadataProviders.Imdb.ToString(), "tt0111161" } })
        };

        _ = _applicationPaths
            .SetupGet(m => m.PluginConfigurationsPath)
            .Returns("KpMovieProvider_GetMetadata_Provider_Imdb");

        using var cancellationTokenSource = new CancellationTokenSource();
        MetadataResult<Movie> result = await _kpMovieProvider.GetMetadata(movieInfo, cancellationTokenSource.Token);

        Movie movie = result.Item;
        Assert.NotNull(movie);
        Assert.True(result.HasMetadata);
        Assert.Equal("326", movie.GetProviderId(Plugin.PluginKey));
        Assert.Equal("tt0111161", movie.GetProviderId(MetadataProviders.Imdb));
        Assert.Equal("278", movie.GetProviderId(MetadataProviders.Tmdb));
        Assert.Equal("Video", movie.MediaType);
        Assert.True(5 < movie.CommunityRating);
        Assert.Equal("326", movie.ExternalId);
        _ = Assert.Single(movie.Genres);
        Assert.Equal("драма", movie.Genres[0]);
        Assert.Equal("Побег из Шоушенка", movie.Name);
        Assert.Equal("r", movie.OfficialRating);
        Assert.Equal("The Shawshank Redemption", movie.OriginalTitle);
        Assert.Equal("Бухгалтер Энди Дюфрейн обвинён в убийстве собственной жены и её любовника. Оказавшись в тюрьме под названием Шоушенк, он сталкивается с жестокостью и беззаконием, царящими по обе стороны решётки. Каждый, кто попадает в эти стены, становится их рабом до конца жизни. Но Энди, обладающий живым умом и доброй душой, находит подход как к заключённым, так и к охранникам, добиваясь их особого к себе расположения.<br/><br/><b>Интересное:</b><br/>&nbsp;&nbsp;&nbsp;&nbsp;* Фильм снят по мотивам повести <a href=\"/name/24263/\" class=\"all\">Стивена Кинга</a> «Рита Хейуорт и спасение из Шоушенка» (Rita Hayworth and Shawshank Redemption), опубликованной в составе сборника «Четыре сезона» (Different Seasons, 1982).<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Съемки проходили в&#160;Мэнсфилдской исправительной колонии в&#160;штате Огайо. Тюрьма находилась в&#160;таком плачевном состоянии, что пришлось приводить её&#160;в&#160;должный вид.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Оригинальная повесть <a href=\"/name/24263/\" class=\"all\">Стивена Кинга</a> была, по словам самого писателя, кульминацией всех его впечатлений от различных тюремных фильмов, которые он смотрел в детстве.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* <a href=\"/name/24263/\" class=\"all\">Стивен Кинг</a> согласился продать права на&#160;свое произведение практически даром, так как с&#160;<a href=\"/name/24262/\" class=\"all\">Фрэнком</a> их&#160;связывает давняя крепкая дружба. Произошло это после того, как Фрэнк довольно успешно экранизировал рассказ Кинга &#171;<a href=\"/film/7429/\" class=\"all\">Женщина в палате</a>&#187;.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Американское общество защиты животных выступило с&#160;критикой в&#160;адрес фильма, в&#160;котором единственным представителем фауны стал ворон старика Брукса. В&#160;картине есть сцена кормления птицы червяком, найденном во&#160;время обеда в&#160;тарелке главного героя фильма. Общество настояло на&#160;том, чтобы была использована уже мертвая личинка, погибшая естественной смертью. После того как такая особь была найдена, сцену отсняли.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Роль Томми Уильямса изначально была написана под <a href=\"/name/25584/\" class=\"all\">Брэда Питта</a>.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Картина, которую смотрят заключенные, — &#171;<a href=\"/film/8299/\" class=\"all\">Гильда</a>&#187;.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Начальник тюрьмы Нортон насвистывает гимн «Eine feste Burg ist unser Gott», название которого переводится примерно так: «Могучая крепость и есть наш бог».<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Фотографии молодого <a href=\"/name/6750/\" class=\"all\">Моргана Фримана</a> на&#160;документах на&#160;самом деле являются фотографиями его сына <a href=\"/name/6767/\" class=\"all\">Альфонсо Фримана</a>, который также снялся в&#160;одном из&#160;эпизодов фильма.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Несмотря на&#160;то, что в&#160;кинотеатрах фильм не&#160;собрал больших денег, он&#160;стал одним из&#160;самых кассовых релизов на&#160;видео, а&#160;впоследствии и&#160;на&#160;DVD.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Фильм посвящён Аллену Грину (Allen Greene) — близкому другу режиссёра. Аллен скончался незадолго до выхода фильма из-за осложнений СПИДа.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Бывший начальник Мэнсфилдской тюрьмы, где проходили натурные съёмки фильма, <a href=\"/name/1104241/\" class=\"all\">Дэннис Бэйкер</a> снялся в роли пожилого заключённого, сидящего в тюремном автобусе позади Томми Уильямса.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Сценарий <a href=\"/name/24262/\" class=\"all\">Фрэнка Дарабонта</a> очень понравился другому режиссёру, успешно экранизировавшему произведения <a href=\"/name/24263/\" class=\"all\">Стивена Кинга</a>, — <a href=\"/name/5899/\" class=\"all\">Робу Райнеру</a>, постановщику &#171;<a href=\"/film/498/\" class=\"all\">Останься со мной</a>&#187; (1986) и &#171;<a href=\"/film/1574/\" class=\"all\">Мизери</a>&#187; (1990). Райнер был так захвачен материалом, что предложил Дарабонту $2,5 млн за права на сценарий и постановку фильма. Дарабонт серьёзно обдумал предложение, но в конечном счёте решил, что для него этот проект — &#171;шанс сделать что-то действительно великое&#187;, и поставил фильм сам.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* <a href=\"/name/5899/\" class=\"all\">Роб Райнер</a> видел в ролях Реда и Энди Дюфрейна соответственно <a href=\"/name/5679/\" class=\"all\">Харрисона Форда</a> и <a href=\"/name/20302/\" class=\"all\">Тома Круза</a>.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Роль Энди Дюфрейна изначально предложили <a href=\"/name/9144/\" class=\"all\">Тому Хэнксу</a>. Он очень заинтересовался, но не смог принять предложение, из-за того что уже был занят в проекте &#171;<a href=\"/film/448/\" class=\"all\">Форрест Гамп</a>&#187; (1994). Впоследствии Том Хэнкс снялся в главной роли в тюремной драме <a href=\"/name/24262/\" class=\"all\">Фрэнка Дарабонта</a> &#171;<a href=\"/film/435/\" class=\"all\">Зеленая миля</a>&#187; (1999), также поставленной по роману <a href=\"/name/24263/\" class=\"all\">Стивена Кинга</a>.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Роль Энди Дюфрейна также предлагали <a href=\"/name/24087/\" class=\"all\">Кевину Костнеру</a>, но актёр отказался от предложения, о чем впоследствии сильно жалел.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* В оригинальной повести <a href=\"/name/24263/\" class=\"all\">Стивена Кинга</a> Ред — ирландец. Несмотря на то, что в экранизации роль Реда сыграл чернокожий <a href=\"/name/6750/\" class=\"all\">Морган Фриман</a>, было решено оставить в фильме реплику Реда «Может быть, потому что я — ирландец», — как удачную шутку.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Несмотря на то что почти все жители города Мэнсфилда изъявили желание принять участие в съёмках массовых сцен фильма, большинство жителей оказались слишком заняты своей работой и не смогли сниматься. Массовку пришлось набирать в местной богадельне, причём некоторые из её обитателей были бывшими заключенными.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Когда на экране показываются крупные планы рук Энди Дюфрейна, когда в начале фильма он заряжает револьвер и когда Энди вырезает своё имя на стене камеры, — это на самом деле руки режиссёра <a href=\"/name/24262/\" class=\"all\">Фрэнка Дарабонта</a>. Эти кадры были сняты в процессе постпроизводства фильма.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Двое из заключённых Шоушенка носят имена Хейвуд и Флойд. Это отсылка к трилогии <a href=\"/name/47956/\" class=\"all\">Артура Ч. Кларка</a> «<a href=\"/film/380/\" class=\"all\">Космическая одиссея</a>», связующим героем которой является доктор Хейвуд Флойд.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Тюремный номер Энди Дюфрейна — 37927.<br/>", movie.Overview);
        Assert.Equal(1994, movie.ProductionYear);
        Assert.True(4 <= movie.RemoteTrailers.Length);
        Assert.Equal(142, movie.Size);
        Assert.Equal(movie.Name, movie.SortName);
        _ = Assert.Single(movie.Studios);
        Assert.Equal("Страх - это кандалы. Надежда - это свобода", movie.Tagline);

        Assert.Equal(80, result.People.Count);
        PersonInfo? person = result.People.FirstOrDefault(p => "Тим Роббинс".Equals(p.Name, StringComparison.Ordinal));
        Assert.NotNull(person);
        Assert.Equal("7987", person.GetProviderId(Plugin.PluginKey));
        Assert.Equal("Andy Dufresne", person.Role);
        Assert.Equal("Тим Роббинс", person.Name);
        Assert.True(!string.IsNullOrWhiteSpace(person.ImageUrl));

        _logManager.Verify(lm => lm.GetLogger(It.IsAny<string>()), Times.Exactly(4));
        _applicationPaths.VerifyGet(ap => ap.PluginConfigurationsPath, Times.Once());
        _xmlSerializer.Verify(xs => xs.DeserializeFromFile(typeof(PluginConfiguration), "KpMovieProvider_GetMetadata_Provider_Imdb/EmbyKinopoiskRu.xml"), Times.Once());
        _localizationManager.Verify(lm => lm.RemoveDiacritics("Побег из Шоушенка"), Times.Once());
        VerifyNoOtherCalls();

        Logger.Info($"Finished '{nameof(KpMovieProvider_GetMetadata_Provider_Imdb)}'");
    }

    [Fact]
    public async void KpMovieProvider_GetMetadata_Provider_Tmdb()
    {
        Logger.Info($"Start '{nameof(KpMovieProvider_GetMetadata_Provider_Tmdb)}'");

        var movieInfo = new MovieInfo()
        {
            ProviderIds = new(new() { { MetadataProviders.Tmdb.ToString(), "278" } })
        };

        _ = _applicationPaths
            .SetupGet(m => m.PluginConfigurationsPath)
            .Returns("KpMovieProvider_GetMetadata_Provider_Tmdb");

        using var cancellationTokenSource = new CancellationTokenSource();
        MetadataResult<Movie> result = await _kpMovieProvider.GetMetadata(movieInfo, cancellationTokenSource.Token);

        Movie movie = result.Item;
        Assert.NotNull(movie);
        Assert.True(result.HasMetadata);
        Assert.Equal("326", movie.GetProviderId(Plugin.PluginKey));
        Assert.Equal("tt0111161", movie.GetProviderId(MetadataProviders.Imdb));
        Assert.Equal("278", movie.GetProviderId(MetadataProviders.Tmdb));
        Assert.Equal("Video", movie.MediaType);
        Assert.True(5 < movie.CommunityRating);
        Assert.Equal("326", movie.ExternalId);
        _ = Assert.Single(movie.Genres);
        Assert.Equal("драма", movie.Genres[0]);
        Assert.Equal("Побег из Шоушенка", movie.Name);
        Assert.Equal("r", movie.OfficialRating);
        Assert.Equal("The Shawshank Redemption", movie.OriginalTitle);
        Assert.Equal("Бухгалтер Энди Дюфрейн обвинён в убийстве собственной жены и её любовника. Оказавшись в тюрьме под названием Шоушенк, он сталкивается с жестокостью и беззаконием, царящими по обе стороны решётки. Каждый, кто попадает в эти стены, становится их рабом до конца жизни. Но Энди, обладающий живым умом и доброй душой, находит подход как к заключённым, так и к охранникам, добиваясь их особого к себе расположения.<br/><br/><b>Интересное:</b><br/>&nbsp;&nbsp;&nbsp;&nbsp;* Фильм снят по мотивам повести <a href=\"/name/24263/\" class=\"all\">Стивена Кинга</a> «Рита Хейуорт и спасение из Шоушенка» (Rita Hayworth and Shawshank Redemption), опубликованной в составе сборника «Четыре сезона» (Different Seasons, 1982).<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Съемки проходили в&#160;Мэнсфилдской исправительной колонии в&#160;штате Огайо. Тюрьма находилась в&#160;таком плачевном состоянии, что пришлось приводить её&#160;в&#160;должный вид.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Оригинальная повесть <a href=\"/name/24263/\" class=\"all\">Стивена Кинга</a> была, по словам самого писателя, кульминацией всех его впечатлений от различных тюремных фильмов, которые он смотрел в детстве.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* <a href=\"/name/24263/\" class=\"all\">Стивен Кинг</a> согласился продать права на&#160;свое произведение практически даром, так как с&#160;<a href=\"/name/24262/\" class=\"all\">Фрэнком</a> их&#160;связывает давняя крепкая дружба. Произошло это после того, как Фрэнк довольно успешно экранизировал рассказ Кинга &#171;<a href=\"/film/7429/\" class=\"all\">Женщина в палате</a>&#187;.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Американское общество защиты животных выступило с&#160;критикой в&#160;адрес фильма, в&#160;котором единственным представителем фауны стал ворон старика Брукса. В&#160;картине есть сцена кормления птицы червяком, найденном во&#160;время обеда в&#160;тарелке главного героя фильма. Общество настояло на&#160;том, чтобы была использована уже мертвая личинка, погибшая естественной смертью. После того как такая особь была найдена, сцену отсняли.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Роль Томми Уильямса изначально была написана под <a href=\"/name/25584/\" class=\"all\">Брэда Питта</a>.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Картина, которую смотрят заключенные, — &#171;<a href=\"/film/8299/\" class=\"all\">Гильда</a>&#187;.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Начальник тюрьмы Нортон насвистывает гимн «Eine feste Burg ist unser Gott», название которого переводится примерно так: «Могучая крепость и есть наш бог».<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Фотографии молодого <a href=\"/name/6750/\" class=\"all\">Моргана Фримана</a> на&#160;документах на&#160;самом деле являются фотографиями его сына <a href=\"/name/6767/\" class=\"all\">Альфонсо Фримана</a>, который также снялся в&#160;одном из&#160;эпизодов фильма.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Несмотря на&#160;то, что в&#160;кинотеатрах фильм не&#160;собрал больших денег, он&#160;стал одним из&#160;самых кассовых релизов на&#160;видео, а&#160;впоследствии и&#160;на&#160;DVD.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Фильм посвящён Аллену Грину (Allen Greene) — близкому другу режиссёра. Аллен скончался незадолго до выхода фильма из-за осложнений СПИДа.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Бывший начальник Мэнсфилдской тюрьмы, где проходили натурные съёмки фильма, <a href=\"/name/1104241/\" class=\"all\">Дэннис Бэйкер</a> снялся в роли пожилого заключённого, сидящего в тюремном автобусе позади Томми Уильямса.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Сценарий <a href=\"/name/24262/\" class=\"all\">Фрэнка Дарабонта</a> очень понравился другому режиссёру, успешно экранизировавшему произведения <a href=\"/name/24263/\" class=\"all\">Стивена Кинга</a>, — <a href=\"/name/5899/\" class=\"all\">Робу Райнеру</a>, постановщику &#171;<a href=\"/film/498/\" class=\"all\">Останься со мной</a>&#187; (1986) и &#171;<a href=\"/film/1574/\" class=\"all\">Мизери</a>&#187; (1990). Райнер был так захвачен материалом, что предложил Дарабонту $2,5 млн за права на сценарий и постановку фильма. Дарабонт серьёзно обдумал предложение, но в конечном счёте решил, что для него этот проект — &#171;шанс сделать что-то действительно великое&#187;, и поставил фильм сам.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* <a href=\"/name/5899/\" class=\"all\">Роб Райнер</a> видел в ролях Реда и Энди Дюфрейна соответственно <a href=\"/name/5679/\" class=\"all\">Харрисона Форда</a> и <a href=\"/name/20302/\" class=\"all\">Тома Круза</a>.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Роль Энди Дюфрейна изначально предложили <a href=\"/name/9144/\" class=\"all\">Тому Хэнксу</a>. Он очень заинтересовался, но не смог принять предложение, из-за того что уже был занят в проекте &#171;<a href=\"/film/448/\" class=\"all\">Форрест Гамп</a>&#187; (1994). Впоследствии Том Хэнкс снялся в главной роли в тюремной драме <a href=\"/name/24262/\" class=\"all\">Фрэнка Дарабонта</a> &#171;<a href=\"/film/435/\" class=\"all\">Зеленая миля</a>&#187; (1999), также поставленной по роману <a href=\"/name/24263/\" class=\"all\">Стивена Кинга</a>.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Роль Энди Дюфрейна также предлагали <a href=\"/name/24087/\" class=\"all\">Кевину Костнеру</a>, но актёр отказался от предложения, о чем впоследствии сильно жалел.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* В оригинальной повести <a href=\"/name/24263/\" class=\"all\">Стивена Кинга</a> Ред — ирландец. Несмотря на то, что в экранизации роль Реда сыграл чернокожий <a href=\"/name/6750/\" class=\"all\">Морган Фриман</a>, было решено оставить в фильме реплику Реда «Может быть, потому что я — ирландец», — как удачную шутку.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Несмотря на то что почти все жители города Мэнсфилда изъявили желание принять участие в съёмках массовых сцен фильма, большинство жителей оказались слишком заняты своей работой и не смогли сниматься. Массовку пришлось набирать в местной богадельне, причём некоторые из её обитателей были бывшими заключенными.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Когда на экране показываются крупные планы рук Энди Дюфрейна, когда в начале фильма он заряжает револьвер и когда Энди вырезает своё имя на стене камеры, — это на самом деле руки режиссёра <a href=\"/name/24262/\" class=\"all\">Фрэнка Дарабонта</a>. Эти кадры были сняты в процессе постпроизводства фильма.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Двое из заключённых Шоушенка носят имена Хейвуд и Флойд. Это отсылка к трилогии <a href=\"/name/47956/\" class=\"all\">Артура Ч. Кларка</a> «<a href=\"/film/380/\" class=\"all\">Космическая одиссея</a>», связующим героем которой является доктор Хейвуд Флойд.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Тюремный номер Энди Дюфрейна — 37927.<br/>", movie.Overview);
        Assert.Equal(1994, movie.ProductionYear);
        Assert.True(4 <= movie.RemoteTrailers.Length);
        Assert.Equal(142, movie.Size);
        Assert.Equal(movie.Name, movie.SortName);
        _ = Assert.Single(movie.Studios);
        Assert.Equal("Страх - это кандалы. Надежда - это свобода", movie.Tagline);

        Assert.Equal(80, result.People.Count);
        PersonInfo? person = result.People.FirstOrDefault(p => "Тим Роббинс".Equals(p.Name, StringComparison.Ordinal));
        Assert.NotNull(person);
        Assert.Equal("7987", person.GetProviderId(Plugin.PluginKey));
        Assert.Equal("Andy Dufresne", person.Role);
        Assert.Equal("Тим Роббинс", person.Name);
        Assert.True(!string.IsNullOrWhiteSpace(person.ImageUrl));

        _logManager.Verify(lm => lm.GetLogger(It.IsAny<string>()), Times.Exactly(4));
        _applicationPaths.VerifyGet(ap => ap.PluginConfigurationsPath, Times.Once());
        _xmlSerializer.Verify(xs => xs.DeserializeFromFile(typeof(PluginConfiguration), "KpMovieProvider_GetMetadata_Provider_Tmdb/EmbyKinopoiskRu.xml"), Times.Once());
        _localizationManager.Verify(lm => lm.RemoveDiacritics("Побег из Шоушенка"), Times.Once());
        VerifyNoOtherCalls();

        Logger.Info($"Finished '{nameof(KpMovieProvider_GetMetadata_Provider_Tmdb)}'");
    }

    [Fact]
    public async void KpMovieProvider_GetMetadata_NameAndYear()
    {
        Logger.Info($"Start '{nameof(KpMovieProvider_GetMetadata_NameAndYear)}'");

        var movieInfo = new MovieInfo()
        {
            Name = "Побег из Шоушенка",
            Year = 1994
        };

        _ = _applicationPaths
            .SetupGet(m => m.PluginConfigurationsPath)
            .Returns("KpMovieProvider_GetMetadata_NameAndYear");

        using var cancellationTokenSource = new CancellationTokenSource();
        MetadataResult<Movie> result = await _kpMovieProvider.GetMetadata(movieInfo, cancellationTokenSource.Token);

        Movie movie = result.Item;
        Assert.NotNull(movie);
        Assert.True(result.HasMetadata);
        Assert.Equal("326", movie.GetProviderId(Plugin.PluginKey));
        Assert.Equal("tt0111161", movie.GetProviderId(MetadataProviders.Imdb));
        Assert.Equal("278", movie.GetProviderId(MetadataProviders.Tmdb));
        Assert.Equal("Video", movie.MediaType);
        Assert.True(5 < movie.CommunityRating);
        Assert.Equal("326", movie.ExternalId);
        _ = Assert.Single(movie.Genres);
        Assert.Equal("драма", movie.Genres[0]);
        Assert.Equal("Побег из Шоушенка", movie.Name);
        Assert.Equal("r", movie.OfficialRating);
        Assert.Equal("The Shawshank Redemption", movie.OriginalTitle);
        Assert.Equal("Бухгалтер Энди Дюфрейн обвинён в убийстве собственной жены и её любовника. Оказавшись в тюрьме под названием Шоушенк, он сталкивается с жестокостью и беззаконием, царящими по обе стороны решётки. Каждый, кто попадает в эти стены, становится их рабом до конца жизни. Но Энди, обладающий живым умом и доброй душой, находит подход как к заключённым, так и к охранникам, добиваясь их особого к себе расположения.<br/><br/><b>Интересное:</b><br/>&nbsp;&nbsp;&nbsp;&nbsp;* Фильм снят по мотивам повести <a href=\"/name/24263/\" class=\"all\">Стивена Кинга</a> «Рита Хейуорт и спасение из Шоушенка» (Rita Hayworth and Shawshank Redemption), опубликованной в составе сборника «Четыре сезона» (Different Seasons, 1982).<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Съемки проходили в&#160;Мэнсфилдской исправительной колонии в&#160;штате Огайо. Тюрьма находилась в&#160;таком плачевном состоянии, что пришлось приводить её&#160;в&#160;должный вид.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Оригинальная повесть <a href=\"/name/24263/\" class=\"all\">Стивена Кинга</a> была, по словам самого писателя, кульминацией всех его впечатлений от различных тюремных фильмов, которые он смотрел в детстве.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* <a href=\"/name/24263/\" class=\"all\">Стивен Кинг</a> согласился продать права на&#160;свое произведение практически даром, так как с&#160;<a href=\"/name/24262/\" class=\"all\">Фрэнком</a> их&#160;связывает давняя крепкая дружба. Произошло это после того, как Фрэнк довольно успешно экранизировал рассказ Кинга &#171;<a href=\"/film/7429/\" class=\"all\">Женщина в палате</a>&#187;.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Американское общество защиты животных выступило с&#160;критикой в&#160;адрес фильма, в&#160;котором единственным представителем фауны стал ворон старика Брукса. В&#160;картине есть сцена кормления птицы червяком, найденном во&#160;время обеда в&#160;тарелке главного героя фильма. Общество настояло на&#160;том, чтобы была использована уже мертвая личинка, погибшая естественной смертью. После того как такая особь была найдена, сцену отсняли.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Роль Томми Уильямса изначально была написана под <a href=\"/name/25584/\" class=\"all\">Брэда Питта</a>.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Картина, которую смотрят заключенные, — &#171;<a href=\"/film/8299/\" class=\"all\">Гильда</a>&#187;.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Начальник тюрьмы Нортон насвистывает гимн «Eine feste Burg ist unser Gott», название которого переводится примерно так: «Могучая крепость и есть наш бог».<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Фотографии молодого <a href=\"/name/6750/\" class=\"all\">Моргана Фримана</a> на&#160;документах на&#160;самом деле являются фотографиями его сына <a href=\"/name/6767/\" class=\"all\">Альфонсо Фримана</a>, который также снялся в&#160;одном из&#160;эпизодов фильма.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Несмотря на&#160;то, что в&#160;кинотеатрах фильм не&#160;собрал больших денег, он&#160;стал одним из&#160;самых кассовых релизов на&#160;видео, а&#160;впоследствии и&#160;на&#160;DVD.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Фильм посвящён Аллену Грину (Allen Greene) — близкому другу режиссёра. Аллен скончался незадолго до выхода фильма из-за осложнений СПИДа.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Бывший начальник Мэнсфилдской тюрьмы, где проходили натурные съёмки фильма, <a href=\"/name/1104241/\" class=\"all\">Дэннис Бэйкер</a> снялся в роли пожилого заключённого, сидящего в тюремном автобусе позади Томми Уильямса.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Сценарий <a href=\"/name/24262/\" class=\"all\">Фрэнка Дарабонта</a> очень понравился другому режиссёру, успешно экранизировавшему произведения <a href=\"/name/24263/\" class=\"all\">Стивена Кинга</a>, — <a href=\"/name/5899/\" class=\"all\">Робу Райнеру</a>, постановщику &#171;<a href=\"/film/498/\" class=\"all\">Останься со мной</a>&#187; (1986) и &#171;<a href=\"/film/1574/\" class=\"all\">Мизери</a>&#187; (1990). Райнер был так захвачен материалом, что предложил Дарабонту $2,5 млн за права на сценарий и постановку фильма. Дарабонт серьёзно обдумал предложение, но в конечном счёте решил, что для него этот проект — &#171;шанс сделать что-то действительно великое&#187;, и поставил фильм сам.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* <a href=\"/name/5899/\" class=\"all\">Роб Райнер</a> видел в ролях Реда и Энди Дюфрейна соответственно <a href=\"/name/5679/\" class=\"all\">Харрисона Форда</a> и <a href=\"/name/20302/\" class=\"all\">Тома Круза</a>.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Роль Энди Дюфрейна изначально предложили <a href=\"/name/9144/\" class=\"all\">Тому Хэнксу</a>. Он очень заинтересовался, но не смог принять предложение, из-за того что уже был занят в проекте &#171;<a href=\"/film/448/\" class=\"all\">Форрест Гамп</a>&#187; (1994). Впоследствии Том Хэнкс снялся в главной роли в тюремной драме <a href=\"/name/24262/\" class=\"all\">Фрэнка Дарабонта</a> &#171;<a href=\"/film/435/\" class=\"all\">Зеленая миля</a>&#187; (1999), также поставленной по роману <a href=\"/name/24263/\" class=\"all\">Стивена Кинга</a>.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Роль Энди Дюфрейна также предлагали <a href=\"/name/24087/\" class=\"all\">Кевину Костнеру</a>, но актёр отказался от предложения, о чем впоследствии сильно жалел.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* В оригинальной повести <a href=\"/name/24263/\" class=\"all\">Стивена Кинга</a> Ред — ирландец. Несмотря на то, что в экранизации роль Реда сыграл чернокожий <a href=\"/name/6750/\" class=\"all\">Морган Фриман</a>, было решено оставить в фильме реплику Реда «Может быть, потому что я — ирландец», — как удачную шутку.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Несмотря на то что почти все жители города Мэнсфилда изъявили желание принять участие в съёмках массовых сцен фильма, большинство жителей оказались слишком заняты своей работой и не смогли сниматься. Массовку пришлось набирать в местной богадельне, причём некоторые из её обитателей были бывшими заключенными.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Когда на экране показываются крупные планы рук Энди Дюфрейна, когда в начале фильма он заряжает револьвер и когда Энди вырезает своё имя на стене камеры, — это на самом деле руки режиссёра <a href=\"/name/24262/\" class=\"all\">Фрэнка Дарабонта</a>. Эти кадры были сняты в процессе постпроизводства фильма.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Двое из заключённых Шоушенка носят имена Хейвуд и Флойд. Это отсылка к трилогии <a href=\"/name/47956/\" class=\"all\">Артура Ч. Кларка</a> «<a href=\"/film/380/\" class=\"all\">Космическая одиссея</a>», связующим героем которой является доктор Хейвуд Флойд.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Тюремный номер Энди Дюфрейна — 37927.<br/>", movie.Overview);
        Assert.Equal(1994, movie.ProductionYear);
        Assert.True(4 <= movie.RemoteTrailers.Length);
        Assert.Equal(142, movie.Size);
        Assert.Equal(movie.Name, movie.SortName);
        _ = Assert.Single(movie.Studios);
        Assert.Equal("Страх - это кандалы. Надежда - это свобода", movie.Tagline);

        Assert.Equal(80, result.People.Count);
        PersonInfo? person = result.People.FirstOrDefault(p => "Тим Роббинс".Equals(p.Name, StringComparison.Ordinal));
        Assert.NotNull(person);
        Assert.Equal("7987", person.GetProviderId(Plugin.PluginKey));
        Assert.Equal("Andy Dufresne", person.Role);
        Assert.Equal("Тим Роббинс", person.Name);
        Assert.True(!string.IsNullOrWhiteSpace(person.ImageUrl));

        _logManager.Verify(lm => lm.GetLogger(It.IsAny<string>()), Times.Exactly(4));
        _applicationPaths.VerifyGet(ap => ap.PluginConfigurationsPath, Times.Once());
        _xmlSerializer.Verify(xs => xs.DeserializeFromFile(typeof(PluginConfiguration), "KpMovieProvider_GetMetadata_NameAndYear/EmbyKinopoiskRu.xml"), Times.Once());
        _localizationManager.Verify(lm => lm.RemoveDiacritics("Побег из Шоушенка"), Times.Once());
        VerifyNoOtherCalls();

        Logger.Info($"Finished '{nameof(KpMovieProvider_GetMetadata_NameAndYear)}'");
    }

    [Fact]
    public async void KpMovieProvider_GetSearchResults_Provider_Kp()
    {
        Logger.Info($"Start '{nameof(KpMovieProvider_GetSearchResults_Provider_Kp)}'");

        _ = _applicationPaths
            .SetupGet(m => m.PluginConfigurationsPath)
            .Returns("KpMovieProvider_GetSearchResults_Provider_Kp");

        var movieInfo = new MovieInfo()
        {
            ProviderIds = new(new() { { Plugin.PluginKey, "326" } })
        };
        using var cancellationTokenSource = new CancellationTokenSource();
        IEnumerable<RemoteSearchResult> result = await _kpMovieProvider.GetSearchResults(movieInfo, cancellationTokenSource.Token);

        RemoteSearchResult searchResult = Assert.Single(result);
        Assert.NotNull(searchResult);
        Assert.Equal("326", searchResult.GetProviderId(Plugin.PluginKey));
        Assert.Equal("tt0111161", searchResult.GetProviderId(MetadataProviders.Imdb));
        Assert.Equal("278", searchResult.GetProviderId(MetadataProviders.Tmdb));
        Assert.Equal("Побег из Шоушенка", searchResult.Name);
        Assert.True(!string.IsNullOrWhiteSpace(searchResult.ImageUrl));
        Assert.Equal(1994, searchResult.ProductionYear);
        Assert.Equal(Plugin.PluginKey, searchResult.SearchProviderName);
        Assert.Equal("Бухгалтер Энди Дюфрейн обвинён в убийстве собственной жены и её любовника. Оказавшись в тюрьме под названием Шоушенк, он сталкивается с жестокостью и беззаконием, царящими по обе стороны решётки. Каждый, кто попадает в эти стены, становится их рабом до конца жизни. Но Энди, обладающий живым умом и доброй душой, находит подход как к заключённым, так и к охранникам, добиваясь их особого к себе расположения.<br/><br/><b>Интересное:</b><br/>&nbsp;&nbsp;&nbsp;&nbsp;* Фильм снят по мотивам повести <a href=\"/name/24263/\" class=\"all\">Стивена Кинга</a> «Рита Хейуорт и спасение из Шоушенка» (Rita Hayworth and Shawshank Redemption), опубликованной в составе сборника «Четыре сезона» (Different Seasons, 1982).<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Съемки проходили в&#160;Мэнсфилдской исправительной колонии в&#160;штате Огайо. Тюрьма находилась в&#160;таком плачевном состоянии, что пришлось приводить её&#160;в&#160;должный вид.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Оригинальная повесть <a href=\"/name/24263/\" class=\"all\">Стивена Кинга</a> была, по словам самого писателя, кульминацией всех его впечатлений от различных тюремных фильмов, которые он смотрел в детстве.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* <a href=\"/name/24263/\" class=\"all\">Стивен Кинг</a> согласился продать права на&#160;свое произведение практически даром, так как с&#160;<a href=\"/name/24262/\" class=\"all\">Фрэнком</a> их&#160;связывает давняя крепкая дружба. Произошло это после того, как Фрэнк довольно успешно экранизировал рассказ Кинга &#171;<a href=\"/film/7429/\" class=\"all\">Женщина в палате</a>&#187;.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Американское общество защиты животных выступило с&#160;критикой в&#160;адрес фильма, в&#160;котором единственным представителем фауны стал ворон старика Брукса. В&#160;картине есть сцена кормления птицы червяком, найденном во&#160;время обеда в&#160;тарелке главного героя фильма. Общество настояло на&#160;том, чтобы была использована уже мертвая личинка, погибшая естественной смертью. После того как такая особь была найдена, сцену отсняли.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Роль Томми Уильямса изначально была написана под <a href=\"/name/25584/\" class=\"all\">Брэда Питта</a>.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Картина, которую смотрят заключенные, — &#171;<a href=\"/film/8299/\" class=\"all\">Гильда</a>&#187;.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Начальник тюрьмы Нортон насвистывает гимн «Eine feste Burg ist unser Gott», название которого переводится примерно так: «Могучая крепость и есть наш бог».<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Фотографии молодого <a href=\"/name/6750/\" class=\"all\">Моргана Фримана</a> на&#160;документах на&#160;самом деле являются фотографиями его сына <a href=\"/name/6767/\" class=\"all\">Альфонсо Фримана</a>, который также снялся в&#160;одном из&#160;эпизодов фильма.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Несмотря на&#160;то, что в&#160;кинотеатрах фильм не&#160;собрал больших денег, он&#160;стал одним из&#160;самых кассовых релизов на&#160;видео, а&#160;впоследствии и&#160;на&#160;DVD.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Фильм посвящён Аллену Грину (Allen Greene) — близкому другу режиссёра. Аллен скончался незадолго до выхода фильма из-за осложнений СПИДа.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Бывший начальник Мэнсфилдской тюрьмы, где проходили натурные съёмки фильма, <a href=\"/name/1104241/\" class=\"all\">Дэннис Бэйкер</a> снялся в роли пожилого заключённого, сидящего в тюремном автобусе позади Томми Уильямса.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Сценарий <a href=\"/name/24262/\" class=\"all\">Фрэнка Дарабонта</a> очень понравился другому режиссёру, успешно экранизировавшему произведения <a href=\"/name/24263/\" class=\"all\">Стивена Кинга</a>, — <a href=\"/name/5899/\" class=\"all\">Робу Райнеру</a>, постановщику &#171;<a href=\"/film/498/\" class=\"all\">Останься со мной</a>&#187; (1986) и &#171;<a href=\"/film/1574/\" class=\"all\">Мизери</a>&#187; (1990). Райнер был так захвачен материалом, что предложил Дарабонту $2,5 млн за права на сценарий и постановку фильма. Дарабонт серьёзно обдумал предложение, но в конечном счёте решил, что для него этот проект — &#171;шанс сделать что-то действительно великое&#187;, и поставил фильм сам.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* <a href=\"/name/5899/\" class=\"all\">Роб Райнер</a> видел в ролях Реда и Энди Дюфрейна соответственно <a href=\"/name/5679/\" class=\"all\">Харрисона Форда</a> и <a href=\"/name/20302/\" class=\"all\">Тома Круза</a>.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Роль Энди Дюфрейна изначально предложили <a href=\"/name/9144/\" class=\"all\">Тому Хэнксу</a>. Он очень заинтересовался, но не смог принять предложение, из-за того что уже был занят в проекте &#171;<a href=\"/film/448/\" class=\"all\">Форрест Гамп</a>&#187; (1994). Впоследствии Том Хэнкс снялся в главной роли в тюремной драме <a href=\"/name/24262/\" class=\"all\">Фрэнка Дарабонта</a> &#171;<a href=\"/film/435/\" class=\"all\">Зеленая миля</a>&#187; (1999), также поставленной по роману <a href=\"/name/24263/\" class=\"all\">Стивена Кинга</a>.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Роль Энди Дюфрейна также предлагали <a href=\"/name/24087/\" class=\"all\">Кевину Костнеру</a>, но актёр отказался от предложения, о чем впоследствии сильно жалел.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* В оригинальной повести <a href=\"/name/24263/\" class=\"all\">Стивена Кинга</a> Ред — ирландец. Несмотря на то, что в экранизации роль Реда сыграл чернокожий <a href=\"/name/6750/\" class=\"all\">Морган Фриман</a>, было решено оставить в фильме реплику Реда «Может быть, потому что я — ирландец», — как удачную шутку.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Несмотря на то что почти все жители города Мэнсфилда изъявили желание принять участие в съёмках массовых сцен фильма, большинство жителей оказались слишком заняты своей работой и не смогли сниматься. Массовку пришлось набирать в местной богадельне, причём некоторые из её обитателей были бывшими заключенными.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Когда на экране показываются крупные планы рук Энди Дюфрейна, когда в начале фильма он заряжает револьвер и когда Энди вырезает своё имя на стене камеры, — это на самом деле руки режиссёра <a href=\"/name/24262/\" class=\"all\">Фрэнка Дарабонта</a>. Эти кадры были сняты в процессе постпроизводства фильма.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Двое из заключённых Шоушенка носят имена Хейвуд и Флойд. Это отсылка к трилогии <a href=\"/name/47956/\" class=\"all\">Артура Ч. Кларка</a> «<a href=\"/film/380/\" class=\"all\">Космическая одиссея</a>», связующим героем которой является доктор Хейвуд Флойд.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Тюремный номер Энди Дюфрейна — 37927.<br/>", searchResult.Overview);

        _logManager.Verify(lm => lm.GetLogger(It.IsAny<string>()), Times.Exactly(4));
        _applicationPaths.VerifyGet(ap => ap.PluginConfigurationsPath, Times.Once());
        _xmlSerializer.Verify(xs => xs.DeserializeFromFile(typeof(PluginConfiguration), "KpMovieProvider_GetSearchResults_Provider_Kp/EmbyKinopoiskRu.xml"), Times.Once());
        VerifyNoOtherCalls();

        Logger.Info($"Finished '{nameof(KpMovieProvider_GetSearchResults_Provider_Kp)}'");
    }

    [Fact]
    public async void KpMovieProvider_GetSearchResults_NameAndYear()
    {
        Logger.Info($"Start '{nameof(KpMovieProvider_GetSearchResults_NameAndYear)}'");

        _ = _applicationPaths
            .SetupGet(m => m.PluginConfigurationsPath)
            .Returns("KpMovieProvider_GetSearchResults_NameAndYear");

        var movieInfo = new MovieInfo()
        {
            Name = "Побег из Шоушенка",
            Year = 1994
        };
        using var cancellationTokenSource = new CancellationTokenSource();
        IEnumerable<RemoteSearchResult> result = await _kpMovieProvider.GetSearchResults(movieInfo, cancellationTokenSource.Token);

        RemoteSearchResult searchResult = Assert.Single(result);
        Assert.NotNull(searchResult);
        Assert.Equal("326", searchResult.GetProviderId(Plugin.PluginKey));
        Assert.Equal("tt0111161", searchResult.GetProviderId(MetadataProviders.Imdb));
        Assert.Equal("278", searchResult.GetProviderId(MetadataProviders.Tmdb));
        Assert.Equal("Побег из Шоушенка", searchResult.Name);
        Assert.True(!string.IsNullOrWhiteSpace(searchResult.ImageUrl));
        Assert.Equal(1994, searchResult.ProductionYear);
        Assert.Equal(Plugin.PluginKey, searchResult.SearchProviderName);
        Assert.Equal("Бухгалтер Энди Дюфрейн обвинён в убийстве собственной жены и её любовника. Оказавшись в тюрьме под названием Шоушенк, он сталкивается с жестокостью и беззаконием, царящими по обе стороны решётки. Каждый, кто попадает в эти стены, становится их рабом до конца жизни. Но Энди, обладающий живым умом и доброй душой, находит подход как к заключённым, так и к охранникам, добиваясь их особого к себе расположения.<br/><br/><b>Интересное:</b><br/>&nbsp;&nbsp;&nbsp;&nbsp;* Фильм снят по мотивам повести <a href=\"/name/24263/\" class=\"all\">Стивена Кинга</a> «Рита Хейуорт и спасение из Шоушенка» (Rita Hayworth and Shawshank Redemption), опубликованной в составе сборника «Четыре сезона» (Different Seasons, 1982).<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Съемки проходили в&#160;Мэнсфилдской исправительной колонии в&#160;штате Огайо. Тюрьма находилась в&#160;таком плачевном состоянии, что пришлось приводить её&#160;в&#160;должный вид.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Оригинальная повесть <a href=\"/name/24263/\" class=\"all\">Стивена Кинга</a> была, по словам самого писателя, кульминацией всех его впечатлений от различных тюремных фильмов, которые он смотрел в детстве.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* <a href=\"/name/24263/\" class=\"all\">Стивен Кинг</a> согласился продать права на&#160;свое произведение практически даром, так как с&#160;<a href=\"/name/24262/\" class=\"all\">Фрэнком</a> их&#160;связывает давняя крепкая дружба. Произошло это после того, как Фрэнк довольно успешно экранизировал рассказ Кинга &#171;<a href=\"/film/7429/\" class=\"all\">Женщина в палате</a>&#187;.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Американское общество защиты животных выступило с&#160;критикой в&#160;адрес фильма, в&#160;котором единственным представителем фауны стал ворон старика Брукса. В&#160;картине есть сцена кормления птицы червяком, найденном во&#160;время обеда в&#160;тарелке главного героя фильма. Общество настояло на&#160;том, чтобы была использована уже мертвая личинка, погибшая естественной смертью. После того как такая особь была найдена, сцену отсняли.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Роль Томми Уильямса изначально была написана под <a href=\"/name/25584/\" class=\"all\">Брэда Питта</a>.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Картина, которую смотрят заключенные, — &#171;<a href=\"/film/8299/\" class=\"all\">Гильда</a>&#187;.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Начальник тюрьмы Нортон насвистывает гимн «Eine feste Burg ist unser Gott», название которого переводится примерно так: «Могучая крепость и есть наш бог».<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Фотографии молодого <a href=\"/name/6750/\" class=\"all\">Моргана Фримана</a> на&#160;документах на&#160;самом деле являются фотографиями его сына <a href=\"/name/6767/\" class=\"all\">Альфонсо Фримана</a>, который также снялся в&#160;одном из&#160;эпизодов фильма.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Несмотря на&#160;то, что в&#160;кинотеатрах фильм не&#160;собрал больших денег, он&#160;стал одним из&#160;самых кассовых релизов на&#160;видео, а&#160;впоследствии и&#160;на&#160;DVD.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Фильм посвящён Аллену Грину (Allen Greene) — близкому другу режиссёра. Аллен скончался незадолго до выхода фильма из-за осложнений СПИДа.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Бывший начальник Мэнсфилдской тюрьмы, где проходили натурные съёмки фильма, <a href=\"/name/1104241/\" class=\"all\">Дэннис Бэйкер</a> снялся в роли пожилого заключённого, сидящего в тюремном автобусе позади Томми Уильямса.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Сценарий <a href=\"/name/24262/\" class=\"all\">Фрэнка Дарабонта</a> очень понравился другому режиссёру, успешно экранизировавшему произведения <a href=\"/name/24263/\" class=\"all\">Стивена Кинга</a>, — <a href=\"/name/5899/\" class=\"all\">Робу Райнеру</a>, постановщику &#171;<a href=\"/film/498/\" class=\"all\">Останься со мной</a>&#187; (1986) и &#171;<a href=\"/film/1574/\" class=\"all\">Мизери</a>&#187; (1990). Райнер был так захвачен материалом, что предложил Дарабонту $2,5 млн за права на сценарий и постановку фильма. Дарабонт серьёзно обдумал предложение, но в конечном счёте решил, что для него этот проект — &#171;шанс сделать что-то действительно великое&#187;, и поставил фильм сам.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* <a href=\"/name/5899/\" class=\"all\">Роб Райнер</a> видел в ролях Реда и Энди Дюфрейна соответственно <a href=\"/name/5679/\" class=\"all\">Харрисона Форда</a> и <a href=\"/name/20302/\" class=\"all\">Тома Круза</a>.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Роль Энди Дюфрейна изначально предложили <a href=\"/name/9144/\" class=\"all\">Тому Хэнксу</a>. Он очень заинтересовался, но не смог принять предложение, из-за того что уже был занят в проекте &#171;<a href=\"/film/448/\" class=\"all\">Форрест Гамп</a>&#187; (1994). Впоследствии Том Хэнкс снялся в главной роли в тюремной драме <a href=\"/name/24262/\" class=\"all\">Фрэнка Дарабонта</a> &#171;<a href=\"/film/435/\" class=\"all\">Зеленая миля</a>&#187; (1999), также поставленной по роману <a href=\"/name/24263/\" class=\"all\">Стивена Кинга</a>.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Роль Энди Дюфрейна также предлагали <a href=\"/name/24087/\" class=\"all\">Кевину Костнеру</a>, но актёр отказался от предложения, о чем впоследствии сильно жалел.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* В оригинальной повести <a href=\"/name/24263/\" class=\"all\">Стивена Кинга</a> Ред — ирландец. Несмотря на то, что в экранизации роль Реда сыграл чернокожий <a href=\"/name/6750/\" class=\"all\">Морган Фриман</a>, было решено оставить в фильме реплику Реда «Может быть, потому что я — ирландец», — как удачную шутку.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Несмотря на то что почти все жители города Мэнсфилда изъявили желание принять участие в съёмках массовых сцен фильма, большинство жителей оказались слишком заняты своей работой и не смогли сниматься. Массовку пришлось набирать в местной богадельне, причём некоторые из её обитателей были бывшими заключенными.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Когда на экране показываются крупные планы рук Энди Дюфрейна, когда в начале фильма он заряжает револьвер и когда Энди вырезает своё имя на стене камеры, — это на самом деле руки режиссёра <a href=\"/name/24262/\" class=\"all\">Фрэнка Дарабонта</a>. Эти кадры были сняты в процессе постпроизводства фильма.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Двое из заключённых Шоушенка носят имена Хейвуд и Флойд. Это отсылка к трилогии <a href=\"/name/47956/\" class=\"all\">Артура Ч. Кларка</a> «<a href=\"/film/380/\" class=\"all\">Космическая одиссея</a>», связующим героем которой является доктор Хейвуд Флойд.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Тюремный номер Энди Дюфрейна — 37927.<br/>", searchResult.Overview);

        _logManager.Verify(lm => lm.GetLogger(It.IsAny<string>()), Times.Exactly(4));
        _applicationPaths.VerifyGet(ap => ap.PluginConfigurationsPath, Times.Once());
        _xmlSerializer.Verify(xs => xs.DeserializeFromFile(typeof(PluginConfiguration), "KpMovieProvider_GetSearchResults_NameAndYear/EmbyKinopoiskRu.xml"), Times.Once());
        VerifyNoOtherCalls();

        Logger.Info($"Finished '{nameof(KpMovieProvider_GetSearchResults_NameAndYear)}'");
    }

}