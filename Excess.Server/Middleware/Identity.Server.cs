using System;
using System.Collections.Concurrent;
using Newtonsoft.Json.Linq;

namespace Middleware
{
    using NetMQ;
    using System.Threading.Tasks;
    using IdentityFunc = Action<string, string, Action<string>>;

    public interface IIdentityServer
    {
        void dispatch(Guid id, string method, string data, Action<string> response);
        void register(Guid id, IdentityFunc func);
    }

    public abstract class LocalIdentityServer : IIdentityServer
    {
        ConcurrentDictionary<Guid, IdentityFunc> _storage = new ConcurrentDictionary<Guid, IdentityFunc>();
        public void dispatch(Guid id, string method, string data, Action<string> response)
        {
            var func = null as IdentityFunc;
            if (_storage.TryGetValue(id, out func))
                func(method, data, response);
            else
                throw new InvalidOperationException($"not found: {id}");
        }

        public void register(Guid id, IdentityFunc func)
        {
            if (_storage.ContainsKey(id))
                throw new InvalidOperationException($"duplicate: {id}");

            _storage[id] = func;
        }
    }

    public class RemoteIdentityServer : IIdentityServer
    {
        public void dispatch(Guid id, string method, string data, Action<string> response)
        {
        }

        public void register(Guid id, IdentityFunc func)
        {
        }
    }

}
