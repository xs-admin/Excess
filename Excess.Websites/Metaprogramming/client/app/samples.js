
var Samples =
{

MetaProgrammingSamples : [
//array sample
"" + '\n' +
"//demonstrates javascript-like arrays," + '\n' +
"//which c# should have anyway." + '\n' +
"" + '\n' +
"class Demo" + '\n' +
"{" + '\n' +
"   public static void Start()" + '\n' +
"   {" + '\n' +
"       var intArray = [1, 2, 3];" + '\n' +
"   }" + '\n' +
"}",

//match example
"" + '\n' +
"//demonstrates a match statement" + '\n' +
"//where case clauses are general conditions" + '\n' +
"class Demo" + '\n' +
"{" + '\n' +
"    public static void Start()" + '\n' +
"    {" + '\n' +
"        match(x)" + '\n' +
"        {" + '\n' +
"             case 0:" + '\n' +
"             case 1:" + '\n' +
"                 Console.Write(\"Zero or One\");" + '\n' +
"                 break;" + '\n' +
"             case > 5: " + '\n' +
"                 Console.Write(\"Greater than 5\");" + '\n' +
"                 break;" + '\n' +
"             case < y: " + '\n' +
"                 Console.Write(\"Less than y\");" + '\n' +
"                 break;" + '\n' +
"             default:" + '\n' +
"                Console.Write(\"Otherwise\");" + '\n' +
"        }" + '\n' +
"    }" + '\n' +
"}",
],

MetaProgrammingResults: [
//array sample
"using System;" + '\n' + 
"using System.Collections.Generic;" + '\n' + 
"using System.Linq;" + '\n' + 
"" + '\n' + 
"//demonstrates javascript-like arrays," + '\n' + 
"//which c# should have anyway." + '\n' + 
"class Demo" + '\n' + 
"{" + '\n' + 
"    public static void Start()" + '\n' + 
"    {" + '\n' + 
"        var intArray = new[]{1, 2, 3};" + '\n' + 
"    }" + '\n' + 
"}",

//match example
"using System;                                  " + '\n' +
"using System.Collections.Generic;              " + '\n' +
"using System.Linq;                             " + '\n' +
"                                               " + '\n' +
"//demonstrates a match statement               " + '\n' +
"//where case clauses are general conditions    " + '\n' +
"class Demo                                     " + '\n' +
"{                                              " + '\n' +
"    public static void Start()                 " + '\n' +
"    {                                          " + '\n' +
"        if (x == 0 || x == 1)                  " + '\n' +
"        {                                      " + '\n' +
"            Console.Write(\"Zero or One\");    " + '\n' +
"            break;                             " + '\n' +
"        }                                      " + '\n' +
"        else if (x > 5)                        " + '\n' +
"        {                                      " + '\n' +
"            Console.Write(\"Greater than 5\"); " + '\n' +
"            break;                             " + '\n' +
"        }                                      " + '\n' +
"        else if (x < y)                        " + '\n' +
"        {                                      " + '\n' +
"            Console.Write(\"Less than y\");    " + '\n' +
"            break;                             " + '\n' +
"        }                                      " + '\n' +
"        else                                   " + '\n' +
"        {                                      " + '\n' +
"            Console.Write(\"Otherwise\");      " + '\n' +
"            break;                             " + '\n' +
"        }                                      " + '\n' +
"    }                                          " + '\n' +
"}                                              ",

],

DataProgrammingDataTypes: {
    number : 
    {
        color: "yellow"
    },
    bool : 
    {
        color: "blue"
    },
    flow : 
    {
        color: "green"
    },
},

getTypeOf: function(value)
{
	if (value[0] == '"' || value[0] == "'")
        return 'string';
    else if (value == 'true' || value == 'false')
        return 'bool';
    else if (!isNaN(value))
        return 'number';
    else
		return 'string';
},

DataProgrammingNodeTypes: {
    parameter:
    {
        name: "parameter",
        width: 100,

        inputs:
        [
        ],
        outputs:
        [
            {
                name: "output",
                dataType: "number",
                label: function () { return "value" },
            },
        ],
    },

    result:
    {
        name: "result",
        width: 100,
        inputs:
        [
            { name: "previous", dataType: "flow" },
            {name: "value", dataType: "*" },
        ],
        outputs:
        [
        ]
    },

    sum:
    {
        name: "sum",
        data: '+',
        width: 100,
        inputs:
        [
            { name: "previous", dataType: "flow" },
            { name: "left", dataType: "number" },
            { name: "right", dataType: "number" },
        ],
        outputs:
        [
            { name: "next", dataType: "flow" },
            { name: "result", dataType: "number" },
        ]
    },

    sub:
    {
        name: "substract",
        data: '-',
        width: 100,
        inputs:
        [
            { name: "previous", dataType: "flow" },
            { name: "left", dataType: "number" },
            { name: "right", dataType: "number" },
        ],
        outputs:
        [
            { name: "next", dataType: "flow" },
            { name: "result", dataType: "number" },
        ]
    },

    mult:
    {
        name: "multiply",
        width: 100,
        data: '*',
        inputs:
        [
            { name: "previous", dataType: "flow" },
            { name: "left", dataType: "number" },
            { name: "right", dataType: "number" },
        ],
        outputs:
        [
            { name: "next", dataType: "flow" },
            { name: "result", dataType: "number" },
        ],
    },

    div:
    {
        name: "divide",
        width: 100,
        data: '/',
        inputs:
        [
            { name: "previous", dataType: "flow" },
            { name: "left", dataType: "number" },
            { name: "right", dataType: "number" },
        ],
        outputs:
        [
            { name: "next", dataType: "flow" },
            { name: "result", dataType: "number" },
        ]
    },
},

DataProgrammingModel: {
    "nodes":[
        {"name":"velocity","x":25.5,"y":71.5,"width":100,"height":60,"typeName":"parameter"},
        {"name":"mass","x":24.4,"y":182,"width":100,"height":60,"typeName":"parameter"},
        {"name":"multiply","x":241,"y":113.5,"width":100,"height":92,"typeName":"mult","data":"*"},
        {"name":"momentum","x":423.5,"y":113,"width":100,"height":74,"typeName":"result"}],
    "links":[
        { "outputNode": 2, "outputSocket": 0, "outputSocketName": "next", "inputNode": 3, "inputSocket": 0, "inputSocketName": "previous" },
        { "outputNode": 2, "outputSocket": 1, "outputSocketName": "result", "inputNode": 3, "inputSocket": 1, "inputSocketName": "value" },
        { "outputNode": 1, "outputSocket": 0, "outputSocketName": "value", "inputNode": 2, "inputSocket": 2, "inputSocketName": "right" },
        { "outputNode": 0, "outputSocket": 0, "outputSocketName": "value", "inputNode": 2, "inputSocket": 1, "inputSocketName": "left"  }]
},

};


