using EmbyKinopoiskRu.Api;
using EmbyKinopoiskRu.Helper;

using FluentAssertions;

using MediaBrowser.Model.Logging;

namespace EmbyKinopoiskRu.Tests.Common;

public class TrailerDlHelperTest : BaseTest
{
    private static readonly NLog.ILogger Logger = NLog.LogManager.GetLogger(nameof(TrailerDlHelperTest));
    private const string UserAgentApiKey = "wd0fL7jxJVrF6K0g2Zb3y1tN3lLHkRwz";

    public TrailerDlHelperTest() : base(Logger)
    {
        ConfigXmlSerializer();
    }

    [Theory]
    [MemberData(nameof(GetYoutubeIdData))]
    public void YtHelper_GetYoutubeId(string url, string youtubeId)
    {
        TrailerDlHelper.GetYoutubeId(url).Should().Be(youtubeId);
    }

    [Fact]
    public void YtHelper_GetIntroName_WithInvalidChars()
    {
        var invalid = Path.GetInvalidFileNameChars();
        if (invalid.Length > 0)
        {
            TrailerDlHelper.GetIntroName($"videoName{invalid[0]}", "youtubeId", "mp4").Should().Be("videoName [youtubeId].mp4");
        }
    }

    [Fact]
    public void YtHelper_GetIntroName_WithoutInvalidChars()
    {
        TrailerDlHelper.GetIntroName("videoName", "youtubeId", "mp4").Should().Be("videoName [youtubeId].mp4");
    }

    [Theory]
    [MemberData(nameof(KpTrailerData))]
    public void YtHelper_GetPartialIntroName(KpTrailer trailer, string expectedResult)
    {
        var introName = TrailerDlHelper.GetPartialTrailerName(trailer);
        introName.Should().Be(expectedResult);
    }

    [Fact]
    public async Task YtHelper_GetUserAgent_Local()
    {
        _ = _applicationPaths
            .SetupGet(m => m.PluginConfigurationsPath)
            .Returns(nameof(YtHelper_GetUserAgent_Local));

        (await TrailerDlHelper.GetUserAgent(_httpClient, new Mock<ILogger>().Object, CancellationToken.None)).Should().BeOneOf(TrailerDlHelper.UserAgents);
    }

    [Fact]
    public async Task YtHelper_GetUserAgent_API()
    {
        _ = _applicationPaths
            .SetupGet(m => m.PluginConfigurationsPath)
            .Returns(nameof(YtHelper_GetUserAgent_Local));

        _pluginConfiguration.UserAgentApiKey = GetUserAgentApiKey();

        Mock<ILogger> logger = new();
        (await TrailerDlHelper.GetUserAgent(_httpClient, logger.Object, CancellationToken.None)).Should().NotBeNullOrWhiteSpace();
        logger.Verify(l => l.Warn(It.Is<string>(m =>
            m.StartsWith("Unable to fetch dynamic")
            || m.StartsWith("The key for UserAgent API is empty")
            || m.StartsWith("Status code received from the API"))), Times.Never());
        logger.Verify(l => l.Info("Successfully fetched User Agent"), Times.Once());
    }

    private static string GetUserAgentApiKey()
    {
        var apiKey = Environment.GetEnvironmentVariable("USER_AGENT_API_KEY");
        Logger.Info($"UserAGent API key length is: {apiKey?.Length ?? 0}");
        return string.IsNullOrWhiteSpace(apiKey) ? UserAgentApiKey : apiKey;
    }

    #region MemberData

    public static TheoryData<KpTrailer, string> KpTrailerData => new()
    {
        {
            new KpTrailer()
            {
                VideoName = "videoName",
                TrailerName = "trailerName",
                PremierDate = new DateTimeOffset(2024, 1, 1, 1, 1, 1, TimeSpan.Zero)
            },
            "videoName (2024) (trailerName)"
        },
        {
            new KpTrailer()
            {
                VideoName = "videoName",
                TrailerName = "trailerName",
            },
            "videoName (trailerName)"
        },
        {
            new KpTrailer()
            {
                VideoName = "videoName",
                TrailerName = "videoName",
                PremierDate = new DateTimeOffset(2024, 1, 1, 1, 1, 1, TimeSpan.Zero)
            },
            "videoName (2024)"
        },
        {
            new KpTrailer()
            {
                VideoName = "videoName",
                TrailerName = "videoName",
            },
            "videoName"
        },
    };

    public static TheoryData<string, string> GetYoutubeIdData => new()
    {
        { "http://www.youtube.com/watch?v=_-m3YhxJ8U0", "_-m3YhxJ8U0" },
        { "http://www.youtube.com/embed/_-m3YhxJ8U0", "_-m3YhxJ8U0" },
        { "http://www.youtube.com/v/_-m3YhxJ8U0", "_-m3YhxJ8U0" },
        { "http://www.youtube.com/watch?v=_-m3YhxJ8U0", "_-m3YhxJ8U0" },
        { "https://www.youtube.com/watch?v=_-m3YhxJ8U0", "_-m3YhxJ8U0" },
        { "https://www.youtube.com/embed/_-m3YhxJ8U0", "_-m3YhxJ8U0" },
        { "https://www.youtube.com/v/_-m3YhxJ8U0", "_-m3YhxJ8U0" },
        { "https://www.youtube.com/watch?v=_-m3YhxJ8U0", "_-m3YhxJ8U0" },
    };

    #endregion
}
