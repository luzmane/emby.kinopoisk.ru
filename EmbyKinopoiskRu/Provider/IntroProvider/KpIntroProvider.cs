using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Logging;

namespace EmbyKinopoiskRu.Provider.IntroProvider
{
    /// <summary>
    /// Kinopoisk intro provider
    /// </summary>
    public class KpIntroProvider : IIntroProvider
    {
        private readonly ILogger _logger;

        /// <inheritdoc />
        public string Name => Plugin.PluginName;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logManager"></param>
        public KpIntroProvider(ILogManager logManager)
        {
            _logger = logManager.GetLogger(GetType().Name);
        }

        /// <inheritdoc />
        public Task<IEnumerable<IntroInfo>> GetIntros(BaseItem item, User user)
        {
            _logger.Info($"GetIntros. Not supporting get intros based on item and user");
            return Task.FromResult(new List<IntroInfo>().AsEnumerable());
        }

        /// <inheritdoc />
        public IEnumerable<string> GetAllIntroFiles()
        {
            var basePath = Plugin.Instance.Configuration.IntrosPath;
            if (string.IsNullOrWhiteSpace(basePath))
            {
                _logger.Info("IntrosPath configuration is empty");
                return new List<string>();
            }

            if (!Directory.Exists(basePath))
            {
                _logger.Warn($"Folder '{basePath}' doesn't exist");
                return new List<string>();
            }

            var toReturn = ListAllFiles(basePath, new List<string>());
            _logger.Info($"Found {toReturn.Count()} files");

            return toReturn;
        }

        private static List<string> ListAllFiles(string baseFolder, List<string> toReturn, int deep = 3)
        {
            if (string.IsNullOrWhiteSpace(baseFolder)
                || !Directory.Exists(baseFolder)
                || deep < 0)
            {
                return toReturn;
            }

            toReturn.AddRange(Directory.GetFiles(baseFolder));

            foreach (var directory in Directory.GetDirectories(baseFolder))
            {
                ListAllFiles(directory, toReturn, --deep);
            }

            return toReturn;
        }
    }
}
