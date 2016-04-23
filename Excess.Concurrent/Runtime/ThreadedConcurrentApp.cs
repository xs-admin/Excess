using System;
using System.Collections.Generic;
using System.Threading;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess.Concurrent.Runtime
{
    using FactoryFunction = Func<IConcurrentApp, object[], IConcurrentObject>;
    using FactoryMap = Dictionary<string, Func<IConcurrentApp, object[], IConcurrentObject>>;

    public class ThreadedConcurrentApp : BaseConcurrentApp
    {
        int _threadCount;
        int _stopCount;
        ManualResetEvent _blockUntilNextEvent;
        public ThreadedConcurrentApp(FactoryMap types = null, 
            int threadCount = 1, 
            bool blockUntilNextEvent = true,
            ThreadPriority priority = ThreadPriority.Normal,
            int stopCount = 1) : base(types)
        {
            _threadCount = threadCount;
            _priority = priority;
            _stopCount = stopCount;
            if (blockUntilNextEvent)
                _blockUntilNextEvent = new ManualResetEvent(false);
        }

        IEnumerable<Thread> _threads;
        public override void Start()
        {
            _threads = startThreads(_threadCount);
        }

        CancellationTokenSource _stop = new CancellationTokenSource();
        public override void Stop()
        {
            if (--_stopCount == 0)
            {
                _stop.Cancel();
                if (_blockUntilNextEvent != null)
                    _blockUntilNextEvent.Set();
            }
        }

        public override void AwaitCompletion()
        { 
            _stop.Token.WaitHandle.WaitOne();
        }


        public override void Schedule(IConcurrentObject who, Action what, Action<Exception> failure)
        {
            base.Schedule(who, what, failure);
            if (_blockUntilNextEvent != null)
                _blockUntilNextEvent.Set();

        }

        public override void Schedule(IConcurrentObject who, Action what, Action<Exception> failure, TimeSpan when)
        {
            Task.Delay(when, _stop.Token)
                .ContinueWith(task => Schedule(who, what, failure));
        }

        protected ThreadPriority _priority = ThreadPriority.Normal;
        protected virtual IEnumerable<Thread> startThreads(int threadCount)
        {
            var cancellation = _stop.Token;
            var result = new List<Thread>();
            for (int i = 0; i < threadCount; i++)
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
                            if (_blockUntilNextEvent != null)
                            {
                                WaitHandle.WaitAny(new WaitHandle[]
                                {
                                    _blockUntilNextEvent,
                                    cancellation.WaitHandle
                                });

                                _blockUntilNextEvent.Reset();
                            }
                            else
                                Thread.Sleep(0);
                            continue;
                        }

                        if (message.Who != null)
                        {
                            var ourObject = message.Who as ConcurrentObject;
                            if (ourObject != null)
                                ourObject.__enter(message.What, message.Failure);
                            else
                            {
                                //td: what?
                            }
                        }
                        else
                        {
                            try
                            {
                                message.What();
                            }
                            catch (Exception ex)
                            {
                                message.Failure?.Invoke(ex);
                            }
                        }
                    }
                });

                thread.Priority = _priority;
                thread.Start();
                result.Add(thread);
            }

            return result;
        }
    }
}
