using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using EmbyKinopoiskRu.Helper;

using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;

namespace EmbyKinopoiskRu.Provider.LocalMetadata
{
    public class KpMovieLocalMetadata : KpBaseLocalMetadata<Movie>
    {
        private static readonly Regex NotAlphaNumeric = new(@"[^0-9ЁA-ZА-Я-]", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex MultiSpaces = new(@" {2,}", RegexOptions.Compiled);

        private readonly ILogger _log;

        public KpMovieLocalMetadata(ILogManager logManager) : base(logManager)
        {
            _log = logManager.GetLogger(GetType().FullName);
        }

        public override async Task<MetadataResult<Movie>> GetMetadata(ItemInfo info, LibraryOptions libraryOptions, IDirectoryService directoryService, CancellationToken cancellationToken)
        {
            MetadataResult<Movie> result = await base.GetMetadata(info, libraryOptions, directoryService, cancellationToken);
            if (result.HasMetadata)
            {
                _log.Info($"Movie has kp id: {result.Item.ProviderIds[Plugin.PluginName]}");
                return result;
            }

            var movieName = info.Name;
            _log.Info($"info.Name - {movieName}");
            movieName = MultiSpaces.Replace(NotAlphaNumeric.Replace(movieName, " "), " ");
            var year = KpHelper.DetectYearFromMoviePath(info.Path, info.Name);
            _log.Info($"Searching movie by name - '{movieName}' and year - {year}");
            List<Movie> movies = await Plugin.Instance!.GetKinopoiskService().GetMoviesByOriginalNameAndYear(movieName, year, cancellationToken);
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
                Movie? movieWithHighestRating = movies
                    .Where(m => m.CommunityRating != null)
                    .OrderByDescending(m => m.CommunityRating)
                    .FirstOrDefault();
                if (movieWithHighestRating != null)
                {
                    result.Item = movieWithHighestRating;
                    _log.Info($"Found {movies.Count} movies. Taking the first one with highest rating in KP. Choose movie with KP id = '{result.Item.GetProviderId(Plugin.PluginName)}' for '{movieName}'");
                }
                else
                { // all films without KP rating
                    result.Item = movies[0];
                    _log.Info($"Found {movies.Count} movies. Taking the first one. Choose movie with KP id = '{result.Item.GetProviderId(Plugin.PluginName)}' for '{movieName}'");
                }
                result.HasMetadata = true;
            }
            return result;
        }

    }
}
