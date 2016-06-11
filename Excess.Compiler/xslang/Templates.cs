using Excess.Compiler.Roslyn;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xslang
{
    using Microsoft.CodeAnalysis;
    using CSharp = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

    internal static class Templates
    {
        public static ClassDeclarationSyntax NamespaceFunction = Template.Parse(@"
            public partial static class Functions
            {
            }").Get<ClassDeclarationSyntax>();

        public static SyntaxToken ScopeToken = CSharp.ParseToken("__scope");
        public static IdentifierNameSyntax ScopeIdentifier = CSharp.IdentifierName("__scope");
        public static TypeSyntax ScopeType = CSharp.ParseTypeName("__scope");
    }
}
