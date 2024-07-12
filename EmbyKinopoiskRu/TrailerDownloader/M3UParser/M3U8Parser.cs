using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using EmbyKinopoiskRu.Helper;
using EmbyKinopoiskRu.TrailerDownloader.M3UParser.Model;

using MediaBrowser.Model.Logging;

namespace EmbyKinopoiskRu.TrailerDownloader.M3UParser
{
    internal class M3U8Parser
    {
        private static readonly char[] NewLineSplitter = { '\n', '\r' };
        internal static readonly Regex KeyValue = new Regex("(?<key>[^,=]+)=(?<value>\"[^\"]*\"|[^,]*)");
        internal static readonly Regex Quotes = new Regex("\"");
        private static readonly Regex ExtXTargetDurationRegex = new Regex($"{ExtXTargetDuration}(?<l>\\d+).*");

        private const string ExtXTargetDuration = "#EXT-X-TARGETDURATION:";
        internal const string ExtXMedia = "#EXT-X-MEDIA:";
        internal const string ExtXStreamInf = "#EXT-X-STREAM-INF:";

        private readonly ILogger _logger;

        public M3U8Parser(ILogManager logManager)
        {
            _logger = logManager.GetLogger(nameof(M3U8Parser));
        }

        internal (List<XStreamInf> xStreamInfs, List<XMedia> xMedia) ParseMasterM3U8(string url, string fileContent, int preferableQuality)
        {
            var host = "https://" + new Uri(url).Host;
            var lines = fileContent.Split(NewLineSplitter, StringSplitOptions.RemoveEmptyEntries);
            var video = new List<XStreamInf>();
            var audio = new List<XMedia>();
            XStreamInf streamInf = null;
            List<int> resolutions = new List<int>();
            foreach (var line in lines)
            {
                if (line.StartsWith(ExtXMedia))
                {
                    audio.Add(new XMedia(line, host));
                }
                else if (line.StartsWith(ExtXStreamInf))
                {
                    streamInf = new XStreamInf(line);
                    resolutions.Add(streamInf.ResolutionY);
                }
                else if (!line.StartsWith("#") && streamInf != null && !string.IsNullOrWhiteSpace(line))
                {
                    streamInf.Url = host + line;
                    video.Add(streamInf);
                    streamInf = null;
                }
            }

            resolutions.RemoveAll(r => r == 0);
            if (resolutions.Count == 0)
            {
                _logger.Info("No resolution info was found during the master m3u8 file parsing. Return all found xStream and xMedia lists");
                return (video, audio);
            }

            var resolution = resolutions
                .Where(r => r >= preferableQuality)
                .Distinct()
                .OrderBy(r => r)
                .FirstOrDefault();
            if (resolution == 0)
            {
                _logger.Debug("No stream was found with at least preferable quality. Search for the nearest resolution from the bottom");
                resolution = resolutions
                    .Where(r => r < preferableQuality)
                    .Distinct()
                    .OrderByDescending(r => r)
                    .FirstOrDefault();
            }

            _logger.Info($"Used resolution: '{resolution}'");

            var xStreamInfs = video
                .GroupBy(x => x.ResolutionY)
                .First(x => resolution == x.Key)
                .ToList();
            _logger.Debug($"Found '{xStreamInfs.Count}' video links and '{audio.Count}' audio links");

            return (xStreamInfs, audio);
        }

        internal List<string> ParseM3U8(string m3U8Content)
        {
            var maxDurationSec = Plugin.Instance.Configuration.TrailerMaxDuration * Constants.OneMinuteInSec;
            if (maxDurationSec > 0)
            {
                var list = m3U8Content.Split(NewLineSplitter, StringSplitOptions.RemoveEmptyEntries);
                var toReturn = new List<string>();
                var targetDurationLength = 0;
                foreach (var line in list)
                {
                    if (line.StartsWith(ExtXTargetDuration))
                    {
                        var match = ExtXTargetDurationRegex.Match(line);
                        if (match.Success && int.TryParse(match.Groups["l"].Value, out var itemDuration))
                        {
                            targetDurationLength = itemDuration;
                            _logger.Debug($"TargetDuration is '{targetDurationLength}'");
                        }
                    }
                    else if (!line.StartsWith("#") && !string.IsNullOrWhiteSpace(line))
                    {
                        toReturn.Add(line);
                    }
                }

                if (targetDurationLength > 0)
                {
                    var durationSec = targetDurationLength * toReturn.Count;
                    _logger.Debug($"Max trailer duration '{maxDurationSec}', media duration '{durationSec}' sec");
                    return TrailerDlHelper.CheckTrailerDuration(durationSec, maxDurationSec) ? toReturn : new List<string>();
                }

                return toReturn;
            }
            else
            {
                return m3U8Content
                    .Split(NewLineSplitter, StringSplitOptions.RemoveEmptyEntries)
                    .Where(l => !l.StartsWith("#"))
                    .ToList();
            }
        }
    }
}
