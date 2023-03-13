using System;

using MediaBrowser.Model.Plugins;

namespace EmbyKinopoiskRu.Configuration
{
    public class PluginConfiguration : BasePluginConfiguration
    {
        private const string DefaultUnofficialToken = "0f162131-81c1-4979-b46c-3eea4263fb11";
        private const string DefaultDevToken = "8DA0EV2-KTP4A5Q-G67QP3K-S2VFBX7";
        public const string KinopoiskDev = "kinopoisk.dev";
        public const string KinopoiskAPIUnofficialTech = "kinopoiskapiunofficial.tech";

        public string Token { get; set; } = string.Empty;
        public string ApiType { get; set; } = KinopoiskAPIUnofficialTech;

        public string GetCurrentToken()
        {
            return !string.IsNullOrWhiteSpace(Token)
                ? Token
                : KinopoiskAPIUnofficialTech.Equals(ApiType, StringComparison.Ordinal)
                    ? DefaultUnofficialToken
                    : DefaultDevToken;
        }

    }
}
