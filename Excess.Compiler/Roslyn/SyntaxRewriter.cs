using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess.Compiler.Roslyn
{
    public class SyntaxRewriter : CSharpSyntaxRewriter
    {
        IEnumerable<ISyntacticalMatch<SyntaxNode>> _matchers;
        IDictionary<int, Func<SyntaxNode, SyntaxNode>> _handlers;

        public SyntaxRewriter(IEnumerable<ISyntacticalMatch<SyntaxNode>> matchers, IDictionary<int, Func<SyntaxNode, SyntaxNode>> handlers)
        {
            _matchers = matchers;
            _handlers = handlers;
        }

        public override SyntaxNode Visit(SyntaxNode node)
        {
            int nodeID = Compiler.GetSyntaxId(node);
            if (nodeID >= 0)
            {
                Func<SyntaxNode, SyntaxNode> handler;
                if (_handlers.TryGetValue(nodeID, out handler))
                    return handler(base.Visit(node));
            }

            ISyntacticalMatchResult <SyntaxNode> matchResult = new SyntacticalMatchResult();
            foreach (var matcher in _matchers)
            {
                if (matcher.matches(node, matchResult))
                {
                    if (matchResult.Preprocess)
                    {
                        var pre = matcher.transform(node, matchResult);
                        return base.Visit(pre);
                    }
                    else
                    {
                        var post = base.Visit(node);
                        return matcher.transform(post, matchResult);
                    }
                }
            }

            return base.Visit(node);
        }
    }
}
