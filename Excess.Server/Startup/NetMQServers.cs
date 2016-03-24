using Middleware;
using Middleware.NetMQ;
using Excess.Concurrent.Runtime;
using System;

namespace Startup
{
    public static class NetMQNode
    {
        public static void Start(
            string url,
            string identityUrl,
            IInstantiator instantiator,
            int threads = 2,
            Action<Exception> started = null)
        {
            var identity = new IdentityClient(url);
            var server = new ConcurrentServer(identity);
            var classes = instantiator.GetConcurrentClasses();
            if (classes != null)
            {
                foreach (var @class in classes)
                    server.RegisterClass(@class);
            }

            var instances = instantiator.GetConcurrentInstances();
            if (instances != null)
            {
                foreach (var instance in instances)
                    server.RegisterInstance(instance.Key, instance.Value);
            }

            server.StartListening();
            identity.Start(url, identityUrl, (ReferenceInstantiator)instantiator, started);
        }
    }
}
