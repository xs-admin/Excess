using Excess.Compiler.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess.Compiler.XS
{
    using CSharp = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
    using ExcessCompiler = ICompiler<SyntaxToken, SyntaxNode, SemanticModel>;

    class Members
    {
        static public void Apply(ExcessCompiler compiler)
        {
            var lexical = compiler.Lexical();
            var sintaxis = compiler.Sintaxis();

            lexical
                //methods 
                .match()
                    .token("method", named: "keyword")
                    .identifier(named: "id")
                    .enclosed('(', ')')
                    .token('{')
                    .then(lexical.transform()
                        .remove("keyword")
                        .then(Method, referenceToken: "id"))

                //properties
                .match()
                    .token("property")
                    .identifier(named: "id")
                    .token("=")
                    .then(Property)

                .match()
                    .token("property", named: "keyword")
                    .identifier(named: "id")
                    .then(lexical.transform()
                        .remove("keyword")
                        .then(Property, referenceToken: "id"));

            sintaxis
                //constructor
                .match<MethodDeclarationSyntax>(method => method.ReturnType.IsMissing && method.Identifier.ToString() == "constructor")
                    .then(Constructor);
        }

        private static PropertyDeclarationSyntax _property = SyntaxFactory.ParseCompilationUnit("__1 __2 {get; set;}")
            .DescendantNodes().OfType<PropertyDeclarationSyntax>().First();

        private static ExpressionStatementSyntax _assignment = (ExpressionStatementSyntax)SyntaxFactory.ParseStatement("__1 = __2;");

        private static SyntaxNode Property(SyntaxNode node, Scope scope)
        {
            var field = node.AncestorsAndSelf()
                .OfType<MemberDeclarationSyntax>()
                .FirstOrDefault()
                as FieldDeclarationSyntax;

            if (field == null)
            {
                //td: error, malformed property
                return node;
            }

            if (field.Declaration.Variables.Count != 1)
            {
                //td: error, malformed property
                return node;
            }

            var variable = field
                .Declaration
                .Variables[0];

            var initializer = variable.Initializer;
            var type = field.Declaration.Type;
            if (type == null || type.IsMissing || type.ToString() == "property") //untyped
            {
                if (initializer != null)
                    type = RoslynCompiler.ConstantType(initializer.Value);
            }

            if (type == null)
                type = RoslynCompiler.@dynamic;

            var property = _property
                .WithIdentifier(variable.Identifier)
                .WithType(type);

            if (!RoslynCompiler.HasVisibilityModifier(field.Modifiers))
                property = property.AddModifiers(CSharp.Token(SyntaxKind.PublicKeyword));

            var document = scope.GetDocument<SyntaxToken, SyntaxNode, SemanticModel>();

            //schedule the field replacement
            //td: coud be done in this pass with the right info from lexical
            document.change(field, RoslynCompiler.ReplaceNode(property));

            //must be initialized
            if (initializer != null)
            {
                var expr = (AssignmentExpressionSyntax)_assignment.Expression;
                document.change(field.Parent, RoslynCompiler
                    .AddInitializers(_assignment.WithExpression(expr
                        .WithLeft(CSharp.IdentifierName(variable.Identifier))
                        .WithRight(initializer.Value))));
            }

            return node;
        }

        private static SyntaxNode Constructor(SyntaxNode node, Scope scope)
        {
            var method = (MethodDeclarationSyntax)node;

            string name = "__xs_constructor";

            ClassDeclarationSyntax parent = method.Parent as ClassDeclarationSyntax;
            if (parent != null)
            {
                name = parent.Identifier.ToString();
            }

            var modifiers = method.Modifiers.Any() ? method.Modifiers : RoslynCompiler.@public;
            return SyntaxFactory.ConstructorDeclaration(name).
                                    WithModifiers(modifiers).
                                    WithParameterList(method.ParameterList).
                                    WithBody(method.Body);
        }

        private static SyntaxNode Method(SyntaxNode node, Scope scope)
        {
            if (!(node is MethodDeclarationSyntax))
            {
                //td: error
                return node;
            }

            var method = node as MethodDeclarationSyntax;

            if (!RoslynCompiler.HasVisibilityModifier(method.Modifiers))
                method = method.AddModifiers(CSharp.Token(SyntaxKind.PublicKeyword));

            if (method.ReturnType.IsMissing)
            {
                var document = scope.GetDocument<SyntaxToken, SyntaxNode, SemanticModel>();
                document.change(method, FixReturnType);

                return method.WithReturnType(RoslynCompiler.@dynamic);
            }

            return method;
        }

        private static SyntaxNode FixReturnType(SyntaxNode node, SyntaxNode newNode, SemanticModel model, Scope scope)
        {
            var method = (MethodDeclarationSyntax)node;
            var type = RoslynCompiler.GetReturnType(method.Body, model);

            return (newNode as MethodDeclarationSyntax)
                .WithReturnType(type);
        }
    }
}