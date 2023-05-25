using System.Net;
using System.Text;

using EmbyKinopoiskRu.ScheduledTasks;
using EmbyKinopoiskRu.Tests.EmbyMock;

using MediaBrowser.Common.Net;
using MediaBrowser.Common.Progress;
using MediaBrowser.Common.Updates;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Model.Updates;

using NLog;

namespace EmbyKinopoiskRu.Tests.Common;

[Collection("Sequential")]
public class UpdateKinopoiskPluginTaskTest : BaseTest
{
    private static readonly ILogger Logger = LogManager.GetLogger(nameof(UpdateKinopoiskPluginTaskTest));

    private readonly UpdateKinopoiskPluginTask _updateKinopoiskPluginTask;
    private readonly Mock<IInstallationManager> _installationManager = new();


    #region Test configs
    public UpdateKinopoiskPluginTaskTest() : base(Logger)
    {
        _updateKinopoiskPluginTask = new(
            _httpClient,
            _jsonSerializer,
            _logManager.Object,
            _installationManager.Object,
            _serverConfigurationManager.Object);
    }
    #endregion

    [Fact]
    public void UpdateKinopoiskPluginTask_ForCodeCoverage()
    {
        Logger.Info($"Start '{nameof(UpdateKinopoiskPluginTask_ForCodeCoverage)}'");

        Assert.False(_updateKinopoiskPluginTask.IsHidden);
        Assert.True(_updateKinopoiskPluginTask.IsEnabled);
        Assert.True(_updateKinopoiskPluginTask.IsLogged);
        Assert.NotNull(_updateKinopoiskPluginTask.Key);

        _ = Assert.Single(_updateKinopoiskPluginTask.GetDefaultTriggers());

        _logManager.Verify(lm => lm.GetLogger("KinopoiskRu"), Times.Once());
        _logManager.Verify(lm => lm.GetLogger("UpdateKinopoiskPluginTask"), Times.Once());

        VerifyNoOtherCalls();

        Logger.Info($"Finished '{nameof(UpdateKinopoiskPluginTask_ForCodeCoverage)}'");
    }

    [Fact]
    public void UpdateKinopoiskPluginTask_GetTranslation_RU()
    {
        Logger.Info($"Start '{nameof(UpdateKinopoiskPluginTask_GetTranslation_RU)}'");

        _ = _serverConfigurationManager
            .SetupGet(scm => scm.Configuration)
            .Returns(new ServerConfiguration() { UICulture = "ru" });

        Assert.Equal("Обновить плагин Кинопоиска", _updateKinopoiskPluginTask.Name);
        Assert.Equal("Скачать и установить новую версию плагина Кинопоиска с GitHub", _updateKinopoiskPluginTask.Description);
        Assert.Equal("Плагин Кинопоиска", _updateKinopoiskPluginTask.Category);

        _logManager.Verify(lm => lm.GetLogger("KinopoiskRu"), Times.Once());
        _logManager.Verify(lm => lm.GetLogger("UpdateKinopoiskPluginTask"), Times.Once());
        _serverConfigurationManager.VerifyGet(scm => scm.Configuration, Times.Exactly(6));
        VerifyNoOtherCalls();

        Logger.Info($"Finished '{nameof(UpdateKinopoiskPluginTask_GetTranslation_RU)}'");
    }

    [Fact]
    public void UpdateKinopoiskPluginTask_GetTranslation_EnUs()
    {
        Logger.Info($"Start '{nameof(UpdateKinopoiskPluginTask_GetTranslation_EnUs)}'");

        _ = _serverConfigurationManager
            .SetupGet(scm => scm.Configuration)
            .Returns(new ServerConfiguration() { UICulture = "en-us" });

        Assert.Equal("Update Kinopoisk Plugin", _updateKinopoiskPluginTask.Name);
        Assert.Equal("Update Kinopoisk Plugin from GitHub", _updateKinopoiskPluginTask.Description);
        Assert.Equal("Kinopoisk Plugin", _updateKinopoiskPluginTask.Category);

        _logManager.Verify(lm => lm.GetLogger("KinopoiskRu"), Times.Once());
        _logManager.Verify(lm => lm.GetLogger("UpdateKinopoiskPluginTask"), Times.Once());
        _serverConfigurationManager.VerifyGet(scm => scm.Configuration, Times.Exactly(6));
        VerifyNoOtherCalls();

        Logger.Info($"Finished '{nameof(UpdateKinopoiskPluginTask_GetTranslation_EnUs)}'");
    }

