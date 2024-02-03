using EmbyKinopoiskRu.Configuration;
using EmbyKinopoiskRu.Provider.LocalMetadata;

using FluentAssertions;

using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Model.Entities;

namespace EmbyKinopoiskRu.Tests.KinopoiskApiUnofficial;

[Collection("Sequential")]
public class KpLocalMetadataTest : BaseTest
{
    private static readonly NLog.ILogger Logger = NLog.LogManager.GetLogger(nameof(KpLocalMetadataTest));

    private readonly KpMovieLocalMetadata _kpMovieLocalMetadata;
    private readonly LibraryOptions _movieLibraryOptions;


    #region Test configs
    public KpLocalMetadataTest() : base(Logger)
    {
        _pluginConfiguration.Token = GetKinopoiskUnofficialToken();
        _pluginConfiguration.ApiType = PluginConfiguration.KinopoiskAPIUnofficialTech;

        ConfigLibraryManager();

        ConfigXmlSerializer();

        _movieLibraryOptions = new LibraryOptions
        {
            ContentType = "movies",
            EnableAdultMetadata = true,
            ImportCollections = true,
            MetadataCountryCode = "RU",
            MinCollectionItems = 1,
            Name = "Movies",
            PathInfos = new[]{
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
            TypeOptions = new[]{
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
    public async void KpLocalMetadata_WithKpInName()
    {
        Logger.Info($"Start '{nameof(KpLocalMetadata_WithKpInName)}'");

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
        result.Item.GetProviderId(Plugin.PluginKey).Should().Be("326");
        result.Item.MediaType.Should().Be("Video");

        _logManager.Verify(lm => lm.GetLogger("KpMovieLocalMetadata"), Times.Once());
        _logManager.Verify(lm => lm.GetLogger("KinopoiskRu"), Times.Once());
        _fileSystem.Verify(fs => fs.GetDirectoryName(It.IsAny<string>()), Times.Once());
        VerifyNoOtherCalls();

        Logger.Info($"Finished '{nameof(KpLocalMetadata_WithKpInName)}'");
    }

    [Fact]
    public async void KpLocalMetadata_WithoutKpInName()
    {
        Logger.Info($"Start '{nameof(KpLocalMetadata_WithoutKpInName)}'");

        _ = _applicationPaths
            .SetupGet(m => m.PluginConfigurationsPath)
            .Returns("UN_KpLocalMetadata_WithoutKpInName");

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

        _logManager.Verify(lm => lm.GetLogger(It.IsAny<string>()), Times.Exactly(2));
        _fileSystem.Verify(fs => fs.GetDirectoryName("/emby/movie_library/Побег из Шоушенка.mkv"), Times.Once());
        VerifyNoOtherCalls();

        Logger.Info($"Finished '{nameof(KpLocalMetadata_WithoutKpInName)}'");
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
