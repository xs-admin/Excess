using Excess.Compiler.Core;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess.Compiler.Roslyn
{
    class RoslynDocument : BaseDocument<SyntaxToken, SyntaxNode, SemanticModel>
    {
        public RoslynDocument(Scope scope) : base(scope)
        {
        }

        public string LexicalText { get; internal set; }
        public SyntaxNode Root { get; internal set; }

        protected override SyntaxNode processAnnotations(SyntaxNode node, Dictionary<string, SourceSpan> annotations)
        {
            throw new NotImplementedException();
        }

        protected override SyntaxNode transform(SyntaxNode node, Dictionary<int, Func<SyntaxNode, Scope, SyntaxNode>> transformers)
        {
            throw new NotImplementedException();
        }
    }
}
