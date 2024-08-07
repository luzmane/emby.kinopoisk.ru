using System.Diagnostics.CodeAnalysis;

using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;

namespace EmbyKinopoiskRu.Provider.ExternalId
{
    /// <summary>
    /// Add link on kinopoisk page to metadata of the Person
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class PersonExternalIdProvider : IExternalId
    {
        /// <inheritdoc />
        public string Name => Plugin.PluginName;

        /// <inheritdoc />
        public string Key => Plugin.PluginKey;

        /// <inheritdoc />
        public string UrlFormatString => "https://www.kinopoisk.ru/name/{0}/";

        /// <inheritdoc />
        public bool Supports(IHasProviderIds item)
        {
            return item is Person;
        }
    }
}
