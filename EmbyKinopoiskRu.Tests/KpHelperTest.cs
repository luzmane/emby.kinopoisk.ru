using EmbyKinopoiskRu.Helper;

namespace EmbyKinopoiskRu.Tests;

public class KpHelperTest
{
    [Fact]
    public void KpHelper_DetectYearFromMoviePath_WithYear()
    {
        var fileName = "/mnt/share2/video_child_vault/Летучий корабль.1979.WEBRip 720p.mkv";
        var movieName = "Летучий корабль";
        Assert.Equal(1979, KpHelper.DetectYearFromMoviePath(fileName, movieName));
    }

    [Fact]
    public void KpHelper_DetectYearFromMoviePath_WithoutYear()
    {
        var fileName = "/mnt/share2/video_child_vault/Леди и бродяга.mkv";
        var movieName = "Леди и бродяга";
        Assert.Null(KpHelper.DetectYearFromMoviePath(fileName, movieName));
    }

    [Fact]
    public void KpHelper_DetectYearFromMoviePath_IncorrectYear_1700()
    {
        var fileName = "/mnt/share2/video_child_vault/Леди и бродяга_1700.mkv";
        var movieName = "Леди и бродяга";
        Assert.Null(KpHelper.DetectYearFromMoviePath(fileName, movieName));
    }

    [Fact]
    public void KpHelper_DetectYearFromMoviePath_IncorrectYear_3000()
    {
        var fileName = "/mnt/share2/video_child_vault/Леди и бродяга_3000.mkv";
        var movieName = "Леди и бродяга";
        Assert.Null(KpHelper.DetectYearFromMoviePath(fileName, movieName));
    }

    [Fact]
    public void KpHelper_CleanName()
    {
        Assert.Equal("леди и бродяга 3000", KpHelper.CleanName("Леди и бродяга_3000"));
        Assert.Equal("не бойся я с тобой", KpHelper.CleanName("Не бойся, я с тобой!"));
        Assert.Equal("korol i shut s01 2023 webrip", KpHelper.CleanName("Korol.i.Shut.S01.2023.WEBRip."));
    }
}
