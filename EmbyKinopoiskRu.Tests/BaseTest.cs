using EmbyKinopoiskRu.Configuration;
using EmbyKinopoiskRu.Tests.Utils;

using FluentAssertions;

using MediaBrowser.Common.Configuration;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Collections;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Notifications;
using MediaBrowser.Controller.Persistence;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Activity;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Globalization;
using MediaBrowser.Model.IO;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Providers;
using MediaBrowser.Model.Serialization;

namespace EmbyKinopoiskRu.Tests;

/*
// install the tool
dotnet tool install --global dotnet-reportgenerator-globaltool --version 5.2.5

dotnet test --collect:"XPlat Code Coverage"
reportgenerator -reports:"coverage.cobertura.xml" -targetdir:"coveragereport" -reporttypes:Html
*/
public abstract class BaseTest
{
    private const string KinopoiskDevToken = "8DA0EV2-KTP4A5Q-G67QP3K-S2VFBX7";
    private const string KinopoiskUnofficialToken = "0f162131-81c1-4979-b46c-3eea4263fb11";

    private readonly NLog.ILogger _logger;


    #region Mock

    protected readonly Mock<ILogManager> _logManager = new();
    protected readonly Mock<IDirectoryService> _directoryService = new();
    protected readonly Mock<IFileSystem> _fileSystem = new();
    protected readonly Mock<IApplicationPaths> _applicationPaths = new();
    protected readonly Mock<IXmlSerializer> _xmlSerializer = new();
    protected readonly Mock<ILibraryManager> _libraryManager = new();
    protected readonly Mock<ICollectionManager> _collectionManager = new();
    protected readonly Mock<ILocalizationManager> _localizationManager = new();
    protected readonly Mock<IServerConfigurationManager> _serverConfigurationManager = new();
    protected readonly Mock<IActivityManager> _activityManager = new();
    protected readonly Mock<IServerApplicationHost> _serverApplicationHost = new();
    protected readonly Mock<IItemRepository> _itemRepository = new();
    protected readonly Mock<INotificationManager> _notificationManager = new();

    #endregion

    #region Not Mock

    protected readonly EmbyHttpClient _httpClient = new();
    protected readonly EmbyJsonSerializer _jsonSerializer = new();
    protected readonly PluginConfiguration _pluginConfiguration;

    #endregion

    #region Config

    protected BaseTest(NLog.ILogger logger)
    {
        _logger = logger;

        _pluginConfiguration = new PluginConfiguration
        {
            CreateSeqCollections = false
        };


        _ = _fileSystem
            .Setup(fs => fs.GetDirectoryName(It.IsAny<string>()))
            .Returns((string path) => new FileInfo(path).DirectoryName!);

        _ = _serverApplicationHost
            .Setup(sah => sah.ExpandVirtualPath(It.IsAny<string>()))
            .Returns((string path) => path);

        _ = _localizationManager
            .Setup(lm => lm.RemoveDiacritics(It.IsAny<string>()))
            .Returns((string name) => name);

        _ = _serverConfigurationManager
            .SetupGet(scm => scm.Configuration)
            .Returns(new ServerConfiguration());

        _ = _logManager
            .Setup(lm => lm.GetLogger(It.IsAny<string>()))
            .Returns((string name) => new EmbyLogger(NLog.LogManager.GetLogger(name)));

        BaseItem.ConfigurationManager = _serverConfigurationManager.Object;
        BaseItem.FileSystem = _fileSystem.Object;
        BaseItem.LibraryManager = _libraryManager.Object;
        BaseItem.LocalizationManager = _localizationManager.Object;
        BaseItem.ItemRepository = _itemRepository.Object;
        CollectionFolder.XmlSerializer = _xmlSerializer.Object;
        CollectionFolder.ApplicationHost = _serverApplicationHost.Object;

        _ = new Plugin(
            _applicationPaths.Object,
            _xmlSerializer.Object,
            _logManager.Object,
            _httpClient,
            _jsonSerializer,
            _activityManager.Object,
            _libraryManager.Object,
            _collectionManager.Object,
            _notificationManager.Object
        );
        Plugin.Instance.SetAttributes("EmbyKinopoiskRu.dll", string.Empty, new Version(1, 0, 0));
    }

    protected void ConfigLibraryManager()
    {
        _ = _libraryManager
            .SetupGet(lm => lm.IsScanRunning)
            .Returns(false);

        _ = _libraryManager
            .SetupGet(lm => lm.RootFolder)
            .Returns(new AggregateFolder());

        _ = _libraryManager
            .SetupGet(lm => lm.RootFolderId)
            .Returns(0L);

        var userRootFolder = new UserRootFolder();
        _ = _libraryManager
            .Setup(lm => lm.GetUserRootFolder())
            .Returns(userRootFolder)
            .Callback(() =>
            {
                userRootFolder
                    .ValidateChildren(
                        new EmbyProgress(),
                        CancellationToken.None,
                        new MetadataRefreshOptions(_fileSystem.Object),
                        false)
                    .Wait();
                _logger.Info("Call ValidateChildren (reset UserRootFolder._cachedChildren)");
            });

        _ = _libraryManager
            .Setup(lm => lm.GetItemLinks(It.IsAny<long>(), It.IsAny<List<ItemLinkType>>()))
            .Returns((long _, List<ItemLinkType> _) => new List<(ItemLinkType, string, long)>());

        _ = _libraryManager
            .Setup(lm => lm.UpdateItem(
                It.IsAny<BaseItem>(),
                It.IsAny<BaseItem>(),
                It.IsAny<ItemUpdateType>(),
                It.IsAny<MetadataRefreshOptions>()));

        _ = _libraryManager
            .Setup(lm => lm.AddVirtualFolder("Collections", It.IsAny<LibraryOptions>(), true));
    }

    protected void ConfigXmlSerializer()
    {
        _ = _xmlSerializer
            .Setup(xs => xs.DeserializeFromFile(typeof(PluginConfiguration), It.IsAny<string>()))
            .Returns(_pluginConfiguration);
    }

    #endregion

    #region Moq verifications

    protected void VerifyNoOtherCalls()
    {
        _activityManager.VerifyNoOtherCalls();
        _applicationPaths.VerifyNoOtherCalls();
        _collectionManager.VerifyNoOtherCalls();
        _directoryService.VerifyNoOtherCalls();
        _fileSystem.VerifyNoOtherCalls();
        _itemRepository.VerifyNoOtherCalls();
        _libraryManager.VerifyNoOtherCalls();
        _localizationManager.VerifyNoOtherCalls();
        _logManager.VerifyNoOtherCalls();
        _notificationManager.VerifyNoOtherCalls();
        _serverApplicationHost.VerifyNoOtherCalls();
        _serverConfigurationManager.VerifyNoOtherCalls();
        _xmlSerializer.VerifyNoOtherCalls();
    }

    private void PrintMockInvocations(Mock mock)
    {
        _logger.Info($"Name: {mock.Object.GetType().Name}");
        foreach (IInvocation? invocation in mock.Invocations)
        {
            _logger.Info(invocation);
        }
    }

    protected void PrintMocks()
    {
        PrintMockInvocations(_activityManager);
        PrintMockInvocations(_applicationPaths);
        PrintMockInvocations(_collectionManager);
        PrintMockInvocations(_directoryService);
        PrintMockInvocations(_fileSystem);
        PrintMockInvocations(_itemRepository);
        PrintMockInvocations(_libraryManager);
        PrintMockInvocations(_localizationManager);
        PrintMockInvocations(_logManager);
        PrintMockInvocations(_notificationManager);
        PrintMockInvocations(_serverApplicationHost);
        PrintMockInvocations(_serverConfigurationManager);
        PrintMockInvocations(_xmlSerializer);
    }

    #endregion

    #region Utils

    protected string GetKinopoiskDevToken()
    {
        var token = Environment.GetEnvironmentVariable("KINOPOISK_DEV_TOKEN");
        _logger.Info($"Env token length is: {token?.Length ?? 0}");
        return string.IsNullOrWhiteSpace(token) ? KinopoiskDevToken : token;
    }

    protected string GetKinopoiskUnofficialToken()
    {
        var token = Environment.GetEnvironmentVariable("KINOPOISK_UNOFFICIAL_TOKEN");
        _logger.Info($"Env token length is: {token?.Length ?? 0}");
        return string.IsNullOrWhiteSpace(token) ? KinopoiskUnofficialToken : token;
    }

    #endregion

    #region Verify

    protected static void VerifyEpisode_452973_1_2(Episode episode)
    {
        episode.Should().NotBeNull();
        episode.IndexNumber.Should().Be(2);
        episode.MediaType.Should().Be("Video");
        episode.Name.Should().Be("Эпизод 2. А я сказал — оседлаю!");
        episode.OriginalTitle.Should().Be("I Said I'm Gonna Pilot That Thing!!");
        episode.ParentIndexNumber.Should().Be(1);
        episode.PremiereDate.Should().NotBeNull();
        episode.PremiereDate!.Value.DateTime.Should().HaveYear(2007).And.HaveMonth(4).And.HaveDay(8);
        episode.SortName.Should().Be(episode.Name);
    }

