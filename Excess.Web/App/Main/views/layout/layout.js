(function () {
    var controllerId = 'app.views.layout';
    angular.module('app').controller(controllerId, [
        '$scope', '$rootScope', '$window', '$modal', 'dialogs', 'xsAuthentication', 'xsMenu',
        function ($scope, $rootScope, $window, $modal, dialogs, xsAuthentication, xsMenu) {
            var vm = this;

            //loading
            $scope.loadingRequests = 0;

            $scope.$on('loading-requests', function (ev, count) {
                $scope.loadingRequests = count;
            })

            $scope.$on('request-loaded', function () {
                $scope.loadingRequests = $scope.loadingRequests - 1;
            })

            $rootScope.$on('$stateChangeSuccess', function (event, toState) {
                $scope.menus = xsMenu.headerMenu(toState.name);
            })
            
            $scope.showLogin = function () {

                var modalInstance = $modal.open({
                    templateUrl: '/App/Main/dialogs/login.html',
                    controller: 'loginCtrl',
                    windowClass: "app-modal-window",
                    backdrop: true
                });
            }

            $scope.showLogout = function () {

                var dlg = dialogs.confirm('Please Confirm', 'Are you sure you want to log off?');
                dlg.result.then(function () {
                    xsAuthentication.closeSession()
                        .then(function () {
                            $rootScope.session = null;
                        })
                });
            }
        }]);
})();