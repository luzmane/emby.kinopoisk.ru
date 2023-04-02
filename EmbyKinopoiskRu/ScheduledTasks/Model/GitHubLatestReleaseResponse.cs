using System.Collections.Generic;

namespace EmbyKinopoiskRu.ScheduledTasks.Model
{
#pragma warning disable IDE1006
#pragma warning disable CA1707
    public class GitHubLatestReleaseResponse
    {
        public string? tag_name { get; set; }
        public string? body { get; set; }
        public List<GitHubLatestReleaseAsset> assets { get; init; } = new();

    }
#pragma warning restore IDE1006
#pragma warning restore CA1707
}