    protected static void VerifyMovie326(Movie movie)
    {
        movie.Should().NotBeNull();
        movie.Collections.Should().BeEmpty();
        movie.CommunityRating.Should().BeGreaterThan(5);
        movie.EndDate.Should().BeNull();
        movie.ExternalId.Should().Be("326");
        movie.Genres.Should().ContainSingle();
        movie.Genres.Should().Contain("драма");
        movie.MediaType.Should().Be("Video");
        movie.Name.Should().Be("Побег из Шоушенка");
        movie.OfficialRating.Should().Be("r");
        movie.OriginalTitle.Should().Be("The Shawshank Redemption");
        movie.Overview.Should().Be("Бухгалтер Энди Дюфрейн обвинён в убийстве собственной жены и её любовника. Оказавшись в тюрьме под названием Шоушенк, он сталкивается с жестокостью и беззаконием, царящими по обе стороны решётки. Каждый, кто попадает в эти стены, становится их рабом до конца жизни. Но Энди, обладающий живым умом и доброй душой, находит подход как к заключённым, так и к охранникам, добиваясь их особого к себе расположения.<br/><br/><b>Интересное:</b><br/>&nbsp;&nbsp;&nbsp;&nbsp;* Фильм снят по мотивам повести <a href=\"/name/24263/\" class=\"all\">Стивена Кинга</a> «Рита Хейуорт и спасение из Шоушенка» (Rita Hayworth and Shawshank Redemption), опубликованной в составе сборника «Четыре сезона» (Different Seasons, 1982).<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Съемки проходили в&#160;Мэнсфилдской исправительной колонии в&#160;штате Огайо. Тюрьма находилась в&#160;таком плачевном состоянии, что пришлось приводить её&#160;в&#160;должный вид.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Оригинальная повесть <a href=\"/name/24263/\" class=\"all\">Стивена Кинга</a> была, по словам самого писателя, кульминацией всех его впечатлений от различных тюремных фильмов, которые он смотрел в детстве.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* <a href=\"/name/24263/\" class=\"all\">Стивен Кинг</a> согласился продать права на&#160;свое произведение практически даром, так как с&#160;<a href=\"/name/24262/\" class=\"all\">Фрэнком</a> их&#160;связывает давняя крепкая дружба. Произошло это после того, как Фрэнк довольно успешно экранизировал рассказ Кинга &#171;<a href=\"/film/7429/\" class=\"all\">Женщина в палате</a>&#187;.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Американское общество защиты животных выступило с&#160;критикой в&#160;адрес фильма, в&#160;котором единственным представителем фауны стал ворон старика Брукса. В&#160;картине есть сцена кормления птицы червяком, найденном во&#160;время обеда в&#160;тарелке главного героя фильма. Общество настояло на&#160;том, чтобы была использована уже мертвая личинка, погибшая естественной смертью. После того как такая особь была найдена, сцену отсняли.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Роль Томми Уильямса изначально была написана под <a href=\"/name/25584/\" class=\"all\">Брэда Питта</a>.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Картина, которую смотрят заключенные, — &#171;<a href=\"/film/8299/\" class=\"all\">Гильда</a>&#187;.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Начальник тюрьмы Нортон насвистывает гимн «Eine feste Burg ist unser Gott», название которого переводится примерно так: «Могучая крепость и есть наш бог».<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Фотографии молодого <a href=\"/name/6750/\" class=\"all\">Моргана Фримана</a> на&#160;документах на&#160;самом деле являются фотографиями его сына <a href=\"/name/6767/\" class=\"all\">Альфонсо Фримана</a>, который также снялся в&#160;одном из&#160;эпизодов фильма.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Несмотря на&#160;то, что в&#160;кинотеатрах фильм не&#160;собрал больших денег, он&#160;стал одним из&#160;самых кассовых релизов на&#160;видео, а&#160;впоследствии и&#160;на&#160;DVD.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Фильм посвящён Аллену Грину (Allen Greene) — близкому другу режиссёра. Аллен скончался незадолго до выхода фильма из-за осложнений СПИДа.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Бывший начальник Мэнсфилдской тюрьмы, где проходили натурные съёмки фильма, <a href=\"/name/1104241/\" class=\"all\">Дэннис Бэйкер</a> снялся в роли пожилого заключённого, сидящего в тюремном автобусе позади Томми Уильямса.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Сценарий <a href=\"/name/24262/\" class=\"all\">Фрэнка Дарабонта</a> очень понравился другому режиссёру, успешно экранизировавшему произведения <a href=\"/name/24263/\" class=\"all\">Стивена Кинга</a>, — <a href=\"/name/5899/\" class=\"all\">Робу Райнеру</a>, постановщику &#171;<a href=\"/film/498/\" class=\"all\">Останься со мной</a>&#187; (1986) и &#171;<a href=\"/film/1574/\" class=\"all\">Мизери</a>&#187; (1990). Райнер был так захвачен материалом, что предложил Дарабонту $2,5 млн за права на сценарий и постановку фильма. Дарабонт серьёзно обдумал предложение, но в конечном счёте решил, что для него этот проект — &#171;шанс сделать что-то действительно великое&#187;, и поставил фильм сам.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* <a href=\"/name/5899/\" class=\"all\">Роб Райнер</a> видел в ролях Реда и Энди Дюфрейна соответственно <a href=\"/name/5679/\" class=\"all\">Харрисона Форда</a> и <a href=\"/name/20302/\" class=\"all\">Тома Круза</a>.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Роль Энди Дюфрейна изначально предложили <a href=\"/name/9144/\" class=\"all\">Тому Хэнксу</a>. Он очень заинтересовался, но не смог принять предложение, из-за того что уже был занят в проекте &#171;<a href=\"/film/448/\" class=\"all\">Форрест Гамп</a>&#187; (1994). Впоследствии Том Хэнкс снялся в главной роли в тюремной драме <a href=\"/name/24262/\" class=\"all\">Фрэнка Дарабонта</a> &#171;<a href=\"/film/435/\" class=\"all\">Зеленая миля</a>&#187; (1999), также поставленной по роману <a href=\"/name/24263/\" class=\"all\">Стивена Кинга</a>.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Роль Энди Дюфрейна также предлагали <a href=\"/name/24087/\" class=\"all\">Кевину Костнеру</a>, но актёр отказался от предложения, о чем впоследствии сильно жалел.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* В оригинальной повести <a href=\"/name/24263/\" class=\"all\">Стивена Кинга</a> Ред — ирландец. Несмотря на то, что в экранизации роль Реда сыграл чернокожий <a href=\"/name/6750/\" class=\"all\">Морган Фриман</a>, было решено оставить в фильме реплику Реда «Может быть, потому что я — ирландец», — как удачную шутку.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Несмотря на то что почти все жители города Мэнсфилда изъявили желание принять участие в съёмках массовых сцен фильма, большинство жителей оказались слишком заняты своей работой и не смогли сниматься. Массовку пришлось набирать в местной богадельне, причём некоторые из её обитателей были бывшими заключенными.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Когда на экране показываются крупные планы рук Энди Дюфрейна, когда в начале фильма он заряжает револьвер и когда Энди вырезает своё имя на стене камеры, — это на самом деле руки режиссёра <a href=\"/name/24262/\" class=\"all\">Фрэнка Дарабонта</a>. Эти кадры были сняты в процессе постпроизводства фильма.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Двое из заключённых Шоушенка носят имена Хейвуд и Флойд. Это отсылка к трилогии <a href=\"/name/47956/\" class=\"all\">Артура Ч. Кларка</a> «<a href=\"/film/380/\" class=\"all\">Космическая одиссея</a>», связующим героем которой является доктор Хейвуд Флойд.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Тюремный номер Энди Дюфрейна — 37927.<br/>");
        movie.PremiereDate.Should().HaveYear(1994).And.HaveMonth(09).And.HaveDay(10);
        movie.ProductionLocations.Should().ContainSingle();
        movie.ProductionLocations.Should().Contain("США");
        movie.ProductionYear.Should().Be(1994);
        movie.ProviderIds.Should().HaveCount(3);
        movie.GetProviderId(Plugin.PluginKey).Should().Be("326");
        movie.GetProviderId(MetadataProviders.Imdb).Should().Be("tt0111161");
        movie.GetProviderId(MetadataProviders.Tmdb).Should().Be("278");
        movie.RemoteTrailers.Should().HaveCount(2);
        movie.Size.Should().Be(142);
        movie.SortName.Should().Be(movie.Name);
        movie.Studios.Should().ContainSingle();
        movie.Tagline.Should().Be("Страх - это кандалы. Надежда - это свобода");

        VerifyMovieCommon(movie);
    }

