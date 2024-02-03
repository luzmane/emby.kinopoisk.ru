using EmbyKinopoiskRu.Configuration;
using EmbyKinopoiskRu.ScheduledTasks;
using EmbyKinopoiskRu.Tests.Utils;

using FluentAssertions;

using MediaBrowser.Controller.Collections;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Querying;

namespace EmbyKinopoiskRu.Tests.KinopoiskDev;

[Collection("Sequential")]
public class CreateTop250CollectionsTaskTest : BaseTest
{
    private static readonly NLog.ILogger Logger = NLog.LogManager.GetLogger(nameof(KpEpisodeProviderTest));

    private readonly CreateTop250CollectionsTask _CreateTop250CollectionsTaskTest;


    #region Test configs
    public CreateTop250CollectionsTaskTest() : base(Logger)
    {
        _pluginConfiguration.Token = GetKinopoiskDevToken();

        ConfigLibraryManager();

        ConfigXmlSerializer();

        _CreateTop250CollectionsTaskTest = new(
            _logManager.Object,
            _libraryManager.Object,
            _collectionManager.Object,
            _jsonSerializer,
            _serverConfigurationManager.Object);
    }

    #endregion

    [Fact(Skip = "task removed")]
    public void CreateTop250CollectionsTaskTest_ForCodeCoverage()
    {
        Logger.Info($"Start '{nameof(CreateTop250CollectionsTaskTest_ForCodeCoverage)}'");

        _CreateTop250CollectionsTaskTest.IsHidden.Should().BeFalse("this is default task config");
        _CreateTop250CollectionsTaskTest.IsEnabled.Should().BeFalse("this is default task config");
        _CreateTop250CollectionsTaskTest.IsLogged.Should().BeTrue("this is default task config");
        _CreateTop250CollectionsTaskTest.Key.Should().Be("KinopoiskTop250");

        _CreateTop250CollectionsTaskTest.GetDefaultTriggers().Should().BeEmpty("there is no triggers defined");

        _logManager.Verify(lm => lm.GetLogger("KinopoiskRu"), Times.Once());
        _logManager.Verify(lm => lm.GetLogger("CreateTop250CollectionsTask"), Times.Once());

        VerifyNoOtherCalls();

        Logger.Info($"Finished '{nameof(CreateTop250CollectionsTaskTest_ForCodeCoverage)}'");
    }

    [Fact(Skip = "task removed")]
    public void CreateTop250CollectionsTaskTest_GetTranslation_RU()
    {
        Logger.Info($"Start '{nameof(CreateTop250CollectionsTaskTest_GetTranslation_RU)}'");

        _ = _serverConfigurationManager
            .SetupGet(scm => scm.Configuration)
            .Returns(new ServerConfiguration { UICulture = "ru" });

        _CreateTop250CollectionsTaskTest.Name.Should().Be("Создать коллекцию топ 250 Кинопоиска");
        _CreateTop250CollectionsTaskTest.Description.Should().Be("Создать коллекцию основываясь на топ 250 Кинопоиска. Поддерживает только kinopoisk.dev");
        _CreateTop250CollectionsTaskTest.Category.Should().Be("Плагин Кинопоиска");

        _logManager.Verify(lm => lm.GetLogger("KinopoiskRu"), Times.Once());
        _logManager.Verify(lm => lm.GetLogger("CreateTop250CollectionsTask"), Times.Once());
        _serverConfigurationManager.VerifyGet(scm => scm.Configuration, Times.Exactly(6));
        VerifyNoOtherCalls();

        Logger.Info($"Finished '{nameof(CreateTop250CollectionsTaskTest_GetTranslation_RU)}'");
    }

