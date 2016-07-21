using System;
using System.Linq;
using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;

namespace Excess.Compiler.Roslyn
{
    using CSharp = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

    public static class CompilerExtensions
    {
        //free form extensions
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
                    var expr = node as ExpressionSyntax;
                    if (expr != null)
                        statement = (ExpressionStatementSyntax)(expr.Parent);
                    else
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

        //member extensions
        private static Func<SyntaxNode, Scope, LexicalExtension<SyntaxToken>, SyntaxNode> TransformMember(
            Func<MethodDeclarationSyntax, Scope, MemberDeclarationSyntax> transform)
        {
            return (node, scope, extension) =>
            {
                var method = node as MethodDeclarationSyntax;
                if (method == null)
                {
                    Debug.Assert(false); //td: error
                    return node;
                }

                if (node.Parent is TypeDeclarationSyntax)
                {
                    var service = scope.GetService<SyntaxToken, SyntaxNode, SemanticModel>();
                    var body = (BlockSyntax)service.ParseCodeFromTokens(extension.Body);

                    var identifier = extension.Identifier.IsKind(SyntaxKind.None)
                        ? CSharp.Identifier("__")
                        : extension.Identifier;

                    var parameters = default(ParameterListSyntax);
                    if (extension.Arguments != null && extension.Arguments.Any())
                    {
                        parameters = (ParameterListSyntax)service.ParseParamListFromTokens(extension.Arguments);
                        method = method.WithParameterList(parameters);
                    }

                    return transform(method
                        .WithIdentifier(identifier)
                        .WithReturnType(CSharp.ParseTypeName(extension.Keyword.ToString()))
                        .WithBody(body), scope);
                }

                Debug.Assert(false); //td: error
                return node;
            };
        }

        public static ICompiler<SyntaxToken, SyntaxNode, SemanticModel> extension(this ICompiler<SyntaxToken, SyntaxNode, SemanticModel> @this,
            string keyword,
            Func<MethodDeclarationSyntax, Scope, MemberDeclarationSyntax> transform)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                throw new ArgumentException(nameof(keyword));

            @this.Lexical()
                .extension(keyword, ExtensionKind.Member, TransformMember(transform));

            return @this;
        }

        //member type extensions
        private static Func<SyntaxNode, Scope, LexicalExtension<SyntaxToken>, SyntaxNode> TransformMemberType(
            Func<ClassDeclarationSyntax, ParameterListSyntax, Scope, MemberDeclarationSyntax> transform)
        {
            return (node, scope, extension) =>
            {
                var @class = node as ClassDeclarationSyntax;
                if (@class == null)
                {
                    Debug.Assert(false); //td: error
                    return node;
                }

                if (node.Parent is TypeDeclarationSyntax)
                {
                    var service = scope.GetService<SyntaxToken, SyntaxNode, SemanticModel>();
                    var members = (service
                        .ParseMembersFromTokens(extension.Body) as ClassDeclarationSyntax)
                            .Members;

                    var identifier = extension.Identifier.IsKind(SyntaxKind.None)
                        ? CSharp.Identifier("__")
                        : extension.Identifier;

                    var parameters = default(ParameterListSyntax);
                    if (extension.Arguments != null && extension.Arguments.Any())
                        parameters = (ParameterListSyntax)service.ParseParamListFromTokens(extension.Arguments);

                    return transform(@class
                        .WithIdentifier(identifier)
                        .WithMembers(members), parameters, scope);
                }

                Debug.Assert(false); //td: error
                return node;
            };
        }

        public static ICompiler<SyntaxToken, SyntaxNode, SemanticModel> extension(this ICompiler<SyntaxToken, SyntaxNode, SemanticModel> @this,
            string keyword,
            Func<ClassDeclarationSyntax, ParameterListSyntax, Scope, MemberDeclarationSyntax> transform)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                throw new ArgumentException(nameof(keyword));

            @this.Lexical()
                .extension(keyword, ExtensionKind.MemberType, TransformMemberType(transform));