    protected static void VerifyMovie689(Movie movie)
    {
        movie.Should().NotBeNull();
        // movie.Collections.Should().ContainSingle();
        // movie.Collections[0].Id.Should().Be(201L);
        movie.CommunityRating.Should().BeGreaterThan(8);
        movie.EndDate.Should().BeNull();
        movie.ExternalId.Should().Be("689");
        movie.Genres.Should().HaveCount(3);
        movie.MediaType.Should().Be("Video");
        movie.Name.Should().Be("Гарри Поттер и философский камень");
        movie.OfficialRating.Should().Be("pg");
        movie.OriginalTitle.Should().Be("Harry Potter and the Sorcerer's Stone");
        movie.Overview.Should().Be("Жизнь десятилетнего Гарри Поттера нельзя назвать сладкой: родители умерли, едва ему исполнился год, а от дяди и тёти, взявших сироту на воспитание, достаются лишь тычки да подзатыльники. Но в одиннадцатый день рождения Гарри всё меняется. Странный гость, неожиданно появившийся на пороге, приносит письмо, из которого мальчик узнаёт, что на самом деле он - волшебник и зачислен в школу магии под названием Хогвартс. А уже через пару недель Гарри будет мчаться в поезде Хогвартс-экспресс навстречу новой жизни, где его ждут невероятные приключения, верные друзья и самое главное — ключ к разгадке тайны смерти его родителей.<br/><br/><b>Интересное:</b><br/>&nbsp;&nbsp;&nbsp;&nbsp;* Фильм снят по мотивам романа <a href=\"/name/40777/\" class=\"all\">Дж.К. Роулинг</a> &#171;Гарри Поттер и философский камень&#187; (Harry Potter and the Philosopher's Stone, 1997).<br/>&nbsp;&nbsp;&nbsp;&nbsp;* <a href=\"/name/40777/\" class=\"all\">Дж. К. Роулинг</a> продала права на создание фильмов по первым четырем книгам приключений Гарри Поттера в 1999 году за скромную сумму в один миллион фунтов стерлингов (на тот момент чуть больше 1,5 млн. долларов). Что намного важнее, было оговорено, что писательница будет получать определённую часть от сборов каждого из фильмов, и будет иметь значительный контроль над всеми стадиями производства картин.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* <a href=\"/name/40777/\" class=\"all\">Дж. К. Роулинг</a> поставила непременное условие, что все актёры в будущих фильмах должны быть британцами. Лишь в четвёртом фильме франчайза, где это было необходимо по содержанию книги, появились актеры из других стран.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Первоначально главным претендентом на место режиссёра картины был <a href=\"/name/22260/\" class=\"all\">Стивен Спилберг</a>. Переговоры с ним велись в течение нескольких месяцев, и в феврале 2000 года закончились его отказом. Стивен Спилберг в качестве основного варианта рассматривал создание анимационной ленты, где голосом главного героя был бы <a href=\"/name/11381/\" class=\"all\">Хэйли Джоэл Осмент</a>. Этот вариант не понравился ни киностудии, ни автору книг. В последующем знаменитый режиссёр продолжал настаивать на участии американского актёра в главной роли. Другой причиной своего отказа он назвал отсутствие творческого интереса к проекту. По словам Стивена Спилберга фильм был обречён на колоссальный коммерческий успех, независимо от того, насколько удачной будет его работа.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* После отказа <a href=\"/name/22260/\" class=\"all\">Спилберга</a> начались переговоры сразу с несколькими режиссёрами. В качестве основных кандидатур рассматривались <a href=\"/name/24817/\" class=\"all\">Крис Коламбус</a>, <a href=\"/name/42918/\" class=\"all\">Терри Гиллиам</a>, <a href=\"/name/12659/\" class=\"all\">Джонатан Демме</a>, <a href=\"/name/20861/\" class=\"all\">Майк Ньюэлл</a>, <a href=\"/name/28208/\" class=\"all\">Алан Паркер</a>, <a href=\"/name/24027/\" class=\"all\">Вольфганг Петерсен</a>, <a href=\"/name/5899/\" class=\"all\">Роб Райнер</a>, <a href=\"/name/7987/\" class=\"all\">Тим Роббинс</a>, <a href=\"/name/39036/\" class=\"all\">Брэд Силберлинг</a> и <a href=\"/name/63414/\" class=\"all\">Питер Уир</a>.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* В марте 2000 года <a href=\"/name/24027/\" class=\"all\">Вольфганг Петерсен</a> и <a href=\"/name/5899/\" class=\"all\">Роб Райнер</a> по различным причинам выбыли из соискателей вакансии, число которых сократилось до четырёх: <a href=\"/name/24817/\" class=\"all\">Криса Коламбуса</a>, <a href=\"/name/42918/\" class=\"all\">Терри Гиллиама</a>, <a href=\"/name/28208/\" class=\"all\">Алана Паркера</a> и <a href=\"/name/39036/\" class=\"all\">Брэда Силберлинга</a> из которых  <a href=\"/name/40777/\" class=\"all\">Дж. К. Роулинг</a> отдавала предпочтение Гиллиаму. Несмотря на это 28 марта 2000 года было объявлено, что режиссёрское кресло досталось <a href=\"/name/24817/\" class=\"all\">Крису Коламбусу</a>.  Впоследствии <a href=\"/name/42918/\" class=\"all\">Терри Гиллиам</a> открыто выразил своё недовольство и разочарование, сказав, что он был идеальным кандидатом на эту роль и назвав фильм Коламбуса просто ужасным, скучным и банальным. Решающую роль в выборе киностудии сыграл большой опыт работы режиссёра с молодыми актёрами и успех его предыдущих  кинолент семейной направленности &#171;<a href=\"/film/8124/\" class=\"all\">Один дома</a>&#187; (1990) и &#171;<a href=\"/film/5939/\" class=\"all\">Миссис Даутфайр</a>&#187; (1993). С другой стороны одной из главных причин согласия Криса Коламбуса стали неустанные просьбы его дочери, большой поклонницы книг Дж. К. Роулинг.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Честь написать сценарий досталась <a href=\"/name/10093/\" class=\"all\">Стивену Кловзу</a>, который вёл переговоры и встречался ещё со <a href=\"/name/22260/\" class=\"all\">Стивеном Спилбергом</a>, когда тот рассматривался в качестве основного кандидата на место режиссёра. По словам сценариста, ему прислали целый ряд коротких книжных синопсисов для адаптации, из которых он сразу выделил для себя Гарри Поттера. Он вышел на улицу, купил книгу и моментально сделался поклонником творчества Роулинг. При этом он брал на себя обязательство написания сценариев и для  следующих фильмов франчайза.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Сценарист <a href=\"/name/30850/\" class=\"all\">Майкл Голденберг</a> также написал свой вариант сценария, но предпочтение было отдано версии <a href=\"/name/10093/\" class=\"all\">Кловза</a>.  Тем не менее,  его работа впечатлила продюсера <a href=\"/name/23449/\" class=\"all\">Дэвида Хеймана</a> и о нём вспомнили, когда Стивен Кловз отказался по причинам  личного характера от работы над сценарием пятого фильма Поттерианы &#171;<a href=\"/film/48356/\" class=\"all\">Гарри Поттер и орден Феникса</a>&#187; (2007).<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Первоначально киностудия Warner Bros. планировала выпустить фильм в прокат на День Независимости, 4 июля 2001 года.  Стеснённость во времени вынудила в итоге перенести премьеру на 16 ноября 2001 года.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Сюзи Фиггис была назначена отвечающей за кастинг актёров. Работая вместе с <a href=\"/name/24817/\" class=\"all\">Коламбусом</a> и <a href=\"/name/40777/\" class=\"all\">Роулинг</a>, она проводила многочисленные прослушивания среди соискателей трёх главных ролей. Были просмотрены тысячи потенциальных кандидатов, но ни один из них не получил одобрение режиссёра и продюсеров. В то же время начались поиски и в Америке, что вызвало недовольство Сюзи Фиггис, которая покинула проект 11 июля 2000 года, утверждая, что многие из просмотренных ей детей были достойны роли, но были отвергнуты режиссёром и продюсерами.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* В конце мая 2000 года официальный сайт будущего фильма поместил сообщение об открытом кастинге на три главные роли. В качестве кандидатов рассматривались исключительно британские дети в возрасте от 9 до 11 лет. На прослушиваниях детям сначала предлагалось прочитать вслух  предложенную им страницу из книги, затем сымпровизировать сцену прибытия учеников в Хогвартс и в третьей стадии прочитать вслух несколько страниц из сценария.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* В июле 2000 года появились многочисленные сообщения о том, что главным кандидатом на роль по настоянию <a href=\"/name/24817/\" class=\"all\">Криса Коламбуса</a> стал американский молодой актёр <a href=\"/name/14185/\" class=\"all\">Лиам Эйкен</a>, который до этого работал с этим режиссёром в фильме &#171;<a href=\"/film/1949/\" class=\"all\">Мачеха</a>&#187; (1998). Он прилетел в Великобританию и даже успел получить официальное предложение сыграть роль Гарри Поттера, которое, однако, было отозвано на следующий день по настоянию <a href=\"/name/40777/\" class=\"all\">Роулинг</a>, твёрдо стоящей на том, что роль должна достаться британскому актёру.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Наконец, 8 августа 2000 года было объявлено, что три главные роли достались практически неизвестным на тот момент британцам <a href=\"/name/40778/\" class=\"all\">Дэниэлу Рэдклиффу</a>, <a href=\"/name/40780/\" class=\"all\">Руперту Гринту</a> и <a href=\"/name/40779/\" class=\"all\">Эмме Уотсон</a>.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* <a href=\"/name/24216/\" class=\"all\">Робби Колтрейн</a>, получивший роль Хагрида, стал первым из взрослых актёров, с которым был заключён контракт на участие в проекте. Второй была <a href=\"/name/20620/\" class=\"all\">Мэгги Смит</a> (Минерва МакГонагалл).<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Главным претендентом на роль Северуса Снейпа долгое время был <a href=\"/name/2145/\" class=\"all\">Тим Рот</a>, по происхождению британец, но с 1990 года живущий в Америке. Тим Рот отклонил предложение, отдав предпочтение &#171;<a href=\"/film/4375/\" class=\"all\">Планете обезьян</a>&#187; <a href=\"/name/21459/\" class=\"all\">Тима Бёртона</a>, после чего роль досталась  <a href=\"/name/202/\" class=\"all\">Алану Рикману</a>, приглашение которого лично одобрила <a href=\"/name/40777/\" class=\"all\">Дж. К. Роулинг</a>.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Несмотря на требования <a href=\"/name/40777/\" class=\"all\">Роулинг</a>, роль миссис Уизли могла достаться американской актрисе <a href=\"/name/21203/\" class=\"all\">Рози О&#8217;Доннелл</a>, которая вела переговоры с <a href=\"/name/24817/\" class=\"all\">Крисом Коламбусом</a>. Роль отошла <a href=\"/name/10356/\" class=\"all\">Джули Уолтерс</a>, которая до этого рассматривалась в качестве главной претендентки на значительно менее важную роль учительницы полётов на метле мадам Трюк.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Роль профессора Квиррела могла достаться <a href=\"/name/14362/\" class=\"all\">Дэвиду Тьюлису</a>, который впоследствии получил роль профессора Люпина в третьем фильме приключений юного волшебника &#171;<a href=\"/film/322/\" class=\"all\">Гарри Поттер и узник Азкабана</a>&#187; (2004).<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Голосом надоедливого полтергейста Пивза стал <a href=\"/name/23578/\" class=\"all\">Рик Майял</a>, правда все сцены с его участием были вырезаны и не вошли в финальную версию фильма.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* <a href=\"/name/22810/\" class=\"all\">Уорвик Дэвис</a>, сыгравший профессора Флитвика, также озвучил второго из гоблинов в банке Гринготтс, роль которого исполнил <a href=\"/name/8953/\" class=\"all\">Верн Тройер</a>.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* <a href=\"/name/10022/\" class=\"all\">Ричард Харрис</a> вначале отклонил предложенную ему роль профессора Альбуса Дамблдора. Пересмотреть своё решение и согласиться его заставила одиннадцатилетняя внучка, которая пригрозила, что никогда с ним больше не будет разговаривать, если он откажется.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Режиссер <a href=\"/name/24817/\" class=\"all\">Крис Коламбус</a> хотел, чтобы оператором фильма непременно был <a href=\"/name/132405/\" class=\"all\">Джон Сил</a> и обратился к киностудии с просьбой привлечь его к участию в проекте, но тот в это время уже был связан контрактом на съёмки киноленты &#171;<a href=\"/film/4582/\" class=\"all\">В ловушке времени</a>&#187; (2003).   К счастью многочисленные задержки в её производстве позволили <a href=\"/name/132405/\" class=\"all\">Джону Силу</a> быть свободным на момент начала съёмок &#171;Гарри Поттера&#187; и принять приглашение Коламбуса.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Один из вариантов сценария фильма предполагал камео для <a href=\"/name/6451/\" class=\"all\">Дрю Бэрримор</a>, провозгласившей себя большой поклонницей книг о Гарри Поттере.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Требования <a href=\"/name/40777/\" class=\"all\">Дж. К. Роулинг</a> о том, что все значительные роли в фильме должны достаться британцам были полностью удовлетворены, тем не менее, в фильме появляется несколько небританских актеров. <a href=\"/name/10022/\" class=\"all\">Ричард Харрис</a> был ирландцем, а <a href=\"/name/51549/\" class=\"all\">Зоэ Уонамейкер</a>, хотя и сделала себе имя как британская актриса, на момент съёмок была гражданкой США. Также роль гоблина в банке Гринготтс, сопровождающего Гарри и Хагрида к хранилищу, оказалась у американца <a href=\"/name/8953/\" class=\"all\">Верна Тройера</a>, а одну из учениц Хогвартса Сюзан Боунс  сыграла <a href=\"/name/40800/\" class=\"all\">Элинор Коламбус</a>, дочь режиссёра <a href=\"/name/24817/\" class=\"all\">Криса Коламбуса</a>.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Съёмки картины начались 2 октября 2000 года в павильонах студии Leavesden Studios, расположенной примерно в 50 километрах от Лондона и в самом городе.  Большая часть сцен, происходящих в Хогвартсе и рядом со школой, были сняты в кафедральных соборах городов Глостера и Дюрэма. Сцены в госпитале и библиотеке школы были сняты в двух  старинных строениях Оксфордского университета Oxford Divinity School и the Duke Humfrey Library.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Решение проводить съёмки в кафедральном соборе Глостера вызвало массовые протесты со стороны местных жителей. Многие негодующие письма были опубликованы в локальной прессе и обвиняли создателей фильма в святотатстве, угрожая не допустить съёмочную группу в собор. Правда, в день, на который было назначено начало съёмок, объявился только один протестующий местный житель.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* В качестве банка волшебников Гринготтс было использовано здание австралийского представительства в Лондоне. В соответствии с книгой съёмки также прошли в Лондонском зоопарке  и на вокзале Кинг-Кросс. Завершился съёмочный процесс в июле 2001 года. <br/>&nbsp;&nbsp;&nbsp;&nbsp;* Режиссёр <a href=\"/name/24817/\" class=\"all\">Крис Коламбус</a> посчитал необходимым, что при создании волшебных существ  в фильме должны быть использованы и специально сконструированные двигающиеся  механические модели (так называемая аниматроника) и компьютерная графика. Компьютерные спецэффекты были главным образом использованы при создании пещерного тролля и дракона Норберта.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Во всём мире, кроме США фильм вышел под названием &#171;Гарри Поттер и философский камень&#187; (&#171;Harry Potter and the Philosopher`s Stone&#187;).  В североамериканском прокате название было изменено на &#171;Гарри Поттер и волшебный камень&#187; (&#171;Harry Potter and the Sorcerer`s Stone&#187;). По этой причине все сцены, в которых упоминается философский камень, были сняты дважды: в одном случае актёры произносят философский камень (Philosopher`s Stone), а в другом волшебный камень (Sorcerer`s stone).<br/>&nbsp;&nbsp;&nbsp;&nbsp;* В одной из сцен фильма, когда Гарри, Рон и Гермиона подходят к хижине Хагрида, он играет на самодельной дудочке. В этот момент он играет тему Хедвиг (&#171;Hedwig`s Theme&#187;) из саундтрэка фильма.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* В качестве Хогвартского экспресса в этом и последующих фильмах был использован паровоз  GWR 4900 Class 5972 Olton Hall, принадлежавший когда-то британской компании  Great Western Railway и построенный в 1937 году. С 2004 года частный туристический оператор  Beyond Boundaries Travel организует на нём туры по Британии исключительно для фанатов Гарри Поттера. <br/>&nbsp;&nbsp;&nbsp;&nbsp;* На Кубке Квиддича помимо имени отца Гарри в числе прочих также выгравированы имена М. Макгонагалл и Р. Дж. Х. Кинг.  Второй персонаж получил свое имя в честь <a href=\"/name/2000075/\" class=\"all\">Джона Кинга</a>, отвечавшего за производство и использование декораций в проекте.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* В Трофейной комнате, справа от Кубка Квиддича можно увидеть награду &#171;за особые заслуги перед школой&#187;. При этом видна часть имени  Тома М. Реддла, выгравированного  на ней.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Среди портретов на исчезающей лестнице можно заметить картину с изображением английской королевы Анны Болейн, второй жены короля Генриха VIII и матери Елизаветы I. Многие современники Анны Болейн полагали, что она ведьма. <br/>&nbsp;&nbsp;&nbsp;&nbsp;* Использованная при съемках полосатая кошка пропала без следа и была найдена после усиленных поисков только через два дня. <br/>&nbsp;&nbsp;&nbsp;&nbsp;* Одним из дублёров <a href=\"/name/24216/\" class=\"all\">Робби Колтрейна</a> был бывший игрок английской национальной регбийной сборной <a href=\"/name/40823/\" class=\"all\">Мартин Бэйфилд</a>.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* В качестве Хедвиг, совы Гарри Поттера, были задействованы сразу три совы Оок, Гизмо и Спраут.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Николас Фламель, упомянутый как создатель философского камня, был в реальности известным алхимиком, по мнению некоторых из его  современников действительно создавший философский камень и смерть которого была окружена таинственными обстоятельствами. Существует легенда, что он жив до сих пор и в таком случае ему было бы примерно столько же лет как в фильме и книге.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* По книге ученики школы отправляются в Хогвартс с невидимой обычному взгляду платформы 9 &#190; на вокзале Кингс-Кросс, попасть на которую можно через барьер между платформами 9 и 10. Однако оказалось, что на вокзале Кингс-Кросс платформы 9 и 10 находятся не в главном здании, а в отдельном строении и их разделяет не барьер, а две железнодорожные колеи.  В одном из интервью <a href=\"/name/40777/\" class=\"all\">Дж. К. Роулинг</a> признала свою ошибку и сказала, что перепутала лондонские вокзалы Кингс-Кросс и Юстон, хотя и на втором вокзале платформы 9 и 10 также разделены колеями, а не барьером. Для съёмок фильма были использованы платформы 4 и 5, которые находятся в главной части вокзала Кинг-Кросс и были просто на время переименованы в 9 и 10. Впоследствии на стене здания,  где находятся реальные платформы 9 и 10 был помещён чугунный знак-указатель &#171;Платформа  9 &#190;&#187;, а под ним вмонтирована четверть багажной тележки, оставшаяся часть которой как бы уже исчезла в стене.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Актёр, сыгравший дежурного на вокзале Кингс-Кросс, к которому Гарри обращается за помощью, на самом деле работает служащим на британской железной дороге, правда, в другой должности: он начальник поезда.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Фамилия Дамблдора в переводе со староанглийского означает шмель. <br/>&nbsp;&nbsp;&nbsp;&nbsp;* Девиз школы волшебства и чародейства Хогвартс «Draco dormiens nunquam titillandus» означает в переводе с латыни «Никогда не буди спящего дракона». <br/>&nbsp;&nbsp;&nbsp;&nbsp;* Во время съёмок <a href=\"/name/40778/\" class=\"all\">Дэниэл Рэдклифф</a> решил подшутить над <a href=\"/name/24216/\" class=\"all\">Робби Колтрейном</a> и поменял интерфейс его мобильного телефона на турецкий. Актёр долгое время не мог разобраться  с этим, и вынужден был обратиться за помощью к отцу отвечавшего за грим и макияж Эйтту Феннелю, турку по происхождению.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Перед выходом фильма в прокат в продажу поступила одноимённая компьютерная игра, созданная студией Electronic Arts и ставшая большим хитом. Американская фирма по производству игрушек Mattel Inc. стала победителем соревнования за право производство игрушек, основанных на будущем фильме. Немногим позднее такое право также получил другой промышленный гигант в этой отрасли фирма Hasbro. <br/>&nbsp;&nbsp;&nbsp;&nbsp;* Композитор <a href=\"/name/225027/\" class=\"all\">Джон Уильямс</a> написал музыкальную композицию специально для трейлера фильма. Впоследствии она вошла в саундтрэк фильма под названием &#171;Пролог&#187; (The Prologue), что само по себе случается крайне редко.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Картина установила новый рекорд сборов премьерного уикенда в американском и британском прокате. В Америке касса уикенда составила 90 294 621 доллар, а в Британии 16 335 627 фунтов стерлингов. <br/>&nbsp;&nbsp;&nbsp;&nbsp;* Фильм стал самым кассовым фильмом года в США, собрав за всё время своего проката 317 575 550 долларов и повторил это же достижение в международном прокате заработав 658,9 млн. долларов. По итогам мирового проката фильм вышел на второе место за всю историю с суммой в 976,5 млн. долларов, уступая на тот момент одному лишь &#171;<a href=\"/film/2213/\" class=\"all\">Титанику</a>&#187; <a href=\"/name/27977/\" class=\"all\">Джеймса Кэмерона</a>.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* <a href=\"/name/4666/\" class=\"all\">Анна Попплуэлл</a> пробовалась на роль Гермионы Грейнджер.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Волшебные шахматы, в которые играют Гарри и Рон были созданы на основе шахмат с острова Льюис, самых известных шахматных фигур в мире, найденных в песке на пляже острова Льюис, Гебридские острова и датируемых XII-м веком.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* На роль Гарри Поттера пробовался актер <a href=\"/name/752/\" class=\"all\">Антон Ельчин</a>.<br/>");
        movie.PremiereDate.Should().HaveYear(2001).And.HaveMonth(11).And.HaveDay(04);
        movie.ProductionLocations.Should().HaveCount(2);
        movie.ProductionLocations.Should().Contain("Великобритания");
        movie.ProductionLocations.Should().Contain("США");
        movie.ProductionYear.Should().Be(2001);
        movie.ProviderIds.Should().HaveCount(3);
        movie.GetProviderId(Plugin.PluginKey).Should().Be("689");
        movie.GetProviderId(MetadataProviders.Imdb).Should().Be("tt0241527");
        movie.GetProviderId(MetadataProviders.Tmdb).Should().Be("671");
        movie.RemoteTrailers.Should().HaveCount(5);
        movie.Size.Should().Be(152);
        movie.SortName.Should().Be(movie.Name);
        movie.Studios.Should().HaveCount(3);
        movie.Tagline.Should().Be("Путешествие в твою мечту");

        VerifyMovieCommon(movie);
    }

