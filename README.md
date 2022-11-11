# emby.kinopoisk.ru

Fetches metadata from https://www.kinopoisk.ru/. This site is popular in the Russian-speaking community and contains almost no English-language information, so further description will be in Russian.

Плагин для Emby для загрузки метаданных фильмов, сериалов с сайта https://www.kinopoisk.ru.

## Установка

* Положить dll в папку plugins

## Настройка

Параметры плагина искать в: Администрирование - Панель - Расширенное - Плагины - вкладка "Мои плагины" - KinopoiskRu.

Плагин умеет работать с двумя сайтами (https://kinopoiskapiunofficial.tech, https://kinopoisk.dev) в настройках можно выбрать откуда получать информацию. По умолчанию запросы идут на https://kinopoiskapiunofficial.tech, работая с общим API токеном. Ограничение для него порядка 20 запросов/сек - для общего Token быстро заканчивается. Поэтому лучше зарегестрировать свой собственный (и указать в параметрах). Для https://kinopoisk.dev общего токена нет, так что перед использованием надо зарегестрироваться.

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
- Студии (только через https://kinopoisk.dev, https://kinopoiskapiunofficial.tech такой информации не возвращает)
- Трейлеры (в виде ссылок)

## Требования

* Плагин тестировался на версии 4.7.8.0
* Собирался c .Net 6.0
