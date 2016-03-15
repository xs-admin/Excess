using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Middleware
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ServerConfiguration : Attribute
    {
    }
}
