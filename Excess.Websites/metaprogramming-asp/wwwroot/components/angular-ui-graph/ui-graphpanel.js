'use strict';

/**
 * Binds a graph panel component
 * As per: https://github.com/angular-ui/ui-ace/blob/master/src/ui-ace.js
 */

angular.module('ui.graphpanel', [])
.constant('uiGraphPanelConfig', {})
.directive('uiGraphPanel', ['uiGraphPanelConfig', '$interval', function (uiGraphPanelConfig, $interval) {

	//td: check the low level lib is present

    var setOptions = function (panel, opts) {
		//console.log('setting options');
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

		if (angular.isDefined(opts.menuSocket)) {
			panel.options.menuSocket = opts.menuSocket;
		}
		
		if (angular.isDefined(opts.menuNode)) {
			panel.options.menuNode = opts.menuNode;
		}
		
		if (angular.isDefined(opts.menuGraph)) {
			panel.options.menuGraph = opts.menuGraph;
		}
		
		//events
		if (angular.isDefined(opts.onInit)) {
			panel.onInit = function () {
				opts.onInit(panel);
			};
		}

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
				opts.onNodeSelected(nodes);
			};
		}
			
		if (angular.isDefined(opts.onContextMenu)) {
			panel.onContextMenu = function (event, node, socket, panelPt) {
				opts.onContextMenu(event, node, socket, panelPt);
			};
		}

		if (angular.isDefined(opts.onHideContextMenu)) {
			panel.onHideContextMenu = function () {
				opts.onHideContextMenu();
			};
			
		}
	};

	var createGraphPanel2 = function (element)
	{
		//create canvas 
		var elem = element[0];
		var div;
		if (elem.tagName == 'ui-graph-panel')
		{
			div = document.createElement('div');
			elem.appendChild(div);
		}
		else
			div = elem;

		//var refElem = elem.parentElement;
		var refElem = elem;
		if (refElem.style.height === '')
			refElem.style.height = '100%';
		
		//div.style.position = 'absolute';
		//div.style.position = 'relative';

		var extra = 0;
		var canvas = document.createElement('canvas');
		var width = canvas.width = refElem.offsetWidth-extra;
		var height = canvas.height = refElem.offsetHeight-extra;
		canvas.style.position = 'absolute';
		div.appendChild(canvas);

		//create component
		var panel = createGraphPanel(canvas);// new GraphPanel(canvas); //td: styles

		//keep the canvas in synch with the parent
		var stop = $interval(function () {
			var widthNow = refElem.offsetWidth-extra;
			var heightNow = refElem.offsetHeight-extra;

			if (widthNow != width || heightNow != height) {
				width = canvas.width = widthNow;
				height = canvas.height = heightNow;

				panel.updateTransform();
			}
		}, 333);

		return panel;
	}

    return {
        restrict: 'AE',
        //restrict: 'E',
		link: function (scope, elm, attrs) {
			var options = {};
			var attrOptions = attrs.uiGraphPanel || attrs.options;
			var opts = angular.extend({}, options, scope.$eval(attrOptions));

			var panel = createGraphPanel2(elm);

			attrs.$observe('nodes', function (nodes) {
				panel.fromJSON(scope.$eval(nodes));
				if (panel.onLoad)
					panel.onLoad();
			});

			// Listen for option updates
			var updateOptions = function (current, previous) {
				if (current === previous) return;

				opts = angular.extend({}, scope.$eval(attrOptions));
				setOptions(panel, opts);
			};

			scope.$watch(attrOptions, updateOptions, true);
			updateOptions(options);

			elm.on('$destroy', function () {
				panel.destroy();
			});

			if (panel.onInit)
				panel.onInit();
		}
	};
}]);
