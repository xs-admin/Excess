using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess.Compiler.Roslyn
{
    public static class ScopeExtensions
    {
        public static RoslynDocument GetDocument(this Scope scope)
        {
            var document = scope.GetDocument<SyntaxToken, SyntaxNode, SemanticModel>();
            return document as RoslynDocument;
        }

        public static void AddError(this Scope scope, string id, string message, SyntaxNode location)
        {
            var doc = scope.GetDocument();
            doc.AddError(id, message, location);
        }
    }
}
