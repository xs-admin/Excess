using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess.Compiler.Roslyn
{
    public class CompilationAnalysisVisitor : CSharpSyntaxVisitor
    {
        CompilationAnalysis _analysis;
        SemanticModel _model;
        Scope _scope;
        public CompilationAnalysisVisitor(CompilationAnalysis analysis, SemanticModel model, Scope scope)
        {
            _analysis = analysis;
            _model = model;
            _scope = scope;
        }

        public override void Visit(SyntaxNode node)
        {
            _analysis.Analyze(node, _model, _scope); //td: optimize
        }
    }
}
