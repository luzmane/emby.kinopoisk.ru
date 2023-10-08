using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Model.Logging;

namespace EmbyKinopoiskRu.Provider.LocalMetadata
{
    /// <inheritdoc />
    public class KpSeriesLocalMetadata : KpBaseLocalMetadata<Series>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="KpSeriesLocalMetadata"/> class.
        /// </summary>
        /// <param name="logManager">Instance of the <see cref="ILogManager"/> interface.</param>
        public KpSeriesLocalMetadata(ILogManager logManager) : base(logManager)
        {
        }
    }
}
