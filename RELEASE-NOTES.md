v.1.34.0
- change the API url - kinopoisk.dev was changed to poiskkino.dev

v.1.33.0
- improvement: surround sort name of the collections created by the "Create Kinopoisk Collections" task with double quotes, so they will appear at the top of the Collections folder

v.1.32.1
- fix: restore 'Identify' action

v.1.32.0
- fix: restore 'Identify' action

v.1.31.0
- improvement: add kinopoisk url in the Identify pop-up

v.1.30.0
- fix: "Collections" folder creation ignored language configuration
- improvement: moved trailer downloader to a separate project emby.kinopoisk.trailers

v.1.29.0
- improvement: leave user notification only for invalid token and limit exceed cases

v.1.28.0
- improvement: add additional Kp trailer URL kind to download during Intro update

v.1.27.0
- improvement: filter out videos that are not trailers during internet channel update flow. Logic - min duration, keywords in the name of a trailer

v.1.26.0
- feature: support download intro (trailer) from Kinopoisk
- feature: support trailers info from kinopoiskapiunofficial.tech
- feature: add filter of maximum duration of an intro

v.1.25.2
- fix: radio button didn't switch the API

v.1.25.1
- fix: set default values for IntroPath and UserAgentAPI config parameters

v.1.25.0
- feature: add Kinopoisk ID to trailers (same as for movie)
- feature: add Kinopoisk trailers channel
- feature: add Kinopoisk Intro provider (used in CinemaMode)
- feature: add a task to remove redundant downloaded trailers

v.1.24.1
- fix: issue#12 - unable to update the plugin

v.1.24.0
- feature: add task "KinopoiskId by IMDB" for kinopoiskapiunofficial.tech

v.1.23.1
- fix: incorrect description for Create Kinopoisk Collections Task

v.1.23.0
- feature: add notification, in case any problems with token (API access)
- improvement: take into account a type of video, filtering the API search response
- feature: create KP collections for kinopoiskapiunofficial.tech API
- refactoring
- small fixes

v.1.22.0
- feature: Update supported Emby version to 4.8.2
- feature: Add number of items in each collection

v.1.21.0
- feature: Move to v.1.4 KinopoiskDev API.
- feature: Remove creation of TOP 250 Kinopoisk collection.
- feature: Add possibility to choose and create all Kinopoisk collections.
- change: Remove option to create collections based on sequels data from kinopoisk.dev - sometimes the sequel collection incorrectly detected and video added to the wrong collection.

v.1.20.0
- Update "Update Kinopoisk Plugin" task to support new tags name

v.1.19.0
- Unite Top 250 tasks - KinopoiskDev supply correct data with 250 items total

v.1.18.1
- Fix top250 collections name was not saved and flag was inverted

v.1.18.0
- Add option to create a single Top250 movie/series collection for all libraries

v.1.17.0
- Remove CriticRating as not relevant for Kinopoisk
- KinopoiskApiUnOfficial. Remove video name normalization as breaking search
- KinopoiskApiUnOfficial. Add search by IMDB
- Improve filtering video by relevant data

v.1.16.1
- Fix Movie's Facts overlapping in Safari browser

v.1.16.0
- Remove notifications, since interface in 4.7 (stable) and 4.8 (beta) is not compatible

v.1.15.0
- Fix person search using kinopoisk.dev
- Add notification in case of token issues

v.1.14.2
- Fix "Plugin update task": instead of updating the plugin installed itself

v.1.14.1
- Fix in "Add KinopoiskId based on IMDB, TMDB" series search

v.1.14.0
- Fix KeyDuplication exception during Kp ID search based on IMDB, TMDB
- Normalize name before search
- Update Kinopoisk.dev to use API 1.3
- Add search by other providers before search by name and year

v.1.13.0
- Add localization
- Fix items search in task "Add KinopoiskId based on IMDB, TMDB"

v.1.12.0
- Improve film filtering

v.1.11.0
- Support .NetStandard 2.0 in addition to .NetCore 6.0 (possibility to run on Android devices)
- Fix kinopoiskapiunofficial.tech populated only partial information for films/series

v.1.10.0
- Add Update Plugin task which will update the DLL of the plugin from GitHub

v.1.9.1
- Tune movie/series search - filter API search result with name and year

v.1.9.0
- Search for kinopoiskapiunofficial.tech include year
- Change default API

v.1.8.2
- Fix incorrect items querying

v.1.8.1
- Suit plugin to be used with 4.8.*-beta versions

v.1.8.0
- Append facts to movie/series description
- New created by plugin collection will have name of the first video in sequels
- Fix Create Top250 collections task: fix incorrect item detection
- Fix Update Kp ID by IMDB/TMDB id task: fix incorrect item detection

v.1.7.0
- During metadata update if found several movies will choose the one with the highest rating
- Add search by English name of the person
- Support of the new API version of kinopoisk.dev
- Added option to create collections based on sequels data from kinopoisk.dev. Configured by the checkbox in plugin setup page
- Translated plugin config
- Added scheduled task to create Kinopoisk Top 250 collection
- Added scheduled task to update movie/series with Kinopoisk ID based on IMDB or TMDB

v.1.6.2
- Fix trailers, add latest trailer from API to be the first, suppose it will be on Russian

v.1.6.1
- Add trailers and teasers to movie search by name result

v.1.6.0
- Update dependency
- Add movie name detection from file name

v.1.5.0
- Add default token for KinopoiskDev API

v.1.4.0
- Change default sort name to be taken from Russian and not the original title
- Add trailers from YouTube only - emby can play trailers only from YouTube

v.1.3.0
- Update plugin due to changes in KinopoiskDev API

v.1.2.0
- Add entry to activity log once an error happens on request to external API

v.1.1.0
- Add detection of КиноПоиск ID in file name
- Bug fixes
- Refactoring
- Add external API tests

v.1.0.0
- Initial release
- Support data load of movie, series and staff
