using System;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

using EmbyKinopoiskRu.Api.KinopoiskDev.Model.Movie;

using MediaBrowser.Model.Activity;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;

namespace EmbyKinopoiskRu.Helper
{
    internal static class KpHelper
    {
        private static readonly Regex Year = new Regex("(?<year>[0-9]{4})", RegexOptions.Compiled);
        private static readonly Regex NonAlphaNumeric = new Regex("[^а-яА-Яa-zA-Z0-9\\s]", RegexOptions.Compiled);
        private static readonly Regex MultiWhitespace = new Regex("\\s\\s+", RegexOptions.Compiled);
        private const string PremierDateFormat = "yyyy-MM-dd'T'HH:mm:ss.fffZ";
        private const string DateTimeYearFormat = "yyyy";

        internal static DateTimeOffset? GetPremierDate(KpPremiere premiere)
        {
            if (premiere == null)
            {
                return null;
            }
            if (DateTimeOffset.TryParseExact(
                premiere.World,
                PremierDateFormat,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateTimeOffset world))
            {
                return world;
            }
            if (DateTimeOffset.TryParseExact(
                premiere.Russia,
                PremierDateFormat,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateTimeOffset russia))
            {
                return russia;
            }
            if (DateTimeOffset.TryParseExact(
                premiere.Cinema,
                PremierDateFormat,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateTimeOffset cinema))
            {
                return cinema;
            }
            if (DateTimeOffset.TryParseExact(
                premiere.Digital,
                PremierDateFormat,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateTimeOffset digital))
            {
                return digital;
            }
            if (DateTimeOffset.TryParseExact(
                premiere.Bluray,
                PremierDateFormat,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateTimeOffset bluray))
            {
                return bluray;
            }
            if (DateTimeOffset.TryParseExact(
                premiere.Dvd,
                PremierDateFormat,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateTimeOffset dvd))
            {
                return dvd;
            }
            return null;
        }
        internal static PersonType? GetPersonType(string enProfesson)
        {
            return Enum.TryParse(enProfesson, true, out PersonType result) ? result : (PersonType?)null;
        }
        internal static void AddToActivityLog(IActivityManager activityManager, string overview, string shortOverview)
        {
            activityManager.Create(new ActivityLogEntry
            {
                Name = Plugin.PluginKey,
                Type = "PluginError",
                Overview = overview,
                ShortOverview = shortOverview,
                Severity = LogSeverity.Error
            });
        }
        internal static int? DetectYearFromMoviePath(string filePath, string movieName)
        {
            var fileName = Path.GetFileNameWithoutExtension(filePath);
            fileName = fileName.Replace(movieName, " ");
            var yearSt = string.Empty;
            if (!string.IsNullOrWhiteSpace(fileName))
            {
                Match match = Year.Match(fileName);
                yearSt = match.Success ? match.Groups["year"].Value : string.Empty;
            }
            _ = int.TryParse(yearSt, out var year);
            _ = int.TryParse(DateTimeOffset.Now.ToString(DateTimeYearFormat, CultureInfo.InvariantCulture), out var currentYear);
            return (year > 1800 && year <= currentYear + 1) ? year : (int?)null;
        }
        internal static string CleanName(string name)
        {
            return string.IsNullOrEmpty(name)
            ? name
            : MultiWhitespace.Replace(NonAlphaNumeric.Replace(name, " ").Trim(), " ").ToLowerInvariant();
        }

    }
}
