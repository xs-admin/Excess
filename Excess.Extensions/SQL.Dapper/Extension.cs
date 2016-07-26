using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Text;
using System.Data;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Excess.Compiler;
using Excess.Compiler.Roslyn;
using Excess.Compiler.Attributes;

namespace SQL.Dapper
{
    using ExcessCompiler = Excess.Compiler.ICompiler<SyntaxToken, SyntaxNode, SemanticModel>;
    using CSharp = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

    [Extension("dapper")]
    public class DapperExtension
    {
        public static void Apply(ExcessCompiler compiler, Scope scope = null)
        {
            scope?.AddKeywords("sql");
            compiler.Lexical()
                .extension("sql", ExtensionKind.Code, ParseDapper);

            compiler.Environment()
                .dependency("Excess.Runtime")
                .dependency("Dapper")
                .dependency<IDbConnection>("System.Data")
                .dependency("System.Collections.Generic")
                .dependency("System.Linq");
        }

        private static SyntaxNode ParseDapper(SyntaxNode node, Scope scope, LexicalExtension<SyntaxToken> extension)
        {
            var tokenCount = extension.Body.Count();
            var withoutBraces = extension
                .Body
                .Skip(1)
                .Take(tokenCount - 2);

            var result = default(SyntaxNode);
            if (node is LocalDeclarationStatementSyntax)
                result = ParseQuery(node as LocalDeclarationStatementSyntax, scope, withoutBraces);
            else if (node is ExpressionStatementSyntax)
                result = ParseCommand(node as ExpressionStatementSyntax, scope, withoutBraces);

            Debug.Assert(result != null); //td: error

            //schedule a search for __connection, after the first pass
            var document = scope.GetDocument<SyntaxToken, SyntaxNode, SemanticModel>();
            return document.change(result, SkipPass); 
        }

        private static SyntaxNode SkipPass(SyntaxNode node, Scope scope)
        {
            var document = scope.GetDocument<SyntaxToken, SyntaxNode, SemanticModel>();
            return document.change(node, LinkDapper);
        }

        private static StatementSyntax ConnectionFromContext = CSharp.ParseStatement("var __connection = __scope.get<IDbConnection>();");
        private static SyntaxNode LinkDapper(SyntaxNode node, Scope scope)
        {
            var method = node
                .Ancestors()
                .OfType<MethodDeclarationSyntax>()
                .FirstOrDefault();

            if (method == null)
            {
                Debug.Assert(false); //td: error
                return node;
            }

            if (method
                .ParameterList
                .Parameters
                .Any(parameter => parameter
                    .Identifier
                    .ToString() == "__scope"))
            {
                var document = scope.GetDocument<SyntaxToken, SyntaxNode, SemanticModel>();
                document.change(method.Body, AddConnectionVariable);
            }

            //assume __connection is available
            return node;
        }

        private static SyntaxNode AddConnectionVariable(SyntaxNode node, Scope scope)
        {
            var block = (BlockSyntax)node;
            if (!block
                .Statements
                .OfType<LocalDeclarationStatementSyntax>()
                .Any(local => local
                    .Declaration
                    .Variables
                    .First()
                    .Identifier.ToString() == "__connection"))
            {
                return block
                    .WithStatements(CSharp.List(
                        new StatementSyntax[] { ConnectionFromContext }
                        .Union(block.Statements)));
            }

            return node;
        }

        class QueryTypeInfo
        {
            public bool isEnumerable { get; set; }
            public TypeSyntax DapperType { get; set; }
        }

        private static Template DapperQuery = Template.ParseExpression("__connection.Query<__0>(__1, __2)");
        private static SyntaxNode ParseQuery(LocalDeclarationStatementSyntax node, Scope scope, IEnumerable<SyntaxToken> body)
        {
            var declaration = node.Declaration;
            var variable = declaration.Variables.Single(); //td: error

            var typeInfo = ParseQueryType(declaration.Type);
            if (typeInfo == null)
            {
                Debug.Assert(false); //td: error
                return node;
            }

            var type = typeInfo.DapperType;

            var parameters = default(AnonymousObjectCreationExpressionSyntax);
            var sqlQueryString = ParseBody(body, scope, out parameters);
            if (sqlQueryString == null)
            {
                Debug.Assert(false); //td: error
                return node;
            }

            var sqlExpression = DapperQuery.Get<ExpressionSyntax>(
                type,
                sqlQueryString,
                parameters);

            if (!typeInfo.isEnumerable)
            {
                //we expect one result
                var expr = CSharp.MemberAccessExpression(
                    kind: SyntaxKind.SimpleMemberAccessExpression,
                    expression: sqlExpression,
                    name: (SimpleNameSyntax)CSharp.ParseName("Single"));

                sqlExpression = CSharp.InvocationExpression(expr);
            }

            return node
                .WithDeclaration(node.Declaration
                    .WithVariables(CSharp.SeparatedList(new[] {
                        variable.WithInitializer(variable.Initializer
                            .WithValue(sqlExpression))})));
        }

        private static Template DapperCommand = Template.ParseExpression("__connection.Execute(__0, __1)");
        private static SyntaxNode ParseCommand(ExpressionStatementSyntax node, Scope scope, IEnumerable<SyntaxToken> body)
        {
            var parameters = default(AnonymousObjectCreationExpressionSyntax);
            var sqlQueryString = ParseBody(body, scope, out parameters);
            if (sqlQueryString == null)
            {
                Debug.Assert(false); //td: error
                return node;
            }

            return node.WithExpression( 
                DapperCommand.Get<ExpressionSyntax>(
                    sqlQueryString, parameters));
        }

        private static QueryTypeInfo ParseQueryType(TypeSyntax type)
        {
            var result = new QueryTypeInfo();
            if (type is GenericNameSyntax)
            {
                var generic = type as GenericNameSyntax;
                if (generic.Identifier.ToString() != "IEnumerable")
                {
                    Debug.Assert(false); //td: error
                    return null;
                }

                if (generic.Arity != 1)
                {
                    Debug.Assert(false); //td: error
                    return null;
                }

                result.isEnumerable = true;
                result.DapperType = generic
                    .TypeArgumentList
                    .Arguments
                    .Single();
            }
            else
            {
                Debug.Assert(result.isEnumerable == false);
                result.DapperType = type;
            }

            return result;
        }

        private static ExpressionSyntax ParseBody(IEnumerable<SyntaxToken> body, Scope scope, out AnonymousObjectCreationExpressionSyntax parameters)
        {
            parameters = null;

            var builder = new StringBuilder();
            var @params = new List<AnonymousObjectMemberDeclaratorSyntax>();
            builder.Append("@\"");
            foreach (var token in body)
            {
                var str = token.ToFullString();
                if (token.IsKind(SyntaxKind.IdentifierToken) && str.StartsWith("@"))
                {
                    var newToken = CSharp.ParseToken(str.Substring(1))
                        .WithAdditionalAnnotations(token.GetAnnotations());

                    @params.Add(CSharp.AnonymousObjectMemberDeclarator(
                        CSharp.NameEquals(CSharp.IdentifierName(newToken)),
                        CSharp.IdentifierName(newToken)));
                }

                builder.Append(str);
            }
            builder.Append("\"");

            var result = CSharp.ParseExpression(builder.ToString());
            if (result == null)
            {
                Debug.Assert(false); //td: error
                return null;
            }

            parameters = CSharp.AnonymousObjectCreationExpression(CSharp.SeparatedList(
                @params));

            return result;
        }
    }
}
