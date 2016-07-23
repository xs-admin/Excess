using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
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

    [Extension("sql")]
    public class DapperExtension
    {
        public static void Apply(ExcessCompiler compiler, Scope scope = null)
        {
            scope?.AddKeywords("sql");
            compiler.Lexical().extension("sql", ExtensionKind.Code, ParseDapper);
        }

        private static SyntaxNode ParseDapper(SyntaxNode node, Scope scope, LexicalExtension<SyntaxToken> extension)
        {
            var tokenCount = extension.Body.Count();
            var withoutBraces = extension
                .Body
                .Skip(1)
                .Take(tokenCount - 2);

            if (node is LocalDeclarationStatementSyntax)
                return ParseQuery(node as LocalDeclarationStatementSyntax, scope, withoutBraces);
            else if (node is ExpressionStatementSyntax)
                return ParseCommand(node as ExpressionStatementSyntax, scope, withoutBraces);

            Debug.Assert(false); //td: error
            return node;
        }

        private static Template DapperQuery = Template.ParseExpression("__connection.Query<__0>(__1, __2)");
        private static SyntaxNode ParseQuery(LocalDeclarationStatementSyntax node, Scope scope, IEnumerable<SyntaxToken> body)
        {
            var declaration = node.Declaration;
            var variable = declaration.Variables.Single(); //td: error
            var type = declaration.Type;
            if (!ValidateQueryType(type, out type))
            {
                Debug.Assert(false); //td: error
                return node;
            }

            var parameters = default(AnonymousObjectCreationExpressionSyntax);
            var sqlQueryString = ParseBody(body, scope, out parameters);
            if (sqlQueryString == null)
            {
                Debug.Assert(false); //td: error
                return node;
            }

            return node
                .WithDeclaration(node.Declaration
                    .WithVariables(CSharp.SeparatedList(new[] {
                        variable.WithInitializer(variable.Initializer
                            .WithValue(DapperQuery.Get<ExpressionSyntax>(
                                type,
                                sqlQueryString,
                                parameters)))})));
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

        private static bool ValidateQueryType(TypeSyntax type, out TypeSyntax innerType)
        {
            innerType = null;
            if (type is GenericNameSyntax)
            {
                var generic = type as GenericNameSyntax;
                if (generic.Arity != 1)
                {
                    Debug.Assert(false); //td: error
                    return false;
                }

                innerType = generic
                    .TypeArgumentList
                    .Arguments
                    .Single();

                return true;
            }

            Debug.Assert(false); //td: error
            return false;
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
