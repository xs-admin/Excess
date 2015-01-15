angular.module('app')

.controller("helpDialogCtrl",
['$scope', '$modalInstance', 'data', function ($scope, $modalInstance, data) {

    $scope.topics    = data;
    $scope.minHeight = '350px';

    var _active = $scope.topics[0];
    $scope.selectTopic = function (topic) {
        _active.visible = false;
        _active = topic;
        _active.visible = true;
    }

    $scope.done = function () {
        $modalInstance.close();
    }
}]);
