using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Excess.Concurrent.Runtime;

namespace ThreadRing
{
    [Concurrent(id = "ae9ae95a-9b5d-40ad-94d5-795a16ed546f")]
    class RingItem : ConcurrentObject
    {
        int _idx;
        public RingItem(int idx)
        {
            _idx = idx;
        }

        public RingItem Next
        {
            get;
            set;
        }

        [Concurrent]
        public void token(int value)
        {
            token(value, default (CancellationToken), null, null);
        }

        private IEnumerable<Expression> __concurrenttoken(int value, CancellationToken __cancellation, Action<object> __success, Action<Exception> __failure)
        {
            if (value == 0)
            {
                Console.WriteLine(_idx);
                App.Stop();
            }
            else
                Next.token(value - 1);
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

        public readonly Guid __ID = Guid.NewGuid();
    }
}