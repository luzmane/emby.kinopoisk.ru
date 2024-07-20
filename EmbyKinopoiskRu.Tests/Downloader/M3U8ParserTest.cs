using EmbyKinopoiskRu.TrailerDownloader.M3UParser;
using EmbyKinopoiskRu.TrailerDownloader.M3UParser.Model;

using FluentAssertions;

namespace EmbyKinopoiskRu.Tests.Downloader;

public class M3U8ParserTest : BaseTest
{
    private static readonly NLog.ILogger Logger = NLog.LogManager.GetLogger(nameof(M3U8ParserTest));

    public M3U8ParserTest() : base(Logger)
    {
        _ = _applicationPaths
            .SetupGet(m => m.PluginConfigurationsPath)
            .Returns(nameof(M3U8ParserTest));
    }


    [Fact]
    public void M3U8Parser_ParseMasterM3U8_Normal()
    {
        const string url = "https://strm.yandex.ru/vh-kp-converted/ott-content/383443412-4433a92e4952769ca8b8a0d0c52a8bac/ysign1=98c912c0be85cd9ce1611719539cec20299c3a7a336356f579318bbdf7bcbbcd,abcID=1358,from=ott-kp,pfx,sfx,ts=66a90ada/master.m3u8";
        var lines = File.ReadAllText(Path.Combine("Files", "normal_master.m3u8"));
        M3U8Parser m3U8Parser = new M3U8Parser(_logManager.Object);
        (List<XStreamInf> xStream, List<XMedia> xMedia) = m3U8Parser.ParseMasterM3U8(url, lines, 480);
        xStream.Should().HaveCount(6);
        xStream.Should().AllSatisfy(x =>
        {
            x.ResolutionY.Should().Be(360);
            x.Url.Should().NotBeEmpty();
            x.Url.Should().StartWith("https://strm.yandex.ru/vh-kp-converted");
        });
        xMedia.Should().HaveCount(6);
        xMedia.Should().AllSatisfy(x =>
        {
            x.Url.Should().NotBeEmpty();
            x.Url.Should().StartWith("https://strm.yandex.ru/vh-kp-converted");
        });
    }

    [Fact]
    public void M3U8Parser_ParseMasterM3U8_NoResolution()
    {
        const string url = "https://strm.yandex.ru/vh-kp-converted/ott-content/383443412-4433a92e4952769ca8b8a0d0c52a8bac/ysign1=98c912c0be85cd9ce1611719539cec20299c3a7a336356f579318bbdf7bcbbcd,abcID=1358,from=ott-kp,pfx,sfx,ts=66a90ada/master.m3u8";
        var lines = File.ReadAllText(Path.Combine("Files", "no_resolution_master.m3u8"));
        M3U8Parser m3U8Parser = new M3U8Parser(_logManager.Object);
        (List<XStreamInf> xStream, List<XMedia> xMedia) = m3U8Parser.ParseMasterM3U8(url, lines, 480);
        xStream.Should().HaveCount(55);
        xStream.Should().AllSatisfy(x =>
        {
            x.ResolutionY.Should().Be(0);
            x.Url.Should().NotBeEmpty();
            x.Url.Should().StartWith("https://strm.yandex.ru/vh-kp-converted");
        });
        xMedia.Should().HaveCount(6);
        xMedia.Should().AllSatisfy(x =>
        {
            x.Url.Should().NotBeEmpty();
            x.Url.Should().StartWith("https://strm.yandex.ru/vh-kp-converted");
        });
    }

    [Fact]
    public void M3U8Parser_ParseMasterM3U8_NoAudio()
    {
        const string url = "https://strm.yandex.ru/vh-kp-converted/ott-content/383443412-4433a92e4952769ca8b8a0d0c52a8bac/ysign1=98c912c0be85cd9ce1611719539cec20299c3a7a336356f579318bbdf7bcbbcd,abcID=1358,from=ott-kp,pfx,sfx,ts=66a90ada/master.m3u8";
        var lines = File.ReadAllText(Path.Combine("Files", "no_audio_master.m3u8"));
        M3U8Parser m3U8Parser = new M3U8Parser(_logManager.Object);
        (List<XStreamInf> xStream, List<XMedia> xMedia) = m3U8Parser.ParseMasterM3U8(url, lines, 480);
        xStream.Should().HaveCount(6);
        xStream.Should().AllSatisfy(x =>
        {
            x.ResolutionY.Should().Be(360);
            x.Url.Should().NotBeEmpty();
            x.Url.Should().StartWith("https://strm.yandex.ru/vh-kp-converted");
        });
        xMedia.Should().HaveCount(0);
    }

    [Fact]
    public void M3U8Parser_ParseM3U8_Fail_MaxDuration()
    {
        Plugin.Instance.Configuration.TrailerMaxDuration = 1;

        var lines = File.ReadAllText(Path.Combine("Files", "trailer_96sec.m3u8"));
        M3U8Parser m3U8Parser = new M3U8Parser(_logManager.Object);
        var urls = m3U8Parser.ParseM3U8(lines);
        urls.Should().HaveCount(0);
    }

    [Fact]
    public void M3U8Parser_ParseM3U8_Pass_Duration()
    {
        Plugin.Instance.Configuration.TrailerMaxDuration = 5;

        var lines = File.ReadAllText(Path.Combine("Files", "trailer_96sec.m3u8"));
        M3U8Parser m3U8Parser = new M3U8Parser(_logManager.Object);
        var urls = m3U8Parser.ParseM3U8(lines);
        urls.Should().HaveCount(24);
        urls.Should().AllSatisfy(x =>
        {
            x.Should().NotBeEmpty();
            x.Should().StartWith("https://strm-rad-23.strm.yandex.net/vh-kp-converted/");
        });
    }

    [Fact]
    public void M3U8Parser_ParseM3U8_Fail_MinDuration()
    {
        Plugin.Instance.Configuration.TrailerMaxDuration = 5;

        var lines = File.ReadAllText(Path.Combine("Files", "teaser_32sec.m3u8"));
        M3U8Parser m3U8Parser = new M3U8Parser(_logManager.Object);
        var urls = m3U8Parser.ParseM3U8(lines);
        urls.Should().HaveCount(0);
    }
}
