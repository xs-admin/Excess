using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess.Runtime
{
    [AttributeUsage(AttributeTargets.Class)]
    public class AutoInit : Attribute
    {
        public AutoInit()
        {
        }
    }
}
