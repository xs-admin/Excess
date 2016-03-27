using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess.Concurrent.Runtime
{
    public interface IConcurrentObject
    {
    }

    public interface IConcurrentApp
    {
        T Spawn<T>() where T : IConcurrentObject, new();
        T Spawn<T>(params object[] args) where T : IConcurrentObject;
        T Spawn<T>(T @object) where T : IConcurrentObject;
        IConcurrentObject Spawn(string type, params object[] args);

        void Start();
        void Stop();
        void AwaitCompletion();

        void Schedule(IConcurrentObject who, Action what, Action<Exception> failure);
        void Schedule(IConcurrentObject who, Action what, Action<Exception> failure, TimeSpan when);
    }
}
