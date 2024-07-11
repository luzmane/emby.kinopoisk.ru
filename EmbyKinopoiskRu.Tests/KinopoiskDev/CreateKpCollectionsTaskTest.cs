using Emby.Notifications;

using EmbyKinopoiskRu.Configuration;
using EmbyKinopoiskRu.ScheduledTasks;
using EmbyKinopoiskRu.Tests.Utils;

using FluentAssertions;

using MediaBrowser.Controller.Collections;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Activity;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Querying;

using Range = Moq.Range;

namespace EmbyKinopoiskRu.Tests.KinopoiskDev;

[Collection("Sequential")]
public class CreateKpCollectionsTaskTest : BaseTest
{
    private static readonly NLog.ILogger Logger = NLog.LogManager.GetLogger(nameof(CreateKpCollectionsTaskTest));

    private readonly CreateKpCollectionsTask _createKpCollectionsTaskTest;


    #region Test configs

    public CreateKpCollectionsTaskTest() : base(Logger)
    {
        _pluginConfiguration.Token = GetKinopoiskDevToken();
        _pluginConfiguration.CollectionsList = new List<CollectionItem>
        {
            new()
            {
                Category = "Онлайн-кинотеатр",
                Id = "hd-family",
                IsEnable = true,
                Name = "Смотрим всей семьей"
            },
            new()
            {
                Category = "Not valid",
                Id = "not valid",
                IsEnable = false,
                Name = "Not valid"
            }
        };

        ConfigLibraryManager();

        ConfigXmlSerializer();

        _createKpCollectionsTaskTest = new CreateKpCollectionsTask(
            _logManager.Object,
            _libraryManager.Object,
            _collectionManager.Object,
            _jsonSerializer,
            _serverConfigurationManager.Object);
    }

    #endregion

    [Fact]
    public void CreateKpCollectionsTaskTest_ForCodeCoverage()
    {
        Logger.Info($"Start '{nameof(CreateKpCollectionsTaskTest_ForCodeCoverage)}'");

        _createKpCollectionsTaskTest.IsHidden.Should().BeFalse();
        _createKpCollectionsTaskTest.IsEnabled.Should().BeFalse();
        _createKpCollectionsTaskTest.IsLogged.Should().BeTrue();
        _createKpCollectionsTaskTest.Key.Should().Be("KinopoiskCollections");

        _createKpCollectionsTaskTest.GetDefaultTriggers().Should().BeEmpty();

        _logManager.Verify(lm => lm.GetLogger("KinopoiskRu"), Times.Once());
        _logManager.Verify(lm => lm.GetLogger("CreateKpCollectionsTask"), Times.Once());

        VerifyNoOtherCalls();

        Logger.Info($"Finished '{nameof(CreateKpCollectionsTaskTest_ForCodeCoverage)}'");
    }

    [Fact]
    public void CreateKpCollectionsTaskTest_GetTranslation_RU()
    {
        Logger.Info($"Start '{nameof(CreateKpCollectionsTaskTest_GetTranslation_RU)}'");

        _ = _serverConfigurationManager
            .SetupGet(scm => scm.Configuration)
            .Returns(new ServerConfiguration
            {
                UICulture = "ru"
            });

        _createKpCollectionsTaskTest.Name.Should().Be("Создать коллекции Кинопоиска");
        _createKpCollectionsTaskTest.Description.Should().Be("Создать коллекции основываясь на коллекциях Кинопоиска и настройках плагина.");
        _createKpCollectionsTaskTest.Category.Should().Be("Плагин Кинопоиска");

        _logManager.Verify(lm => lm.GetLogger("KinopoiskRu"), Times.Once());
        _logManager.Verify(lm => lm.GetLogger("CreateKpCollectionsTask"), Times.Once());
        _serverConfigurationManager.VerifyGet(scm => scm.Configuration, Times.Exactly(6));
        VerifyNoOtherCalls();

        Logger.Info($"Finished '{nameof(CreateKpCollectionsTaskTest_GetTranslation_RU)}'");
    }

