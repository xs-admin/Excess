angular.module('app')

.controller("newClassCtrl",
['$scope', '$rootScope', '$modalInstance', 'xsProject', function ($scope, $rootScope, $modalInstance, xsProject) {

    $scope.className  = "";
    $scope.buttonText = "Create";
    $scope.done = function ()
    {
        if ($scope.className != "")
        {
            $modalInstance.close($scope.className);
        }
    }
}]);
