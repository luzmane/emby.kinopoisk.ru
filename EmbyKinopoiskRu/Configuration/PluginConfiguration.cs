using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using EmbyKinopoiskRu.Api;

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
        public bool CreateSeqCollections { get; set; }

        /// <summary>
        /// Available collections from Kinopoisk
        /// </summary>
        public string Collections
        {
            get => Plugin.Instance.JsonSerializer.SerializeToString(CollectionsList);
            set => SetCollection(value);
        }

        /// <summary>
        /// Base folder to download trailers
        /// </summary>
        public string IntrosPath { get; set; } = string.Empty;

        /// <summary>
        /// Preferable quality of downloaded trailers
        /// </summary>
        public int IntrosQuality { get; set; } = 480;

        /// <summary>
        /// API from https://apilayer.com/marketplace/user_agent-api
        /// </summary>
        public string UserAgentApiKey { get; set; } = string.Empty;

        /// <summary>
        /// Download only trailers in Russian
        /// </summary>
        public bool OnlyRussianTrailers { get; set; }

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

        internal void SetCollection(string value)
        {
            if (s_fetchingCollections)
            {
                return;
            }

            s_fetchingCollections = true;
            CollectionsList = Plugin.Instance.JsonSerializer.DeserializeFromString<List<CollectionItem>>(value)
                              ?? new List<CollectionItem>();

            Task<List<KpLists>> fetchTask = Plugin.Instance.GetKinopoiskService().GetKpCollectionsAsync(CancellationToken.None);
            if (fetchTask.Wait(TimeSpan.FromSeconds(10)))
            {
                CollectionsList = (from i in fetchTask.Result
                        from j in CollectionsList.Where(j => j.Id == i.Slug).DefaultIfEmpty()
                        select new CollectionItem
                        {
                            Id = i.Slug,
                            Category = i.Category,
                            Name = i.Name,
                            IsEnable = j?.IsEnable ?? false,
                            MovieCount = (j?.MovieCount ?? 0) == 0 ? i.MoviesCount : j.MovieCount
                        })
                    .ToList();
            }

            s_fetchingCollections = false;
        }
    }
}
