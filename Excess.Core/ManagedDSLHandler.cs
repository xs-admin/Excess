using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess.Core
{
    public interface IParser
    {
        SyntaxNode ParseNamespace(SyntaxNode node, SyntaxToken id, ParameterListSyntax args, BlockSyntax code);
        SyntaxNode ParseClass(SyntaxNode node, SyntaxToken id, ParameterListSyntax args);
        SyntaxNode ParseMethod(SyntaxNode node, SyntaxToken id, ParameterListSyntax args, BlockSyntax code);
        SyntaxNode ParseCodeHeader(SyntaxNode node);
        SyntaxNode ParseCode(SyntaxNode node, SyntaxToken id, ParameterListSyntax args, BlockSyntax code, bool expectsResult);
    }

    public interface ILinker
    {
        SyntaxNode Link(SyntaxNode node, SemanticModel model);
    }

    public class ManagedDSLHandler : IDSLHandler
    {
        public ManagedDSLHandler(IParser parser, ILinker linker)
        {
            _parser = parser;
            _linker = linker;
        }

        public SyntaxNode compile(ExcessContext ctx, DSLContext dctx)
        {
            var node = dctx.MainNode;
            switch (dctx.Surroundings)
            {
                case DSLSurroundings.Global:
                {
                    if (node is ClassDeclarationSyntax)
                    {
                        var classDeclaration = (ClassDeclarationSyntax)ctx.Rewriter.Visit(node);

                        var id = classDeclaration.Identifier;
                        var result = ctx.Rewriter.Visit(node);
                        return _parser.ParseClass(classDeclaration, id, SyntaxFactory.ParameterList());
                    }
                    else if (node is MethodDeclarationSyntax)
                    {
                        var method = (MethodDeclarationSyntax)node;
                        var id     = method.ReturnType.IsMissing? new SyntaxToken() :  method.Identifier;
                        var code   = (BlockSyntax)ctx.Rewriter.Visit(method.Body);
                        var result = _parser.ParseNamespace(method, id, method.ParameterList, code);
                        return result;
                    }

                    //td: error
                    break;
                }

                case DSLSurroundings.TypeBody:
                {
                    var method = (MethodDeclarationSyntax)node;
                    var code = (BlockSyntax)ctx.Rewriter.Visit(method.Body);
                    var result = _parser.ParseMethod(method, new SyntaxToken(), method.ParameterList, code);
                    return result;
                }

                case DSLSurroundings.Code:
                {
                    return _parser.ParseCodeHeader(node); 
                }
            }

            return node;
        }

        public SyntaxNode setCode(ExcessContext ctx, DSLContext dctx, BlockSyntax code)
        {
            SyntaxNode          node       =  dctx.MainNode;
            SyntaxToken         identifier = new SyntaxToken();
            ParameterListSyntax args       = null;

            var invocation = node as InvocationExpressionSyntax;
            if (invocation != null)
            {
                args = toParameterList(invocation.ArgumentList.Arguments);
            }
            else
            {
                var varDeclarator = node as VariableDeclaratorSyntax;
                if (varDeclarator == null)
                {
                    //td: error
                    return node;
                }

                identifier = varDeclarator.Identifier;
                args = toParameterList(varDeclarator.ArgumentList.Arguments);
            }

            code = (BlockSyntax)ctx.Rewriter.Visit(code);
            return _parser.ParseCode(code, identifier, args, code, dctx.Assign); 
        }

        public SyntaxNode link(ExcessContext ctx, SyntaxNode node, SemanticModel model)
        {
            return _linker.Link(node, model);
        }


        private IParser _parser;
        private ILinker _linker;

        private ParameterListSyntax toParameterList(IEnumerable<ArgumentSyntax> arguments)
        {
            return SyntaxFactory.ParameterList(SyntaxFactory.SeparatedList(
                                    arguments.Select<ArgumentSyntax, ParameterSyntax>(arg =>
                                    {
                                        SyntaxToken      identifier = SyntaxFactory.ParseToken("_");
                                        ExpressionSyntax value = arg.Expression;
                                        TypeSyntax       type  = Compiler.Void;

                                        var expr = arg.Expression;
                                        if (expr is ParenthesizedExpressionSyntax)
                                        {
                                            var parenthesized = (ParenthesizedExpressionSyntax)expr;
                                            expr = parenthesized.Expression;
                                        }
                                        
                                        if (expr is BinaryExpressionSyntax)
                                        {
                                            var binary = (BinaryExpressionSyntax)expr;

                                            identifier = SyntaxFactory.ParseToken(binary.Left.ToString());
                                            value      = binary.Right;
                                            type       = Compiler.ConstantType(binary.Right);

                                            if (type == null)
                                                type = Compiler.Void;
                                        }
                                        else if (expr is IdentifierNameSyntax)
                                        {
                                            var idSyntax = (IdentifierNameSyntax)expr;
                                            identifier = idSyntax.Identifier;
                                        }

                                        return SyntaxFactory.Parameter(identifier).
                                                             WithType(type).
                                                             WithDefault(SyntaxFactory.EqualsValueClause(value));
                                    })));
        }
    }
}
