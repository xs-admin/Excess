angular.module('ui.jq.mmenu', [])

.constant('mmenuConfig', {
    classes: "mm-light",
    counters: true,
    searchfield: true,
    header: {
        add: true,
        update: true,
        title: "Main Menu"
    }
})

.controller('MMenuController', ['$scope', '$attrs', 'mmenuConfig', function ($scope, $attrs, mmenuConfig) {
    //remember the options
    //td: options from attrs
    this.options = mmenuConfig;
}])

.directive('mmenu', function () {
    return {
        restrict: 'EA',
        controller: 'MMenuController',
        transclude: true,
        replace: false,
        templateUrl: '/App/Main/components/mmenu.html',
        link: function (scope, element, attrs, mmenuCtrl) {

            //td: options from attrs
            var options = mmenuCtrl.options;
            angular.element(element).mmenu(options);
        }
    };
})