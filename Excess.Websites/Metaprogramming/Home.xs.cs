#line 1 "C:\dev\Excess\Excess.Websites\metaprogramming\Home.xs"
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
namespace metaprogramming
#line 7
{
#line hidden
    [Service(id: "2114f9ad-7774-47ad-a9d6-26d0b0fced7e")]
    [Concurrent(id = "a525b7b3-a362-42c1-be66-9517485044a6")]
#line 8
    public class Home : ConcurrentObject
#line 9
    {
#line 8
        public Home(
#line 12
ICodeTranspiler ___transpiler, 
#line 13
IGraphTranspiler ___graphTranspiler)
#line hidden
        {
#line 12
            _transpiler = ___transpiler;
#line 13
            _graphTranspiler = ___graphTranspiler;
#line hidden
        }

        [Concurrent]
#line 16
        public string Transpile(string text)
#line hidden
        {
#line 16
            return Transpile(text, default (CancellationToken)).Result;
#line hidden
        }

        [Concurrent]
#line 21
        public string TranspileGraph(string text)
#line hidden
        {
#line 21
            return TranspileGraph(text, default (CancellationToken)).Result;
#line hidden
        }

#line 16
        private IEnumerable<Expression> __concurrentTranspile(string text, CancellationToken __cancellation, Action<object> __success, Action<Exception> __failure)
#line 17
        {
#line hidden
            {
                __dispatch("Transpile");
                if (__success != null)
#line 18
                    __success(_transpiler.Transpile(text));
#line hidden
                yield break;
            }
#line 19
        }

#line 16
        public Task<string> Transpile(string text, CancellationToken cancellation)
#line hidden
        {
#line 16
            var completion = new TaskCompletionSource<string>();
#line 16
            Action<object> __success = (__res) => completion.SetResult((string)__res);
#line hidden
            Action<Exception> __failure = (__ex) => completion.SetException(__ex);
            var __cancellation = cancellation;
#line 16
            __enter(() => __advance(__concurrentTranspile(text, __cancellation, __success, __failure).GetEnumerator()), __failure);
#line hidden
            return completion.Task;
        }

#line 16
        public void Transpile(string text, CancellationToken cancellation, Action<object> success, Action<Exception> failure)
#line hidden
        {
            var __success = success;
            var __failure = failure;
            var __cancellation = cancellation;
#line 16
            __enter(() => __advance(__concurrentTranspile(text, __cancellation, __success, __failure).GetEnumerator()), failure);
#line hidden
        }

#line 21
        private IEnumerable<Expression> __concurrentTranspileGraph(string text, CancellationToken __cancellation, Action<object> __success, Action<Exception> __failure)
#line 22
        {
#line hidden
            {
                __dispatch("TranspileGraph");
                if (__success != null)
#line 23
                    __success(_graphTranspiler.Transpile(text));
#line hidden
                yield break;
            }
#line 24
        }

#line 21
        public Task<string> TranspileGraph(string text, CancellationToken cancellation)
#line hidden
        {
#line 21
            var completion = new TaskCompletionSource<string>();
#line 21
            Action<object> __success = (__res) => completion.SetResult((string)__res);
#line hidden
            Action<Exception> __failure = (__ex) => completion.SetException(__ex);
            var __cancellation = cancellation;
#line 21
            __enter(() => __advance(__concurrentTranspileGraph(text, __cancellation, __success, __failure).GetEnumerator()), __failure);
#line hidden
            return completion.Task;
        }

#line 21
        public void TranspileGraph(string text, CancellationToken cancellation, Action<object> success, Action<Exception> failure)
#line hidden
        {
            var __success = success;
            var __failure = failure;
            var __cancellation = cancellation;
#line 21
            __enter(() => __advance(__concurrentTranspileGraph(text, __cancellation, __success, __failure).GetEnumerator()), failure);
#line hidden
        }

        public readonly Guid __ID = Guid.NewGuid();
#line 12
        ICodeTranspiler _transpiler;
#line 13
        IGraphTranspiler _graphTranspiler;
#line 25
    }
}