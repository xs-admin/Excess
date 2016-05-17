'use strict';

angular.module('ui.graphpanel', [])
    .directive('graphPanel', ['$interval', function ($interval) {
        return {
            restrict: 'E',
            link: function (scope, element, attributes) {
                //create canvas 
                var div = element[0];
                var canvas = document.createElement('canvas');
                div.appendChild(canvas);

                //create component
                var panel = new GraphPanel(canvas, scope.graphstyle);

                //keep the canvas in synch with the parent
                var width = 0;
                var height = 0;
                var stop = $interval(function() {
                    var widthNow = div.parentElement.offsetWidth;
                    var heightNow = div.parentElement.offsetHeight;

                    if (widthNow != width || heightNow != height)
                    {
                        width = canvas.width = widthNow;
                        height = canvas.height = heightNow;

                        panel.update();
                    }
                }, 333);

                //do the angular
                scope.graphPanel = panel;
                if (scope.onLoad)
                    panel.onLoad = function () {
                        scope.onLoad(panel);
                    };

                panel.onChange = function (nodes) {
                    if (!angular.equals(scope.nodes, nodes)) {
                        scope.nodes = nodes;
                        scope.lastPanelNodes = nodes;
                        scope.$apply();
                    }
                }

                if (scope.onNodeSelected)
                    panel.onNodeSelected = function (nodes) {
                        scope.onNodeSelected(nodes);
                    }

                //model
                var nodeTypes = null;
                var nodes = null;

                function loadNodes()
                {
                    panel.nodeTypes = nodeTypes;
                    panel.fromJSON(nodes);
                }

                scope.$watch('nodeTypes', function (newVal) {
                    if (nodeTypes == null) {
                        nodeTypes = newVal;
                        if (nodes != null)
                            loadNodes();
                    }
                });

                scope.$watch('nodes', function (newVal) {
                    if (nodes == null) {
                        nodes = newVal;
                        if (nodeTypes != null)
                            loadNodes();
                    }
                });

                scope.$watch('editable', function (newVal) {
                    panel.options.editable = (newVal != 'false');
                });

                scope.$watch('scrollAndZoom', function (newVal) {
                    panel.options.scrollAndZoom = (newVal != 'false');
                });

                scope.$watch('selectable', function (newVal) {
                    panel.options.selectable = (newVal != 'false');
                });

                scope.$watch('editNodes', function (newVal) {
                    panel.options.editNodes = (newVal != 'false');
                });

                scope.$watch('editLinks', function (newVal) {
                    panel.options.editLinks = (newVal != 'false');
                });

                panel.draw();
            },
            scope: {
                name: "@",
                width: "@",
                height: "@",
                nodeTypes: "=",
                //nodes: "=",
                editable: "@",
                editNodes: "@",
                editLinks: "@",
                scrollAndZoom: "@",
                selectable: "@",
                graphstyle: "@",
                onLoad: "@",
                onChange: "@",
                onNodeSelected: "@",
            }
        };
  }]);