using System.Net;
using System.Globalization;

using Emby.Notifications;

using EmbyKinopoiskRu.Configuration;
using EmbyKinopoiskRu.Provider.RemoteMetadata;

using FluentAssertions;

using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Providers;
using MediaBrowser.Model.Querying;
using MediaBrowser.Controller.Collections;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Activity;

namespace EmbyKinopoiskRu.Tests.KinopoiskDev;

[Collection("Sequential")]
public class KpMovieProviderTest : BaseTest
{
    private static readonly NLog.ILogger Logger = NLog.LogManager.GetLogger(nameof(KpMovieProviderTest));

    private readonly KpMovieProvider _kpMovieProvider;


    #region Test configs

    public KpMovieProviderTest() : base(Logger)
    {
        _pluginConfiguration.Token = GetKinopoiskDevToken();

        SetupLibraryManager();

        ConfigXmlSerializer();

        _kpMovieProvider = new KpMovieProvider(_httpClient, _logManager.Object);
    }

    private void SetupLibraryManager()
    {
        ConfigLibraryManager();

        List<KeyValuePair<string, string>> potterSequences = new long[] { 688, 322, 8_408, 48_356, 89_515, 276_762, 407_636, 4_716_622 }
            .Select(id => new KeyValuePair<string, string>(Plugin.PluginKey, id.ToString(CultureInfo.InvariantCulture)))
            .ToList();
        List<KeyValuePair<string, string>> imdbPotterSequences = new[] { "tt1201607", "tt0330373", "tt0373889", "tt0417741", "tt0926084", "tt16116174" }
            .Select(id => new KeyValuePair<string, string>(MetadataProviders.Imdb.ToString(), id.ToString(CultureInfo.InvariantCulture)))
            .ToList();
        List<KeyValuePair<string, string>> tmdbPotterSequences = new[] { "12445", "674", "675", "767", "12444", "899082" }
            .Select(id => new KeyValuePair<string, string>(MetadataProviders.Tmdb.ToString(), id.ToString(CultureInfo.InvariantCulture)))
            .ToList();

        _ = _libraryManager // EmbyHelper.GetSequenceInternalIds(). Items in lib with Kp internal Id
            .Setup(m => m.QueryItems(It.Is<InternalItemsQuery>(query =>
                !query.Recursive
                && query.IsVirtualItem == false
                && query.IncludeItemTypes.Length == 2
                && nameof(Movie).Equals(query.IncludeItemTypes[0], StringComparison.Ordinal)
                && nameof(Series).Equals(query.IncludeItemTypes[1], StringComparison.Ordinal)
                && query.AnyProviderIdEquals.Count == potterSequences.Count
                && query.AnyProviderIdEquals.All(item => potterSequences.Contains(item)))))
            .Returns(new QueryResult<BaseItem>
            {
                Items = new BaseItem[]
                {
                    new Movie
                    {
                        Name = "Гарри Поттер и Тайная комната",
                        InternalId = 101L,
                        ProviderIds = new ProviderIdDictionary(new Dictionary<string, string>
                        {
                            { Plugin.PluginKey, "688" },
                            { MetadataProviders.Imdb.ToString(), "tt0295297" },
                            { MetadataProviders.Tmdb.ToString(), "672" }
                        })
                    },
                    new Movie
                    {
                        Name = "Гарри Поттер и узник Азкабана",
                        InternalId = 102L,
                        ProviderIds = new ProviderIdDictionary(new Dictionary<string, string>
                        {
                            { Plugin.PluginKey, "322" },
                            { MetadataProviders.Imdb.ToString(), "tt0304141" },
                            { MetadataProviders.Tmdb.ToString(), "673" }
                        })
                    }
                }
            });

        _ = _libraryManager // EmbyHelper.GetSequenceInternalIds(). Items in lib with IMDB internal Id
            .Setup(m => m.QueryItems(It.Is<InternalItemsQuery>(query =>
                !query.Recursive
                && query.IsVirtualItem == false
                && query.IncludeItemTypes.Length == 2
                && nameof(Movie).Equals(query.IncludeItemTypes[0], StringComparison.Ordinal)
                && nameof(Series).Equals(query.IncludeItemTypes[1], StringComparison.Ordinal)
                && query.AnyProviderIdEquals.Count == imdbPotterSequences.Count
                && query.AnyProviderIdEquals.All(item => imdbPotterSequences.Contains(item))
            )))
            .Returns(new QueryResult<BaseItem>
            {
                Items = new BaseItem[]
                {
                    new Movie
                    {
                        Name = "Гарри Поттер и Кубок огня",
                        InternalId = 103L,
                        ProviderIds = new ProviderIdDictionary(new Dictionary<string, string>
                        {
                            { Plugin.PluginKey, "8408" },
                            { MetadataProviders.Imdb.ToString(), "tt0330373" },
                            { MetadataProviders.Tmdb.ToString(), "674" }
                        })
                    },
                    new Movie
                    {
                        Name = "Гарри Поттер и Орден Феникса",
                        InternalId = 104L,
                        ProviderIds = new ProviderIdDictionary(new Dictionary<string, string>
                        {
                            { Plugin.PluginKey, "48356" },
                            { MetadataProviders.Tmdb.ToString(), "675" },
                            { MetadataProviders.Imdb.ToString(), "tt0373889" }
                        })
                    },
                    new Movie
                    {
                        Name = "Гарри Поттер и Принц-полукровка",
                        InternalId = 105L,
                        ProviderIds = new ProviderIdDictionary(new Dictionary<string, string>
                        {
                            { Plugin.PluginKey, "89515" },
                            { MetadataProviders.Tmdb.ToString(), "767" },
                            { MetadataProviders.Imdb.ToString(), "tt0417741" }
                        })
                    },
                    new Movie
                    {
                        Name = "Гарри Поттер и Дары Смерти: Часть I",
                        InternalId = 106L,
                        ProviderIds = new ProviderIdDictionary(new Dictionary<string, string>
                        {
                            { Plugin.PluginKey, "276762" },
                            { MetadataProviders.Tmdb.ToString(), "12444" },
                            { MetadataProviders.Imdb.ToString(), "tt0926084" }
                        })
                    }
                }
            });

        _ = _libraryManager // EmbyHelper.GetSequenceInternalIds(). Items in lib with TMDB internal Id
            .Setup(m => m.QueryItems(It.Is<InternalItemsQuery>(query =>
                !query.Recursive
                && query.IsVirtualItem == false
                && query.IncludeItemTypes.Length == 2
                && nameof(Movie).Equals(query.IncludeItemTypes[0], StringComparison.Ordinal)
                && nameof(Series).Equals(query.IncludeItemTypes[1], StringComparison.Ordinal)
                && query.AnyProviderIdEquals.Count == tmdbPotterSequences.Count
                && query.AnyProviderIdEquals.All(item => tmdbPotterSequences.Contains(item))
            )))
            .Returns(new QueryResult<BaseItem>
            {
                Items = new BaseItem[]
                {
                    new Movie
                    {
                        Name = "Гарри Поттер и Дары Смерти: Часть II",
                        InternalId = 107L,
                        ProviderIds = new ProviderIdDictionary(new Dictionary<string, string>
                        {
                            { Plugin.PluginKey, "407636" },
                            { MetadataProviders.Imdb.ToString(), "tt1201607" },
                            { MetadataProviders.Tmdb.ToString(), "12445" }
                        })
                    },
                    new Movie
                    {
                        Name = "Гарри Поттер 20 лет спустя: Возвращение в Хогвартс",
                        InternalId = 108L,
                        ProviderIds = new ProviderIdDictionary(new Dictionary<string, string>
                        {
                            { Plugin.PluginKey, "4716622" },
                            { MetadataProviders.Tmdb.ToString(), "899082" },
                            { MetadataProviders.Imdb.ToString(), "tt16116174" }
                        })
                    }
                }
            });
    }

