using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess.Core
{
    public class ResolveTypedFunction : ITreeLookAhead
    {
        public ResolveTypedFunction(FieldDeclarationSyntax field, ExcessContext ctx, bool asPublic)
        {
            field_ = field;
            ctx_   = ctx;
            asPublic_ = asPublic;
        }

        public SyntaxNode rewrite(CSharpSyntaxRewriter transform, SyntaxNode node, out LookAheadAction action)
        {
            if (node is MethodDeclarationSyntax)
            {
                action = LookAheadAction.SUCCEDED;
                MethodDeclarationSyntax method = (MethodDeclarationSyntax)node;

                TypeSyntax type = field_.Declaration.Type;
                SyntaxToken identifier = method.Identifier;
                SyntaxTokenList modifiers = field_.Modifiers.Any() ? field_.Modifiers
                                                                   : SyntaxFactory.TokenList(asPublic_ ? Compiler.Public : Compiler.Private);

                return SyntaxFactory.MethodDeclaration(type, identifier)
                    .WithLeadingTrivia(SyntaxFactory.Space)
                    .WithModifiers(modifiers)
                    .WithParameterList((ParameterListSyntax)(new ExcessParamListRewriter(ctx_)).Visit(method.ParameterList))
                    .WithBody((BlockSyntax)transform.Visit(method.Body));
            }

            action = LookAheadAction.FAILED;
            return null;
        }

        private FieldDeclarationSyntax field_;
        private ExcessContext          ctx_;
        private bool                   asPublic_; 
    }

    public class ResolveTypedef : ITreeLookAhead
    {
        public ResolveTypedef(FieldDeclarationSyntax field, ExcessContext ctx)
        {
            field_ = field;
            ctx_   = ctx;
        }

        public SyntaxNode rewrite(CSharpSyntaxRewriter transform, SyntaxNode node, out LookAheadAction action)
        {
            if (node is IncompleteMemberSyntax)
            {
                action = LookAheadAction.SUCCEDED;

                IncompleteMemberSyntax incomplete = (IncompleteMemberSyntax)node;

                string typeName = incomplete.Type.ToString();
                string typeDef  = field_.Declaration.Variables.First().Identifier.ToString();
                
                var typeNode = SyntaxFactory.ParseTypeName(typeDef);

                ClassDeclarationSyntax parent = (ClassDeclarationSyntax)field_.Parent;
                ctx_.AddTypeInfo(parent.Identifier.ToString(), "typedefs", new Typedef(typeName, (TypeSyntax)ctx_.CompileType(typeNode, parent)));
                return null;
            }

            action = LookAheadAction.FAILED;
            return node;
        }

        private FieldDeclarationSyntax field_;
        private ExcessContext          ctx_;
    }

    public class ResolveDSLCode : ITreeLookAhead
    {
        public ResolveDSLCode(IDSLHandler dsl, ExcessContext ctx, DSLContext dctx)
        {
            dsl_  = dsl;
            ctx_  = ctx;
            dctx_ = dctx;
        }

        public SyntaxNode rewrite(CSharpSyntaxRewriter transform, SyntaxNode node, out LookAheadAction action)
        {
            if (node is BlockSyntax)
            {
                action = LookAheadAction.SUCCEDED;

                BlockSyntax code = (BlockSyntax)node;
                return dsl_.setCode(ctx_, dctx_, code);
            }

            action = LookAheadAction.FAILED;
            return null;
        }

        private IDSLHandler   dsl_;
        private ExcessContext ctx_;
        private DSLContext    dctx_;
    }

    public class ResolveDSLClass : ITreeLookAhead
    {
        public ResolveDSLClass(IDSLHandler dsl, ExcessContext ctx, List<MemberDeclarationSyntax> extraMembers)
        {
            dsl_ = dsl;
            ctx_ = ctx;
            extraMembers_ = extraMembers;
        }

        public SyntaxNode rewrite(CSharpSyntaxRewriter transform, SyntaxNode node, out LookAheadAction action)
        {
            if (node is ClassDeclarationSyntax)
            {
                action = LookAheadAction.SUCCEDED;
                ClassDeclarationSyntax clazz = (ClassDeclarationSyntax)node;
                DSLContext dctx = new DSLContext { MainNode = node, Surroundings = DSLSurroundings.Global, ExtraMembers = extraMembers_ };
                return dsl_.compile(ctx_, dctx);
            }

            action = LookAheadAction.FAILED;
            return null;
        }

        private IDSLHandler           dsl_;
        private ExcessContext         ctx_;
        List<MemberDeclarationSyntax> extraMembers_;
    }

    public class ResolveEventArguments : ITreeLookAhead
    {
        public ResolveEventArguments(string name, ExcessContext ctx, List<MemberDeclarationSyntax> extraMembers, SyntaxTokenList modifiers)
        {
            name_         = name;
            ctx_          = ctx;
            extraMembers_ = extraMembers;
            modifiers_    = modifiers;  
        }

        public SyntaxNode rewrite(CSharpSyntaxRewriter transform, SyntaxNode node, out LookAheadAction action)
        {
            if (node is FieldDeclarationSyntax)
            {
                FieldDeclarationSyntax fd = (FieldDeclarationSyntax)node;

                var type = fd.Declaration.Type;

                string param_name = fd.Declaration.Variables.First().Identifier.ToString().Replace(")", "");
                params_.Add(SyntaxFactory.Parameter(SyntaxFactory.Identifier(param_name)).
                                            WithType(type));

                switch (fd.Declaration.Variables.Count)
                {
                    case 1:
                    {
                        action = LookAheadAction.SUCCEDED;
                        extraMembers_.Add(SyntaxFactory.DelegateDeclaration(Compiler.Void, name_).
                                            WithParameterList(SyntaxFactory.ParameterList(SyntaxFactory.SeparatedList(params_))).
                                            WithModifiers(modifiers_));
                        break;
                    }

                    case 2:
                    {
                        action = LookAheadAction.CONTINUE;
                        break;
                    }

                    default:
                    {
                        action = LookAheadAction.FAILED;
                        break;
                    }
                }

                return null;
            }
            else
            {
                Debug.Assert(false); //td: error
            }

            action = LookAheadAction.FAILED;
            return null;
        }

        private string                name_;
        private ExcessContext         ctx_;
        List<MemberDeclarationSyntax> extraMembers_;
        List<ParameterSyntax>         params_ = new List<ParameterSyntax>();  
        SyntaxTokenList               modifiers_;
    }

    public class ResolveProperty : ITreeLookAhead
    {
        public ResolveProperty(PropertyDeclarationSyntax prop)
        {
            result_ = prop;
        }

        public SyntaxNode rewrite(CSharpSyntaxRewriter transform, SyntaxNode node, out LookAheadAction action)
        {
            if (node is IncompleteMemberSyntax)
            {
                var incomplete = (IncompleteMemberSyntax)node;
                var identifier = incomplete.Type.ToString();
                var complete   = SyntaxFactory.ParseStatement(incomplete.ToFullString());
                var value      = null as ExpressionSyntax;
                if (complete is ExpressionStatementSyntax)
                {
                    ExpressionStatementSyntax exprStatement = (ExpressionStatementSyntax)complete;
                    switch (exprStatement.Expression.CSharpKind())
                    {
                        case SyntaxKind.SimpleAssignmentExpression:
                        {
                            BinaryExpressionSyntax bs = (BinaryExpressionSyntax)exprStatement.Expression;
                            value = bs.Right;
                            break;
                        }
                        case SyntaxKind.IdentifierName:
                        {
                            break;
                        }
                        default:
                        {
                            //td: error
                            break;
                        }
                    }
                }

                if (value != null)
                {
                    //td: !!! initializer
                }

                action = LookAheadAction.SUCCEDED;
                return result_.ReplaceToken(result_.Identifier, SyntaxFactory.ParseToken(identifier));
            }

            action = LookAheadAction.FAILED;
            return node;
        }

        private PropertyDeclarationSyntax result_;
    }

    public class ResolveArrayArgument : ITreeLookAhead
    {
        public ResolveArrayArgument(ExcessContext ctx, SyntaxNode node, ArrayCreationExpressionSyntax arg)
        {
            ctx_  = ctx;
            node_ = node;
            
            args_.Add(SyntaxFactory.Argument(arg));
        }

        public SyntaxNode rewrite(CSharpSyntaxRewriter transform, SyntaxNode node, out LookAheadAction action)
        {
            if (node is EmptyStatementSyntax)
            {
                var arguments = SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(args_));
                ctx_.AddLinkData(node_, arguments);
                
                action = LookAheadAction.SUCCEDED;
                return SyntaxFactory.EmptyStatement(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
            }

            if (node is VariableDeclaratorSyntax)
            {
                //found another array
                var vard   = (VariableDeclaratorSyntax)node;
                if (vard.ArgumentList != null)
                {
                    var values = vard.ArgumentList.Arguments.Select<ArgumentSyntax, ExpressionSyntax>(arg => arg.Expression);
                    var array = SyntaxFactory.ArrayCreationExpression(SyntaxFactory.ArrayType(SyntaxFactory.IdentifierName("object[]")),
                                                            SyntaxFactory.InitializerExpression(SyntaxKind.ArrayInitializerExpression,
                                                                SyntaxFactory.SeparatedList(values)));
                    //td: link
                    args_.Add(SyntaxFactory.Argument(array));
                }
                
                action = LookAheadAction.CONTINUE;
                return null;
            }

            if (node is ExpressionStatementSyntax)
            {
                var expr = (ExpressionStatementSyntax)transform.Visit(node);
                args_.Add(SyntaxFactory.Argument(expr.Expression));
                action = LookAheadAction.CONTINUE;
                return null;
            }

            action = LookAheadAction.FAILED;
            return node;
        }

        private ExcessContext        ctx_;
        private SyntaxNode           node_;
        private List<ArgumentSyntax> args_ = new List<ArgumentSyntax>(); 
    }
    
}
