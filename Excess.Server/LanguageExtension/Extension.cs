using System;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;
using Excess.Compiler;
using Excess.Compiler.Roslyn;

namespace LanguageExtension
{
    using ExcessCompiler = ICompiler<SyntaxToken, SyntaxNode, SemanticModel>;
    using ExcessCompilation = ICompilationAnalysis<SyntaxToken, SyntaxNode, SemanticModel>;
    using System.Diagnostics;

    public class Extension
    {
        public static void Apply(ExcessCompiler compiler)
        {
            compiler.Syntax()
                .extension("server", ExtensionKind.Type, CompileConfig);
        }

        public static void Apply(ExcessCompilation compilation)
        {
            compilation
                .match<ClassDeclarationSyntax>(isConcurrentClass)
                    .then(jsConcurrentClass)
                .match<ClassDeclarationSyntax>(isConcurrentObject)
                    .then(jsConcurrentObject);
        }

        private static SyntaxNode CompileConfig(SyntaxNode node, Scope scope, SyntacticalExtension<SyntaxNode> data)
        {
            Debug.Assert(node is MethodDeclarationSyntax);
            var methodSyntax = node as MethodDeclarationSyntax;

            var classToAdd = Templates
                .ConfigClass
                .Get<ClassDeclarationSyntax>(data.Identifier);

            var document = scope.GetDocument();
            document.change(node.Parent, RoslynCompiler.AddType(classToAdd));
            document.change(node.Parent, RoslynCompiler.RemoveMember(node));

            return node;
        }

        private static bool isConcurrentObject(ClassDeclarationSyntax arg1, SemanticModel arg2, Scope arg3)
        {
            throw new NotImplementedException();
        }

        private static void jsConcurrentObject(SyntaxNode arg1, SemanticModel arg2, Scope arg3)
        {
            throw new NotImplementedException();
        }

        private static bool isConcurrentClass(ClassDeclarationSyntax arg1, SemanticModel arg2, Scope arg3)
        {
            throw new NotImplementedException();
        }

        private static void jsConcurrentClass(SyntaxNode arg1, SemanticModel arg2, Scope arg3)
        {
            throw new NotImplementedException();
        }
    }
}
