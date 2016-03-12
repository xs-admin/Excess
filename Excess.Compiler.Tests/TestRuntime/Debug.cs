using Excess.Compiler.Tests.TestRuntime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Excess.Compiler.Tests.Demos
{
    [Concurrent]
    class ring_item : ConcurrentObject
    {
        public int _idx;

        public ring_item Next
        {
            get;
            set;
        }

        static int ITERATIONS = 50 * 1000 * 1000;
        public void token(int value)
        {
            token(value, default(CancellationToken)).Wait();
        }

        private IEnumerable<Expression> __concurrenttoken(int value, CancellationToken __cancellation, Action<object> __success, Action<Exception> __failure)
        {
            console.write(value);
            if (value >= ITERATIONS)
            {
                console.write(_idx);
                Node.Stop();
            }
            else
                Next.token(value + 1);
            {
                __dispatch("token");
                if (__success != null)
                    __success(null);
                yield break;
            }
        }

        public Task<object> token(int value, CancellationToken cancellation)
        {
            var completion = new TaskCompletionSource<object>();
            Action<object> __success = (__res) => completion.SetResult((object)__res);
            Action<Exception> __failure = (__ex) => completion.SetException(__ex);
            var __cancellation = cancellation;
            __enter(() => __advance(__concurrenttoken(value, __cancellation, __success, __failure).GetEnumerator()), __failure);
            return completion.Task;
        }

        public void token(int value, CancellationToken cancellation, Action<object> success, Action<Exception> failure)
        {
            var __success = success;
            var __failure = failure;
            var __cancellation = cancellation;
            __enter(() => __advance(__concurrenttoken(value, __cancellation, __success, __failure).GetEnumerator()), failure);
        }
    }
}
