using System;
using System.Collections.Generic;
using System.Linq;

using MediaBrowser.Model.Plugins;

namespace EmbyKinopoiskRu.Configuration
{
    /// <summary>
    /// Plugin configuration.
    /// </summary>
    public class PluginConfiguration : BasePluginConfiguration
    {
        private const string DefaultUnofficialToken = "0f162131-81c1-4979-b46c-3eea4263fb11";
        private const string DefaultDevToken = "8DA0EV2-KTP4A5Q-G67QP3K-S2VFBX7";
        internal const string KinopoiskDev = "kinopoisk.dev";
        internal const string KinopoiskApiUnofficialTech = "kinopoiskapiunofficial.tech";


        /// <summary>
        /// Gets or sets Token.
        /// </summary>
        public string Token { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets ApiType.
        /// </summary>
        public string ApiType { get; set; } = KinopoiskDev;

        /// <summary>
        /// Gets or sets a value indicating whether Token.
        /// </summary>
        public bool CreateSeqCollections { get; set; } = true;

        /// <summary>
        /// Available collections from Kinopoisk
        /// </summary>
        public string Collections
        {
            get
            {
                return Plugin.Instance.JsonSerializer.SerializeToString(CollectionsList);
            }
            set
            {
                SetCollection(value);
            }
        }

        internal List<CollectionItem> CollectionsList = new List<CollectionItem>();
        private static bool s_fetchingCollections;

        internal string GetCurrentToken()
        {
            return !string.IsNullOrWhiteSpace(Token)
                ? Token
                : KinopoiskApiUnofficialTech.Equals(ApiType, StringComparison.Ordinal)
                    ? DefaultUnofficialToken
                    : DefaultDevToken;
        }

        internal bool NeedToCreateSequenceCollection()
        {
            return KinopoiskDev.Equals(ApiType, StringComparison.Ordinal) && CreateSeqCollections;
        }

        private void SetCollection(string value)
        {
            if (s_fetchingCollections)
            {
                return;
            }

            s_fetchingCollections = true;
            CollectionsList = Plugin.Instance.JsonSerializer.DeserializeFromString<List<CollectionItem>>(value)
                              ?? new List<CollectionItem>();

            var fetchTask = Plugin.Instance.GetKinopoiskService().GetKpCollectionsAsync();
            if (fetchTask.Wait(TimeSpan.FromSeconds(10)))
            {
                var fetchedCollections = fetchTask.Result
                    .Select(x => new CollectionItem
                    {
                        Category = x.Category,
                        Id = x.Slug,
                        IsEnable = false,
                        Name = x.Name,
                        MovieCount = x.MoviesCount
                    })
                    .ToList();

                CollectionsList = CollectionsList
                    .Union(fetchedCollections)
                    .Where(x => fetchedCollections.Contains(x))
                    .ToList();
            }

            s_fetchingCollections = false;
        }
    }
}
