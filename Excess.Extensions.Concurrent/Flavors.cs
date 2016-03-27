using Excess.Compiler.Roslyn;
using Excess.Concurrent.Runtime.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess.Extensions.Concurrent
{
    public static class Flavors
    {
        public static void Default(RoslynCompiler compiler)
        {
            Extension.Apply(compiler);
        }

        public static void Distributed(RoslynCompiler compiler)
        {
            Extension.Apply(compiler, new Options
            {
                GenerateInterface = true,
                GenerateRemote = true,
            });

            compiler.Environment()
                .dependency(new[]
                {
                    "System.Threading",
                    "System.Threading.Tasks",
                })
                .dependency<ConcurrentObject>(new string[]
                {
                    "Excess.Concurrent.Runtime",
                    "Excess.Concurrent.Runtime.Core",
                });
        }
    }
}
