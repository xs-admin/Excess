using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess.Compiler.Core
{
    public static class ScopeExtensions
    {
        public static void AddInstanceInitializer(this Scope scope, SyntaxNode node)
        {
            List<SyntaxNode> _instanceInitializer = scope.find<List<SyntaxNode>>("_instanceInitializer");
            Debug.Assert(_instanceInitializer != null);

            _instanceInitializer.Add(node);
        }

        public static List<SyntaxNode> GetInstanceInitializers(this Scope scope)
        {
            List<SyntaxNode> _instanceInitializer = scope.find<List<SyntaxNode>>("_instanceInitializer");
            Debug.Assert(_instanceInitializer != null);

            return _instanceInitializer;
        }

        public static void AddInstanceDeclaration(this Scope scope, SyntaxNode node)
        {
            List<SyntaxNode> _instanceDeclaration = scope.find<List<SyntaxNode>>("_instanceDeclaration");
            Debug.Assert(_instanceDeclaration != null);

            _instanceDeclaration.Add(node);
        }

        public static List<SyntaxNode> GetInstanceDeclarations(this Scope scope)
        {
            List<SyntaxNode> _instanceDeclaration = scope.find<List<SyntaxNode>>("_instanceDeclaration");
            Debug.Assert(_instanceDeclaration != null);

            return _instanceDeclaration;
        }

        internal static void InitInstance(this Scope scope)
        {
            Debug.Assert(scope.find<List<SyntaxNode>>("_instanceDeclaration") == null);

            scope.set("_instanceDeclaration", new List<SyntaxNode>());
            scope.set("_instanceInitializer", new List<SyntaxNode>());
        }
    }
}
