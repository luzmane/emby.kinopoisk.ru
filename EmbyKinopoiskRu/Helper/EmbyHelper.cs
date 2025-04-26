using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Emby.Resources.App;

using EmbyKinopoiskRu.ScheduledTasks.Model;

using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Extensions;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Querying;
using MediaBrowser.Model.Serialization;

namespace EmbyKinopoiskRu.Helper
{
    internal static class EmbyHelper
    {
        private static readonly object Locker = new Object();

        /// <summary>
        /// Search "Collections" folder
        /// </summary>
        /// <param name="libraryManager"></param>
        /// <returns></returns>
        private static IEnumerable<CollectionFolder> FindCollectionFolders(ILibraryManager libraryManager)
        {
            return libraryManager.QueryItems(new InternalItemsQuery
                {
                    IncludeItemTypes = new[] { nameof(CollectionFolder) },
                    Recursive = true,
                    IsFolder = true,
                    Name = AppResources.ResourceManager.GetString("Collections")
                })
                .Items
                .Cast<CollectionFolder>()
                .Where(folder => "boxsets".EqualsIgnoreCase(folder.CollectionType));
        }

        /// <summary>
        /// Check or create "Collections" virtual folder
        /// </summary>
        /// <param name="libraryManager"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        internal static CollectionFolder InsureCollectionLibraryFolderAsync(ILibraryManager libraryManager, ILogger logger)
        {
            IEnumerable<CollectionFolder> folders = FindCollectionFolders(libraryManager);
            if (folders.Any())
            {
                return folders.FirstOrDefault();
            }

            var collectionsName = AppResources.ResourceManager.GetString("Collections");
            logger.Info($"Creating 'Collections' virtual folder with name: '{collectionsName}'");
            var options = new LibraryOptions
            {
                EnableRealtimeMonitor = false,
                SaveLocalMetadata = true,
                ContentType = CollectionType.BoxSets.ToString()
            };
            libraryManager.AddVirtualFolder(collectionsName, options, true);
            return FindCollectionFolders(libraryManager).FirstOrDefault();
        }

        /// <summary>
        /// Search for items based on internal IDs
        /// </summary>
        /// <param name="providerIds"></param>
        /// <param name="libraryManager"></param>
        /// <returns></returns>
        internal static BaseItem[] GetItemsByProviderIds(IEnumerable<KeyValuePair<string, string>> providerIds, ILibraryManager libraryManager)
        {
            return providerIds.Any()
                ? libraryManager.QueryItems(new InternalItemsQuery
                    {
                        IncludeItemTypes = new[] { nameof(Movie), nameof(Series) },
                        Recursive = true,
                        IsVirtualItem = false,
                        AnyProviderIdEquals = providerIds.ToArray()
                    })
                    .Items
                : Array.Empty<BaseItem>();
        }

        #region Task Translations

        internal static Dictionary<string, string> GetAvailableTranslations(string key)
        {
            var basePath = Plugin.Instance.GetType().Namespace + $".i18n.{key}.";
            return typeof(EmbyHelper).Assembly.GetManifestResourceNames()
                .Where(i => i.StartsWith(basePath, StringComparison.OrdinalIgnoreCase))
                .Select(i =>
                    new TranslationInfo
                    {
                        Locale = Path.GetFileNameWithoutExtension(i.Substring(basePath.Length)),
                        EmbeddedResourcePath = i
                    })
                .ToDictionary(i => i.Locale, j => j.EmbeddedResourcePath);
        }

        internal static TaskTranslation GetTaskTranslation(
            Dictionary<string, TaskTranslation> translations,
            IServerConfigurationManager serverConfigurationManager,
            IJsonSerializer jsonSerializer,
            Dictionary<string, string> availableTranslations)
        {
            if (translations.TryGetValue(serverConfigurationManager.Configuration.UICulture, out TaskTranslation translation))
            {
                return translation;
            }

            lock (Locker)
            {
                if (!translations.TryGetValue(serverConfigurationManager.Configuration.UICulture, out TaskTranslation tmp))
                {
                    if (!availableTranslations.TryGetValue(serverConfigurationManager.Configuration.UICulture, out var resourcePath))
                    {
                        resourcePath = availableTranslations["en-US"];
                    }

                    using (Stream stream = typeof(EmbyHelper).Assembly.GetManifestResourceStream(resourcePath))
                    {
                        translation = jsonSerializer.DeserializeFromStream<TaskTranslation>(stream);
                    }

                    if (translation != null)
                    {
                        translations.Add(serverConfigurationManager.Configuration.UICulture, translation);
                    }
                }
                else
                {
                    translation = tmp;
                }
            }

            return translation;
        }

        #endregion

        /// <summary>
        /// Get library objects of types Movie and Series that have IMDB or TMDB IDs, but have no Kinopoisk IDs
        /// </summary>
        /// <param name="libraryManager"></param>
        /// <returns></returns>
        internal static BaseItem[] GetVideoWithoutKpWithImdbTmdb(ILibraryManager libraryManager)
        {
            return libraryManager.QueryItems(new InternalItemsQuery
                {
                    IncludeItemTypes = new[] { nameof(Movie), nameof(Series) },
                    MissingAnyProviderId = new[] { Plugin.PluginKey },
                    Recursive = true,
                    IsVirtualItem = false,
                    HasAnyProviderId = new[] { MetadataProviders.Imdb.ToString(), MetadataProviders.Tmdb.ToString() }
                })
                .Items;
        }

        /// <summary>
        /// Get movie, series items by provider ids and library ids splitter by pages
        /// </summary>
        /// <param name="libraryManager"></param>
        /// <param name="providerIdList"></param>
        /// <param name="librariesIds"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        internal static BaseItem[] GetPagedMovieSeriesByProviderIdsAndLibraryIds(
            ILibraryManager libraryManager,
            IEnumerable<KeyValuePair<string, string>> providerIdList,
            long[] librariesIds,
            int page,
            int pageSize)
        {
            return libraryManager.GetItemList(new InternalItemsQuery
                {
                    IncludeItemTypes = new[] { nameof(Movie), nameof(Series) },
                    AnyProviderIdEquals = providerIdList.Skip(page * pageSize).Take(pageSize).ToList(),
                    Recursive = true,
                    IsVirtualItem = false,
                    ParentIds = librariesIds,
                    HasPath = true
                });
        }

        /// <summary>
        /// Get collection by name
        /// </summary>
        /// <param name="libraryManager"></param>
        /// <param name="collectionName"></param>
        /// <returns></returns>
        internal static QueryResult<BaseItem> GetCollectionByName(ILibraryManager libraryManager, string collectionName)
        {
            return libraryManager.QueryItems(new InternalItemsQuery
            {
                IncludeItemTypes = new[] { nameof(BoxSet) },
                Recursive = true,
                Name = collectionName
            });
        }

        /// <summary>
        /// Get all libraries
        /// </summary>
        /// <param name="libraryManager"></param>
        /// <returns></returns>
        internal static IEnumerable<CollectionFolder> GetLibraries(ILibraryManager libraryManager)
        {
            return libraryManager.GetItemList(new InternalItemsQuery
                {
                    IncludeItemTypes = new[] { nameof(CollectionFolder) },
                    IsFolder = true,
                    IsVirtualItem = false
                })
                .Cast<CollectionFolder>();
        }
    }
}
