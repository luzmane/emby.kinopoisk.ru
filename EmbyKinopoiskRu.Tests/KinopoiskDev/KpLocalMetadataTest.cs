using System.Globalization;

using EmbyKinopoiskRu.Configuration;
using EmbyKinopoiskRu.Provider.LocalMetadata;

using MediaBrowser.Controller.Collections;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Querying;

namespace EmbyKinopoiskRu.Tests.KinopoiskDev;

[Collection("Sequential")]
public class KpLocalMetadataTest : BaseTest
{
    private static readonly NLog.ILogger Logger = NLog.LogManager.GetLogger(nameof(KpLocalMetadataTest));

    private readonly KpMovieLocalMetadata _kpMovieLocalMetadata;
    private readonly LibraryOptions _movieLibraryOptions;
    private readonly long[] _internalIdPotterSequence = new long[] { 101, 102, 103, 104, 105, 106, 107, 108 };


    #region Test configs
    public KpLocalMetadataTest() : base(Logger)
    {
        _pluginConfiguration.Token = GetKinopoiskDevToken();

        ConfigLibraryManager();

        ConfigXmlSerializer();

        _movieLibraryOptions = new LibraryOptions()
        {
            ContentType = "movies",
            EnableAdultMetadata = true,
            ImportCollections = true,
            MetadataCountryCode = "RU",
            MinCollectionItems = 1,
            Name = "Movies",
            PathInfos = new MediaPathInfo[]{
                new MediaPathInfo()
                {
                    NetworkPath = null,
                    Path = "/emby/movie_library"
                }
            },
            PreferredImageLanguage = "ru",
            PreferredMetadataLanguage = "ru",
            SkipSubtitlesIfEmbeddedSubtitlesPresent = true,
            SkipSubtitlesIfAudioTrackMatches = true,
            TypeOptions = new TypeOptions[]{
                new TypeOptions()
                {
                    Type = "Movie"
                }
            }
        };

        _kpMovieLocalMetadata = new KpMovieLocalMetadata(_logManager.Object);
    }
    protected override void ConfigLibraryManager()
    {
        base.ConfigLibraryManager();

        var potterSequences = new long[] { 688, 322, 8408, 48356, 89515, 276762, 407636, 4716622 }
            .Select(id => new KeyValuePair<string, string>(Plugin.PluginKey, id.ToString(CultureInfo.InvariantCulture)))
            .ToList();
        var imdbPotterSequences = new string[] { "tt1201607", "tt0330373", "tt0373889", "tt0417741", "tt0926084", "tt16116174" }
            .Select(id => new KeyValuePair<string, string>(MetadataProviders.Imdb.ToString(), id.ToString(CultureInfo.InvariantCulture)))
            .ToList();
        var tmdbPotterSequences = new string[] { "12445", "674", "675", "767", "12444", "899082" }
            .Select(id => new KeyValuePair<string, string>(MetadataProviders.Tmdb.ToString(), id.ToString(CultureInfo.InvariantCulture)))
            .ToList();

        _ = _libraryManager // EmbyHelper.GetSequenceInternalIds(). Items in lib with Kp internal Id
            .Setup(m => m.QueryItems(It.Is<InternalItemsQuery>(query =>
                query.Recursive == false
                && query.IsVirtualItem == false
                && query.IncludeItemTypes.Length == 2
                && nameof(Movie).Equals(query.IncludeItemTypes[0], StringComparison.Ordinal)
                && nameof(Series).Equals(query.IncludeItemTypes[1], StringComparison.Ordinal)
                && query.AnyProviderIdEquals.Count == potterSequences.Count
                && query.AnyProviderIdEquals.All(item => potterSequences.Contains(item)))))
            .Returns(new QueryResult<BaseItem>()
            {
                Items = new BaseItem[] {
                    new Movie() {
                        Name = "Гарри Поттер и Тайная комната",
                        InternalId = 101L,
                        ProviderIds = new(new Dictionary<string, string>()
                        {
                            { Plugin.PluginKey, "688" },
                            { MetadataProviders.Imdb.ToString(), "tt0295297" },
                            { MetadataProviders.Tmdb.ToString(), "672" }
                        })
                    },
                    new Movie() {
                        Name = "Гарри Поттер и узник Азкабана",
                        InternalId = 102L,
                        ProviderIds = new(new Dictionary<string, string>()
                        {
                            { Plugin.PluginKey, "322" },
                            { MetadataProviders.Imdb.ToString(), "tt0304141" },
                            { MetadataProviders.Tmdb.ToString(), "673" }
                        }),
                    },
                }
            });

        _ = _libraryManager // EmbyHelper.GetSequenceInternalIds(). Items in lib with IMDB internal Id
            .Setup(m => m.QueryItems(It.Is<InternalItemsQuery>(query =>
                query.Recursive == false
                && query.IsVirtualItem == false
                && query.IncludeItemTypes.Length == 2
                && nameof(Movie).Equals(query.IncludeItemTypes[0], StringComparison.Ordinal)
                && nameof(Series).Equals(query.IncludeItemTypes[1], StringComparison.Ordinal)
                && query.AnyProviderIdEquals.Count == imdbPotterSequences.Count
                && query.AnyProviderIdEquals.All(item => imdbPotterSequences.Contains(item))
            )))
            .Returns(new QueryResult<BaseItem>()
            {
                Items = new BaseItem[] {
                    new Movie() {
                        Name = "Гарри Поттер и Кубок огня",
                        InternalId = 103L,
                        ProviderIds = new(new Dictionary<string, string>()
                        {
                            { Plugin.PluginKey, "8408" },
                            { MetadataProviders.Imdb.ToString(), "tt0330373" },
                            { MetadataProviders.Tmdb.ToString(), "674" }
                        })
                    },
                    new Movie() {
                        Name = "Гарри Поттер и Орден Феникса",
                        InternalId = 104L,
                        ProviderIds = new(new Dictionary<string, string>()
                        {
                            { Plugin.PluginKey, "48356" },
                            { MetadataProviders.Tmdb.ToString(), "675" },
                            { MetadataProviders.Imdb.ToString(), "tt0373889" }
                        })
                    },
                    new Movie() {
                        Name = "Гарри Поттер и Принц-полукровка",
                        InternalId = 105L,
                        ProviderIds = new(new Dictionary<string, string>()
                        {
                            { Plugin.PluginKey, "89515" },
                            { MetadataProviders.Tmdb.ToString(), "767" },
                            { MetadataProviders.Imdb.ToString(), "tt0417741" }
                        })
                    },
                    new Movie() {
                        Name = "Гарри Поттер и Дары Смерти: Часть I",
                        InternalId = 106L,
                        ProviderIds = new(new Dictionary<string, string>()
                        {
                            { Plugin.PluginKey, "276762" },
                            { MetadataProviders.Tmdb.ToString(), "12444" },
                            { MetadataProviders.Imdb.ToString(), "tt0926084" }
                        }),
                    },
                }
            });

        _ = _libraryManager // EmbyHelper.GetSequenceInternalIds(). Items in lib with TMDB internal Id
            .Setup(m => m.QueryItems(It.Is<InternalItemsQuery>(query =>
                query.Recursive == false
                && query.IsVirtualItem == false
                && query.IncludeItemTypes.Length == 2
                && nameof(Movie).Equals(query.IncludeItemTypes[0], StringComparison.Ordinal)
                && nameof(Series).Equals(query.IncludeItemTypes[1], StringComparison.Ordinal)
                && query.AnyProviderIdEquals.Count == tmdbPotterSequences.Count
                && query.AnyProviderIdEquals.All(item => tmdbPotterSequences.Contains(item))
            )))
            .Returns(new QueryResult<BaseItem>()
            {
                Items = new BaseItem[] {
                    new Movie() {
                        Name = "Гарри Поттер и Дары Смерти: Часть II",
                        InternalId = 107L,
                        ProviderIds = new(new Dictionary<string, string>()
                        {
                            { Plugin.PluginKey, "407636" },
                            { MetadataProviders.Imdb.ToString(), "tt1201607" },
                            { MetadataProviders.Tmdb.ToString(), "12445" }
                        })
                    },
                    new Movie() {
                        Name = "Гарри Поттер 20 лет спустя: Возвращение в Хогвартс",
                        InternalId = 108L,
                        ProviderIds = new(new Dictionary<string, string>()
                        {
                            { Plugin.PluginKey, "4716622" },
                            { MetadataProviders.Tmdb.ToString(), "899082" },
                            { MetadataProviders.Imdb.ToString(), "tt16116174" }
                        })
                    },
                }
            });

    }
    protected override void ConfigXmlSerializer()
    {
        base.ConfigXmlSerializer();

    }

    #endregion

    [Fact]
    public async void KpLocalMetadata_WithKpInName()
    {
        Logger.Info($"Start '{nameof(KpLocalMetadata_WithKpInName)}'");

        var itemInfo = new ItemInfo(new Movie()
        {
            Path = "/emby/movie_library/kp326_Побег из Шоушенка.mkv",
            Container = "mkv",
            IsInMixedFolder = false,
            Id = new Guid(),
            Name = "Побег из Шоушенка"
        });

        using var cancellationTokenSource = new CancellationTokenSource();
        MetadataResult<Movie> result = await _kpMovieLocalMetadata.GetMetadata(itemInfo, _movieLibraryOptions, _directoryService.Object, cancellationTokenSource.Token);

        Assert.NotNull(result.Item);
        Assert.True(result.HasMetadata);
        Assert.Equal("326", result.Item.GetProviderId(Plugin.PluginKey));
        Assert.Equal("Video", result.Item.MediaType);

        _logManager.Verify(lm => lm.GetLogger("KpMovieLocalMetadata"), Times.Exactly(2));
        _logManager.Verify(lm => lm.GetLogger("KinopoiskRu"), Times.Exactly(1));
        _fileSystem.Verify(fs => fs.GetDirectoryName(It.IsAny<string>()), Times.Once());
        VerifyNoOtherCalls();

        Logger.Info($"Finished '{nameof(KpLocalMetadata_WithKpInName)}'");
    }

