using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Model.Logging;

namespace EmbyKinopoiskRu.Provider.LocalMetadata
{
    public class KpSeriesLocalMetadata : KpBaseLocalMetadata<Series>
    {
        public KpSeriesLocalMetadata(ILogManager logManager) : base(logManager)
        {
        }
    }
}
