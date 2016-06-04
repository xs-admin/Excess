using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Excess.Compiler;
using Excess.Compiler.Attributes;

namespace NInjector
{
    using ExcessCompiler = ICompiler<SyntaxToken, SyntaxNode, SemanticModel>;

    [Extension("ninject")]
    public class NinjectExtension
    {
        public static IEnumerable<string> GetKeywords()
        {
            return new[] { "injector" };
        }

        public static void Apply(ExcessCompiler compiler, Scope scope = null)
        {
            if (scope != null)
            {
                var keywords = scope.get("keywords") as List<string>;
                if (keywords != null)
                    keywords.AddRange(GetKeywords());
            }

            var lexical = compiler.Lexical();
            var syntax = compiler.Syntax();

            lexical
                .match()
                    .token("injector", named: "keyword")
                    .enclosed('{', '}', end: "lastBrace")
                    .then(lexical.transform()
                        .replace("keyword", "class __ninject { void __load()")
                        .insert("}", after: "lastBrace")
                        .then(InjectorClass));

            compiler.Environment()
                .dependency("Excess.Runtime")
                .dependency("Ninject");
        }

        private static SyntaxNode InjectorClass(SyntaxNode node, Scope scope)
        {
            var @class = node as ClassDeclarationSyntax;
            Debug.Assert(@class != null);

            var parentNS = @class.Parent as NamespaceDeclarationSyntax;
            if (parentNS == null)
            {
                //td: error
                return node;
            }

            List<StatementSyntax> statements = new List<StatementSyntax>();
            if (!ParseInjector(@class, scope, statements))
                return node;

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

        private static bool ParseInjector(ClassDeclarationSyntax @class, Scope scope, List<StatementSyntax> statements)
        {
            if (@class.Members.Count != 1)
            {
                //td: error
                return false;
            }

            var mainMethod = @class
                .Members
                .Single() as MethodDeclarationSyntax;

            if (mainMethod == null)
            {
                //td: error
                return false;
            }

            foreach (var member in mainMethod.Body.Statements)
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