    [Fact]
    public async void KpLocalMetadata_WithNameOnly_SingleResult()
    {
        Logger.Info($"Start '{nameof(KpLocalMetadata_WithNameOnly_SingleResult)}'");

        _ = _applicationPaths
            .SetupGet(m => m.PluginConfigurationsPath)
            .Returns("KpLocalMetadata_WithNameOnly_SingleResult");

        var itemInfo = new ItemInfo(new Movie()
        {
            Path = "/emby/movie_library/Побег из Шоушенка.mkv",
            Container = "mkv",
            IsInMixedFolder = false,
            Id = new Guid(),
            Name = "Побег из Шоушенка"
        });
        using var cancellationTokenSource = new CancellationTokenSource();
        MetadataResult<Movie> result = await _kpMovieLocalMetadata.GetMetadata(itemInfo, _movieLibraryOptions, _directoryService.Object, cancellationTokenSource.Token);

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

        _logManager.Verify(lm => lm.GetLogger(It.IsAny<string>()), Times.Exactly(5));
        _fileSystem.Verify(fs => fs.GetDirectoryName("/emby/movie_library/Побег из Шоушенка.mkv"), Times.Once());
        _applicationPaths.VerifyGet(ap => ap.PluginConfigurationsPath, Times.Once());
        _xmlSerializer.Verify(xs => xs.DeserializeFromFile(typeof(PluginConfiguration), "KpLocalMetadata_WithNameOnly_SingleResult/EmbyKinopoiskRu.xml"), Times.Once());
        _localizationManager.Verify(lm => lm.RemoveDiacritics("Побег из Шоушенка"), Times.Once());
        VerifyNoOtherCalls();

        Logger.Info($"Finished '{nameof(KpLocalMetadata_WithNameOnly_SingleResult)}'");
    }

    [Fact]
    public async void KpLocalMetadata_WithNameOnly_MultiResult()
    {
        Logger.Info($"Start '{nameof(KpLocalMetadata_WithNameOnly_MultiResult)}'");

        _ = _applicationPaths
            .SetupGet(m => m.PluginConfigurationsPath)
            .Returns("KpLocalMetadata_WithNameOnly_MultiResult");

        var itemInfo = new ItemInfo(new Movie()
        {
            Path = "/emby/movie_library/Робин Гуд.mkv",
            Container = "mkv",
            IsInMixedFolder = false,
            Id = new Guid(),
            Name = "Робин Гуд"
        });
        using var cancellationTokenSource = new CancellationTokenSource();
        MetadataResult<Movie> result = await _kpMovieLocalMetadata.GetMetadata(itemInfo, _movieLibraryOptions, _directoryService.Object, cancellationTokenSource.Token);

        Movie movie = result.Item;
        Assert.NotNull(movie);
        Assert.True(result.HasMetadata);
        Assert.True(8 < movie.CommunityRating);
        Assert.Equal("17579", movie.ExternalId);
        Assert.Equal(6, movie.Genres.Length);
        Assert.Equal("Video", movie.MediaType);
        Assert.Equal("Робин Гуд", movie.Name);
        Assert.Equal("g", movie.OfficialRating);
        Assert.Equal("Robin Hood", movie.OriginalTitle);
        Assert.Equal("Добро пожаловать в дремучий Шервудский лес, где ты встретишь храброго и забавного лисенка по имени Робин Гуд и его лучшего друга Крошку Джона - большого добродушного медведя.\n\nЭту веселую компанию давно разыскивает шериф Нотингема. Он готовит друзьям ловушку: на турнире лучников Робин Гуда будет ждать засада. Но отважный лисенок все равно собирается участвовать в состязании: ведь только там у него есть шанс увидеть красавицу Мариан.\n\nИ вот турнир начался, однако шериф никак не может узнать среди участников переодетого Робин Гуда. Правда, один точный выстрел способен сразу выдать самого лучшего стрелка в королевстве.<br/><br/><b>Интересное:</b><br/>&nbsp;&nbsp;&nbsp;&nbsp;* При создании «Робина Гуда» был использовано около 350 000 рисунков, более 100 000 листов целлулоидной бумаги и 800 фонов для мультфильма.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Из-за маленького бюджета фильма, аниматорам студии Дисней пришлось повторно использовать анимационные последовательности как из самого фильма, так и заимствованные из предыдущих фильмов Диснея, включая &#171;<a href=\"/film/551/\" class=\"all\">Белоснежку и семь гномов</a>&#187;, &#171;<a href=\"/film/8133/\" class=\"all\">Книгу Джунглей</a>&#187;, &#171;<a href=\"/film/26656/\" class=\"all\">Котов-аристократов</a>&#187;, &#171;<a href=\"/film/8231/\" class=\"all\">Золушку</a>&#187; и &#171;<a href=\"/film/19504/\" class=\"all\">Алису в Стране чудес</a>&#187;.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Знаменитой расщелиной в зубах <a href=\"/name/176055/\" class=\"all\">Терри-Томаса</a> (1911-1990) воспользовались для показа персонажа, которого актёр озвучил, сэра Хисса. Именно через эту расщелину изо рта змея выскакивал раздвоенный язык.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Изначально отца Тука планировали сделать боровом, но всё-таки сделали барсуком, чтобы избежать обвинений в оскорблении верующих. Шерифа Ноттингема сначала планировали сделать козлом, но сделали волком, т.к. антагонисты традиционно ассоциируются именно с волками.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* За несколько месяцев до выхода фильма в прокат аниматорам потребовалось, чтобы <a href=\"/name/20665/\" class=\"all\">Питер Устинов</a> (1921-2004) переозвучил некоторые реплики принца Джона. В поисках актёра аниматоры обзвонили Нью-Йорк, Лондон, Париж, Вену и Токио, а обнаружился Устинов в студии в Бербанке в Калифорнии, примерно в паре километров от них.<br/>", movie.Overview);
        Assert.Equal(1973, movie.ProductionYear);
        Assert.Equal("17579", movie.GetProviderId(Plugin.PluginKey));
        Assert.Equal("tt0070608", movie.GetProviderId(MetadataProviders.Imdb));
        Assert.Equal("11886", movie.GetProviderId(MetadataProviders.Tmdb));
        Assert.True(6 <= movie.RemoteTrailers.Length);
        Assert.Equal(83, movie.Size);
        Assert.Equal(movie.Name, movie.SortName);
        Assert.Equal(2, movie.Studios.Length);
        Assert.Equal("The way it REALLY happened...", movie.Tagline);

        _logManager.Verify(lm => lm.GetLogger(It.IsAny<string>()), Times.Exactly(5));
        _fileSystem.Verify(fs => fs.GetDirectoryName("/emby/movie_library/Робин Гуд.mkv"), Times.Once());
        _applicationPaths.VerifyGet(ap => ap.PluginConfigurationsPath, Times.Once());
        _xmlSerializer.Verify(xs => xs.DeserializeFromFile(typeof(PluginConfiguration), "KpLocalMetadata_WithNameOnly_MultiResult/EmbyKinopoiskRu.xml"), Times.Once());
        _localizationManager.Verify(lm => lm.RemoveDiacritics(It.IsAny<string>()), Times.Exactly(10));
        VerifyNoOtherCalls();

        Logger.Info($"Finished '{nameof(KpLocalMetadata_WithNameOnly_MultiResult)}'");
    }

