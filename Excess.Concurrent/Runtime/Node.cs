using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Excess.Concurrent.Runtime
{
    using System.Linq;
    using System.Threading.Tasks;
    using Spawner = Func<object[], ConcurrentObject>;

    public class Node
    {
        IDictionary<string, Spawner> _types;
        int _threads;
        public Node(int threads, IDictionary<string, Spawner> types = null, bool afap = true)
        {
            _threads = threads;
            _types = types;

            Debug.Assert(_threads > 0);
            createThreads(_threads, afap);
        }

        public T Spawn<T>(params object[] args) where T : ConcurrentObject, new()
        {
            var result = new T();
            result.startRunning(this, args);
            return result;
        }

        public T Spawn<T>(T @object, params object[] args) where T : ConcurrentObject
        {
            @object.startRunning(this, args);
            return @object;
        }

        public Task<bool> Queue(Action what)
        {
            var completion = new TaskCompletionSource<bool>();
            Queue(null, 
                () => { what(); completion.SetResult(false); }, 
                ex => completion.SetException(ex));
            return completion.Task;
        }

        public ConcurrentObject Spawn(string type, params object[] args)
        {
            var caller = null as Func<object[], ConcurrentObject>;
            if (!_types.TryGetValue(type, out caller))
                throw new InvalidOperationException(type + " is not defined");

            var result = caller(args);
            result.startRunning(this, args);
            return result;
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

        public void StopCount(int stopCount)
        {
            _stopCount = stopCount;
        }

        private void createThreads(int threads, bool asFastAsPossible)
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
                            Thread.Sleep(0);
                            continue;
                        }

                        if (message.Target != null)
                            message.Target.__enter(message.What, message.Failure);
                        else
                        {
                            try
                            {
                                message.What();
                            }
                            catch (Exception ex)
                            {
                                if (message.Failure != null)
                                    message.Failure(ex);
                                else
                                    throw;
                            }
                        }
                    }
                });

                if (asFastAsPossible)
                    thread.Priority = ThreadPriority.AboveNormal;

                thread.Start();
            }
        }
    }
}
