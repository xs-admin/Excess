'use strict';

// Declare app level module which depends on views, and components
angular.module('$safeprojectname$', [
  'ngRoute',
  '$safeprojectname$.view1',
  '$safeprojectname$.view2',
  '$safeprojectname$.version'
]).
config(['$routeProvider', function($routeProvider) {
  $routeProvider.otherwise({redirectTo: '/view1'});
}]);