    [Fact]
    public async void KpLocalMetadata_WithNameYearAndAddToExistingCollection()
    {
        Logger.Info($"Start '{nameof(KpLocalMetadata_WithNameYearAndAddToExistingCollection)}'");

        _ = _libraryManager
            .Setup(m => m.GetInternalItemIds(It.Is<InternalItemsQuery>(q => Equals(true, q.IsFolder))))
            .Returns(new long[] { 1L });

        _ = _libraryManager // EmbyHelper.SearchExistingCollection(). Search boxset contains all sequence movies
            .Setup(m => m.QueryItems(It.Is<InternalItemsQuery>(query =>
                query.IncludeItemTypes.Length == 1
                && query.IncludeItemTypes.All(item => "boxset".Equals(item, StringComparison.Ordinal))
                && query.ListItemIds.Length == _internalIdPotterSequence.Length
                && query.ListItemIds.All(item => _internalIdPotterSequence.Contains(item))
            )))
            .Returns(new QueryResult<BaseItem>()
            {
                Items = new BaseItem[] {
                    new BoxSet() {
                        InternalId = 201L,
                        Name = "Гарри Поттер"
                    }
                }
            });

        _ = _libraryManager // EmbyHelper.SearchExistingCollection(). Search movies in boxset
            .Setup(m => m.QueryItems(It.Is<InternalItemsQuery>(query =>
                query.Recursive == false
                && query.IsVirtualItem == false
                && query.IncludeItemTypes.Length == 2
                && nameof(Movie).Equals(query.IncludeItemTypes[0], StringComparison.Ordinal)
                && nameof(Series).Equals(query.IncludeItemTypes[1], StringComparison.Ordinal)
                && query.CollectionIds.Length == 1
                && query.CollectionIds.All(item => item == 201L))))
            .Returns(new QueryResult<BaseItem>()
            {
                TotalRecordCount = 4,
                Items = new BaseItem[] {
                    new Movie() {
                        Name = "Гарри Поттер и Дары Смерти: Часть II",
                        InternalId = 107L,
                        ProviderIds = new(new Dictionary<string, string>()
                        {
                            { Plugin.PluginKey, "407636" },
                            { MetadataProviders.Imdb.ToString(), "tt1201607" },
                            { MetadataProviders.Tmdb.ToString(), "12445" }
                        })
                    },
                    new Movie() {
                        Name = "Гарри Поттер и Кубок огня",
                        InternalId = 103L,
                        ProviderIds = new(new Dictionary<string, string>()
                        {
                            { Plugin.PluginKey, "8408" },
                            { MetadataProviders.Imdb.ToString(), "tt0330373" },
                            { MetadataProviders.Tmdb.ToString(), "674" }
                        })
                    },
                    new Movie() {
                        Name = "Гарри Поттер и Орден Феникса",
                        InternalId = 104L,
                        ProviderIds = new(new Dictionary<string, string>()
                        {
                            { Plugin.PluginKey, "48356" },
                            { MetadataProviders.Tmdb.ToString(), "675" },
                            { MetadataProviders.Imdb.ToString(), "tt0373889" }
                        })
                    },
                    new Movie() {
                        Name = "Гарри Поттер и Дары Смерти: Часть I",
                        InternalId = 106L,
                        ProviderIds = new(new Dictionary<string, string>()
                        {
                            { Plugin.PluginKey, "276762" },
                            { MetadataProviders.Tmdb.ToString(), "12444" },
                            { MetadataProviders.Imdb.ToString(), "tt0926084" }
                        }),
                    },
                }
            });

        LibraryOptions boxsetLibraryOptions = new()
        {
            ContentType = CollectionType.BoxSets.ToString(),
            MetadataCountryCode = "RU",
            MinCollectionItems = 1,
            Name = "Collections",
            PathInfos = new MediaPathInfo[]{
                        new MediaPathInfo()
                        {
                            NetworkPath = null,
                            Path = "/emby/movie_library"
                        }
                    },
            PreferredImageLanguage = "ru",
            PreferredMetadataLanguage = "ru",
        };
        _ = _xmlSerializer
            .Setup(m => m.DeserializeFromFile(typeof(LibraryOptions), "KpLocalMetadata_WithNameYearAndAddToExistingCollection/options.xml"))
            .Returns(boxsetLibraryOptions);

        _ = _applicationPaths
            .SetupGet(m => m.PluginConfigurationsPath)
            .Returns("KpLocalMetadata_WithNameYearAndAddToExistingCollection");

        _ = _libraryManager
            .Setup(m => m.GetItemById(It.Is<long>(id => id == 1L)))
            .Returns(new CollectionFolder()
            {
                Name = "Collections",
                Path = "KpLocalMetadata_WithNameYearAndAddToExistingCollection"
            });

        var itemInfo = new ItemInfo(new Movie()
        {
            Path = "/emby/movie_library/Гарри Поттер и философский камень (2001).mkv",
            Container = "mkv",
            IsInMixedFolder = false,
            Id = new Guid(),
            Name = "Гарри Поттер и философский камень"
        });
        using var cancellationTokenSource = new CancellationTokenSource();
        _pluginConfiguration.CreateCollections = true;
        MetadataResult<Movie> result = await _kpMovieLocalMetadata.GetMetadata(itemInfo, _movieLibraryOptions, _directoryService.Object, cancellationTokenSource.Token);


        Movie movie = result.Item;
        Assert.NotNull(movie);
        Assert.True(result.HasMetadata);
        LinkedItemInfo collection = Assert.Single(movie.Collections);
        Assert.True(201L == collection.Id);
        Assert.True(8 < movie.CommunityRating);
        Assert.Equal("689", movie.ExternalId);
        Assert.Equal(3, movie.Genres.Length);
        Assert.Equal("Video", movie.MediaType);
        Assert.Equal("Гарри Поттер и философский камень", movie.Name);
        Assert.Equal("pg", movie.OfficialRating);
        Assert.Equal("Harry Potter and the Sorcerer's Stone", movie.OriginalTitle);
        Assert.Equal("Жизнь десятилетнего Гарри Поттера нельзя назвать сладкой: родители умерли, едва ему исполнился год, а от дяди и тёти, взявших сироту на воспитание, достаются лишь тычки да подзатыльники. Но в одиннадцатый день рождения Гарри всё меняется. Странный гость, неожиданно появившийся на пороге, приносит письмо, из которого мальчик узнаёт, что на самом деле он - волшебник и зачислен в школу магии под названием Хогвартс. А уже через пару недель Гарри будет мчаться в поезде Хогвартс-экспресс навстречу новой жизни, где его ждут невероятные приключения, верные друзья и самое главное — ключ к разгадке тайны смерти его родителей.<br/><br/><b>Интересное:</b><br/>&nbsp;&nbsp;&nbsp;&nbsp;* Фильм снят по мотивам романа <a href=\"/name/40777/\" class=\"all\">Дж.К. Роулинг</a> &#171;Гарри Поттер и философский камень&#187; (Harry Potter and the Philosopher's Stone, 1997).<br/>&nbsp;&nbsp;&nbsp;&nbsp;* <a href=\"/name/40777/\" class=\"all\">Дж. К. Роулинг</a> продала права на создание фильмов по первым четырем книгам приключений Гарри Поттера в 1999 году за скромную сумму в один миллион фунтов стерлингов (на тот момент чуть больше 1,5 млн. долларов). Что намного важнее, было оговорено, что писательница будет получать определённую часть от сборов каждого из фильмов, и будет иметь значительный контроль над всеми стадиями производства картин.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* <a href=\"/name/40777/\" class=\"all\">Дж. К. Роулинг</a> поставила непременное условие, что все актёры в будущих фильмах должны быть британцами. Лишь в четвёртом фильме франчайза, где это было необходимо по содержанию книги, появились актеры из других стран.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Первоначально главным претендентом на место режиссёра картины был <a href=\"/name/22260/\" class=\"all\">Стивен Спилберг</a>. Переговоры с ним велись в течение нескольких месяцев, и в феврале 2000 года закончились его отказом. Стивен Спилберг в качестве основного варианта рассматривал создание анимационной ленты, где голосом главного героя был бы <a href=\"/name/11381/\" class=\"all\">Хэйли Джоэл Осмент</a>. Этот вариант не понравился ни киностудии, ни автору книг. В последующем знаменитый режиссёр продолжал настаивать на участии американского актёра в главной роли. Другой причиной своего отказа он назвал отсутствие творческого интереса к проекту. По словам Стивена Спилберга фильм был обречён на колоссальный коммерческий успех, независимо от того, насколько удачной будет его работа.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* После отказа <a href=\"/name/22260/\" class=\"all\">Спилберга</a> начались переговоры сразу с несколькими режиссёрами. В качестве основных кандидатур рассматривались <a href=\"/name/24817/\" class=\"all\">Крис Коламбус</a>, <a href=\"/name/42918/\" class=\"all\">Терри Гиллиам</a>, <a href=\"/name/12659/\" class=\"all\">Джонатан Демме</a>, <a href=\"/name/20861/\" class=\"all\">Майк Ньюэлл</a>, <a href=\"/name/28208/\" class=\"all\">Алан Паркер</a>, <a href=\"/name/24027/\" class=\"all\">Вольфганг Петерсен</a>, <a href=\"/name/5899/\" class=\"all\">Роб Райнер</a>, <a href=\"/name/7987/\" class=\"all\">Тим Роббинс</a>, <a href=\"/name/39036/\" class=\"all\">Брэд Силберлинг</a> и <a href=\"/name/63414/\" class=\"all\">Питер Уир</a>.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* В марте 2000 года <a href=\"/name/24027/\" class=\"all\">Вольфганг Петерсен</a> и <a href=\"/name/5899/\" class=\"all\">Роб Райнер</a> по различным причинам выбыли из соискателей вакансии, число которых сократилось до четырёх: <a href=\"/name/24817/\" class=\"all\">Криса Коламбуса</a>, <a href=\"/name/42918/\" class=\"all\">Терри Гиллиама</a>, <a href=\"/name/28208/\" class=\"all\">Алана Паркера</a> и <a href=\"/name/39036/\" class=\"all\">Брэда Силберлинга</a> из которых  <a href=\"/name/40777/\" class=\"all\">Дж. К. Роулинг</a> отдавала предпочтение Гиллиаму. Несмотря на это 28 марта 2000 года было объявлено, что режиссёрское кресло досталось <a href=\"/name/24817/\" class=\"all\">Крису Коламбусу</a>.  Впоследствии <a href=\"/name/42918/\" class=\"all\">Терри Гиллиам</a> открыто выразил своё недовольство и разочарование, сказав, что он был идеальным кандидатом на эту роль и назвав фильм Коламбуса просто ужасным, скучным и банальным. Решающую роль в выборе киностудии сыграл большой опыт работы режиссёра с молодыми актёрами и успех его предыдущих  кинолент семейной направленности &#171;<a href=\"/film/8124/\" class=\"all\">Один дома</a>&#187; (1990) и &#171;<a href=\"/film/5939/\" class=\"all\">Миссис Даутфайр</a>&#187; (1993). С другой стороны одной из главных причин согласия Криса Коламбуса стали неустанные просьбы его дочери, большой поклонницы книг Дж. К. Роулинг.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Честь написать сценарий досталась <a href=\"/name/10093/\" class=\"all\">Стивену Кловзу</a>, который вёл переговоры и встречался ещё со <a href=\"/name/22260/\" class=\"all\">Стивеном Спилбергом</a>, когда тот рассматривался в качестве основного кандидата на место режиссёра. По словам сценариста, ему прислали целый ряд коротких книжных синопсисов для адаптации, из которых он сразу выделил для себя Гарри Поттера. Он вышел на улицу, купил книгу и моментально сделался поклонником творчества Роулинг. При этом он брал на себя обязательство написания сценариев и для  следующих фильмов франчайза.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Сценарист <a href=\"/name/30850/\" class=\"all\">Майкл Голденберг</a> также написал свой вариант сценария, но предпочтение было отдано версии <a href=\"/name/10093/\" class=\"all\">Кловза</a>.  Тем не менее,  его работа впечатлила продюсера <a href=\"/name/23449/\" class=\"all\">Дэвида Хеймана</a> и о нём вспомнили, когда Стивен Кловз отказался по причинам  личного характера от работы над сценарием пятого фильма Поттерианы &#171;<a href=\"/film/48356/\" class=\"all\">Гарри Поттер и орден Феникса</a>&#187; (2007).<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Первоначально киностудия Warner Bros. планировала выпустить фильм в прокат на День Независимости, 4 июля 2001 года.  Стеснённость во времени вынудила в итоге перенести премьеру на 16 ноября 2001 года.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Сюзи Фиггис была назначена отвечающей за кастинг актёров. Работая вместе с <a href=\"/name/24817/\" class=\"all\">Коламбусом</a> и <a href=\"/name/40777/\" class=\"all\">Роулинг</a>, она проводила многочисленные прослушивания среди соискателей трёх главных ролей. Были просмотрены тысячи потенциальных кандидатов, но ни один из них не получил одобрение режиссёра и продюсеров. В то же время начались поиски и в Америке, что вызвало недовольство Сюзи Фиггис, которая покинула проект 11 июля 2000 года, утверждая, что многие из просмотренных ей детей были достойны роли, но были отвергнуты режиссёром и продюсерами.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* В конце мая 2000 года официальный сайт будущего фильма поместил сообщение об открытом кастинге на три главные роли. В качестве кандидатов рассматривались исключительно британские дети в возрасте от 9 до 11 лет. На прослушиваниях детям сначала предлагалось прочитать вслух  предложенную им страницу из книги, затем сымпровизировать сцену прибытия учеников в Хогвартс и в третьей стадии прочитать вслух несколько страниц из сценария.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* В июле 2000 года появились многочисленные сообщения о том, что главным кандидатом на роль по настоянию <a href=\"/name/24817/\" class=\"all\">Криса Коламбуса</a> стал американский молодой актёр <a href=\"/name/14185/\" class=\"all\">Лиам Эйкен</a>, который до этого работал с этим режиссёром в фильме &#171;<a href=\"/film/1949/\" class=\"all\">Мачеха</a>&#187; (1998). Он прилетел в Великобританию и даже успел получить официальное предложение сыграть роль Гарри Поттера, которое, однако, было отозвано на следующий день по настоянию <a href=\"/name/40777/\" class=\"all\">Роулинг</a>, твёрдо стоящей на том, что роль должна достаться британскому актёру.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Наконец, 8 августа 2000 года было объявлено, что три главные роли достались практически неизвестным на тот момент британцам <a href=\"/name/40778/\" class=\"all\">Дэниэлу Рэдклиффу</a>, <a href=\"/name/40780/\" class=\"all\">Руперту Гринту</a> и <a href=\"/name/40779/\" class=\"all\">Эмме Уотсон</a>.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* <a href=\"/name/24216/\" class=\"all\">Робби Колтрейн</a>, получивший роль Хагрида, стал первым из взрослых актёров, с которым был заключён контракт на участие в проекте. Второй была <a href=\"/name/20620/\" class=\"all\">Мэгги Смит</a> (Минерва МакГонагалл).<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Главным претендентом на роль Северуса Снейпа долгое время был <a href=\"/name/2145/\" class=\"all\">Тим Рот</a>, по происхождению британец, но с 1990 года живущий в Америке. Тим Рот отклонил предложение, отдав предпочтение &#171;<a href=\"/film/4375/\" class=\"all\">Планете обезьян</a>&#187; <a href=\"/name/21459/\" class=\"all\">Тима Бёртона</a>, после чего роль досталась  <a href=\"/name/202/\" class=\"all\">Алану Рикману</a>, приглашение которого лично одобрила <a href=\"/name/40777/\" class=\"all\">Дж. К. Роулинг</a>.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Несмотря на требования <a href=\"/name/40777/\" class=\"all\">Роулинг</a>, роль миссис Уизли могла достаться американской актрисе <a href=\"/name/21203/\" class=\"all\">Рози О&#8217;Доннелл</a>, которая вела переговоры с <a href=\"/name/24817/\" class=\"all\">Крисом Коламбусом</a>. Роль отошла <a href=\"/name/10356/\" class=\"all\">Джули Уолтерс</a>, которая до этого рассматривалась в качестве главной претендентки на значительно менее важную роль учительницы полётов на метле мадам Трюк.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Роль профессора Квиррела могла достаться <a href=\"/name/14362/\" class=\"all\">Дэвиду Тьюлису</a>, который впоследствии получил роль профессора Люпина в третьем фильме приключений юного волшебника &#171;<a href=\"/film/322/\" class=\"all\">Гарри Поттер и узник Азкабана</a>&#187; (2004).<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Голосом надоедливого полтергейста Пивза стал <a href=\"/name/23578/\" class=\"all\">Рик Майял</a>, правда все сцены с его участием были вырезаны и не вошли в финальную версию фильма.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* <a href=\"/name/22810/\" class=\"all\">Уорвик Дэвис</a>, сыгравший профессора Флитвика, также озвучил второго из гоблинов в банке Гринготтс, роль которого исполнил <a href=\"/name/8953/\" class=\"all\">Верн Тройер</a>.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* <a href=\"/name/10022/\" class=\"all\">Ричард Харрис</a> вначале отклонил предложенную ему роль профессора Альбуса Дамблдора. Пересмотреть своё решение и согласиться его заставила одиннадцатилетняя внучка, которая пригрозила, что никогда с ним больше не будет разговаривать, если он откажется.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Режиссер <a href=\"/name/24817/\" class=\"all\">Крис Коламбус</a> хотел, чтобы оператором фильма непременно был <a href=\"/name/132405/\" class=\"all\">Джон Сил</a> и обратился к киностудии с просьбой привлечь его к участию в проекте, но тот в это время уже был связан контрактом на съёмки киноленты &#171;<a href=\"/film/4582/\" class=\"all\">В ловушке времени</a>&#187; (2003).   К счастью многочисленные задержки в её производстве позволили <a href=\"/name/132405/\" class=\"all\">Джону Силу</a> быть свободным на момент начала съёмок &#171;Гарри Поттера&#187; и принять приглашение Коламбуса.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Один из вариантов сценария фильма предполагал камео для <a href=\"/name/6451/\" class=\"all\">Дрю Бэрримор</a>, провозгласившей себя большой поклонницей книг о Гарри Поттере.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Требования <a href=\"/name/40777/\" class=\"all\">Дж. К. Роулинг</a> о том, что все значительные роли в фильме должны достаться британцам были полностью удовлетворены, тем не менее, в фильме появляется несколько небританских актеров. <a href=\"/name/10022/\" class=\"all\">Ричард Харрис</a> был ирландцем, а <a href=\"/name/51549/\" class=\"all\">Зоэ Уонамейкер</a>, хотя и сделала себе имя как британская актриса, на момент съёмок была гражданкой США. Также роль гоблина в банке Гринготтс, сопровождающего Гарри и Хагрида к хранилищу, оказалась у американца <a href=\"/name/8953/\" class=\"all\">Верна Тройера</a>, а одну из учениц Хогвартса Сюзан Боунс  сыграла <a href=\"/name/40800/\" class=\"all\">Элинор Коламбус</a>, дочь режиссёра <a href=\"/name/24817/\" class=\"all\">Криса Коламбуса</a>.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Съёмки картины начались 2 октября 2000 года в павильонах студии Leavesden Studios, расположенной примерно в 50 километрах от Лондона и в самом городе.  Большая часть сцен, происходящих в Хогвартсе и рядом со школой, были сняты в кафедральных соборах городов Глостера и Дюрэма. Сцены в госпитале и библиотеке школы были сняты в двух  старинных строениях Оксфордского университета Oxford Divinity School и the Duke Humfrey Library.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Решение проводить съёмки в кафедральном соборе Глостера вызвало массовые протесты со стороны местных жителей. Многие негодующие письма были опубликованы в локальной прессе и обвиняли создателей фильма в святотатстве, угрожая не допустить съёмочную группу в собор. Правда, в день, на который было назначено начало съёмок, объявился только один протестующий местный житель.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* В качестве банка волшебников Гринготтс было использовано здание австралийского представительства в Лондоне. В соответствии с книгой съёмки также прошли в Лондонском зоопарке  и на вокзале Кинг-Кросс. Завершился съёмочный процесс в июле 2001 года. <br/>&nbsp;&nbsp;&nbsp;&nbsp;* Режиссёр <a href=\"/name/24817/\" class=\"all\">Крис Коламбус</a> посчитал необходимым, что при создании волшебных существ  в фильме должны быть использованы и специально сконструированные двигающиеся  механические модели (так называемая аниматроника) и компьютерная графика. Компьютерные спецэффекты были главным образом использованы при создании пещерного тролля и дракона Норберта.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Во всём мире, кроме США фильм вышел под названием &#171;Гарри Поттер и философский камень&#187; (&#171;Harry Potter and the Philosopher`s Stone&#187;).  В североамериканском прокате название было изменено на &#171;Гарри Поттер и волшебный камень&#187; (&#171;Harry Potter and the Sorcerer`s Stone&#187;). По этой причине все сцены, в которых упоминается философский камень, были сняты дважды: в одном случае актёры произносят философский камень (Philosopher`s Stone), а в другом волшебный камень (Sorcerer`s stone).<br/>&nbsp;&nbsp;&nbsp;&nbsp;* В одной из сцен фильма, когда Гарри, Рон и Гермиона подходят к хижине Хагрида, он играет на самодельной дудочке. В этот момент он играет тему Хедвиг (&#171;Hedwig`s Theme&#187;) из саундтрэка фильма.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* В качестве Хогвартского экспресса в этом и последующих фильмах был использован паровоз  GWR 4900 Class 5972 Olton Hall, принадлежавший когда-то британской компании  Great Western Railway и построенный в 1937 году. С 2004 года частный туристический оператор  Beyond Boundaries Travel организует на нём туры по Британии исключительно для фанатов Гарри Поттера. <br/>&nbsp;&nbsp;&nbsp;&nbsp;* На Кубке Квиддича помимо имени отца Гарри в числе прочих также выгравированы имена М. Макгонагалл и Р. Дж. Х. Кинг.  Второй персонаж получил свое имя в честь <a href=\"/name/2000075/\" class=\"all\">Джона Кинга</a>, отвечавшего за производство и использование декораций в проекте.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* В Трофейной комнате, справа от Кубка Квиддича можно увидеть награду &#171;за особые заслуги перед школой&#187;. При этом видна часть имени  Тома М. Реддла, выгравированного  на ней.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Среди портретов на исчезающей лестнице можно заметить картину с изображением английской королевы Анны Болейн, второй жены короля Генриха VIII и матери Елизаветы I. Многие современники Анны Болейн полагали, что она ведьма. <br/>&nbsp;&nbsp;&nbsp;&nbsp;* Использованная при съемках полосатая кошка пропала без следа и была найдена после усиленных поисков только через два дня. <br/>&nbsp;&nbsp;&nbsp;&nbsp;* Одним из дублёров <a href=\"/name/24216/\" class=\"all\">Робби Колтрейна</a> был бывший игрок английской национальной регбийной сборной <a href=\"/name/40823/\" class=\"all\">Мартин Бэйфилд</a>.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* В качестве Хедвиг, совы Гарри Поттера, были задействованы сразу три совы Оок, Гизмо и Спраут.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Николас Фламель, упомянутый как создатель философского камня, был в реальности известным алхимиком, по мнению некоторых из его  современников действительно создавший философский камень и смерть которого была окружена таинственными обстоятельствами. Существует легенда, что он жив до сих пор и в таком случае ему было бы примерно столько же лет как в фильме и книге.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* По книге ученики школы отправляются в Хогвартс с невидимой обычному взгляду платформы 9 &#190; на вокзале Кингс-Кросс, попасть на которую можно через барьер между платформами 9 и 10. Однако оказалось, что на вокзале Кингс-Кросс платформы 9 и 10 находятся не в главном здании, а в отдельном строении и их разделяет не барьер, а две железнодорожные колеи.  В одном из интервью <a href=\"/name/40777/\" class=\"all\">Дж. К. Роулинг</a> признала свою ошибку и сказала, что перепутала лондонские вокзалы Кингс-Кросс и Юстон, хотя и на втором вокзале платформы 9 и 10 также разделены колеями, а не барьером. Для съёмок фильма были использованы платформы 4 и 5, которые находятся в главной части вокзала Кинг-Кросс и были просто на время переименованы в 9 и 10. Впоследствии на стене здания,  где находятся реальные платформы 9 и 10 был помещён чугунный знак-указатель &#171;Платформа  9 &#190;&#187;, а под ним вмонтирована четверть багажной тележки, оставшаяся часть которой как бы уже исчезла в стене.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Актёр, сыгравший дежурного на вокзале Кингс-Кросс, к которому Гарри обращается за помощью, на самом деле работает служащим на британской железной дороге, правда, в другой должности: он начальник поезда.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Фамилия Дамблдора в переводе со староанглийского означает шмель. <br/>&nbsp;&nbsp;&nbsp;&nbsp;* Девиз школы волшебства и чародейства Хогвартс «Draco dormiens nunquam titillandus» означает в переводе с латыни «Никогда не буди спящего дракона». <br/>&nbsp;&nbsp;&nbsp;&nbsp;* Во время съёмок <a href=\"/name/40778/\" class=\"all\">Дэниэл Рэдклифф</a> решил подшутить над <a href=\"/name/24216/\" class=\"all\">Робби Колтрейном</a> и поменял интерфейс его мобильного телефона на турецкий. Актёр долгое время не мог разобраться  с этим, и вынужден был обратиться за помощью к отцу отвечавшего за грим и макияж Эйтту Феннелю, турку по происхождению.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Перед выходом фильма в прокат в продажу поступила одноимённая компьютерная игра, созданная студией Electronic Arts и ставшая большим хитом. Американская фирма по производству игрушек Mattel Inc. стала победителем соревнования за право производство игрушек, основанных на будущем фильме. Немногим позднее такое право также получил другой промышленный гигант в этой отрасли фирма Hasbro. <br/>&nbsp;&nbsp;&nbsp;&nbsp;* Композитор <a href=\"/name/225027/\" class=\"all\">Джон Уильямс</a> написал музыкальную композицию специально для трейлера фильма. Впоследствии она вошла в саундтрэк фильма под названием &#171;Пролог&#187; (The Prologue), что само по себе случается крайне редко.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Картина установила новый рекорд сборов премьерного уикенда в американском и британском прокате. В Америке касса уикенда составила 90 294 621 доллар, а в Британии 16 335 627 фунтов стерлингов. <br/>&nbsp;&nbsp;&nbsp;&nbsp;* Фильм стал самым кассовым фильмом года в США, собрав за всё время своего проката 317 575 550 долларов и повторил это же достижение в международном прокате заработав 658,9 млн. долларов. По итогам мирового проката фильм вышел на второе место за всю историю с суммой в 976,5 млн. долларов, уступая на тот момент одному лишь &#171;<a href=\"/film/2213/\" class=\"all\">Титанику</a>&#187; <a href=\"/name/27977/\" class=\"all\">Джеймса Кэмерона</a>.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* <a href=\"/name/4666/\" class=\"all\">Анна Попплуэлл</a> пробовалась на роль Гермионы Грейнджер.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Волшебные шахматы, в которые играют Гарри и Рон были созданы на основе шахмат с острова Льюис, самых известных шахматных фигур в мире, найденных в песке на пляже острова Льюис, Гебридские острова и датируемых XII-м веком.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* На роль Гарри Поттера пробовался актер <a href=\"/name/752/\" class=\"all\">Антон Ельчин</a>.<br/>", movie.Overview);
        Assert.Equal(2001, movie.ProductionYear);
        Assert.Equal("689", movie.GetProviderId(Plugin.PluginKey));
        Assert.Equal("tt0241527", movie.GetProviderId(MetadataProviders.Imdb));
        Assert.Equal("671", movie.GetProviderId(MetadataProviders.Tmdb));
        Assert.True(20 <= movie.RemoteTrailers.Length);
        Assert.Equal(152, movie.Size);
        Assert.Equal(movie.Name, movie.SortName);
        Assert.Equal(3, movie.Studios.Length);
        Assert.Equal("Путешествие в твою мечту", movie.Tagline);

        _logManager.Verify(lm => lm.GetLogger(It.IsAny<string>()), Times.Exactly(5));
        _fileSystem.Verify(fs => fs.GetDirectoryName("/emby/movie_library/Гарри Поттер и философский камень (2001).mkv"), Times.Once());
        _applicationPaths.VerifyGet(ap => ap.PluginConfigurationsPath, Times.Once());
        _xmlSerializer.Verify(xs => xs.DeserializeFromFile(typeof(PluginConfiguration), "KpLocalMetadata_WithNameYearAndAddToExistingCollection/EmbyKinopoiskRu.xml"), Times.Once());
        _xmlSerializer.Verify(xs => xs.DeserializeFromFile(typeof(LibraryOptions), "KpLocalMetadata_WithNameYearAndAddToExistingCollection/options.xml"), Times.Once());
        _localizationManager.Verify(lm => lm.RemoveDiacritics("Гарри Поттер и философский камень"), Times.Once());
        _libraryManager.Verify(lm => lm.GetUserRootFolder(), Times.Once());
        _libraryManager.Verify(lm => lm.GetLibraryOptions(It.IsAny<UserRootFolder>()), Times.Once());
        _libraryManager.Verify(lm => lm.GetInternalItemIds(It.IsAny<InternalItemsQuery>()), Times.Exactly(2));
        _libraryManager.Verify(lm => lm.GetItemById(1L), Times.Once());
        _libraryManager.Verify(lm => lm.QueryItems(It.IsAny<InternalItemsQuery>()), Times.Exactly(5));
        _libraryManager.Verify(lm => lm.GetItemLinks(It.IsInRange(101L, 108L, Moq.Range.Inclusive), It.IsAny<List<ItemLinkType>>()), Times.Exactly(8));
        _libraryManager.Verify(lm => lm.UpdateItem(It.IsAny<BaseItem>(), It.IsAny<BaseItem>(), ItemUpdateType.MetadataEdit, null), Times.Exactly(8));
        _serverApplicationHost.Verify(sah => sah.ExpandVirtualPath("/emby/movie_library"), Times.Once());
        VerifyNoOtherCalls();

        Logger.Info($"Finished '{nameof(KpLocalMetadata_WithNameYearAndAddToExistingCollection)}'");
    }

