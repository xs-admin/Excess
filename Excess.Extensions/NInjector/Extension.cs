using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Excess.Compiler;
using Excess.Compiler.Attributes;
using Excess.Compiler.Roslyn;

namespace NInjector
{
    using System;
    using ExcessCompiler = ICompiler<SyntaxToken, SyntaxNode, SemanticModel>;

    [Extension("ninject")]
    public class NinjectExtension
    {
        public static void Apply(ExcessCompiler compiler, Scope scope = null)
        {
            scope?.AddKeywords("injector");

            compiler.extension("injector", ParseDependencyInjector);
            compiler.Environment()
                .dependency("Excess.Runtime")
                .dependency("Ninject");
        }

        private static TypeDeclarationSyntax ParseDependencyInjector(
            ClassDeclarationSyntax @class,
            BlockSyntax code,
            Scope scope)
        {
            //td: we're getting a modified class (no parent)
            //var parentNS = @class.Parent as NamespaceDeclarationSyntax;
            //if (parentNS == null)
            //{
            //    //td: error
            //    return @class;
            //}

            var statements = new List<StatementSyntax>();
            if (!ParseInjector(code, scope, statements))
                return @class;

            var module = Templates.Module;
            var loadMethod = module
                .DescendantNodes()
                .OfType<MethodDeclarationSyntax>()
                .Where(method => method.Identifier.ToString() == "Load")
                .Single();

            return module.ReplaceNode(
                loadMethod,
                loadMethod.AddBodyStatements(statements.ToArray()));
        }

        private static bool ParseInjector(BlockSyntax block, Scope scope, List<StatementSyntax> statements)
        {
            foreach (var member in block.Statements)
            {
                var field = member as ExpressionStatementSyntax;
                if (field == null)
                {
                    //td: error
                    return false;
                }

                var assignment = field.Expression as AssignmentExpressionSyntax;
                if (assignment == null)
                {
                    //td: error
                    return false;
                }

                statements.Add(Templates
                    .Bind
                    .Get<StatementSyntax>(
                        assignment.Left
                            .WithLeadingTrivia()
                            .WithTrailingTrivia(), 
                        assignment.Right
                            .WithLeadingTrivia()
                            .WithTrailingTrivia()));
            }

            return true;
        }
    }
}