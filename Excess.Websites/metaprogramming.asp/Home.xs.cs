using metaprogramming.interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using Excess.Runtime;
using System.Configuration;
using System.Security.Principal;
using Microsoft.Owin;
using Excess.Server.Middleware;
using System.Threading;
using System.Threading.Tasks;
using Excess.Concurrent.Runtime;

namespace metaprogramming.Home
{
    public static partial class Functions
    {
        [route("/transpile/code")]
        static public string Transpile(string text, __Scope __scope)
        {
            ICodeTranspiler _transpiler = __scope.get<ICodeTranspiler>("_transpiler");
            return _transpiler.Transpile(text);
        }
    }

    public static partial class Functions
    {
        [route("/transpile/graph")]
        static public string TranspileGraph(string text, __Scope __scope)
        {
            IGraphTranspiler _graphTranspiler = __scope.get<IGraphTranspiler>("_graphTranspiler");
            return _graphTranspiler.Transpile(text);
        }
    }
}