    // Will create a Collections virtual folder
    [Fact]
    public async void KpLocalMetadata_WithNameYearAndAddToNewCollection()
    {
        Logger.Info($"Start '{nameof(KpLocalMetadata_WithNameYearAndAddToNewCollection)}'");

        _ = _libraryManager
            .SetupSequence(m => m.GetInternalItemIds(It.Is<InternalItemsQuery>(q => Equals(true, q.IsFolder))))
            .Returns(Array.Empty<long>())
            .Returns(new long[] { 1L });

        _ = _libraryManager // EmbyHelper.SearchExistingCollection(). Search boxset contains all sequence movies
            .Setup(m => m.QueryItems(It.Is<InternalItemsQuery>(query =>
                query.IncludeItemTypes.Length == 1
                && query.IncludeItemTypes.All(item => "boxset".Equals(item, StringComparison.Ordinal))
                && query.ListItemIds.Length == _internalIdPotterSequence.Length
                && query.ListItemIds.All(item => _internalIdPotterSequence.Contains(item))
            )))
            .Returns(new QueryResult<BaseItem>() { Items = Array.Empty<BaseItem>() });

        _ = _collectionManager
            .Setup(m => m.CreateCollection(It.IsAny<CollectionCreationOptions>()))
            .Returns((CollectionCreationOptions options) =>
                Task.FromResult(new BoxSet()
                {
                    Name = options.Name,
                    ParentId = options.ParentId,
                    InternalId = 201L,
                }));

        LibraryOptions boxsetLibraryOptions = new()
        {
            ContentType = CollectionType.BoxSets.ToString(),
            MetadataCountryCode = "RU",
            MinCollectionItems = 1,
            Name = "Collections",
            PathInfos = new MediaPathInfo[]{
                        new MediaPathInfo()
                        {
                            NetworkPath = null,
                            Path = "/emby/movie_library"
                        }
                    },
            PreferredImageLanguage = "ru",
            PreferredMetadataLanguage = "ru",
        };
        _ = _xmlSerializer
            .Setup(m => m.DeserializeFromFile(typeof(LibraryOptions), "KpLocalMetadata_WithNameYearAndAddToNewCollection/options.xml"))
            .Returns(boxsetLibraryOptions);

        _ = _applicationPaths
            .SetupGet(m => m.PluginConfigurationsPath)
            .Returns("KpLocalMetadata_WithNameYearAndAddToNewCollection");

        _ = _libraryManager
            .Setup(m => m.GetItemById(It.Is<long>(id => id == 1L)))
            .Returns(new CollectionFolder()
            {
                Name = "Collections",
                Path = "KpLocalMetadata_WithNameYearAndAddToNewCollection"
            });

        var itemInfo = new ItemInfo(new Movie()
        {
            Path = "/emby/movie_library/Гарри Поттер и философский камень (2001).mkv",
            Container = "mkv",
            IsInMixedFolder = false,
            Id = new Guid(),
            Name = "Гарри Поттер и философский камень"
        });
        using var cancellationTokenSource = new CancellationTokenSource();
        _pluginConfiguration.CreateCollections = true;
        MetadataResult<Movie> result = await _kpMovieLocalMetadata.GetMetadata(itemInfo, _movieLibraryOptions, _directoryService.Object, cancellationTokenSource.Token);

        Movie movie = result.Item;
        Assert.NotNull(movie);
        Assert.True(result.HasMetadata);
        LinkedItemInfo collection = Assert.Single(movie.Collections);
        Assert.True(201L == collection.Id);
        Assert.True(8 < movie.CommunityRating);
        Assert.Equal("689", movie.ExternalId);
        Assert.Equal(3, movie.Genres.Length);
        Assert.Equal("Video", movie.MediaType);
        Assert.Equal("Гарри Поттер и философский камень", movie.Name);
        Assert.Equal("pg", movie.OfficialRating);
        Assert.Equal("Harry Potter and the Sorcerer's Stone", movie.OriginalTitle);
        Assert.Equal("Жизнь десятилетнего Гарри Поттера нельзя назвать сладкой: родители умерли, едва ему исполнился год, а от дяди и тёти, взявших сироту на воспитание, достаются лишь тычки да подзатыльники. Но в одиннадцатый день рождения Гарри всё меняется. Странный гость, неожиданно появившийся на пороге, приносит письмо, из которого мальчик узнаёт, что на самом деле он - волшебник и зачислен в школу магии под названием Хогвартс. А уже через пару недель Гарри будет мчаться в поезде Хогвартс-экспресс навстречу новой жизни, где его ждут невероятные приключения, верные друзья и самое главное — ключ к разгадке тайны смерти его родителей.<br/><br/><b>Интересное:</b><br/>&nbsp;&nbsp;&nbsp;&nbsp;* Фильм снят по мотивам романа <a href=\"/name/40777/\" class=\"all\">Дж.К. Роулинг</a> &#171;Гарри Поттер и философский камень&#187; (Harry Potter and the Philosopher's Stone, 1997).<br/>&nbsp;&nbsp;&nbsp;&nbsp;* <a href=\"/name/40777/\" class=\"all\">Дж. К. Роулинг</a> продала права на создание фильмов по первым четырем книгам приключений Гарри Поттера в 1999 году за скромную сумму в один миллион фунтов стерлингов (на тот момент чуть больше 1,5 млн. долларов). Что намного важнее, было оговорено, что писательница будет получать определённую часть от сборов каждого из фильмов, и будет иметь значительный контроль над всеми стадиями производства картин.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* <a href=\"/name/40777/\" class=\"all\">Дж. К. Роулинг</a> поставила непременное условие, что все актёры в будущих фильмах должны быть британцами. Лишь в четвёртом фильме франчайза, где это было необходимо по содержанию книги, появились актеры из других стран.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Первоначально главным претендентом на место режиссёра картины был <a href=\"/name/22260/\" class=\"all\">Стивен Спилберг</a>. Переговоры с ним велись в течение нескольких месяцев, и в феврале 2000 года закончились его отказом. Стивен Спилберг в качестве основного варианта рассматривал создание анимационной ленты, где голосом главного героя был бы <a href=\"/name/11381/\" class=\"all\">Хэйли Джоэл Осмент</a>. Этот вариант не понравился ни киностудии, ни автору книг. В последующем знаменитый режиссёр продолжал настаивать на участии американского актёра в главной роли. Другой причиной своего отказа он назвал отсутствие творческого интереса к проекту. По словам Стивена Спилберга фильм был обречён на колоссальный коммерческий успех, независимо от того, насколько удачной будет его работа.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* После отказа <a href=\"/name/22260/\" class=\"all\">Спилберга</a> начались переговоры сразу с несколькими режиссёрами. В качестве основных кандидатур рассматривались <a href=\"/name/24817/\" class=\"all\">Крис Коламбус</a>, <a href=\"/name/42918/\" class=\"all\">Терри Гиллиам</a>, <a href=\"/name/12659/\" class=\"all\">Джонатан Демме</a>, <a href=\"/name/20861/\" class=\"all\">Майк Ньюэлл</a>, <a href=\"/name/28208/\" class=\"all\">Алан Паркер</a>, <a href=\"/name/24027/\" class=\"all\">Вольфганг Петерсен</a>, <a href=\"/name/5899/\" class=\"all\">Роб Райнер</a>, <a href=\"/name/7987/\" class=\"all\">Тим Роббинс</a>, <a href=\"/name/39036/\" class=\"all\">Брэд Силберлинг</a> и <a href=\"/name/63414/\" class=\"all\">Питер Уир</a>.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* В марте 2000 года <a href=\"/name/24027/\" class=\"all\">Вольфганг Петерсен</a> и <a href=\"/name/5899/\" class=\"all\">Роб Райнер</a> по различным причинам выбыли из соискателей вакансии, число которых сократилось до четырёх: <a href=\"/name/24817/\" class=\"all\">Криса Коламбуса</a>, <a href=\"/name/42918/\" class=\"all\">Терри Гиллиама</a>, <a href=\"/name/28208/\" class=\"all\">Алана Паркера</a> и <a href=\"/name/39036/\" class=\"all\">Брэда Силберлинга</a> из которых  <a href=\"/name/40777/\" class=\"all\">Дж. К. Роулинг</a> отдавала предпочтение Гиллиаму. Несмотря на это 28 марта 2000 года было объявлено, что режиссёрское кресло досталось <a href=\"/name/24817/\" class=\"all\">Крису Коламбусу</a>.  Впоследствии <a href=\"/name/42918/\" class=\"all\">Терри Гиллиам</a> открыто выразил своё недовольство и разочарование, сказав, что он был идеальным кандидатом на эту роль и назвав фильм Коламбуса просто ужасным, скучным и банальным. Решающую роль в выборе киностудии сыграл большой опыт работы режиссёра с молодыми актёрами и успех его предыдущих  кинолент семейной направленности &#171;<a href=\"/film/8124/\" class=\"all\">Один дома</a>&#187; (1990) и &#171;<a href=\"/film/5939/\" class=\"all\">Миссис Даутфайр</a>&#187; (1993). С другой стороны одной из главных причин согласия Криса Коламбуса стали неустанные просьбы его дочери, большой поклонницы книг Дж. К. Роулинг.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Честь написать сценарий досталась <a href=\"/name/10093/\" class=\"all\">Стивену Кловзу</a>, который вёл переговоры и встречался ещё со <a href=\"/name/22260/\" class=\"all\">Стивеном Спилбергом</a>, когда тот рассматривался в качестве основного кандидата на место режиссёра. По словам сценариста, ему прислали целый ряд коротких книжных синопсисов для адаптации, из которых он сразу выделил для себя Гарри Поттера. Он вышел на улицу, купил книгу и моментально сделался поклонником творчества Роулинг. При этом он брал на себя обязательство написания сценариев и для  следующих фильмов франчайза.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Сценарист <a href=\"/name/30850/\" class=\"all\">Майкл Голденберг</a> также написал свой вариант сценария, но предпочтение было отдано версии <a href=\"/name/10093/\" class=\"all\">Кловза</a>.  Тем не менее,  его работа впечатлила продюсера <a href=\"/name/23449/\" class=\"all\">Дэвида Хеймана</a> и о нём вспомнили, когда Стивен Кловз отказался по причинам  личного характера от работы над сценарием пятого фильма Поттерианы &#171;<a href=\"/film/48356/\" class=\"all\">Гарри Поттер и орден Феникса</a>&#187; (2007).<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Первоначально киностудия Warner Bros. планировала выпустить фильм в прокат на День Независимости, 4 июля 2001 года.  Стеснённость во времени вынудила в итоге перенести премьеру на 16 ноября 2001 года.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Сюзи Фиггис была назначена отвечающей за кастинг актёров. Работая вместе с <a href=\"/name/24817/\" class=\"all\">Коламбусом</a> и <a href=\"/name/40777/\" class=\"all\">Роулинг</a>, она проводила многочисленные прослушивания среди соискателей трёх главных ролей. Были просмотрены тысячи потенциальных кандидатов, но ни один из них не получил одобрение режиссёра и продюсеров. В то же время начались поиски и в Америке, что вызвало недовольство Сюзи Фиггис, которая покинула проект 11 июля 2000 года, утверждая, что многие из просмотренных ей детей были достойны роли, но были отвергнуты режиссёром и продюсерами.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* В конце мая 2000 года официальный сайт будущего фильма поместил сообщение об открытом кастинге на три главные роли. В качестве кандидатов рассматривались исключительно британские дети в возрасте от 9 до 11 лет. На прослушиваниях детям сначала предлагалось прочитать вслух  предложенную им страницу из книги, затем сымпровизировать сцену прибытия учеников в Хогвартс и в третьей стадии прочитать вслух несколько страниц из сценария.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* В июле 2000 года появились многочисленные сообщения о том, что главным кандидатом на роль по настоянию <a href=\"/name/24817/\" class=\"all\">Криса Коламбуса</a> стал американский молодой актёр <a href=\"/name/14185/\" class=\"all\">Лиам Эйкен</a>, который до этого работал с этим режиссёром в фильме &#171;<a href=\"/film/1949/\" class=\"all\">Мачеха</a>&#187; (1998). Он прилетел в Великобританию и даже успел получить официальное предложение сыграть роль Гарри Поттера, которое, однако, было отозвано на следующий день по настоянию <a href=\"/name/40777/\" class=\"all\">Роулинг</a>, твёрдо стоящей на том, что роль должна достаться британскому актёру.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Наконец, 8 августа 2000 года было объявлено, что три главные роли достались практически неизвестным на тот момент британцам <a href=\"/name/40778/\" class=\"all\">Дэниэлу Рэдклиффу</a>, <a href=\"/name/40780/\" class=\"all\">Руперту Гринту</a> и <a href=\"/name/40779/\" class=\"all\">Эмме Уотсон</a>.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* <a href=\"/name/24216/\" class=\"all\">Робби Колтрейн</a>, получивший роль Хагрида, стал первым из взрослых актёров, с которым был заключён контракт на участие в проекте. Второй была <a href=\"/name/20620/\" class=\"all\">Мэгги Смит</a> (Минерва МакГонагалл).<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Главным претендентом на роль Северуса Снейпа долгое время был <a href=\"/name/2145/\" class=\"all\">Тим Рот</a>, по происхождению британец, но с 1990 года живущий в Америке. Тим Рот отклонил предложение, отдав предпочтение &#171;<a href=\"/film/4375/\" class=\"all\">Планете обезьян</a>&#187; <a href=\"/name/21459/\" class=\"all\">Тима Бёртона</a>, после чего роль досталась  <a href=\"/name/202/\" class=\"all\">Алану Рикману</a>, приглашение которого лично одобрила <a href=\"/name/40777/\" class=\"all\">Дж. К. Роулинг</a>.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Несмотря на требования <a href=\"/name/40777/\" class=\"all\">Роулинг</a>, роль миссис Уизли могла достаться американской актрисе <a href=\"/name/21203/\" class=\"all\">Рози О&#8217;Доннелл</a>, которая вела переговоры с <a href=\"/name/24817/\" class=\"all\">Крисом Коламбусом</a>. Роль отошла <a href=\"/name/10356/\" class=\"all\">Джули Уолтерс</a>, которая до этого рассматривалась в качестве главной претендентки на значительно менее важную роль учительницы полётов на метле мадам Трюк.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Роль профессора Квиррела могла достаться <a href=\"/name/14362/\" class=\"all\">Дэвиду Тьюлису</a>, который впоследствии получил роль профессора Люпина в третьем фильме приключений юного волшебника &#171;<a href=\"/film/322/\" class=\"all\">Гарри Поттер и узник Азкабана</a>&#187; (2004).<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Голосом надоедливого полтергейста Пивза стал <a href=\"/name/23578/\" class=\"all\">Рик Майял</a>, правда все сцены с его участием были вырезаны и не вошли в финальную версию фильма.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* <a href=\"/name/22810/\" class=\"all\">Уорвик Дэвис</a>, сыгравший профессора Флитвика, также озвучил второго из гоблинов в банке Гринготтс, роль которого исполнил <a href=\"/name/8953/\" class=\"all\">Верн Тройер</a>.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* <a href=\"/name/10022/\" class=\"all\">Ричард Харрис</a> вначале отклонил предложенную ему роль профессора Альбуса Дамблдора. Пересмотреть своё решение и согласиться его заставила одиннадцатилетняя внучка, которая пригрозила, что никогда с ним больше не будет разговаривать, если он откажется.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Режиссер <a href=\"/name/24817/\" class=\"all\">Крис Коламбус</a> хотел, чтобы оператором фильма непременно был <a href=\"/name/132405/\" class=\"all\">Джон Сил</a> и обратился к киностудии с просьбой привлечь его к участию в проекте, но тот в это время уже был связан контрактом на съёмки киноленты &#171;<a href=\"/film/4582/\" class=\"all\">В ловушке времени</a>&#187; (2003).   К счастью многочисленные задержки в её производстве позволили <a href=\"/name/132405/\" class=\"all\">Джону Силу</a> быть свободным на момент начала съёмок &#171;Гарри Поттера&#187; и принять приглашение Коламбуса.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Один из вариантов сценария фильма предполагал камео для <a href=\"/name/6451/\" class=\"all\">Дрю Бэрримор</a>, провозгласившей себя большой поклонницей книг о Гарри Поттере.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Требования <a href=\"/name/40777/\" class=\"all\">Дж. К. Роулинг</a> о том, что все значительные роли в фильме должны достаться британцам были полностью удовлетворены, тем не менее, в фильме появляется несколько небританских актеров. <a href=\"/name/10022/\" class=\"all\">Ричард Харрис</a> был ирландцем, а <a href=\"/name/51549/\" class=\"all\">Зоэ Уонамейкер</a>, хотя и сделала себе имя как британская актриса, на момент съёмок была гражданкой США. Также роль гоблина в банке Гринготтс, сопровождающего Гарри и Хагрида к хранилищу, оказалась у американца <a href=\"/name/8953/\" class=\"all\">Верна Тройера</a>, а одну из учениц Хогвартса Сюзан Боунс  сыграла <a href=\"/name/40800/\" class=\"all\">Элинор Коламбус</a>, дочь режиссёра <a href=\"/name/24817/\" class=\"all\">Криса Коламбуса</a>.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Съёмки картины начались 2 октября 2000 года в павильонах студии Leavesden Studios, расположенной примерно в 50 километрах от Лондона и в самом городе.  Большая часть сцен, происходящих в Хогвартсе и рядом со школой, были сняты в кафедральных соборах городов Глостера и Дюрэма. Сцены в госпитале и библиотеке школы были сняты в двух  старинных строениях Оксфордского университета Oxford Divinity School и the Duke Humfrey Library.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Решение проводить съёмки в кафедральном соборе Глостера вызвало массовые протесты со стороны местных жителей. Многие негодующие письма были опубликованы в локальной прессе и обвиняли создателей фильма в святотатстве, угрожая не допустить съёмочную группу в собор. Правда, в день, на который было назначено начало съёмок, объявился только один протестующий местный житель.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* В качестве банка волшебников Гринготтс было использовано здание австралийского представительства в Лондоне. В соответствии с книгой съёмки также прошли в Лондонском зоопарке  и на вокзале Кинг-Кросс. Завершился съёмочный процесс в июле 2001 года. <br/>&nbsp;&nbsp;&nbsp;&nbsp;* Режиссёр <a href=\"/name/24817/\" class=\"all\">Крис Коламбус</a> посчитал необходимым, что при создании волшебных существ  в фильме должны быть использованы и специально сконструированные двигающиеся  механические модели (так называемая аниматроника) и компьютерная графика. Компьютерные спецэффекты были главным образом использованы при создании пещерного тролля и дракона Норберта.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Во всём мире, кроме США фильм вышел под названием &#171;Гарри Поттер и философский камень&#187; (&#171;Harry Potter and the Philosopher`s Stone&#187;).  В североамериканском прокате название было изменено на &#171;Гарри Поттер и волшебный камень&#187; (&#171;Harry Potter and the Sorcerer`s Stone&#187;). По этой причине все сцены, в которых упоминается философский камень, были сняты дважды: в одном случае актёры произносят философский камень (Philosopher`s Stone), а в другом волшебный камень (Sorcerer`s stone).<br/>&nbsp;&nbsp;&nbsp;&nbsp;* В одной из сцен фильма, когда Гарри, Рон и Гермиона подходят к хижине Хагрида, он играет на самодельной дудочке. В этот момент он играет тему Хедвиг (&#171;Hedwig`s Theme&#187;) из саундтрэка фильма.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* В качестве Хогвартского экспресса в этом и последующих фильмах был использован паровоз  GWR 4900 Class 5972 Olton Hall, принадлежавший когда-то британской компании  Great Western Railway и построенный в 1937 году. С 2004 года частный туристический оператор  Beyond Boundaries Travel организует на нём туры по Британии исключительно для фанатов Гарри Поттера. <br/>&nbsp;&nbsp;&nbsp;&nbsp;* На Кубке Квиддича помимо имени отца Гарри в числе прочих также выгравированы имена М. Макгонагалл и Р. Дж. Х. Кинг.  Второй персонаж получил свое имя в честь <a href=\"/name/2000075/\" class=\"all\">Джона Кинга</a>, отвечавшего за производство и использование декораций в проекте.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* В Трофейной комнате, справа от Кубка Квиддича можно увидеть награду &#171;за особые заслуги перед школой&#187;. При этом видна часть имени  Тома М. Реддла, выгравированного  на ней.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Среди портретов на исчезающей лестнице можно заметить картину с изображением английской королевы Анны Болейн, второй жены короля Генриха VIII и матери Елизаветы I. Многие современники Анны Болейн полагали, что она ведьма. <br/>&nbsp;&nbsp;&nbsp;&nbsp;* Использованная при съемках полосатая кошка пропала без следа и была найдена после усиленных поисков только через два дня. <br/>&nbsp;&nbsp;&nbsp;&nbsp;* Одним из дублёров <a href=\"/name/24216/\" class=\"all\">Робби Колтрейна</a> был бывший игрок английской национальной регбийной сборной <a href=\"/name/40823/\" class=\"all\">Мартин Бэйфилд</a>.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* В качестве Хедвиг, совы Гарри Поттера, были задействованы сразу три совы Оок, Гизмо и Спраут.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Николас Фламель, упомянутый как создатель философского камня, был в реальности известным алхимиком, по мнению некоторых из его  современников действительно создавший философский камень и смерть которого была окружена таинственными обстоятельствами. Существует легенда, что он жив до сих пор и в таком случае ему было бы примерно столько же лет как в фильме и книге.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* По книге ученики школы отправляются в Хогвартс с невидимой обычному взгляду платформы 9 &#190; на вокзале Кингс-Кросс, попасть на которую можно через барьер между платформами 9 и 10. Однако оказалось, что на вокзале Кингс-Кросс платформы 9 и 10 находятся не в главном здании, а в отдельном строении и их разделяет не барьер, а две железнодорожные колеи.  В одном из интервью <a href=\"/name/40777/\" class=\"all\">Дж. К. Роулинг</a> признала свою ошибку и сказала, что перепутала лондонские вокзалы Кингс-Кросс и Юстон, хотя и на втором вокзале платформы 9 и 10 также разделены колеями, а не барьером. Для съёмок фильма были использованы платформы 4 и 5, которые находятся в главной части вокзала Кинг-Кросс и были просто на время переименованы в 9 и 10. Впоследствии на стене здания,  где находятся реальные платформы 9 и 10 был помещён чугунный знак-указатель &#171;Платформа  9 &#190;&#187;, а под ним вмонтирована четверть багажной тележки, оставшаяся часть которой как бы уже исчезла в стене.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Актёр, сыгравший дежурного на вокзале Кингс-Кросс, к которому Гарри обращается за помощью, на самом деле работает служащим на британской железной дороге, правда, в другой должности: он начальник поезда.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Фамилия Дамблдора в переводе со староанглийского означает шмель. <br/>&nbsp;&nbsp;&nbsp;&nbsp;* Девиз школы волшебства и чародейства Хогвартс «Draco dormiens nunquam titillandus» означает в переводе с латыни «Никогда не буди спящего дракона». <br/>&nbsp;&nbsp;&nbsp;&nbsp;* Во время съёмок <a href=\"/name/40778/\" class=\"all\">Дэниэл Рэдклифф</a> решил подшутить над <a href=\"/name/24216/\" class=\"all\">Робби Колтрейном</a> и поменял интерфейс его мобильного телефона на турецкий. Актёр долгое время не мог разобраться  с этим, и вынужден был обратиться за помощью к отцу отвечавшего за грим и макияж Эйтту Феннелю, турку по происхождению.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Перед выходом фильма в прокат в продажу поступила одноимённая компьютерная игра, созданная студией Electronic Arts и ставшая большим хитом. Американская фирма по производству игрушек Mattel Inc. стала победителем соревнования за право производство игрушек, основанных на будущем фильме. Немногим позднее такое право также получил другой промышленный гигант в этой отрасли фирма Hasbro. <br/>&nbsp;&nbsp;&nbsp;&nbsp;* Композитор <a href=\"/name/225027/\" class=\"all\">Джон Уильямс</a> написал музыкальную композицию специально для трейлера фильма. Впоследствии она вошла в саундтрэк фильма под названием &#171;Пролог&#187; (The Prologue), что само по себе случается крайне редко.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Картина установила новый рекорд сборов премьерного уикенда в американском и британском прокате. В Америке касса уикенда составила 90 294 621 доллар, а в Британии 16 335 627 фунтов стерлингов. <br/>&nbsp;&nbsp;&nbsp;&nbsp;* Фильм стал самым кассовым фильмом года в США, собрав за всё время своего проката 317 575 550 долларов и повторил это же достижение в международном прокате заработав 658,9 млн. долларов. По итогам мирового проката фильм вышел на второе место за всю историю с суммой в 976,5 млн. долларов, уступая на тот момент одному лишь &#171;<a href=\"/film/2213/\" class=\"all\">Титанику</a>&#187; <a href=\"/name/27977/\" class=\"all\">Джеймса Кэмерона</a>.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* <a href=\"/name/4666/\" class=\"all\">Анна Попплуэлл</a> пробовалась на роль Гермионы Грейнджер.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Волшебные шахматы, в которые играют Гарри и Рон были созданы на основе шахмат с острова Льюис, самых известных шахматных фигур в мире, найденных в песке на пляже острова Льюис, Гебридские острова и датируемых XII-м веком.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* На роль Гарри Поттера пробовался актер <a href=\"/name/752/\" class=\"all\">Антон Ельчин</a>.<br/>", movie.Overview);
        Assert.Equal(2001, movie.ProductionYear);
        Assert.Equal("689", movie.GetProviderId(Plugin.PluginKey));
        Assert.Equal("tt0241527", movie.GetProviderId(MetadataProviders.Imdb));
        Assert.Equal("671", movie.GetProviderId(MetadataProviders.Tmdb));
        Assert.True(20 <= movie.RemoteTrailers.Length);
        Assert.Equal(152, movie.Size);
        Assert.Equal(movie.Name, movie.SortName);
        Assert.Equal(3, movie.Studios.Length);
        Assert.Equal("Путешествие в твою мечту", movie.Tagline);

        _logManager.Verify(lm => lm.GetLogger(It.IsAny<string>()), Times.Exactly(5));
        _fileSystem.Verify(fs => fs.GetDirectoryName("/emby/movie_library/Гарри Поттер и философский камень (2001).mkv"), Times.Once());
        _applicationPaths.VerifyGet(ap => ap.PluginConfigurationsPath, Times.Once());
        _xmlSerializer.Verify(xs => xs.DeserializeFromFile(typeof(PluginConfiguration), "KpLocalMetadata_WithNameYearAndAddToNewCollection/EmbyKinopoiskRu.xml"), Times.Once());
        _xmlSerializer.Verify(xs => xs.DeserializeFromFile(typeof(LibraryOptions), "KpLocalMetadata_WithNameYearAndAddToNewCollection/options.xml"), Times.Once());
        _localizationManager.Verify(lm => lm.RemoveDiacritics("Гарри Поттер и философский камень"), Times.Once());
        _libraryManager.Verify(lm => lm.GetUserRootFolder(), Times.Exactly(2));
        _libraryManager.Verify(lm => lm.GetLibraryOptions(It.IsAny<UserRootFolder>()), Times.Exactly(2));
        _libraryManager.Verify(lm => lm.GetInternalItemIds(It.IsAny<InternalItemsQuery>()), Times.Exactly(3));
        _libraryManager.Verify(lm => lm.AddVirtualFolder("Collections", It.IsAny<LibraryOptions>(), true), Times.Once());
        _libraryManager.Verify(lm => lm.GetItemById(1L), Times.Once());
        _libraryManager.Verify(lm => lm.QueryItems(It.IsAny<InternalItemsQuery>()), Times.Exactly(4));
        _collectionManager.Verify(cm => cm.CreateCollection(It.IsAny<CollectionCreationOptions>()), Times.Once());
        _serverApplicationHost.Verify(sah => sah.ExpandVirtualPath("/emby/movie_library"), Times.Once());
        VerifyNoOtherCalls();

        Logger.Info($"Finished '{nameof(KpLocalMetadata_WithNameYearAndAddToNewCollection)}'");
    }

    [Fact]
    public void KpLocalMetadata_ForCodeCoverage()
    {
        Logger.Info($"Start '{nameof(KpLocalMetadata_ForCodeCoverage)}'");

        Assert.NotNull(_kpMovieLocalMetadata.Name);
        _ = new KpSeriesLocalMetadata(_logManager.Object);

        Logger.Info($"Finish '{nameof(KpLocalMetadata_ForCodeCoverage)}'");
    }

}
