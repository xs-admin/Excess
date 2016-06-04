#line 1 "C:\dev\Excess\Excess.Websites\metaprogramming\Home.xs"
using System;
#line hidden

using System.Collections.Generic;
using System.Linq;
using Middleware;
using System.Threading;
using System.Threading.Tasks;
using Excess.Concurrent.Runtime;


#line 4
namespace metaprogramming

#line 5
{
#line hidden

    [Service(id: "d9042c75-1156-423a-8b38-caa4b870d704")]
    [Concurrent(id = "df03833e-a7a8-4e69-a4a3-76b1b56e7544")]

#line 6
    public class Home : ConcurrentObject
#line hidden


#line 7
    {
#line hidden

        public 
#line 6
Home(
#line 10
ITranspiler ___transpiler, 
#line 11
IGraphTranspiler ___graphTranspiler)
#line hidden

        {

#line 10
            _transpiler = ___transpiler;
#line hidden


#line 11
            _graphTranspiler = ___graphTranspiler;
#line hidden

        }

        [Concurrent]

#line 14
        public string Transpile(string text)
#line hidden

        {
            return 
#line 14
Transpile(text, default (CancellationToken)).Result;
#line hidden

        }

        [Concurrent]

#line 19
        public string TranspileGraph(string text)
#line hidden

        {
            return 
#line 19
TranspileGraph(text, default (CancellationToken)).Result;
#line hidden

        }

        private IEnumerable<Expression> __concurrentTranspile
#line 14
(string text, CancellationToken __cancellation, Action<object> __success, Action<Exception> __failure)

#line 15
        {
#line hidden

            {
                __dispatch("Transpile");
                if (__success != null)
                    __success(
#line 16
_transpiler.Process(text));
#line hidden

                yield break;
            }

#line 17
        }

#line hidden

        public Task<
#line 14
string> Transpile(string text, CancellationToken cancellation)
#line hidden

        {
            var completion = new TaskCompletionSource<
#line 14
string>();
#line hidden

            Action<object> __success = (__res) => completion.SetResult((
#line 14
string)__res);
#line hidden

            Action<Exception> __failure = (__ex) => completion.SetException(__ex);
            var __cancellation = cancellation;
            __enter(() => __advance(__concurrentTranspile(
#line 14
text, __cancellation, __success, __failure).GetEnumerator()), __failure);
#line hidden

            return completion.Task;
        }

        public void Transpile(
#line 14
string text, CancellationToken cancellation, Action<object> success, Action<Exception> failure)
#line hidden

        {
            var __success = success;
            var __failure = failure;
            var __cancellation = cancellation;
            __enter(() => __advance(__concurrentTranspile(
#line 14
text, __cancellation, __success, __failure).GetEnumerator()), failure);
#line hidden

        }

        private IEnumerable<Expression> __concurrentTranspileGraph
#line 19
(string text, CancellationToken __cancellation, Action<object> __success, Action<Exception> __failure)

#line 20
        {
#line hidden

            {
                __dispatch("TranspileGraph");
                if (__success != null)
                    __success(
#line 21
_graphTranspiler.Process(text));
#line hidden

                yield break;
            }

#line 22
        }

#line hidden

        public Task<
#line 19
string> TranspileGraph(string text, CancellationToken cancellation)
#line hidden

        {
            var completion = new TaskCompletionSource<
#line 19
string>();
#line hidden

            Action<object> __success = (__res) => completion.SetResult((
#line 19
string)__res);
#line hidden

            Action<Exception> __failure = (__ex) => completion.SetException(__ex);
            var __cancellation = cancellation;
            __enter(() => __advance(__concurrentTranspileGraph(
#line 19
text, __cancellation, __success, __failure).GetEnumerator()), __failure);
#line hidden

            return completion.Task;
        }

        public void TranspileGraph(
#line 19
string text, CancellationToken cancellation, Action<object> success, Action<Exception> failure)
#line hidden

        {
            var __success = success;
            var __failure = failure;
            var __cancellation = cancellation;
            __enter(() => __advance(__concurrentTranspileGraph(
#line 19
text, __cancellation, __success, __failure).GetEnumerator()), failure);
#line hidden

        }

        public readonly Guid __ID = Guid.NewGuid();

#line 10
        ITranspiler _transpiler;

#line 11
        IGraphTranspiler _graphTranspiler;

#line 23
    }

#line 24
}