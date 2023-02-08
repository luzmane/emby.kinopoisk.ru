using EmbyKinopoiskRu.Helper;

using NUnit.Framework;

namespace EmbyKinopoiskRu.Tests
{
    public class HelperTests
    {
        [Test]
        public void DetectYearFromMoviePath_WithYear()
        {
            var fileName = "/mnt/share2/video_child_vault/Летучий корабль.1979.WEBRip 720p.mkv";
            var movieName = "Летучий корабль";
            Assert.AreEqual(1979, KpHelper.DetectYearFromMoviePath(fileName, movieName));
        }

        [Test]
        public void DetectYearFromMoviePath_WithoutYear()
        {
            var fileName = "/mnt/share2/video_child_vault/Леди и бродяга.mkv";
            var movieName = "Леди и бродяга";
            Assert.IsNull(KpHelper.DetectYearFromMoviePath(fileName, movieName));
        }

        [Test]
        public void DetectYearFromMoviePath_IncorrectYear()
        {
            var fileName = "/mnt/share2/video_child_vault/Леди и бродяга_1700.mkv";
            var movieName = "Леди и бродяга";
            Assert.IsNull(KpHelper.DetectYearFromMoviePath(fileName, movieName));
        }

        [Test]
        public void DetectYearFromMoviePath_IncorrectYear_2()
        {
            var fileName = "/mnt/share2/video_child_vault/Леди и бродяга_3000.mkv";
            var movieName = "Леди и бродяга";
            Assert.IsNull(KpHelper.DetectYearFromMoviePath(fileName, movieName));
        }
    }
}
