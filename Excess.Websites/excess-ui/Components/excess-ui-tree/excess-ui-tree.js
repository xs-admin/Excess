angular.module('excess.ui')

.controller('xsuiTreeController', ['$scope', '$attrs', function ($scope, $attrs) {
    
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

    $scope.actionSelected = function (event, action, data)
    {
        if ($scope.action)
            $scope.action({ action: action, data: data });

        event.stopPropagation();
    }
}])

.directive('xsuiTree', ['$parse', function ($parse) {
    return {
        restrict: 'AE',
        replace: true,
        controller: 'xsuiTreeController',
        scope: {
            tree:   '=',
            action: '&',
        },
        template:
            '<ul class="xs-tree">' + 
            '    <script type="text/ng-template" id="node_renderer.html">' + 
            '        <span ng-click="nodeSelected($event, node.action, node.data)">' + 
            '            <i ng-if="node.icon" class="no-highlighting fa {{node.icon}}"></i> ' + 
            '            <span ng-if="node.color" class="no-highlighting" style="width:20px; background-color: {{node.color}}"></span>' + 
            '            {{node.label}}' + 
            '            <img width="10"/>' + 
            '            <a ng-repeat="action in node.actions track by $index">' + 
            '                <i class="fa {{action.icon}}" ng-click="actionSelected($event, action.id, node.data)"></i>' + 
            '            </a>' + 
            '        </span>' + 
            '        <ul>' + 
            '            <li ng-repeat="node in node.children track by $index"' + 
            '                ng-include="&quotnode_renderer.html&quot"' + 
            '                ng-class="{&quotxs-tree-parent&quot: node.children}"></li>' + 
            '        </ul>' + 
            '    </script>' + 
            '    <li ng-repeat="node in tree track by $index" ' + 
            '        ng-include="&quotnode_renderer.html&quot" ' + 
            '        ng-class="{&quotxs-tree-parent&quot: node.children}"' +
            '        ng-include-variables="{node: node}">' + 
            '    </li>' + 
            '</ul>',
    };
}])