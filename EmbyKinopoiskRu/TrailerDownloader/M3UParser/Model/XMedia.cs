using System.Text.RegularExpressions;

namespace EmbyKinopoiskRu.TrailerDownloader.M3UParser.Model
{
    internal class XMedia : IHasUrl
    {
        public string Type { get; set; }
        public string GroupId { get; set; }
        public string Name { get; set; }
        public string Default { get; set; }
        public string AutoSelect { get; set; }
        public string Language { get; set; }
        public string Url { get; set; }

        public XMedia(string line, string host)
        {
            line = line.Substring(M3U8Parser.ExtXMedia.Length);
            var matchCollection = M3U8Parser.KeyValue.Matches(line);
            foreach (Match match in matchCollection)
            {
                var key = match.Groups["key"].Value;
                var value = M3U8Parser.Quotes.Replace(match.Groups["value"].Value, string.Empty);
                switch (key)
                {
                    case "TYPE":
                        Type = value;
                        break;
                    case "GROUP-ID":
                        GroupId = value;
                        break;
                    case "NAME":
                        Name = value;
                        break;
                    case "DEFAULT":
                        Default = value;
                        break;
                    case "AUTOSELECT":
                        AutoSelect = value;
                        break;
                    case "LANGUAGE":
                        Language = value;
                        break;
                    case "URI":
                        Url = value.StartsWith("/") ? host + value : value;
                        break;
                }
            }
        }
    }
}
