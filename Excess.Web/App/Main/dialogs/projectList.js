angular.module('app')

.controller("projectListCtrl",
['$scope', '$state', '$modalInstance', 'xsProject', function ($scope, $state, $modalInstance, xsProject) {

    $scope.projects = null;
    $scope.busy = true;

    xsProject.userProjects()
        .then(function (result) {
            $scope.busy = false;
            $scope.projects = result.data;
        });

    $scope.editProject = function (id) {
        $modalInstance.close();
        $state.go('project', { projectId: id });
    }
    
    $scope.done = function ()
    {
        $modalInstance.close();
    }
}]);
