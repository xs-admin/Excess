using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Middleware;
using Middleware.NetMQ;
using Excess.Concurrent.Runtime;

namespace Startup
{
    public static class NetMQNode
    {
        public static void Start(
            string url,
            string identityUrl,
            int threads = 2,
            IEnumerable<Type> classes = null,
            IEnumerable<KeyValuePair<Guid, ConcurrentObject>> instances = null)
        {
            var identity = new IdentityClient();
            var server = new ConcurrentServer(identity);
            if (classes != null)
            {
                foreach (var @class in classes)
                    server.RegisterClass(@class);
            }

            if (instances != null)
            {
                foreach (var instance in instances)
                    server.RegisterInstance(instance.Key, instance.Value);
            }

            identity.Start(url, identityUrl);
        }
    }
}