    protected static void VerifyMovie17579(Movie movie)
    {
        movie.Should().NotBeNull();
        movie.Collections.Should().BeEmpty();
        movie.CommunityRating.Should().BeGreaterThan(8);
        movie.EndDate.Should().BeNull();
        movie.ExternalId.Should().Be("17579");
        movie.Genres.Should().HaveCount(6);
        movie.MediaType.Should().Be("Video");
        movie.Name.Should().Be("Робин Гуд");
        movie.OfficialRating.Should().Be("g");
        movie.OriginalTitle.Should().Be("Robin Hood");
        movie.Overview.Should().Be("Добро пожаловать в дремучий Шервудский лес, где ты встретишь храброго и забавного лисенка по имени Робин Гуд и его лучшего друга Крошку Джона - большого добродушного медведя.\n\nЭту веселую компанию давно разыскивает шериф Нотингема. Он готовит друзьям ловушку: на турнире лучников Робин Гуда будет ждать засада. Но отважный лисенок все равно собирается участвовать в состязании: ведь только там у него есть шанс увидеть красавицу Мариан.\n\nИ вот турнир начался, однако шериф никак не может узнать среди участников переодетого Робин Гуда. Правда, один точный выстрел способен сразу выдать самого лучшего стрелка в королевстве.<br/><br/><b>Интересное:</b><br/>&nbsp;&nbsp;&nbsp;&nbsp;* При создании «Робина Гуда» был использовано около 350 000 рисунков, более 100 000 листов целлулоидной бумаги и 800 фонов для мультфильма.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Из-за маленького бюджета фильма, аниматорам студии Дисней пришлось повторно использовать анимационные последовательности как из самого фильма, так и заимствованные из предыдущих фильмов Диснея, включая &#171;<a href=\"/film/551/\" class=\"all\">Белоснежку и семь гномов</a>&#187;, &#171;<a href=\"/film/8133/\" class=\"all\">Книгу Джунглей</a>&#187;, &#171;<a href=\"/film/26656/\" class=\"all\">Котов-аристократов</a>&#187;, &#171;<a href=\"/film/8231/\" class=\"all\">Золушку</a>&#187; и &#171;<a href=\"/film/19504/\" class=\"all\">Алису в Стране чудес</a>&#187;.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Знаменитой расщелиной в зубах <a href=\"/name/176055/\" class=\"all\">Терри-Томаса</a> (1911-1990) воспользовались для показа персонажа, которого актёр озвучил, сэра Хисса. Именно через эту расщелину изо рта змея выскакивал раздвоенный язык.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Изначально отца Тука планировали сделать боровом, но всё-таки сделали барсуком, чтобы избежать обвинений в оскорблении верующих. Шерифа Ноттингема сначала планировали сделать козлом, но сделали волком, т.к. антагонисты традиционно ассоциируются именно с волками.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* За несколько месяцев до выхода фильма в прокат аниматорам потребовалось, чтобы <a href=\"/name/20665/\" class=\"all\">Питер Устинов</a> (1921-2004) переозвучил некоторые реплики принца Джона. В поисках актёра аниматоры обзвонили Нью-Йорк, Лондон, Париж, Вену и Токио, а обнаружился Устинов в студии в Бербанке в Калифорнии, примерно в паре километров от них.<br/>");
        movie.PremiereDate.Should().HaveYear(1973).And.HaveMonth(11).And.HaveDay(08);
        movie.ProductionLocations.Should().ContainSingle();
        movie.ProductionLocations.Should().Contain("США");
        movie.ProductionYear.Should().Be(1973);
        movie.ProviderIds.Should().HaveCount(3);
        movie.GetProviderId(MetadataProviders.Imdb).Should().Be("tt0070608");
        movie.GetProviderId(MetadataProviders.Tmdb).Should().Be("11886");
        movie.GetProviderId(Plugin.PluginKey).Should().Be("17579");
        movie.RemoteTrailers.Should().HaveCount(5);
        movie.Size.Should().Be(83);
        movie.SortName.Should().Be(movie.Name);
        movie.Studios.Should().HaveCount(2);
        movie.Tagline.Should().Be("The way it REALLY happened...");

        VerifyMovieCommon(movie);
    }

