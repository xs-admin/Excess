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

namespace metaprogramming
{
    [Service(id: "4f2f5b4c-ae3d-4904-84e2-495859ded449")]
    [Concurrent(id = "ccec9308-7878-4c19-9b08-45d5f82c5911")]
    public class Home : ConcurrentObject
    {
        public Home(ICodeTranspiler ___transpiler, IGraphTranspiler ___graphTranspiler)
        {
            _transpiler = ___transpiler;
            _graphTranspiler = ___graphTranspiler;
        }

        [Concurrent]
        public string Transpile(string text)
        {
            return Transpile(text, default (CancellationToken)).Result;
        }

        [Concurrent]
        public string TranspileGraph(string text)
        {
            return TranspileGraph(text, default (CancellationToken)).Result;
        }

        private IEnumerable<Expression> __concurrentTranspile(string text, CancellationToken __cancellation, Action<object> __success, Action<Exception> __failure)
        {
            {
                __dispatch("Transpile");
                if (__success != null)
                    __success(_transpiler.Transpile(text));
                yield break;
            }
        }

        public Task<string> Transpile(string text, CancellationToken cancellation)
        {
            var completion = new TaskCompletionSource<string>();
            Action<object> __success = (__res) => completion.SetResult((string)__res);
            Action<Exception> __failure = (__ex) => completion.SetException(__ex);
            var __cancellation = cancellation;
            __enter(() => __advance(__concurrentTranspile(text, __cancellation, __success, __failure).GetEnumerator()), __failure);
            return completion.Task;
        }

        public void Transpile(string text, CancellationToken cancellation, Action<object> success, Action<Exception> failure)
        {
            var __success = success;
            var __failure = failure;
            var __cancellation = cancellation;
            __enter(() => __advance(__concurrentTranspile(text, __cancellation, __success, __failure).GetEnumerator()), failure);
        }

        private IEnumerable<Expression> __concurrentTranspileGraph(string text, CancellationToken __cancellation, Action<object> __success, Action<Exception> __failure)
        {
            {
                __dispatch("TranspileGraph");
                if (__success != null)
                    __success(_graphTranspiler.Transpile(text));
                yield break;
            }
        }

        public Task<string> TranspileGraph(string text, CancellationToken cancellation)
        {
            var completion = new TaskCompletionSource<string>();
            Action<object> __success = (__res) => completion.SetResult((string)__res);
            Action<Exception> __failure = (__ex) => completion.SetException(__ex);
            var __cancellation = cancellation;
            __enter(() => __advance(__concurrentTranspileGraph(text, __cancellation, __success, __failure).GetEnumerator()), __failure);
            return completion.Task;
        }

        public void TranspileGraph(string text, CancellationToken cancellation, Action<object> success, Action<Exception> failure)
        {
            var __success = success;
            var __failure = failure;
            var __cancellation = cancellation;
            __enter(() => __advance(__concurrentTranspileGraph(text, __cancellation, __success, __failure).GetEnumerator()), failure);
        }

        public readonly Guid __ID = Guid.NewGuid();
        ICodeTranspiler _transpiler;
        IGraphTranspiler _graphTranspiler;
    }
}