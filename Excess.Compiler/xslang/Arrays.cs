using Excess.Compiler;
using Microsoft.CodeAnalysis;

namespace xslang
{
    using ExcessCompiler = ICompiler<SyntaxToken, SyntaxNode, SemanticModel>;

    public class Arrays
    {
        static public void Apply(ExcessCompiler compiler)
        {
            compiler.Lexical()
                .match()
                    .any(new[] { '(', '=', ',' }, named: "start", matchDocumentStart: true)
                    .enclosed('[', ']', start: "open", end: "close")
                    .then(compiler.Lexical().transform()
                        .insert("new []", after: "start")
                        .replace("open", "{")
                        .replace("close", "}"));
        }
    }
}