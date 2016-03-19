using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess.Concurrent.Runtime
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class Concurrent : Attribute
    {
        public string id;
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class ConcurrentSingleton : Attribute
    {
        public string id;

        public ConcurrentSingleton(string id_)
        {
            id = id_;
        }
    }
}
