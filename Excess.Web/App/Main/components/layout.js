angular.module('ui.jq.layout', [])

.controller('LayoutController', ['$scope', '$attrs', function ($scope, $attrs) {
}])

.directive('layout', ['$parse', '$timeout', function ($parse, $timeout) {
    return {
        restrict: 'EA',
        controller: 'LayoutController',
        transclude: true,
        replace: true,
        templateUrl: '/App/Main/components/layout.html',
        link: function (scope, element, attrs) {

            function parsePane(attr)
            {
                if (!angular.isDefined(attr))
                    return null;

                var pane = scope.$eval(attr);
                if (pane.resize)
                {
                    pane.onresize = function (paneId, paneObject, paneState, paneOptions)
                    {
                        $timeout(function () {
                            pane.resize(paneId, paneObject, paneState, paneOptions);
                        });
                    }
                }

                return pane;
            }

            var north  = parsePane(attrs.ngNorth);
            var south  = parsePane(attrs.ngSouth); 
            var east   = parsePane(attrs.ngEast); 
            var west   = parsePane(attrs.ngWest); 
            var center = parsePane(attrs.ngCenter);

            var options = {
                enableCursorHotkey: false,
                //applyDefaultStyles: true,
                showOverflowOnHover: false,
            };

            if (angular.isDefined(attrs.ngSplitters))
            {
                var splitters = String((attrs.ngSplitters));
                switch (splitters)
                {
                    case "none":
                    case "false":
                    {
                        options.spacing_open   = 0;
                        options.spacing_closed = 0;
                        break;
                    }
                    case "when-hidden":
                    {
                        options.spacing_open = 0;
                        break;
                    }
                }
            }

            if (north)  options.north  = north;
            if (south)  options.south  = south;
            if (east)   options.east   = east;
            if (west)   options.west   = west;
            if (center) options.center = center;

            var layout = angular.element(element).layout(options);

            var control = scope.$eval(attrs.ngControl);
            if (control) {
                control.open = function (paneId)
                {
                    layout.open(paneId);
                }
            }
        }
    };
}])