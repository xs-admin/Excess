using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess.Compiler.Roslyn
{
    using CSharp = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

    public class TransformExtensions : CSharpSyntaxRewriter
    {
        IEventBus _events;

        Dictionary<string, Func<ISyntacticalMatchResult<SyntaxNode>, SyntacticalExtension<SyntaxNode>, SyntaxNode>> _codeExtensions = new Dictionary<string, Func<ISyntacticalMatchResult<SyntaxNode>, SyntacticalExtension<SyntaxNode>, SyntaxNode>>();
        Dictionary<string, Func<ISyntacticalMatchResult<SyntaxNode>, SyntacticalExtension<SyntaxNode>, SyntaxNode>> _memberExtensions = new Dictionary<string, Func<ISyntacticalMatchResult<SyntaxNode>, SyntacticalExtension<SyntaxNode>, SyntaxNode>>();
        Dictionary<string, Func<ISyntacticalMatchResult<SyntaxNode>, SyntacticalExtension<SyntaxNode>, SyntaxNode>> _typeExtensions = new Dictionary<string, Func<ISyntacticalMatchResult<SyntaxNode>, SyntacticalExtension<SyntaxNode>, SyntaxNode>>();

        public TransformExtensions(IEnumerable<SyntacticExtensionEvent<SyntaxNode>> extensions, IEventBus events)
        {
            _events = events;

            foreach (var ev in extensions)
            {
                switch (ev.Kind)
                {
                    case ExtensionKind.Code: _codeExtensions[ev.Keyword] = ev.Handler; break;
                    case ExtensionKind.Member: _memberExtensions[ev.Keyword] = ev.Handler; break;
                    case ExtensionKind.Type: _typeExtensions[ev.Keyword] = ev.Handler; break;
                    default: throw new NotImplementedException();
                }
            }
        }

        class LookAheadResult
        {
            public bool Matched { get; set; }
            public SyntaxNode Result { get; set; }
            public Func<SyntaxNode, LookAheadResult> Continuation { get; set; }
        }

        Func<SyntaxNode, LookAheadResult> _lookahead;

        public override SyntaxNode Visit(SyntaxNode node)
        {
            if (_lookahead != null)
            {
                var result = _lookahead(node);
                _lookahead = result.Continuation;
                return result.Result;
            }

            return base.Visit(node);
        }

        public override SyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax method)
        {
            SyntacticalExtension<SyntaxNode> extension = methodExtension(method);
            if (extension != null)
            {
                switch (extension.Kind)
                {
                    case ExtensionKind.Member:
                    case ExtensionKind.Type:
                    {
                        RoslynSyntacticalMatchResult result = new RoslynSyntacticalMatchResult(new Scope(), _events, method);
                        return extension.Handler(result, extension);
                    }
                    default:
                    {
                        //td: error, incorrect extension (i.e. a code extension being used inside a type)
                        return null;
                    }
                }
            }

            return base.VisitMethodDeclaration(method);
        }

        private SyntacticalExtension<SyntaxNode> methodExtension(MethodDeclarationSyntax method)
        {
            string extName = null;
            string extIdentifier = null;
            if (!method.ReturnType.IsMissing)
            {
                extName = method.ReturnType.ToString();
                extIdentifier = method.Identifier.ToString();
            }
            else
            {
                extName = method.Identifier.ToString();
            }

            if (method.Parent is CompilationUnitSyntax || method.Parent is NamespaceDeclarationSyntax)
            {
                if (_typeExtensions.ContainsKey(extName))
                    return new SyntacticalExtension<SyntaxNode>
                    {
                        Kind = ExtensionKind.Type,
                        Keyword = extName,
                        Identifier = extIdentifier,
                        Arguments = method.ParameterList,
                        Body = method.Body,
                        Handler = _typeExtensions[extName]
                    };
            }


            if (!_memberExtensions.ContainsKey(extName))
                return null;

            return new SyntacticalExtension<SyntaxNode>
            {
                Kind = ExtensionKind.Member,
                Keyword = extName,
                Identifier = extIdentifier,
                Arguments = method.ParameterList,
                Body = method.Body,
                Handler = _memberExtensions[extName]
            };
        }

        public override SyntaxNode VisitIncompleteMember(IncompleteMemberSyntax node)
        {
            SyntacticalExtension<SyntaxNode> extension = typeExtension(node);
            if (extension != null)
            {
                if (extension.Kind == ExtensionKind.Type)
                {
                    _lookahead = MatchTypeExtension(node, extension); 
                    return null; //remove the incomplete member
                }
                else
                {
                    //td: error, incorrect extension (i.e. a code extension being used inside a type)
                    return null;
                }
            }

            return node; //error, stop processing
        }

        private Func<SyntaxNode, LookAheadResult> MatchTypeExtension(IncompleteMemberSyntax incomplete, SyntacticalExtension<SyntaxNode> extension)
        {
            return node =>
            {
                var resultNode = node;
                if (node is ClassDeclarationSyntax)
                {
                    ClassDeclarationSyntax clazz = (ClassDeclarationSyntax)node;
                    clazz = clazz
                        .WithAttributeLists(incomplete.AttributeLists)
                        .WithModifiers(incomplete.Modifiers);

                    RoslynSyntacticalMatchResult result = new RoslynSyntacticalMatchResult(new Scope(), _events, clazz);
                    resultNode = extension.Handler(result, extension);
                }

                //td: error?, expecting class
                return new LookAheadResult
                {
                    Matched = resultNode != null,
                    Result = resultNode 
                };
            };
        }

        private SyntacticalExtension<SyntaxNode> typeExtension(IncompleteMemberSyntax node)
        {
            throw new NotImplementedException(); 
        }

        public override SyntaxNode VisitExpressionStatement(ExpressionStatementSyntax node)
        {
            var expr = node.Expression;
            InvocationExpressionSyntax call = null;

            if (expr is InvocationExpressionSyntax)
            {
                call = expr as InvocationExpressionSyntax;
            }
            else if (expr is AssignmentExpressionSyntax)
            {
                var assignment = expr as AssignmentExpressionSyntax;
                call = assignment.Right as InvocationExpressionSyntax;
            }

            if (call != null)
            {
                SyntacticalExtension<SyntaxNode> extension = codeExtension(call);
                if (extension != null)
                {

                    if (extension.Kind != ExtensionKind.Code)
                    {
                        //td: error, incorrect extension (i.e. a code extension being used inside a type)
                        return node;
                    }

                    _lookahead = CheckCodeExtension(node, extension); 
                    return null;
                }
            }

            return node;
        }

        public override SyntaxNode VisitLocalDeclarationStatement(LocalDeclarationStatementSyntax node)
        {
            if (node.Declaration.Variables.Count == 1)
            {
                var call = node
                    .Declaration
                    .Variables[0]
                    .Initializer
                    .Value as InvocationExpressionSyntax;

                if (call != null)
                {
                    SyntacticalExtension<SyntaxNode> extension = codeExtension(call);
                    if (extension != null)
                    {

                        if (extension.Kind != ExtensionKind.Code)
                        {
                            //td: error, incorrect extension (i.e. a code extension being used inside a type)
                            return node;
                        }

                        _lookahead = CheckCodeExtension(node, extension); 
                        return null;
                    }
                }
            }

            return base.VisitLocalDeclarationStatement(node);
        }

        private SyntacticalExtension<SyntaxNode> codeExtension(InvocationExpressionSyntax call)
        {
            if (!(call.Expression is SimpleNameSyntax))
                return null; //extensions are simple identifiers

            var extName = call.Expression.ToString();
            if (!_codeExtensions.ContainsKey(extName))
                return null; //not an extension

            return new SyntacticalExtension<SyntaxNode>
            {
                Kind = ExtensionKind.Code,
                Keyword = extName,
                Identifier = null,
                Arguments = call.ArgumentList,
                Body = null,
                Handler = _codeExtensions[extName]
            };
        }

        //rewriters
        private Func<SyntaxNode, LookAheadResult> CheckCodeExtension(SyntaxNode original, SyntacticalExtension<SyntaxNode> extension)
        {
            return node =>
            {
                var code = node as BlockSyntax;
                if (code == null)
                    return new LookAheadResult { Matched = false };

                RoslynSyntacticalMatchResult result = new RoslynSyntacticalMatchResult(new Scope(), _events, node);
                extension.Body = code;

                SyntaxNode resultNode = null;

                if (original is LocalDeclarationStatementSyntax)
                {
                    extension.Kind = ExtensionKind.Expression;
                    resultNode = extension.Handler(result, extension);
                    if (!(resultNode is ExpressionSyntax))
                    {
                        //td: error, expecting expression
                        return new LookAheadResult { Matched = false };
                    }

                    var localDecl = original as LocalDeclarationStatementSyntax;
                    resultNode = localDecl
                        .WithDeclaration(localDecl.Declaration
                            .WithVariables(CSharp.SeparatedList(new[] {
                                localDecl.Declaration.Variables[0]
                                    .WithInitializer(localDecl.Declaration.Variables[0].Initializer
                                        .WithValue((ExpressionSyntax)resultNode))})))
                        .WithSemicolonToken(CSharp.ParseToken(";"));
                }
                else if (original is ExpressionStatementSyntax)
                {
                    var exprStatement = original as ExpressionStatementSyntax;
                    var assignment = exprStatement.Expression as AssignmentExpressionSyntax;
                    if (assignment != null)
                    {
                        extension.Kind = ExtensionKind.Expression;
                        resultNode = extension.Handler(result, extension);
                        if (!(resultNode is ExpressionSyntax))
                        {
                            //td: error, expecting expression
                            return new LookAheadResult { Matched = false };
                        }

                        resultNode = exprStatement
                            .WithExpression(assignment
                                .WithRight((ExpressionSyntax)resultNode))
                            .WithSemicolonToken(CSharp.ParseToken(";"));
                    }
                    else
                    {
                        resultNode = extension.Handler(result, extension);
                    }
                }
                else
                    throw new NotImplementedException();

                return new LookAheadResult
                {
                    Matched = true,
                    Result = resultNode,
                };
            };
        }
    }
}
