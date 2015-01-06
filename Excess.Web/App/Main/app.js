(function () {
    'use strict';

    var app = angular.module('app', [
        'ngAnimate',
        'ngSanitize',

        'ui.router',
        'ui.bootstrap',
        'ui.jq',

        'ui.jq.layout',
        'ui.xs.codemirror',
        'ui.xs.tree',
        'ui.xs.console',

        'rcWizard',
        'dialogs.main',
        'cfp.hotkeys',
    ]);

    //Configuration for Angular UI routing.
    app.config([
        '$stateProvider', '$urlRouterProvider',
        function($stateProvider, $urlRouterProvider) {
            $urlRouterProvider.otherwise('/');
            $stateProvider
                .state('home', {
                    url: '/',
                    templateUrl: '/App/Main/views/home/home.html',
                })
                .state('project', {
                    url: '/project/:projectId',
                    templateUrl: '/App/Main/views/project/project.html',
                })
                .state('about', {
                    url: '/about',
                    templateUrl: '/App/Main/views/about/about.html',
                });
        }
    ]);
})();