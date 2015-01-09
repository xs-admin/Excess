angular.module('app')

.controller("dslDebuggerCtrl",
['$scope', '$modalInstance', 'xsProject', function ($scope, $modalInstance, xsProject) {

    $scope.sourceCode = '//your test code here';
    $scope.targetCode = '//results here after compiling';

    $scope.compileTest = function () {
        var sourceEditor = $('#source-editor').isolateScope();
        xsProject.debugDSL(sourceEditor.content())
            .then(function (result) {
                $scope.targetCode = result.data;
            });
    }

    $scope.done = function () {
        if ($scope.className != "") {
            $modalInstance.close($scope.className);
        }
    }
}]);