    [Fact]
    public void CreateKpCollectionsTaskTest_GetTranslation_EnUs()
    {
        Logger.Info($"Start '{nameof(CreateKpCollectionsTaskTest_GetTranslation_EnUs)}'");

        _ = _serverConfigurationManager
            .SetupGet(scm => scm.Configuration)
            .Returns(new ServerConfiguration
            {
                UICulture = "en-us"
            });

        _createKpCollectionsTaskTest.Name.Should().Be("Create Kinopoisk collections");
        _createKpCollectionsTaskTest.Description.Should().Be("Create collections based on Kinopoisk.ru collections and configuration of the plugin.");
        _createKpCollectionsTaskTest.Category.Should().Be("Kinopoisk Plugin");

        _logManager.Verify(lm => lm.GetLogger("KinopoiskRu"), Times.Once());
        _logManager.Verify(lm => lm.GetLogger("CreateKpCollectionsTask"), Times.Once());
        _serverConfigurationManager.VerifyGet(scm => scm.Configuration, Times.Exactly(6));
        VerifyNoOtherCalls();

        Logger.Info($"Finished '{nameof(CreateKpCollectionsTaskTest_GetTranslation_EnUs)}'");
    }

    [Fact]
    public void CreateKpCollectionsTaskTest_GetTranslation_UK()
    {
        Logger.Info($"Start '{nameof(CreateKpCollectionsTaskTest_GetTranslation_UK)}'");

        _ = _serverConfigurationManager
            .SetupGet(scm => scm.Configuration)
            .Returns(new ServerConfiguration
            {
                UICulture = "uk"
            });

        _createKpCollectionsTaskTest.Name.Should().Be("Створити колекції Кінопошуку");
        _createKpCollectionsTaskTest.Description.Should().Be("Створити колекції ґрунтуючись на колекціях Кінопошуку та налаштуваннях плагіна.");
        _createKpCollectionsTaskTest.Category.Should().Be("Плагін Кінопошуку");

        _logManager.Verify(lm => lm.GetLogger("KinopoiskRu"), Times.Once());
        _logManager.Verify(lm => lm.GetLogger("CreateKpCollectionsTask"), Times.Once());
        _serverConfigurationManager.VerifyGet(scm => scm.Configuration, Times.Exactly(6));
        VerifyNoOtherCalls();

        Logger.Info($"Finished '{nameof(CreateKpCollectionsTaskTest_GetTranslation_UK)}'");
    }

    [Fact]
    public void CreateKpCollectionsTaskTest_GetTranslation_BG()
    {
        Logger.Info($"Start '{nameof(CreateKpCollectionsTaskTest_GetTranslation_BG)}'");

        _ = _serverConfigurationManager
            .SetupGet(scm => scm.Configuration)
            .Returns(new ServerConfiguration
            {
                UICulture = "bg"
            });

        _createKpCollectionsTaskTest.Name.Should().Be("Create Kinopoisk collections");
        _createKpCollectionsTaskTest.Description.Should().Be("Create collections based on Kinopoisk.ru collections and configuration of the plugin.");
        _createKpCollectionsTaskTest.Category.Should().Be("Kinopoisk Plugin");

        _logManager.Verify(lm => lm.GetLogger("KinopoiskRu"), Times.Once());
        _logManager.Verify(lm => lm.GetLogger("CreateKpCollectionsTask"), Times.Once());
        _serverConfigurationManager.VerifyGet(scm => scm.Configuration, Times.Exactly(6));
        VerifyNoOtherCalls();

        Logger.Info($"Finished '{nameof(CreateKpCollectionsTaskTest_GetTranslation_BG)}'");
    }

