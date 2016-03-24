using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Linq;
using System.Threading.Tasks;

namespace Excess.Concurrent.Runtime
{
    public class Node
    {
        int _threads;
        IInstantiator _instatiator;
        public Node(int threads, IInstantiator instatiator = null, bool afap = true)
        {
            _threads = threads;
            _instatiator = instatiator;

            Debug.Assert(_threads > 0);
            createThreads(_threads, afap);
        }

        public T Spawn<T>(params object[] args) where T : ConcurrentObject, new()
        {
            return (T)doSpawn(new T(), args);
        }

        public T Spawn<T>(T @object, params object[] args) where T : ConcurrentObject
        {
            return (T)doSpawn(@object, args);
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
            if (_instatiator == null)
                throw new InvalidOperationException("cannot create");

            return doSpawn(_instatiator.Instantiate(type, args), args);
        }

        public void Start(ConcurrentObject @object, params object[] args)
        {
            doSpawn(@object, args);
        }

        List<Action<ConcurrentObject>> _listeners = new List<Action<ConcurrentObject>>();
        public void AddSpawnListener(Action<ConcurrentObject> listener)
        {
            _listeners.Add(listener);
        }

        private ConcurrentObject doSpawn(ConcurrentObject @object, params object[] args)
        {
            @object.startRunning(this, args);
            foreach (var listener in _listeners)
                listener(@object);

            return @object;
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
