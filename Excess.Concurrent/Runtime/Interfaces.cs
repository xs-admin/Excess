using System;

namespace Excess.Concurrent.Runtime
{
    public interface IConcurrentObject
    {
    }

    public interface IConcurrentApp
    {
        T Spawn<T>() where T : IConcurrentObject, new();
        T Spawn<T>(params object[] args) where T : IConcurrentObject;
        T Spawn<T>(T t) where T : IConcurrentObject;
        void Spawn(IConcurrentObject @object);
        IConcurrentObject Spawn(string type, params object[] args);

        void AddSpawnListener(Action<Guid, IConcurrentObject> listener);

        void Start();
        void Stop();
        void AwaitCompletion();

        void Schedule(IConcurrentObject who, Action what, Action<Exception> failure);
        void Schedule(IConcurrentObject who, Action what, Action<Exception> failure, TimeSpan when);

        void RegisterClass(Type type);
        void RegisterClass<T>() where T : IConcurrentObject;
        void RegisterRemoteClass(Type type);
        double rand();
    }
}
