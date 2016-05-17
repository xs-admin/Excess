
var MetaProgrammingSamples = [

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


];

var DataProgrammingNodeTypes = {
    input:
    {
        name: "input",
        width: 100,
        data: '5',
        input:
        [
        ],
        output:
        [
            {
                name: "5",
                dataType: "number",
            },
        ]
    },

    output:
    {
        name: "output",
        width: 100,
        onPreDraw: function (context) {
            this.input[0].name = $scope.eval(this.input[0].evaluate());
        },
        input:
        [
            { name: "", dataType: "*" },
        ],
        output:
        [
        ]
    },

    sum:
    {
        name: "sum",
        data: '+',
        width: 100,
        input:
        [
            { name: "operand1", dataType: "number" },
            { name: "operand2", dataType: "number" },
        ],
        output:
        [
            { name: "result", dataType: "number" },
        ]
    },

    sub:
    {
        name: "substract",
        data: '-',
        width: 100,
        input:
        [
            { name: "operand1", dataType: "number" },
            { name: "operand2", dataType: "number" },
        ],
        output:
        [
            { name: "result", dataType: "number" },
        ]
    },

    mult:
    {
        name: "multiply",
        width: 100,
        data: '*',
        input:
        [
            { name: "operand1", dataType: "number" },
            { name: "operand2", dataType: "number" },
        ],
        output:
        [
            { name: "result", dataType: "number" },
        ],
    },

    div:
    {
        name: "divide",
        width: 100,
        data: '/',
        input:
        [
            { name: "operand1", dataType: "number" },
            { name: "operand2", dataType: "number" },
        ],
        output:
        [
            { name: "result", dataType: "number" },
        ]
    },

    neg:
    {
        name: "negate",
        width: 100,
        data: '-',
        input:
        [
            { name: "operand", dataType: "number" },
        ],
        output:
        [
            { name: "result", dataType: "number" },
        ]
    },

    isEqual:
    {
        name: "is-equal",
        width: 100,
        data: '==',
        onUpdate: function (node) {
            node.input[1].dataType = node.input[0].dataType;
        },
        input:
        [
            { name: "operand1", dataType: "*" },
            { name: "operand2", dataType: "*" },
        ],
        output:
        [
            { name: "result", dataType: "bool" },
        ]
    },

    isLess:
    {
        name: "is-less",
        width: 100,
        data: '<',
        onUpdate: function (node) {
            node.input[1].dataType = node.input[0].dataType;
        },
        input:
        [
            { name: "operand1", dataType: "number" },
            { name: "operand2", dataType: "number" },
        ],
        output:
        [
            { name: "result", dataType: "bool" },
        ]
    },

    not:
    {
        name: "not",
        width: 100,
        data: '!',
        input:
        [
            { name: "operand", dataType: "bool" },
        ],
        output:
        [
            { name: "result", dataType: "bool" },
        ]
    },

    and:
    {
        name: "and",
        width: 100,
        data: '&&',
        input:
        [
            { name: "operand1", dataType: "bool" },
            { name: "operand2", dataType: "bool" },
        ],
        output:
        [
            { name: "result", dataType: "bool" },
        ]
    },

    or:
    {
        name: "or",
        width: 100,
        data: '||',
        input:
        [
            { name: "operand1", dataType: "bool" },
            { name: "operand2", dataType: "bool" },
        ],
        output:
        [
            { name: "result", dataType: "bool" },
        ]
    },

    ifThenElse:
    {
        name: "if-then-else",
        width: 100,
        onUpdate: function (node) {
            var dataType = node.input[1].dataType;
            node.input[2].dataType = dataType;
            node.output[0].dataType = dataType;
        },

        input:
        [
            { name: "if", dataType: "bool" },
            { name: "then", dataType: "*" },
            { name: "else", dataType: "*" },
        ],
        output:
        [
            {
                name: "result",
                dataType: "*",
            },
        ]
    }
};

var VisualProgrammingScene = {
"nodes":[{"name":"data1","x":87,"y":70,"width":100,"height":58,"input":[],"output":[{"name":3,"dataType":"number"}],"typeName":"input","data":3},{"name":"data2","x":90,"y":158,"width":100,"height":58,"input":[],"output":[{"name":5,"dataType":"number"}],"typeName":"input","data":5},{"name":"is-less","x":263,"y":103,"width":100,"height":76,"input":[{"name":"operand1","dataType":"number"},{"name":"operand2","dataType":"number"}],"output":[{"name":"result","dataType":"bool"}],"typeName":"isLess","data":"<"},{"name":"data3","x":93,"y":241,"width":100,"height":58,"input":[],"output":[{"name":6,"dataType":"number"}],"typeName":"input","data":6},{"name":"is-less","x":261,"y":197,"width":100,"height":76,"input":[{"name":"operand1","dataType":"number"},{"name":"operand2","dataType":"number"}],"output":[{"name":"result","dataType":"bool"}],"typeName":"isLess","data":"<"},{"name":"and","x":426,"y":138,"width":100,"height":76,"input":[{"name":"operand1","dataType":"bool"},{"name":"operand2","dataType":"bool"}],"output":[{"name":"result","dataType":"bool"}],"typeName":"and","data":"&&"},{"name":"if-then-else","x":570,"y":186,"width":100,"height":94,"input":[{"name":"if","dataType":"bool"},{"name":"then","dataType":"string"},{"name":"else","dataType":"string"}],"output":[{"name":"result","dataType":"string"}],"typeName":"ifThenElse"},{"name":"output","x":713,"y":203,"width":100,"height":58,"input":[{"name":"'ordered secuence'","dataType":"string"}],"output":[],"typeName":"output","data":5},{"name":"msg1","x":375,"y":246,"width":100,"height":58,"input":[],"output":[{"name":"'ordered secuence'","dataType":"string"}],"typeName":"input","data":"'ordered secuence'"},{"name":"msg2","x":373,"y":321,"width":100,"height":58,"input":[],"output":[{"name":"'incorrect order'","dataType":"string"}],"typeName":"input","data":"'incorrect order'"}],"links":[{"outputNode":1,"outputSocket":0,"inputNode":2,"inputSocket":1},{"outputNode":0,"outputSocket":0,"inputNode":2,"inputSocket":0},{"outputNode":1,"outputSocket":0,"inputNode":4,"inputSocket":0},{"outputNode":3,"outputSocket":0,"inputNode":4,"inputSocket":1},{"outputNode":2,"outputSocket":0,"inputNode":5,"inputSocket":0},{"outputNode":4,"outputSocket":0,"inputNode":5,"inputSocket":1},{"outputNode":5,"outputSocket":0,"inputNode":6,"inputSocket":0},{"outputNode":6,"outputSocket":0,"inputNode":7,"inputSocket":0},{"outputNode":8,"outputSocket":0,"inputNode":6,"inputSocket":1},{"outputNode":9,"outputSocket":0,"inputNode":6,"inputSocket":2}]
}