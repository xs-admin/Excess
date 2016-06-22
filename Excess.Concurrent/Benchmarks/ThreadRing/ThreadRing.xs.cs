using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Excess.Concurrent.Runtime;

namespace ThreadRing
{
    [Concurrent(id = "3af83722-d05c-483e-bec6-d4ba5b3bbef2")]
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

    [Concurrent(id = "96a6f36b-f5b1-4b94-89b6-0224b41740b9")]
    [ConcurrentSingleton(id: "b296fbf4-67e7-41f7-9980-b5b4ca1c3e5d")]
    public class __app : ConcurrentObject
    {
        protected override void __started()
        {
            var __enum = __concurrentmain(default (CancellationToken), null, null);
            __enter(() => __advance(__enum.GetEnumerator()), null);
        }

        private IEnumerable<Expression> __concurrentmain(CancellationToken __cancellation, Action<object> __success, Action<Exception> __failure)
        {
            //create the ring
            const int ringCount = 503;
            var items = Enumerable.Range(1, ringCount).Select(index => spawn<RingItem>(index)).ToArray();
            //update connectivity
            for (int i = 0; i < ringCount; i++)
            {
                var item = items[i];
                item.Next = i < ringCount - 1 ? items[i + 1] : items[0];
            }

            var n = 0;
            if (Arguments.Length != 1 || !int.TryParse(Arguments[0], out n))
                n = 50 * 1000 * 1000;
            //run n times around the ring
            items[0].token(n);
            {
                __dispatch("main");
                if (__success != null)
                    __success(null);
                yield break;
            }
        }

        private static __app __singleton;
        public static void Start(IConcurrentApp app)
        {
            __singleton = app.Spawn<__app>();
        }

        public readonly Guid __ID = Guid.NewGuid();
        public static void Start(string[] args)
        {
            Arguments = args;
            var app = new ThreadedConcurrentApp(threadCount: 4, blockUntilNextEvent: false, priority: ThreadPriority.Highest);
            app.Start();
            app.Spawn(new __app());
            Await = () => app.AwaitCompletion();
            Stop = () => app.Stop();
        }

        public static string[] Arguments
        {
            get;
            set;
        }

        public static Action Stop
        {
            get;
            private set;
        }

        public static Action Await
        {
            get;
            private set;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            __app.Start(args);
            {
            }

            __app.Await();
        }
    }
}