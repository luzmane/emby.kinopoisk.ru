using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;

namespace EmbyKinopoiskRu.Provider
{
    /// <summary>
    /// Add link on kinopoisk page to metadate of the Person
    /// </summary>
    public class PersonExternalIdProvider : IExternalId
    {
        public string Name => Plugin.PluginName;

        public string Key => Plugin.PluginName;

        /// <summary>
        /// Used on paget for link
        /// </summary>
        public string UrlFormatString => "https://www.kinopoisk.ru/name/{0}/";

        public bool Supports(IHasProviderIds item)
        {
            return item is Person;
        }
    }
}
