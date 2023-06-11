using EmbyKinopoiskRu.Configuration;
using EmbyKinopoiskRu.ScheduledTasks;
using EmbyKinopoiskRu.Tests.EmbyMock;

using FluentAssertions;

using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Querying;

namespace EmbyKinopoiskRu.Tests.KinopoiskDev;

[Collection("Sequential")]
public class CreateKinopoiskIdTaskTest : BaseTest
{
    private static readonly NLog.ILogger Logger = NLog.LogManager.GetLogger(nameof(KpEpisodeProviderTest));

    private readonly CreateKinopoiskIdTask _createKinopoiskIdTask;


    #region Test configs
    public CreateKinopoiskIdTaskTest() : base(Logger)
    {
        _pluginConfiguration.Token = GetKinopoiskDevToken();

        ConfigLibraryManager();

        ConfigXmlSerializer();

        _createKinopoiskIdTask = new(
            _logManager.Object,
            _libraryManager.Object,
            _serverConfigurationManager.Object,
            _jsonSerializer);
    }

    #endregion

    [Fact]
    public void CreateKinopoiskIdTask_ForCodeCoverage()
    {
        Logger.Info($"Start '{nameof(CreateKinopoiskIdTask_ForCodeCoverage)}'");

        _createKinopoiskIdTask.IsHidden.Should().BeFalse("this is default task config");
        _createKinopoiskIdTask.IsEnabled.Should().BeFalse("this is default task config");
        _createKinopoiskIdTask.IsLogged.Should().BeTrue("this is default task config");
        _createKinopoiskIdTask.Key.Should().NotBeNull("key is hardcoded");

        _createKinopoiskIdTask.GetDefaultTriggers().Should().BeEmpty("there is no triggers defined");

        _logManager.Verify(lm => lm.GetLogger("KinopoiskRu"), Times.Once());
        _logManager.Verify(lm => lm.GetLogger("CreateKinopoiskIdTask"), Times.Once());

        VerifyNoOtherCalls();

        Logger.Info($"Finished '{nameof(CreateKinopoiskIdTask_ForCodeCoverage)}'");
    }

    [Fact]
    public void CreateKinopoiskIdTask_GetTranslation_RU()
    {
        Logger.Info($"Start '{nameof(CreateKinopoiskIdTask_GetTranslation_RU)}'");

        _ = _serverConfigurationManager
            .SetupGet(scm => scm.Configuration)
            .Returns(new ServerConfiguration() { UICulture = "ru" });

        _createKinopoiskIdTask.Name.Should().Be("Добавить ID Кинопоиска по ключам IMDB, TMDB", "this is the name of the task");
        _createKinopoiskIdTask.Description.Should().Be("Добавить ID Кинопоиска, ища через API по ID IMDB и TMDB. Поддерживает только kinopoisk.dev", "this is the description of the task");
        _createKinopoiskIdTask.Category.Should().Be("Плагин Кинопоиска", "this is the category of the task");

        _logManager.Verify(lm => lm.GetLogger("KinopoiskRu"), Times.Once());
        _logManager.Verify(lm => lm.GetLogger("CreateKinopoiskIdTask"), Times.Once());
        _serverConfigurationManager.VerifyGet(scm => scm.Configuration, Times.Exactly(6));
        VerifyNoOtherCalls();

        Logger.Info($"Finished '{nameof(CreateKinopoiskIdTask_GetTranslation_RU)}'");
    }

    [Fact]
    public void CreateKinopoiskIdTask_GetTranslation_EnUs()
    {
        Logger.Info($"Start '{nameof(CreateKinopoiskIdTask_GetTranslation_EnUs)}'");

        _ = _serverConfigurationManager
            .SetupGet(scm => scm.Configuration)
            .Returns(new ServerConfiguration() { UICulture = "en-us" });

        _createKinopoiskIdTask.Name.Should().Be("Add KinopoiskId based on IMDB, TMDB", "this is the name of the task");
        _createKinopoiskIdTask.Description.Should().Be("Add KinopoiskId searching them by IMDB and TMDB ids. Support kinopoisk.dev only", "this is the description of the task");
        _createKinopoiskIdTask.Category.Should().Be("Kinopoisk Plugin", "this is the category of the task");

        _logManager.Verify(lm => lm.GetLogger("KinopoiskRu"), Times.Once());
        _logManager.Verify(lm => lm.GetLogger("CreateKinopoiskIdTask"), Times.Once());
        _serverConfigurationManager.VerifyGet(scm => scm.Configuration, Times.Exactly(6));
        VerifyNoOtherCalls();

        Logger.Info($"Finished '{nameof(CreateKinopoiskIdTask_GetTranslation_EnUs)}'");
    }

    [Fact]
    public void CreateKinopoiskIdTask_GetTranslation_UK()
    {
        Logger.Info($"Start '{nameof(CreateKinopoiskIdTask_GetTranslation_UK)}'");

        _ = _serverConfigurationManager
            .SetupGet(scm => scm.Configuration)
            .Returns(new ServerConfiguration() { UICulture = "uk" });

        _createKinopoiskIdTask.Name.Should().Be("Додати ID Кінопошуку за ключами IMDB, TMDB", "this is the name of the task");
        _createKinopoiskIdTask.Description.Should().Be("Додати ID Кінопошуку, шукаючи через API за ID IMDB та TMDB. Підтримує лише kinopoisk.dev", "this is the description of the task");
        _createKinopoiskIdTask.Category.Should().Be("Плагін Кінопошуку", "this is the category of the task");

        _logManager.Verify(lm => lm.GetLogger("KinopoiskRu"), Times.Once());
        _logManager.Verify(lm => lm.GetLogger("CreateKinopoiskIdTask"), Times.Once());
        _serverConfigurationManager.VerifyGet(scm => scm.Configuration, Times.Exactly(6));
        VerifyNoOtherCalls();

        Logger.Info($"Finished '{nameof(CreateKinopoiskIdTask_GetTranslation_UK)}'");
    }

