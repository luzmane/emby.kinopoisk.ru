using System.Diagnostics.CodeAnalysis;

using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;

namespace EmbyKinopoiskRu.Provider.ExternalId
{
    /// <summary>
    /// Add link on kinopoisk page to metadata of the Movie
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class MovieExternalIdProvider : IExternalId, IHasWebsite, IHasSupportedExternalIdentifiers
    {
        /// <inheritdoc />
        public string Name => Plugin.PluginName;

        /// <inheritdoc />
        public string Key => Plugin.PluginKey;

        /// <inheritdoc />
        public string UrlFormatString => "https://www.kinopoisk.ru/film/{0}/";

        /// <inheritdoc />
        public bool Supports(IHasProviderIds item)
        {
            return item is Movie;
        }

        /// <inheritdoc />
        public string Website => "https://www.kinopoisk.ru";

        /// <inheritdoc />
        public string[] GetSupportedExternalIdentifiers()
        {
            return new[] { Plugin.PluginKey };
        }
    }
}
