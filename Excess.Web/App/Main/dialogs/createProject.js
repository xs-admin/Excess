angular.module('app')

.controller("createProjectCtrl",
['$scope', '$modalInstance', 'xsProject', function ($scope, $modalInstance, xsProject) {

    $scope.name = "";
    $scope.kind = "extension";

    $scope.done = function () {
        xsProject.createProject($scope.name, $scope.kind, {})
            .then(function (result) {
                $modalInstance.close(result.data.projectId);
            });
    }
}]);
