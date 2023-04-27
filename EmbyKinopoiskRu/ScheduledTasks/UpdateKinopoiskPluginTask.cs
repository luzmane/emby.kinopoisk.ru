using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

using EmbyKinopoiskRu.Helper;
using EmbyKinopoiskRu.ScheduledTasks.Model;

using MediaBrowser.Common.Net;
using MediaBrowser.Common.Progress;
using MediaBrowser.Common.Updates;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Model.Extensions;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Net;
using MediaBrowser.Model.Serialization;
using MediaBrowser.Model.Tasks;
using MediaBrowser.Model.Updates;

namespace EmbyKinopoiskRu.ScheduledTasks
{
    public class UpdateKinopoiskPluginTask : IScheduledTask, IConfigurableScheduledTask
    {
        private const string DLL_NAME = "EmbyKinopoiskRu.dll";

        public bool IsHidden => false;
        public bool IsEnabled => true;
        public bool IsLogged => true;
        public string Name => GetTranslation().Name;
        public string Key => "KinopoiskNewVersion";
        public string Description => GetTranslation().Description;
        public string Category => GetTranslation().Category;

        private readonly IHttpClient _httpClient;
        private readonly IJsonSerializer _jsonSerializer;
        private readonly ILogger _logger;
        private readonly IInstallationManager _installationManager;
        private readonly IServerConfigurationManager _serverConfigurationManager;
        private readonly Dictionary<string, TaskTranslation> _translations = new Dictionary<string, TaskTranslation>();
        private readonly Dictionary<string, string> _availableTranslations = new Dictionary<string, string>();

        public UpdateKinopoiskPluginTask(
            IHttpClient httpClient,
            IJsonSerializer jsonSerializer,
            ILogManager logManager,
            IInstallationManager installationManager,
            IServerConfigurationManager serverConfigurationManager)
        {
            _httpClient = httpClient;
            _jsonSerializer = jsonSerializer;
            _logger = logManager.GetLogger("CheckNewPluginVersion");
            _installationManager = installationManager;
            _serverConfigurationManager = serverConfigurationManager;

            _availableTranslations = EmbyHelper.GetAvailableTransactionsForTasks(Key);
        }
        public IEnumerable<TaskTriggerInfo> GetDefaultTriggers()
        {
            return new List<TaskTriggerInfo>()
            {
                new TaskTriggerInfo
                {
                    Type = TaskTriggerInfo.TriggerWeekly,
                    DayOfWeek = (DayOfWeek)new Random().Next(7),
                    TimeOfDayTicks = new Random().Next(96) * 9000000000,
                    MaxRuntimeTicks = 36000000000
                }
            };
        }
        public async Task Execute(CancellationToken cancellationToken, IProgress<double> progress)
        {
            Version version = Assembly.GetExecutingAssembly().GetName().Version ?? throw new Exception("Unable to get dll version");
            var currentVersion = $"{version.Major}.{version.Minor}.{version.Build}";

            GitHubLatestReleaseResponse release = await GetGitHubLatestRelease(cancellationToken);

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

                var checksum = await PrepareMd5Checksum(release.assets, cancellationToken);
                _logger.Info($"Calculated md5 hash: '{checksum}'");

                var package = new PackageVersionInfo()
                {
                    name = Plugin.PluginKey,
                    versionStr = release.tag_name,
                    classification = PackageVersionClass.Release,
                    description = release.body,
                    requiredVersionStr = "4.7.9",
                    sourceUrl = release.assets[0].browser_download_url,
                    checksum = checksum,
                    targetFilename = DLL_NAME,
                    infoUrl = null,
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
                        throw;
                    }
                }
                catch (HttpException exception2)
                {
                    _logger.ErrorException("Error downloading {0}", exception2, package.name);
                }
                catch (IOException exception3)
                {
                    _logger.ErrorException("Error updating {0}", exception3, package.name);
                }
                catch (Exception ex)
                {
                    _logger.ErrorException("Error updating {0}", ex, package.name);
                }
            }
        }

        private async Task<string> PrepareMd5Checksum(List<GitHubLatestReleaseAsset> assets, CancellationToken cancellationToken)
        {
            var downloadUrl = assets
                    .Where(a => DLL_NAME.EqualsIgnoreCase(a.name) && !string.IsNullOrWhiteSpace(a.browser_download_url))
                    .Select(a => a.browser_download_url)
                    .Single();
            var options = new HttpRequestOptions()
            {
                CancellationToken = cancellationToken,
                Url = downloadUrl,
                CacheLength = TimeSpan.FromHours(12),
                CacheMode = CacheMode.Unconditional,
                TimeoutMs = 180000,
                EnableDefaultUserAgent = true
            };
            options.Sanitation.SanitizeDefaultParams = false;
            var checksum = string.Empty;
            using (var reader = new StreamReader((await _httpClient.GetResponse(options)).Content))
            {
                using (var memstream = new MemoryStream())
                {
                    var bytes = default(byte[]);
                    reader.BaseStream.CopyTo(memstream);
                    bytes = memstream.ToArray();
                    checksum = CalculateMd5(bytes);
                    return checksum;
                }
            }
        }
        private async Task<GitHubLatestReleaseResponse> GetGitHubLatestRelease(CancellationToken cancellationToken)
        {
            var options = new HttpRequestOptions()
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
                return _jsonSerializer.DeserializeFromString<GitHubLatestReleaseResponse>(latestVersionJson);
            }
        }
        private static string CalculateMd5(byte[] bytes)
        {
#pragma warning disable CA5351
            using (var md5 = MD5.Create())
            {
                return BitConverter.ToString(md5.ComputeHash(bytes)).Replace("-", "");
            }
#pragma warning restore CA5351

        }
        private TaskTranslation GetTranslation()
        {
            return EmbyHelper.GetTaskTranslation(_translations, _serverConfigurationManager, _jsonSerializer, _availableTranslations);
        }
    }
}
