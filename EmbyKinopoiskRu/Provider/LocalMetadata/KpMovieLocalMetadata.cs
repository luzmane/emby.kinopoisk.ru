using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Model.Logging;

namespace EmbyKinopoiskRu.Provider.LocalMetadata
{
    public class KpMovieLocalMetadata : KpBaseLocalMetadata<Movie>
    {
        public KpMovieLocalMetadata(ILogManager logManager) : base(logManager)
        {
        }
    }
}
