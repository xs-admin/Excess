using Excess.Compiler.Roslyn;
using Excess.Concurrent.Runtime;

namespace Excess.Extensions.Concurrent
{
    public static class Flavors
    {
        public static void Default(RoslynCompiler compiler)
        {
            Extension.Apply(compiler);
        }

        public static void Performance(RoslynCompiler compiler)
        {
            Extension.Apply(compiler, new Options
            {
                GenerateAppProgram = true, //td
                GenerateAppConstructor = false,
                BlockUntilNextEvent = false,
                AsFastAsPossible = true,
                GenerateInterface = false,
                GenerateRemote = false,
            });
        }

        public static void Distributed(RoslynCompiler compiler)
        {
            Extension.Apply(compiler, new Options
            {
                GenerateInterface = true,
                GenerateRemote = true,
            });
        }
    }
}
