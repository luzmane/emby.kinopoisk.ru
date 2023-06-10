using System.Net;
using System.Text;

using EmbyKinopoiskRu.ScheduledTasks;
using EmbyKinopoiskRu.Tests.EmbyMock;

using FluentAssertions;

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

        _updateKinopoiskPluginTask.IsHidden.Should().BeFalse("this is default task config");
        _updateKinopoiskPluginTask.IsEnabled.Should().BeTrue("this is default task config");
        _updateKinopoiskPluginTask.IsLogged.Should().BeTrue("this is default task config");
        _updateKinopoiskPluginTask.Key.Should().NotBeNull("this is key of the task");
        _updateKinopoiskPluginTask.GetDefaultTriggers().Should().NotBeEmpty("it should have one trigger");

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

        _updateKinopoiskPluginTask.Name.Should().Be("Обновить плагин Кинопоиска", "this is name of the task on Russian");
        _updateKinopoiskPluginTask.Description.Should().Be("Скачать и установить новую версию плагина Кинопоиска с GitHub", "this is description of the task on Russian");
        _updateKinopoiskPluginTask.Category.Should().Be("Плагин Кинопоиска", "this is category of the task on Russian");

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

        _updateKinopoiskPluginTask.Name.Should().Be("Update Kinopoisk Plugin", "this is name of the task on English");
        _updateKinopoiskPluginTask.Description.Should().Be("Update Kinopoisk Plugin from GitHub", "this is description of the task on English");
        _updateKinopoiskPluginTask.Category.Should().Be("Kinopoisk Plugin", "this is category of the task on English");

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

        _updateKinopoiskPluginTask.Name.Should().Be("Оновити плагін Кінопошуку", "this is name of the task on Ukranian");
        _updateKinopoiskPluginTask.Description.Should().Be("Завантажити та встановити нову версію плагіна Кінопошуку з GitHub", "this is description of the task on Ukranian");
        _updateKinopoiskPluginTask.Category.Should().Be("Плагін Кінопошуку", "this is category of the task on Ukranian");

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

        _updateKinopoiskPluginTask.Name.Should().Be("Update Kinopoisk Plugin", "this is name of the task on not available language");
        _updateKinopoiskPluginTask.Description.Should().Be("Update Kinopoisk Plugin from GitHub", "this is description of the task on not available language");
        _updateKinopoiskPluginTask.Category.Should().Be("Kinopoisk Plugin", "this is category of the task on not available language");

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
