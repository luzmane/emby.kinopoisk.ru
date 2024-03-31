using EmbyKinopoiskRu.Provider.LocalMetadata;

using FluentAssertions;

using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Configuration;

namespace EmbyKinopoiskRu.Tests.KinopoiskDev;

[Collection("Sequential")]
public class KpLocalMetadataTest : BaseTest
{
    private static readonly NLog.ILogger Logger = NLog.LogManager.GetLogger(nameof(KpLocalMetadataTest));

    private readonly KpMovieLocalMetadata _kpMovieLocalMetadata;
    private readonly LibraryOptions _movieLibraryOptions;

    #region Test configs

    public KpLocalMetadataTest() : base(Logger)
    {
        _pluginConfiguration.Token = GetKinopoiskDevToken();

        ConfigLibraryManager();

        ConfigXmlSerializer();

        _movieLibraryOptions = new LibraryOptions
        {
            ContentType = "movies",
            EnableAdultMetadata = true,
            ImportCollections = true,
            MetadataCountryCode = "RU",
            MinCollectionItems = 1,
            PathInfos = new[]
            {
                new MediaPathInfo
                {
                    NetworkPath = null,
                    Path = "/emby/movie_library"
                }
            },
            PreferredImageLanguage = "ru",
            PreferredMetadataLanguage = "ru",
            SkipSubtitlesIfEmbeddedSubtitlesPresent = true,
            SkipSubtitlesIfAudioTrackMatches = true,
            TypeOptions = new[]
            {
                new TypeOptions
                {
                    Type = "Movie"
                }
            }
        };

        _kpMovieLocalMetadata = new KpMovieLocalMetadata(_logManager.Object);
    }

    #endregion

    [Fact]
    public async void KpLocalMetadata_With_KpInName()
    {
        Logger.Info($"Start '{nameof(KpLocalMetadata_With_KpInName)}'");

        var itemInfo = new ItemInfo(new Movie
        {
            Path = "/emby/movie_library/kp326_Побег из Шоушенка.mkv",
            Container = "mkv",
            IsInMixedFolder = false,
            Id = Guid.NewGuid(),
            Name = "Побег из Шоушенка"
        });

        using var cancellationTokenSource = new CancellationTokenSource();
        MetadataResult<Movie> result = await _kpMovieLocalMetadata.GetMetadata(itemInfo, _movieLibraryOptions, _directoryService.Object, cancellationTokenSource.Token);

        result.HasMetadata.Should().BeTrue();
        result.Item.Should().NotBeNull();
        result.Item.ProviderIds.Should().ContainSingle();
        result.Item.ProviderIds[Plugin.PluginKey].Should().Be("326");
        result.Item.MediaType.Should().Be("Video");

        _logManager.Verify(lm => lm.GetLogger("KpMovieLocalMetadata"), Times.Once());
        _logManager.Verify(lm => lm.GetLogger("KinopoiskRu"), Times.Once());
        _fileSystem.Verify(fs => fs.GetDirectoryName(It.IsAny<string>()), Times.Once());
        VerifyNoOtherCalls();

        Logger.Info($"Finished '{nameof(KpLocalMetadata_With_KpInName)}'");
    }

    [Fact]
    public async void KpLocalMetadata_Without_KpInName()
    {
        Logger.Info($"Start '{nameof(KpLocalMetadata_Without_KpInName)}'");

        var itemInfo = new ItemInfo(new Movie
        {
            Path = "/emby/movie_library/Побег из Шоушенка.mkv",
            Container = "mkv",
            IsInMixedFolder = false,
            Id = Guid.NewGuid(),
            Name = "Побег из Шоушенка"
        });

        using var cancellationTokenSource = new CancellationTokenSource();
        MetadataResult<Movie> result = await _kpMovieLocalMetadata.GetMetadata(itemInfo, _movieLibraryOptions, _directoryService.Object, cancellationTokenSource.Token);

        result.HasMetadata.Should().BeFalse();
        result.Item.Should().BeNull();

        _logManager.Verify(lm => lm.GetLogger("KpMovieLocalMetadata"), Times.Once());
        _logManager.Verify(lm => lm.GetLogger("KinopoiskRu"), Times.Once());
        _fileSystem.Verify(fs => fs.GetDirectoryName(It.IsAny<string>()), Times.Once());
        VerifyNoOtherCalls();

        Logger.Info($"Finished '{nameof(KpLocalMetadata_Without_KpInName)}'");
    }

    [Fact]
    public void KpLocalMetadata_ForCodeCoverage()
    {
        Logger.Info($"Start '{nameof(KpLocalMetadata_ForCodeCoverage)}'");

        _kpMovieLocalMetadata.Name.Should().NotBeNull();
        _ = new KpSeriesLocalMetadata(_logManager.Object);

        Logger.Info($"Finish '{nameof(KpLocalMetadata_ForCodeCoverage)}'");
    }
}