    [Fact]
    public void UpdateKinopoiskPluginTask_GetTranslation_UK()
    {
        Logger.Info($"Start '{nameof(UpdateKinopoiskPluginTask_GetTranslation_UK)}'");

        _ = _serverConfigurationManager
            .SetupGet(scm => scm.Configuration)
            .Returns(new ServerConfiguration() { UICulture = "uk" });

        Assert.Equal("Оновити плагін Кінопошуку", _updateKinopoiskPluginTask.Name);
        Assert.Equal("Завантажити та встановити нову версію плагіна Кінопошуку з GitHub", _updateKinopoiskPluginTask.Description);
        Assert.Equal("Плагін Кінопошуку", _updateKinopoiskPluginTask.Category);

        _logManager.Verify(lm => lm.GetLogger("KinopoiskRu"), Times.Once());
        _logManager.Verify(lm => lm.GetLogger("UpdateKinopoiskPluginTask"), Times.Once());
        _serverConfigurationManager.VerifyGet(scm => scm.Configuration, Times.Exactly(6));
        VerifyNoOtherCalls();

        Logger.Info($"Finished '{nameof(UpdateKinopoiskPluginTask_GetTranslation_UK)}'");
    }

    [Fact]
    public void UpdateKinopoiskPluginTask_GetTranslation_BG()
    {
        Logger.Info($"Start '{nameof(UpdateKinopoiskPluginTask_GetTranslation_BG)}'");

        _ = _serverConfigurationManager
            .SetupGet(scm => scm.Configuration)
            .Returns(new ServerConfiguration() { UICulture = "bg" });

        Assert.Equal("Update Kinopoisk Plugin", _updateKinopoiskPluginTask.Name);
        Assert.Equal("Update Kinopoisk Plugin from GitHub", _updateKinopoiskPluginTask.Description);
        Assert.Equal("Kinopoisk Plugin", _updateKinopoiskPluginTask.Category);

        _logManager.Verify(lm => lm.GetLogger("KinopoiskRu"), Times.Once());
        _logManager.Verify(lm => lm.GetLogger("UpdateKinopoiskPluginTask"), Times.Once());
        _serverConfigurationManager.VerifyGet(scm => scm.Configuration, Times.Exactly(6));
        VerifyNoOtherCalls();

        Logger.Info($"Finished '{nameof(UpdateKinopoiskPluginTask_GetTranslation_BG)}'");
    }

    [Fact]
    public async void UpdateKinopoiskPluginTask_Execute_NoChanges()
    {
        Logger.Info($"Start '{nameof(UpdateKinopoiskPluginTask_Execute_NoChanges)}'");

        using var cancellationTokenSource = new CancellationTokenSource();
        await _updateKinopoiskPluginTask.Execute(cancellationTokenSource.Token, new EmbyProgress());

        _logManager.Verify(lm => lm.GetLogger(It.IsAny<string>()), Times.Exactly(2));
        VerifyNoOtherCalls();

        Logger.Info($"Finished '{nameof(UpdateKinopoiskPluginTask_Execute_NoChanges)}'");
    }

    [Fact]
    public async void UpdateKinopoiskPluginTask_Execute_Updated()
    {
        Logger.Info($"Start '{nameof(UpdateKinopoiskPluginTask_Execute_Updated)}'");

        using HttpResponseInfo response = new()
        {
            Content = new MemoryStream(Encoding.UTF8.GetBytes(/*lang=json,strict*/ "{\"html_url\":\"https://\",\"tag_name\":\"1.0.0\",\"assets\":[{\"name\":\"EmbyKinopoiskRu.dll\",\"content_type\":\"program\",\"browser_download_url\":\"https://\"}],\"body\":\"description\"}")),
            StatusCode = HttpStatusCode.OK
        };
        _httpClient.ReturnResponse = response;

        using var cancellationTokenSource = new CancellationTokenSource();
        await _updateKinopoiskPluginTask.Execute(cancellationTokenSource.Token, new EmbyProgress());

        _logManager.Verify(lm => lm.GetLogger(It.IsAny<string>()), Times.Exactly(2));
        _installationManager.Verify(
            im => im.InstallPackage(
                It.IsAny<PackageVersionInfo>(),
                true,
                It.IsAny<SimpleProgress<double>>(),
                It.IsAny<CancellationToken>()),
            Times.Once());
        _installationManager.VerifyNoOtherCalls();
        VerifyNoOtherCalls();

        Logger.Info($"Finished '{nameof(UpdateKinopoiskPluginTask_Execute_Updated)}'");
    }
}
