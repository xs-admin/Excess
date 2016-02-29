using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Excess.Concurrent.Runtime
{
    public class ConcurrentObject
    {
        protected Node _node;
        internal void startRunning(Node node, object[] args)
        {
            Debug.Assert(_busy == 0);
            _node = node;
            __start(args);
        }

        protected Node Node { get { return _node; } }
        protected virtual void __start(object[] args)
        {
        }

        protected T spawn<T>(params object[] args) where T : ConcurrentObject, new()
        {
            return _node.Spawn<T>(args);
        }

        int _busy = 0;
        protected internal void __enter(Action what, Action<Exception> failure)
        {
            var was_busy = Interlocked.CompareExchange(ref _busy, 1, 0) == 1;
            if (was_busy)
            {
                _node.Queue(this, what, failure);
            }
            else
            {
                try
                {
                    what();
                }
                catch (Exception ex)
                {
                    if (failure != null)
                        failure(ex);
                    else
                        throw;
                }
                finally
                {
                    Interlocked.CompareExchange(ref _busy, 0, 1);
                }
            }
        }

        protected void __advance(IEnumerator<Expression> thread)
        {
            if (!thread.MoveNext())
                return;

            var expr = thread.Current;
            expr.Continuator = thread;
            if (expr.Start != null)
                expr.Start(expr);
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

        protected bool __awaiting(string signal)
        {
            return _listeners
                .Where(kvp => kvp.Key == signal
                           && kvp.Value.Any())
                .Any();

        }

        Random __rand = new Random(); //td: test only
        protected double rand()
        {
            return __rand.NextDouble();
        }

        protected double rand(double from, double to)
        {
            return from + (to - from) * __rand.NextDouble();
        }
    }
}
