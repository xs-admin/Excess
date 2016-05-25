using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Excess.Compiler;
using Excess.Compiler.Roslyn;

namespace xslang
{
    using CSharp = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
    using ExcessCompiler = ICompiler<SyntaxToken, SyntaxNode, SemanticModel>;

    public class DependencyInjection
    {
        static public void Apply(ExcessCompiler compiler)
        {
            var lexical = compiler.Lexical();
            var syntax = compiler.Syntax();

            lexical
                //methods 
                .match()
                    .token("inject", named: "keyword")
                    .token('{')
                    .then(lexical.transform()
                        .replace("keyword", "class __inject")
                        .then(InjectionClass));
        }

        private static SyntaxNode InjectionClass(SyntaxNode node, Scope scope)
        {
            var @class = node as ClassDeclarationSyntax;
            Debug.Assert(@class != null);

            var parentClass = null as ClassDeclarationSyntax;
            if (!ParseInjection(@class, scope, out parentClass))
                return node;

            var members = @class.Members;
            var document = scope.GetDocument<SyntaxToken, SyntaxNode, SemanticModel>();
            document.change(parentClass, AddFields(members));

            return CSharp.ConstructorDeclaration(parentClass.Identifier)
                .WithModifiers(RoslynCompiler.@public)
                .WithParameterList(CSharp.ParameterList(CSharp.SeparatedList(
                    members
                    .Select(member => InjectionParameter(member)))))
                .WithBody(CSharp.Block(
                    members
                    .Select(member => InjectionAssignment(member))));
        }

        private static SyntaxToken MemberIdentifier(MemberDeclarationSyntax member, out TypeSyntax type)
        {
            if (member is FieldDeclarationSyntax)
            {
                var declaration = (member as FieldDeclarationSyntax).Declaration;
                var variable = declaration
                    .Variables
                    .Single();

                type = declaration.Type;
                return variable.Identifier;
            }

            var property = (PropertyDeclarationSyntax)member;
            type = property.Type;
            return property.Identifier;
        }

        private static StatementSyntax InjectionAssignment(MemberDeclarationSyntax member)
        {
            var type = null as TypeSyntax;
            var identifier = MemberIdentifier(member, out type);
            return CSharp.ExpressionStatement(CSharp.AssignmentExpression(
                SyntaxKind.SimpleAssignmentExpression,
                CSharp.IdentifierName(identifier),
                CSharp.IdentifierName("__" + identifier.ToString())));
        }

        private static ParameterSyntax InjectionParameter(MemberDeclarationSyntax member)
        {
            var type = null as TypeSyntax;
            var identifier = MemberIdentifier(member, out type);
            return CSharp.Parameter(
                CSharp.List<AttributeListSyntax>(),
                CSharp.TokenList(),
                type,
                CSharp.Identifier("__" + identifier.ToString()),
                null);
        }

        private static Func<SyntaxNode, Scope, SyntaxNode> AddFields(IEnumerable<MemberDeclarationSyntax> members)
        {
            return (node, scope) => (node as ClassDeclarationSyntax)
                .AddMembers(members.ToArray());
        }

        private static bool ParseInjection(ClassDeclarationSyntax @class, Scope scope, out ClassDeclarationSyntax parentClass)
        {
            if (@class == null)
            {
                //td: error
                parentClass = null;
                return false;
            }

            parentClass = @class.Parent as ClassDeclarationSyntax;
            if (parentClass == null)
            {
                //td: error
                return false;
            }

            if (@class
                .Members
                .Any(member => !(
                    member is FieldDeclarationSyntax
                    || member is PropertyDeclarationSyntax)))
            {
                //td: error
                return false;
            }

            return true;
        }
    }
}