    #endregion

    [Fact]
    public async void KpMovieProvider_ForCodeCoverage()
    {
        Logger.Info($"Start '{nameof(KpMovieProvider_ForCodeCoverage)}'");

        _kpMovieProvider.Name.Should().NotBeNull();

        _kpMovieProvider.Features.Should().NotBeEmpty();

        HttpResponseInfo response = await _kpMovieProvider.GetImageResponse("https://www.google.com", CancellationToken.None);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        _logManager.Verify(lm => lm.GetLogger("KpMovieProvider"), Times.Once());
        _logManager.Verify(lm => lm.GetLogger("KinopoiskRu"), Times.Once());
        VerifyNoOtherCalls();

        Logger.Info($"Finished '{nameof(KpMovieProvider_ForCodeCoverage)}'");
    }

    [Fact]
    public async void KpMovieProvider_GetMetadata_Provider_Kp()
    {
        Logger.Info($"Start '{nameof(KpMovieProvider_GetMetadata_Provider_Kp)}'");

        var movieInfo = new MovieInfo
        {
            ProviderIds = new ProviderIdDictionary(new Dictionary<string, string>
            {
                { Plugin.PluginKey, "326" }
            })
        };

        _ = _applicationPaths
            .SetupGet(m => m.PluginConfigurationsPath)
            .Returns("KpMovieProvider_GetMetadata_Provider_Kp");

        using var cancellationTokenSource = new CancellationTokenSource();
        MetadataResult<Movie> result = await _kpMovieProvider.GetMetadata(movieInfo, cancellationTokenSource.Token);

        result.HasMetadata.Should().BeTrue();
        VerifyMovie326(result.Item);

        result.People.Should().NotBeEmpty();
        result.People.Should().HaveCountGreaterThanOrEqualTo(17);
        VerifyPersonInfo7987(result.People.FirstOrDefault(p => "Тим Роббинс".Equals(p.Name, StringComparison.Ordinal)));

        _logManager.Verify(lm => lm.GetLogger(It.IsAny<string>()), Times.Exactly(4));
        _applicationPaths.VerifyGet(ap => ap.PluginConfigurationsPath, Times.Once());
        _xmlSerializer.Verify(xs => xs.DeserializeFromFile(typeof(PluginConfiguration), "KpMovieProvider_GetMetadata_Provider_Kp/EmbyKinopoiskRu.xml"), Times.Once());
        _localizationManager.Verify(lm => lm.RemoveDiacritics("Побег из Шоушенка"), Times.Once());
        VerifyNoOtherCalls();

        Logger.Info($"Finished '{nameof(KpMovieProvider_GetMetadata_Provider_Kp)}'");
    }

