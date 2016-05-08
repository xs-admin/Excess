using Excess.Compiler;
using Microsoft.CodeAnalysis;
using Excess.Compiler.Core;

namespace Excess.Entensions.XS
{
    using ExcessCompiler = ICompiler<SyntaxToken, SyntaxNode, SemanticModel, Compiler.Roslyn.Compilation>;
    using Injector = ICompilerInjector<SyntaxToken, SyntaxNode, SemanticModel, Compiler.Roslyn.Compilation>;
    using DelegateInjector = DelegateInjector<SyntaxToken, SyntaxNode, SemanticModel, Compiler.Roslyn.Compilation>;

    public class XSLang
    {
        public static void Apply(ExcessCompiler compiler)
        {
            Functions.Apply(compiler);
            Members.Apply(compiler);
            Events.Apply(compiler);
            TypeDef.Apply(compiler);
            Arrays.Apply(compiler);
            Match.Apply(compiler);

            //base libs
            compiler.Environment()
                .dependency(new string[]
                {
                    "System",
                    "System.Collections.Generic",
                    "System.Linq"
                });
        }

        public static Injector Create()
        {
            return new DelegateInjector(compiler => Apply(compiler));
        }
    }
}