    [Fact(Skip = "task removed")]
    public void CreateTop250CollectionsTaskTest_GetTranslation_EnUs()
    {
        Logger.Info($"Start '{nameof(CreateTop250CollectionsTaskTest_GetTranslation_EnUs)}'");

        _ = _serverConfigurationManager
            .SetupGet(scm => scm.Configuration)
            .Returns(new ServerConfiguration { UICulture = "en-us" });

        _CreateTop250CollectionsTaskTest.Name.Should().Be("Create Top250 collection from Kinopoisk");
        _CreateTop250CollectionsTaskTest.Description.Should().Be("Create a collection based on the top 250 list from Kinopoisk.ru. Support kinopoisk.dev only");
        _CreateTop250CollectionsTaskTest.Category.Should().Be("Kinopoisk Plugin");

        _logManager.Verify(lm => lm.GetLogger("KinopoiskRu"), Times.Once());
        _logManager.Verify(lm => lm.GetLogger("CreateTop250CollectionsTask"), Times.Once());
        _serverConfigurationManager.VerifyGet(scm => scm.Configuration, Times.Exactly(6));
        VerifyNoOtherCalls();

        Logger.Info($"Finished '{nameof(CreateTop250CollectionsTaskTest_GetTranslation_EnUs)}'");
    }

    [Fact(Skip = "task removed")]
    public void CreateTop250CollectionsTaskTest_GetTranslation_UK()
    {
        Logger.Info($"Start '{nameof(CreateTop250CollectionsTaskTest_GetTranslation_UK)}'");

        _ = _serverConfigurationManager
            .SetupGet(scm => scm.Configuration)
            .Returns(new ServerConfiguration { UICulture = "uk" });

        _CreateTop250CollectionsTaskTest.Name.Should().Be("Створити колекцію топ 250 Кінопошуку");
        _CreateTop250CollectionsTaskTest.Description.Should().Be("Створити колекцію ґрунтуючись на топ 250 Кінопошуку. Підтримує лише kinopoisk.dev");
        _CreateTop250CollectionsTaskTest.Category.Should().Be("Плагін Кінопошуку");

        _logManager.Verify(lm => lm.GetLogger("KinopoiskRu"), Times.Once());
        _logManager.Verify(lm => lm.GetLogger("CreateTop250CollectionsTask"), Times.Once());
        _serverConfigurationManager.VerifyGet(scm => scm.Configuration, Times.Exactly(6));
        VerifyNoOtherCalls();

        Logger.Info($"Finished '{nameof(CreateTop250CollectionsTaskTest_GetTranslation_UK)}'");
    }

    [Fact(Skip = "task removed")]
    public void CreateTop250CollectionsTaskTest_GetTranslation_BG()
    {
        Logger.Info($"Start '{nameof(CreateTop250CollectionsTaskTest_GetTranslation_BG)}'");

        _ = _serverConfigurationManager
            .SetupGet(scm => scm.Configuration)
            .Returns(new ServerConfiguration { UICulture = "bg" });

        _CreateTop250CollectionsTaskTest.Name.Should().Be("Create Top250 collection from Kinopoisk");
        _CreateTop250CollectionsTaskTest.Description.Should().Be("Create a collection based on the top 250 list from Kinopoisk.ru. Support kinopoisk.dev only");
        _CreateTop250CollectionsTaskTest.Category.Should().Be("Kinopoisk Plugin");

        _logManager.Verify(lm => lm.GetLogger("KinopoiskRu"), Times.Once());
        _logManager.Verify(lm => lm.GetLogger("CreateTop250CollectionsTask"), Times.Once());
        _serverConfigurationManager.VerifyGet(scm => scm.Configuration, Times.Exactly(6));
        VerifyNoOtherCalls();

        Logger.Info($"Finished '{nameof(CreateTop250CollectionsTaskTest_GetTranslation_BG)}'");
    }

