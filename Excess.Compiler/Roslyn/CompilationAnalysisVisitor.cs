using Excess.Compiler.Core;
using Microsoft.CodeAnalysis;

namespace Excess.Compiler.Roslyn
{
    using CompilationAnalysis = CompilationAnalysisBase<SyntaxToken, SyntaxNode, SemanticModel>;
    using Compilation = ICompilation<SyntaxToken, SyntaxNode, SemanticModel>;

    public class CompilationAnalysisVisitor : SyntaxWalker
    {
        CompilationAnalysis _analysis;
        Compilation _compilation;
        Scope _scope;
        public CompilationAnalysisVisitor(CompilationAnalysis analysis, Compilation compilation, Scope scope)
        {
            _analysis = analysis;
            _compilation = compilation;
            _scope = scope;
        }

        public override void Visit(SyntaxNode node)
        {
            _analysis.Analyze(node, _compilation, _scope); //td: optimize
            base.Visit(node);
        }
    }
}
