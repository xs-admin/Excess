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
        public static TypeSyntax ScopeType = CSharp.ParseTypeName("__Scope");
        public static Template ScopeGet = Template.ParseExpression("__scope.get<__0>(__1)");
        public static StatementSyntax NewScope = CSharp.ParseStatement("var __newScope = new Scope(__scope);");
        public static ExpressionSyntax NewScopeValue = CSharp.IdentifierName("__newScope");
        public static Template AddToNewScope = Template.ParseStatement("__newScope.set(__0, __1);");
    }
}
