define(['loading', 'emby-input', 'emby-button', 'emby-checkbox'], function (loading) {
    'use strict';

    function loadPage(page, config) {
        page.querySelector('.txtToken').value = config.Token || '';
        page.querySelector('.createCollections').checked = (config.ApiType == "kinopoisk.dev" && config.CreateCollections);
        page.querySelector('#radioAPI_dev').checked = (config.ApiType == "kinopoisk.dev");
        page.querySelector('#radioAPI_unoff').checked = (config.ApiType == "kinopoiskapiunofficial.tech");
        page.querySelector('#radioAPI_unoff').addEventListener('change', (event) => {
            if (event.currentTarget.checked) {
                page.querySelector('.createCollections').checked = false;
            }
        })
        loading.hide();
    }
    function onSubmit(e) {
        e.preventDefault();
        loading.show();
        var form = this;
        getConfig().then(function (config) {
            config.Token = form.querySelector('.txtToken').value;
            config.CreateCollections = form.querySelector('.createCollections').checked;
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
