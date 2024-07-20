namespace EmbyKinopoiskRu.Tests.Utils;

public class IntroNameData
{
    public IntroNameData(string videoName, string videoId, string extension)
    {
        VideoName = videoName;
        VideoId = videoId;
        Extension = extension;
    }

    public string VideoName { get; set; }
    public string VideoId { get; set; }
    public string Extension { get; set; }
}
