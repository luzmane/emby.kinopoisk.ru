using EmbyKinopoiskRu.Api;
using EmbyKinopoiskRu.Helper;
using EmbyKinopoiskRu.Tests.Utils;

using FluentAssertions;

using MediaBrowser.Model.Logging;

namespace EmbyKinopoiskRu.Tests.Common;

public class TrailerDlHelperTest : BaseTest
{
    private const string UserAgentApiKey = "wd0fL7jxJVrF6K0g2Zb3y1tN3lLHkRwz";

    private static readonly NLog.ILogger Logger = NLog.LogManager.GetLogger(nameof(TrailerDlHelperTest));
    private static readonly char[] InvalidFileNameChars = Path.GetInvalidFileNameChars();

    public TrailerDlHelperTest() : base(Logger)
    {
    }

    [Theory]
    [MemberData(nameof(GetYoutubeIdData))]
    public void TrailerDlHelper_GetYoutubeId(string url, string youtubeId)
    {
        TrailerDlHelper.GetYoutubeId(url).Should().Be(youtubeId);
    }

    [Theory]
    [MemberData(nameof(GetIntroNameData))]
    public void TrailerDlHelper_GetIntroName(IntroNameData introNameData, string expectedResult)
    {
        TrailerDlHelper.GetIntroName(introNameData.VideoName, introNameData.VideoId, introNameData.Extension).Should().Be(expectedResult);
    }

    [Theory]
    [MemberData(nameof(GetPartialIntroNameData))]
    public void TrailerDlHelper_GetPartialIntroName(KpTrailer trailer, string expectedResult)
    {
        var introName = TrailerDlHelper.GetPartialTrailerName(trailer);
        introName.Should().Be(expectedResult);
    }

    [Fact]
    public async Task TrailerDlHelper_GetUserAgent_Local()
    {
        _ = _applicationPaths
            .SetupGet(m => m.PluginConfigurationsPath)
            .Returns(nameof(TrailerDlHelper_GetUserAgent_Local));

        (await TrailerDlHelper.GetUserAgent(_httpClient, new Mock<ILogger>().Object, CancellationToken.None)).Should().BeOneOf(TrailerDlHelper.UserAgents);
    }

    [Fact]
    public async Task TrailerDlHelper_GetUserAgent_API()
    {
        _ = _applicationPaths
            .SetupGet(m => m.PluginConfigurationsPath)
            .Returns(nameof(TrailerDlHelper_GetUserAgent_Local));

        _pluginConfiguration.UserAgentApiKey = GetUserAgentApiKey();

        Mock<ILogger> logger = new();
        (await TrailerDlHelper.GetUserAgent(_httpClient, logger.Object, CancellationToken.None)).Should().NotBeNullOrWhiteSpace();
        logger.Verify(l => l.Warn(It.Is<string>(m =>
            m.StartsWith("Unable to fetch dynamic")
            || m.StartsWith("The key for UserAgent API is empty")
            || m.StartsWith("Status code received from the API"))), Times.Never());
        logger.Verify(l => l.Info("Successfully fetched User Agent"), Times.Once());
    }

    [Theory]
    [MemberData(nameof(GetKinopoiskTrailerIdData))]
    public void TrailerDlHelper_GetKinopoiskTrailerId(string url, string kpId)
    {
        TrailerDlHelper.GetKinopoiskTrailerId(url).Should().Be(kpId);
    }

    [Theory]
    [MemberData(nameof(IsValidTrailerData))]
    public void TrailerDlHelper_IsValidTrailer(KpTrailer trailer, bool expectedResult)
    {
        TrailerDlHelper.IsValidTrailer(trailer).Should().Be(expectedResult);
    }

    [Theory]
    [MemberData(nameof(IsRussianTrailerData))]
    public void TrailerDlHelper_IsRussianTrailer(KpTrailer trailer, bool expectedResult)
    {
        TrailerDlHelper.IsRussianTrailer(trailer).Should().Be(expectedResult);
    }


