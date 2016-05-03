using Excess.Compiler.Attributes;
using Excess.Compiler.Roslyn;
using Excess.Concurrent.Runtime;
using System.Collections.Generic;

namespace Excess.Extensions.Concurrent
{
    [Extension("concurrent")]
    public static class Flavors
    {
        [Flavor]
        public static void Default(RoslynCompiler compiler, Dictionary<string, object> props)
        {
            ConcurrentExtension.Apply(compiler, props: props);
        }

        [Flavor]
        public static void Performance(RoslynCompiler compiler, Dictionary<string, object> props)
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
        public static void Console(RoslynCompiler compiler, Dictionary<string, object> props)
        {
            ConcurrentExtension.Apply(compiler, new Options
            {
                GenerateAppProgram = true, 
                GenerateAppConstructor = false,
            }, props);
        }

        [Flavor]
        public static void Distributed(RoslynCompiler compiler, Dictionary<string, object> props)
        {
            ConcurrentExtension.Apply(compiler, new Options
            {
                GenerateInterface = true,
                GenerateRemote = true,
            }, props);
        }
    }
}
