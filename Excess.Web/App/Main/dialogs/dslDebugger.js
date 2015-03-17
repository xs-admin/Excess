angular.module('app')

.controller("dslDebuggerCtrl",
['$scope', '$timeout', '$modalInstance', 'xsProject', 'data', function ($scope, $timeout, $modalInstance, xsProject, data) {

    $scope.sourceCode = '//your test code here';
    $scope.targetCode = '//results here after compiling';
    $scope.editorKeywords = data.keywords;

    $scope.gotTests = false;
    $scope.testName = '';
    $scope.tests    = [];

    xsProject.dslTests()
        .then(function (result) {
            $scope.tests = result.data;
        })
        .finally(function () {
            $scope.gotTests = true;
        });


    $scope.compileTest = function () {
        $scope.targetCode = "Compiling...";

        var sourceEditor = $('#source-editor').isolateScope();
        xsProject.debugDSL(sourceEditor.content())
            .then(function (result) {
                $scope.targetCode = result.data;
            });
    }

    $scope.setCurrentTest = function (test) {
        $scope.testName   = test.Caption;
        $scope.sourceCode = test.Contents;
    }

    $scope.saveCurrentTest = function () {
        var found = null;
        angular.forEach($scope.tests, function (value, key) {
            if (value.Caption == $scope.testName) {
                found = value;
            }
        });

        var sourceEditor = $('#source-editor').isolateScope();
        var testContent  = sourceEditor.content();

        if (found) {
            found.Contents = testContent;
            xsProject.udateDslTest(found);
        }
        else {
            xsProject.addDslTest($scope.testName, testContent)
                .then(function (result) {
                    $scope.tests.push(result.data);
                })
        }
    }

    $scope.done = function () {
        if ($scope.className != "") {
            $modalInstance.close($scope.className);
        }
    }
}]);
