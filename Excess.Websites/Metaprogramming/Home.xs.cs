#line 1 "C:\dev\Excess\Excess.Websites\metaprogramming\Home.xs"
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
namespace metaprogramming
#line 7
{
#line hidden
    [Service(id: "97eb75ce-1eac-4060-bb0a-38adf2930c31")]
    [Concurrent(id = "0833c722-d1be-4796-8f13-3330be19f849")]
#line 8
    public class Home : ConcurrentObject
#line 9
    {
#line 8
        public Home(
#line 12
ITranspiler ___transpiler, 
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
                    __success(_transpiler.Process(text));
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
                    __success(_graphTranspiler.Process(text));
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
        ITranspiler _transpiler;
#line 13
        IGraphTranspiler _graphTranspiler;
#line 25
    }
}