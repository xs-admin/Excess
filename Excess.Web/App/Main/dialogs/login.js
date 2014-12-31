angular.module('app')

.controller("loginCtrl",
['$scope', '$window', '$modalInstance', function ($scope, $window, $modalInstance) {
    
    $scope.listenToLogin = function ()
    {
        $window.loginListener = $scope;
    }

    $scope.loggedSuccessfully = function () {
        $scope.refreshSession();
        $modalInstance.close();
    }

}])

.directive('xsLogin', function () {
    return {
        restrict: 'E',
        replace: true,
        template: '<iframe src="Account/Login" width="533px" height="280px" scrolling="no" seamless frameborder="0"></iframe>',
        link: function (scope, element, attrs) {
            scope.listenToLogin();
        }
    };
})