    [Fact]
    public async void KpMovieProvider_GetMetadata_Provider_Imdb()
    {
        Logger.Info($"Start '{nameof(KpMovieProvider_GetMetadata_Provider_Imdb)}'");

        var movieInfo = new MovieInfo
        {
            ProviderIds = new ProviderIdDictionary(new Dictionary<string, string>
            {
                { MetadataProviders.Imdb.ToString(), "tt0111161" }
            })
        };

        _ = _applicationPaths
            .SetupGet(m => m.PluginConfigurationsPath)
            .Returns("KpMovieProvider_GetMetadata_Provider_Imdb");

        using var cancellationTokenSource = new CancellationTokenSource();
        MetadataResult<Movie> result = await _kpMovieProvider.GetMetadata(movieInfo, cancellationTokenSource.Token);

        result.HasMetadata.Should().BeTrue();
        VerifyMovie326(result.Item);

        result.People.Should().NotBeEmpty();
        result.People.Should().HaveCountGreaterThanOrEqualTo(17);
        VerifyPersonInfo7987(result.People.FirstOrDefault(p => "Тим Роббинс".Equals(p.Name, StringComparison.Ordinal)));

        _logManager.Verify(lm => lm.GetLogger(It.IsAny<string>()), Times.Exactly(4));
        _applicationPaths.VerifyGet(ap => ap.PluginConfigurationsPath, Times.Once());
        _xmlSerializer.Verify(xs => xs.DeserializeFromFile(typeof(PluginConfiguration), "KpMovieProvider_GetMetadata_Provider_Imdb/EmbyKinopoiskRu.xml"), Times.Once());
        _localizationManager.Verify(lm => lm.RemoveDiacritics("Побег из Шоушенка"), Times.Once());
        VerifyNoOtherCalls();

        Logger.Info($"Finished '{nameof(KpMovieProvider_GetMetadata_Provider_Imdb)}'");
    }

    [Fact]
    public async void KpMovieProvider_GetMetadata_Provider_Tmdb()
    {
        Logger.Info($"Start '{nameof(KpMovieProvider_GetMetadata_Provider_Tmdb)}'");

        var movieInfo = new MovieInfo
        {
            ProviderIds = new ProviderIdDictionary(new Dictionary<string, string>
            {
                { MetadataProviders.Tmdb.ToString(), "278" }
            })
        };

        _ = _applicationPaths
            .SetupGet(m => m.PluginConfigurationsPath)
            .Returns("KpMovieProvider_GetMetadata_Provider_Tmdb");

        using var cancellationTokenSource = new CancellationTokenSource();
        MetadataResult<Movie> result = await _kpMovieProvider.GetMetadata(movieInfo, cancellationTokenSource.Token);

        result.HasMetadata.Should().BeTrue();
        VerifyMovie326(result.Item);

        result.People.Should().NotBeEmpty();
        result.People.Should().HaveCountGreaterThanOrEqualTo(17);
        VerifyPersonInfo7987(result.People.FirstOrDefault(p => "Тим Роббинс".Equals(p.Name, StringComparison.Ordinal)));

        _logManager.Verify(lm => lm.GetLogger(It.IsAny<string>()), Times.Exactly(4));
        _applicationPaths.VerifyGet(ap => ap.PluginConfigurationsPath, Times.Once());
        _xmlSerializer.Verify(xs => xs.DeserializeFromFile(typeof(PluginConfiguration), "KpMovieProvider_GetMetadata_Provider_Tmdb/EmbyKinopoiskRu.xml"), Times.Once());
        _localizationManager.Verify(lm => lm.RemoveDiacritics("Побег из Шоушенка"), Times.Once());
        VerifyNoOtherCalls();

        Logger.Info($"Finished '{nameof(KpMovieProvider_GetMetadata_Provider_Tmdb)}'");
    }

