using Excess.Compiler;
using Excess.Compiler.Attributes;
using Excess.Compiler.Roslyn;

namespace Excess.Concurrent.Compiler
{
    [Extension("concurrent")]
    public static class Flavors
    {
        [Flavor]
        public static void Default(RoslynCompiler compiler, Scope scope)
        {
            ConcurrentExtension.Apply(compiler, scope);
        }

        [Flavor]
        public static void Performance(RoslynCompiler compiler, Scope props)
        {
            ConcurrentExtension.Apply(compiler, new Options
            {
                GenerateAppProgram = true, //td
                GenerateAppConstructor = false,
                BlockUntilNextEvent = false,
                AsFastAsPossible = true,
                GenerateInterface = false,
                GenerateRemote = false,
            }, props);
        }

        [Flavor]
        public static void Console(RoslynCompiler compiler, Scope props)
        {
            ConcurrentExtension.Apply(compiler, new Options
            {
                GenerateAppProgram = true, 
                GenerateAppConstructor = false,
            }, props);
        }

        [Flavor]
        public static void Distributed(RoslynCompiler compiler, Scope props)
        {
            ConcurrentExtension.Apply(compiler, new Options
            {
                GenerateInterface = true,
                GenerateRemote = true,
            }, props);
        }
    }
}
