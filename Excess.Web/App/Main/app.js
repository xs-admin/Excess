(function () {
    'use strict';

    var app = angular.module('app', [
        'ngAnimate',
        'ngSanitize',

        'ui.router',
        'ui.bootstrap',
        'ui.jq',

        'ui.jq.mmenu',
        'ui.jq.layout',

        'ui.xs.codemirror',
        'ui.xs.tree',
        'ui.xs.console',

        'rcWizard',
        'dialogs.main',
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
                    menu: 'Home' //Matches to name of 'Home' menu in ExcessNavigationProvider
                })
                .state('project', {
                    url: '/project',
                    templateUrl: '/App/Main/views/project/project.html',
                    menu: 'Project' 
                });
        }
    ]);
})();