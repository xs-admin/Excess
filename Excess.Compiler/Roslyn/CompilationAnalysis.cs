using Excess.Compiler.Core;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess.Compiler.Roslyn
{
    public class CompilationAnalysis : CompilationAnalysisBase<SyntaxToken, SyntaxNode, SemanticModel>
    {
        public bool Analyze(SyntaxNode node, SemanticModel model, Scope scope)
        {
            var result = false;
            foreach (var matcher in _matchers)
            {
                result = result | matcher.matched(node, model, scope);
            }

            return result;
        }
    }
}
