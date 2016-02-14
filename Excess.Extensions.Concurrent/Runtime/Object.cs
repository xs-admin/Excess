using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Concurrent.Runtime
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

        protected Expression __advance(Expression expression)
        {
            var thread = expression.Continuator;
            if (!thread.MoveNext())
                return null;

            var expr = thread.Current;
            expr.Continuator = thread;
            if (expr.Start != null)
                expr.Start(expr);

            return expr;
        }

        Dictionary<string, List<Action>> _listeners = new Dictionary<string, List<Action>>();
        protected void __listen(string signal, Action callback)
        {
            List<Action> actions;
            if (!_listeners.TryGetValue(signal, out actions))
            {
                actions = new List<Action>();
                _listeners[signal] = actions;
            }

            actions.Add(callback);
        }

        protected void __dispatch(string signal)
        {
            List<Action> actions;
            if (_listeners.TryGetValue(signal, out actions))
            {
                foreach (var action in actions)
                {
                    try
                    {
                        action();
                    }
                    catch
                    {
                    }
                }

                actions.Clear();
            }
        }
    }

    public class Expression
    {
        public Action<Expression>      Start        { get; set; }
        public IEnumerator<Expression> Continuator  { get; set; }
        public Action<Expression>      Continuation { get; set; }
        public Exception               Failure      { get; set; }

        List<Exception> _exceptions = new List<Exception>();
        protected void __complete(bool success, Exception failure)
        {
            Debug.Assert(Continuator != null);
            Debug.Assert(Continuation != null);

            IEnumerable<Exception> allFailures = _exceptions;
            if (failure != null)
                allFailures = allFailures.Union(new[] { failure });

            if (!success)
            {
                Debug.Assert(allFailures.Any());

                if (allFailures.Count() == 1)
                    Failure = allFailures.First();
                else
                    Failure = new AggregateException(allFailures);
            }

            Continuation(this);
        }

        protected bool tryUpdate(bool? v1, bool? v2, ref bool? s1, ref bool? s2, Exception ex)
        {
            Debug.Assert(!v1.HasValue || !v2.HasValue);

            if (ex != null)
            {
                Debug.Assert((v1.HasValue && !v1.Value) || (v2.HasValue && !v2.Value));
                _exceptions.Add(ex);
            }

            if (v1.HasValue)
            {
                if (s1.HasValue)
                    return false;

                s1 = v1;
            }
            else
            {
                Debug.Assert(v2.HasValue);
                if (s2.HasValue)
                    return false;

                s2 = v2;
            }

            return true;
        }
    }
}
