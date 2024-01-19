using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using EmbyKinopoiskRu.Helper;
using EmbyKinopoiskRu.ScheduledTasks.Model;

using MediaBrowser.Common.Net;
using MediaBrowser.Common.Progress;
using MediaBrowser.Common.Updates;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Net;
using MediaBrowser.Model.Serialization;
using MediaBrowser.Model.Tasks;
using MediaBrowser.Model.Updates;

namespace EmbyKinopoiskRu.ScheduledTasks
{
    /// <inheritdoc />
    public class UpdateKinopoiskPluginTask : IScheduledTask, IConfigurableScheduledTask
    {
        private static readonly Regex Version = new Regex(".*(?<version>\\d+\\.\\d+\\.\\d+).*", RegexOptions.Compiled);

        private const string DLL_NAME = "EmbyKinopoiskRu.dll";

        /// <inheritdoc />
        public bool IsHidden => false;

        /// <inheritdoc />
        public bool IsEnabled => true;

        /// <inheritdoc />
        public bool IsLogged => true;

        /// <inheritdoc />
        public string Name => GetTranslation().Name;

        /// <inheritdoc />
        public string Key => "KinopoiskNewVersion";

        /// <inheritdoc />
        public string Description => GetTranslation().Description;

        /// <inheritdoc />
        public string Category => GetTranslation().Category;

        private readonly IHttpClient _httpClient;
        private readonly IJsonSerializer _jsonSerializer;
        private readonly ILogger _logger;
        private readonly IInstallationManager _installationManager;
        private readonly IServerConfigurationManager _serverConfigurationManager;
        private readonly Dictionary<string, TaskTranslation> _translations = new Dictionary<string, TaskTranslation>();
        private readonly Dictionary<string, string> _availableTranslations;

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateKinopoiskPluginTask"/> class.
        /// </summary>
        /// <param name="httpClient">Instance of the <see cref="IHttpClient"/> interface.</param>
        /// <param name="jsonSerializer">Instance of the <see cref="IJsonSerializer"/> interface.</param>
        /// <param name="logManager">Instance of the <see cref="ILogManager"/> interface.</param>
        /// <param name="installationManager">Instance of the <see cref="IInstallationManager"/> interface.</param>
        /// <param name="serverConfigurationManager">Instance of the <see cref="IServerConfigurationManager"/> interface.</param>
        public UpdateKinopoiskPluginTask(
                 IHttpClient httpClient,
                 IJsonSerializer jsonSerializer,
                 ILogManager logManager,
                 IInstallationManager installationManager,
                 IServerConfigurationManager serverConfigurationManager)
        {
            _httpClient = httpClient;
            _jsonSerializer = jsonSerializer;
            _logger = logManager.GetLogger(GetType().Name);
            _installationManager = installationManager;
            _serverConfigurationManager = serverConfigurationManager;

            _availableTranslations = EmbyHelper.GetAvailableTransactions($"ScheduledTasks.{Key}");
        }

        /// <inheritdoc />
        public IEnumerable<TaskTriggerInfo> GetDefaultTriggers()
        {
            return new List<TaskTriggerInfo>
            {
                new TaskTriggerInfo
                {
                    Type = TaskTriggerInfo.TriggerWeekly,
                    DayOfWeek = (DayOfWeek)new Random().Next(7),
                    TimeOfDayTicks = new Random().Next(96) * 9_000_000_000,
                    MaxRuntimeTicks = 36_000_000_000
                }
            };
        }

        /// <inheritdoc />
        public async Task Execute(CancellationToken cancellationToken, IProgress<double> progress)
        {
            Version version = typeof(UpdateKinopoiskPluginTask).Assembly.GetName().Version ?? throw new Exception("Unable to get dll version");
            var currentVersion = $"{version.Major}.{version.Minor}.{version.Build}";

            GitHubLatestReleaseResponse release = await GetGitHubLatestReleaseAsync(cancellationToken);

            progress.Report(40d);

            if (string.IsNullOrWhiteSpace(release.tag_name))
            {
                _logger.Error("Unable to receive plugin version - it's null");
            }
            else if (currentVersion.Equals(release.tag_name, StringComparison.Ordinal))
            {
                _logger.Info("Plugin version didn't change");
            }
            else
            {
                _logger.Info($"Update plugin from version {currentVersion} to version {release.tag_name}");
                var package = new PackageVersionInfo
                {
                    guid = Plugin.PluginGuid,
                    name = Plugin.PluginName,
                    versionStr = release.tag_name,
                    classification = PackageVersionClass.Release,
                    description = release.body,
                    requiredVersionStr = "4.7.9",
                    sourceUrl = release.assets[0].browser_download_url,
                    targetFilename = DLL_NAME,
                    infoUrl = release.html_url,
                    runtimes = "netcore"
                };
                try
                {
                    await _installationManager.InstallPackage(package, isPlugin: true, new SimpleProgress<double>(), cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
                    _logger.Info("Plugin installed successfully");
                    progress.Report(100d);
                }
                catch (OperationCanceledException)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        _logger.Info("Cancelation was requested");
                        throw;
                    }
                }
                catch (HttpException exception2)
                {
                    _logger.ErrorException("Error downloading {0}", exception2, package.name);
                }
                catch (Exception ex)
                {
                    _logger.ErrorException("Error updating {0}", ex, package.name);
                }
            }
        }

        private async Task<GitHubLatestReleaseResponse> GetGitHubLatestReleaseAsync(CancellationToken cancellationToken)
        {
            var options = new HttpRequestOptions
            {
                CancellationToken = cancellationToken,
                Url = "https://api.github.com/repos/luzmane/emby.kinopoisk.ru/releases/latest",
                LogResponse = true,
                CacheLength = TimeSpan.FromHours(12),
                CacheMode = CacheMode.Unconditional,
                TimeoutMs = 180000,
                EnableDefaultUserAgent = true,
            };
            using (var reader = new StreamReader((await _httpClient.GetResponse(options)).Content))
            {
                var latestVersionJson = await reader.ReadToEndAsync();
                var gitResponse = _jsonSerializer.DeserializeFromString<GitHubLatestReleaseResponse>(latestVersionJson);
                gitResponse.tag_name = PrepareTagName(gitResponse.tag_name);
                return gitResponse;
            }
        }
        private TaskTranslation GetTranslation()
        {
            return EmbyHelper.GetTaskTranslation(_translations, _serverConfigurationManager, _jsonSerializer, _availableTranslations);
        }
        private string PrepareTagName(string tagName)
        {
            Match match = Version.Match(tagName);
            if (match.Success)
            {
                var version = match.Groups["version"].Value;
                _logger.Info("Converting tag '{0}' to '{1}'", tagName, version);
                return version;
            }
            else
            {
                _logger.Info("Unable to parse tag: '{0}'", tagName);
                return string.Empty;
            }
        }
    }
}
