using Excess.Compiler.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess.Compiler.Roslyn
{
    public class CompilationAnalysis : CompilationAnalysisBase<SyntaxToken, SyntaxNode, Compilation>
    {
        public bool Analyze(SyntaxNode node, Compilation compilation, Scope scope)
        {
            var result = false;
            foreach (var matcher in _matchers)
            {
                result = result | matcher.matched(node, compilation, scope);
            }

            return result;
        }

        public bool isNeeded()
        {
            return _matchers.Any() || _after.Any();
        }

        public void Finish(Compilation compilation, Scope scope)
        {
            foreach (var after in _after)
            {
                after(compilation, scope);
            }
        }
    }
}
