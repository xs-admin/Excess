using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Excess.Core
{
    public class ManagedDSLHandler : IDSLHandler
    {
        public ManagedDSLHandler(object parser, object linker)
        {
            _parser = parser;
            _linker = linker;

            _parseNamespace = parser.GetType().GetMethod("ParseNamespace");
            _parseClass = parser.GetType().GetMethod("ParseClass");
            _parseMethod = parser.GetType().GetMethod("ParseMethod");
            _parseCodeHeader = parser.GetType().GetMethod("ParseCodeHeader");
            _parseCode = parser.GetType().GetMethod("ParseCode");

            _link = linker.GetType().GetMethod("Link");
        }

        public SyntaxNode compile(ExcessContext ctx, DSLContext dctx)
        {
            setContext(_parser, ctx);
            setContext(_linker, ctx);

            var node = dctx.MainNode;
            switch (dctx.Surroundings)
            {
                case DSLSurroundings.Global:
                {
                    if (node is ClassDeclarationSyntax)
                    {
                        if (_parseClass == null)
                            throw new InvalidOperationException("This dsl does not support types");

                        var classDeclaration = (ClassDeclarationSyntax)ctx.Rewriter.Visit(node);

                        var id = classDeclaration.Identifier.ToString();
                        var result = ctx.Rewriter.Visit(node);
                        return (SyntaxNode)_parseClass.Invoke(_parser, new object[] { classDeclaration, id, SyntaxFactory.ParameterList() });
                    }
                    else if (node is MethodDeclarationSyntax)
                    {
                        if (_parseNamespace == null)
                            throw new InvalidOperationException("This dsl does not support namespaces");

                        var method = (MethodDeclarationSyntax)node;
                        var id     = method.ReturnType.IsMissing? "" :  method.Identifier.ToString();
                        var code   = (BlockSyntax)ctx.Rewriter.Visit(method.Body);

                        return (SyntaxNode)_parseNamespace.Invoke(_parser, new object[] { method, id, method.ParameterList, code });
                    }

                    //td: error
                    break;
                }

                case DSLSurroundings.TypeBody:
                {
                    if (_parseMethod == null)
                        throw new InvalidOperationException("This dsl does not support methods");

                    var method = (MethodDeclarationSyntax)node;
                    var code = (BlockSyntax)ctx.Rewriter.Visit(method.Body);

                    return (SyntaxNode)_parseMethod.Invoke(_parser, new object[] { method, "", method.ParameterList, code });
                }

                case DSLSurroundings.Code:
                {
                    if (_parseCodeHeader == null)
                        return null;

                    return (SyntaxNode)_parseCodeHeader.Invoke(_parser, new object[] { node });
                }
            }

            return node;
        }

        private void setContext(object obj, ExcessContext ctx)
        {
            var method = obj.GetType().GetMethod("SetContext");
            method.Invoke(obj, new object[] { ctx });
        }

        public SyntaxNode setCode(ExcessContext ctx, DSLContext dctx, BlockSyntax code)
        {
            if (_parseCode == null)
                throw new InvalidOperationException("This dsl does not support code");

            SyntaxNode          node       =  dctx.MainNode;
            string              identifier = "";
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

                identifier = varDeclarator.Identifier.ToString();
                args = toParameterList(varDeclarator.ArgumentList.Arguments);
            }

            code = (BlockSyntax)ctx.Rewriter.Visit(code);
            return (SyntaxNode)_parseCode.Invoke(_parser, new object[] { code, identifier, args, code, dctx.Assign });
        }

        public SyntaxNode link(ExcessContext ctx, SyntaxNode node, SemanticModel model)
        {
            if (_link == null)
                throw new InvalidOperationException("This dsl does not support linking");

            return (SyntaxNode)_link.Invoke(_linker, new object[] { node, model });
        }


        private object _parser;
        private object _linker;

        private MethodInfo _parseNamespace;
        private MethodInfo _parseClass;
        private MethodInfo _parseMethod;
        private MethodInfo _parseCodeHeader;
        private MethodInfo _parseCode;
        private MethodInfo _link;

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

    public class ManagedDSLFactory : IDSLFactory
    {
        public ManagedDSLFactory(string name, object parser, object linker)
        {
            _name   = name;
            _parser = parser;
            _linker = linker;
        }

        protected string _name;
        protected object _parser;
        protected object _linker;

        public IDSLHandler create(string name)
        {
            if (name == _name)
                return new ManagedDSLHandler(_parser, _linker);

            return null;
        }

        public IEnumerable<string> supported()
        {
            yield return _name;
        }
    }
}
