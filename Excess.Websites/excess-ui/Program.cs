using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Excess.Server.Middleware;
using System.IO;

namespace excess_ui
{
    class Program
    {
        static void Main(string[] args)
        {
            Loader.Run(new[] { typeof(Program).Assembly }, args);
        }
    }
}

