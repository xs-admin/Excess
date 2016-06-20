#line 1 "C:\dev\Excess\Excess.Websites\metaprogramming-asp\Home.xs"
#line 4
using demo_transpiler;
#line hidden
using System;
using System.Collections.Generic;
using System.Linq;
using Excess.Runtime;
using Excess.Server.Middleware;
using System.Threading;
using System.Threading.Tasks;
using Excess.Concurrent.Runtime;

#line 6
namespace Home
#line 7
{
#line hidden
    public static partial class Functions
    {
#line 8
        public static String Transpile(string text, __Scope __scope)
#line 9
        {
#line 12
            ITranspiler _transpiler = __scope.get<ITranspiler>("_transpiler");
#line 15
            return _transpiler.Process(text);
#line 16
        }
#line hidden
    }

    public static partial class Functions
    {
#line 18
        public static String TranspileGraph(string text, __Scope __scope)
#line 19
        {
#line 22
            IGraphTranspiler _graphTranspiler = __scope.get<IGraphTranspiler>("_graphTranspiler");
#line 25
            return _graphTranspiler.Process(text);
#line 26
        }
#line hidden
    }
}