
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
    "             case > 5: " + '\n' +
    "                 Console.Write(\"Greater than 5\");" + '\n' +
    "             case < y: " + '\n' +
    "                 Console.Write(\"Less than y\");" + '\n' +
    "             default:" + '\n' +
    "                Console.Write(\"Otherwise\");" + '\n' +
    "        }" + '\n' +
    "    }" + '\n' +
    "}",

    //constructor sample
    "" + '\n' +
    "//demonstrates a constructor member" + '\n' + 
    "//because words matter             " + '\n' + 
    "class Demo                         " + '\n' + 
    "{                                  " + '\n' + 
    "    int _x;                        " + '\n' + 
    "    constructor(int x)             " + '\n' + 
    "    {                              " + '\n' + 
    "        _x = x;                    " + '\n' + 
    "    }                              " + '\n' + 
    "}",    

    //concurrent example
    "" + '\n' +
    "//demonstrates a concurrent vending machine            " + '\n' +
    "//sometimes you mean a lot.                            " + '\n' +
    "namespace Demo                                         " + '\n' +
    "{                                                      " + '\n' +
    "    concurrent class VendingMachine                    " + '\n' +
    "    {                                                  " + '\n' +
    "        void main()                                    " + '\n' +
    "        {                                              " + '\n' +
    "            for (;;)                                   " + '\n' +
    "            {                                          " + '\n' +
    "                coin >> (choc | toffee);               " + '\n' +
    "            }                                          " + '\n' +
    "        }                                              " + '\n' +
    "" + '\n' +
    "        int _money = 0;                                " + '\n' +
    "        protected void coin()                          " + '\n' +
    "        {                                              " + '\n' +
    "            _money++;                                  " + '\n' +
    "        }                                              " + '\n' +
    "" + '\n' +
    "        //dispense methods, called only after \"coin\" " + '\n' +
    "        protected void choc();                         " + '\n' +
    "        protected void toffee();                       " + '\n' +
    "    }                                                  " + '\n' +
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
    "" + '\n' +
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
    "}",

    //constructor sample
    "using System;                      " + '\n' +
    "using System.Collections.Generic;  " + '\n' +
    "using System.Linq;                 " + '\n' +
    ""                                    + '\n' +
    "//demonstrates a constructor member" + '\n' +
    "//because words matter             " + '\n' +
    "class Demo                         " + '\n' +
    "{                                  " + '\n' +
    "    int _x;                        " + '\n' +
    "    public Demo(int x)             " + '\n' +
    "    {                              " + '\n' +
    "        _x = x;                    " + '\n' +
    "    }                              " + '\n' +
    "}",

    //concurrent example
    "using System;" + '\n' +
    "using System.Collections.Generic;" + '\n' +
    "using System.Linq;" + '\n' +
    "using System.Threading;" + '\n' +
    "using System.Threading.Tasks;" + '\n' +
    "using Excess.Concurrent.Runtime;" + '\n' +
    "" + '\n' +
    "//demonstrates a concurrent vending machine" + '\n' +
    "//operations are executed only in the right order" + '\n' +
    "namespace Demo" + '\n' +
    "{" + '\n' +
    "    [Concurrent(id = \"5ea73451-cf4c-4228-b4ff-70d2a01d8c94\")]" + '\n' +
    "    class VendingMachine : ConcurrentObject" + '\n' +
    "    {" + '\n' +
    "        protected override void __started()" + '\n' +
    "    {" + '\n' +
    "            var __enum = __concurrentmain(default (CancellationToken), null, null);" + '\n' +
    "            __enter(() => __advance(__enum.GetEnumerator()), null);" + '\n' +
    "    }" + '\n' +
    "" + '\n' +
    "        int _money = 0;" + '\n' +
    "        [Concurrent]" + '\n' +
    "        protected void coin()" + '\n' +
    "        {" + '\n' +
    "            coin(default (CancellationToken), null, null);" + '\n' +
    "        }" + '\n' +
    "" + '\n' +
    "        [Concurrent]" + '\n' +
    "        protected void choc()" + '\n' +
    "        {" + '\n' +
    "            choc(default (CancellationToken), null, null);" + '\n' +
    "        }" + '\n' +
    "" + '\n' +
    "        ;" + '\n' +
    "        [Concurrent]" + '\n' +
    "        protected void toffee()" + '\n' +
    "        {" + '\n' +
    "            toffee(default (CancellationToken), null, null);" + '\n' +
    "        }" + '\n' +
    "" + '\n' +
    "        ;" + '\n' +
    "        private IEnumerable<Expression> __concurrentmain(CancellationToken __cancellation, Action<object> __success, Action<Exception> __failure)" + '\n' +
    "        {" + '\n' +
    "            for (;;)" + '\n' +
    "            {" + '\n' +
    "                var __expr1_var = new __expr1{Start = (___expr) =>" + '\n' +
    "                {" + '\n' +
    "                    var __expr = (__expr1)___expr;" + '\n' +
    "                    __listen(\"coin\", () =>" + '\n' +
    "                    {" + '\n' +
    "                        __expr.__op1(true, null, null);" + '\n' +
    "                    }" + '\n' +
    "" + '\n' +
    "                    );" + '\n' +
    "                }" + '\n' +
    "" + '\n' +
    "                , End = (__expr) =>" + '\n' +
    "                {" + '\n' +
    "                    __enter(() => __advance(__expr.Continuator), __failure);" + '\n' +
    "                }" + '\n' +
    "" + '\n' +
    "                , __start1 = (___expr) =>" + '\n' +
    "                {" + '\n' +
    "                    var __expr = (__expr1)___expr;" + '\n' +
    "                    __enter(() =>" + '\n' +
    "                    {" + '\n' +
    "                        __listen(\"choc\", () =>" + '\n' +
    "                        {" + '\n' +
    "                            __expr.__op2(true, null, null);" + '\n' +
    "                        }" + '\n' +
    "" + '\n' +
    "                        );" + '\n' +
    "                        __listen(\"toffee\", () =>" + '\n' +
    "                        {" + '\n' +
    "                            __expr.__op2(null, true, null);" + '\n' +
    "                        }" + '\n' +
    "" + '\n' +
    "                        );" + '\n' +
    "                    }" + '\n' +
    "" + '\n' +
    "                    , __failure);" + '\n' +
    "                }" + '\n' +
    "                };" + '\n' +
    "                yield return __expr1_var;" + '\n' +
    "                if (__expr1_var.Failure != null)" + '\n' +
    "                    throw __expr1_var.Failure;" + '\n' +
    "            }" + '\n' +
    "" + '\n' +
    "            {" + '\n' +
    "                __dispatch(\"main\");" + '\n' +
    "                if (__success != null)" + '\n' +
    "                    __success(null);" + '\n' +
    "                yield break;" + '\n' +
    "            }" + '\n' +
    "        }" + '\n' +
    "" + '\n' +
    "        private IEnumerable<Expression> __concurrentcoin(CancellationToken __cancellation, Action<object> __success, Action<Exception> __failure)" + '\n' +
    "        {" + '\n' +
    "            _money++;" + '\n' +
    "            {" + '\n' +
    "                __dispatch(\"coin\");" + '\n' +
    "                if (__success != null)" + '\n' +
    "                    __success(null);" + '\n' +
    "                yield break;" + '\n' +
    "            }" + '\n' +
    "        }" + '\n' +
    "" + '\n' +
    "        public Task<object> coin(CancellationToken cancellation)" + '\n' +
    "        {" + '\n' +
    "            var completion = new TaskCompletionSource<object>();" + '\n' +
    "            Action<object> __success = (__res) => completion.SetResult((object)__res);" + '\n' +
    "            Action<Exception> __failure = (__ex) => completion.SetException(__ex);" + '\n' +
    "            var __cancellation = cancellation;" + '\n' +
    "            __enter(() => __advance(__concurrentcoin(__cancellation, __success, __failure).GetEnumerator()), __failure);" + '\n' +
    "            return completion.Task;" + '\n' +
    "        }" + '\n' +
    "" + '\n' +
    "        public void coin(CancellationToken cancellation, Action<object> success, Action<Exception> failure)" + '\n' +
    "        {" + '\n' +
    "            var __success = success;" + '\n' +
    "            var __failure = failure;" + '\n' +
    "            var __cancellation = cancellation;" + '\n' +
    "            __enter(() => __advance(__concurrentcoin(__cancellation, __success, __failure).GetEnumerator()), failure);" + '\n' +
    "        }" + '\n' +
    "" + '\n' +
    "        private IEnumerable<Expression> __concurrentchoc(CancellationToken __cancellation, Action<object> __success, Action<Exception> __failure)" + '\n' +
    "        {" + '\n' +
    "            if (true && !__awaiting(\"choc\"))" + '\n' +
    "                throw new InvalidOperationException(\"choc\" + \" can not be executed in this state\");" + '\n' +
    "            __dispatch(\"choc\");" + '\n' +
    "            if (__success != null)" + '\n' +
    "                __success(null);" + '\n' +
    "            yield break;" + '\n' +
    "        }" + '\n' +
    "" + '\n' +
    "        public Task<object> choc(CancellationToken cancellation)" + '\n' +
    "        {" + '\n' +
    "            var completion = new TaskCompletionSource<object>();" + '\n' +
    "            Action<object> __success = (__res) => completion.SetResult((object)__res);" + '\n' +
    "            Action<Exception> __failure = (__ex) => completion.SetException(__ex);" + '\n' +
    "            var __cancellation = cancellation;" + '\n' +
    "            __enter(() => __advance(__concurrentchoc(__cancellation, __success, __failure).GetEnumerator()), __failure);" + '\n' +
    "            return completion.Task;" + '\n' +
    "        }" + '\n' +
    "" + '\n' +
    "        public void choc(CancellationToken cancellation, Action<object> success, Action<Exception> failure)" + '\n' +
    "        {" + '\n' +
    "            var __success = success;" + '\n' +
    "            var __failure = failure;" + '\n' +
    "            var __cancellation = cancellation;" + '\n' +
    "            __enter(() => __advance(__concurrentchoc(__cancellation, __success, __failure).GetEnumerator()), failure);" + '\n' +
    "        }" + '\n' +
    "" + '\n' +
    "        private IEnumerable<Expression> __concurrenttoffee(CancellationToken __cancellation, Action<object> __success, Action<Exception> __failure)" + '\n' +
    "        {" + '\n' +
    "            if (true && !__awaiting(\"toffee\"))" + '\n' +
    "                throw new InvalidOperationException(\"toffee\" + \" can not be executed in this state\");" + '\n' +
    "            __dispatch(\"toffee\");" + '\n' +
    "            if (__success != null)" + '\n' +
    "                __success(null);" + '\n' +
    "            yield break;" + '\n' +
    "        }" + '\n' +
    "" + '\n' +
    "        public Task<object> toffee(CancellationToken cancellation)" + '\n' +
    "        {" + '\n' +
    "            var completion = new TaskCompletionSource<object>();" + '\n' +
    "            Action<object> __success = (__res) => completion.SetResult((object)__res);" + '\n' +
    "            Action<Exception> __failure = (__ex) => completion.SetException(__ex);" + '\n' +
    "            var __cancellation = cancellation;" + '\n' +
    "            __enter(() => __advance(__concurrenttoffee(__cancellation, __success, __failure).GetEnumerator()), __failure);" + '\n' +
    "            return completion.Task;" + '\n' +
    "        }" + '\n' +
    "" + '\n' +
    "        public void toffee(CancellationToken cancellation, Action<object> success, Action<Exception> failure)" + '\n' +
    "        {" + '\n' +
    "            var __success = success;" + '\n' +
    "            var __failure = failure;" + '\n' +
    "            var __cancellation = cancellation;" + '\n' +
    "            __enter(() => __advance(__concurrenttoffee(__cancellation, __success, __failure).GetEnumerator()), failure);" + '\n' +
    "        }" + '\n' +
    "" + '\n' +
    "        private class __expr1 : Expression" + '\n' +
    "        {" + '\n' +
    "            public void __op1(bool ? v1, bool ? v2, Exception __ex)" + '\n' +
    "            {" + '\n' +
    "                if (!tryUpdate(v1, v2, ref __op1_Left, ref __op1_Right, __ex))" + '\n' +
    "                    return;" + '\n' +
    "                if (v1.HasValue)" + '\n' +
    "                {" + '\n' +
    "                    if (__op1_Left.Value)" + '\n' +
    "                        __start1(this);" + '\n' +
    "                    else" + '\n' +
    "                        __complete(false, __ex);" + '\n' +
    "                }" + '\n' +
    "                else" + '\n' +
    "                {" + '\n' +
    "                    if (__op1_Right.Value)" + '\n' +
    "                        __complete(true, null);" + '\n' +
    "                    else" + '\n' +
    "                        __complete(false, __ex);" + '\n' +
    "                }" + '\n' +
    "            }" + '\n' +
    "" + '\n' +
    "            private bool ? __op1_Left;" + '\n' +
    "            private bool ? __op1_Right;" + '\n' +
    "            public Action<__expr1> __start1;" + '\n' +
    "            public void __op2(bool ? v1, bool ? v2, Exception __ex)" + '\n' +
    "            {" + '\n' +
    "               if (!tryUpdate(v1, v2, ref __op2_Left, ref __op2_Right, __ex))" + '\n' +
    "                   return;" + '\n' +
    "               if (v1.HasValue)" + '\n' +
    "               {" + '\n' +
    "                   if (__op2_Left.Value)" + '\n' +
    "                        __op1(null, true, null);" + '\n' +
    "                   else if (__op2_Right.HasValue)" + '\n' +
    "                       __op1(null, false, __ex);" + '\n' +
    "               }" + '\n' +
    "               else" + '\n' +
    "               {" + '\n' +
    "                   if (__op2_Right.Value)" + '\n' +
    "                        __op1(null, true, null);" + '\n' +
    "                   else if (__op2_Left.HasValue)" + '\n' +
    "                       __op1(null, false, __ex);" + '\n' +
    "               }" + '\n' +
    "           }" + '\n' +
    "" + '\n' +
    "           private bool ? __op2_Left;" + '\n' +
    "           private bool ? __op2_Right;" + '\n' +
    "       }" + '\n' +
    "" + '\n' +
    "       public readonly Guid __ID = Guid.NewGuid();" + '\n' +
    "   }" + '\n' +
    "}",
],

DataProgrammingDataTypes: {
    number : 
    {
        color: "yellow"
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


