using EmbyKinopoiskRu.Configuration;
using EmbyKinopoiskRu.Tests.EmbyMock;

using MediaBrowser.Common.Configuration;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Collections;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Extensions;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Activity;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Model.Globalization;
using MediaBrowser.Model.IO;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Serialization;

namespace EmbyKinopoiskRu.Tests;
/*
dotnet test --collect:"XPlat Code Coverage"
reportgenerator -reports:"coverage.cobertura.xml" -targetdir:"coveragereport" -reporttypes:Html
*/
public abstract class BaseTest
{
    private const string KINOPOISK_DEV_TOKEN = "8DA0EV2-KTP4A5Q-G67QP3K-S2VFBX7";
    private const string KINOPOISK_UNOFFICIAL_TOKEN = "0f162131-81c1-4979-b46c-3eea4263fb11";

    private readonly NLog.ILogger _logger;


    #region Mock
    protected readonly Mock<ILogManager> _logManager = new();
    protected readonly Mock<IDirectoryService> _directoryService = new();
    protected readonly Mock<IFileSystem> _fileSystem = new();
    protected readonly Mock<IApplicationPaths> _applicationPaths = new();
    protected readonly Mock<IXmlSerializer> _xmlSerializer = new();
    protected readonly Mock<IActivityManager> _activityManager = new();
    protected readonly Mock<ILibraryManager> _libraryManager = new();
    protected readonly Mock<ICollectionManager> _collectionManager = new();
    protected readonly Mock<ILocalizationManager> _localizationManager = new();
    protected readonly Mock<IServerConfigurationManager> _serverConfigurationManager = new();
    protected readonly Mock<IServerApplicationHost> _serverApplicationHost = new();

    #endregion

    #region Not Mock
    protected readonly EmbyHttpClient _httpClient = new();
    protected readonly EmbyJsonSerializer _jsonSerializer = new();
    protected readonly PluginConfiguration _pluginConfiguration;

    #endregion

    protected BaseTest(NLog.ILogger logger)
    {
        _logger = logger;

        _pluginConfiguration = new() { CreateCollections = false };


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
        CollectionFolder.XmlSerializer = _xmlSerializer.Object;
        CollectionFolder.ApplicationHost = _serverApplicationHost.Object;
        StringExtensions.LocalizationManager = _localizationManager.Object;

        _ = new Plugin(
            _applicationPaths.Object,
            _xmlSerializer.Object,
            _logManager.Object,
            _httpClient,
            _jsonSerializer,
            _activityManager.Object,
            _libraryManager.Object,
            _collectionManager.Object
        );
        Plugin.Instance.SetAttributes("EmbyKinopoiskRu.dll", "", new Version(1, 0, 0));
    }

    protected virtual void ConfigLibraryManager()
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
            .Returns((long itemId, List<ItemLinkType> types) => new());

        _ = _libraryManager
            .Setup(lm => lm.UpdateItem(
                It.IsAny<BaseItem>(),
                It.IsAny<BaseItem>(),
                It.IsAny<ItemUpdateType>(),
                It.IsAny<MetadataRefreshOptions>()));

        _ = _libraryManager
            .Setup(lm => lm.AddVirtualFolder("Collections", It.IsAny<LibraryOptions>(), true));

    }
    protected virtual void ConfigXmlSerializer()
    {
        _ = _xmlSerializer
            .Setup(xs => xs.DeserializeFromFile(typeof(PluginConfiguration), It.IsAny<string>()))
            .Returns(_pluginConfiguration);

    }

    protected void VerifyNoOtherCalls()
    {
        _logManager.VerifyNoOtherCalls();
        _directoryService.VerifyNoOtherCalls();
        _fileSystem.VerifyNoOtherCalls();
        _applicationPaths.VerifyNoOtherCalls();
        _xmlSerializer.VerifyNoOtherCalls();
        _activityManager.VerifyNoOtherCalls();
        _libraryManager.VerifyNoOtherCalls();
        _collectionManager.VerifyNoOtherCalls();
        _localizationManager.VerifyNoOtherCalls();
        _serverConfigurationManager.VerifyNoOtherCalls();
        _serverApplicationHost.VerifyNoOtherCalls();
    }
    private void PrintMockInvocations(Mock mock)
    {
        _logger.Info($"Name: {mock?.Object.GetType().Name}");
        foreach (IInvocation? invocation in mock!.Invocations)
        {
            _logger.Info(invocation);
        }
    }
    protected void PrintMocks()
    {
        PrintMockInvocations(_logManager);
        PrintMockInvocations(_directoryService);
        PrintMockInvocations(_fileSystem);
        PrintMockInvocations(_applicationPaths);
        PrintMockInvocations(_xmlSerializer);
        PrintMockInvocations(_activityManager);
        PrintMockInvocations(_libraryManager);
        PrintMockInvocations(_collectionManager);
        PrintMockInvocations(_localizationManager);
        PrintMockInvocations(_serverConfigurationManager);
        PrintMockInvocations(_serverApplicationHost);
    }

    protected string GetKinopoiskDevToken()
    {
        var token = Environment.GetEnvironmentVariable("KINOPOISK_DEV_TOKEN");
        _logger.Info($"Env token length is: {(token != null ? token.Length : 0)}");
        return string.IsNullOrWhiteSpace(token) ? KINOPOISK_DEV_TOKEN : token;
    }
    protected string GetKinopoiskUnofficialToken()
    {
        var token = Environment.GetEnvironmentVariable("KINOPOISK_UNOFFICIAL_TOKEN");
        _logger.Info($"Env token length is: {(token != null ? token.Length : 0)}");
        return string.IsNullOrWhiteSpace(token) ? KINOPOISK_UNOFFICIAL_TOKEN : token;
    }
}
