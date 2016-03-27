using Excess.Concurrent.Runtime.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Excess.Concurrent.Runtime
{
    public class TestConcurrentApp : BaseConcurrentApp
    {
        public TestConcurrentApp() : base(null)
        {
        }

        public override void AwaitCompletion()
        {
            throw new InvalidOperationException("test app is synchronous");
        }

        int _scheduleCount = 0;
        public override void Schedule(IConcurrentObject who, Action what, Action<Exception> failure, TimeSpan when)
        {
            _scheduleCount++;
            Task.Delay(when)
                .ContinueWith(task => 
                {
                    Schedule(who, what, failure);
                    _scheduleCount--;
                });
        }

        public override void Start()
        {
            while (!_queue.IsEmpty || _scheduleCount > 0)
            {
                var @event = null as Event;
                if (!_queue.TryDequeue(out @event))
                {
                    if (_scheduleCount == 0)
                        throw new InvalidOperationException("test app must not be consumed from elsewhere");

                    BlockingCollection<Event> blockingQueue = new BlockingCollection<Event>(_queue);
                    @event = blockingQueue.Take();
                }

                var ourObject = (ConcurrentObject)@event.Who;
                ourObject.__enter(@event.What, @event.Failure);
            }
        }

        public override void Stop()
        {
        }
    }
}
