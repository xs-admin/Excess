'use strict';

/**
 * Binds a graph panel component
 * As per: https://github.com/angular-ui/ui-ace/blob/master/src/ui-ace.js
 */
angular.module('ui.graphpanel', [])
  .constant('uiGraphPanelConfig', {})
  .directive('uiGraphPanel', ['uiGraphPanelConfig', '$interval', function (uiGraphPanelConfig, $interval) {

      //td: check the low level lib is present

      /**
       * Sets editor options
       *
       * The supported options are:
       *
       *   <ul>
       *     <li>nodeTypes (mandatory)</li>
       *     <li>dataTypes (mandatory)</li>
       *     <li>editable</li>
       *     <li>selectable</li>
       *     <li>scrollAndZoom</li>
       *     <li>editNodes</li>
       *     <li>editLinks</li>
       *     <li>onLoad</li>
       *     <li>onChange</li>
       *     <li>onNodeSelected</li>
       *   </ul>
       *
       * @param graphpanel
       * @param {object} opts Options to be set
       */
      var setOptions = function (panel, opts) {
          if (!angular.isDefined(opts.nodeTypes))
              throw new Error('must provide node types');

          if (!angular.isDefined(opts.dataTypes))
              throw new Error('must provide data types');

          panel.dataTypes = opts.dataTypes;
          panel.nodeTypes = opts.nodeTypes;

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
          var panel = new GraphPanel(canvas); //td: styles

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

          return panel;
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
                  panel.fromJSON(scope.$eval(nodes));
              });

              // Listen for option updates
              var updateOptions = function (current, previous) {
                  if (current === previous) return;

                  opts = angular.extend({}, scope.$eval(attrs.uiGraphPanel));
                  setOptions(panel, opts);
              };

              scope.$watch(attrs.uiGraphPanel, updateOptions, /* deep watch */ true);
              updateOptions(options);

              elm.on('$destroy', function () {
                  panel.destroy();
              });
          }
      };
  }]);