    private static string GetUserAgentApiKey()
    {
        var apiKey = Environment.GetEnvironmentVariable("USER_AGENT_API_KEY");
        Logger.Info($"UserAGent API key length is: {apiKey?.Length ?? 0}");
        return string.IsNullOrWhiteSpace(apiKey) ? UserAgentApiKey : apiKey;
    }

    #region MemberData

    public static TheoryData<KpTrailer, bool> IsRussianTrailerData => new()
    {
        {
            new KpTrailer
            {
                VideoName = "trailer Name Equal To Video Name",
                TrailerName = "trailer name equal to video name"
            },
            false
        },
        {
            new KpTrailer
            {
                VideoName = "video Name",
                TrailerName = "название трейлера"
            },
            true
        },
        {
            new KpTrailer
            {
                VideoName = "video Name",
                TrailerName = "испанский трейлер"
            },
            false
        },
        {
            new KpTrailer
            {
                VideoName = "video Name",
                TrailerName = "трейлер"
            },
            true
        },
    };

    public static TheoryData<KpTrailer, bool> IsValidTrailerData => new()
    {
        {
            new KpTrailer
            {
                TrailerName = "no video name - invalid"
            },
            false
        },
        {
            new KpTrailer
            {
                VideoName = "no trailer name - invalid",
            },
            false
        },
        {
            new KpTrailer
            {
                VideoName = "trailer name equal to video name",
                TrailerName = "trailer name equal to video name "
            },
            true
        },
        {
            new KpTrailer
            {
                VideoName = "TrailerStopWordList",
                TrailerName = "TrailerStopWord фрагмент"
            },
            false
        },
        {
            new KpTrailer
            {
                VideoName = "TrailerStopWordList",
                TrailerName = "TrailerStopWord fragment"
            },
            false
        },
        {
            new KpTrailer
            {
                VideoName = "valid video name",
                TrailerName = "valid trailer name"
            },
            true
        },
    };

    public static TheoryData<KpTrailer, string> GetPartialIntroNameData => new()
    {
        {
            new KpTrailer
            {
                VideoName = "videoName",
                TrailerName = "trailerName",
                PremierDate = new DateTimeOffset(2024, 1, 1, 1, 1, 1, TimeSpan.Zero)
            },
            "videoName (2024) (trailerName)"
        },
        {
            new KpTrailer
            {
                VideoName = "videoName",
                TrailerName = "trailerName",
            },
            "videoName (trailerName)"
        },
        {
            new KpTrailer
            {
                VideoName = "videoName",
                TrailerName = "videoName",
                PremierDate = new DateTimeOffset(2024, 1, 1, 1, 1, 1, TimeSpan.Zero)
            },
            "videoName (2024)"
        },
        {
            new KpTrailer
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

    public static TheoryData<string, string> GetKinopoiskTrailerIdData => new()
    {
        { "https://widgets.kinopoisk.ru/discovery/trailer/1234?onlyPlayer=1&autoplay=1&cover=1", "1234" },
        { "http://widgets.kinopoisk.ru/discovery/trailer/4321?onlyPlayer=1&autoplay=1&cover=1", "4321" },
        { "https://trailers.s3.mds.yandex.net/video_original/183824-7139700739965229.mp4", "183824" },
        { "http://trailers.s3.mds.yandex.net/video_original/172729-c5c47f9b0dd2d39452c40c5a8766f3ba.mov", "172729" },
    };

    public static TheoryData<IntroNameData, string> GetIntroNameData => new()
    {
        { new IntroNameData("All Valid Chars", "videoId", "mp4"), "All Valid Chars [videoId].mp4" },
        { new IntroNameData("All Valid Chars Trim ", "videoId", "mp4"), "All Valid Chars Trim [videoId].mp4" },
        { new IntroNameData($"Has Invalid Chars{(InvalidFileNameChars.Length > 0 ? InvalidFileNameChars[0] : "")}", "videoId", "mp4"), "Has Invalid Chars [videoId].mp4" },
        { new IntroNameData($"Has Invalid Chars Trim {(InvalidFileNameChars.Length > 0 ? InvalidFileNameChars[0] : "")}", "videoId", "mp4"), "Has Invalid Chars Trim [videoId].mp4" },
    };

    #endregion
}
