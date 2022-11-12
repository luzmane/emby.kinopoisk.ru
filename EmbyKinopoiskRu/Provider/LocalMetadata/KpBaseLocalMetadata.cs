using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;

namespace EmbyKinopoiskRu.Provider.LocalMetadata
{
    public abstract class KpBaseLocalMetadata<T> : ILocalMetadataProvider<T>
        where T : BaseItem, IHasProviderIds, new()
    {
        private static readonly Regex _kinopoiskIdRegex = new(@"kp-?(?<kinopoiskId>\d+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private readonly ILogger _log;

        public string Name => Plugin.PluginName;

        public KpBaseLocalMetadata(ILogManager logManager)
        {
            _log = logManager.GetLogger(GetType().FullName);
        }

        public Task<MetadataResult<T>> GetMetadata(ItemInfo info, LibraryOptions libraryOptions, IDirectoryService directoryService, CancellationToken cancellationToken)
        {
            _log.Info("GetMetadata");
            _log.Info($"info.Path - {info.Path}");
            MetadataResult<T> result = new();

            if (!string.IsNullOrEmpty(info.Path))
            {
                Match match = _kinopoiskIdRegex.Match(info.Path);
                if (match.Success && int.TryParse(match.Groups["kinopoiskId"].Value, out int kinopoiskId))
                {
                    _log.Info($"Detected kinopoisk id '{kinopoiskId}' for file '{info.Path}'");
                    T movie = new();
                    movie.SetProviderId(Plugin.PluginName, match.Groups["kinopoiskId"].Value);

                    result.Item = movie;
                    result.HasMetadata = true;
                }
            }

            return Task.FromResult(result);
        }
    }
}