    protected static void VerifyPerson29855(Person? person)
    {
        person.Should().NotBeNull();
        person!.Name.Should().Be("Джеки Чан");
        person.OriginalTitle.Should().Be("Jackie Chan");
        person.Overview.Should().Be("Настоящее имя — Чань Кунсан (Chan Kongsang), что означает &#171;Чан, рожденный в Гонконге&#187;.\nЧан делает самостоятельно большую часть трюков, а также иногда дублирует других актёров; он неоднократно получал травмы (во время финальных титров в его фильмах обычно демонстрируются неудавшиеся дубли), поэтому Чан внесён в чёрные списки страховых компаний по всему миру.\nУ Чана есть дочь Этта Нг (род. 19 ноября 1999) от внебрачной связи с актрисой Элейн Нг.\nЕго родители — Чарльз Чан и Ли-Ли Чан — бежали в Гонконг с континента во время гражданской войны, а в 1960 году перебрались в Австралию.\nВ возрасте 6 лет Чан был отдан в школу пекинской оперы в Гонконге.\nДважды попадал в Книгу рекордов Гиннесса за «наибольшее число трюков в карьере» и «самое большое число упоминаний в титрах одного фильма». Так, в фильме «<a href=\"/film/654749/\" class=\"all\">Доспехи Бога 3: Миссия Зодиак</a>» Джеки был задействован на 15 должностях (актёр, режиссёр, сценарист, продюсер, оператор, осветитель, постановщик трюков, исполнитель песни, ответственный за питание съёмочной группы и др.), тем самым побив рекорд <a href=\"/name/30966/\" class=\"all\">Роберта Родригеса</a>.");
        person.PremiereDate.Should().HaveYear(1954).And.HaveMonth(04).And.HaveDay(07);
        person.ProductionLocations.Should().ContainSingle();
        person.ProductionLocations.Should().Contain("Гонконг");
        person.ProviderIds.Should().HaveCount(1);
        person.GetProviderId(Plugin.PluginKey).Should().Be("29855");
        person.SortName.Should().Be(person.Name);
    }