    [Fact]
    public void CreateKinopoiskIdTask_GetTranslation_BG()
    {
        Logger.Info($"Start '{nameof(CreateKinopoiskIdTask_GetTranslation_BG)}'");

        _ = _serverConfigurationManager
            .SetupGet(scm => scm.Configuration)
            .Returns(new ServerConfiguration() { UICulture = "bg" });

        _createKinopoiskIdTask.Name.Should().Be("Add KinopoiskId based on IMDB, TMDB", "this is the name of the task");
        _createKinopoiskIdTask.Description.Should().Be("Add KinopoiskId searching them by IMDB and TMDB ids. Support kinopoisk.dev only", "this is the description of the task");
        _createKinopoiskIdTask.Category.Should().Be("Kinopoisk Plugin", "this is the category of the task");

        _logManager.Verify(lm => lm.GetLogger("KinopoiskRu"), Times.Once());
        _logManager.Verify(lm => lm.GetLogger("CreateKinopoiskIdTask"), Times.Once());
        _serverConfigurationManager.VerifyGet(scm => scm.Configuration, Times.Exactly(6));
        VerifyNoOtherCalls();

        Logger.Info($"Finished '{nameof(CreateKinopoiskIdTask_GetTranslation_BG)}'");
    }

    [Fact]
    public async void CreateKinopoiskIdTask_Execute()
    {
        Logger.Info($"Start '{nameof(CreateKinopoiskIdTask_Execute)}'");

        _ = _applicationPaths
            .SetupGet(m => m.PluginConfigurationsPath)
            .Returns("CreateKinopoiskIdTask_Execute");

        _ = _serverConfigurationManager
            .SetupGet(scm => scm.Configuration)
            .Returns(new ServerConfiguration() { UICulture = "ru" });

        _ = _libraryManager // Search for items to update
            .Setup(m => m.QueryItems(It.Is<InternalItemsQuery>(query =>
                query.IncludeItemTypes.Length == 2
                && nameof(Movie).Equals(query.IncludeItemTypes[0], StringComparison.Ordinal)
                && nameof(Series).Equals(query.IncludeItemTypes[1], StringComparison.Ordinal)
                && query.MissingAnyProviderId.Length == 1
                && Plugin.PluginKey.Equals(query.MissingAnyProviderId[0], StringComparison.Ordinal)
                && query.HasAnyProviderId.Length == 2
                && MetadataProviders.Imdb.ToString().Equals(query.HasAnyProviderId[0], StringComparison.Ordinal)
                && MetadataProviders.Tmdb.ToString().Equals(query.HasAnyProviderId[1], StringComparison.Ordinal))))
            .Returns(new QueryResult<BaseItem>()
            {
                TotalRecordCount = 3,
                Items = new BaseItem[] {
                    new Movie() {
                        Name = "Гарри Поттер и Тайная комната",
                        InternalId = 101L,
                        ProviderIds = new(new Dictionary<string, string>()
                        {
                            { MetadataProviders.Imdb.ToString(), "tt0295297" },
                            { MetadataProviders.Tmdb.ToString(), "672" }
                        })
                    },
                    new Movie() {
                        Name = "Гарри Поттер и узник Азкабана",
                        InternalId = 102L,
                        ProviderIds = new(new Dictionary<string, string>()
                        {
                            { MetadataProviders.Tmdb.ToString(), "673" }
                        }),
                    },
                    new Movie() {
                        Name = "Гарри Поттер и Кубок огня",
                        InternalId = 103L,
                        ProviderIds = new(new Dictionary<string, string>()
                        {
                            { MetadataProviders.Imdb.ToString(), "tt0330373" },
                        })
                    }
                }
            });

        using var cancellationTokenSource = new CancellationTokenSource();
        await _createKinopoiskIdTask.Execute(cancellationTokenSource.Token, new EmbyProgress());

        _logManager.Verify(lm => lm.GetLogger(It.IsAny<string>()), Times.Exactly(4));
        _applicationPaths.VerifyGet(ap => ap.PluginConfigurationsPath, Times.Once());
        _xmlSerializer.Verify(xs => xs.DeserializeFromFile(typeof(PluginConfiguration), "CreateKinopoiskIdTask_Execute/EmbyKinopoiskRu.xml"), Times.Once());
        _libraryManager.Verify(lm => lm.QueryItems(It.IsAny<InternalItemsQuery>()), Times.Once());
        _libraryManager.Verify(lm => lm.GetItemLinks(It.IsInRange(101L, 103L, Moq.Range.Inclusive), It.IsAny<List<ItemLinkType>>()), Times.Exactly(3));
        _libraryManager.Verify(lm => lm.UpdateItem(It.IsAny<BaseItem>(), It.IsAny<BaseItem>(), ItemUpdateType.MetadataEdit, null), Times.Exactly(3));
        VerifyNoOtherCalls();

        Logger.Info($"Finished '{nameof(CreateKinopoiskIdTask_Execute)}'");
    }


}
