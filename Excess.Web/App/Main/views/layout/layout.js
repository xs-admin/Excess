(function () {
    var controllerId = 'app.views.layout';
    angular.module('app').controller(controllerId, [
        '$scope', '$window', function ($scope, $window) {
            var vm = this;

            $scope.menus = [
                { icon: 'fa-home',    url: '#/'        },
                { icon: 'fa-info',    url: '#/about'   },
                { icon: 'fa-share-alt-square', url: '#/project' }
            ];

            $scope.openMenu = function () {
                $('#main-menu').trigger('open.mm');
            }
        }]);
})();