    protected static void VerifyPersonInfo7987(PersonInfo? person)
    {
        person.Should().NotBeNull();
        person!.Id.Should().Be(0);
        person.ItemId.Should().Be(0);
        person.ImageUrl.Should().NotBeNullOrWhiteSpace();
        person.Name.Should().Be("Тим Роббинс");
        person.ProviderIds.Should().HaveCount(1);
        person.GetProviderId(Plugin.PluginKey).Should().Be("7987");
        person.Role.Should().Be("Andy Dufresne");
        person.Type.Should().Be(PersonType.Actor);
    }

    protected static void VerifyPersonInfo34549(PersonInfo? person)
    {
        person.Should().NotBeNull();
        person!.Id.Should().Be(0);
        person.ItemId.Should().Be(0);
        person.ImageUrl.Should().NotBeNullOrWhiteSpace();
        person.Name.Should().Be("Бенедикт Камбербэтч");
        person.ProviderIds.Should().HaveCount(1);
        person.GetProviderId(Plugin.PluginKey).Should().Be("34549");
        person.Role.Should().Be("Sherlock Holmes");
        person.Type.Should().Be(PersonType.Actor);
    }

    protected static void VerifyPersonInfo40779(PersonInfo? person)
    {
        person.Should().NotBeNull();
        person!.Id.Should().Be(0);
        person.ItemId.Should().Be(0);
        person.ImageUrl.Should().NotBeNullOrWhiteSpace();
        person.Name.Should().Be("Эмма Уотсон");
        person.ProviderIds.Should().HaveCount(1);
        person.GetProviderId(Plugin.PluginKey).Should().Be("40779");
        person.Role.Should().Be("Hermione Granger");
        person.Type.Should().Be(PersonType.Actor);
    }

    protected static void VerifyRemoteSearchResult326(RemoteSearchResult remoteSearchResult)
    {
        remoteSearchResult.Should().NotBeNull();
        remoteSearchResult.ImageUrl.Should().NotBeNullOrWhiteSpace();
        remoteSearchResult.Name.Should().Be("Побег из Шоушенка");
        remoteSearchResult.ProductionYear.Should().Be(1994);
        remoteSearchResult.Overview.Should().Be("Бухгалтер Энди Дюфрейн обвинён в убийстве собственной жены и её любовника. Оказавшись в тюрьме под названием Шоушенк, он сталкивается с жестокостью и беззаконием, царящими по обе стороны решётки. Каждый, кто попадает в эти стены, становится их рабом до конца жизни. Но Энди, обладающий живым умом и доброй душой, находит подход как к заключённым, так и к охранникам, добиваясь их особого к себе расположения.");
        remoteSearchResult.ProviderIds.Should().HaveCount(3);
        remoteSearchResult.GetProviderId(Plugin.PluginKey).Should().Be("326");
        remoteSearchResult.GetProviderId(MetadataProviders.Imdb).Should().Be("tt0111161");
        remoteSearchResult.GetProviderId(MetadataProviders.Tmdb).Should().Be("278");
        remoteSearchResult.SearchProviderName.Should().Be(Plugin.PluginKey);
    }

    protected static void VerifyRemoteSearchResult29855(RemoteSearchResult remoteSearchResult)
    {
        remoteSearchResult.Should().NotBeNull();
        remoteSearchResult.ImageUrl.Should().NotBeNullOrWhiteSpace();
        remoteSearchResult.Name.Should().Be("Джеки Чан");
        remoteSearchResult.PremiereDate.Should().HaveYear(1954).And.HaveMonth(04).And.HaveDay(07);
        remoteSearchResult.ProviderIds.Should().ContainSingle();
        remoteSearchResult.GetProviderId(Plugin.PluginKey).Should().Be("29855");
        remoteSearchResult.SearchProviderName.Should().Be(Plugin.PluginKey);
    }

    protected static void VerifyRemoteSearchResult_452973_1_2(RemoteSearchResult remoteSearchResult)
    {
        remoteSearchResult.Should().NotBeNull();
        remoteSearchResult.ImageUrl.Should().NotBeNullOrWhiteSpace();
        remoteSearchResult.IndexNumber.Should().Be(2);
        remoteSearchResult.ParentIndexNumber.Should().Be(1);
        remoteSearchResult.Name.Should().Be("Эпизод 2. А я сказал — оседлаю!");
        remoteSearchResult.Overview.Should().Be("Симон и Камина с Лаганном попадают в родную деревню Ёко, Ритону. Герои оказываются втянуты в войну, которые ведут живущие на поверхности люди и таинственные зверолюди. В бою с ними Камина захватывает ганмена одного из противников и нарекает «Гурреном».");
        remoteSearchResult.PremiereDate.Should().HaveYear(2007).And.HaveMonth(04).And.HaveDay(08);
        remoteSearchResult.ProviderIds.Should().ContainSingle();
        remoteSearchResult.GetProviderId(Plugin.PluginKey).Should().Be("452973");
        remoteSearchResult.ProductionYear.Should().Be(2007);
        remoteSearchResult.SearchProviderName.Should().Be(Plugin.PluginKey);
    }

