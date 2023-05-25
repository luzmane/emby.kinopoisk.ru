using EmbyKinopoiskRu.Configuration;
using EmbyKinopoiskRu.ScheduledTasks;
using EmbyKinopoiskRu.Tests.EmbyMock;

using MediaBrowser.Controller.Collections;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Querying;

namespace EmbyKinopoiskRu.Tests.KinopoiskDev;

[Collection("Sequential")]
public class CreateTop250MovieCollectionsTaskTest : BaseTest
{
    private static readonly NLog.ILogger Logger = NLog.LogManager.GetLogger(nameof(KpEpisodeProviderTest));

    private readonly CreateTop250MovieCollectionsTask _createTop250MovieCollectionsTask;


    #region Test configs
    public CreateTop250MovieCollectionsTaskTest() : base(Logger)
    {
        _pluginConfiguration.Token = KINOPOISK_DEV_TOKEN;

        ConfigLibraryManager();

        ConfigXmlSerializer();

        _createTop250MovieCollectionsTask = new(
            _logManager.Object,
            _libraryManager.Object,
            _collectionManager.Object,
            _jsonSerializer,
            _serverConfigurationManager.Object);
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
    public void CreateTop250MovieCollectionsTask_ForCodeCoverage()
    {
        Logger.Info($"Start '{nameof(CreateTop250MovieCollectionsTask_ForCodeCoverage)}'");

        Assert.False(_createTop250MovieCollectionsTask.IsHidden);
        Assert.False(_createTop250MovieCollectionsTask.IsEnabled);
        Assert.True(_createTop250MovieCollectionsTask.IsLogged);
        Assert.NotNull(_createTop250MovieCollectionsTask.Key);

        Assert.Empty(_createTop250MovieCollectionsTask.GetDefaultTriggers());

        _logManager.Verify(lm => lm.GetLogger("KinopoiskRu"), Times.Once());
        _logManager.Verify(lm => lm.GetLogger("CreateTop250MovieCollectionsTask"), Times.Once());

        VerifyNoOtherCalls();

        Logger.Info($"Finished '{nameof(CreateTop250MovieCollectionsTask_ForCodeCoverage)}'");
    }

    [Fact]
    public void CreateTop250MovieCollectionsTask_GetTranslation_RU()
    {
        Logger.Info($"Start '{nameof(CreateTop250MovieCollectionsTask_GetTranslation_RU)}'");

        _ = _serverConfigurationManager
            .SetupGet(scm => scm.Configuration)
            .Returns(new ServerConfiguration() { UICulture = "ru" });

        Assert.Equal("Создать коллекцию фильмов топ 250 Кинопоиска", _createTop250MovieCollectionsTask.Name);
        Assert.Equal("Создать коллекцию фильмов основываясь на топ 250 Кинопоиска. Поддерживает только kinopoisk.dev", _createTop250MovieCollectionsTask.Description);
        Assert.Equal("Плагин Кинопоиска", _createTop250MovieCollectionsTask.Category);

        _logManager.Verify(lm => lm.GetLogger("KinopoiskRu"), Times.Once());
        _logManager.Verify(lm => lm.GetLogger("CreateTop250MovieCollectionsTask"), Times.Once());
        _serverConfigurationManager.VerifyGet(scm => scm.Configuration, Times.Exactly(6));
        VerifyNoOtherCalls();

        Logger.Info($"Finished '{nameof(CreateTop250MovieCollectionsTask_GetTranslation_RU)}'");
    }

    [Fact]
    public void CreateTop250MovieCollectionsTask_GetTranslation_EnUs()
    {
        Logger.Info($"Start '{nameof(CreateTop250MovieCollectionsTask_GetTranslation_EnUs)}'");

        _ = _serverConfigurationManager
            .SetupGet(scm => scm.Configuration)
            .Returns(new ServerConfiguration() { UICulture = "en-us" });

        Assert.Equal("Create Top250 Movie collection from Kinopoisk", _createTop250MovieCollectionsTask.Name);
        Assert.Equal("Create a movie collection based on the top 250 list from Kinopoisk.ru. Support kinopoisk.dev only", _createTop250MovieCollectionsTask.Description);
        Assert.Equal("Kinopoisk Plugin", _createTop250MovieCollectionsTask.Category);

        _logManager.Verify(lm => lm.GetLogger("KinopoiskRu"), Times.Once());
        _logManager.Verify(lm => lm.GetLogger("CreateTop250MovieCollectionsTask"), Times.Once());
        _serverConfigurationManager.VerifyGet(scm => scm.Configuration, Times.Exactly(6));
        VerifyNoOtherCalls();

        Logger.Info($"Finished '{nameof(CreateTop250MovieCollectionsTask_GetTranslation_EnUs)}'");
    }

    [Fact]
    public void CreateTop250MovieCollectionsTask_GetTranslation_UK()
    {
        Logger.Info($"Start '{nameof(CreateTop250MovieCollectionsTask_GetTranslation_UK)}'");

        _ = _serverConfigurationManager
            .SetupGet(scm => scm.Configuration)
            .Returns(new ServerConfiguration() { UICulture = "uk" });

        Assert.Equal("Створити колекцію фільмів топ 250 Кінопошуку", _createTop250MovieCollectionsTask.Name);
        Assert.Equal("Створити колекцію фільмів ґрунтуючись на топ 250 Кінопошуку. Підтримує лише kinopoisk.dev", _createTop250MovieCollectionsTask.Description);
        Assert.Equal("Плагін Кінопошуку", _createTop250MovieCollectionsTask.Category);

        _logManager.Verify(lm => lm.GetLogger("KinopoiskRu"), Times.Once());
        _logManager.Verify(lm => lm.GetLogger("CreateTop250MovieCollectionsTask"), Times.Once());
        _serverConfigurationManager.VerifyGet(scm => scm.Configuration, Times.Exactly(6));
        VerifyNoOtherCalls();

        Logger.Info($"Finished '{nameof(CreateTop250MovieCollectionsTask_GetTranslation_UK)}'");
    }

    [Fact]
    public void CreateTop250MovieCollectionsTask_GetTranslation_BG()
    {
        Logger.Info($"Start '{nameof(CreateTop250MovieCollectionsTask_GetTranslation_BG)}'");

        _ = _serverConfigurationManager
            .SetupGet(scm => scm.Configuration)
            .Returns(new ServerConfiguration() { UICulture = "bg" });

        Assert.Equal("Create Top250 Movie collection from Kinopoisk", _createTop250MovieCollectionsTask.Name);
        Assert.Equal("Create a movie collection based on the top 250 list from Kinopoisk.ru. Support kinopoisk.dev only", _createTop250MovieCollectionsTask.Description);
        Assert.Equal("Kinopoisk Plugin", _createTop250MovieCollectionsTask.Category);

        _logManager.Verify(lm => lm.GetLogger("KinopoiskRu"), Times.Once());
        _logManager.Verify(lm => lm.GetLogger("CreateTop250MovieCollectionsTask"), Times.Once());
        _serverConfigurationManager.VerifyGet(scm => scm.Configuration, Times.Exactly(6));
        VerifyNoOtherCalls();

        Logger.Info($"Finished '{nameof(CreateTop250MovieCollectionsTask_GetTranslation_BG)}'");
    }

    [Fact]
    public async void CreateTop250MovieCollectionsTask_Execute_CollectionExists()
    {
        Logger.Info($"Start '{nameof(CreateTop250MovieCollectionsTask_Execute_CollectionExists)}'");

        _ = _applicationPaths
            .SetupGet(m => m.PluginConfigurationsPath)
            .Returns("CreateTop250MovieCollectionsTask_Execute_CollectionExists");

        LibraryOptions collectionLibraryOptions = new()
        {
            ContentType = CollectionType.Movies.ToString(),
            EnableAdultMetadata = true,
            ImportCollections = true,
            MetadataCountryCode = "RU",
            MinCollectionItems = 1,
            Name = PluginConfiguration.DefaultTop250MovieCollectionName,
            PathInfos = new MediaPathInfo[]{
                        new MediaPathInfo()
                        {
                            NetworkPath = null,
                            Path = "/emby/film_library"
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
        _ = _xmlSerializer
            .Setup(m => m.DeserializeFromFile(typeof(LibraryOptions), It.IsAny<string>()))
            .Returns(collectionLibraryOptions);

        _ = _libraryManager
            .Setup(m => m.QueryItems(It.Is<InternalItemsQuery>(query =>
                query.Recursive == false
                && query.IncludeItemTypes.Length == 1
                && nameof(CollectionFolder).Equals(query.IncludeItemTypes[0], StringComparison.Ordinal))))
            .Returns(new QueryResult<BaseItem>()
            {
                Items = new BaseItem[] {
                    new CollectionFolder()
                    {
                        Name = "My Movies",
                        Path = "/emby/film_library",
                        InternalId = 123L
                    }
                }
            });

        _ = _libraryManager
            .Setup(m => m.QueryItems(It.Is<InternalItemsQuery>(query =>
                query.Recursive == false
                && query.IsVirtualItem == false
                && query.IncludeItemTypes.Length == 1
                && nameof(Movie).Equals(query.IncludeItemTypes[0], StringComparison.Ordinal)
                && query.ParentIds.Length == 1
                && query.ParentIds[0] == 123L)))
            .Returns(new QueryResult<BaseItem>()
            {
                Items = new BaseItem[] {
                     new Movie() {
                        Name = "Гарри Поттер и Кубок огня",
                        InternalId = 103L,
                        Path = "/tmp/103",
                        ProviderIds = new(new Dictionary<string, string>()
                        {
                            { Plugin.PluginKey, "535341" },
                        })
                    },
                    new Movie() {
                        Name = "Гарри Поттер и Орден Феникса",
                        InternalId = 104L,
                        Path = "/tmp/104",
                        ProviderIds = new(new Dictionary<string, string>()
                        {
                            { MetadataProviders.Tmdb.ToString(), "522627" },
                        })
                    },
                    new Movie() {
                        Name = "Гарри Поттер и Принц-полукровка",
                        InternalId = 105L,
                        Path = "/tmp/105",
                        ProviderIds = new(new Dictionary<string, string>()
                        {
                            { MetadataProviders.Imdb.ToString(), "tt0993846" }
                        })
                    },
                    new Movie() {
                        Name = "Гарри Поттер и Дары Смерти: Часть I",
                        InternalId = 106L,
                        Path = "/tmp/106",
                        ProviderIds = new(new Dictionary<string, string>()
                        {
                            { Plugin.PluginKey, "41519" },
                            { MetadataProviders.Tmdb.ToString(), "20992" },
                            { MetadataProviders.Imdb.ToString(), "tt0118767" }
                        }),
                    },

                }
            });

        _ = _libraryManager
            .Setup(m => m.QueryItems(It.Is<InternalItemsQuery>(query =>
                query.Recursive == false
                && query.Name == "Кинопоиск Топ 250 (My Movies)"
                && query.IncludeItemTypes.Length == 1
                && "BoxSet".Equals(query.IncludeItemTypes[0], StringComparison.Ordinal))))
            .Returns(new QueryResult<BaseItem>()
            {
                TotalRecordCount = 1,
                Items = new BaseItem[] {
                    new BoxSet()
                    {
                        Name = "Кинопоиск Топ 250 (My Movies)",
                        Path = "/emby/film_library",
                        InternalId = 321L
                    }
                }
            });

        using CancellationTokenSource cancellationTokenSource = new();
        await _createTop250MovieCollectionsTask.Execute(cancellationTokenSource.Token, new EmbyProgress());

        _logManager.Verify(lm => lm.GetLogger(It.IsAny<string>()), Times.Exactly(4));
        _applicationPaths.VerifyGet(ap => ap.PluginConfigurationsPath, Times.Once());
        _xmlSerializer.Verify(xs => xs.DeserializeFromFile(typeof(PluginConfiguration), "CreateTop250MovieCollectionsTask_Execute_CollectionExists/EmbyKinopoiskRu.xml"), Times.Once());
        _xmlSerializer.Verify(xs => xs.DeserializeFromFile(typeof(LibraryOptions), "/emby/film_library/options.xml"), Times.Once());
        _libraryManager.Verify(lm => lm.QueryItems(It.IsAny<InternalItemsQuery>()), Times.Exactly(3));
        _libraryManager.Verify(lm => lm.GetItemLinks(It.IsInRange(103L, 106L, Moq.Range.Inclusive), It.IsAny<List<ItemLinkType>>()), Times.Exactly(4));
        _libraryManager.Verify(lm => lm.UpdateItem(It.IsAny<BaseItem>(), It.IsAny<BaseItem>(), ItemUpdateType.MetadataEdit, null), Times.Exactly(4));
        _serverApplicationHost.Verify(sah => sah.ExpandVirtualPath("/emby/film_library"), Times.Once());
        VerifyNoOtherCalls();

        Logger.Info($"Finish '{nameof(CreateTop250MovieCollectionsTask_Execute_CollectionExists)}'");
    }

    [Fact]
    public async void CreateTop250MovieCollectionsTask_Execute_CollectionNotExists()
    {
        Logger.Info($"Start '{nameof(CreateTop250MovieCollectionsTask_Execute_CollectionNotExists)}'");

        LibraryOptions collectionLibraryOptions = new()
        {
            ContentType = CollectionType.Movies.ToString(),
            EnableAdultMetadata = true,
            ImportCollections = true,
            MetadataCountryCode = "RU",
            MinCollectionItems = 1,
            Name = PluginConfiguration.DefaultTop250MovieCollectionName,
            PathInfos = new MediaPathInfo[]{
                        new MediaPathInfo()
                        {
                            NetworkPath = null,
                            Path = "/emby/video_library"
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
        _ = _xmlSerializer
            .Setup(m => m.DeserializeFromFile(typeof(LibraryOptions), "/emby/video_library/options.xml"))
            .Returns(collectionLibraryOptions);

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
                            Path = "/emby/video_library"
                        }
                    },
            PreferredImageLanguage = "ru",
            PreferredMetadataLanguage = "ru",
        };
        _ = _xmlSerializer
            .Setup(m => m.DeserializeFromFile(typeof(LibraryOptions), "CreateTop250MovieCollectionsTask_Execute_CollectionNotExists/options.xml"))
            .Returns(boxsetLibraryOptions);

        _ = _libraryManager
            .Setup(m => m.QueryItems(It.Is<InternalItemsQuery>(query =>
                query.Recursive == false
                && query.IncludeItemTypes.Length == 1
                && nameof(CollectionFolder).Equals(query.IncludeItemTypes[0], StringComparison.Ordinal))))
            .Returns(new QueryResult<BaseItem>()
            {
                Items = new BaseItem[] {
                    new CollectionFolder()
    {
        Name = "My Movies",
                        Path = "/emby/video_library",
                        InternalId = 123L
                    }
}
            });

        _ = _libraryManager
            .Setup(m => m.QueryItems(It.Is<InternalItemsQuery>(query =>
                query.Recursive == false
                && query.IsVirtualItem == false
                && query.IncludeItemTypes.Length == 1
                && nameof(Movie).Equals(query.IncludeItemTypes[0], StringComparison.Ordinal)
                && query.ParentIds.Length == 1
                && query.ParentIds[0] == 123L)))
            .Returns(new QueryResult<BaseItem>()
            {
                Items = new BaseItem[] {
                     new Movie() {
                        Name = "Гарри Поттер и Кубок огня",
                        InternalId = 103L,
                        Path = "/tmp/103",
                        ProviderIds = new(new Dictionary<string, string>()
                        {
                            { Plugin.PluginKey, "535341" },
                        })
                    },
                    new Movie() {
                        Name = "Гарри Поттер и Орден Феникса",
                        InternalId = 104L,
                        Path = "/tmp/104",
                        ProviderIds = new(new Dictionary<string, string>()
                        {
                            { MetadataProviders.Tmdb.ToString(), "522627" },
                        })
                    },
                    new Movie() {
                        Name = "Гарри Поттер и Принц-полукровка",
                        InternalId = 105L,
                        Path = "/tmp/105",
                        ProviderIds = new(new Dictionary<string, string>()
                        {
                            { MetadataProviders.Imdb.ToString(), "tt0993846" }
                        })
                    },
                    new Movie() {
                        Name = "Гарри Поттер и Дары Смерти: Часть I",
                        InternalId = 106L,
                        Path = "/tmp/106",
                        ProviderIds = new(new Dictionary<string, string>()
                        {
                            { Plugin.PluginKey, "41519" },
                            { MetadataProviders.Tmdb.ToString(), "20992" },
                            { MetadataProviders.Imdb.ToString(), "tt0118767" }
                        }),
                    },

                }
            });

        _ = _libraryManager
            .Setup(m => m.QueryItems(It.Is<InternalItemsQuery>(query =>
                query.Recursive == false
                && query.Name == "Кинопоиск Топ 250 (My Movies)"
                && query.IncludeItemTypes.Length == 1
                && "BoxSet".Equals(query.IncludeItemTypes[0], StringComparison.Ordinal))))
            .Returns(new QueryResult<BaseItem>()
            {
                TotalRecordCount = 0,
                Items = Array.Empty<BaseItem>()
            });

        _ = _collectionManager
            .Setup(m => m.CreateCollection(It.IsAny<CollectionCreationOptions>()))
            .Returns((CollectionCreationOptions options) =>
                Task.FromResult(new BoxSet()
                {
                    Name = options.Name,
                    ParentId = options.ParentId,
                    InternalId = 456L,
                }));

        _ = _libraryManager
            .Setup(m => m.GetInternalItemIds(It.Is<InternalItemsQuery>(q => Equals(true, q.IsFolder))))
            .Returns(new long[] { 1L });

        _ = _libraryManager
            .Setup(m => m.GetItemById(It.Is<long>(id => id == 1L)))
            .Returns(new CollectionFolder()
            {
                Name = "Collections",
                Path = "CreateTop250MovieCollectionsTask_Execute_CollectionNotExists"
            });

        _ = _applicationPaths
            .SetupGet(m => m.PluginConfigurationsPath)
            .Returns("CreateTop250MovieCollectionsTask_Execute_CollectionNotExists");

        using CancellationTokenSource cancellationTokenSource = new();
        await _createTop250MovieCollectionsTask.Execute(cancellationTokenSource.Token, new EmbyProgress());

        _logManager.Verify(lm => lm.GetLogger(It.IsAny<string>()), Times.Exactly(4));
        _applicationPaths.VerifyGet(ap => ap.PluginConfigurationsPath, Times.Once());
        _xmlSerializer.Verify(xs => xs.DeserializeFromFile(typeof(PluginConfiguration), "CreateTop250MovieCollectionsTask_Execute_CollectionNotExists/EmbyKinopoiskRu.xml"), Times.Once());
        _xmlSerializer.Verify(xs => xs.DeserializeFromFile(typeof(LibraryOptions), "/emby/video_library/options.xml"), Times.Once());
        _xmlSerializer.Verify(xs => xs.DeserializeFromFile(typeof(LibraryOptions), "CreateTop250MovieCollectionsTask_Execute_CollectionNotExists/options.xml"), Times.Once());
        _libraryManager.Verify(lm => lm.QueryItems(It.IsAny<InternalItemsQuery>()), Times.Exactly(3));
        _libraryManager.Verify(lm => lm.GetUserRootFolder(), Times.Once());
        _libraryManager.Verify(lm => lm.GetLibraryOptions(It.IsAny<UserRootFolder>()), Times.Once());
        _libraryManager.Verify(lm => lm.GetInternalItemIds(It.IsAny<InternalItemsQuery>()), Times.Exactly(2));
        _libraryManager.Verify(lm => lm.GetItemById(1L), Times.Once());
        _collectionManager.Verify(cm => cm.CreateCollection(It.IsAny<CollectionCreationOptions>()), Times.Once());
        _serverApplicationHost.Verify(sah => sah.ExpandVirtualPath("/emby/video_library"), Times.Exactly(2));
        VerifyNoOtherCalls();

        Logger.Info($"Finish '{nameof(CreateTop250MovieCollectionsTask_Execute_CollectionNotExists)}'");
    }

}
