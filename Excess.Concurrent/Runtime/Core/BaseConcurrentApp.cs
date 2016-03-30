using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace Excess.Concurrent.Runtime.Core
{
    using FactoryFunction = Func<IConcurrentApp, object[], IConcurrentObject>;
    using FactoryMap = Dictionary<string, Func<IConcurrentApp, object[], IConcurrentObject>>;

    public abstract class BaseConcurrentApp : IConcurrentApp
    {
        protected FactoryMap _types;
        public BaseConcurrentApp(FactoryMap types)
        {
            _types = types;
        }

        //type management
        //caution: _types is expected to be immutable after Start()
        //it is also, by default, null. subclasses can decide to opt out or in.
        public virtual IConcurrentObject Spawn(string type, params object[] args)
        {
            if (_types != null)
            {
                var func = null as FactoryFunction;
                if (_types.TryGetValue(type, out func))
                    return spawnObject(func(this, args)); 
            }

            throw new ArgumentException($"{type} can not be created in this app");
        }

        public virtual T Spawn<T>() where T : IConcurrentObject, new()
        {
            if (_types != null)
                return (T)Spawn(typeof(T).Name);

            return spawnObject(new T());
        }

        public virtual T Spawn<T>(params object[] args) where T : IConcurrentObject
        {
            var type = typeof(T);
            if (_types != null)
                return (T)Spawn(type.Name, args);

            var constructor = type
                .GetConstructor(args
                    .Select(arg => arg.GetType())
                    .ToArray());

            if (constructor == null)
                throw new ArgumentException($"{type.Name} can not be created with these arguments");

            return spawnObject((T)constructor.Invoke(args));
        }

        public virtual T Spawn<T>(T @object) where T : IConcurrentObject
        {
            var type = typeof(T);
            if (_types != null && !_types.ContainsKey(typeof(T).Name))
                throw new ArgumentException($"{type.Name} is not registered to run in ths app");

            return spawnObject(@object);
        }

        public abstract void Start();
        public abstract void Stop();
        public abstract void AwaitCompletion();

        public virtual void Schedule(IConcurrentObject who, Action what, Action<Exception> failure)
        {
            _queue.Enqueue(new Event
            {
                Who = who,
                What = what,
                Failure = failure,
            });
        }

        public abstract void Schedule(IConcurrentObject who, Action what, Action<Exception> failure, TimeSpan when);

        protected virtual T spawnObject<T>(T @object) where T : IConcurrentObject
        {
            var ourObject = @object as ConcurrentObject;
            if (ourObject == null)
                return default(T);

            ourObject.__start(this);
            return @object;
        }

        //event queue
        protected class Event
        {
            public int Tries { get; set; }
            public IConcurrentObject Who { get; set; }
            public Action What { get; set; }
            public Action<Exception> Failure { get; set; }
        }

        protected ConcurrentQueue<Event> _queue = new ConcurrentQueue<Event>();
    }

}