    protected static void VerifyRemoteSearchResult502838(RemoteSearchResult remoteSearchResult)
    {
        remoteSearchResult.Should().NotBeNull();
        remoteSearchResult.ImageUrl.Should().NotBeNullOrWhiteSpace();
        remoteSearchResult.Name.Should().Be("Шерлок");
        remoteSearchResult.ProductionYear.Should().Be(2010);
        remoteSearchResult.ProviderIds.Should().HaveCount(3);
        remoteSearchResult.GetProviderId(Plugin.PluginKey).Should().Be("502838");
        remoteSearchResult.GetProviderId(MetadataProviders.Imdb).Should().Be("tt1475582");
        remoteSearchResult.GetProviderId(MetadataProviders.Tmdb).Should().Be("19885");
        remoteSearchResult.SearchProviderName.Should().Be(Plugin.PluginKey);
        remoteSearchResult.Overview.Should().Be("События разворачиваются в наши дни. Он прошел Афганистан, остался инвалидом. По возвращении в родные края встречается с загадочным, но своеобразным гениальным человеком. Тот в поиске соседа по квартире. Лондон, 2010 год. Происходят необъяснимые убийства. Скотланд-Ярд без понятия, за что хвататься. Существует лишь один человек, который в силах разрешить проблемы и найти ответы на сложные вопросы.");
    }

    protected static void VerifySeries502838(Series series)
    {
        series.Should().NotBeNull();
        series.Collections.Should().BeEmpty();
        series.CommunityRating.Should().BeGreaterThan(8);
        series.EndDate.Should().HaveYear(2017).And.HaveMonth(1).And.HaveDay(1);
        series.ExternalId.Should().Be("502838");
        series.Genres.Should().HaveCount(4);
        series.MediaType.Should().BeNull();
        series.IsFolder.Should().BeTrue();
        series.Name.Should().Be("Шерлок");
        series.OfficialRating.Should().BeNull();
        series.OriginalTitle.Should().Be("Sherlock");
        series.Overview.Should().Be("События разворачиваются в наши дни. Он прошел Афганистан, остался инвалидом. По возвращении в родные края встречается с загадочным, но своеобразным гениальным человеком. Тот в поиске соседа по квартире. Лондон, 2010 год. Происходят необъяснимые убийства. Скотланд-Ярд без понятия, за что хвататься. Существует лишь один человек, который в силах разрешить проблемы и найти ответы на сложные вопросы.<br/><br/><b>Интересное:</b><br/>&nbsp;&nbsp;&nbsp;&nbsp;* Сериал снят по мотивам произведений <a href=\"/name/70084/\" class=\"all\">Артура Конана Дойля</a> о Шерлоке Холмсе (изданных с 1887 по 1927 годы), однако действие происходит в наши дни.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Как и в произведениях <a href=\"/name/70084/\" class=\"all\">Артура Конана Дойла</a>, в сериале термин «дедукция» использован неверно. Дедукция — это частный случай умозаключения, переход от общего к частному. Все свои умозаключения Шерлок Холмс делает на индуктивных рассуждениях. Индукция — это метод рассуждения от частного к общему.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* <a href=\"/name/34549/\" class=\"all\">Бенедикт Камбербэтч</a> был единственным, кто прослушивался на роль Шерлока. <a href=\"/name/1070406/\" class=\"all\">Стивен Моффат</a> и <a href=\"/name/50871/\" class=\"all\">Марк Гейтисс</a> пригласили его на пробы после просмотра фильма &#171;<a href=\"/film/255611/\" class=\"all\">Искупление</a>&#187; (2007) с его участием. На пробах стало очевидно, что образ настолько ему подходит, что рассылать сценарий кому-либо еще не имеет смысла.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Квартира Холмса и Ватсона по адресу Бейкер-стрит, 221 Б, снималась на Норт-Гоуэр-стрит, 187 (187 North Gower Street) в лондонском районе Блумсбери. Для пилотного эпизода некоторые сцены снимались на Бейкер-стрит. Представитель Би-би-си объяснил отказ от съёмок на Бейкер-стрит тем, что на этой улице очень плотное автомобильное движение, а также тем, что там очень много предметов и вывесок с надписями &#171;Шерлок Холмс&#187;, в частности на доме 221 Б, которые бы пришлось убирать.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Банк «Shad Sanders» из серии «Слепой банкир» снимали в небоскрёбе «Debevoise & Plimpton», расположенном по адресу Old Broad Street, City of London, Greater London EC2N 1HQ.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* На роль Доктора Ватсона пробовался <a href=\"/name/607007/\" class=\"all\">Мэтт Смит</a> — до того, как его утвердили на главную роль в сериале «<a href=\"/film/252089/\" class=\"all\">Доктор Кто</a>» (2005).<br/>&nbsp;&nbsp;&nbsp;&nbsp;* По версии сериала Ватсон страдает от психосоматических болей в ноге при фактическом ранении в плечо. Это тонкая ирония на тему путаницы в оригинальных произведениях — в рассказах о Шерлоке Холмсе <a href=\"/name/70084/\" class=\"all\">Артур Конан Дойл</a> был весьма непоследователен относительно расположения раны доктора Ватсона.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Джим Мориарти использует мобильный телефон HTC Incredible S (можно отличить по необычной форме задней крышки и двойной вспышке). Что примечательно, в одной из сцен первой серии второго сезона он держит его вверх тормашками (камера и вспышка у телефона располагаются сверху, однако, когда он получает сообщение и достаёт телефон, ни камеры, ни вспышки в верхней части корпуса нет, но есть характерный изгиб крышки, который у телефона присутствует снизу).<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Родителей Шерлока сыграли родители <a href=\"/name/34549/\" class=\"all\">Бенедикта Камбербэтча</a>.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* В последней серии третьего сезона &laquo;Его последний обет&raquo; роль маленького Шерлока сыграл Луис Моффат &#8211; сын продюсера и сценариста сериала <a href=\"/name/1070406/\" class=\"all\">Стивена Моффата</a> и исполнительного продюсера <a href=\"/name/918481/\" class=\"all\">Сью Верчью</a>.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* В основу имени соперника Шерлока Холмса — шведского медиамагната Чарльза Магнуссона была взята реально существовавшая личность. Магнуссон (1879-1948) был владельцем нескольких газет, продюсером, и основателем киностудии Svensk Filmindustri в 1919 году.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Персонаж Грега Лестрейда является комбинацией инспекторов Грегсона и Лестрейда. В книгах имя последнего начинается на букву «г», но так и остается тайной. Именно поэтому Шерлок постоянно забывает имя Лестрейда.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Персонаж Молли Хупер, которого не было ни в одной из книг о Шерлоке Холмсе, задумывался как персонаж &#171;на один эпизод&#187;, чтобы еще больше подчеркнуть некоммуникабельность Шерлока, особенно его неумение общаться с женщинами. Однако Стивену Моффату и другим продюсерам так понравилась игра <a href=\"/name/905716/\" class=\"all\">Луизы Брили</a>, что они решили вписать ее персонажа в основную сюжетную линию сериала.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Шерлок иногда использует прием мнемотехники под названием «Дворец памяти» или «Чертоги разума». Это не придумка сценаристов, а реальный мнемонический метод, уходящий своими корнями во времена древнего Рима.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* В названиях некоторых эпизодов сериала обыгрываются заголовки рассказов <a href=\"/name/70084/\" class=\"all\">Артура Конана Дойля</a>, например: &#171;Этюд в багровых тонах&#187; превратился в &#171;Этюд в розовых тонах&#187;, &#171;Скандал в Богемии&#187; в &#171;Скандал в Белгравии&#187;, &#171;Знак четырех&#187; в &#171;Знак трех&#187; и так далее.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Готовясь к роли Холмса, <a href=\"/name/34549/\" class=\"all\">Бенедикт Камбербэтч</a> прочитал полное собрание сочинений <a href=\"/name/70084/\" class=\"all\">Конана Дойля</a> о знаменитом сыщике.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Пальто, которое <a href=\"/name/34549/\" class=\"all\">Бенедикт Камбербэтч</a> носил в пилотном эпизоде, было выкуплено <a href=\"/name/50871/\" class=\"all\">Марком Гейтиссом</a> и позже подарено им актеру на день рождения.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* В перерыве между съемками первого и второго сезонов <a href=\"/name/33496/\" class=\"all\">Мартин Фриман</a> и <a href=\"/name/34549/\" class=\"all\">Бенедикт Камбербэтч</a> были утверждены на роли в трилогии Питера Джексона &#171;Хоббит&#187;. В этот раз уже Фриман исполнил заглавную роль Бильбо Бэггинса, а Камбербэтч довольствовался озвучкой дракона Смауга и Некроманта.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Непосредственно перед первыми пробами на роль в сериале у <a href=\"/name/33496/\" class=\"all\">Мартина Фримана</a> украли бумажник. Он пришел в плохом настроении, и у продюсеров сложилось впечатление, что роль ему неинтересна. К счастью, ко вторым пробам актер уже отошел и заполучил роль.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Как и в произведениях <a href=\"/name/70084/\" class=\"all\">Конана Дойля</a> в сериале Ватсон проходил военную службу в Афганистане.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* По своей внешности Джим Мориарти представляет собой легкую противоположность Шерлока: у Шерлока длинные вьющиеся волосы, а Мориарти идеально причесан; Шерлок носит рубашку с расстегнутым воротником, а Мориарти всегда при галстуке; Шерлок носит классическое пальто с поднятым высоким воротником, а Мориарти – более короткий хорошо подогнанный френч со стоячим воротником а-ля Неру; Шерлок в основном идеально выбрит, а Мориарти щеголяет легкой щетиной.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* В 2011 году <a href=\"/name/41816/\" class=\"all\">Дэнни Бойл</a> поставил спектакль &#171;Франкенштейн&#187; в Королевском национальном театре, в котором главные роли исполнили <a href=\"/name/34549/\" class=\"all\">Бенедикт Камбербэтч</a> и <a href=\"/name/14843/\" class=\"all\">Джонни Ли Миллер</a>, попеременно играющие Виктора Франкенштейна и его чудовище. Оба актера сыграли другого персонажа викторианской эпохи Шерлока Холмса, живущего в наши дни, но по разные стороны Атлантического океана: первый в сериале &#171;<a href=\"/film/502838/\" class=\"all\">Шерлок</a>&#187; (2010), а второй в сериале &#171;<a href=\"/film/661210/\" class=\"all\">Элементарно</a>&#187; (2012).<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Начальные титры и подписи в сериале выполнены шрифтом Johnston Sans, известным своим использованием в лондонском метро.<br/>&nbsp;&nbsp;&nbsp;&nbsp;* Изначально планировалось, что каждый сезон будет состоять из 6 эпизодов (вместо 3) продолжительностью 60 минут каждый (вместо 90).<br/>&nbsp;&nbsp;&nbsp;&nbsp;* <a href=\"/name/34549/\" class=\"all\">Бенедикт Камбербэтч</a>, <a href=\"/name/33496/\" class=\"all\">Мартин Фриман</a> и <a href=\"/name/14435/\" class=\"all\">Тоби Джонс</a> появлялись в фильмах Кинематографической вселенной Marvel: Камбербэтч сыграл доктора Стивена Стрэнджа в &#171;<a href=\"/film/409600/\" class=\"all\">Докторе Стрэндже</a>&#187; (2016), Фриман &#8211; Эверетта Росса в фильме &#171;<a href=\"/film/822708/\" class=\"all\">Первый мститель: Противостояние</a>&#187; (2016), а Джонс &#8211; доктора Арнима Золя в картинах &#171;<a href=\"/film/160946/\" class=\"all\">Первый мститель</a>&#187; (2011) и &#171;<a href=\"/film/676266/\" class=\"all\">Первый мститель: Другая война</a>&#187; (2014).<br/>&nbsp;&nbsp;&nbsp;&nbsp;* <a href=\"/name/34549/\" class=\"all\">Бенедикт Камбербэтч</a> снимался в трилогии <a href=\"/name/32383/\" class=\"all\">Питера Джексона</a> &#171;Хоббит&#187; вместе с <a href=\"/name/21465/\" class=\"all\">Кристофером Ли</a> и <a href=\"/name/8215/\" class=\"all\">Иэном МакКелленом</a>. Всех троих объединяет то, что они изображали Шерлока Холмса.<br/>");
        series.PremiereDate.Should().HaveYear(2010).And.HaveMonth(07).And.HaveDay(22);
        series.ProductionLocations.Should().HaveCount(2);
        series.ProductionLocations.Should().Contain("Великобритания");
        series.ProductionLocations.Should().Contain("США");
        series.ProductionYear.Should().Be(2010);
        series.ProviderIds.Should().HaveCount(3);
        series.GetProviderId(Plugin.PluginKey).Should().Be("502838");
        series.GetProviderId(MetadataProviders.Imdb).Should().Be("tt1475582");
        series.GetProviderId(MetadataProviders.Tmdb).Should().Be("19885");
        series.RemoteTrailers.Should().HaveCount(2);
        series.Size.Should().Be(0);
        series.SortName.Should().Be(series.Name);
        series.Studios.Should().ContainSingle();
        series.Studios.Should().Contain("Hartswood Films");
        series.Tagline.Should().Be("Лучший сыщик 21-го века");

        VerifySeriesCommon(series);
    }