    [Fact]
    public async void CreateKpCollectionsTaskTest_Execute_CollectionExists()
    {
        Logger.Info($"Start '{nameof(CreateKpCollectionsTaskTest_Execute_CollectionExists)}'");

        #region CreateKpCollectionsTaskTest_Execute_CollectionExists config

        _pluginConfiguration.CollectionsList.Add(
            new CollectionItem
            {
                Category = "Сериалы",
                Id = "series_about_vampires",
                IsEnable = true,
                Name = "Сериалы про вампиров"
            });

        _ = _applicationPaths
            .SetupGet(m => m.PluginConfigurationsPath)
            .Returns(nameof(CreateKpCollectionsTaskTest_Execute_CollectionExists));

        _ = _libraryManager
            .Setup(m => m.QueryItems(It.Is<InternalItemsQuery>(query =>
                true.Equals(query.HasPath)
                && true.Equals(query.IsFolder)
                && false.Equals(query.IsVirtualItem)
                && query.IncludeItemTypes.Length == 1
                && "CollectionFolder".Equals(query.IncludeItemTypes[0], StringComparison.Ordinal))
            ))
            .Returns(new QueryResult<BaseItem>
            {
                Items = new BaseItem[]
                {
                    new CollectionFolder
                    {
                        Name = "My Movies",
                        Path = "/emby/film_library",
                        InternalId = 123L
                    },
                    new CollectionFolder
                    {
                        Name = "My Series",
                        Path = "/emby/series_library",
                        InternalId = 124L
                    },
                    new CollectionFolder
                    {
                        Name = "My Mix",
                        Path = "/emby/mix_library",
                        InternalId = 125L
                    },
                    new CollectionFolder
                    {
                        Name = "My Audio",
                        Path = "/emby/audio_library",
                        InternalId = 126L
                    }
                }
            });

        _ = _libraryManager
            .Setup(m => m.QueryItems(It.Is<InternalItemsQuery>(query =>
                true.Equals(query.Recursive)
                && false.Equals(query.IsVirtualItem)
                && true.Equals(query.HasPath)
                && query.IncludeItemTypes.Length == 2
                && "Movie".Equals(query.IncludeItemTypes[0], StringComparison.Ordinal)
                && "Series".Equals(query.IncludeItemTypes[1], StringComparison.Ordinal)
                && query.ParentIds.Length == 4
                && query.ParentIds.All(x => x >= 123 && x <= 126)
                && query.AnyProviderIdEquals.Any())
            ))
            .Returns(new QueryResult<BaseItem>
            {
                Items = Array.Empty<BaseItem>()
            });

        _ = _libraryManager
            .Setup(m => m.QueryItems(It.Is<InternalItemsQuery>(query =>
                true.Equals(query.Recursive)
                && false.Equals(query.IsVirtualItem)
                && true.Equals(query.HasPath)
                && query.IncludeItemTypes.Length == 2
                && "Movie".Equals(query.IncludeItemTypes[0], StringComparison.Ordinal)
                && "Series".Equals(query.IncludeItemTypes[1], StringComparison.Ordinal)
                && query.ParentIds.Length == 4
                && query.ParentIds.All(x => x >= 123 && x <= 126)
                && query.AnyProviderIdEquals.Count == 100
                && query.AnyProviderIdEquals.Contains(new KeyValuePair<string, string>(Plugin.PluginKey, "689"))
            )))
            .Returns(new QueryResult<BaseItem>
            {
                Items = new BaseItem[]
                {
                    new Movie
                    {
                        Name = "Гарри Поттер и философский камень",
                        InternalId = 103L,
                        Path = "/tmp/103",
                        ProviderIds = new ProviderIdDictionary(new Dictionary<string, string>
                        {
                            { Plugin.PluginKey, "689" }
                        })
                    },
                    new Movie
                    {
                        Name = "Один дома",
                        InternalId = 104L,
                        Path = "/tmp/104",
                        ProviderIds = new ProviderIdDictionary(new Dictionary<string, string>
                        {
                            { Plugin.PluginKey, "8124" }
                        }),
                        Collections = new[]
                        {
                            new LinkedItemInfo
                            {
                                Id = 321L,
                                Name = "Смотрим всей семьей",
                                ProviderIds = new ProviderIdDictionary()
                            }
                        }
                    }
                }
            });

        _ = _libraryManager
            .Setup(m => m.QueryItems(It.Is<InternalItemsQuery>(query =>
                "Смотрим всей семьей".Equals(query.Name, StringComparison.Ordinal)
                && true.Equals(query.Recursive)
                && query.IncludeItemTypes.Length == 1
                && "BoxSet".Equals(query.IncludeItemTypes[0], StringComparison.Ordinal))
            ))
            .Returns(new QueryResult<BaseItem>
            {
                TotalRecordCount = 1,
                Items = new BaseItem[]
                {
                    new BoxSet
                    {
                        Name = "Смотрим всей семьей",
                        Path = "/emby/hd-family",
                        InternalId = 321L
                    }
                }
            });

        #endregion

        using CancellationTokenSource cancellationTokenSource = new();
        await _createKpCollectionsTaskTest.Execute(cancellationTokenSource.Token, new EmbyProgress());

        // check that no errors in API
        _activityManager.Verify(a => a.Create(It.IsAny<ActivityLogEntry>()), Times.Never());
        _notificationManager.Verify(n => n.SendNotification(It.IsAny<NotificationRequest>()), Times.Never());

        _applicationPaths.VerifyGet(ap => ap.PluginConfigurationsPath, Times.Once());
        // uncomment in case single test run (static/cache invocations)
        //_itemRepository.Verify(lm => lm.GetExtradataTypeId("LibraryOptions"), Times.Exactly(4));
        //_itemRepository.Verify(lm => lm.GetItemExtradataValue(It.IsInRange(123L, 126L, Range.Inclusive), 0), Times.Exactly(4));
        _libraryManager.Verify(lm => lm.QueryItems(It.IsAny<InternalItemsQuery>()), Times.Exactly(11));
        _libraryManager.Verify(lm => lm.GetItemLinks(It.IsInRange(103L, 104L, Range.Inclusive), It.IsAny<List<ItemLinkType>>()), Times.Exactly(2));
        _libraryManager.Verify(lm => lm.UpdateItem(It.IsAny<BaseItem>(), It.IsAny<BaseItem>(), ItemUpdateType.MetadataEdit, null), Times.Exactly(2));
        _logManager.Verify(lm => lm.GetLogger(It.IsAny<string>()), Times.Exactly(4));
        _xmlSerializer.Verify(xs => xs.DeserializeFromFile(typeof(PluginConfiguration), $"{nameof(CreateKpCollectionsTaskTest_Execute_CollectionExists)}/EmbyKinopoiskRu.xml"), Times.Once());

        VerifyNoOtherCalls();

        Logger.Info($"Finish '{nameof(CreateKpCollectionsTaskTest_Execute_CollectionExists)}'");
    }

