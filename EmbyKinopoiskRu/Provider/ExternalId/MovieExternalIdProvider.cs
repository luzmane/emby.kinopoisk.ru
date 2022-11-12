using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;

namespace EmbyKinopoiskRu.Provider.ExternalId
{
    /// <summary>
    /// Add link on kinopoisk page to metadate of the Movie
    /// </summary>
    public class MovieExternalIdProvider : IExternalId
    {
        public string Name => Plugin.PluginName;

        public string Key => Plugin.PluginName;

        /// <summary>
        /// Used on paget for link
        /// </summary>
        public string UrlFormatString => "https://www.kinopoisk.ru/film/{0}/";

        public bool Supports(IHasProviderIds item)
        {
            return item is Movie;
        }
    }
}
