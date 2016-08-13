using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Excess.Runtime;
using Excess.Server.Middleware;
using metaprogramming.server;
using System.Diagnostics;

namespace metaprogramming
{
    class Program
    {
        static void Main(string[] args)
        {
            Loader.Run(new[] { typeof(Program).Assembly }, args);
        }
    }
}

