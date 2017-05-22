angular.module('ui.tree', [])

.controller('treeController', ['$scope', '$attrs', function ($scope, $attrs) {
    
    $scope.nodeSelected = function (ev, action, data) {
        var parent = $(ev.currentTarget.parentNode);
        if (parent.hasClass('xs-tree-parent')) {
            var children = parent.find(' > ul > li');
            var expandSign = parent.find(' > span > i');
            if (children.is(":visible")) {
                expandSign.removeClass("fa-chevron-down");
                expandSign.addClass("fa-chevron-right");
                children.hide('fast');
            } else {
                expandSign.removeClass("fa-chevron-right");
                expandSign.addClass("fa-chevron-down");
                children.show('fast');
            }
        }

        if (action && $scope.action) {
            $scope.action({ action: action, data: data });
        }

        ev.stopPropagation();
    }

    $scope.actionSelected = function (event, action, data)
    {
        if ($scope.action)
            $scope.action({ action: action, data: data });

        event.stopPropagation();
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
        //templateUrl: '/components/ui-tree/ui-tree.html',
		templateUrl: function(element, attrs) {
			return '/components/ui-tree/ui-tree.html';
		}
    };
}])