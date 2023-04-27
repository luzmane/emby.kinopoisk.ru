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
        private static readonly Regex KinopoiskIdRegex = new Regex(@"kp-?(?<kinopoiskId>\d+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private readonly ILogger _log;

        public string Name => Plugin.PluginName;

        protected KpBaseLocalMetadata(ILogManager logManager)
        {
            _log = logManager.GetLogger(GetType().Name);
        }

        public virtual Task<MetadataResult<T>> GetMetadata(ItemInfo info, LibraryOptions libraryOptions, IDirectoryService directoryService, CancellationToken cancellationToken)
        {
            _log.Info("GetMetadata");
            _log.Info($"info.Path - {info.Path}");
            var result = new MetadataResult<T>();

            if (!string.IsNullOrEmpty(info.Path))
            {
                Match match = KinopoiskIdRegex.Match(info.Path);
                if (match.Success && int.TryParse(match.Groups["kinopoiskId"].Value, out var kinopoiskId))
                {
                    _log.Info($"Detected kinopoisk id '{kinopoiskId}' for file '{info.Path}'");
                    var item = new T();
                    item.SetProviderId(Plugin.PluginKey, match.Groups["kinopoiskId"].Value);

                    result.Item = item;
                    result.HasMetadata = true;
                }
            }

            return Task.FromResult(result);
        }
    }
}
