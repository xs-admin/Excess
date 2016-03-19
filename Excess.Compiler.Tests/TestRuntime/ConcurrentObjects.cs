using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess.Compiler.Tests.TestRuntime
{
    using System.Collections.Concurrent;
    using System.Diagnostics;
    using System.Threading;
    using Spawner = Func<object[], ConcurrentObject>;

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class Concurrent : Attribute
    {
        public Guid Id;

        public Concurrent(Guid id)
        {
            Id = id;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class ConcurrentSingleton : Attribute
    {
        public Guid Id;

        public ConcurrentSingleton(Guid id)
        {
            Id = id;
        }
    }

    public interface IInstantiator
    {
        ConcurrentObject Create(Type type, params object[] args);
        T CreateSingleton<T>() where T : ConcurrentObject;
    }

    public class Node
    {
        IDictionary<string, Spawner> _types;
        int _threads;
        IInstantiator _instantiator;
        IDictionary<string, ConcurrentObject> _singletons;
        public Node(int threads, IDictionary<string, Spawner> types, IDictionary<string, ConcurrentObject> singletons)
        {
            _threads = threads;
            _types = types;
            _instantiator = new Instantiator(this);
            _singletons = singletons;
            foreach (var singleton in singletons)
                singleton.Value.startRunning(this, new object[] { });

            Debug.Assert(_threads > 0);
            createThreads(_threads);
        }

        //public T Spawn<T>(params object[] args) where T : ConcurrentObject, new()
        //{
        //    var result = new T();
        //    result.startRunning(this, args);
        //    return result;
        //}

        public ConcurrentObject Spawn(string type, params object[] args)
        {
            if (_singletons.ContainsKey(type))
                throw new InvalidOperationException(type + " cannot be spawned, is a singleton");

            var caller = null as Func<object[], ConcurrentObject>;
            if (!_types.TryGetValue(type, out caller))
                throw new InvalidOperationException(type + " is not defined");

            var result = caller(args); 
            result.startRunning(this, args);
            return result;
        }

        public ConcurrentObject Get(string className)
        {
            return _singletons[className];
        }

        public T Get<T>() where T : ConcurrentObject
        {
            return (T)Get(typeof(T).Name);
        }

        public void Start(ConcurrentObject @object, params object[] args)
        {
            @object.startRunning(this, args);
        }

        private class Event
        {
            public int Tries { get; set; }
            public ConcurrentObject Target { get; set; }
            public Action What { get; set; }
            public Action<Exception> Failure { get; set; }
        }

        ConcurrentQueue<Event> _queue = new ConcurrentQueue<Event>();
        public void Queue(ConcurrentObject who, Action what, Action<Exception> failure)
        {
            _queue.Enqueue(new Event
            {
                Tries = 0,
                Target = who,
                What = what,
                Failure = failure
            });
            //_queue.Enqueue(queueEvent(0, who, what, failure));
        }

        CancellationTokenSource _stop = new CancellationTokenSource();
        int _stopCount = 1;
        public void Stop()
        {
            _stopCount--;
            if (_stopCount > 0)
                return;

            _stop.Cancel();
            Thread.Sleep(1);
        }

        public void WaitForCompletion()
        {
            _stop.Token.WaitHandle.WaitOne();
        }

        public void Restart()
        {
            Debug.Assert(_stop.Token.IsCancellationRequested);
            _stop = new CancellationTokenSource();
            createThreads(_threads);
        }

        public void StopCount(int stopCount)
        {
            _stopCount = stopCount;
        }

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

                        message.Target.__enter(message.What, message.Failure);
                    }
                });

                thread.Priority = ThreadPriority.AboveNormal;
                thread.Start();
            }
        }

    }

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

        //protected T spawn<T>(params object[] args) where T : ConcurrentObject, new()
        //{
        //    return _node.Spawn<T>(args);
        //}

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
            return from + (to - from)*__rand.NextDouble();
        }
    }

    public class Expression
    {
        public Action<Expression> Start { get; set; }
        public IEnumerator<Expression> Continuator { get; set; }
        public Action<Expression> End { get; set; }
        public Exception Failure { get; set; }

        List<Exception> _exceptions = new List<Exception>();
        protected void __complete(bool success, Exception failure)
        {
            Debug.Assert(Continuator != null);
            Debug.Assert(End != null);

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

            End(this);
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

    public class Instantiator : IInstantiator
    {
        Node _node;
        public Instantiator(Node node)
        {
            _node = node;
        }

        public ConcurrentObject Create(Type type, params object[] args)
        {
            var result = Activator.CreateInstance(type) as ConcurrentObject;
            result.startRunning(_node, args);
            return result;
        }

        public T CreateSingleton<T>() where T : ConcurrentObject
        {
            var result = (T)Create(typeof(T));
            return result;
        }
    }
}
