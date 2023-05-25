using System;

using MediaBrowser.Model.Plugins;

namespace EmbyKinopoiskRu.Configuration
{
    public class PluginConfiguration : BasePluginConfiguration
    {
        private const string DefaultUnofficialToken = "0f162131-81c1-4979-b46c-3eea4263fb11";
        private const string DefaultDevToken = "8DA0EV2-KTP4A5Q-G67QP3K-S2VFBX7";
        public const string DefaultTop250MovieCollectionName = "Кинопоиск Топ 250";
        public const string DefaultTop250SeriesCollectionName = "Кинопоиск Топ 250 (Сериалы)";
        public const string KinopoiskDev = "kinopoisk.dev";
        public const string KinopoiskAPIUnofficialTech = "kinopoiskapiunofficial.tech";


        public string Token { get; set; } = string.Empty;
        public string ApiType { get; set; } = KinopoiskDev;
        public bool CreateCollections { get; set; } = true;
        public string Top250MovieCollectionName { get; set; } = DefaultTop250MovieCollectionName;
        public string Top250SeriesCollectionName { get; set; } = DefaultTop250SeriesCollectionName;


        public string GetCurrentToken()
        {
            return !string.IsNullOrWhiteSpace(Token)
                ? Token
                : KinopoiskAPIUnofficialTech.Equals(ApiType, StringComparison.Ordinal)
                    ? DefaultUnofficialToken
                    : DefaultDevToken;
        }
        public bool NeedToCreateCollection()
        {
            return KinopoiskDev.Equals(ApiType, StringComparison.Ordinal) && CreateCollections;
        }
        public string GetCurrentTop250MovieCollectionName()
        {
            return !string.IsNullOrWhiteSpace(Top250MovieCollectionName)
                ? Top250MovieCollectionName
                : DefaultTop250MovieCollectionName;
        }
        public string GetCurrentTop250SeriesCollectionName()
        {
            return !string.IsNullOrWhiteSpace(Top250SeriesCollectionName)
                ? Top250SeriesCollectionName
                : DefaultTop250SeriesCollectionName;
        }
    }
}
