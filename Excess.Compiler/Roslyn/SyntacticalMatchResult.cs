using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Excess.Compiler.Core;
using Microsoft.CodeAnalysis;

namespace Excess.Compiler.Roslyn
{
    public class SyntacticalMatchResult : BaseSyntacticalMatchResult<SyntaxNode>
    {
        protected override IEnumerable<SyntaxNode> children(SyntaxNode node)
        {
            return node.ChildNodes();
        }

        protected override IEnumerable<SyntaxNode> descendants(SyntaxNode node)
        {
            return node.DescendantNodes();
        }
}
}
