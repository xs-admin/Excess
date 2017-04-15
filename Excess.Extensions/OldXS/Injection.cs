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
    using Roslyn = RoslynCompiler;
    using ExcessCompiler = ICompiler<SyntaxToken, SyntaxNode, SemanticModel>;
    using Excess.Runtime;
    public class DependencyInjection
    {
        static public void Apply(ExcessCompiler compiler)
        {
            var lexical = compiler.Lexical();
            lexical
                .match()
                    .token("inject", named: "keyword")
                    .enclosed('{', '}', end: "lastBrace")
                    .then(lexical.transform()
                        .replace("keyword", "__injectFunction __inject = _ => ")
                        .insert(";", after: "lastBrace")
                        .then(ProcessInjection));

            //td: 
            //compiler.Environment()
            //    .dependency<__Scope>("Excess.Runtime");
        }

        private static SyntaxNode ProcessInjection(SyntaxNode node, Scope scope)
        {
            var document = scope.GetDocument<SyntaxToken, SyntaxNode, SemanticModel>();
            var field = node as FieldDeclarationSyntax;
            if (field == null)
            {
                //must be injecting on a function
                var toInject = (((node as LocalDeclarationStatementSyntax)
                    ?.Declaration
                    .Variables
                    .SingleOrDefault()
                        ?.Initializer
                        ?.Value as SimpleLambdaExpressionSyntax)
                            ?.Body as BlockSyntax)
                            ?.Statements;

                if (toInject == null)
                {
                    //td: error
                    return node;
                }

                document.change(node.Parent, FunctionInjection(node, toInject));
                return node;
            }

            //otherwise, class injection
            var parentClass = null as ClassDeclarationSyntax;
            if (!ParseInjection(field, scope, out parentClass))
                return node;

            var memberStatements = (((node as FieldDeclarationSyntax)
                ?.Declaration
                .Variables
                .SingleOrDefault()
                    ?.Initializer
                    ?.Value as SimpleLambdaExpressionSyntax)
                        ?.Body as BlockSyntax)
                        ?.Statements;

            if (memberStatements == null)
            {
                //td: error
                return node;
            }

            var members = membersFromStatements(memberStatements);
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

        private static IEnumerable<MemberDeclarationSyntax> membersFromStatements(SyntaxList<StatementSyntax>? memberStatements)
        {
            Debug.Assert(memberStatements.HasValue);
            foreach (var injectionStatement in memberStatements.Value)
            {
                var injectionDeclaration = (injectionStatement as LocalDeclarationStatementSyntax);

                if (injectionDeclaration == null)
                {
                    //td: error
                    continue;
                }

                if (injectionDeclaration.Declaration.Variables.Count != 1)
                {
                    //td: error
                    continue;
                }

                var injectionVariable = injectionDeclaration
                    .Declaration
                    .Variables
                    .Single();

                var type = injectionDeclaration.Declaration.Type;
                if (type.ToString() == "var")
                {
                    //td: error
                    continue;
                }

                var name = injectionVariable.Identifier;
                yield return Templates
                    .InjectionMember
                    .Get<MemberDeclarationSyntax>(type, name);
            }
        }

        private static Func<SyntaxNode, Scope, SyntaxNode> FunctionInjection(SyntaxNode toReplace, SyntaxList<StatementSyntax>? toInject)
        {
            return (node, scope) =>
            {
                var scopeParameter = node
                    .Ancestors()
                    .OfType<MethodDeclarationSyntax>()
                    .FirstOrDefault()
                        ?.ParameterList
                        .Parameters
                        .LastOrDefault();

                if (scopeParameter == null || scopeParameter.Type.ToString() != "__Scope")
                {
                    //td: error
                    return node;
                }

                var block = node as BlockSyntax;
                Debug.Assert(block != null); //td: error

                var variables = new List<StatementSyntax>();
                foreach (var injectionStatement in toInject.Value)
                {
                    var injectionDeclaration = (injectionStatement as LocalDeclarationStatementSyntax);

                    if (injectionDeclaration == null)
                    {
                        //td: error
                        continue;
                    }

                    if (injectionDeclaration.Declaration.Variables.Count != 1)
                    {
                        //td: error
                        continue;
                    }

                    var injectionVariable = injectionDeclaration
                        .Declaration
                        .Variables
                        .Single();

                    var type = injectionDeclaration.Declaration.Type;
                    if (type.ToString() == "var")
                    {
                        //td: error
                        continue;
                    }

                    var name = injectionVariable.Identifier;
                    variables.Add(injectionDeclaration
                        .WithDeclaration(injectionDeclaration.Declaration
                        .WithVariables(CSharp.SeparatedList(new[] {
                            injectionVariable.WithInitializer(CSharp.EqualsValueClause(
                                Templates.ScopeGet.Get<ExpressionSyntax>(
                                    type, 
                                    RoslynCompiler.Quoted(name.ToString()))))}))));
                }

                return block.WithStatements(CSharp.List(
                    FunctionInjectionStatements(block, toReplace, variables)));
            };
        }

        private static IEnumerable<StatementSyntax> FunctionInjectionStatements(BlockSyntax block, SyntaxNode toReplace, IEnumerable<StatementSyntax> toReplaceWith)
        {
            foreach (var statement in block.Statements)
            {
                if (Roslyn.SameNode(statement, toReplace))
                {
                    foreach (var newStatement in toReplaceWith)
                        yield return newStatement;
                }
                else
                    yield return statement;
            }
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

        private static bool ParseInjection(FieldDeclarationSyntax field, Scope scope, out ClassDeclarationSyntax parentClass)
        {
            if (field == null)
            {
                //td: error
                parentClass = null;
                return false;
            }

            parentClass = field.Parent as ClassDeclarationSyntax;
            if (parentClass == null)
            {
                //td: error
                return false;
            }

            return true;
        }
    }
}