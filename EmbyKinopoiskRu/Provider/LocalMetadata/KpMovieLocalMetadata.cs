using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using EmbyKinopoiskRu.Api;

using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;

namespace EmbyKinopoiskRu.Provider.LocalMetadata
{
    public class KpMovieLocalMetadata : KpBaseLocalMetadata<Movie>
    {
        private static readonly Regex AlphaNumeric = new(@"[^0-9A-ZА-Я-]", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex DubleWhitespace = new(@" {2,}", RegexOptions.Compiled);
        private static readonly Regex Year = new("(?<year>[0-9]{4})", RegexOptions.Compiled);

        private readonly ILogger _log;

        public KpMovieLocalMetadata(ILogManager logManager) : base(logManager)
        {
            _log = logManager.GetLogger(GetType().FullName);
        }

        public override async Task<MetadataResult<Movie>> GetMetadata(ItemInfo info, LibraryOptions libraryOptions, IDirectoryService directoryService, CancellationToken cancellationToken)
        {
            _log.Info("GetMetadata");
            MetadataResult<Movie> result = await base.GetMetadata(info, libraryOptions, directoryService, cancellationToken);
            if (result.HasMetadata)
            {
                _log.Info($"Movie has kp id: {result.Item.ProviderIds[Plugin.PluginName]}");
                return result;
            }

            var movieName = info.Name;
            _log.Info($"info.Name - {movieName}");
            movieName = DubleWhitespace.Replace(AlphaNumeric.Replace(movieName, " "), " ");

            var yearSt = DetectYearFromMoviePath(info.Path, info.Name);
            _ = int.TryParse(yearSt, out var year);
            _log.Info($"Searching movie by name - {movieName} and year - {year}");
            List<Movie> movies = await KinopoiskRuServiceFactory.GetService().GetMoviesByOriginalNameAndYear(movieName, year, cancellationToken);

            if (movies.Count == 0)
            {
                _log.Info($"Nothing found for movie name '{movieName}");
            }
            else if (movies.Count == 1)
            {
                _log.Info($"For movie name '{movieName}' found movie with KP id = '{movies[0].GetProviderId(Plugin.PluginName)}'");
                result.Item = movies[0];
                result.HasMetadata = true;
            }
            else
            {
                _log.Info($"Found {movies.Count} movies. Taking the first one. For movie name '{movieName}' found movie with KP id = '{movies[0].GetProviderId(Plugin.PluginName)}'");
                result.Item = movies[0];
                result.HasMetadata = true;
            }
            return result;
        }
        private static string DetectYearFromMoviePath(string filePath, string movieName)
        {
            var fileName = Path.GetFileNameWithoutExtension(filePath);
            fileName = fileName.Replace(movieName, " ");
            if (!string.IsNullOrWhiteSpace(fileName))
            {
                Match match = Year.Match(fileName);
                return match.Success ? match.Groups["year"].Value : string.Empty;
            }
            return string.Empty;
        }
    }
}
