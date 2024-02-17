define(['loading', 'globalize', 'emby-input', 'emby-button', 'emby-radio', 'emby-checkbox'], function (loading) {
    'use strict';

    function loadPage(page, config) {
        const groupBy = (x, f) => x.reduce((a, b, i) => ((a[f(b, i, x)] ||= []).push(b), a), {});
        page.querySelector('.txtToken').value = config.Token || '';
        //page.querySelector('.chkCreateSeqCollections').checked = (config.ApiType == "kinopoisk.dev" && config.CreateSeqCollections);
        page.querySelector('.kinopoiskUnofficial').checked = (config.ApiType == "kinopoiskapiunofficial.tech");
        page.querySelector('.kinopoiskUnofficial').addEventListener('change', (event) => {
            if (event.currentTarget.checked) page.querySelectorAll('.kinopoiskDevOnly').forEach(item => item.style.display = 'none');
        });
        page.querySelector('.kinopoiskDev').checked = (config.ApiType == "kinopoisk.dev");
        page.querySelector('.kinopoiskDev').addEventListener('change', (event) => {
            if (event.currentTarget.checked) page.querySelectorAll('.kinopoiskDevOnly').forEach(item => item.style.display = '');
        });
        var result = JSON.parse(config.Collections);
        var template = page.querySelector('div.pluginConfigurationPage:not(.hide) label.kpCollectionTemplate');
        if (result && result.length !== 0) {
            Object.entries(groupBy(result, v => v.Category))
                .forEach(([summaryName, list]) => {
                    var details = document.createElement("details");
                    template.parentNode.appendChild(details);
                    var summary = document.createElement("summary");
                    summary.classList.add('checkboxListLabel');
                    summary.textContent = summaryName;
                    details.appendChild(summary);
                    list.forEach(v => {
                        var label = template.cloneNode(true);
                        details.appendChild(label);
                        label.removeAttribute("id");
                        label.removeAttribute("style");
                        label.classList.add('kpCollectionList');
                        label.classList.remove('kpCollectionTemplate');
                        label.firstElementChild.classList.add('kp-' + v.Id);
                        label.firstElementChild.setAttribute('category', v.Category);
                        label.firstElementChild.checked = v.IsEnable;
                        label.lastElementChild.textContent = v.Name;
                    });
                });
        }
        else {
            var div = document.createElement("div");
            div.textContent = "Сохраните конфигурацию и обновите страницу чтоб показать список коллекций Кинопоиска";
            template.parentNode.appendChild(div);
            template.parentNode.appendChild(document.createElement("br"));
            template.parentNode.appendChild(document.createElement("br"));
            template.parentNode.appendChild(document.createElement("br"));
            div = document.createElement("div");
            div.textContent = "Save configuration once to enable list of collections and refresh page";
            template.parentNode.appendChild(div);
        }
        loading.hide();
    }
    function onSubmit(e) {
        e.preventDefault();
        loading.show();
        var form = this;
        getConfig().then(function (config) {
            config.Token = form.querySelector('.txtToken').value;
            //config.CreateSeqCollections = form.querySelector('.chkCreateSeqCollections').checked;
            config.ApiType = form.querySelector('input[name="radioAPI"]:checked').value;
            var list = form.querySelectorAll('div.pluginConfigurationPage:not(.hide) label.kpCollectionList');
            var tmp = [];
            list.forEach(label => {
                var IsEnable = label.firstElementChild.checked;
                var Category = label.firstElementChild.getAttribute('category');
                var Name = label.lastElementChild.textContent;
                var Id = '';
                for (var i of label.firstElementChild.classList) if (i.startsWith('kp-')) Id = i.substr(3);
                if (Id) tmp.push({ Id, Name, IsEnable, Category });
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
            var page = this;
            getConfig().then(function (config) {
                loadPage(page, config);
            });
        });
    };
});
