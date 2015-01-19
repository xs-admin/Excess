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
        public SyntaxRewriter(IEnumerable<ISyntacticalMatch<SyntaxNode>> matchers)
        {
            _matchers = matchers;
        }

        public override SyntaxNode Visit(SyntaxNode node)
        {
            //TD: !!!
            //foreach (var matcher in _matchers)
            //{
            //    if (matcher.matches(node, result))
            //    {
            //        if (result.prePocess())
            //        {
            //            var pre = matcher.transform(node, result);
            //            return base.Visit(pre);
            //        }
            //        else
            //        {
            //            var post = base.Visit(node);
            //            return matcher.transform(post, result);
            //        }
            //    }
            //}

            return base.Visit(node);
        }
    }
}