    [Fact]
    public async void KpMovieProvider_GetMetadata_NameAndYear()
    {
        Logger.Info($"Start '{nameof(KpMovieProvider_GetMetadata_NameAndYear)}'");

        var movieInfo = new MovieInfo
        {
            Name = "Побег из Шоушенка",
            Year = 1994
        };

        _ = _applicationPaths
            .SetupGet(m => m.PluginConfigurationsPath)
            .Returns("KpMovieProvider_GetMetadata_NameAndYear");

        using var cancellationTokenSource = new CancellationTokenSource();
        MetadataResult<Movie> result = await _kpMovieProvider.GetMetadata(movieInfo, cancellationTokenSource.Token);

        result.HasMetadata.Should().BeTrue();
        VerifyMovie326(result.Item);

        result.People.Should().NotBeEmpty();
        result.People.Should().HaveCountGreaterThanOrEqualTo(17);
        VerifyPersonInfo7987(result.People.FirstOrDefault(p => "Тим Роббинс".Equals(p.Name, StringComparison.Ordinal)));

        _logManager.Verify(lm => lm.GetLogger(It.IsAny<string>()), Times.Exactly(4));
        _applicationPaths.VerifyGet(ap => ap.PluginConfigurationsPath, Times.Once());
        _xmlSerializer.Verify(xs => xs.DeserializeFromFile(typeof(PluginConfiguration), "KpMovieProvider_GetMetadata_NameAndYear/EmbyKinopoiskRu.xml"), Times.Once());
        _localizationManager.Verify(lm => lm.RemoveDiacritics("Побег из Шоушенка"), Times.Once());
        VerifyNoOtherCalls();

        Logger.Info($"Finished '{nameof(KpMovieProvider_GetMetadata_NameAndYear)}'");
    }

    [Fact]
    public async void KpMovieProvider_GetSearchResults_Provider_Kp()
    {
        Logger.Info($"Start '{nameof(KpMovieProvider_GetSearchResults_Provider_Kp)}'");

        _ = _applicationPaths
            .SetupGet(m => m.PluginConfigurationsPath)
            .Returns("KpMovieProvider_GetSearchResults_Provider_Kp");

        var movieInfo = new MovieInfo
        {
            ProviderIds = new ProviderIdDictionary(new Dictionary<string, string>
            {
                { Plugin.PluginKey, "326" }
            })
        };
        using var cancellationTokenSource = new CancellationTokenSource();
        IEnumerable<RemoteSearchResult> result = await _kpMovieProvider.GetSearchResults(movieInfo, cancellationTokenSource.Token);
        result.Should().ContainSingle();
        VerifyRemoteSearchResult326(result.First());

        _logManager.Verify(lm => lm.GetLogger(It.IsAny<string>()), Times.Exactly(4));
        _applicationPaths.VerifyGet(ap => ap.PluginConfigurationsPath, Times.Once());
        _xmlSerializer.Verify(xs => xs.DeserializeFromFile(typeof(PluginConfiguration), "KpMovieProvider_GetSearchResults_Provider_Kp/EmbyKinopoiskRu.xml"), Times.Once());
        VerifyNoOtherCalls();

        Logger.Info($"Finished '{nameof(KpMovieProvider_GetSearchResults_Provider_Kp)}'");
    }

