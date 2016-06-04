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

//examples
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
.controller("ctrlVisual", ['$scope', '$timeout', 'Home', function ($scope, $timeout, Home) {
    $scope.Model = Samples.DataProgrammingModel;

    var graphEditor;
    $scope.graphLoaded = function (graphPanel) {
        graphEditor = graphPanel;

        $timeout(function () {
            graphEditor.fit();
        }, 500); 
    }

    var sourceEditor;
    $scope.sourceLoaded = function (editor) {
        sourceEditor = editor;
    }

    $scope.Options =
	{
	    dataTypes: Samples.DataProgrammingDataTypes,
	    nodeTypes: Samples.DataProgrammingNodeTypes,
	    editable: true, scrollAndZoom: true, selectable: true, editNodes: true, editLinks: true,
	    menuSocket: {
	        items: [
                { label: 'Disconnect', onClick: function (p) { p.socket.disconnect() } }
	        ]
	    },

	    onLoad: $scope.graphLoaded,
	}

    $scope.transpileGraph = function () {
        Home.TranspileGraph(JSON.stringify(graphEditor.toJSON()))
            .then(function (value) {
                sourceEditor.setValue(value.data.__res);
                sourceEditor.focus();
            });
    }
}]);