    private static void VerifyMovieCommon(Movie movie)
    {
        movie.Album.Should().BeNull();
        movie.AlbumId.Should().Be(0);
        movie.AudioStreamIndex.Should().BeNull();
        movie.Container.Should().BeNull();
        movie.ContainingFolderPath.Should().BeNull();
        movie.CriticRating.Should().BeNull();
        movie.CustomRating.Should().BeNull();
        movie.DateCreated.Should().Be(DateTimeOffset.MinValue);
        movie.DateLastRefreshed.Should().Be(DateTimeOffset.MinValue);
        movie.DateLastSaved.Should().Be(DateTimeOffset.MinValue);
        movie.DateModified.Should().Be(DateTimeOffset.MinValue);
        movie.DisplayParent.Should().BeNull();
        movie.DisplayParentId.Should().Be(0);
        movie.FileName.Should().BeNull();
        movie.FileNameWithoutExtension.Should().BeNull();
        movie.HasPathProtocol.Should().BeFalse();
        movie.ImportedCollections.Should().BeNull();
        movie.IndexNumber.Should().BeNull();
        movie.InternalId.Should().Be(0);
        movie.Is3D.Should().BeFalse();
        movie.IsDisplayedAsFolder.Should().BeFalse();
        movie.IsFileProtocol.Should().BeFalse();
        movie.IsFolder.Should().BeFalse();
        movie.IsHD.Should().BeFalse();
        movie.IsHidden.Should().BeFalse();
        movie.IsInMixedFolder.Should().BeFalse();
        movie.IsLocked.Should().BeFalse();
        movie.IsPlaceHolder.Should().BeFalse();
        movie.IsResolvedToFolder.Should().BeFalse();
        movie.IsSecondaryMergedItemInSameFolder.Should().BeFalse();
        movie.IsShortcut.Should().BeFalse();
        movie.IsThemeMedia.Should().BeFalse();
        movie.IsTopParent.Should().BeFalse();
        movie.IsVirtualItem.Should().BeFalse();
        movie.LocationType.Should().Be(LocationType.FileSystem);
        movie.Parent.Should().BeNull();
        movie.ParentId.Should().Be(0);
        movie.Path.Should().BeNull();
        movie.PathProtocol.Should().BeNull();
        movie.PrimaryImagePath.Should().BeNull();
    }

    private static void VerifySeriesCommon(Series series)
    {
        series.Album.Should().BeNull();
        series.AlbumId.Should().Be(0);
        series.AudioStreamIndex.Should().BeNull();
        series.Container.Should().BeNull();
        series.ContainingFolderPath.Should().BeNull();
        series.CriticRating.Should().BeNull();
        series.CustomRating.Should().BeNull();
        series.DateCreated.Should().Be(DateTimeOffset.MinValue);
        series.DateLastRefreshed.Should().Be(DateTimeOffset.MinValue);
        series.DateLastSaved.Should().Be(DateTimeOffset.MinValue);
        series.DateModified.Should().Be(DateTimeOffset.MinValue);
        series.DisplayParent.Should().BeNull();
        series.DisplayParentId.Should().Be(0);
        series.FileName.Should().BeNull();
        series.FileNameWithoutExtension.Should().BeNull();
        series.HasPathProtocol.Should().BeFalse();
        series.ImportedCollections.Should().BeNull();
        series.IndexNumber.Should().BeNull();
        series.InternalId.Should().Be(0);
        series.IsDisplayedAsFolder.Should().BeTrue();
        series.IsFileProtocol.Should().BeFalse();
        series.IsHD.Should().BeFalse();
        series.IsHidden.Should().BeFalse();
        series.IsInMixedFolder.Should().BeFalse();
        series.IsLocked.Should().BeFalse();
        series.IsPlaceHolder.Should().BeFalse();
        series.IsResolvedToFolder.Should().BeTrue();
        series.IsSecondaryMergedItemInSameFolder.Should().BeFalse();
        series.IsShortcut.Should().BeFalse();
        series.IsThemeMedia.Should().BeFalse();
        series.IsTopParent.Should().BeFalse();
        series.IsVirtualItem.Should().BeFalse();
        series.LocationType.Should().Be(LocationType.FileSystem);
        series.Parent.Should().BeNull();
        series.ParentId.Should().Be(0);
        series.Path.Should().BeNull();
        series.PathProtocol.Should().BeNull();
        series.PrimaryImagePath.Should().BeNull();
    }

    #endregion
}
