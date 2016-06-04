using Excess.Compiler;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace metaprogramming.server
{
    internal class Operator
    {
        public Operator(string opString, string result)
        {
            OperatorString = opString;
            Result = result;
        }

        public string OperatorString { get; private set; }

        public string Result { get; set; }
        public string LeftOperand { get; set; }
        public string RightOperand { get; set; }
    }

    internal class Parameter
    {
        public Parameter(string name)
        {
            Name = name;
        }

        public string Name { get; private set; }
    }

    internal class Return
    {
        public string Value { get; set; }
    }

    internal class GraphModel
    {
        public static IDictionary<string, Func<JToken, Scope, object>> Serializers { get; private set; }

        static GraphModel()
        {
            Serializers = new Dictionary<string, Func<JToken, Scope, object>>();
            Serializers["parameter"] = (jtoken, _) => new Parameter(jtoken.ToString());
            Serializers["result"] = (_, __) => new Return();
            Serializers["sum"] = OperatorSerializer;
            Serializers["sub"] = OperatorSerializer;
            Serializers["mult"] = OperatorSerializer;
            Serializers["div"] = OperatorSerializer;
        }

        private static int index = 0;
        private static object OperatorSerializer(JToken jtoken, Scope scope) => new Operator(jtoken.ToString(), scope.GetUniqueId("v"));
    }
}