    [Fact(Skip = "task removed")]
    public async void CreateTop250CollectionsTaskTest_Execute_CollectionExists()
    {
        Logger.Info($"Start '{nameof(CreateTop250CollectionsTaskTest_Execute_CollectionExists)}'");

        _ = _applicationPaths
            .SetupGet(m => m.PluginConfigurationsPath)
            .Returns("CreateTop250CollectionsTaskTest_Execute_CollectionExists");

        LibraryOptions collectionLibraryOptions = new()
        {
            ContentType = CollectionType.Movies.ToString(),
            EnableAdultMetadata = true,
            ImportCollections = true,
            MetadataCountryCode = "RU",
            MinCollectionItems = 1,
            Name = PluginConfiguration.DefaultTop250CollectionName,
            PathInfos = new[]{
                        new MediaPathInfo
                        {
                            NetworkPath = null,
                            Path = "/emby/film_library"
                        }
                    },
            PreferredImageLanguage = "ru",
            PreferredMetadataLanguage = "ru",
            SkipSubtitlesIfEmbeddedSubtitlesPresent = true,
            SkipSubtitlesIfAudioTrackMatches = true,
            TypeOptions = new[]{
                        new TypeOptions
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
                !query.Recursive
                && query.IncludeItemTypes.Length == 1
                && nameof(CollectionFolder).Equals(query.IncludeItemTypes[0], StringComparison.Ordinal))))
            .Returns(new QueryResult<BaseItem>
            {
                Items = new BaseItem[] {
                    new CollectionFolder
                    {
                        Name = "My Movies",
                        Path = "/emby/film_library",
                        InternalId = 123L
                    }
                }
            });

