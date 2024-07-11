using System;
using System.Diagnostics;
using System.Linq;

using MediaBrowser.Model.Entities;

namespace EmbyKinopoiskRu.Api
{
    /// <summary>
    /// Video trailer
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public sealed class KpTrailer
    {
        /// <summary>
        /// URL of a poster of the trailer video
        /// </summary>
        public string ImageUrl { get; set; }

        /// <summary>
        /// Name of the trailer video
        /// </summary>
        public string VideoName { get; set; }

        /// <summary>
        /// Trailer overview
        /// </summary>
        public string Overview { get; set; }

        /// <summary>
        /// Video premier date
        /// </summary>
        public DateTimeOffset? PremierDate { get; set; }

        /// <summary>
        /// Provider IDs of the trailer video
        /// </summary>
        public ProviderIdDictionary ProviderIds = new ProviderIdDictionary();

        /// <summary>
        /// Name of the trailer
        /// </summary>
        public string TrailerName { get; set; }

        /// <summary>
        /// URL of the trailer
        /// </summary>
        public string Url { get; set; }

        private string DebuggerDisplay => $"VideoName '{VideoName}', TrailerName: '{TrailerName}', Url '{Url}', PremierDate '{PremierDate}'" +
                                          $"ImageUrl '{string.IsNullOrWhiteSpace(ImageUrl)}', Overview '{string.IsNullOrWhiteSpace(Overview)}', " +
                                          $"ProviderIds '{string.Join(",", ProviderIds.Select(x => x.Key))}'";

        /// <inheritdoc />
        public override string ToString() => DebuggerDisplay;
    }
}
