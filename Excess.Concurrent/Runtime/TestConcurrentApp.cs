using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Excess.Concurrent.Runtime
{
    using Excess.Runtime;
    using FactoryMap = Dictionary<string, Func<IConcurrentApp, object[], IConcurrentObject>>;

    public class TestConcurrentApp : BaseConcurrentApp
    {
        public TestConcurrentApp(FactoryMap types, IInstantiator instantiator) : base(types, instantiator)
        {
        }

        public override void Stop()
        {
            throw new InvalidOperationException("test app is synchronous");
        }

        public override void AwaitCompletion()
        {
            //only wait for scheduled calls
            while (_scheduleCount > 0)
            {
                Thread.Sleep(100);
            }
        }

        public override void Schedule(IConcurrentObject who, Action what, Action<Exception> failure)
        {
            lock (this)
            {
                try
                {
                    what();
                }
                catch (Exception ex)
                {
                    failure(ex);
                }
            }
        }

        int _scheduleCount = 0;
        object _locker = new object();
        public override void Schedule(IConcurrentObject who, Action what, Action<Exception> failure, TimeSpan when)
        {
            _scheduleCount++;
            Task.Delay(when)
                .ContinueWith(task => 
                {
                    //this will run in a different thread, make synchronous
                    lock(_locker)
                    {
                        Schedule(who, what, failure);
                        _scheduleCount--;
                    }
                });
        }

        public override void Start()
        {
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
}