    [Fact]
    public async void KpMovieProvider_GetSearchResults_Provider_Imdb()
    {
        Logger.Info($"Start '{nameof(KpMovieProvider_GetSearchResults_Provider_Imdb)}'");

        _ = _applicationPaths
            .SetupGet(m => m.PluginConfigurationsPath)
            .Returns("KpMovieProvider_GetSearchResults_Provider_Imdb");

        var movieInfo = new MovieInfo
        {
            ProviderIds = new ProviderIdDictionary(new Dictionary<string, string>
            {
                { MetadataProviders.Imdb.ToString(), "tt0111161" }
            })
        };
        using var cancellationTokenSource = new CancellationTokenSource();
        IEnumerable<RemoteSearchResult> result = await _kpMovieProvider.GetSearchResults(movieInfo, cancellationTokenSource.Token);
        result.Should().ContainSingle();
        VerifyRemoteSearchResult326(result.First());

        _logManager.Verify(lm => lm.GetLogger(It.IsAny<string>()), Times.Exactly(4));
        _applicationPaths.VerifyGet(ap => ap.PluginConfigurationsPath, Times.Once());
        _xmlSerializer.Verify(xs => xs.DeserializeFromFile(typeof(PluginConfiguration), "KpMovieProvider_GetSearchResults_Provider_Imdb/EmbyKinopoiskRu.xml"), Times.Once());
        VerifyNoOtherCalls();

        Logger.Info($"Finished '{nameof(KpMovieProvider_GetSearchResults_Provider_Imdb)}'");
    }

    [Fact]
    public async void KpMovieProvider_GetSearchResults_Name()
    {
        Logger.Info($"Start '{nameof(KpMovieProvider_GetSearchResults_Name)}'");

        _ = _applicationPaths
            .SetupGet(m => m.PluginConfigurationsPath)
            .Returns("KpMovieProvider_GetSearchResults_Name");

        var movieInfo = new MovieInfo
        {
            Name = "Побег из Шоушенка",
            Year = 1994
        };
        using var cancellationTokenSource = new CancellationTokenSource();
        IEnumerable<RemoteSearchResult> result = await _kpMovieProvider.GetSearchResults(movieInfo, cancellationTokenSource.Token);
        result.Should().HaveCount(20);
        VerifyRemoteSearchResult326(result.First(x => "Побег из Шоушенка".Equals(x.Name, StringComparison.Ordinal)));

        _logManager.Verify(lm => lm.GetLogger(It.IsAny<string>()), Times.Exactly(4));
        _applicationPaths.VerifyGet(ap => ap.PluginConfigurationsPath, Times.Once());
        _xmlSerializer.Verify(xs => xs.DeserializeFromFile(typeof(PluginConfiguration), "KpMovieProvider_GetSearchResults_Name/EmbyKinopoiskRu.xml"), Times.Once());
        VerifyNoOtherCalls();

        Logger.Info($"Finished '{nameof(KpMovieProvider_GetSearchResults_Name)}'");
    }

    [Fact]
    public async void KpMovieProvider_GetSearchResults_NameAndYear()
    {
        Logger.Info($"Start '{nameof(KpMovieProvider_GetSearchResults_NameAndYear)}'");

        _ = _applicationPaths
            .SetupGet(m => m.PluginConfigurationsPath)
            .Returns("KpMovieProvider_GetSearchResults_NameAndYear");

        var movieInfo = new MovieInfo
        {
            Name = "Побег из Шоушенка",
            Year = 1994
        };
        using var cancellationTokenSource = new CancellationTokenSource();
        IEnumerable<RemoteSearchResult> result = await _kpMovieProvider.GetSearchResults(movieInfo, cancellationTokenSource.Token);
        result.Should().HaveCount(20);
        VerifyRemoteSearchResult326(result.First(x => "Побег из Шоушенка".Equals(x.Name, StringComparison.Ordinal)));

        _logManager.Verify(lm => lm.GetLogger(It.IsAny<string>()), Times.Exactly(4));
        _applicationPaths.VerifyGet(ap => ap.PluginConfigurationsPath, Times.Once());
        _xmlSerializer.Verify(xs => xs.DeserializeFromFile(typeof(PluginConfiguration), "KpMovieProvider_GetSearchResults_NameAndYear/EmbyKinopoiskRu.xml"), Times.Once());
        VerifyNoOtherCalls();

        Logger.Info($"Finished '{nameof(KpMovieProvider_GetSearchResults_NameAndYear)}'");
    }

