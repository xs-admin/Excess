using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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

        public static void AddError(this Scope scope, string id, string message, int offset, int length)
        {
            var doc = scope.GetDocument();
            doc.AddError(id, message, offset, length);
        }

        internal static void InitDocumentScope(this Scope scope)
        {
            scope.set("__additionalTypes", new List <TypeDeclarationSyntax>());
        }

        public static void AddType(this Scope scope, TypeDeclarationSyntax type)
        {
            var types = scope.find<List<TypeDeclarationSyntax>>("__additionalTypes");
            if (types == null)
                throw new InvalidOperationException("document scope not initialized");

            types.Add(type);
        }

        public static IEnumerable<TypeDeclarationSyntax> GetAdditionalTypes(this Scope scope)
        {
            var types = scope.find<List<TypeDeclarationSyntax>>("__additionalTypes");
            if (types == null)
                throw new InvalidOperationException("document scope not initialized");

            return types;
        }
    }
}
