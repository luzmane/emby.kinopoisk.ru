define(['loading', 'globalize', 'emby-input', 'emby-button', 'emby-radio', 'emby-checkbox'], function (loading) {
    'use strict';
    function loadPage(page, config) {
        const groupBy = (x, f) => x.reduce((a, b, i) => ((a[f(b, i, x)] ||= []).push(b), a), {});
        page.querySelector('.txtToken').value = config.Token || '';
        //page.querySelector('.chkCreateSeqCollections').checked = (config.ApiType == "kinopoisk.dev" && config.CreateSeqCollections);
        page.querySelector('.kinopoiskUnofficial').checked = (config.ApiType === "kinopoiskapiunofficial.tech");
        page.querySelector('.kinopoiskUnofficial').addEventListener('change', (event) => {
            if (event.currentTarget.checked) page.querySelectorAll('.kinopoiskDevOnly').forEach(item => item.style.display = 'none');
        });
        page.querySelector('.kinopoiskDev').checked = (config.ApiType === "kinopoisk.dev");
        page.querySelector('.kinopoiskDev').addEventListener('change', (event) => {
            if (event.currentTarget.checked) page.querySelectorAll('.kinopoiskDevOnly').forEach(item => item.style.display = '');
        });
        const result = JSON.parse(config.Collections);
        const template = page.querySelector('div.pluginConfigurationPage:not(.hide) label.kpCollectionTemplate');
        if (result && result.length !== 0) {
            Object.entries(groupBy(result, v => v.Category))
                .forEach(([summaryName, list]) => {
                    const details = document.createElement("details");
                    template.parentNode.appendChild(details);
                    const summary = document.createElement("summary");
                    summary.classList.add('checkboxListLabel');
                    summary.textContent = summaryName;
                    details.appendChild(summary);
                    list.forEach(v => {
                        const label = template.cloneNode(true);
                        details.appendChild(label);
                        label.removeAttribute("id");
                        label.removeAttribute("style");
                        label.classList.add('kpCollectionList');
                        label.classList.remove('kpCollectionTemplate');
                        const input = label.querySelector('input[is="emby-checkbox"]');
                        input.classList.add('kp-' + v.Id);
                        input.setAttribute('category', v.Category);
                        input.checked = v.IsEnable;
                        const span = label.querySelector('span.checkboxButtonLabel');
                        span.textContent = v.Name + ' (' + v.MovieCount + ' фильмов)';
                    });
                });
        }
        else {
            let div = document.createElement("div");
            div.textContent = 'Сохраните конфигурацию (кнопка "Сохранить") и обновите страницу чтоб показать список коллекций Кинопоиска';
            template.parentNode.appendChild(div);
            template.parentNode.appendChild(document.createElement("br"));
            template.parentNode.appendChild(document.createElement("br"));
            template.parentNode.appendChild(document.createElement("br"));
            div = document.createElement("div");
            div.textContent = 'Save configuration once (button "Save") and refresh page to enable list of collections';
            template.parentNode.appendChild(div);
        }
        loading.hide();
    }
    function onSubmit(e) {
        e.preventDefault();
        loading.show();
        const form = this;
        getConfig().then(function (config) {
            config.Token = form.querySelector('.txtToken').value;
            //config.CreateSeqCollections = form.querySelector('.chkCreateSeqCollections').checked;
            config.ApiType = form.querySelector('input[name="radioAPI"]:checked').value;
            const list = form.querySelectorAll('div.pluginConfigurationPage:not(.hide) label.kpCollectionList');
            const tmp = [];
            const fetchMovieCount = /^(.+) \((\d+) фильмов\)$/;
            list.forEach(label => {
                const input = label.querySelector('input[is="emby-checkbox"]');
                const IsEnable = input.checked;
                const Category = input.getAttribute('category');
                const span = label.querySelector('span.checkboxButtonLabel');
                const match = span.textContent.match(fetchMovieCount);
                if (match == null) {
                    console.warn("Collection name was not matched: '" + span.textContent + "'");
                    return;
                }
                const Name = match[1];
                const MovieCount = match[2];
                let Id = '';
                for (const i of input.classList) if (i.startsWith('kp-')) Id = i.substring(3);
                if (Id) tmp.push({ Id, Name, IsEnable, Category, MovieCount });
            });
            config.Collections = JSON.stringify(tmp);
            ApiClient.updatePluginConfiguration('0417364b-5a93-4ad0-a5f0-b8756957cf80', config)
                .then(Dashboard.processServerConfigurationUpdateResult);
        });
        return false;
    }
    function getConfig() {
        return ApiClient.getPluginConfiguration('0417364b-5a93-4ad0-a5f0-b8756957cf80');
    }
    return function (view, params) {
        view.querySelector('form').addEventListener('submit', onSubmit);
        view.addEventListener('viewshow', function () {
            loading.show();
            const page = this;
            getConfig().then(function (config) {
                loadPage(page, config);
            });
        });
    };
});
