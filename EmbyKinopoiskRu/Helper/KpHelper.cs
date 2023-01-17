using System;
using System.Globalization;
using EmbyKinopoiskRu.Api.KinopoiskDev.Model.Movie;
using MediaBrowser.Model.Activity;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;

namespace EmbyKinopoiskRu.Helper
{
    internal class KpHelper
    {
        internal static DateTimeOffset? GetPremierDate(KpPremiere? premiere)
        {
            if (premiere == null)
            {
                return null;
            }
            if (DateTimeOffset.TryParseExact(
                premiere.World,
                "yyyy-MM-dd'T'HH:mm:ss.fffZ",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateTimeOffset world))
            {
                return world;
            }
            if (DateTimeOffset.TryParseExact(
                premiere.Russia,
                "yyyy-MM-dd'T'HH:mm:ss.fffZ",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateTimeOffset russia))
            {
                return russia;
            }
            if (DateTimeOffset.TryParseExact(
                premiere.Cinema,
                "yyyy-MM-dd'T'HH:mm:ss.fffZ",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateTimeOffset cinema))
            {
                return cinema;
            }
            if (DateTimeOffset.TryParseExact(
                premiere.Digital,
                "yyyy-MM-dd'T'HH:mm:ss.fffZ",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateTimeOffset digital))
            {
                return digital;
            }
            if (DateTimeOffset.TryParseExact(
                premiere.Bluray,
                "yyyy-MM-dd'T'HH:mm:ss.fffZ",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateTimeOffset bluray))
            {
                return bluray;
            }
            if (DateTimeOffset.TryParseExact(
                premiere.Dvd,
                "yyyy-MM-dd'T'HH:mm:ss.fffZ",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateTimeOffset dvd))
            {
                return dvd;
            }
            return null;
        }
        internal static PersonType? GetPersonType(string? enProfesson)
        {
            return Enum.TryParse(enProfesson, true, out PersonType result) ? result : null;
        }
        internal static void AddToActivityLog(IActivityManager activityManager, string overview, string shortOverview)
        {
            activityManager.Create(new()
            {
                Name = Plugin.PluginName,
                Type = "PluginError",
                Overview = overview,
                ShortOverview = shortOverview,
                Severity = LogSeverity.Error
            });
        }

    }
}
