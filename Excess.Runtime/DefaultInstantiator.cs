using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess.Runtime
{
    public class DefaultInstantiator : IInstantiator
    {
        public object Create(Type type)
        {
            return Activator.CreateInstance(type);
        }
    }
}