    [Fact]
    public async void KpMovieProvider_WithNameYearAndAddToExistingCollection()
    {
        Logger.Info($"Start '{nameof(KpMovieProvider_WithNameYearAndAddToExistingCollection)}'");

        #region KpMovieProvider_WithNameYearAndAddToExistingCollection config

        _ = _libraryManager
            .Setup(m => m.GetInternalItemIds(It.Is<InternalItemsQuery>(q => Equals(true, q.IsFolder))))
            .Returns(new[] { 1L });

        // EmbyHelper.FindCollectionFolders
        _ = _libraryManager
            .Setup(m => m.QueryItems(It.Is<InternalItemsQuery>(query =>
                "Collections".Equals(query.Name, StringComparison.Ordinal)
                && true.Equals(query.Recursive)
                && true.Equals(query.IsFolder)
                && query.IncludeItemTypes.Length == 1
                && "CollectionFolder".Equals(query.IncludeItemTypes[0], StringComparison.Ordinal))
            ))
            .Returns(new QueryResult<BaseItem>
            {
                TotalRecordCount = 0,
                Items = new BaseItem[]
                {
                    new CollectionFolder
                    {
                        Name = "Collections",
                        Path = "/CreateKpCollectionsTaskTest_Execute_CollectionNotExists",
                        InternalId = 2L
                    }
                }
            });

        // EmbyHelper.GetItemsByProviderIds
        _ = _libraryManager
            .Setup(m => m.QueryItems(It.Is<InternalItemsQuery>(query =>
                true.Equals(query.Recursive)
                && false.Equals(query.IsVirtualItem)
                && true.Equals(query.HasPath)
                && query.IncludeItemTypes.Length == 2
                && "Movie".Equals(query.IncludeItemTypes[0], StringComparison.Ordinal)
                && "Series".Equals(query.IncludeItemTypes[1], StringComparison.Ordinal)
                && query.AnyProviderIdEquals.Any())
            ))
            .Returns(new QueryResult<BaseItem>
            {
                Items = Array.Empty<BaseItem>()
            });

        // EmbyHelper.GetItemsByProviderIds
        _ = _libraryManager
            .Setup(m => m.QueryItems(It.Is<InternalItemsQuery>(query =>
                true.Equals(query.Recursive)
                && false.Equals(query.IsVirtualItem)
                && query.IncludeItemTypes.Length == 2
                && "Movie".Equals(query.IncludeItemTypes[0], StringComparison.Ordinal)
                && "Series".Equals(query.IncludeItemTypes[1], StringComparison.Ordinal)
                && query.AnyProviderIdEquals.Contains(new KeyValuePair<string, string>(Plugin.PluginKey, "688"))
            )))
            .Returns(new QueryResult<BaseItem>
            {
                Items = new BaseItem[]
                {
                    new Movie
                    {
                        Name = "Гарри Поттер и Тайная комната",
                        InternalId = 103L,
                        Path = "/tmp/103",
                        ProviderIds = new ProviderIdDictionary(new Dictionary<string, string>
                        {
                            { Plugin.PluginKey, "688" }
                        })
                    }
                }
            });

        // EmbyHelper.GetCollectionByName
        _ = _libraryManager
            .Setup(m => m.QueryItems(It.Is<InternalItemsQuery>(query =>
                "Гарри Поттер и философский камень".Equals(query.Name, StringComparison.Ordinal)
                && true.Equals(query.Recursive)
                && query.IncludeItemTypes.Length == 1
                && "BoxSet".Equals(query.IncludeItemTypes[0], StringComparison.Ordinal))
            ))
            .Returns(new QueryResult<BaseItem>
            {
                TotalRecordCount = 1,
                Items = new BaseItem[]
                {
                    new BoxSet
                    {
                        Name = "Гарри Поттер и философский камень",
                        Path = "/emby/harry_potter_exists",
                        InternalId = 567L
                    }
                }
            });

        _ = _xmlSerializer
            .Setup(m => m.DeserializeFromFile(typeof(LibraryOptions), "KpMovieProvider_WithNameYearAndAddToExistingCollection/options.xml"))
            .Returns(new LibraryOptions
            {
                ContentType = CollectionType.BoxSets.ToString(),
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
                PreferredMetadataLanguage = "ru"
            });

        _ = _applicationPaths
            .SetupGet(m => m.PluginConfigurationsPath)
            .Returns("KpMovieProvider_WithNameYearAndAddToExistingCollection");

        _ = _libraryManager
            .Setup(m => m.GetItemById(It.Is<long>(id => id == 1L)))
            .Returns(new CollectionFolder
            {
                Name = "Collections",
                Path = "KpMovieProvider_WithNameYearAndAddToExistingCollection"
            });

        var movieInfo = new MovieInfo();
        movieInfo.SetProviderId(Plugin.PluginKey, "689");
        using var cancellationTokenSource = new CancellationTokenSource();
        _pluginConfiguration.CreateSeqCollections = true;

        #endregion

        MetadataResult<Movie> result = await _kpMovieProvider.GetMetadata(movieInfo, cancellationTokenSource.Token);

        result.HasMetadata.Should().BeTrue();
        VerifyMovie689(result.Item);

        result.People.Should().NotBeEmpty();
        result.People.Should().HaveCountGreaterThanOrEqualTo(18);
        VerifyPersonInfo40779(result.People.FirstOrDefault(p => "Эмма Уотсон".Equals(p.Name, StringComparison.Ordinal)));

        // check that no errors in API
        _activityManager.Verify(a => a.Create(It.IsAny<ActivityLogEntry>()), Times.Never());
        _notificationManager.Verify(n => n.SendNotification(It.IsAny<NotificationRequest>()), Times.Never());

        _applicationPaths.VerifyGet(ap => ap.PluginConfigurationsPath, Times.Once());
        _libraryManager.Verify(lm => lm.QueryItems(It.IsAny<InternalItemsQuery>()), Times.Exactly(3));
        _libraryManager.Verify(lm => lm.GetItemLinks(103L, It.IsAny<List<ItemLinkType>>()), Times.Once());
        _libraryManager.Verify(lm => lm.UpdateItem(It.IsAny<BaseItem>(), It.IsAny<BaseItem>(), ItemUpdateType.MetadataEdit, null), Times.Once());
        _localizationManager.Verify(lm => lm.RemoveDiacritics("Гарри Поттер и философский камень"), Times.Once());
        _logManager.Verify(lm => lm.GetLogger(It.IsAny<string>()), Times.Exactly(4));
        _xmlSerializer.Verify(xs => xs.DeserializeFromFile(typeof(PluginConfiguration), "KpMovieProvider_WithNameYearAndAddToExistingCollection/EmbyKinopoiskRu.xml"), Times.Once());

        VerifyNoOtherCalls();

        Logger.Info($"Finished '{nameof(KpMovieProvider_WithNameYearAndAddToExistingCollection)}'");
    }

