using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace Excess.Concurrent.Runtime
{
    using System.Reflection;
    using System.Threading;
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

        public virtual void Spawn(IConcurrentObject @object)
        {
            var type = @object.GetType();
            if (_types != null && !_types.ContainsKey(type.Name))
                throw new ArgumentException($"{type.Name} is not registered to run in ths app");

            spawnObject(@object);
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
            return @object;
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
