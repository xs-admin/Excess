using Excess.Concurrent.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Startup
{
    public static class NetMQ_RequestResponseClient
    {
        public static void Start(
            string url,
            int threads = 8,
            IEnumerable<Type> classes = null,
            IEnumerable<KeyValuePair<Guid, ConcurrentObject>> instances = null)
        {
            throw new NotImplementedException();
        }
    }

    public static class NetMQ_RequestResponseServer
    {
        public static void Start(
            string url,
            int threads = 8,
            IEnumerable<Type> classes = null,
            IEnumerable<KeyValuePair<Guid, ConcurrentObject>> instances = null)
        {
            throw new NotImplementedException();
        }
    }
}
