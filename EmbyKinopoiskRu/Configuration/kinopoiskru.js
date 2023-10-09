define(['loading', 'globalize', 'emby-input', 'emby-button', 'emby-radio', 'emby-checkbox'], function (loading) {
    'use strict';

    function loadPage(page, config) {
        page.querySelector('.txtToken').value = config.Token || '';
        page.querySelector('.top250Name').value = config.Top250CollectionName;// || 'КинопоискТоп250';
        page.querySelector('.chkCreateSeqCollections').checked = (config.ApiType == "kinopoisk.dev" && config.CreateSeqCollections);
        page.querySelector('.chkTop250InOneLib').checked = (config.ApiType == "kinopoisk.dev" && config.Top250InOneLib);
        page.querySelector('.kinopoiskUnofficial').checked = (config.ApiType == "kinopoiskapiunofficial.tech");
        page.querySelector('.kinopoiskUnofficial').addEventListener('change', (event) => {
            if (event.currentTarget.checked) page.querySelectorAll('.kinopoiskDevOnly').forEach(item => item.style.display = 'none');
        });
        page.querySelector('.kinopoiskDev').checked = (config.ApiType == "kinopoisk.dev");
        page.querySelector('.kinopoiskDev').addEventListener('change', (event) => {
            if (event.currentTarget.checked) page.querySelectorAll('.kinopoiskDevOnly').forEach(item => item.style.display = '');
        });
        loading.hide();
    }
    function onSubmit(e) {
        e.preventDefault();
        loading.show();
        var form = this;
        getConfig().then(function (config) {
            config.Token = form.querySelector('.txtToken').value;
            config.CreateSeqCollections = form.querySelector('.chkCreateSeqCollections').checked;
            config.Top250InOneLib = form.querySelector('.chkTop250InOneLib').checked;
            config.Top250CollectionName = form.querySelector('.top250Name').value;
            config.ApiType = form.querySelector('input[name="radioAPI"]:checked').value;
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
