using System.Globalization;

using EmbyKinopoiskRu.Configuration;
using EmbyKinopoiskRu.Provider.LocalMetadata;

using FluentAssertions;

using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Querying;

namespace EmbyKinopoiskRu.Tests.KinopoiskApiUnofficial;

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
        _pluginConfiguration.Token = GetKinopoiskUnofficialToken();
        _pluginConfiguration.ApiType = PluginConfiguration.KinopoiskAPIUnofficialTech;

        SetupLibraryManager();

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
    private void SetupLibraryManager()
    {
        ConfigLibraryManager();

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
                !query.Recursive
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
                !query.Recursive
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
                !query.Recursive
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

        result.HasMetadata.Should().BeTrue("that mean the item was found");
        result.Item.Should().NotBeNull("that mean the item was found");
        result.Item.GetProviderId(Plugin.PluginKey).Should().Be("326", "id of the requested item");
        result.Item.MediaType.Should().Be("Video", "this is video");

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
            .Returns("UN_KpLocalMetadata_WithNameOnly_SingleResult");

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

        result.HasMetadata.Should().BeTrue("that mean the item was found");
        Movie movie = result.Item;
        movie.Should().NotBeNull("that mean the movie was found");
        movie.GetProviderId(Plugin.PluginKey).Should().Be("326", "id of the requested item");
        movie.GetProviderId(MetadataProviders.Imdb).Should().Be("tt0111161", "IMDB id of the requested item");
        movie.MediaType.Should().Be("Video", "this is video");
        movie.CommunityRating.Should().BeGreaterThan(5, "such value received from API");
        movie.ExternalId.Should().Be("326", "KP id of requested item");
        movie.Genres.Should().ContainSingle();
        movie.Genres[0].Should().Be("драма", "the film has only this genre");
        movie.Name.Should().Be("Побег из Шоушенка", "this is the name of the movie");
        movie.OfficialRating.Should().Be("r", "this is film's OfficialRating");
        movie.OriginalTitle.Should().Be("The Shawshank Redemption", "this is the original name of the movie");
        movie.Overview.Should().Be("Бухгалтер Энди Дюфрейн обвинён в убийстве собственной жены и её любовника. Оказавшись в тюрьме под названием Шоушенк, он сталкивается с жестокостью и беззаконием, царящими по обе стороны решётки. Каждый, кто попадает в эти стены, становится их рабом до конца жизни. Но Энди, обладающий живым умом и доброй душой, находит подход как к заключённым, так и к охранникам, добиваясь их особого к себе расположения.", "this is film's Overview");
        movie.ProductionYear.Should().Be(1994, "this is movie ProductionYear");
        movie.RemoteTrailers.Should().BeEmpty("the film doesn't have RemoteTrailers");
        movie.Size.Should().Be(142, "this is movie Size");
        movie.SortName.Should().Be(movie.Name, "SortName should be equal to Name");
        movie.Studios.Should().BeEmpty("the movie doesn't have Studios");
        movie.Tagline.Should().Be("Страх - это кандалы. Надежда - это свобода", "this is a Tagline of the movie");

        _logManager.Verify(lm => lm.GetLogger(It.IsAny<string>()), Times.Exactly(5));
        _fileSystem.Verify(fs => fs.GetDirectoryName("/emby/movie_library/Побег из Шоушенка.mkv"), Times.Once());
        _applicationPaths.VerifyGet(ap => ap.PluginConfigurationsPath, Times.Once());
        _xmlSerializer.Verify(xs => xs.DeserializeFromFile(typeof(PluginConfiguration), "UN_KpLocalMetadata_WithNameOnly_SingleResult/EmbyKinopoiskRu.xml"), Times.Once());
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
            .Returns("UN_KpLocalMetadata_WithNameOnly_MultiResult");

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

        result.HasMetadata.Should().BeTrue("that mean the item was found");
        Movie movie = result.Item;
        movie.Should().NotBeNull("that mean the movie was found");
        movie.CommunityRating.Should().BeGreaterThan(8, "such value received from API");
        movie.ExternalId.Should().Be("17579", "KP id of requested item");
        movie.Genres.Should().HaveCount(6);
        movie.MediaType.Should().Be("Video", "this is video");
        movie.Name.Should().Be("Робин Гуд", "this is the name of the movie");
        movie.OfficialRating.Should().Be("g", "this is film's OfficialRating");
        movie.OriginalTitle.Should().Be("Robin Hood", "this is the original name of the movie");
        movie.Overview.Should().Be("Добро пожаловать в дремучий Шервудский лес, где ты встретишь храброго и забавного лисенка по имени Робин Гуд и его лучшего друга Крошку Джона - большого добродушного медведя.\n\nЭту веселую компанию давно разыскивает шериф Нотингема. Он готовит друзьям ловушку: на турнире лучников Робин Гуда будет ждать засада. Но отважный лисенок все равно собирается участвовать в состязании: ведь только там у него есть шанс увидеть красавицу Мариан.\n\nИ вот турнир начался, однако шериф никак не может узнать среди участников переодетого Робин Гуда. Правда, один точный выстрел способен сразу выдать самого лучшего стрелка в королевстве.", "this is film's Overview");
        movie.ProductionYear.Should().Be(1973, "this is movie ProductionYear");
        movie.GetProviderId(Plugin.PluginKey).Should().Be("17579", "id of the requested item");
        movie.GetProviderId(MetadataProviders.Imdb).Should().Be("tt0070608", "IMDB id of the requested item");
        movie.RemoteTrailers.Should().BeEmpty("the film doesn't have RemoteTrailers");
        movie.Size.Should().Be(83, "this is movie Size");
        movie.SortName.Should().Be(movie.Name, "SortName should be equal to Name");
        movie.Studios.Should().BeEmpty("the movie doesn't have Studios");
        movie.Tagline.Should().Be("The way it REALLY happened...", "this is a Tagline of the movie");

        _logManager.Verify(lm => lm.GetLogger(It.IsAny<string>()), Times.Exactly(5));
        _fileSystem.Verify(fs => fs.GetDirectoryName("/emby/movie_library/Робин Гуд.mkv"), Times.Once());
        _applicationPaths.VerifyGet(ap => ap.PluginConfigurationsPath, Times.Once());
        _xmlSerializer.Verify(xs => xs.DeserializeFromFile(typeof(PluginConfiguration), "UN_KpLocalMetadata_WithNameOnly_MultiResult/EmbyKinopoiskRu.xml"), Times.Once());
        _localizationManager.Verify(lm => lm.RemoveDiacritics("Робин Гуд"), Times.Exactly(6));
        VerifyNoOtherCalls();

        Logger.Info($"Finished '{nameof(KpLocalMetadata_WithNameOnly_MultiResult)}'");
    }

    [Fact]
    public async void KpLocalMetadata_WithNameYear()
    {
        Logger.Info($"Start '{nameof(KpLocalMetadata_WithNameYear)}'");

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
                !query.Recursive
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
            .Setup(m => m.DeserializeFromFile(typeof(LibraryOptions), "UN_KpLocalMetadata_WithNameYear/options.xml"))
            .Returns(boxsetLibraryOptions);

        _ = _applicationPaths
            .SetupGet(m => m.PluginConfigurationsPath)
            .Returns("UN_KpLocalMetadata_WithNameYear");

        _ = _libraryManager
            .Setup(m => m.GetItemById(It.Is<long>(id => id == 1L)))
            .Returns(new CollectionFolder()
            {
                Name = "Collections",
                Path = "UN_KpLocalMetadata_WithNameYearAndAddToExistingCollection"
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
        MetadataResult<Movie> result = await _kpMovieLocalMetadata.GetMetadata(itemInfo, _movieLibraryOptions, _directoryService.Object, cancellationTokenSource.Token);


        result.HasMetadata.Should().BeTrue("that mean the item was found");
        Movie movie = result.Item;
        movie.Should().NotBeNull("that mean the movie was found");
        movie.CommunityRating.Should().BeGreaterThan(8, "such value received from API");
        movie.ExternalId.Should().Be("689", "KP id of requested item");
        movie.Genres.Should().HaveCount(3);
        movie.MediaType.Should().Be("Video", "this is video");
        movie.Name.Should().Be("Гарри Поттер и философский камень", "this is the name of the movie");
        movie.OfficialRating.Should().Be("pg", "this is film's OfficialRating");
        movie.OriginalTitle.Should().Be("Harry Potter and the Sorcerer's Stone", "this is the original name of the movie");
        movie.Overview.Should().Be("Жизнь десятилетнего Гарри Поттера нельзя назвать сладкой: родители умерли, едва ему исполнился год, а от дяди и тёти, взявших сироту на воспитание, достаются лишь тычки да подзатыльники. Но в одиннадцатый день рождения Гарри всё меняется. Странный гость, неожиданно появившийся на пороге, приносит письмо, из которого мальчик узнаёт, что на самом деле он - волшебник и зачислен в школу магии под названием Хогвартс. А уже через пару недель Гарри будет мчаться в поезде Хогвартс-экспресс навстречу новой жизни, где его ждут невероятные приключения, верные друзья и самое главное — ключ к разгадке тайны смерти его родителей.", "this is film's Overview");
        movie.ProductionYear.Should().Be(2001, "this is movie ProductionYear");
        movie.GetProviderId(Plugin.PluginKey).Should().Be("689", "id of the requested item");
        movie.GetProviderId(MetadataProviders.Imdb).Should().Be("tt0241527", "IMDB id of the requested item");
        movie.RemoteTrailers.Should().BeEmpty("the film doesn't have RemoteTrailers");
        movie.Size.Should().Be(152, "this is movie Size");
        movie.SortName.Should().Be(movie.Name, "SortName should be equal to Name");
        movie.Studios.Should().BeEmpty("the movie doesn't have Studios");
        movie.Tagline.Should().Be("Путешествие в твою мечту", "this is a Tagline of the movie");

        _logManager.Verify(lm => lm.GetLogger(It.IsAny<string>()), Times.Exactly(5));
        _fileSystem.Verify(fs => fs.GetDirectoryName("/emby/movie_library/Гарри Поттер и философский камень (2001).mkv"), Times.Once());
        _applicationPaths.VerifyGet(ap => ap.PluginConfigurationsPath, Times.Once());
        _xmlSerializer.Verify(xs => xs.DeserializeFromFile(typeof(PluginConfiguration), "UN_KpLocalMetadata_WithNameYear/EmbyKinopoiskRu.xml"), Times.Once());
        _localizationManager.Verify(lm => lm.RemoveDiacritics("Гарри Поттер и философский камень"), Times.Once());
        VerifyNoOtherCalls();

        Logger.Info($"Finished '{nameof(KpLocalMetadata_WithNameYear)}'");
    }

    [Fact]
    public void KpLocalMetadata_ForCodeCoverage()
    {
        Logger.Info($"Start '{nameof(KpLocalMetadata_ForCodeCoverage)}'");

        _kpMovieLocalMetadata.Name.Should().NotBeNull("name is hardcoded");
        _ = new KpSeriesLocalMetadata(_logManager.Object);

        Logger.Info($"Finish '{nameof(KpLocalMetadata_ForCodeCoverage)}'");
    }

}
