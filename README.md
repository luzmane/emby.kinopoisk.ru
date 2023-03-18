# emby.kinopoisk.ru

Fetches metadata from [kinopoisk.ru](https://www.kinopoisk.ru). This site is popular in the Russian-speaking community and contains almost no English-language information, so further description will be in Russian.

Плагин для Emby для загрузки метаданных фильмов, сериалов с сайта [kinopoisk.ru](https://www.kinopoisk.ru) использую сторонние API:
- [kinopoiskapiunofficial.tech](https://kinopoiskapiunofficial.tech)
- [kinopoisk.dev](https://kinopoisk.dev)

Если что-то не работает - смело создавай новый issue. Я не пользуюсь плагином 24/7 - могу и не знать о сломавшейся функциональности.

## Установка

* Положить dll в папку plugins
* Настроить Медиатеку (Emby Library) использовать новый плагин (поставить галочки)

## Настройка

Параметры плагина искать в: Администрирование - Панель - Расширенное - Плагины - вкладка "Мои плагины" - KinopoiskRu.

Плагин умеет работать с двумя сайтами ([kinopoiskapiunofficial.tech](https://kinopoiskapiunofficial.tech), [kinopoisk.dev](https://kinopoisk.dev)) в настройках можно выбрать откуда получать информацию. По умолчанию запросы идут на [kinopoiskapiunofficial.tech](https://kinopoiskapiunofficial.tech), работая с общим API токеном. Ограничение для него 500 запросов в день - для общего Token быстро заканчивается. Поэтому лучше зарегестрировать свой собственный (и указать в параметрах). Для [kinopoisk.dev](https://kinopoisk.dev) также есть общий токен, его хватает на 200 запросов в день (спасибо [mdwitr](https://github.com/mdwitr0)) - для общего Token быстро заканчивается. Поэтому лучше зарегестрировать свой собственный (и указать в параметрах).

Плагин умеет подхватывать ID КиноПоиска в имени файла по шаблону "<текст>kp<ID КиноПоиска><текст без цифр><текст>" или "<текст>kp-<ID КиноПоиска><текст без цифр><текст>" и использовать его для поиска в базе. Также умеет искать по названию фильма (если сможет название распознать из имени файла).

Плагин умеет автоматически создавать коллекции фильмом основываясь на данных сиквелов и приквелов Кинопоиска. Функция отключаемая в настройках плагина. Работает только для [kinopoisk.dev](https://kinopoisk.dev) - API предоставляет эти данные.

Плагин умеет создавать коллекцию Кинопоиска Топ 250 из фильмов в медиатеке. Работает только для [kinopoisk.dev](https://kinopoisk.dev) - API предоставляет эти данные.

Плагин умеет добавлять ID КиноПоиска по ID IMDB или TMDB (отдельная задача). Работает только для [kinopoisk.dev](https://kinopoisk.dev) - API предоставляет эти данные.

## Использование

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

* Плагин тестировался на версии 4.7.11
* Собирался c .Net 6.0

## TODO

* Add stopwatch to all API calls
* Use facts in film description
* Add user filtering in all tasks
