using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess.Compiler.Roslyn
{
    public static class CompilerExtensions
    {
        //free form
        public static ICompiler<SyntaxToken, SyntaxNode, SemanticModel> extension(this ICompiler<SyntaxToken, SyntaxNode, SemanticModel> @this,
            string keyword,
            Func<SyntaxNode, Scope, LexicalExtension<SyntaxToken>, SyntaxNode> transform) 
        {
            @this.Lexical().extension(keyword, ExtensionKind.None, transform);
            return @this;
        }

        //code extensions
        private static Func<SyntaxNode, Scope, LexicalExtension<SyntaxToken>, SyntaxNode> TransformCode(Func<BlockSyntax, SyntaxToken, ParameterListSyntax, Scope, SyntaxNode> transform)
        {
            return (node, scope, extension) =>
            {
                var statement = node as ExpressionStatementSyntax;
                if (statement == null)
                {
                    Debug.Assert(false); //td: error
                    return node;
                }

                if (node.Parent is StatementSyntax)
                {
                    var service = scope.GetService<SyntaxToken, SyntaxNode, SemanticModel>();
                    var body = (BlockSyntax)service.ParseCodeFromTokens(extension.Body);
                    var parameters = default(ParameterListSyntax);

                    if (extension.Arguments != null && extension.Arguments.Any())
                        parameters = (ParameterListSyntax)service.ParseParamListFromTokens(extension.Arguments);

                    return transform(body, extension.Identifier, parameters, scope);
                }

                Debug.Assert(false); //td: error
                return node;
            };
        }

        public static ICompiler<SyntaxToken, SyntaxNode, SemanticModel> extension(this ICompiler<SyntaxToken, SyntaxNode, SemanticModel> @this,
            string keyword,
            Func<BlockSyntax, SyntaxToken, ParameterListSyntax, Scope, SyntaxNode> transform)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                throw new ArgumentException(nameof(keyword));

            @this.Lexical()
                .extension(keyword, ExtensionKind.Code, TransformCode(transform));

            return @this;
        }

        public static ICompiler<SyntaxToken, SyntaxNode, SemanticModel> extension(this ICompiler<SyntaxToken, SyntaxNode, SemanticModel> @this,
            string keyword,
            Func<BlockSyntax, Scope, SyntaxNode> transform) => extension(@this, keyword, 
                (block, indentifier, parameters, scope) => transform(block, scope));

        public static ICompiler<SyntaxToken, SyntaxNode, SemanticModel> extension(this ICompiler<SyntaxToken, SyntaxNode, SemanticModel> @this,
            string keyword,
            Func<BlockSyntax, SyntaxToken, Scope, SyntaxNode> transform) => extension(@this, keyword,
                (block, indentifier, parameters, scope) => transform(block, indentifier, scope));

        public static ICompiler<SyntaxToken, SyntaxNode, SemanticModel> extension(this ICompiler<SyntaxToken, SyntaxNode, SemanticModel> @this,
            string keyword,
            Func<BlockSyntax, ParameterListSyntax, Scope, SyntaxNode> transform) => extension(@this, keyword,
                (block, indentifier, parameters, scope) => transform(block, parameters, scope));
    }
}
