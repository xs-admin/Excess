'use strict';

// Declare app level module which depends on views, and components
angular.module('metaprogramming', [
    'ngRoute',
    'metaprogramming.view1',
    'metaprogramming.version',
    'ui.layout',
    'ui.ace',
    'ui.graphpanel'])

.config(['$routeProvider', function ($routeProvider) {
    $routeProvider.otherwise({ redirectTo: '/view1' });
}])

.controller("ctrlExamples", ['$scope', function ($scope) {
    var sourceEditor, transpileEditor;

    $scope.sourceLoaded = function (editor) {
        sourceEditor = editor;
        sourceEditor.setValue(Samples.MetaProgrammingSamples[0], -1);
    }

    $scope.transpileLoaded = function (editor) {
        transpileEditor = editor;
    }

    $scope.setSample = function (index)
    {
        sourceEditor.setValue(Samples.MetaProgrammingSamples[index], -1);
        sourceEditor.focus();
    }
}])

//graph
.controller("ctrlVisual", ['$scope', function ($scope) {
    $scope.Model = Samples.DataProgrammingModel;
    $scope.NodeTypes = Samples.DataProgrammingNodeTypes;
    $scope.DataTypes = Samples.DataProgrammingDataTypes;
}])

;