        _ = _libraryManager
            .Setup(m => m.QueryItems(It.Is<InternalItemsQuery>(query =>
                !query.Recursive
                && query.IsVirtualItem == false
                && query.IncludeItemTypes.Length == 1
                && nameof(Movie).Equals(query.IncludeItemTypes[0], StringComparison.Ordinal)
                && query.ParentIds.Length == 1
                && query.ParentIds[0] == 123L)))
            .Returns(new QueryResult<BaseItem>
            {
                Items = new BaseItem[] {
                     new Movie {
                        Name = "Гарри Поттер и Кубок огня",
                        InternalId = 103L,
                        Path = "/tmp/103",
                        ProviderIds = new(new Dictionary<string, string>
                        {
                            { Plugin.PluginKey, "535341" },
                        })
                    },
                    new Movie {
                        Name = "Гарри Поттер и Орден Феникса",
                        InternalId = 104L,
                        Path = "/tmp/104",
                        ProviderIds = new(new Dictionary<string, string>
                        {
                            { MetadataProviders.Tmdb.ToString(), "522627" },
                        })
                    },
                    new Movie {
                        Name = "Гарри Поттер и Принц-полукровка",
                        InternalId = 105L,
                        Path = "/tmp/105",
                        ProviderIds = new(new Dictionary<string, string>
                        {
                            { MetadataProviders.Imdb.ToString(), "tt0993846" }
                        })
                    },
                    new Movie {
                        Name = "Гарри Поттер и Дары Смерти: Часть I",
                        InternalId = 106L,
                        Path = "/tmp/106",
                        ProviderIds = new(new Dictionary<string, string>
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
                !query.Recursive
                && query.Name == "Кинопоиск Топ 250 (My Movies)"
                && query.IncludeItemTypes.Length == 1
                && "BoxSet".Equals(query.IncludeItemTypes[0], StringComparison.Ordinal))))
            .Returns(new QueryResult<BaseItem>
            {
                TotalRecordCount = 1,
                Items = new BaseItem[] {
                    new BoxSet
                    {
                        Name = "Кинопоиск Топ 250 (My Movies)",
                        Path = "/emby/film_library",
                        InternalId = 321L
                    }
                }
            });

        using CancellationTokenSource cancellationTokenSource = new();
        await _CreateTop250CollectionsTaskTest.Execute(cancellationTokenSource.Token, new EmbyProgress());

        _logManager.Verify(lm => lm.GetLogger(It.IsAny<string>()), Times.Exactly(4));
        _applicationPaths.VerifyGet(ap => ap.PluginConfigurationsPath, Times.Once());
        _xmlSerializer.Verify(xs => xs.DeserializeFromFile(typeof(PluginConfiguration), "CreateTop250CollectionsTaskTest_Execute_CollectionExists/EmbyKinopoiskRu.xml"), Times.Once());
        _xmlSerializer.Verify(xs => xs.DeserializeFromFile(typeof(LibraryOptions), "/emby/film_library/options.xml"), Times.Once());
        _libraryManager.Verify(lm => lm.QueryItems(It.IsAny<InternalItemsQuery>()), Times.Exactly(3));
        _libraryManager.Verify(lm => lm.GetItemLinks(It.IsInRange(103L, 106L, Moq.Range.Inclusive), It.IsAny<List<ItemLinkType>>()), Times.Exactly(4));
        _libraryManager.Verify(lm => lm.UpdateItem(It.IsAny<BaseItem>(), It.IsAny<BaseItem>(), ItemUpdateType.MetadataEdit, null), Times.Exactly(4));
        _serverApplicationHost.Verify(sah => sah.ExpandVirtualPath("/emby/film_library"), Times.Once());
        VerifyNoOtherCalls();

        Logger.Info($"Finish '{nameof(CreateTop250CollectionsTaskTest_Execute_CollectionExists)}'");
    }

    [Fact(Skip = "task removed")]
    public async void CreateTop250CollectionsTaskTest_Execute_CollectionNotExists_OneLib()
    {
        Logger.Info($"Start '{nameof(CreateTop250CollectionsTaskTest_Execute_CollectionNotExists_OneLib)}'");

        _pluginConfiguration.Top250InOneLib = true;

        LibraryOptions collectionLibraryOptions = new()
        {
            ContentType = CollectionType.Movies.ToString(),
            EnableAdultMetadata = true,
            ImportCollections = true,
            MetadataCountryCode = "RU",
            MinCollectionItems = 1,
            Name = PluginConfiguration.DefaultTop250CollectionName,
            PathInfos = new[]{
                        new MediaPathInfo
                        {
                            NetworkPath = null,
                            Path = "/emby/video_library_onelib"
                        }
                    },
            PreferredImageLanguage = "ru",
            PreferredMetadataLanguage = "ru",
            SkipSubtitlesIfEmbeddedSubtitlesPresent = true,
            SkipSubtitlesIfAudioTrackMatches = true,
            TypeOptions = new[]{
                        new TypeOptions
                        {
                            Type = "Movie"
                        }
                    }
        };
        _ = _xmlSerializer
            .Setup(m => m.DeserializeFromFile(typeof(LibraryOptions), "/emby/video_library_onelib/options.xml"))
            .Returns(collectionLibraryOptions);

        LibraryOptions boxsetLibraryOptions = new()
        {
            ContentType = CollectionType.BoxSets.ToString(),
            MetadataCountryCode = "RU",
            MinCollectionItems = 1,
            Name = "Collections",
            PathInfos = new[]{
                        new MediaPathInfo
                        {
                            NetworkPath = null,
                            Path = "/emby/video_library_onelib"
                        }
                    },
            PreferredImageLanguage = "ru",
            PreferredMetadataLanguage = "ru",
        };
        _ = _xmlSerializer
            .Setup(m => m.DeserializeFromFile(typeof(LibraryOptions), "CreateTop250CollectionsTaskTest_Execute_CollectionNotExists_OneLib/options.xml"))
            .Returns(boxsetLibraryOptions);

        _ = _libraryManager
            .Setup(m => m.QueryItems(It.Is<InternalItemsQuery>(query =>
                !query.Recursive
                && query.IncludeItemTypes.Length == 1
                && nameof(CollectionFolder).Equals(query.IncludeItemTypes[0], StringComparison.Ordinal))))
            .Returns(new QueryResult<BaseItem>
            {
                Items = new BaseItem[]
                {
                    new CollectionFolder
                    {
                        Name = "My Movies",
                        Path = "/emby/video_library_onelib",
                        InternalId = 123L
                    }
                }
            });

        _ = _libraryManager
            .Setup(m => m.QueryItems(It.Is<InternalItemsQuery>(query =>
                !query.Recursive
                && query.IsVirtualItem == false
                && query.IncludeItemTypes.Length == 1
                && nameof(Movie).Equals(query.IncludeItemTypes[0], StringComparison.Ordinal)
                && query.ParentIds.Length == 1
                && query.ParentIds[0] == 123L)))
            .Returns(new QueryResult<BaseItem>
            {
                Items = new BaseItem[] {
                     new Movie {
                        Name = "Гарри Поттер и Кубок огня",
                        InternalId = 103L,
                        Path = "/tmp/103",
                        ProviderIds = new(new Dictionary<string, string>
                        {
                            { Plugin.PluginKey, "535341" },
                        })
                    },
                    new Movie {
                        Name = "Гарри Поттер и Орден Феникса",
                        InternalId = 104L,
                        Path = "/tmp/104",
                        ProviderIds = new(new Dictionary<string, string>
                        {
                            { MetadataProviders.Tmdb.ToString(), "522627" },
                        })
                    },
                    new Movie {
                        Name = "Гарри Поттер и Принц-полукровка",
                        InternalId = 105L,
                        Path = "/tmp/105",
                        ProviderIds = new(new Dictionary<string, string>
                        {
                            { MetadataProviders.Imdb.ToString(), "tt0993846" }
                        })
                    },
                    new Movie {
                        Name = "Гарри Поттер и Дары Смерти: Часть I",
                        InternalId = 106L,
                        Path = "/tmp/106",
                        ProviderIds = new(new Dictionary<string, string>
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
                !query.Recursive
                && query.Name == _pluginConfiguration.GetCurrentTop250CollectionName()
                && query.IncludeItemTypes.Length == 1
                && "BoxSet".Equals(query.IncludeItemTypes[0], StringComparison.Ordinal))))
            .Returns(new QueryResult<BaseItem>
            {
                TotalRecordCount = 0,
                Items = Array.Empty<BaseItem>()
            });

        _ = _collectionManager
            .Setup(m => m.CreateCollection(It.IsAny<CollectionCreationOptions>()))
            .Returns((CollectionCreationOptions options) =>
                Task.FromResult(new BoxSet
                {
                    Name = options.Name,
                    ParentId = options.ParentId,
                    InternalId = 456L,
                }));

        _ = _libraryManager
            .Setup(m => m.GetInternalItemIds(It.Is<InternalItemsQuery>(q => Equals(true, q.IsFolder))))
            .Returns(new[] { 1L });

        _ = _libraryManager
            .Setup(m => m.GetItemById(It.Is<long>(id => id == 1L)))
            .Returns(new CollectionFolder
            {
                Name = "Collections",
                Path = "CreateTop250CollectionsTaskTest_Execute_CollectionNotExists_OneLib"
            });

        _ = _applicationPaths
            .SetupGet(m => m.PluginConfigurationsPath)
            .Returns("CreateTop250CollectionsTaskTest_Execute_CollectionNotExists_OneLib");

        using CancellationTokenSource cancellationTokenSource = new();
        await _CreateTop250CollectionsTaskTest.Execute(cancellationTokenSource.Token, new EmbyProgress());

        _logManager.Verify(lm => lm.GetLogger(It.IsAny<string>()), Times.Exactly(4));
        _applicationPaths.VerifyGet(ap => ap.PluginConfigurationsPath, Times.Once());
        _xmlSerializer.Verify(xs => xs.DeserializeFromFile(typeof(PluginConfiguration), "CreateTop250CollectionsTaskTest_Execute_CollectionNotExists_OneLib/EmbyKinopoiskRu.xml"), Times.Once());
        _xmlSerializer.Verify(xs => xs.DeserializeFromFile(typeof(LibraryOptions), "/emby/video_library_onelib/options.xml"), Times.Once());
        _xmlSerializer.Verify(xs => xs.DeserializeFromFile(typeof(LibraryOptions), "CreateTop250CollectionsTaskTest_Execute_CollectionNotExists_OneLib/options.xml"), Times.Once());
        _libraryManager.Verify(lm => lm.QueryItems(It.IsAny<InternalItemsQuery>()), Times.Exactly(3));
        _libraryManager.Verify(lm => lm.GetUserRootFolder(), Times.Once());
        _libraryManager.Verify(lm => lm.GetLibraryOptions(It.IsAny<UserRootFolder>()), Times.Once());
        _libraryManager.Verify(lm => lm.GetInternalItemIds(It.IsAny<InternalItemsQuery>()), Times.Exactly(2));
        _libraryManager.Verify(lm => lm.GetItemById(1L), Times.Once());
        _collectionManager.Verify(cm => cm.CreateCollection(It.IsAny<CollectionCreationOptions>()), Times.Once());
        _serverApplicationHost.Verify(sah => sah.ExpandVirtualPath("/emby/video_library_onelib"), Times.Exactly(2));
        VerifyNoOtherCalls();

        Logger.Info($"Finish '{nameof(CreateTop250CollectionsTaskTest_Execute_CollectionNotExists_OneLib)}'");
    }

    [Fact(Skip = "task removed")]
    public async void CreateTop250CollectionsTaskTest_Execute_CollectionNotExists()
    {
        Logger.Info($"Start '{nameof(CreateTop250CollectionsTaskTest_Execute_CollectionNotExists)}'");

        LibraryOptions collectionLibraryOptions = new()
        {
            ContentType = CollectionType.Movies.ToString(),
            EnableAdultMetadata = true,
            ImportCollections = true,
            MetadataCountryCode = "RU",
            MinCollectionItems = 1,
            Name = PluginConfiguration.DefaultTop250CollectionName,
            PathInfos = new[]{
                        new MediaPathInfo
                        {
                            NetworkPath = null,
                            Path = "/emby/video_library"
                        }
                    },
            PreferredImageLanguage = "ru",
            PreferredMetadataLanguage = "ru",
            SkipSubtitlesIfEmbeddedSubtitlesPresent = true,
            SkipSubtitlesIfAudioTrackMatches = true,
            TypeOptions = new[]{
                        new TypeOptions
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
            PathInfos = new[]{
                        new MediaPathInfo
                        {
                            NetworkPath = null,
                            Path = "/emby/video_library"
                        }
                    },
            PreferredImageLanguage = "ru",
            PreferredMetadataLanguage = "ru",
        };
        _ = _xmlSerializer
            .Setup(m => m.DeserializeFromFile(typeof(LibraryOptions), "CreateTop250CollectionsTaskTest_Execute_CollectionNotExists/options.xml"))
            .Returns(boxsetLibraryOptions);

        _ = _libraryManager
            .Setup(m => m.QueryItems(It.Is<InternalItemsQuery>(query =>
                !query.Recursive
                && query.IncludeItemTypes.Length == 1
                && nameof(CollectionFolder).Equals(query.IncludeItemTypes[0], StringComparison.Ordinal))))
            .Returns(new QueryResult<BaseItem>
            {
                Items = new BaseItem[]
                {
                    new CollectionFolder
                    {
                        Name = "My Movies",
                        Path = "/emby/video_library",
                        InternalId = 123L
                    }
                }
            });

        _ = _libraryManager
            .Setup(m => m.QueryItems(It.Is<InternalItemsQuery>(query =>
                !query.Recursive
                && query.IsVirtualItem == false
                && query.IncludeItemTypes.Length == 1
                && nameof(Movie).Equals(query.IncludeItemTypes[0], StringComparison.Ordinal)
                && query.ParentIds.Length == 1
                && query.ParentIds[0] == 123L)))
            .Returns(new QueryResult<BaseItem>
            {
                Items = new BaseItem[] {
                     new Movie {
                        Name = "Гарри Поттер и Кубок огня",
                        InternalId = 103L,
                        Path = "/tmp/103",
                        ProviderIds = new(new Dictionary<string, string>
                        {
                            { Plugin.PluginKey, "535341" },
                        })
                    },
                    new Movie {
                        Name = "Гарри Поттер и Орден Феникса",
                        InternalId = 104L,
                        Path = "/tmp/104",
                        ProviderIds = new(new Dictionary<string, string>
                        {
                            { MetadataProviders.Tmdb.ToString(), "522627" },
                        })
                    },
                    new Movie {
                        Name = "Гарри Поттер и Принц-полукровка",
                        InternalId = 105L,
                        Path = "/tmp/105",
                        ProviderIds = new(new Dictionary<string, string>
                        {
                            { MetadataProviders.Imdb.ToString(), "tt0993846" }
                        })
                    },
                    new Movie {
                        Name = "Гарри Поттер и Дары Смерти: Часть I",
                        InternalId = 106L,
                        Path = "/tmp/106",
                        ProviderIds = new(new Dictionary<string, string>
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
                !query.Recursive
                && query.Name == $"{_pluginConfiguration.GetCurrentTop250CollectionName()} (My Movies)"
                && query.IncludeItemTypes.Length == 1
                && "BoxSet".Equals(query.IncludeItemTypes[0], StringComparison.Ordinal))))
            .Returns(new QueryResult<BaseItem>
            {
                TotalRecordCount = 0,
                Items = Array.Empty<BaseItem>()
            });

        _ = _collectionManager
            .Setup(m => m.CreateCollection(It.IsAny<CollectionCreationOptions>()))
            .Returns((CollectionCreationOptions options) =>
                Task.FromResult(new BoxSet
                {
                    Name = options.Name,
                    ParentId = options.ParentId,
                    InternalId = 456L,
                }));

        _ = _libraryManager
            .Setup(m => m.GetInternalItemIds(It.Is<InternalItemsQuery>(q => Equals(true, q.IsFolder))))
            .Returns(new[] { 1L });

        _ = _libraryManager
            .Setup(m => m.GetItemById(It.Is<long>(id => id == 1L)))
            .Returns(new CollectionFolder
            {
                Name = "Collections",
                Path = "CreateTop250CollectionsTaskTest_Execute_CollectionNotExists"
            });

        _ = _applicationPaths
            .SetupGet(m => m.PluginConfigurationsPath)
            .Returns("CreateTop250CollectionsTaskTest_Execute_CollectionNotExists");

        using CancellationTokenSource cancellationTokenSource = new();
        await _CreateTop250CollectionsTaskTest.Execute(cancellationTokenSource.Token, new EmbyProgress());

        _logManager.Verify(lm => lm.GetLogger(It.IsAny<string>()), Times.Exactly(4));
        _applicationPaths.VerifyGet(ap => ap.PluginConfigurationsPath, Times.Once());
        _xmlSerializer.Verify(xs => xs.DeserializeFromFile(typeof(PluginConfiguration), "CreateTop250CollectionsTaskTest_Execute_CollectionNotExists/EmbyKinopoiskRu.xml"), Times.Once());
        _xmlSerializer.Verify(xs => xs.DeserializeFromFile(typeof(LibraryOptions), "/emby/video_library/options.xml"), Times.Once());
        _xmlSerializer.Verify(xs => xs.DeserializeFromFile(typeof(LibraryOptions), "CreateTop250CollectionsTaskTest_Execute_CollectionNotExists/options.xml"), Times.Once());
        _libraryManager.Verify(lm => lm.QueryItems(It.IsAny<InternalItemsQuery>()), Times.Exactly(3));
        _libraryManager.Verify(lm => lm.GetUserRootFolder(), Times.Once());
        _libraryManager.Verify(lm => lm.GetLibraryOptions(It.IsAny<UserRootFolder>()), Times.Once());
        _libraryManager.Verify(lm => lm.GetInternalItemIds(It.IsAny<InternalItemsQuery>()), Times.Exactly(2));
        _libraryManager.Verify(lm => lm.GetItemById(1L), Times.Once());
        _collectionManager.Verify(cm => cm.CreateCollection(It.IsAny<CollectionCreationOptions>()), Times.Once());
        _serverApplicationHost.Verify(sah => sah.ExpandVirtualPath("/emby/video_library"), Times.Exactly(2));
        VerifyNoOtherCalls();

        Logger.Info($"Finish '{nameof(CreateTop250CollectionsTaskTest_Execute_CollectionNotExists)}'");
    }

}
