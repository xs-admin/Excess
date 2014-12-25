angular.module('ui.xs.tree', [])

.controller('treeController', ['$scope', '$attrs', function ($scope, $attrs) {
    
    $scope.nodeSelected = function (ev, action, data)
    {
        var parent = $(ev.currentTarget.parentNode);
        if (parent.hasClass('xs-tree-parent'))
        {
            var children = parent.find(' > ul > li');
            if (children.is(":visible")) {
                children.hide('fast');
            } else {
                children.show('fast');
            }
        }

        if (action && $scope.action) {
            $scope.action({ action: action, data: data });
        }

        ev.stopPropagation();
    }

    $scope.actionSelected = function (action, data)
    {
        if ($scope.action)
            $scope.action({ action: action, data: data });
    }
}])

.directive('xsTree', ['$parse', function ($parse) {
    return {
        restrict: 'E',
        replace: true,
        controller: 'treeController',
        scope: {
            tree:   '=',
            action: '&',
        },
        templateUrl: '/App/Main/components/tree.html',
    };
}])