using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Model.Logging;

namespace EmbyKinopoiskRu.Provider.LocalMetadata
{
    /// <inheritdoc />
    public class KpMovieLocalMetadata : KpBaseLocalMetadata<Movie>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="KpMovieLocalMetadata"/> class.
        /// </summary>
        /// <param name="logManager">Instance of the <see cref="ILogManager"/> interface.</param>
        public KpMovieLocalMetadata(ILogManager logManager) : base(logManager)
        {
        }

    }
}
