using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace Excess.Concurrent.Runtime
{
    using FactoryFunction = Func<IConcurrentApp, object[], IConcurrentObject>;
    using FactoryMap = Dictionary<string, Func<IConcurrentApp, object[], IConcurrentObject>>;
    using MethodFunc = Action<IConcurrentObject, object[], Action<object>, Action<Exception>>;

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
        public virtual IConcurrentObject Spawn(string typeName, params object[] args)
        {
            if (_types != null)
            {
                var func = null as FactoryFunction;
                if (_types.TryGetValue(typeName, out func))
                    return spawnObject(func(this, args)); 
            }

            throw new ArgumentException($"{typeName} can not be created in this app");
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
            if (_types != null && _types.ContainsKey(type.Name))
                return (T)Spawn(type.Name, args);

            var constructor = type
                .GetConstructor(args
                    .Select(arg => arg.GetType())
                    .ToArray());

            if (constructor == null)
                throw new ArgumentException($"{type.Name} can not be created with these arguments");

            return spawnObject((T)constructor.Invoke(args));
        }

        public virtual T Spawn<T>(T t) where T : IConcurrentObject
        {
            spawnObject(t);
            return t;
        }

        public virtual void Spawn(IConcurrentObject @object)
        {
            //td: consider the policies based on the type list
            //var type = @object.GetType();
            //if (_types != null && !_types.ContainsKey(type.Name))
            //    throw new ArgumentException($"{type.Name} is not registered to run in ths app");

            spawnObject(@object);
        }

        List<Action<Guid, IConcurrentObject>> _spawnListeners = new List<Action<Guid, IConcurrentObject>>();
        public void AddSpawnListener(Action<Guid, IConcurrentObject> listener)
        {
            notWhileRunning();
            _spawnListeners.Add(listener);
        }

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

        //start a concurrent object
        protected virtual T spawnObject<T>(T @object) where T : IConcurrentObject
        {
            var ourObject = @object as ConcurrentObject;
            if (ourObject == null)
                return default(T);

            ourObject.__start(this);

            //notify, if any
            if (_spawnListeners.Any())
            {
                var id = getUniqueId(ourObject);
                foreach (var listener in _spawnListeners)
                {
                    try
                    {
                        listener(id, @object);
                    }
                    catch
                    {
                    }
                }
            }

            return @object;
        }

        protected virtual Guid getUniqueId(ConcurrentObject @object)
        {
            var idField = @object.GetType().GetField("__ID");
            if (idField != null)
                return (Guid)idField.GetValue(@object);

            return Guid.NewGuid();
        }

        protected bool _running = false;
        public virtual void Start()
        {
            doStart();
            _running = true;
        }

        protected virtual void doStart()
        {
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

        Dictionary<Type, Dictionary<string, MethodFunc>> _methods = new Dictionary<Type, Dictionary<string, MethodFunc>>();
        public void RegisterClass(Type type)
        {
            notWhileRunning();

            var publicMethods = type
                .GetMethods()
                .Where(method => isConcurrent(method));

            var methodFuncs = new Dictionary<string, MethodFunc>();
            foreach (var method in publicMethods)
            {
                var parameters = method
                    .GetParameters();

                var paramCount = parameters.Length - 3;
                var paramNames = parameters
                    .Take(paramCount)
                    .Select(param => param.Name)
                    .ToArray();

                var paramTypes = parameters
                    .Take(paramCount)
                    .Select(param => param.ParameterType)
                    .ToArray();

                methodFuncs[method.Name] = (@object, args, success, failure) =>
                {
                    Schedule(@object, () =>
                    {
                        var result = method.Invoke(@object, args);
                        success(result);
                    }, failure);
                };
            }

            _methods[type] = methodFuncs;
            _types[type.Name] = (app, args) => (IConcurrentObject)Activator.CreateInstance(type, args);
        }

        public void RegisterClass<T>() where T : IConcurrentObject
        {
            RegisterClass(typeof(T));
        }

        public void RegisterRemoteClass(Type type)
        {
            notWhileRunning();
            _types[type.Name] = (app, args) => { throw new NotImplementedException(); };
        }

        private void notWhileRunning()
        {
            if (_running)
                throw new InvalidProgramException("operation not permitted while the app is running");
        }

        private bool isConcurrent(MethodInfo method)
        {
            if (!method.IsPublic)
                return false;

            var parameters = method
                .GetParameters()
                .ToArray();

            var count = parameters.Length;
            return count > 2
                && parameters[count - 2].ParameterType == typeof(Action<object>)
                && parameters[count - 1].ParameterType == typeof(Action<Exception>);
        }
    }
}