            return @this;
        }

        public static ICompiler<SyntaxToken, SyntaxNode, SemanticModel> extension(this ICompiler<SyntaxToken, SyntaxNode, SemanticModel> @this,
            string keyword,
            Func<ClassDeclarationSyntax, Scope, MemberDeclarationSyntax> transform) => extension(@this, keyword,
                (@class, parameters, scope) => transform(@class, scope));

        public static ICompiler<SyntaxToken, SyntaxNode, SemanticModel> extension(this ICompiler<SyntaxToken, SyntaxNode, SemanticModel> @this,
            string keyword,
            Func<ClassDeclarationSyntax, MemberDeclarationSyntax> transform) => extension(@this, keyword,
                (@class, parameters, scope) => transform(@class));

        //type extensions
        private static Func<SyntaxNode, Scope, LexicalExtension<SyntaxToken>, SyntaxNode> TransformType(
            Func<ClassDeclarationSyntax, ParameterListSyntax, Scope, TypeDeclarationSyntax> transform)
        {
            return (node, scope, extension) =>
            {
                var @class = node as ClassDeclarationSyntax;
                if (@class == null)
                {
                    Debug.Assert(false); //td: error
                    return node;
                }

                if (node.Parent is NamespaceDeclarationSyntax || node.Parent is CompilationUnitSyntax)
                {
                    var service = scope.GetService<SyntaxToken, SyntaxNode, SemanticModel>();
                    var members = (service
                        .ParseMembersFromTokens(extension.Body) as ClassDeclarationSyntax)
                            .Members;

                    var identifier = extension.Identifier.IsKind(SyntaxKind.None)
                        ? CSharp.Identifier("__")
                        : extension.Identifier;

                    var parameters = default(ParameterListSyntax);
                    if (extension.Arguments != null && extension.Arguments.Any())
                        parameters = (ParameterListSyntax)service.ParseParamListFromTokens(extension.Arguments);

                    return transform(@class
                        .WithIdentifier(identifier)
                        .WithMembers(members), parameters, scope);
                }

                Debug.Assert(false); //td: error
                return node;
            };
        }

        public static ICompiler<SyntaxToken, SyntaxNode, SemanticModel> extension(this ICompiler<SyntaxToken, SyntaxNode, SemanticModel> @this,
            string keyword,
            Func<ClassDeclarationSyntax, ParameterListSyntax, Scope, TypeDeclarationSyntax> transform)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                throw new ArgumentException(nameof(keyword));

            @this.Lexical()
                .extension(keyword, ExtensionKind.Type, TransformType(transform));

            return @this;
        }

        public static ICompiler<SyntaxToken, SyntaxNode, SemanticModel> extension(this ICompiler<SyntaxToken, SyntaxNode, SemanticModel> @this,
            string keyword,
            Func<ClassDeclarationSyntax, Scope, TypeDeclarationSyntax> transform) => extension(@this, keyword,
                (Func<ClassDeclarationSyntax, ParameterListSyntax, Scope, TypeDeclarationSyntax>)
                ((@class, parameters, scope) => transform(@class, scope)));

        //type code extensions
        private static Func<SyntaxNode, Scope, LexicalExtension<SyntaxToken>, SyntaxNode> TransformTypeCode(
            Func<ClassDeclarationSyntax, ParameterListSyntax, BlockSyntax, Scope, TypeDeclarationSyntax> transform)
        {
            return (node, scope, extension) =>
            {
                var @class = node as ClassDeclarationSyntax;
                if (@class == null)
                {
                    Debug.Assert(false); //td: error
                    return node;
                }

                if (node.Parent is NamespaceDeclarationSyntax || node.Parent is CompilationUnitSyntax)
                {
                    var service = scope.GetService<SyntaxToken, SyntaxNode, SemanticModel>();
                    var body = (BlockSyntax)service
                        .ParseCodeFromTokens(extension.Body);

                    var identifier = extension.Identifier.IsKind(SyntaxKind.None)
                        ? CSharp.Identifier("__")
                        : extension.Identifier;

                    var parameters = default(ParameterListSyntax);
                    if (extension.Arguments != null && extension.Arguments.Any())
                        parameters = (ParameterListSyntax)service.ParseParamListFromTokens(extension.Arguments);

                    return transform(@class
                        .WithIdentifier(identifier), parameters, body, scope);
                }

                Debug.Assert(false); //td: error
                return node;
            };
        }

        public static ICompiler<SyntaxToken, SyntaxNode, SemanticModel> extension(this ICompiler<SyntaxToken, SyntaxNode, SemanticModel> @this,
            string keyword,
            Func<ClassDeclarationSyntax, ParameterListSyntax, BlockSyntax, Scope, TypeDeclarationSyntax> transform)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                throw new ArgumentException(nameof(keyword));

            @this.Lexical()
                .extension(keyword, ExtensionKind.TypeCode, TransformTypeCode(transform));

            return @this;
        }

        public static ICompiler<SyntaxToken, SyntaxNode, SemanticModel> extension(this ICompiler<SyntaxToken, SyntaxNode, SemanticModel> @this,
            string keyword,
            Func<ClassDeclarationSyntax, BlockSyntax, Scope, TypeDeclarationSyntax> transform) => extension(@this, keyword,
                (@class, parameters, code, scope) => transform(@class, code, scope));
    }
}
