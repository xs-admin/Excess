using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Excess.Concurrent.Runtime.Core;

namespace Excess.Concurrent.Runtime
{
    using FactoryMap = Dictionary<string, Func<IConcurrentApp, object[], IConcurrentObject>>;

    public class TestConcurrentApp : BaseConcurrentApp
    {
        public TestConcurrentApp(FactoryMap types) : base(types)
        {
        }

        public override void Stop()
        {
            throw new InvalidOperationException("test app is synchronous");
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

                    //somebody waiting for a scheduled event in time
                    BlockingCollection<Event> blockingQueue = new BlockingCollection<Event>(_queue);
                    @event = blockingQueue.Take();
                }

                var ourObject = (ConcurrentObject)@event.Who;
                ourObject.__enter(@event.What, @event.Failure);
            }
        }

        //helper methods for Testing
        Dictionary<string, IConcurrentObject> _singletons = new Dictionary<string, IConcurrentObject>();
        public bool HasSingleton(string typeName)
        {
            return _singletons.ContainsKey(typeName);
        }

        public void AddSingleton(string typeName, IConcurrentObject concurrentObject)
        {
            _singletons[typeName] = concurrentObject;
            spawnObject(concurrentObject);
        }

        public IConcurrentObject GetSingleton(string typeName)
        {
            IConcurrentObject result;
            if (_singletons.TryGetValue(typeName, out result))
                return result;

            return null;
        }
    }

    public class TestDistributedApp : TestConcurrentApp, IDistributedApp
    {
        IDictionary<Guid, IConcurrentObject> _objects;
        public TestDistributedApp(FactoryMap types, IDictionary<Guid, IConcurrentObject> objects = null) : base(types)
        {
            _objects = objects ?? new Dictionary<Guid, IConcurrentObject>();
        }

        public Func<IDistributedApp, Exception> Connect { set { throw new NotImplementedException(); }}
        public Action<DistributedAppMessage> Receive { get { throw new NotImplementedException(); }}
        public Action<DistributedAppMessage> Send { set { throw new NotImplementedException(); }}

        public bool HasObject(Guid id)
        {
            return _objects.ContainsKey(id);
        }

        public void RegisterClass(Type type)
        {
            throw new NotImplementedException();
        }

        public void RegisterClass<T>() where T : IConcurrentObject
        {
            throw new NotImplementedException();
        }

        public void RegisterInstance(Guid id, IConcurrentObject @object)
        {
            throw new NotImplementedException();
        }

        public void RegisterRemoteClass(Type type)
        {
            throw new NotImplementedException();
        }

        public IDistributedApp WithInitializer(Action<IDistributedApp> initializer)
        {
            throw new NotImplementedException();
        }
    }
}
