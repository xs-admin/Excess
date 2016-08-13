#line 1 "C:\dev\Excess\Excess.Websites\metaprogramming-asp\Home.xs"
#line 4
using metaprogramming.interfaces;
#line hidden
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

#line 6
namespace metaprogramming.Home
#line 7
{
#line hidden
    public static partial class Functions
    {
#line 8
        [route("/transpile/code")]
#line 9
        static public string Transpile(string text, __Scope __scope)
#line 10
        {
#line 13
            ICodeTranspiler _transpiler = __scope.get<ICodeTranspiler>("_transpiler");
#line 16
            return _transpiler.Transpile(text);
#line 17
        }
#line hidden
    }

    public static partial class Functions
    {
#line 19
        [route("/transpile/graph")]
#line 20
        static public string TranspileGraph(string text, __Scope __scope)
#line 21
        {
#line 24
            IGraphTranspiler _graphTranspiler = __scope.get<IGraphTranspiler>("_graphTranspiler");
#line 27
            return _graphTranspiler.Transpile(text);
#line 28
        }
#line hidden
    }
}