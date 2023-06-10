using EmbyKinopoiskRu.Helper;

using FluentAssertions;

namespace EmbyKinopoiskRu.Tests.Common;

public class KpHelperTest
{
    [Fact]
    public void KpHelper_DetectYearFromMoviePath_WithYear()
    {
        var fileName = "/mnt/share2/video_child_vault/Летучий корабль.1979.WEBRip 720p.mkv";
        var movieName = "Летучий корабль";
        KpHelper.DetectYearFromMoviePath(fileName, movieName).Should().Be(1979, "this value was in file name");
    }

    [Fact]
    public void KpHelper_DetectYearFromMoviePath_WithoutYear()
    {
        var fileName = "/mnt/share2/video_child_vault/Леди и бродяга.mkv";
        var movieName = "Леди и бродяга";
        KpHelper.DetectYearFromMoviePath(fileName, movieName).Should().BeNull("file doesn't have year");
    }

    [Fact]
    public void KpHelper_DetectYearFromMoviePath_IncorrectYear_1700()
    {
        var fileName = "/mnt/share2/video_child_vault/Леди и бродяга_1700.mkv";
        var movieName = "Леди и бродяга";
        KpHelper.DetectYearFromMoviePath(fileName, movieName).Should().BeNull("year is not in range");
    }

    [Fact]
    public void KpHelper_DetectYearFromMoviePath_IncorrectYear_3000()
    {
        var fileName = "/mnt/share2/video_child_vault/Леди и бродяга_3000.mkv";
        var movieName = "Леди и бродяга";
        KpHelper.DetectYearFromMoviePath(fileName, movieName).Should().BeNull("year is not in range");
    }

    [Fact]
    public void KpHelper_CleanName()
    {
        KpHelper.CleanName("Леди и бродяга_3000").Should().Be("леди и бродяга 3000", "removed non alphanumeric and lowercase");
        KpHelper.CleanName("Не бойся, я с тобой!").Should().Be("не бойся я с тобой", "removed non alphanumeric and lowercase");
        KpHelper.CleanName("Korol.i.Shut.S01.2023.WEBRip.").Should().Be("korol i shut s01 2023 webrip", "removed non alphanumeric and lowercase");
    }
}
