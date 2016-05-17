'use strict';

/**
 * Binds a graph panel component
 * As per: https://github.com/angular-ui/ui-ace/blob/master/src/ui-ace.js
 */
angular.module('ui.graphpanel', [])
  .constant('uiGraphPanelConfig', {})
  .directive('uiGraphPanel', ['uiGraphPanelConfig', function (uiGraphPanelConfig) {

      if (angular.isUndefined(window.graphpanel)) {
          throw new Error('graphpanel is missing');
      }

      /**
       * Sets editor options such as the wrapping mode or the syntax checker.
       *
       * The supported options are:
       *
       *   <ul>
       *     <li>showGutter</li>
       *     <li>useWrapMode</li>
       *     <li>onLoad</li>
       *     <li>theme</li>
       *     <li>mode</li>
       *   </ul>
       *
       * @param graphpanel
       * @param {object} opts Options to be set
       */
      var setOptions = function (panel, opts) {
          if (!angular.isDefined(opts.NodeTypes))
              throw new Error('must provide node types');

          panel.nodeTypes = opts.NodeTypes;

          //boolean options
          if (angular.isDefined(opts.editable)) {
              panel.options.editable = opts.editable;
          }

          if (angular.isDefined(opts.scrollAndZoom)) {
              panel.options.scrollAndZoom = opts.scrollAndZoom;
          }
          
          if (angular.isDefined(opts.selectable)) {
              panel.options.selectable = opts.selectable;
          }
          
          if (angular.isDefined(opts.editNodes)) {
              panel.options.editNodes = opts.editNodes;
          }

          if (angular.isDefined(opts.editLinks)) {
              panel.options.editLinks = opts.editLinks;
          }

          //events
          if (angular.isDefined(opts.onLoad)) {
              panel.onLoad = function () {
                  opts.onLoad(panel);
              };
          }

          if (angular.isDefined(opts.onChange)) {
              panel.onChange = function (nodes) {
                  opts.onChange(panel, nodes);
              };
          }

          if (angular.isDefined(opts.onNodeSelected)) {
              panel.onNodeSelected = function (nodes) {
                  opts.onNodeSelected(panel, nodes);
              };
          }
      };

      var createGraphPanel = function (element)
      {
          //create canvas 
          var div = element[0];
          var canvas = document.createElement('canvas');
          div.appendChild(canvas);

          //create component
          var panel = new GraphPanel(canvas, scope.graphstyle);

          //keep the canvas in synch with the parent
          var width = 0;
          var height = 0;
          var stop = $interval(function () {
              var widthNow = div.parentElement.offsetWidth;
              var heightNow = div.parentElement.offsetHeight;

              if (widthNow != width || heightNow != height) {
                  width = canvas.width = widthNow;
                  height = canvas.height = heightNow;

                  panel.update();
              }
          }, 333);
      }

      return {
          restrict: 'EA',
          link: function (scope, elm, attrs) {

              var options = {};
              var opts = angular.extend({}, options, scope.$eval(attrs.uiGraphPanel));

              /**
               * graph panel editor
               * @type object
               */
              var panel = createGraphPanel(elm);


              attrs.$observe('nodes', function (nodes) {
                  panel.fromJSON(nodes);
              });

              // Listen for option updates
              var updateOptions = function (current, previous) {
                  if (current === previous) return;

                  opts = angular.extend({}, scope.$eval(attrs.uiGraphPanel));
                  setOptions(panel, opts);
              };

              scope.$watch(attrs.uiGraphPanel, updateOptions, /* deep watch */ true);

              // set the options here, even if we try to watch later, if this
              // line is missing things go wrong (and the tests will also fail)
              updateOptions(options);

              elm.on('$destroy', function () {
                  panel.destroy();
              });

              scope.$watch(function () {
                  return [elm[0].offsetWidth, elm[0].offsetHeight];
              }, function () {
                  panel.resize(elm[0].offsetWidth, elm[0].offsetHeight);
                  panel.draw();
              }, true);

          }
      };
  }]);