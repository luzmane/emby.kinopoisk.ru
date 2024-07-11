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

    [Fact]
    public void YtHelper_GetYoutubeId()
    {
        const string youtubeId = "_-m3YhxJ8U0";
        TrailerDlHelper.GetYoutubeId($"https://www.youtube.com/watch?v={youtubeId}").Should().Be(youtubeId);
        TrailerDlHelper.GetYoutubeId($"https://www.youtube.com/embed/{youtubeId}").Should().Be(youtubeId);
        TrailerDlHelper.GetYoutubeId($"https://www.youtube.com/v/{youtubeId}").Should().Be(youtubeId);
        TrailerDlHelper.GetYoutubeId($"https://www.youtu.be/{youtubeId}").Should().Be(youtubeId);
        TrailerDlHelper.GetYoutubeId($"http://www.youtube.com/watch?v={youtubeId}").Should().Be(youtubeId);
        TrailerDlHelper.GetYoutubeId($"http://www.youtube.com/embed/{youtubeId}").Should().Be(youtubeId);
        TrailerDlHelper.GetYoutubeId($"http://www.youtube.com/v/{youtubeId}").Should().Be(youtubeId);
        TrailerDlHelper.GetYoutubeId($"http://www.youtu.be/{youtubeId}").Should().Be(youtubeId);
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
    public void YtHelper_GetPartialIntroName(KpTrailer trailer, bool withDate)
    {
        var introName = TrailerDlHelper.GetPartialTrailerName(trailer);
        introName.Should().Be(withDate ? "videoName (2024)" : "videoName");
    }

    public static TheoryData<KpTrailer, bool> KpTrailerData => new()
    {
        {
            new KpTrailer()
            {
                VideoName = "videoName",
                PremierDate = new DateTimeOffset(2024, 1, 1, 1, 1, 1, TimeSpan.Zero)
            },
            true
        },
        {
            new KpTrailer()
            {
                VideoName = "videoName"
            },
            false
        }
    };

    [Fact]
    public async void YtHelper_GetUserAgent_Local()
    {
        _ = _applicationPaths
            .SetupGet(m => m.PluginConfigurationsPath)
            .Returns(nameof(YtHelper_GetUserAgent_Local));

        (await TrailerDlHelper.GetUserAgent(_httpClient, new Mock<ILogger>().Object, CancellationToken.None)).Should().BeOneOf(TrailerDlHelper.UserAgents);
    }

    [Fact]
    public async void YtHelper_GetUserAgent_API()
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
}
