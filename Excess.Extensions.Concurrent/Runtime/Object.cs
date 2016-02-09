using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Excess.Extensions.Concurrent.Runtime
{
    class Node
    {
        private class Event
        {
            public int Tries { get; set; }
            public Object Target { get; set; }
            public Func<object> What { get; set; }
            public Action<object> Success { get; set; }
            public Action<Exception> Failure { get; set; }
        }

        ConcurrentQueue<Event> _queue = new ConcurrentQueue<Event>();

        public void Queue(Func<object> what, Action<object> success, Action<Exception> failure)
        {
            _queue.Enqueue(new Event
            {
                Tries = 0,
                What = what,
                Success = success,
                Failure = failure
            });
        }

        CancellationTokenSource _stop = new CancellationTokenSource();
        private void createThreads(int threads)
        {
            var cancellation = _stop.Token;
            for (int i = 0; i < threads; i++)
            {
                var thread = new Thread(() => {
                    while (true)
                    {

                        Event message;
                        _queue.TryDequeue(out message);
                        if (cancellation.IsCancellationRequested)
                            break;

                        if (message == null)
                        {
                            Thread.Sleep(1);
                            continue;
                        }

                        message.Target.run(message.What, message.Success, message.Failure);
                    }
                });

                thread.Priority = ThreadPriority.AboveNormal;
                thread.Start();
            }
        }
    }

    class Object
    {
        Node _node;
        int _busy;
        internal void run(Func<object> what, Action<object> success, Action<Exception> failure)
        {
            var was_busy = Interlocked.CompareExchange(ref _busy, 1, 0) == 1;
            if (was_busy)
            {
                _node.Queue(what, success, failure);
            }
            else
            {
                try
                {
                    var result = what();
                    if (success != null)
                        success(result);
                }
                catch (Exception e)
                {
                    if (failure != null)
                        failure(e);
                }

                Interlocked.CompareExchange(ref _busy, 0, 1);
            }
        }

        internal void advance(IEnumerator<Expression> thread)
        {
            if (!thread.MoveNext())
                return;

            var expr = thread.Current;
            expr.Continuation = thread;
            if (expr.Start != null)
                expr.Start();
        }
    }

    class Expression
    {
        public IEnumerator<Expression> Continuation { get; set; }
        public Action Start { get; set; }
    }
}
