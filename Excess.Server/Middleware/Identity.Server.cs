using System;
using System.Collections.Concurrent;

namespace Middleware
{
    using IdentityFunc = Action<string, string, Action<string>>;

    public interface IIdentityServer
    {
        void dispatch(Guid id, string method, string data, Action<string> response);
        void register(Guid id, IdentityFunc func);
    }

    public class BaseIdentityServer : IIdentityServer
    {
        ConcurrentDictionary<Guid, IdentityFunc> _storage = new ConcurrentDictionary<Guid, IdentityFunc>();
        public void dispatch(Guid id, string method, string data, Action<string> response)
        {
            var func = null as IdentityFunc;
            if (_storage.TryGetValue(id, out func))
                func(method, data, response);
            else 
                remoteDispatch(id, method, data, response);
        }

        public void register(Guid id, IdentityFunc func)
        {
            if (_storage.ContainsKey(id))
                throw new InvalidOperationException($"duplicate: {id}");

            _storage[id] = func;
            remoteRegister(id);
        }

        protected virtual void remoteDispatch(Guid id, string method, string data, Action<string> response)
        {
            throw new InvalidOperationException($"not found: {id}");
        }

        protected virtual void remoteRegister(Guid id)
        {
        }
    }
}
