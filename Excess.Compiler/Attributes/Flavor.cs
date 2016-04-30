using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess.Compiler.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class Flavor : Attribute
    {
        public string id;
    }
}
