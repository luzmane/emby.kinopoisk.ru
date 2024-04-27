using System;
using System.Diagnostics;
using System.Linq;

using MediaBrowser.Model.Entities;

namespace EmbyKinopoiskRu.Api
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    internal sealed class KpTrailer
    {
        public string ImageUrl { get; set; }
        public string VideoName { get; set; }
        public string Overview { get; set; }
        public DateTimeOffset? PremierDate { get; set; }
        public ProviderIdDictionary ProviderIds = new ProviderIdDictionary();
        public string TrailerName { get; set; }
        public string Url { get; set; }

        private string DebuggerDisplay => $"VideoName '{VideoName}', TrailerName: '{TrailerName}', Url '{Url}', PremierDate '{PremierDate}'" +
                                          $"ImageUrl '{string.IsNullOrWhiteSpace(ImageUrl)}', Overview '{string.IsNullOrWhiteSpace(Overview)}', " +
                                          $"ProviderIds '{string.Join(",", ProviderIds.Select(x => x.Key))}'";

        public override string ToString() => DebuggerDisplay;
    }
}