    [Fact]
    public async void CreateKpCollectionsTaskTest_Execute_CollectionNotExists()
    {
        Logger.Info($"Start '{nameof(CreateKpCollectionsTaskTest_Execute_CollectionNotExists)}'");

        #region CreateKpCollectionsTaskTest_Execute_CollectionNotExists config

        _ = _applicationPaths
            .SetupGet(m => m.PluginConfigurationsPath)
            .Returns(nameof(CreateKpCollectionsTaskTest_Execute_CollectionNotExists));

        _ = _libraryManager
            .Setup(m => m.QueryItems(It.Is<InternalItemsQuery>(query =>
                true.Equals(query.HasPath)
                && true.Equals(query.IsFolder)
                && false.Equals(query.IsVirtualItem)
                && query.IncludeItemTypes.Length == 1
                && "CollectionFolder".Equals(query.IncludeItemTypes[0], StringComparison.Ordinal))
            ))
            .Returns(new QueryResult<BaseItem>
            {
                Items = new BaseItem[]
                {
                    new CollectionFolder
                    {
                        Name = "My Movies",
                        Path = "/emby/film_library_ext",
                        InternalId = 123L
                    },
                    new CollectionFolder
                    {
                        Name = "My Series",
                        Path = "/emby/series_library_ext",
                        InternalId = 124L
                    },
                    new CollectionFolder
                    {
                        Name = "My Mix",
                        Path = "/emby/mix_library_ext",
                        InternalId = 125L
                    },
                    new CollectionFolder
                    {
                        Name = "My Audio",
                        Path = "/emby/audio_library_ext",
                        InternalId = 126L
                    }
                }
            });

        _ = _libraryManager
            .Setup(m => m.QueryItems(It.Is<InternalItemsQuery>(query =>
                true.Equals(query.Recursive)
                && false.Equals(query.IsVirtualItem)
                && true.Equals(query.HasPath)
                && query.IncludeItemTypes.Length == 2
                && "Movie".Equals(query.IncludeItemTypes[0], StringComparison.Ordinal)
                && "Series".Equals(query.IncludeItemTypes[1], StringComparison.Ordinal)
                && query.ParentIds.Length == 4
                && query.ParentIds.All(x => x >= 123 && x <= 126)
                && query.AnyProviderIdEquals.Any())
            ))
            .Returns(new QueryResult<BaseItem>
            {
                Items = Array.Empty<BaseItem>()
            });

        _ = _libraryManager
            .Setup(m => m.QueryItems(It.Is<InternalItemsQuery>(query =>
                true.Equals(query.Recursive)
                && false.Equals(query.IsVirtualItem)
                && true.Equals(query.HasPath)
                && query.IncludeItemTypes.Length == 2
                && "Movie".Equals(query.IncludeItemTypes[0], StringComparison.Ordinal)
                && "Series".Equals(query.IncludeItemTypes[1], StringComparison.Ordinal)
                && query.ParentIds.Length == 4
                && query.ParentIds.All(x => x >= 123 && x <= 126)
                && query.AnyProviderIdEquals.Count == 100
                && query.AnyProviderIdEquals.Contains(new KeyValuePair<string, string>(Plugin.PluginKey, "689"))
            )))
            .Returns(new QueryResult<BaseItem>
            {
                Items = new BaseItem[]
                {
                    new Movie
                    {
                        Name = "Гарри Поттер и философский камень",
                        InternalId = 103L,
                        Path = "/tmp/103",
                        ProviderIds = new ProviderIdDictionary(new Dictionary<string, string>
                        {
                            { Plugin.PluginKey, "689" }
                        })
                    },
                    new Movie
                    {
                        Name = "Один дома",
                        InternalId = 104L,
                        Path = "/tmp/104",
                        ProviderIds = new ProviderIdDictionary(new Dictionary<string, string>
                        {
                            { Plugin.PluginKey, "8124" }
                        }),
                        Collections = new[]
                        {
                            new LinkedItemInfo
                            {
                                Id = 321L,
                                Name = "Смотрим всей семьей",
                                ProviderIds = new ProviderIdDictionary()
                            }
                        }
                    }
                }
            });

        _ = _libraryManager
            .Setup(m => m.QueryItems(It.Is<InternalItemsQuery>(query =>
                "Смотрим всей семьей".Equals(query.Name, StringComparison.Ordinal)
                && true.Equals(query.Recursive)
                && query.IncludeItemTypes.Length == 1
                && "BoxSet".Equals(query.IncludeItemTypes[0], StringComparison.Ordinal))
            ))
            .Returns(new QueryResult<BaseItem>
            {
                TotalRecordCount = 0,
                Items = Array.Empty<BaseItem>()
            });

        // FindCollectionFolders
        _ = _libraryManager
            .Setup(m => m.QueryItems(It.Is<InternalItemsQuery>(query =>
                "Collections".Equals(query.Name, StringComparison.Ordinal)
                && true.Equals(query.Recursive)
                && true.Equals(query.IsFolder)
                && query.IncludeItemTypes.Length == 1
                && "CollectionFolder".Equals(query.IncludeItemTypes[0], StringComparison.Ordinal))
            ))
            .Returns(new QueryResult<BaseItem>
            {
                TotalRecordCount = 0,
                Items = new BaseItem[]
                {
                    new CollectionFolder
                    {
                        Name = "Collections",
                        Path = nameof(CreateKpCollectionsTaskTest_Execute_CollectionNotExists),
                        InternalId = 2L
                    }
                }
            });

        _ = _libraryManager
            .Setup(m => m.GetInternalItemIds(It.Is<InternalItemsQuery>(q => true.Equals(q.IsFolder))))
            .Returns(new[] { 1L });
        _ = _libraryManager
            .Setup(m => m.GetItemById(It.Is<long>(id => id == 1L)))
            .Returns(new CollectionFolder
            {
                Name = "Collections",
                Path = nameof(CreateKpCollectionsTaskTest_Execute_CollectionNotExists)
            });

        _ = _collectionManager
            .Setup(m => m.CreateCollection(It.IsAny<CollectionCreationOptions>()))
            .Returns((CollectionCreationOptions options) =>
                Task.FromResult(new BoxSet
                {
                    Name = options.Name,
                    ParentId = options.ParentId,
                    InternalId = 456L
                }));

        #endregion

        using CancellationTokenSource cancellationTokenSource = new();
        await _createKpCollectionsTaskTest.Execute(cancellationTokenSource.Token, new EmbyProgress());

        // check that no errors in API
        _activityManager.Verify(a => a.Create(It.IsAny<ActivityLogEntry>()), Times.Never());
        _notificationManager.Verify(n => n.SendNotification(It.IsAny<NotificationRequest>()), Times.Never());

        _applicationPaths.VerifyGet(ap => ap.PluginConfigurationsPath, Times.Once());
        _collectionManager.Verify(cm => cm.CreateCollection(It.IsAny<CollectionCreationOptions>()), Times.Once());
        _itemRepository.Verify(lm => lm.GetExtradataTypeId("LibraryOptions"), Times.Exactly(4));
        _itemRepository.Verify(lm => lm.GetItemExtradataValue(It.IsInRange(123L, 126L, Range.Inclusive), 0), Times.Exactly(4));
        _libraryManager.Verify(lm => lm.QueryItems(It.IsAny<InternalItemsQuery>()), Times.Exactly(10));
        _logManager.Verify(lm => lm.GetLogger(It.IsAny<string>()), Times.Exactly(4));
        _xmlSerializer.Verify(xs => xs.DeserializeFromFile(typeof(PluginConfiguration), $"{nameof(CreateKpCollectionsTaskTest_Execute_CollectionNotExists)}/EmbyKinopoiskRu.xml"), Times.Once());

        VerifyNoOtherCalls();

        Logger.Info($"Finish '{nameof(CreateKpCollectionsTaskTest_Execute_CollectionNotExists)}'");
    }
}
