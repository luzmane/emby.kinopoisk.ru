# emby.kinopoisk.ru

Fetches metadata from [kinopoisk.ru](https://www.kinopoisk.ru). This site is popular in the Russian-speaking community and contains almost no English-language information, so further description will be in Russian.

Плагин для Emby для загрузки метаданных фильмов, сериалов с сайта [kinopoisk.ru](https://www.kinopoisk.ru) используя сторонние API:
- [kinopoiskapiunofficial.tech](https://kinopoiskapiunofficial.tech)
- [kinopoisk.dev](https://kinopoisk.dev)

Если что-то не работает - смело создавай новый issue. Я не пользуюсь плагином 24/7 - могу и не знать о сломавшейся функциональности.

### Благодарность
* Спасибо [svyaznoy362](https://github.com/svyaznoy362) за тестирования версий.
* Спасибо [azharkov78](https://github.com/azharkov78) за тестирования функциональности, связанной с роликами к CinemaMode и каналом.
* Спасибо [Sanchous98](https://github.com/Sanchous98) за тестирования совместимости с Android устройствами.

## Установка

* Положить dll в папку plugins.
* Настроить Медиатеку (Emby Library) использовать новый плагин (поставить галочки).
* (Рекомендуется) Получить личные токены для API.

## Настройка

Параметры плагина искать в "Управление Emby Server" - Расширенное - Плагины - вкладка "Мои плагины (My Plugins)" - КиноПоиск.

## Использование

* Плагин умеет работать с двумя сайтами ([kinopoiskapiunofficial.tech](https://kinopoiskapiunofficial.tech), [kinopoisk.dev](https://kinopoisk.dev)) в настройках можно выбрать откуда получать информацию. По умолчанию запросы идут на [kinopoisk.dev](https://kinopoisk.dev), работая с общим API токеном (спасибо [mdwitr](https://github.com/mdwitr0)). Его хватает на 200 запросов в день - Token быстро заканчивается. Поэтому лучше зарегистрировать свой собственный (и указать в параметрах). Для [kinopoiskapiunofficial.tech](https://kinopoiskapiunofficial.tech) также есть общий токен. Ограничение для него 500 запросов в день - тоже не бесконечный. Поэтому лучше зарегистрировать свой собственный (и указать в параметрах).
* Плагин умеет подхватывать ID КиноПоиска в имени файла по шаблону "<текст>kp<ID КиноПоиска><текст без цифр><текст>" или "<текст>kp-<ID КиноПоиска><текст без цифр><текст>" и использовать его для поиска в базе. Также умеет искать по названию фильма (если сможет название распознать из имени файла).
* Плагин умеет автоматически создавать коллекции фильмов основываясь на данных сиквелов и приквелов КиноПоиска. Функция отключаемая в настройках плагина. Работает только для [kinopoisk.dev](https://kinopoisk.dev) - API предоставляет эти данные.
* Плагин переведён на 3 языка: английский, русский, украинский. Язык зависит от настроек языка отображения.
* Плагин умеет создавать канал, содержащий трейлеры фильмов **выбранных коллекций** КиноПоиска и при помощи CinemaMode (CinemaIntro) демонстрировать их перед фильмом. Для этого:
  * в настройках плагина надо указать куда скачивать трейлеры. Поле "Куда скачивать трейлеры". (обязательно). Путь должен быть локальным, а-ля /mnt/library или c:\\library, даже если фактически эта папка лежит где-то в сети.
  * указать ключ для UserAgent API с сайта [apilayer.com](https://apilayer.com/). Выбрать там https://apilayer.com/marketplace/user_agent-api, бесплатно даёт 10 000 запросов в месяц. Без него тоже будет работать, но шанс, что стороннее API заблокирует запрос выше.
  * указать желаемое качество роликов (480, 720, 1080). По умолчанию 480.
  * выбрать коллекции, трейлеры фильмов которых интересуют.
  * запустить задачу "Обновить интернет каналы" - она и будет скачивать ролики.
  * подождать (займёт время - чтоб сайт по подготовке ссылок не блокировал, я поставил ожидание 5-15 секунд между скачиванием каждого ролика, плюс временное ограничение на скачивание одного ролика в 5 минут (необходимо, если скачивание проходит неудачно)).
  * скачанные ролики будут доступны на главном экране под названием "Трейлеры из коллекций КиноПоиска" (или похожее по смыслу на другом языке).
  * никаких настроек CinemaMode делать не надо - плагин работает на подобии плагина Trailers - поставляет intros в систему.
  * нюансы (как же без них :smile: ):
    * https://apilayer.com доступен из России без VPN
    * для скачивания используются сайты [https://tomp3.cc](https://tomp3.cc) и [https://www.y2mate.com](https://www.y2mate.com)
    * https://tomp3.cc доступен из России без VPN
    * https://www.y2mate.com **НЕ** доступен из России без VPN
    * https://www.y2mate.com иногда требует доказать, что скачивает человек (тестировщик [azharkov78](https://github.com/azharkov78) с этим столкнулся)
    * часть ссылок "битая" - такого ролика на youtube больше нет, канал был удалён и т.п. Плагин будет запоминать и не пытаться скачать ещё раз.
    * часть ссылок сайты конвертации не могут обработать.
    * есть таймауты на скачивание одного ролика - 5 минут.
    * если задача по обновлению каналов отменена, в канале показывается то, что было скачано.

### Доступные плановые задачи

* Создавать коллекции КиноПоиска из фильмов/сериалов в медиатеке (отдельная задача). Чтоб начать использовать эту задачу, в настройках плагина надо выбрать какие коллекции создавать.
* Добавлять ID КиноПоиска по ID IMDB или TMDB (отдельная задача).  Для [kinopoiskapiunofficial.tech](https://kinopoiskapiunofficial.tech) - только по IMDB
* Обновляться самостоятельно (отдельная задача).
* Удалять трейлеры, скачанные с фильмов из коллекций КиноПоиска, которые перестали быть актуальными.

### Загружаемые данные
Поддерживаются:

- Фильмы
- Сериалы
- Актёры

На данный момент грузятся:

- Жанры
- Название
- Оригинальное название (на английском)
- Рейтинги (оценки фильма и рейтинг MPAA)
- Слоган
- Дата выхода фильма
- Описание
- Постеры и задники
- Актёры
- Названия эпизодов
- Дата выхода эпизодов
- Студии
- Трейлеры (только youtube - ограничения emby)
- Факты о фильме/сериале/персоне (встраивается в описание)

## Требования

* Плагин тестировался на версии 4.8.2
* Собирался c .Net 7.0 для .NetStandard 2.0
* Поддерживает Emby соответствующих версий на Android устройствах (спасибо [Sanchous98](https://github.com/Sanchous98))

### TODO
* add tests to trailer functionality
