using System.Text.RegularExpressions;

namespace EmbyKinopoiskRu.TrailerDownloader.M3UParser.Model
{
    internal class XStreamInf : IHasUrl
    {
        private static readonly Regex ResolutionRegex = new Regex("(?<x>\\d+)x(?<y>\\d+)");

        public string Bandwidth { get; set; }
        public string AverageBandwidth { get; set; }
        public string Codecs { get; set; }
        public string Resolution { get; set; }
        public int ResolutionX { get; private set; }
        public int ResolutionY { get; private set; }
        public string Audio { get; set; }
        public string Url { get; set; }

        public XStreamInf(string line)
        {
            line = line.Substring(M3U8Parser.ExtXStreamInf.Length);
            var matchCollection = M3U8Parser.KeyValue.Matches(line);
            foreach (Match match in matchCollection)
            {
                var key = match.Groups["key"].Value;
                var value = M3U8Parser.Quotes.Replace(match.Groups["value"].Value, string.Empty);
                switch (key)
                {
                    case "BANDWIDTH":
                        Bandwidth = value;
                        break;
                    case "AVERAGE-BANDWIDTH":
                        AverageBandwidth = value;
                        break;
                    case "CODECS":
                        Codecs = value;
                        break;
                    case "RESOLUTION":
                        Resolution = value;
                        ParseResolution(this, Resolution);
                        break;
                    case "AUDIO":
                        Audio = value;
                        break;
                }
            }
        }

        private static void ParseResolution(XStreamInf obj, string resolution)
        {
            var match = ResolutionRegex.Match(resolution);
            if (!match.Success)
            {
                return;
            }

            if (int.TryParse(match.Groups["x"].Value, out int x))
            {
                obj.ResolutionX = x;
            }

            if (int.TryParse(match.Groups["y"].Value, out int y))
            {
                obj.ResolutionY = y;
            }
        }
    }
}
