
var createGraphPanel;

(function(){

createGraphPanel = function(canvas, style)
{
	return new GraphPanel(canvas, style);
}

function $$(item, thiz, a1, a2, a3, a4)
{
	return (typeof item == 'function') ? item.call(thiz, a1, a2, a3, a4) : item;
}

function $$menu(menu, thiz, a1, a2, a3, a4)
{
	if (menu === undefined)
		return undefined;
	
	var newMenu = $$(menu, thiz, a1, a2, a3, a4);
	newMenu = copyJSON(newMenu, false);
	
	if (typeof newMenu == 'string')
		newMenu = { label: newMenu };
	else
	{
		if (newMenu.label)
			newMenu.label = $$(newMenu.label, thiz, a1, a2, a3, a4);
		if ('disabled' in newMenu)
			newMenu.disabled = $$(newMenu.disabled, thiz, a1, a2, a3, a4);
	}
	
	if (newMenu.items)
	{
		newMenu.items = $$(newMenu.items, thiz, a1, a2, a3, a4);
		
		newMenu.items = newMenu.items.map(function(menuItem)
		{
			var newItem = $$menu(menuItem, thiz, a1, a2, a3, a4);
			return newItem;
		});
	}
		
	return newMenu;
}

function concatMenu(menu1, menu2)
{
	var menuRes = { items: [], onClick: (menu1 && menu1.onClick) };
	menuRes.items = menu1 && menu1.items.concat();
	if (menuRes && menuRes.items.length > 0)
	{
		if (menu2 && menu2.items.length > 0)
		{
			menuRes.items = menuRes.items.concat('-', menu2.items.map(function(item)
			{
				newItem = copyJSON(item);
				if (!item.onClick && menu2.onClick)
					newItem.onClick = menu2.onClick;
				return newItem;
			}));
		}
	}
	else
		menuRes.items = menu2 && menu2.items.concat();
	
	return (menuRes && menuRes.items.length > 0) ? menuRes : undefined;
}

var GRAPH_SCROLL_MARGIN = 40;
var GRAPH_SCROLL_SPEED = 0.4;
var GRAPH_NODE_EXTRA_VMARGIN = 6;
var GRAPH_MAX_ZOOM = 2.0;

var __VALUE_CACHE = {};
var __VALUE_CACHE_ID = 1;

function getUniqueID()
{
	return __VALUE_CACHE_ID++;
}

function resetCache()
{
	//console.log('cache reset');
	__VALUE_CACHE = {};
}

function arrayToJSON(array1)
{
    return array1.map(function(item) { return item.toJSON() });
}

function copyJSON(source, deep)
{
	deep = deep === undefined ? true : false;
	if (source instanceof Array)
	{
		var newArray = new Array(source.length);
		for(var i=0; i<source.length; i++)
			newArray[i] = deep ? copyJSON(source[i], true) : source[i];
			
		return newArray;
	}
	else if (typeof source == "object")
	{
		var dest = {};
		for(var item in source)
			dest[item] = deep ? copyJSON(source[item], true) : source[item];
			
		return dest;
	}
	else
		return source;
}
	
function getDefaultGraphStyle()
{
	return {
		backgroundColor : "rgb(64, 64, 64)",
		
		node :
		{
			headerSize : 20,
			headerFont : "13px sans-serif",
			headerFillColor : "rgba(0, 0, 0, 0.5)",	
			headerTextColor : "rgb(255, 255, 255)",
			cornerRadius : 8,
			lineWidth : 1,
			outlineColor : "rgb(180, 180, 180)",
			fillColor : "rgb(128, 128, 128)",
			socket : 
			{
				radius : 5,
				vmargin : 8,
				hitZoneBleed : 2,
				font : "10px sans-serif",
				textColor : "rgb(44, 44, 44)",
				outlineColor : "black",
				defaultColor : "white"	// fill color of the sockets with no dataType associated
			},
			selected :
			{
				lineWidth : 2,
				//outlineColor : "black",//"rgb(192, 160, 128)",
				//fillColor : "rgb(160, 128, 100)",
				socket : 
				{
					//textColor : "rgb(44, 44, 44)",
					outlineColor : "lightgray"
				}
			}
		},

		link :
		{
			color : "dataType", // "dataType" means the links will use the color of the dataType associated with it's sockets
			//color : "white",
			lineWidth : 1,
			defaultColor : "white", // default color of links with no dataType
			shadowColor : "rgba(0,0,0,0.8)"
		}
	}
}

function mixStyles(style1, style2, style3)
{
	var newStyle = {};
	
	for(var st in style3)
		//if (st != "selected")
		if (typeof style3[st] == "object")
			newStyle[st] = mixStyles(style3[st], newStyle[st]);
		else
			newStyle[st] = style3[st];
	
	for(var st in style2)
		//if (st != "selected")
		if (typeof style2[st] == "object")
			newStyle[st] = mixStyles(style2[st], newStyle[st]);
		else
			newStyle[st] = style2[st];
	
	for(var st in style1)
		//if (st != "selected")
		if (typeof style1[st] == "object")
			newStyle[st] = mixStyles(style1[st], newStyle[st]);
		else
			newStyle[st] = style1[st];
	
	return newStyle;
}

function getSocketHeight(socketStyle)
{
	return socketStyle.radius*2;// + socketStyle.vmargin;
}

function getNodeTopMargin(nodeStyle)
{
	return (nodeStyle.headerSize > 0) 
		? nodeStyle.headerSize + nodeStyle.socket.vmargin/2 + GRAPH_NODE_EXTRA_VMARGIN
		: nodeStyle.cornerRadius + nodeStyle.socket.vmargin/2 + GRAPH_NODE_EXTRA_VMARGIN;
}

function getNodeBottomMargin(nodeStyle)
{
	return nodeStyle.cornerRadius;// + GRAPH_NODE_EXTRA_VMARGIN;
}

function getElementPosition(element) 
{
	var pt = new Point(0, 0);

	if (element.offsetParent)
	{
		pt.x = element.offsetLeft;
		pt.y = element.offsetTop;
		var parent = element.offsetParent;
		while (parent)
		{
			pt.x += parent.offsetLeft;
			pt.y += parent.offsetTop;
			if (parent.clientTop && parent.clientLeft) 
			{
				pt.x += parent.clientLeft;
				pt.y += parent.clientTop;
			}
			
			parent = parent.offsetParent;
		}
	}
	else if (element.left && element.top) 
	{
		pt.x = element.left;
		pt.y = element.top;
	}
	
	return pt;
}


function drawRect(context, color, x, y, w, h)
{
	if (x instanceof Rectangle)
	{
		y = x.y;
		w = x.width;
		h = x.height;
		x = x.x;
	}
	
	/*context.beginPath();
	context.lineWidth = 1;
	context.strokeStyle = color;
	context.strokeRect(x, y, w, h);
	context.closePath();*/
	
	context.fillStyle = color;
	context.fillRect(x, y, w, 1);
	context.fillRect(x, y+h, w, 1);
	context.fillRect(x, y, 1, h);
	context.fillRect(x+w, y, 1, h+1);
}

function drawRoundRect(context, color, x, y, w, h, radius, lineWidth)
{
	context.beginPath();
	context.lineWidth = lineWidth;
	context.strokeStyle = color;
	
	//if (2*radius > w) radius = w/2;
	
	context.moveTo(x + radius, y);
	context.lineTo(x + w - radius, y);
	context.arcTo(x + w, y, x + w, y + radius, radius);
	context.lineTo(x + w, y + h - radius);
	context.arcTo(x + w, y + h, x + w - radius, y + h, radius);
	context.lineTo(x + radius, y + h);
	context.arcTo(x, y + h, x, y + h - radius, radius);
	context.lineTo(x, y + radius);
	context.arcTo(x, y, x + radius, y, radius);
	
	context.closePath();
	context.stroke();
}

//---------------------------------------------------------------------------------------------------
// Point
//---------------------------------------------------------------------------------------------------

function Point(x, y)
{
	this.x = x;
	this.y = y;
}

Point.prototype = 
{
	delta: function(pt)
	{
		var deltaX = Math.abs(this.x - pt.x);
		var deltaY = Math.abs(this.y - pt.y);
		return deltaX > deltaY ? deltaX : deltaY;
	}
}

//---------------------------------------------------------------------------------------------------
// Rectangle 
//---------------------------------------------------------------------------------------------------

function Rectangle(x, y, width, height)
{
	if (x instanceof Point)
	{	// x and y are Points, width and height not used
		if (y.x - x.x >= 0)
		{
			this.x = x.x;
			this.width = y.x - x.x;
		}
		else
		{
			this.x = y.x;
			this.width = x.x - y.x;
		}
		
		if (y.y - x.y >= 0)
		{
			this.y = x.y;
			this.height = y.y - x.y;
		}
		else
		{
			this.y = y.y;
			this.height = x.y - y.y;
		}
	}
	else
	{
		this.x = x;
		this.y = y;
		this.width = width;
		this.height = height;
	}
}
	
Rectangle.prototype = 
{
	top: function() { return this.y; },
	left: function() { return this.x; },
	right: function() { return this.x + this.width; },
	bottom: function() { return this.y + this.height; },
	center: function() { return new Point(this.x + this.width/2, this.y + this.height/2); },
	
	inflate: function(size)
	{
		return new Rectangle(this.x - size, this.y - size, this.width + 2*size, this.height + 2*size);
	},
	
	union: function(rc)
	{
		var minX = Math.min(this.x, rc.x);
		var minY = Math.min(this.y, rc.y);
		var maxX = Math.max(this.right(), rc.right());
		var maxY = Math.max(this.bottom(), rc.bottom());
		return new Rectangle(minX, minY, maxX-minX, maxY-minY);
	},

	zoom: function(factor)
	{
		var center = this.center();
		var newWidth = factor*this.width;
		var newHeight = factor*this.height;
		return new Rectangle(center.x - newWidth/2, center.y - newHeight/2, newWidth, newHeight);
	},
	
	contain: function(point)
	{
		return this.x <= point.x && this.right() > point.x && this.y <= point.y && this.bottom() > point.y;
	},
	
	intersectsWith: function(rect)
	{
		if ((rect.right() < this.x) || (rect.x > this.right()) ||
			(rect.bottom() < this.y) || (rect.y > this.bottom()))
			return false;
		return true;
	}
}

//---------------------------------------------------------------------------------------------------
// GraphSocket
//---------------------------------------------------------------------------------------------------

function GraphSocket(name, label, parentNode, socketDir, dataType, def)
{
	this.parentNode = parentNode;
	this.panel = parentNode.panel;
	this.socketDir = socketDir;
	this.uniqueID = getUniqueID();
	this.def = def;
	
	Object.defineProperty(this, "name", 
	{
		set: function (x) 
		{
			if (this._name === x)
				return;  // nothing to change
			
			this._name = x;
			this.panel && this.panel.update();
		},
		get: function () 
		{
			return this._name;
		}
	});
	
	Object.defineProperty(this, "dataType", 
	{
		set: function (x) 
		{
			// if x is an array then it's a generic type
			this.isGeneric = (x instanceof Array) || (x == '*');
			
			var newDataType = (x instanceof Array) ? x[0] : x;
			
			if (newDataType == "*" && this.linkedOutput && this.socketDir == "input")
				// when setting to generic, retain the dataType imposed by the linked output socket
				newDataType = this.linkedOutput.dataType;
			
			if (this._dataType === newDataType)
				return;  // nothing to change
			
			this._dataType = newDataType;
			
			if (this.panel)
			{
				//this.parentNode.customSockets = true;
				if (this.socketDir == "output")
				{
					var inputs = this.panel.getLinkedInputs(this)
					for(var idx in inputs)
					{
						var input = inputs[idx];
						if (input.isGeneric)
						{	// if a linked input socket is generic, update it's dataType
							input.dataType = [this.dataType];
						}
					}
				}
				
				this.parentNode.update();
				this.panel.update();
			}
		},
		get: function () 
		{
			if (this.isGeneric)
				return (this.linkedOutput && this.linkedOutput.dataType) || '*';
			else
				return $$(this._dataType, this.parentNode);
		}
	});
	
	this._dataType = dataType;
	this.isGeneric = (dataType == '*');
	this.name = name;
	this.label = label;
}

GraphSocket.prototype = 
{
	toJSON: function()
	{
		return { 'name' : this.name, 'dataType' : this.dataType };
	},
	
	disconnect: function()
	{
		var panel = this.panel;
		panel.getLinksBySocket(this).forEach(function(link) { panel.deleteLink(link) });
	},
	
	_connectToOutput: function(outputSocket)
	{
		this.linkedOutput = outputSocket;
	},
	
	evaluate: function()
	{
		if (this.uniqueID in __VALUE_CACHE)
			return __VALUE_CACHE[this.uniqueID];
		var result;
		
        if (this.socketDir == "output")
			// use the "evaluate" function from the socket definition
			result = this.def && this.def.onEvaluate && this.def.onEvaluate.call(this.parentNode);
		else
		{	// input socket, use the data from the linked socket
			var linkedOutput = this.panel.getLinkedOutput(this);
			if (linkedOutput)
				result = linkedOutput.evaluate(this.parentNode);
			else
				// if the socket is not connected, use the default value
				result = this.def && this.def.default;
		}
		
		__VALUE_CACHE[this.uniqueID] = result;
		return result;
	},
	
	getMenu : function()
	{
		return this.def && this.def.menu;
	},

	getRect: function()
	{
		var rect;
		var nodeStyle = this.parentNode.getStyle();
		//var topMargin = getNodeTopMargin(nodeStyle);
		var yc = (this.rc.top() + this.rc.bottom()) / 2;

		if (this.socketDir == "input")
		{
			rect = new Rectangle(this.parentNode.x - nodeStyle.socket.radius, 
				this.parentNode.y + yc - nodeStyle.socket.radius,
				nodeStyle.socket.radius*2, nodeStyle.socket.radius*2);
		}
		else
		{
			rect = new Rectangle(this.parentNode.x + this.parentNode.width - nodeStyle.socket.radius, 
				this.parentNode.y + yc - nodeStyle.socket.radius,
				nodeStyle.socket.radius*2, nodeStyle.socket.radius*2);
		}

		return rect;
	},
	
	getHitArea: function()
	{
		return this.getRect().inflate(this.parentNode.getStyle().socket.hitZoneBleed);
	},
	
    getTextPosition: function(context)
    {
        var ptText;
		var rect = this.getRect();

        if (this.socketDir == "input")
        {
            ptText = new Point(rect.right() + 2, rect.y);
        }
        else
        {
            var measure = context.measureText(this.getLabel());
            ptText = new Point(rect.x - 2 - measure.width, rect.y);
			if (ptText.x < this.parentNode.x + 2)
				ptText.x = this.parentNode.x + 2;
		}

        return ptText;
	},
	
	getLabel: function()
	{
		return this.label === undefined ? this.name : typeof(this.label) == 'function' ? this.label.call(this.parentNode) : this.label;
	},
	
	draw: function(context, graphPanel)
	{
		var typeDef;
		
		if (this.dataType && this.dataType != '*')
		{
			typeDef = graphPanel.dataTypes && graphPanel.dataTypes[this.dataType];
			if (!typeDef)
				console.error('Error: unknown dataType "' + this.dataType + '"');
		}
		
		var socketStyle = this.parentNode.getStyle().socket;
		var rect = this.getRect();
		context.beginPath();
		context.lineWidth = 1;
		var fillColor = typeDef ? typeDef.color : socketStyle.defaultColor;
		context.fillStyle = fillColor;
		context.strokeStyle = socketStyle.outlineColor;
		var ptCenter = rect.center();
		context.arc(ptCenter.x, ptCenter.y, socketStyle.radius, 0, 2*Math.PI, true);
		context.fill();
		context.stroke();
		context.closePath();

		if (graphPanel.zoom >= 0.5)
		{	// draw the text of the socket
			context.save();
			context.beginPath();
			if (this.socketDir == "output")
				context.rect(this.parentNode.x+2, rect.top(), this.parentNode.width-rect.width/2-4, rect.height);
			else
				context.rect(rect.right()+2, rect.top(), this.parentNode.width-rect.width/2-4, rect.height);
			context.clip();

			var pt = this.getTextPosition(context);

			if (socketStyle.textColor == "" || socketStyle.textColor == "dataType")
				context.fillStyle = fillColor;
			else
				context.fillStyle = socketStyle.textColor;
			context.font = socketStyle.font;
			context.textBaseline = "middle";
			context.fillText(this.getLabel(), pt.x, pt.y + socketStyle.radius);
			context.textBaseline = "top";
			
			context.restore();
		}
	}
}

//---------------------------------------------------------------------------------------------------
// GraphDragLink
//---------------------------------------------------------------------------------------------------

function GraphDragLink(socket, point)
{
	this.socket = socket;
	this.endPoint = point;
}

GraphDragLink.prototype = 
{
	moveTo: function(point)
	{
		this.endPoint.x = point.x;
		this.endPoint.y = point.y;
	},
	
	draw: function(context, graphPanel)
	{
		var ptStart = this.socket.getRect().center();
		
		context.beginPath();
		context.lineWidth = graphPanel.style.link.lineWidth;
		
		if (graphPanel.style.link.color == "dataType")
		{
			var dataType;
			if (this.socket.dataType && this.socket.dataType != '*')
			{
				dataType = graphPanel.dataTypes && graphPanel.dataTypes[this.socket.dataType];
				if (dataType == undefined)
					console.error('Error: unknown data type: "' + this.socket.dataType + '"');
			}
			
			context.strokeStyle = dataType ? dataType.color : graphPanel.style.link.defaultColor;
		}
		else
			context.strokeStyle = graphPanel.style.link.color;
		
		context.moveTo(ptStart.x, ptStart.y);
		var centerX = (this.endPoint.x + ptStart.x)/2;
		context.bezierCurveTo(centerX, ptStart.y, centerX, this.endPoint.y, this.endPoint.x, this.endPoint.y);
		context.stroke();
	}
}

//---------------------------------------------------------------------------------------------------
// GraphLink
//---------------------------------------------------------------------------------------------------

function GraphLink(outputSocket, inputSocket)
{
	this.outputSocket = outputSocket;
	this.inputSocket = inputSocket;
}

GraphLink.prototype = 
{
	toJSON: function()
	{
		var outputNode = this.outputSocket.parentNode;
		var inputNode = this.inputSocket.parentNode;
		var graphPanel = outputNode.panel;
		
		return { 
			"outputNode": graphPanel.getNodeIndex(outputNode), 
			"outputSocket": outputNode.getOutputIndex(this.outputSocket),
			"outputSocketName": this.outputSocket._name,
			"inputNode": graphPanel.getNodeIndex(inputNode),
			"inputSocket": inputNode.getInputIndex(this.inputSocket),
			"inputSocketName": this.inputSocket._name,
		}
	},
	
	draw: function(context, graphPanel)
	{
		var ptOutput = this.outputSocket.getRect().center();
		var ptInput = this.inputSocket.getRect().center();
		
		var strokeStyle;
		
		if (graphPanel.style.link.color == "dataType")
		{
			var dataType;
			if (this.outputSocket.dataType && this.outputSocket.dataType != '*')
			{
				dataType = graphPanel.dataTypes && graphPanel.dataTypes[this.outputSocket.dataType];
				if (dataType == undefined)
					console.error('Error: unknown data type: "' + this.outputSocket.dataType + '"');
			}
			
			strokeStyle = dataType ? dataType.color : graphPanel.style.link.defaultColor;
		}
		else
			strokeStyle = graphPanel.style.link.color;

		context.beginPath();
		
		context.strokeStyle = graphPanel.style.link.shadowColor;
		context.lineWidth = graphPanel.style.link.lineWidth + 1.5;
		var centerX = (ptInput.x + ptOutput.x)/2;
		context.moveTo(ptOutput.x, ptOutput.y);
		context.bezierCurveTo(centerX, ptOutput.y, centerX, ptInput.y, ptInput.x, ptInput.y);
		context.stroke();
		
		context.strokeStyle = strokeStyle;
		context.lineWidth = graphPanel.style.link.lineWidth;
		context.stroke();
	}
}

//---------------------------------------------------------------------------------------------------
// GraphNode
//---------------------------------------------------------------------------------------------------

function GraphNode(panel, typeName, name, x, y, width, height, style)
{
	//this.name = name;
	this.panel = panel;
	this.typeName = typeName;
	this.def = typeName && panel.nodeTypes && panel.nodeTypes[typeName];
	this._onUpdate = this.def && this.def.onUpdate
	this.onPreDraw = this.def && this.def.onPreDraw;
	this.onPostDraw = this.def && this.def.onPostDraw;
	this.x = x;
	this.y = y;
	this.width = width;
	this.minHeight = height;

	this.inputs = [];
	this.outputs = [];
	this.selected = false;
	this.style = style;
	this.updateStyle();
	this.dirty = false;
	//this.customSockets = false;

	Object.defineProperty(this, "name", 
	{
		set: function (x) 
		{
			this._name = x;
			if (this.panel)
				this.panel.update();
		},
		get: function () 
		{
			return this._name;
		}
		//enumerable: true,
		//configurable: true
	});
	
	Object.defineProperty(this, "layout", 
	{
		set: function (x) 
		{
			if (this._layout === x)
				return;  // nothing to change
			
			this._layout = x;
			this.updateLayout();
			this.panel && this.panel.update();
		},
		get: function () 
		{
			return this._layout;
		}
	});
	
	this.name = name;
	
	this.layout = (this.def && this.def.layout) || ['io'];
}

GraphNode.prototype = 
{
	toJSON: function()
	{
		var jsonObj;
		
		jsonObj = { name : this.name, x : this.x, y : this.y, width : this.width, height : this.height,
			/*inputs: arrayToJSON(this.inputs), outputs: arrayToJSON(this.outputs)*/ };
		
		if (this.typeName)
			jsonObj.typeName = this.typeName;
		
		if (this.customStyle)
			jsonObj.style = this.customStyle;
		
		if (this.data)
			jsonObj.data = copyJSON(this.data);
		
		/*	{ typeName : this.typeName, x : this.x, y : this.y };
			if (this.name != this.panel.nodeTypes[this.typeName].name)
				jsonObj.name = this.name;
			if (this.customSockets)
			{
				jsonObj.inputs = arrayToJSON(this.inputs);
				jsonObj.outputs = arrayToJSON(this.outputs);
			}
		}
		else
			jsonObj = { name : this.name, x : this.x, y : this.y, width : this.width, height : this.height, 
				inputs: arrayToJSON(this.inputs), outputs: arrayToJSON(this.outputs) };
		*/
		
		return jsonObj;
	},
	
	getMenu : function()
	{
		return this.def && this.def.menu;
	},
	
	updateStyle: function()
	{
		this.styleSelected = mixStyles(this.style.selected, this.style);
	},
	
	addInput: function(socketName, dataType, label, def)
	{
		this.inputs.push(new GraphSocket(socketName, label, this, "input", dataType, def));
		
		this.dirty = true;
		//this.customSockets = true;
		this.panel.update();
	},
	
	addOutput: function(socketName, dataType, label, def)
	{
		this.outputs.push(new GraphSocket(socketName, label, this, "output", dataType, def));
		
		this.dirty = true;
		//this.customSockets = true;
		this.panel.update();
	},
	
	delInput: function(socket)
	{
		var index;
		
		if (socket instanceof GraphSocket)
			index = this.getInputIndex(socket);
		else
		{
			index = socket;
			socket = this.inputs[index];
		}
		
		if (index >= 0)
		{
			var linkIdx = this.panel.getLinkIndex(socket);
			if (linkIdx >= 0)
				this.panel.deleteLinkByIndex(linkIdx);
			
			this.inputs.splice(index, 1);
			
			this.dirty = true;
			//this.customSockets = true;
			this.panel.update();
		}
	},
	
	delOutput: function(socket)
	{
		var index;
		
		if (socket instanceof GraphSocket)
			index = this.getOutputIndex(socket);
		else
		{
			index = socket;
			socket = this.outputs[index];
		}

		if (index >= 0)
		{
			var linkIdx = this.panel.getLinkIndex(socket);
			if (linkIdx >= 0)
				this.panel.deleteLinkByIndex(linkIdx);
			
			this.outputs.splice(index, 1);
			
			this.dirty = true;
			//this.customSockets = true;
			this.panel.update();
		}
	},
	
	getInputIndex: function(socket)
	{
		return this.inputs.indexOf(socket);
	},
	
	getOutputIndex: function(socket)
	{
		return this.outputs.indexOf(socket);
	},
	
	moveTo: function(point)
	{
		this.x = point.x;
		this.y = point.y;
	},
	
	moveDelta: function(point)
	{
		this.x += point.x;
		this.y += point.y;
	},
	
	hitRectangle: function()
	{
		return new Rectangle(this.x, this.y, this.width, this.height).inflate(this.getStyle().socket.radius);
	},
	
	getSocketHit: function(point)
	{
		for(var idx in this.inputs)
		{
			var socket = this.inputs[idx]
			if (socket.getHitArea().contain(point))
				return socket;
		}
		
		for(var idx in this.outputs)
		{
			var socket = this.outputs[idx]
			if (socket.getHitArea().contain(point))
				return socket;
		}
	},
	
	getStyle: function()
	{
		return this.selected ? this.styleSelected : this.style;
	},

	update: function()
	{
		for(var idx in this.inputs)
		{
			var input = this.inputs[idx];
			if (input.isGeneric)
			{	// in generic sockets, update the dataType based on the linked socket
				var outputSocket = this.panel.getLinkedOutput(input);
				//input.setGenericType(outputSocket ? outputSocket.dataType : '*');
				// the array is to indicate generic type
				input.dataType = [(outputSocket ? outputSocket.dataType : '*')];
			}
		}

		if (this._onUpdate)
		{
			this._onUpdate(this);
		}
		this.panel.update();
	},
	
	updateLayout: function()
	{
		var style = this.getStyle();
		var topMargin = getNodeTopMargin(style);

		y = topMargin;
		
		this.layout.forEach(function(item)
		{
			if (typeof item == 'string')
			{
				var inputMaxY = y;
				var outputMaxY = y;
				
				if (item.indexOf('i') >= 0)
					this.inputs.forEach(function(input, index)
					{
						var rc = new Rectangle(5, inputMaxY, this.width/2, getSocketHeight(this.style.socket));
						input.rc = rc;
						inputMaxY = rc.bottom() + this.style.socket.vmargin;
					}, this);

				if (item.indexOf('o') >= 0)
					this.outputs.forEach(function(output, index)
					{
						var rc = new Rectangle(5, outputMaxY, this.width/2, getSocketHeight(this.style.socket));
						output.rc = rc;
						outputMaxY = rc.bottom() + this.style.socket.vmargin;
					}, this);
				
				if (item == 'i')
					y = inputMaxY;
				else if (item == 'o')
					y = outputMaxY;
				else if (item == 'io')
					y = Math.max(inputMaxY, outputMaxY);
			}
			else if (typeof item == 'number')
				y += item;
				
		}, this);
		
		this.height = y + getNodeBottomMargin(style);

		if (this.minHeight && this.height < this.minHeight)
			this.height = this.minHeight;
	},
	
	draw: function(context)
	{
		if (this.height == undefined || this.dirty)
		{
			this.updateLayout();
			this.dirty = false;
		}
		
		var style = this.getStyle();
		
		// visibility test
		var thisRect = new Rectangle(this.x, this.y, this.width, this.height);
		if (!thisRect.inflate(style.socket.radius).intersectsWith(this.panel.viewRect))
			return;

		if (this.onPreDraw && this.onPreDraw(context) === false)
			return;

		var cornerRadius = style.cornerRadius;
		// limit corner radius to reasonable values
		if (cornerRadius > style.headerSize && style.headerSize > 0)
			cornerRadius = style.headerSize;
		if (cornerRadius*2 > this.width)
			cornerRadius = this.width/2;
		
		drawRoundRect(context, style.outlineColor, this.x, this.y, this.width, this.height, cornerRadius, style.lineWidth);

		context.save();
		context.clip();
		
		context.fillStyle = style.fillColor;
		context.fillRect(this.x, this.y, this.width, this.height);
		
		if (style.headerSize > 0)
		{
			context.textBaseline = "top";
			context.fillStyle = style.headerFillColor;
			context.fillRect(this.x, this.y, this.width, style.headerSize);
		}
		
		/*var image = document.getElementById("imgValid");
		if (image.width > 0 && image.height > 0)
		{
			var scalex = this.width / image.width;
			var scaley = (this.height - style.headerSize) / image.height;
			var scale = scalex <= scaley ? scalex : scaley;
			context.drawImage(image, 
				this.x + (this.width - image.width*scale)/2, this.y + style.headerSize, 
				image.width*scale, image.height*scale);
		}*/
			
		if (this.panel.zoom >= 0.5 && style.headerSize > 0)
		{	// the header text
			context.font = style.headerFont;
			context.fillStyle = style.headerTextColor;
			var leftMargin = cornerRadius > 4 ? cornerRadius : 4;
			var textWidth = this.width - cornerRadius*2;
			context.fillText(this.name, this.x + leftMargin, this.y + 4.0, textWidth <= 0 ? 1 : textWidth);
		}
		
		context.restore();
		
		// paint the border again
		context.stroke();
		
		this.inputs.forEach( function(socket) { socket.draw(context, this.panel) }, this);
		this.outputs.forEach( function(socket) { socket.draw(context, this.panel) }, this);
		
		if (this.onPostDraw)
			this.onPostDraw(context);
	}
}

//---------------------------------------------------------------------------------------------------
// GraphPanel
//---------------------------------------------------------------------------------------------------

function GraphPanel(canvas, graphStyle)
{
	this.nodes = [];
	this.links = [];
	this.options = 
	{
		zoomFromCursor : true,
		editable : true,
		editLinks : true,
		editNodes : true,
		scrollAndZoom : true,
		selectable : true
	}
	
	this.style = mixStyles(graphStyle, getDefaultGraphStyle());
	this.dataTypes = undefined;
	this.nodeTypes = undefined;
	this.zoom = 1.0;
	this.originPanelPt = new Point(0, 0);
	this.mouseMoving = false;
	this.selectedNodes = [];
	//this.inputHelper = document.getElementById("input1");

	this.dragElem = undefined;
	
	this.attachToCanvas(canvas);
	
	this.onContextMenu = undefined;
	this.onHideContextMenu = undefined;
	this.onNodeSelected = undefined;
	this.onChange = undefined;
}

var KEY_HOME = 36;
var KEY_LEFT = 37;
var KEY_UP = 38;
var KEY_RIGHT = 39;
var KEY_DOWN = 40;
var KEY_DEL = 46;
var KEY_PLUS = 107;
var KEY_MINUS = 109;
var KEY_a = 65;
var KEY_z = 90;

GraphPanel.prototype = 
{
	attachToCanvas: function(canvas)
	{
		if (!canvas) return;
		
		this.canvas = canvas;
		this.canvas.style.cursor = 'default';
		this.windowPt = getElementPosition(canvas);
		
		var context = canvas.getContext("2d");
		
		// set the events
		var thisPanel = this;
		canvas.onmousedown = function(event) { thisPanel.onMouseDown(event) };
		canvas.onmouseup = function(event) { thisPanel.onMouseUp(event) };
		canvas.onmousemove = function(event) { thisPanel.onMouseMove(event) };
		canvas.onwheel = function(event) { return thisPanel.onMouseWheel(event) };
		canvas.onmousewheel = function(event) { return thisPanel.onMouseWheel(event) }; //for IE
		canvas.oncontextmenu = function(event) { return false };
		
        /*
		this.inputHandler = document.createElement('input');
		this.inputHandler.style.cssText = "position:absolute; top:-1000px; left:-1000px; width:0px; height:0px";
		//this.inputHandler.onkeypress = function(event) 
		this.inputHandler.onkeydown = function(event) 
		{ 
			var key = (event.keyCode >= KEY_a && event.keyCode <= KEY_z) ? String.fromCharCode(event.keyCode).toLowerCase() : '';
			console.log('keyCode: ' + event.keyCode + ' key: ' + key);
			if (event.keyCode == KEY_HOME || key == 'f')
				thisPanel.fit();
			else if (event.keyCode == KEY_DEL)
				thisPanel.delSelection();
			else if (event.keyCode >= KEY_LEFT && event.keyCode <= KEY_DOWN)
			{
				var nodeSelected = false;
				var delta = [
					new Point(-1,0), new Point(0,-1), new Point(1,0), new Point(0,1)
				][event.keyCode-KEY_LEFT];
				
				thisPanel.nodes.forEach(function(node)
				{
					if (node.selected)
					{
						nodeSelected = true;
						node.x += delta.x;
						node.y += delta.y;
					}
				});
				
				if (!nodeSelected)
					thisPanel.scroll(delta.x*5, delta.y*5);
				
				thisPanel.invalidate();
			}
			else if (event.keyCode == KEY_MINUS)
				thisPanel.zoomOut();
			else if (event.keyCode == KEY_PLUS)
				thisPanel.zoomIn();
		};
		
		canvas.parentNode.insertBefore(this.inputHandler, canvas);
		this.inputHandler.focus();
        */
		this.updateTransform();
	},
	
	clientToPanel: function (clientPt)
	{
		return new Point((1.0*clientPt.x)/this.zoom + this.originPanelPt.x, 
			(1.0*clientPt.y)/this.zoom + this.originPanelPt.y);
	},

	panelToClient: function(panelPt)
	{
		return new Point((panelPt.x - this.originPanelPt.x)*this.zoom, 
			(panelPt.y - this.originPanelPt.y)*this.zoom);
	},

	newNode: function(typeName, x, y, customStyle)
	{
		var jsonpar;
		var width = 100;
		var height = undefined;
		var nodeName = '';
		
		if (arguments.length == 1 && typeof typeName == 'object')
		{
			jsonpar = typeName;
			typeName = jsonpar.typeName || undefined;
		}
		
		var nodeDef;
		if (typeName)
		{
			nodeDef = this.nodeTypes && this.nodeTypes[typeName];
			if (!nodeDef)
			{
				console.error('Error: unknown node type: "' + typeName + '"');
				return;
			}
			
			nodeName = nodeDef.name || nodeName;
			width = nodeDef.width || width;
			height = nodeDef.height || height;
		}

		if (jsonpar)
		{
			x = jsonpar.x;
			y = jsonpar.y;
			nodeName = jsonpar.name || nodeName;
			width = jsonpar.width || width;
			height = jsonpar.height || height;
			customStyle = jsonpar.style;
		}
		
		var style;
		if (nodeDef)
			style = mixStyles(customStyle, nodeDef.style, this.style.node);
		else
			style = mixStyles(customStyle, this.style.node);
		
		if (x === undefined)
			x = this.viewRect.center().x - width/2;
		var yUndefined = false;
		if (y === undefined)
		{
			y = this.viewRect.center().y - (height/2 || 0);
			yUndefined = true;
		}
			
		var node = new GraphNode(this, typeName, nodeName, x, y, width, height, style);
		
		node.typeName = typeName;
		node.customStyle = customStyle;
		node.data = copyJSON((jsonpar && jsonpar.data) || (nodeDef && nodeDef.data));
		
		var inputSockets = (jsonpar && jsonpar.inputs) || (nodeDef && nodeDef.inputs) || [];
		var outputSockets = (jsonpar && jsonpar.outputs) || (nodeDef && nodeDef.outputs) || [];
		
		inputSockets.forEach( function(socketDef) 
		{
			node.addInput(socketDef.name, socketDef.dataType, socketDef.label, socketDef);
		});
				
		outputSockets.forEach( function(socketDef) 
		{
			node.addOutput(socketDef.name, socketDef.dataType, socketDef.label, socketDef);
		});
		
		//node.customSockets = !nodeDef || nodeDef.inputs !== inputSockets || nodeDef.outputs !== outputSockets;
		
		node.updateLayout();
		
		if (yUndefined)
			node.y = this.viewRect.center().y - node.height/2;
		
		this.nodes.push(node);
		
		return node;
	},

	// set origin and zoom so all nodes fits nicely inside the canvas
	fit: function()
	{
		var rc; // enclosing rectangle of all nodes
		this.nodes.forEach( function(node)
		{
			if (!rc)
				rc = node.hitRectangle();
			else
				rc = rc.union(node.hitRectangle());
		});
		
		if (rc)
		{
			rc = rc.zoom(1.07); // extra margin
			this.originPanelPt.x = rc.x;
			this.originPanelPt.y = rc.y;
			var scaleX = this.canvas.width / rc.width;
			var scaleY = this.canvas.height / rc.height;

			if (scaleX !== 0 && scaleY !== 0)
			{
			    if (scaleX < scaleY)
			        this.zoom = Math.min(GRAPH_MAX_ZOOM, scaleX);
			    else
			        this.zoom = Math.min(GRAPH_MAX_ZOOM, scaleY);
			}
			
			this.updateTransform();
			
			this.originPanelPt.x = rc.center().x - this.viewRect.width/2;
			this.originPanelPt.y = rc.center().y - this.viewRect.height/2;
			
			this.updateTransform();
		}
	},
	
	autoLayout: function()
	{
		this.nodes.forEach(function(node) { node._layout = { idxColumn:-1 }});
		
		var firstColumn = [];
		var arrColumns = [firstColumn];
		var thiz = this;
		
		// get the first column
		this.nodes.forEach(function(node)
		{
			if (!node.inputs.some(function(socket) { return socket.linkedOutput }))
			{	// nodes without inputs connected
				node._layout.idxColumn = 0;
				firstColumn.push(node);
			}
		});
		
		firstColumn.sort(function (first, second)
		{
			var firstNumOuputs = 0;
			for(var i in first.outputs)
				firstNumOuputs += thiz.getLinkedInputs(first.outputs[i]).length;
			
			var secondNumOuputs = 0;
			for(var i in second.outputs)
				secondNumOuputs += thiz.getLinkedInputs(second.outputs[i]).length;
			
			if (firstNumOuputs == secondNumOuputs)
				return 0;
			if (firstNumOuputs > secondNumOuputs)
				return -1;
			else
				return 1; 
		});
		
		// determine the column of each node
		var idxColumn = 1;
		
		firstColumn.forEach(function exploreNode(node)
		{
			node.outputs.forEach(function(output)
			{
				thiz.getLinkedInputs(output).forEach(function(input)
				{
					input.parentNode._layout.idxColumn = Math.max(input.parentNode._layout.idxColumn, idxColumn);
					idxColumn++;
					exploreNode(input.parentNode);
					idxColumn--;
				});
			});
		});
		
		// distribute the nodes in the columns
		this.nodes.forEach(function(node) 
		{
			//console.log(node._layout.idxColumn);
			if (node._layout.idxColumn > 0)
			{
				if (!arrColumns[node._layout.idxColumn])
					arrColumns[node._layout.idxColumn] = [];
				arrColumns[node._layout.idxColumn].push(node);
			}
		});
		
		var lastX = 0;
		for(var idxColumn in arrColumns)
		{
			var column = arrColumns[idxColumn];
			var lastY = 0;
			for(var idx in column)
			{
				var node = column[idx];
				node.x = lastX;
				node.y = lastY;
				lastY += node.height + 20;
			}
			
			lastX += 200;
		}
		
		this.nodes.forEach(function(node) { delete node._layout });
		
		this.update();
		
	},
	
	// hittest with nodes and sockets
	hitAll: function(point)
	{
		var rcHitTest;
		if (point instanceof Rectangle)
			rcHitTest = point;
		else	
			rcHitTest = new Rectangle(point.x, point.y, 0, 0);

		// search in reversed order
		for(var idx=this.nodes.length-1; idx>=0; idx--)
		{
			var node = this.nodes[idx];
			if (rcHitTest.intersectsWith(node.hitRectangle()))
			{
				if (this.options.editable && this.options.editLinks)
				{
					var socket = node.getSocketHit(point);
					if (socket)
						return socket;
				}
				
				return node;
			}
		}

		return undefined;
	},
	
	// if callback returns true, no more nodes are processed and the function exit.
	// return the last hitted node
	hitNodes: function(rcHitTest, callback)
	{
		if (rcHitTest instanceof Point)
			rcHitTest = new Rectangle(rcHitTest.x, rcHitTest.y, 0, 0);

		var lastHitNode = undefined;
		
		// search in reversed order
		for(var idx=this.nodes.length-1; idx>=0; idx--)
		{
			var node = this.nodes[idx];
			if (rcHitTest.intersectsWith(node.hitRectangle()))
			{
				lastHitNode = node;
				if (callback(node) === true)
					break;
			}
		}
		
		return lastHitNode;
	},
	
	unselectAll: function()
	{
		this.nodes.forEach( function(node) { node.selected = false; } )
	},
	
	addLink: function(outputSocket, inputSocket)
	{
		this.links.push(new GraphLink(outputSocket, inputSocket));
		
		inputSocket._connectToOutput(outputSocket);
		
		outputSocket.parentNode.update();
		inputSocket.parentNode.update();
	},
	
	// return the output connected to a input socket
	getLinkedOutput: function(inputSocket)
	{
		for(var idx in this.links)
		{
			var link = this.links[idx];
			if (link.inputSocket == inputSocket)
				return link.outputSocket;
		}
	},
	
	// return the array of inputs connected to a output socket
	getLinkedInputs: function(outputSocket)
	{
		var inputs = [];
		
		for(var idx in this.links)
		{
			var link = this.links[idx];
			if (link.outputSocket == outputSocket)
				inputs.push(link.inputSocket);
		}
		
		return inputs;
	},
	
	getLinksBySocket: function(socket)
	{
		var links = [];
		
		this.links.forEach(function(link)
		{
			if (link.inputSocket == socket || link.outputSocket == socket)
				links.push(link);
		}, this);
		
		return links;
	},
	
	// return the index of the link containing a socket
	getLinkIndex: function(socket)
	{
		for(var idx in this.links)
		{
			var link = this.links[idx];
			if (link.inputSocket == socket || link.outputSocket == socket)
				return idx;
		}
		
		return -1;
	},
	
	deleteLink: function(link)
	{
		this.deleteLinkByIndex(this.links.indexOf(link));
	},
	
	deleteLinkByIndex: function(linkIdx)
	{
		if (linkIdx < 0 || linkIdx >= this.links.length)
			return;
		
		var link = this.links[linkIdx];
		
		this.links.splice(linkIdx, 1);
		
		link.inputSocket._connectToOutput(undefined);
		
		link.outputSocket.parentNode.update();
		link.inputSocket.parentNode.update();
	},
	
	getNodeIndex: function(node)
	{
		return this.nodes.indexOf(node);
	},
	
	deleteNode: function(idxNode)
	{
		idxNode = idxNode instanceof GraphNode ? this.getNodeIndex(idxNode) : idxNode;
		var node = this.nodes[idxNode];
		
		// delete all the node links
		for(var idxLink=0; idxLink<this.links.length; idxLink++)
		{
			var link = this.links[idxLink];
			if (link.inputSocket.parentNode == node || link.outputSocket.parentNode == node)
			{
				this.deleteLinkByIndex(idxLink);
				idxLink--;
			}
		}

		this.nodes.splice(idxNode, 1);
		
		this.update();
	},
	
	delSelection: function()
	{
		if (!this.options.editable || !this.options.editNodes)
			return;
		
		for(var idxNode=0; idxNode < this.nodes.length; idxNode++)
		{
			var node = this.nodes[idxNode];
			if (node.selected)
				{
				this.deleteNode(idxNode);
				idxNode--;
				}
		}
	},

	update: function()
	{
		resetCache();
		if (this._changedPendingID == undefined)
		{
			this.invalidate();
			
			var thiz = this;
			this._changedPendingID = setTimeout(function() 
			{ 
				thiz._changedPendingID = undefined;
				
				if (thiz.onChange)
					thiz.onChange(thiz.toJSON())
			}, 0);
		}
	},
	
	zoomIn: function(amount, ptZoomCenter)
	{
		amount = amount || 0.1;
		ptZoomCenter = ptZoomCenter || new Point(this.canvas.width/2, this.canvas.height/2);
		
		var newCenterPanelPt = this.clientToPanel(ptZoomCenter);
		
		this.zoom += amount;
		
		if (this.zoom < 0.1) this.zoom = 0.1;
		if (this.zoom > GRAPH_MAX_ZOOM) this.zoom = GRAPH_MAX_ZOOM;
		
		// ajust origin coordinates
		this.originPanelPt.x = newCenterPanelPt.x - (1.0*ptZoomCenter.x)/this.zoom;
		this.originPanelPt.y = newCenterPanelPt.y - (1.0*ptZoomCenter.y)/this.zoom;
		
		this.updateTransform();
	},
	
	zoomOut: function(amount, ptZoomCenter)
	{
		amount = amount || 0.1;
		this.zoomIn(-amount, ptZoomCenter);
	},

	scroll: function(dx, dy)
	{
		if (!this.options.scrollAndZoom)
			return;
		
		this.originPanelPt.x += dx/this.zoom;
		this.originPanelPt.y += dy/this.zoom;
		
		this.updateTransform();
	},
	
	scrollIfNearBorder: function(clientPt)
	{
		if (!this.options.scrollAndZoom)
			return;
		
		var delta = new Point(0,0);
		if (clientPt.x + GRAPH_SCROLL_MARGIN > this.canvas.width)
		{
			delta.x = clientPt.x + GRAPH_SCROLL_MARGIN - this.canvas.width;
			if (delta.x > 50) delta.x = 50;
		}
		else if (clientPt.x < GRAPH_SCROLL_MARGIN)
		{
			delta.x = clientPt.x - GRAPH_SCROLL_MARGIN;
			if (delta.x < -50) delta.x = -50;
		}
		if (clientPt.y + GRAPH_SCROLL_MARGIN > this.canvas.height)
		{
			delta.y = clientPt.y + GRAPH_SCROLL_MARGIN - this.canvas.height;
			if (delta.y > 50) delta.y = 50;
		}
		else if (clientPt.y < GRAPH_SCROLL_MARGIN)
		{
			delta.y = clientPt.y - GRAPH_SCROLL_MARGIN;
			if (delta.y < -50) delta.y = -50;
		}
		
		if (delta.x != 0 || delta.y != 0)
			this.scroll(delta.x*GRAPH_SCROLL_SPEED, delta.y*GRAPH_SCROLL_SPEED);
	},
	
	updateTransform: function()
	{
		if (!this.canvas) return;
		
		var context = this.canvas.getContext("2d");

		var bottomRightPanelPt = this.clientToPanel(new Point(this.canvas.width, this.canvas.height));
		this.viewRect = new Rectangle(this.originPanelPt, bottomRightPanelPt);
		
		context.setTransform(this.zoom, 0, 0, this.zoom, -this.originPanelPt.x*this.zoom, -this.originPanelPt.y*this.zoom);
		this.invalidate();
	},
	
	invalidate: function()
	{
		if (this._drawPendingID == undefined)
		{
			var thiz = this;
			this._drawPendingID = setTimeout(function() { thiz.draw() }, 0);
		}
	},
	
	draw: function()
	{
		if (this._drawPendingID)
		{	// cancel the pending draws
			clearTimeout(this._drawPendingID);
			this._drawPendingID = undefined;
		}
		
		if (!this.canvas) return;
		//console.log('draw');
		
		var context = this.canvas.getContext("2d");
		
		context.fillStyle = this.style.backgroundColor;
		var pt1 = this.clientToPanel(new Point(0, 0));
		var pt2 = this.clientToPanel(new Point(this.canvas.width, this.canvas.height));
		context.fillRect(pt1.x, pt1.y, pt2.x-pt1.x, pt2.y-pt1.y);
		
		var graphPanel = this;
		
		this.links.forEach( function(link) { link.draw(context, graphPanel); } );
		
		var newSelectedNodes = [];
		this.nodes.forEach( function(node) 
		{
			if (node.selected)
				newSelectedNodes.push(node);
			
			try 
			{
				node.draw(context); 
			}
			catch(e)
			{
				console.error('Drawing error: ' + e.toString());
				context.restore();
			}
		} );
		
		if (this.dragElem instanceof GraphDragLink)
			this.dragElem.draw(context, graphPanel);
			
		if (this.selRectangle)
			drawRect(context, "lightblue", this.selRectangle);
		
		// determine if the selection has changed
		var selectionChanged = this.selectedNodes.length != newSelectedNodes.length;
		if (!selectionChanged)
			for(var idx in this.selectedNodes)
				if (newSelectedNodes.indexOf(this.selectedNodes[idx]) < 0)
				{
					selectionChanged = true;
					break;
				}
		
		if (selectionChanged)
		{
			this.selectedNodes = newSelectedNodes;
			if (this.onNodeSelected)
				this.onNodeSelected(this.selectedNodes);
		}
		
		//var clientPt = this.panelToClient(new Point(this.nodes[0].x, this.nodes[0].y));
		//inputHelper.style.left = this.windowPt.x + clientPt.x;
		//inputHelper.style.top = this.windowPt.y + clientPt.y;
		//inputHelper.style.transform = "scale(" + this.zoom + ")";
			
	},	
	
	onMouseDown: function(event)
	{
		this.hideContextMenu();
		//this.inputHandler.focus();
		
		var clientPt = new Point(event.offsetX, event.offsetY);
		var panelPt = this.clientToPanel(clientPt);
		this.mouseDownClientPt = clientPt;
		this.mouseDownPanelPt = panelPt;
		this.lastDragPanelPt = panelPt;
		
		this.mouseMoving = false;
		this.dragElem = undefined;
		
		if (event.buttons == 1)
		{	// left button
			var hitElem = this.hitAll(panelPt);
			if (hitElem instanceof GraphNode)
			{
				this.dragElem = hitElem;
				
				if (this.options.selectable)
				{
					if (!event.ctrlKey && !this.dragElem.selected)
						this.unselectAll();
						
					if (!event.ctrlKey)
						this.dragElem.selected = true;
				}
			}
			else if (hitElem instanceof GraphSocket)
			{
				var socket = hitElem;
				if (socket.socketDir == "input")
				{
					// if it's a input socket and it has a link, we have to delete the link and create a new link
					// between the output and the dragging point
					for(var idx in this.links)
					{
						var link = this.links[idx];
						if (link.inputSocket == socket)
						{	
							this.dragElem = new GraphDragLink(link.outputSocket, panelPt);
							this.deleteLinkByIndex(idx);
							break;
						}
					}
					
				}
					
				if (this.dragElem == undefined)
					this.dragElem = new GraphDragLink(socket, panelPt);
			}
			else if (!event.ctrlKey && this.options.selectable)
				this.unselectAll();
		
			this.draw();
		}
	},

	onMouseUp: function(event)
	{
		if (this.canvas.releaseCapture)
			this.canvas.releaseCapture();
		//this.inputHandler.focus();
		
		if (this.selRectangle)
		{
			this.selRectangle = undefined;
			this.draw();
		}
		else if (this.dragElem instanceof GraphNode)
		{
			if (this.mouseMoving == false)
			{	
				if (this.options.selectable)
				{
					// update the node selection
					if (event.ctrlKey)
					{
						if (this.dragElem.selected)
							this.dragElem.selected = false;
						else
							this.dragElem.selected = true;
					}
					else
					{
						this.unselectAll();
						this.dragElem.selected = true;
					}
					
					this.draw();
				}
			}
			else
			{	// a node was moved
				if (this.onChange)
					this.onChange(this.toJSON());
			}
				
		}
		else if (this.dragElem instanceof GraphDragLink)
		{	
			var panelPt = this.clientToPanel(new Point(event.offsetX, event.offsetY));
			var hitElem = this.hitAll(panelPt);
			if (hitElem instanceof GraphSocket)
			{	// create a new link
				var socket = hitElem;
				var typesMatch = socket.isGeneric || socket.dataType == this.dragElem.socket.dataType;
				//var typesMatch = socket.dataType == '*' || socket.dataType == this.dragElem.socket.dataType;
				if (this.dragElem.socket.socketDir == "input")
				{
					if (socket.socketDir == "output" && typesMatch)
						this.addLink(socket, this.dragElem.socket);
				}
				else if (socket.socketDir == "input" && typesMatch)
				{
					// if the input socket had a link, we have to delete it
					var idx = this.getLinkIndex(socket);
					if (idx >= 0)
						this.deleteLinkByIndex(idx);
					this.addLink(this.dragElem.socket, socket);
				}
			}
			
			this.dragElem = undefined;
			
			// at this point, a link was added or deleted (including the case of deleting the link being dragged)
			if (this.onChange)
				this.onChange(this.toJSON());
						
			this.draw();
		}
		
		if (!this.mouseMoving && event.button == 2)
		{	// show the context menu
			var panelPt = this.clientToPanel(new Point(event.offsetX, event.offsetY));
			//var hitElem = this.hitNodes(panelPt, function(node) { return true; });
			var hitElem = this.hitAll(panelPt);
			var hitSocket = hitElem instanceof GraphSocket ? hitElem : undefined;
			var hitNode = hitSocket ? hitSocket.parentNode : hitElem;
			if (hitNode && !hitNode.selected)
			{
				this.unselectAll();
				hitNode.selected = true;
				this.draw(); // to force selection update
				//this.invalidate();
			}
			
			this.showContextMenu(event, hitNode, hitSocket, panelPt);
			//if (this.onChange)
			//	this.onChange(this.toJSON());
		}
		
		this.mouseMoving = false;
		this.dragElem = undefined;
	},

	onMouseMove: function(event)
	{
		var clientPt = new Point(event.offsetX, event.offsetY);
		var panelPt = this.clientToPanel(clientPt);
		
		this.mouseMoving = (event.buttons != 0) && (this.mouseMoving || this.mouseDownClientPt.delta(clientPt) > 5);
		
		if (this.dragElem && this.mouseMoving)
		{ // moving an element (node or link)
			if (this.canvas.setCapture)
				this.canvas.setCapture(true);
			
			if (this.dragElem instanceof GraphDragLink)
			{
				var hitElem = this.hitAll(panelPt);
				if (hitElem instanceof GraphSocket)
					// snap effect toward the sockets
					this.dragElem.moveTo(hitElem.getHitArea().center());
				else
					this.dragElem.moveTo(panelPt);
			}
			else if (this.options.selectable)
			{	// move the selected nodes
				this.dragElem.selected = true;
				
				// scroll when near the boundary of the canvas
				this.scrollIfNearBorder(clientPt);

				if (this.options.editable && this.options.editNodes)
				{
					var deltaPt = new Point(panelPt.x - this.lastDragPanelPt.x, panelPt.y - this.lastDragPanelPt.y);
					this.nodes.forEach(function(node)
					{
						if (node.selected)
							node.moveDelta(deltaPt);
					}, this);
				}
			}
			
			this.draw();
		}
		
		this.lastDragPanelPt = panelPt;
		
		if (event.buttons == 1 && this.dragElem == undefined && this.mouseMoving && this.options.selectable)
		{	// handle the selecting rectangle
			if (this.canvas.setCapture)
				this.canvas.setCapture(true);

			// scroll when near the boundary of the canvas
			this.scrollIfNearBorder(clientPt);
			
			// deselect all the nodes, except when the control key is pressed
			this.nodes.forEach(function(node)
			{
				if (!this.selRectangle)
				{
					node.wasSelected = node.selected;
					if (!event.ctrlKey)
						node.selected = false;
				}
				else
					node.selected = node.wasSelected;
				
			}, this);

			this.selRectangle = new Rectangle(this.mouseDownPanelPt, this.lastDragPanelPt);
			this.hitNodes(this.selRectangle, function(node)
			{
				node.selected = true;
			});
			
			this.draw();
		}
		else if (event.buttons == 2)
		{	// scroll when dragging the right mouse button
			if (this.mouseMoving && this.options.scrollAndZoom)
			{
				this.originPanelPt.x += this.mouseDownPanelPt.x - panelPt.x;
				this.originPanelPt.y += this.mouseDownPanelPt.y - panelPt.y;
				
				this.updateTransform();
			}
		}
	},

	onMouseWheel: function(event)
	{
		if (!this.options.scrollAndZoom)
			return;
		
		var amount = ((event.wheelDelta === undefined ? event.deltaY : -event.wheelDelta) > 0) ? -0.1 : 0.1;
		
		if (this.options.zoomFromCursor)
			this.zoomIn(amount, new Point(event.offsetX, event.offsetY));
		else
			this.zoomIn(amount);
		
		return false;
	},
	
	showContextMenu: function(event, node, socket, panelPt)
	{
		if (this.onContextMenu)
			this.onContextMenu(event, node, socket, panelPt);
		else 
		{
			var params = { 'panel': this, 'node': node, 'socket': socket };
			var menuSocket = $$menu(socket && (socket.getMenu() || this.options.menuSocket), node, params);
			var menuNode = node && concatMenu(
					$$menu(this.options.menuNode, node, params), 
					$$menu(node.getMenu(), node, params));
			var menu = menuSocket || menuNode || $$menu(this.options.menuGraph, this, params);
			/*var menuItems = (socket && (socket.getMenu() || this.options.menuSocket)) 
				|| (node && (node.getMenu() || this.options.menuNode)) 
				|| this.options.menuGraph;*/
			
			if (menu)
			{
				//menu = $$menu(menu, this, params);
				var clickPt = new Point(event.pageX, event.pageY);
				this._showContextMenu(clickPt, node, socket, panelPt, menu, 0);
			}
		}
	},
	
	_showContextMenu: function(clickPt, node, socket, panelPt, parentItem, nestLevel, parentOnClick)
	{
		var menuDiv = document.createElement('div');
		this.menuDivList.push(menuDiv);
		menuDiv.className = 'wn-menu';
		menuDiv.oncontextmenu = function() { return false };
		menuDiv.onmousedown = function() { return false } // to avoid text selection
		var thiz = this;
		var menuItems = parentItem.items;
		menuItems.forEach(function(item)
		{
			var itemText = item.label ? item.label : item;
			
			if (itemText == '-')
			{	// a separator
				var div = document.createElement('div');
				//div.style.height = '0px';
				//div.style.borderTop = 'solid 1px';
				
				div.className = 'wn-menuSeparator';
				menuDiv.appendChild(div);
			}
			else
			{	// a normal menu item
				var div = document.createElement('div');
				
				div.className = item.disabled == true ? 'wn-menuItemDisabled' : 'wn-menuItem';
				div.innerHTML = '<table cellPadding="0px" cellSpacing="0px" style="width:100%"><tr><td></td>'
					+ '<td style="font-size:10px;padding-left:10px;text-align:right">'
					+ (item.items ? String.fromCharCode(0x25ba)/*'►'*/ : '') 
					+ '</td></tr></table>';
				var td = div.querySelector('table tr td');
				var tn = document.createTextNode(itemText)
				td.appendChild(tn);
				
				var onClick = item.onClick || parentItem.onClick || parentOnClick; 
				if (!item.disabled && !item.items)
					div.onclick = function() 
					{
						thiz.hideContextMenu();
						if (onClick)
						{
							var param = { 'panel': thiz, 'node': node, 'socket': socket, 'item': item, 'pt': panelPt };
							onClick.call(thiz, param);
						}
					};
				
				div.onmouseenter = function() 
				{
					if (thiz.onMenuMouseEnterID)
						clearTimeout(thiz.onMenuMouseEnterID);
					thiz.onMenuMouseEnterID = setTimeout(function()
					{
						// hide all submenus
						thiz._hideContextMenu(nestLevel+1);
						
						if (item.items && !item.disabled)
						{	// show the item submenu if it's not disabled
							var newPt = new Point(clickPt.x + div.offsetLeft + div.offsetWidth, clickPt.y + div.offsetTop);
							thiz._showContextMenu(newPt, node, socket, panelPt, item, nestLevel+1, onClick);
						}
					}, 200);
				}
				
				menuDiv.appendChild(div);
			}
			
		});
		
		menuDiv.style.left = clickPt.x + 'px';
		menuDiv.style.top = clickPt.y + 'px';
		menuDiv.panelPt = panelPt;
		this.canvas.parentNode.appendChild(menuDiv);
	},
	
	_hideContextMenu: function(nestLevel)
	{
		if (this.menuDivList)
			this.menuDivList = this.menuDivList.filter(function(menuDiv, index)
			{
				if (index >= nestLevel)
				{
					this.canvas.parentNode.removeChild(menuDiv);
					return false;
				}
				
				return true;
			}, this)
		else
			this.menuDivList = [];
		
	},
	
	hideContextMenu: function()
	{
		if (this.onHideContextMenu)
			this.onHideContextMenu();
		else 
			this._hideContextMenu(0);
	},
	
	onKeyPress: function(event)
	{
		//console.log(event.keyCode);
	},
	
	newGraph: function()
	{
		this.nodes = [];
		this.links = [];
		
		this.zoom = 1.0;
		this.originPanelPt = new Point(0, 0);
		this.updateTransform();
		this.update();
	},
	
	toJSON: function()
	{
		var result = 
		{ 
			nodes: arrayToJSON(this.nodes),
			links: arrayToJSON(this.links)
			//originPanelPt: JSON.stringify(this.originPanelPt),
			//zoom: this.zoom,
		};
			
		return result;
	},
	
	fromJSON: function(json)
	{
		this.newGraph();
		if (!json || !json.nodes || !json.links)
			return;
		
		var thiz = this;
		
		json.nodes.forEach(function(jsonNode)
		{
			thiz.newNode(jsonNode);
		});
		
		//this.links = json.links.map(function(link)
		json.links.forEach(function(link)
		{
			var outputNode = thiz.nodes[link.outputNode];
			var inputNode = thiz.nodes[link.inputNode];
			var outputSocket = outputNode && outputNode.outputs && link.outputSocket && outputNode.outputs[link.outputSocket];
			var inputSocket = inputNode && inputNode.inputs && link.inputSocket && inputNode.inputs[link.inputSocket];
			if (outputSocket !== undefined && inputSocket !== undefined)
			{
				//link = new GraphLink(outputNode.outputs[link.outputSocket], inputNode.inputs[link.inputSocket]);
				thiz.addLink(outputNode.outputs[link.outputSocket], inputNode.inputs[link.inputSocket]);
			}
			else
			{
				console.error('Error: link references an non existent socket');
				//link = null;
			}
			
			//return link;
		});
		
		this.fit();
	},
	
	save: function(storageKey)
	{
		var json = JSON.stringify(this.toJSON());
		
		localStorage[storageKey] = json;
	},
	
	load: function(storageKey)
	{
		var jsonText = localStorage[storageKey];
		if (jsonText == undefined || jsonText == '')
		{
			alert("Error: storageKey '" + storageKey + "' not found");
			return;
		}
		
		this.fromJSON(JSON.parse(jsonText));
		this.update();
	},
	
	"export": function()
	{
		var json = this.toJSON();
		
		//var myBlob = new Blob([json], { type: 'application/json' });
		var myBlob = new Blob([json], { type: 'binary/json' });

		window.open(window.URL.createObjectURL(myBlob));
	},
	
	"import": function(file)
	{
		var reader = new FileReader();
		var thiz = this;
		reader.onloadend = function(evt) 
		{
			if (evt.target.readyState == FileReader.DONE)
				thiz.fromJSON(evt.target.result);
		}

		reader.onerror = function(evt) 
		{
			alert('error reading file');
		}

		reader.readAsText(file); 
	}
	
}

var style = document.createElement('style');
style.innerHTML = '.wn-menu{ position:absolute; border: solid 1px black; background-color: #FDFDFD; cursor:default; padding: 1px; min-width: 90px; font: normal 10pt Arial}'
	+ '.wn-menuItem, .wn-menuItemDisabled{ border: solid 1px #FDFDFD; padding-left: 15px; padding-top: 0px; padding-bottom: 0px; margin: 0px; }'
	+ '.wn-menuItem:hover{ border: solid 1px #7DA8F6; background-color: #BDE8E6; }'
	+ '.wn-menuItemDisabled{ color: #B0B0B0; }'
	+ '.wn-menuItemDisabled:hover{ border: solid 1px #B8B8B8; background-color: #E8E8E8; }'
	+ '.wn-menuSeparator{ color: #C0C0C0; height: 0px; border-top: solid 1px; margin-left: 2px; margin-right: 2px; margin-top: 2px; margin-bottom: 2px; }';
document.head.insertBefore(style, document.head.firstChild);

})();