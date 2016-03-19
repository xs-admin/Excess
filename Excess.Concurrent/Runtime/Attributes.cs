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
        public Guid Id;
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class ConcurrentSingleton : Attribute
    {
        public Guid Id;

        public ConcurrentSingleton(Guid id)
        {
            Id = id;
        }
    }
}
