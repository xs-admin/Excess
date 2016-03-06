using Excess.Compiler;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LanguageExtension
{
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using ExcessCompilation = ICompilationAnalysis<SyntaxToken, SyntaxNode, SemanticModel>;

    public class Extension
    {
        public static void Apply(ExcessCompilation compilation)
        {
            compilation
                .match<ClassDeclarationSyntax>(isConcurrentClass)
                    .then(jsConcurrentClass)
                .match<ClassDeclarationSyntax>(isConcurrentObject)
                    .then(jsConcurrentObject);
        }

        private static void jsConcurrentObject(SyntaxNode arg1, SemanticModel arg2, Scope arg3)
        {
            throw new NotImplementedException();
        }

        private static void jsConcurrentClass(SyntaxNode arg1, SemanticModel arg2, Scope arg3)
        {
            throw new NotImplementedException();
        }

        private static bool isConcurrentObject(ClassDeclarationSyntax arg1, SemanticModel arg2, Scope arg3)
        {
            throw new NotImplementedException();
        }

        private static bool isConcurrentClass(ClassDeclarationSyntax arg1, SemanticModel arg2, Scope arg3)
        {
            throw new NotImplementedException();
        }
    }
}
