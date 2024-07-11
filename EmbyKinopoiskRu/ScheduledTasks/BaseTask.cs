using System.Collections.Generic;

using EmbyKinopoiskRu.Helper;
using EmbyKinopoiskRu.ScheduledTasks.Model;

using MediaBrowser.Controller.Configuration;
using MediaBrowser.Model.Serialization;

namespace EmbyKinopoiskRu.ScheduledTasks
{
    /// <summary>
    /// Base class for the Kinopoisk plugin
    /// </summary>
    public abstract class BaseTask
    {
        private readonly Dictionary<string, TaskTranslation> _translations = new Dictionary<string, TaskTranslation>();
        private readonly Dictionary<string, string> _availableTranslations;

        private readonly IServerConfigurationManager _serverConfigurationManager;

        /// <summary>
        /// Json serializer
        /// </summary>
        protected readonly IJsonSerializer _jsonSerializer;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="taskKey"></param>
        /// <param name="jsonSerializer"></param>
        /// <param name="serverConfigurationManager"></param>
        protected BaseTask(string taskKey,
            IJsonSerializer jsonSerializer,
            IServerConfigurationManager serverConfigurationManager)
        {
            _jsonSerializer = jsonSerializer;
            _serverConfigurationManager = serverConfigurationManager;
            _availableTranslations = EmbyHelper.GetAvailableTranslations($"ScheduledTasks.{taskKey}");
        }

        /// <summary>
        /// Get task translation
        /// </summary>
        protected TaskTranslation GetTranslation()
        {
            return EmbyHelper.GetTaskTranslation(_translations, _serverConfigurationManager, _jsonSerializer, _availableTranslations);
        }
    }
}
