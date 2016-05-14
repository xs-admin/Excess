'use strict';

// Declare app level module which depends on views, and components
angular.module('metaprogramming', [
  'ngRoute',
  'metaprogramming.view1',
  'metaprogramming.version'
]).
config(['$routeProvider', function($routeProvider) {
  $routeProvider.otherwise({redirectTo: '/view1'});
}]);
