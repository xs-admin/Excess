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
        IEventBus _events;
        Scope _scope;

        public SyntaxRewriter(IEventBus events, Scope scope,
                              IEnumerable<ISyntacticalMatch<SyntaxNode>> matchers, 
                              IDictionary<int, Func<SyntaxNode, SyntaxNode>> handlers)
        {
            _matchers = matchers;
            _handlers = handlers;
            _events   = events;
            _scope    = scope;
        }

        public override SyntaxNode Visit(SyntaxNode node)
        {
            if (node == null)
                return  null;

            int nodeID = RoslynCompiler.GetSyntaxId(node);
            if (nodeID >= 0)
            {
                Func<SyntaxNode, SyntaxNode> handler;
                if (_handlers.TryGetValue(nodeID, out handler))
                    return handler(base.Visit(node));
            }

            ISyntacticalMatchResult<SyntaxNode> matchResult = new SyntacticalMatchResult(_scope, _events);
            foreach (var matcher in _matchers)
            {
                if (matcher.matches(node, matchResult))
                {
                    matchResult.Node = node;
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
