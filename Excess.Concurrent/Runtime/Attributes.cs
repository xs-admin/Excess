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
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class ConcurrentSingleton : Attribute
    {
    }
}
