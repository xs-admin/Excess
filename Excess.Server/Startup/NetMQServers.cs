using Excess.Concurrent.Runtime;
using NetMQNode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Startup
{
    public static class NetMQ_RequestResponseServer
    {
        public static void Start(
            string url,
            int threads = 8,
            IEnumerable<Type> classes = null,
            IEnumerable<KeyValuePair<Guid, ConcurrentObject>> instances = null)
        {
            Task.Run(() =>
            {
                var server = new RequestResponseServer(url);
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

                server.Start();
            }); 
        }
    }
}
