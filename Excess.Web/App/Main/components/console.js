angular.module('ui.xs.console', [])

.directive('xsConsole', function () {
    return {
        restrict: 'E',
        replace: true,
        scope: {
            control: '='
        },
        template: '<div class="xs-console"></div>',
        link: function (scope, element, attrs) {
            if (scope.control)
            {
                scope.control.add = function (text)
                {
                    element.append('<p class="xs-console-text">' + text + '</p>');
                    element.scrollTop(1E10);
                }

                scope.control.clear = function (text) {
                    element.html('<p class="xs-console-text">' + text + '</p>');
                }
            }
        }
    };
})