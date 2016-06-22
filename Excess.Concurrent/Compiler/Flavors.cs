using Excess.Compiler;
using Excess.Compiler.Attributes;
using Excess.Compiler.Roslyn;
using Microsoft.CodeAnalysis;

namespace Excess.Concurrent.Compiler
{
    using CompilationAnalysis = ICompilationAnalysis<SyntaxToken, SyntaxNode, SemanticModel>;

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

        public static void PerformanceCompilation(CompilationAnalysis compilation)  => ConcurrentExtension.AppCompilation(compilation);

        [Flavor]
        public static void Console(RoslynCompiler compiler, Scope props)
        {
            ConcurrentExtension.Apply(compiler, new Options
            {
                GenerateAppProgram = true, 
                GenerateAppConstructor = false,
            }, props);
        }

        public static void ConsoleCompilation(CompilationAnalysis compilation) => ConcurrentExtension.AppCompilation(compilation);

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
