using EmbyKinopoiskRu.Helper;

using FluentAssertions;

namespace EmbyKinopoiskRu.Tests.Common;

public class KpHelperTest
{
    [Fact]
    public void KpHelper_DetectYearFromMoviePath_WithYear()
    {
        const string fileName = "/mnt/share2/video_child_vault/Летучий корабль.1979.WEBRip 720p.mkv";
        const string movieName = "Летучий корабль";
        KpHelper.DetectYearFromMoviePath(fileName, movieName).Should().Be(1979);
    }

    [Fact]
    public void KpHelper_DetectYearFromMoviePath_WithoutYear()
    {
        const string fileName = "/mnt/share2/video_child_vault/Леди и бродяга.mkv";
        const string movieName = "Леди и бродяга";
        KpHelper.DetectYearFromMoviePath(fileName, movieName).Should().BeNull();
    }

    [Fact]
    public void KpHelper_DetectYearFromMoviePath_IncorrectYear_1700()
    {
        const string fileName = "/mnt/share2/video_child_vault/Леди и бродяга_1700.mkv";
        const string movieName = "Леди и бродяга";
        KpHelper.DetectYearFromMoviePath(fileName, movieName).Should().BeNull();
    }

    [Fact]
    public void KpHelper_DetectYearFromMoviePath_IncorrectYear_3000()
    {
        const string fileName = "/mnt/share2/video_child_vault/Леди и бродяга_3000.mkv";
        const string movieName = "Леди и бродяга";
        KpHelper.DetectYearFromMoviePath(fileName, movieName).Should().BeNull();
    }

    [Fact]
    public void KpHelper_CleanName()
    {
        KpHelper.CleanName("Леди и бродяга_3000").Should().Be("леди и бродяга 3000");
        KpHelper.CleanName("Не бойся, я с тобой!").Should().Be("не бойся я с тобой");
        KpHelper.CleanName("Korol.i.Shut.S01.2023.WEBRip.").Should().Be("korol i shut s01 2023 webrip");
    }
}
