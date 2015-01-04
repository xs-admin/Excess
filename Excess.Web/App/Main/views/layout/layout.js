(function () {
    var controllerId = 'app.views.layout';
    angular.module('app').controller(controllerId, [
        '$scope', '$rootScope', '$window', '$modal', 'dialogs', 'xsAuthentication',
        function ($scope, $rootScope, $window, $modal, dialogs, xsAuthentication) {
            var vm = this;

            //loading
            $scope.loadingRequests = 0;

            $scope.$on('loading-requests', function (ev, count) {
                $scope.loadingRequests = count;
            })

            $scope.$on('request-loaded', function () {
                $scope.loadingRequests = $scope.loadingRequests - 1;
            })

            $scope.menus = [
                { icon: 'fa-home',             url: '#/'        },
                { icon: 'fa-info-circle',      url: '#/about'   },
                { icon: 'fa-code', url: '#/project/1' },
                { icon: 'fa-question-circle', url: '#/project' }
            ];

            $scope.openMenu = function () {
                $('#main-menu').trigger('open.mm');
            }

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