using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Excess.Compiler;
using Excess.Compiler.Roslyn;

namespace Excess.Entensions.XS
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
            var members = ParseInjection(@class, scope, out parentClass);
            if (members == null)
                return node;

            var document = scope.GetDocument<SyntaxToken, SyntaxNode, SemanticModel>();
            document.change(parentClass, AddFields(members));

            return CSharp.ConstructorDeclaration(parentClass.Identifier)
                .WithParameterList(CSharp.ParameterList(CSharp.SeparatedList(
                    members
                    .Select(member => InjectionParameter(member)))))
                .WithBody(CSharp.Block(
                    members
                    .Select(member => InjectionAssignment(member))));
        }

        private static StatementSyntax InjectionAssignment(FieldDeclarationSyntax member)
        {
            var variable = member.Declaration
                .Variables
                .Single();

            return CSharp.ExpressionStatement(CSharp.AssignmentExpression(
                SyntaxKind.SimpleAssignmentExpression,
                CSharp.IdentifierName(variable.Identifier),
                CSharp.IdentifierName("__" + variable.Identifier.ToString())));
        }

        private static ParameterSyntax InjectionParameter(FieldDeclarationSyntax member)
        {
            var variable = member.Declaration
                .Variables
                .Single();

            return CSharp.Parameter(
                CSharp.List<AttributeListSyntax>(),
                RoslynCompiler.@public,
                member.Declaration.Type,
                CSharp.Identifier("__" + variable.Identifier.ToString()),
                variable.Initializer);
        }

        private static Func<SyntaxNode, Scope, SyntaxNode> AddFields(IEnumerable<FieldDeclarationSyntax> members)
        {
            return (node, scope) => (node as ClassDeclarationSyntax)
                .AddMembers(members.ToArray());
        }

        private static IEnumerable<FieldDeclarationSyntax> ParseInjection(ClassDeclarationSyntax @class, Scope scope, out ClassDeclarationSyntax parentClass)
        {
            if (@class == null)
            {
                //td: error
                parentClass = null;
                return null;
            }

            parentClass = @class.Parent as ClassDeclarationSyntax;
            if (parentClass == null)
            {
                //td: error
                return null;
            }

            if (@class
                .Members
                .Any(member => !(member is FieldDeclarationSyntax)))
            {
                //td: error
                return null;
            }

            return @class.Members.Select(member => (FieldDeclarationSyntax)member);
        }
    }
}