    // Will create a Collections virtual folder
    [Fact]
    public async void KpMovieProvider_WithNameYearAndAddToNewCollection()
    {
        Logger.Info($"Start '{nameof(KpMovieProvider_WithNameYearAndAddToNewCollection)}'");

        #region KpMovieProvider_WithNameYearAndAddToNewCollection config

        _ = _libraryManager
            .SetupSequence(m => m.GetInternalItemIds(It.Is<InternalItemsQuery>(q => Equals(true, q.IsFolder))))
            .Returns(Array.Empty<long>())
            .Returns(new[] { 1L });

        _ = _collectionManager
            .Setup(m => m.CreateCollection(It.IsAny<CollectionCreationOptions>()))
            .Returns((CollectionCreationOptions options) => Task.FromResult(new BoxSet
            {
                Name = options.Name,
                ParentId = options.ParentId,
                InternalId = 201L
            }));

        // EmbyHelper.FindCollectionFolders
        _ = _libraryManager
            .Setup(m => m.QueryItems(It.Is<InternalItemsQuery>(query =>
                "Collections".Equals(query.Name, StringComparison.Ordinal)
                && true.Equals(query.Recursive)
                && true.Equals(query.IsFolder)
                && query.IncludeItemTypes.Length == 1
                && "CollectionFolder".Equals(query.IncludeItemTypes[0], StringComparison.Ordinal))
            ))
            .Returns(new QueryResult<BaseItem>
            {
                TotalRecordCount = 0,
                Items = new BaseItem[]
                {
                    new CollectionFolder
                    {
                        Name = "Collections",
                        Path = "/CreateKpCollectionsTaskTest_Execute_CollectionNotExists",
                        InternalId = 2L
                    }
                }
            });

        // EmbyHelper.GetItemsByProviderIds
        _ = _libraryManager
            .Setup(m => m.QueryItems(It.Is<InternalItemsQuery>(query =>
                true.Equals(query.Recursive)
                && false.Equals(query.IsVirtualItem)
                && true.Equals(query.HasPath)
                && query.IncludeItemTypes.Length == 2
                && "Movie".Equals(query.IncludeItemTypes[0], StringComparison.Ordinal)
                && "Series".Equals(query.IncludeItemTypes[1], StringComparison.Ordinal)
                && query.AnyProviderIdEquals.Any())
            ))
            .Returns(new QueryResult<BaseItem>
            {
                Items = Array.Empty<BaseItem>()
            });

        // EmbyHelper.GetItemsByProviderIds
        _ = _libraryManager
            .Setup(m => m.QueryItems(It.Is<InternalItemsQuery>(query =>
                true.Equals(query.Recursive)
                && false.Equals(query.IsVirtualItem)
                && query.IncludeItemTypes.Length == 2
                && "Movie".Equals(query.IncludeItemTypes[0], StringComparison.Ordinal)
                && "Series".Equals(query.IncludeItemTypes[1], StringComparison.Ordinal)
                && query.AnyProviderIdEquals.Contains(new KeyValuePair<string, string>(Plugin.PluginKey, "688"))
            )))
            .Returns(new QueryResult<BaseItem>
            {
                Items = new BaseItem[]
                {
                    new Movie
                    {
                        Name = "Гарри Поттер и Тайная комната",
                        InternalId = 103L,
                        Path = "/tmp/103",
                        ProviderIds = new ProviderIdDictionary(new Dictionary<string, string>
                        {
                            { Plugin.PluginKey, "688" }
                        })
                    }
                }
            });

        // EmbyHelper.GetCollectionByName
        _ = _libraryManager
            .Setup(m => m.QueryItems(It.Is<InternalItemsQuery>(query =>
                "Гарри Поттер и философский камень".Equals(query.Name, StringComparison.Ordinal)
                && true.Equals(query.Recursive)
                && query.IncludeItemTypes.Length == 1
                && "BoxSet".Equals(query.IncludeItemTypes[0], StringComparison.Ordinal))
            ))
            .Returns(new QueryResult<BaseItem>
            {
                TotalRecordCount = 0,
                Items = Array.Empty<BaseItem>()
            });

        _ = _xmlSerializer
            .Setup(m => m.DeserializeFromFile(typeof(LibraryOptions), "KpMovieProvider_WithNameYearAndAddToNewCollection/options.xml"))
            .Returns(new LibraryOptions
            {
                ContentType = CollectionType.BoxSets.ToString(),
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
                PreferredMetadataLanguage = "ru"
            });

        _ = _applicationPaths
            .SetupGet(m => m.PluginConfigurationsPath)
            .Returns("KpMovieProvider_WithNameYearAndAddToNewCollection");

        _ = _libraryManager
            .Setup(m => m.GetItemById(It.Is<long>(id => id == 1L)))
            .Returns(new CollectionFolder
            {
                Name = "Collections",
                Path = "KpMovieProvider_WithNameYearAndAddToNewCollection"
            });

        var movieInfo = new MovieInfo();
        movieInfo.SetProviderId(Plugin.PluginKey, "689");
        using var cancellationTokenSource = new CancellationTokenSource();
        _pluginConfiguration.CreateSeqCollections = true;

        #endregion

        MetadataResult<Movie> result = await _kpMovieProvider.GetMetadata(movieInfo, cancellationTokenSource.Token);

        result.HasMetadata.Should().BeTrue();
        VerifyMovie689(result.Item);

        result.People.Should().NotBeEmpty();
        result.People.Should().HaveCountGreaterThanOrEqualTo(18);
        VerifyPersonInfo40779(result.People.FirstOrDefault(p => "Эмма Уотсон".Equals(p.Name, StringComparison.Ordinal)));

        // check that no errors in API
        _activityManager.Verify(a => a.Create(It.IsAny<ActivityLogEntry>()), Times.Never());
        _notificationManager.Verify(n => n.SendNotification(It.IsAny<NotificationRequest>()), Times.Never());

        _applicationPaths.VerifyGet(ap => ap.PluginConfigurationsPath, Times.Once());
        _collectionManager.Verify(cm => cm.CreateCollection(It.IsAny<CollectionCreationOptions>()), Times.Once());
        _libraryManager.Verify(lm => lm.QueryItems(It.IsAny<InternalItemsQuery>()), Times.Exactly(3));
        _localizationManager.Verify(lm => lm.RemoveDiacritics("Гарри Поттер и философский камень"), Times.Once());
        _logManager.Verify(lm => lm.GetLogger(It.IsAny<string>()), Times.Exactly(4));
        _xmlSerializer.Verify(xs => xs.DeserializeFromFile(typeof(PluginConfiguration), "KpMovieProvider_WithNameYearAndAddToNewCollection/EmbyKinopoiskRu.xml"), Times.Once());

        VerifyNoOtherCalls();

        Logger.Info($"Finished '{nameof(KpMovieProvider_WithNameYearAndAddToNewCollection)}'");
    }
}
