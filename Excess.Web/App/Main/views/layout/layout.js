(function () {
    var controllerId = 'app.views.layout';
    angular.module('app').controller(controllerId, [
        '$scope', '$window', function ($scope, $window) {
            var vm = this;

            $scope.menus = [
                { icon: 'fa-home',             url: '#/'        },
                { icon: 'fa-info-circle',      url: '#/about'   },
                { icon: 'fa-code', url: '#/project' },
                { icon: 'fa-question-circle', url: '#/project' }
            ];

            $scope.openMenu = function () {
                $('#main-menu').trigger('open.mm');
            }
        }]);
})();