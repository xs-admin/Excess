#line 1 "C:\dev\Excess\Excess.Websites\metaprogramming\Home.xs"
#line 4
using demo_transpiler;
#line hidden
using System;
using System.Collections.Generic;
using System.Linq;
using Excess.Runtime;

#line 6
namespace metaprogramming
#line 7
{
#line 8
    public service Home
#line 9
    {
#line 11
        __injectFunction __inject = _ => {
#line 12
            ITranspiler _transpiler;
#line 13
            IGraphTranspiler _graphTranspiler;
#line 14
        }

#line hidden
        ;
#line 16
    public string Transpile(string text)
#line 17
    {
#line 18
        return _transpiler.Process(text);
#line 19
    }

#line 21
    public string TranspileGraph(string text)
#line 22
    {
#line 23
        return _graphTranspiler.Process(text);
#line 24
    }
#line 25
} }
