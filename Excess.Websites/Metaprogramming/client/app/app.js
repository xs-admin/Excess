'use strict';

// Declare app level module which depends on views, and components
angular.module('metaprogramming', [
    'ngRoute',
    'metaprogramming.view1',
    'metaprogramming.version',
    'xs.Services',
    'ui.layout',
    'ui.ace',
    'ui.graphpanel'])

.config(['$routeProvider', function ($routeProvider) {
    $routeProvider.otherwise({ redirectTo: '/view1' });
}])

.controller("ctrlExamples", ['$scope', 'Home', function ($scope, Home) {
    var sourceEditor, transpileEditor;

    $scope.sourceLoaded = function (editor) {
        sourceEditor = editor;
        sourceEditor.setValue(Samples.MetaProgrammingSamples[0], -1);
    }

    $scope.transpileLoaded = function (editor) {
        transpileEditor = editor;
    }

    $scope.setSample = function (index){
        sourceEditor.setValue(Samples.MetaProgrammingSamples[index], -1);
        sourceEditor.focus();
    }

    $scope.transpileExample = function () {
        var text = sourceEditor.getValue();
        var result = "";
        for (var i = 0; i < Samples.MetaProgrammingSamples.length; i++)
        {
            if (text == Samples.MetaProgrammingSamples[i])
            {
                transpileEditor.setValue(Samples.MetaProgrammingResults[i]);
                return; //cached
            }
        }

        Home.Transpile(text)
            .then(function (value){
                transpileEditor.setValue(value.data.__res);
                transpileEditor.focus();
            });
    }
    
}])

//graph
.controller("ctrlVisual", ['$scope', function ($scope) {
    $scope.Model = Samples.DataProgrammingModel;
    $scope.NodeTypes = Samples.DataProgrammingNodeTypes;
    $scope.DataTypes = Samples.DataProgrammingDataTypes;
}])

;

