using System.Collections.Generic;

namespace EmbyKinopoiskRu.ScheduledTasks.Model
{
#pragma warning disable IDE1006
#pragma warning disable CA1707
    public class GitHubLatestReleaseResponse
    {
        public string tag_name { get; set; }
        public string body { get; set; }
        public string html_url { get; set; }
        public List<GitHubLatestReleaseAsset> assets { get; set; } = new List<GitHubLatestReleaseAsset>();

    }
#pragma warning restore IDE1006
#pragma warning restore CA1707
}
