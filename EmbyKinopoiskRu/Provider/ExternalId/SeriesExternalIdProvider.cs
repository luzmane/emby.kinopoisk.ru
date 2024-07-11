using System.Diagnostics.CodeAnalysis;

using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;

namespace EmbyKinopoiskRu.Provider.ExternalId
{
    /// <summary>
    /// Add link on kinopoisk page to metadata of the Series
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class SeriesExternalIdProvider : IExternalId
    {
        /// <inheritdoc />
        public string Name => Plugin.PluginName;

        /// <inheritdoc />
        public string Key => Plugin.PluginKey;

        /// <inheritdoc />
        public string UrlFormatString => "https://www.kinopoisk.ru/series/{0}/";

        /// <inheritdoc />
        public bool Supports(IHasProviderIds item)
        {
            return item is Series;
        }
    }
}
