using System;

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
        internal const string DefaultTop250MovieCollectionName = "Кинопоиск Топ 250";
        internal const string DefaultTop250SeriesCollectionName = "Кинопоиск Топ 250 (Сериалы)";
        internal const string KinopoiskDev = "kinopoisk.dev";
        internal const string KinopoiskAPIUnofficialTech = "kinopoiskapiunofficial.tech";


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
        /// Gets or sets a value indicating whether Top250InOneLib.
        /// </summary>
        public bool Top250InOneLib { get; set; }

        /// <summary>
        /// Gets or sets Top250MovieCollectionName.
        /// </summary>
        public string Top250MovieCollectionName { get; set; } = DefaultTop250MovieCollectionName;

        /// <summary>
        /// Gets or sets Token.
        /// </summary>
        public string Top250SeriesCollectionName { get; set; } = DefaultTop250SeriesCollectionName;


        internal string GetCurrentToken()
        {
            return !string.IsNullOrWhiteSpace(Token)
                ? Token
                : KinopoiskAPIUnofficialTech.Equals(ApiType, StringComparison.Ordinal)
                    ? DefaultUnofficialToken
                    : DefaultDevToken;
        }
        internal bool NeedToCreateSequenceCollection()
        {
            return KinopoiskDev.Equals(ApiType, StringComparison.Ordinal) && CreateSeqCollections;
        }
        internal bool NeedToCreateTop250InOneLib()
        {
            return KinopoiskDev.Equals(ApiType, StringComparison.Ordinal) && Top250InOneLib;
        }
        internal string GetCurrentTop250MovieCollectionName()
        {
            return !string.IsNullOrWhiteSpace(Top250MovieCollectionName)
                ? Top250MovieCollectionName
                : DefaultTop250MovieCollectionName;
        }
        internal string GetCurrentTop250SeriesCollectionName()
        {
            return !string.IsNullOrWhiteSpace(Top250SeriesCollectionName)
                ? Top250SeriesCollectionName
                : DefaultTop250SeriesCollectionName;
        }
    }
}
