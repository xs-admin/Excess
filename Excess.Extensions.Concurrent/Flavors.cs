using Excess.Compiler.Attributes;
using Excess.Compiler.Roslyn;
using Excess.Concurrent.Runtime;

namespace Excess.Extensions.Concurrent
{
    [Compiler.Attributes.Extension]
    public static class Flavors
    {
        [Flavor]
        public static void Default(RoslynCompiler compiler)
        {
            Extension.Apply(compiler);
        }

        [Flavor]
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

        [Flavor]
        public static void Console(RoslynCompiler compiler)
        {
            Extension.Apply(compiler, new Options
            {
                GenerateAppProgram = true, 
                GenerateAppConstructor = false,
            });
        }

        [Flavor]
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
