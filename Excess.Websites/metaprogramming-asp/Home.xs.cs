#line hidden
using System;
using System.Collections.Generic;
using System.Linq;
using Middleware;
using System.Threading;
using System.Threading.Tasks;
using Excess.Concurrent.Runtime;

#line 4
namespace metaprogramming_asp
#line 5
{
#line hidden
    [Service(id: "b1f32524-0402-4721-9492-c7db004fb252")]
    [Concurrent(id = "45385ae9-4629-4d19-88c5-5cb0ed3a81b5")]
#line 6
    public class Home : ConcurrentObject
#line 7
    {
#line 6
        public Home(
#line 10
ITranspiler ___transpiler, 
#line 11
IGraphTranspiler ___graphTranspiler)
#line hidden
        {
#line 10
            _transpiler = ___transpiler;
#line 11
            _graphTranspiler = ___graphTranspiler;
#line hidden
        }

        [Concurrent]
#line 15
        public string Transpile(string text)
#line hidden
        {
#line 15
            return Transpile(text, default (CancellationToken)).Result;
#line hidden
        }

        [Concurrent]
#line 20
        public string TranspileGraph(string text)
#line hidden
        {
#line 20
            return TranspileGraph(text, default (CancellationToken)).Result;
#line hidden
        }

#line 15
        private IEnumerable<Expression> __concurrentTranspile(string text, CancellationToken __cancellation, Action<object> __success, Action<Exception> __failure)
#line 16
        {
#line hidden
            {
                __dispatch("Transpile");
                if (__success != null)
#line 17
                    __success(_transpiler.Process(text));
#line hidden
                yield break;
            }
#line 18
        }

#line 15
        public Task<string> Transpile(string text, CancellationToken cancellation)
#line hidden
        {
#line 15
            var completion = new TaskCompletionSource<string>();
#line 15
            Action<object> __success = (__res) => completion.SetResult((string)__res);
#line hidden
            Action<Exception> __failure = (__ex) => completion.SetException(__ex);
            var __cancellation = cancellation;
#line 15
            __enter(() => __advance(__concurrentTranspile(text, __cancellation, __success, __failure).GetEnumerator()), __failure);
#line hidden
            return completion.Task;
        }

#line 15
        public void Transpile(string text, CancellationToken cancellation, Action<object> success, Action<Exception> failure)
#line hidden
        {
            var __success = success;
            var __failure = failure;
            var __cancellation = cancellation;
#line 15
            __enter(() => __advance(__concurrentTranspile(text, __cancellation, __success, __failure).GetEnumerator()), failure);
#line hidden
        }

#line 20
        private IEnumerable<Expression> __concurrentTranspileGraph(string text, CancellationToken __cancellation, Action<object> __success, Action<Exception> __failure)
#line 21
        {
#line hidden
            {
                __dispatch("TranspileGraph");
                if (__success != null)
#line 22
                    __success(_graphTranspiler.Process(text));
#line hidden
                yield break;
            }
#line 23
        }

#line 20
        public Task<string> TranspileGraph(string text, CancellationToken cancellation)
#line hidden
        {
#line 20
            var completion = new TaskCompletionSource<string>();
#line 20
            Action<object> __success = (__res) => completion.SetResult((string)__res);
#line hidden
            Action<Exception> __failure = (__ex) => completion.SetException(__ex);
            var __cancellation = cancellation;
#line 20
            __enter(() => __advance(__concurrentTranspileGraph(text, __cancellation, __success, __failure).GetEnumerator()), __failure);
#line hidden
            return completion.Task;
        }

#line 20
        public void TranspileGraph(string text, CancellationToken cancellation, Action<object> success, Action<Exception> failure)
#line hidden
        {
            var __success = success;
            var __failure = failure;
            var __cancellation = cancellation;
#line 20
            __enter(() => __advance(__concurrentTranspileGraph(text, __cancellation, __success, __failure).GetEnumerator()), failure);
#line hidden
        }

        public readonly Guid __ID = Guid.NewGuid();
#line 10
        ITranspiler _transpiler;
#line 11
        IGraphTranspiler _graphTranspiler;
#line 24
    }
}