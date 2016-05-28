using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess.Runtime
{
    public interface IInstantiator
    {
        object Create(Type type);
    }
}
