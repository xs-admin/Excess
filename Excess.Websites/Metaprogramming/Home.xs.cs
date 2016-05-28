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

    [Service(id: "7c8e5b77-4c5b-4137-8551-2f2bfa561dc5")]
    [Concurrent(id = "91bcaacf-fb50-4d9b-a7f8-ce670fd386d5")]

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
ITranspiler ___transpiler)
#line hidden

        {

#line 10
            _transpiler = ___transpiler;
#line hidden

        }

        [Concurrent]

#line 13
        public string Transpile(string text)
#line hidden

        {
            return 
#line 13
Transpile(text, default (CancellationToken)).Result;
#line hidden

        }

        private IEnumerable<Expression> __concurrentTranspile
#line 13
(string text, CancellationToken __cancellation, Action<object> __success, Action<Exception> __failure)

#line 14
        {
#line hidden

            {
                __dispatch("Transpile");
                if (__success != null)
                    __success(
#line 15
_transpiler.Process(text));
#line hidden

                yield break;
            }

#line 16
        }

#line hidden

        public Task<
#line 13
string> Transpile(string text, CancellationToken cancellation)
#line hidden

        {
            var completion = new TaskCompletionSource<
#line 13
string>();
#line hidden

            Action<object> __success = (__res) => completion.SetResult((
#line 13
string)__res);
#line hidden

            Action<Exception> __failure = (__ex) => completion.SetException(__ex);
            var __cancellation = cancellation;
            __enter(() => __advance(__concurrentTranspile(
#line 13
text, __cancellation, __success, __failure).GetEnumerator()), __failure);
#line hidden

            return completion.Task;
        }

        public void Transpile(
#line 13
string text, CancellationToken cancellation, Action<object> success, Action<Exception> failure)
#line hidden

        {
            var __success = success;
            var __failure = failure;
            var __cancellation = cancellation;
            __enter(() => __advance(__concurrentTranspile(
#line 13
text, __cancellation, __success, __failure).GetEnumerator()), failure);
#line hidden

        }

        public readonly Guid __ID = Guid.NewGuid();

#line 10
        ITranspiler _transpiler;

#line 17
    }

#line 18
}