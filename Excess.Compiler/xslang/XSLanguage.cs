using Microsoft.CodeAnalysis;
using Excess.Compiler;
using Excess.Compiler.Roslyn;

namespace xslang
{
    using Compiler = ICompiler<SyntaxToken, SyntaxNode, SemanticModel>;

    public class XSLanguage
    {
        public static string[] Keywords = new[]
        {
            "function",
            "method",
            "property",
            "on",
            "match",
            "constructor",
            "var",
            "inject"
        };

        public static Compiler CreateCompiler()
        {
            var result = new RoslynCompiler();
            Apply(result);
            return result;
        }

        public static void Apply(ICompiler<SyntaxToken, SyntaxNode, SemanticModel> compiler)
        {
            DependencyInjection.Apply(compiler);
            Arrays.Apply(compiler);
            Events.Apply(compiler);
            DependencyInjection.Apply(compiler);
            Match.Apply(compiler);
            Members.Apply(compiler);
        }
    }
}
