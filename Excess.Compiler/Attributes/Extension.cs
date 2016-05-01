using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess.Compiler.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class Extension : Attribute
    {
        public string Id;
        public Extension(string id)
        {
            Id = id;
        }
    }
}
