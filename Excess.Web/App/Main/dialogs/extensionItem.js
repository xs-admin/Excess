angular.module('app')

.controller("extensionItemCtrl",
['$scope', '$modalInstance', 'xsProject', 'data', function ($scope, $modalInstance, xsProject, data) {

    $scope.method = data;
    $scope.transformKind = "";
    $scope.kindLabel = "Kind";

    $scope.$watch("transformKind", function (value) {
        if (value.indexOf("lexical") != -1)
            $scope.kindLabel = "Lexical";
        else if (value.indexOf("syntax") != -1)
            $scope.kindLabel = "Syntactical";
        else if (value.indexOf("semantical") != -1)
            $scope.kindLabel = "Semantical";
    });

    $scope.done = function () {
        if ($scope.transform != "" && $scope.transformKind != "") {
            $modalInstance.close(xsProject.extensionItem($scope.method, $scope.transformKind));
        }
    }
}]);
