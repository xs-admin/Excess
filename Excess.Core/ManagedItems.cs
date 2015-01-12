using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Excess.Core
{
    public class ManagedBase
    {
        public static string DSLName
        {
            get { return _name;  }
            set { _name = value; }
        }

        protected static string    _name;
        protected ExcessContext    _ctx;
        protected SyntaxAnnotation _mark;

        public void SetContext(ExcessContext ctx)
        {
            _ctx  = ctx;
            _mark = new SyntaxAnnotation(ctx.GetUnique(_name));
        }

        public void Error(SyntaxNode node, string error)
        {
            _ctx.AddError(node, "XS0000", error);
        }

        public static bool IsVisible(SyntaxTokenList modifiers)
        {
            return modifiers.Select(modifier => modifier.CSharpKind() == SyntaxKind.PublicKeyword).Any();
        }

        public static bool IsVisible(PropertyDeclarationSyntax prop)
        {
            return IsVisible(prop.Modifiers);
        }

        public static bool IsVisible(FieldDeclarationSyntax field)
        {
            return IsVisible(field.Modifiers);
        }

        public static bool IsVisible(MethodDeclarationSyntax method)
        {
            return IsVisible(method.Modifiers);
        }

        public static ClassDeclarationSyntax WithModifiers(ClassDeclarationSyntax decl, bool @static = false, bool @public = false, bool @protected = false, bool @private = false)
        {
            SyntaxTokenList modifiers = new SyntaxTokenList();
            foreach (var modifier in decl.Modifiers)
            {
                bool add = true;
                switch (modifier.CSharpKind())
                {
                    case SyntaxKind.PrivateKeyword:
                    case SyntaxKind.ProtectedKeyword:
                    case SyntaxKind.PublicKeyword:
                        add = !(@public || @private || @protected);
                        break;

                    case SyntaxKind.StaticKeyword:
                        add = !@static;
                        break;
                }

                if (add)
                    modifiers.Add(modifier);
            }

            if (@static)
                modifiers.Add(SyntaxFactory.Token(SyntaxKind.StaticKeyword));

            if (@public)
                modifiers.Add(SyntaxFactory.Token(SyntaxKind.PublicKeyword));

            if (@protected)
                modifiers.Add(SyntaxFactory.Token(SyntaxKind.ProtectedKeyword));

            if (@private)
                modifiers.Add(SyntaxFactory.Token(SyntaxKind.PrivateKeyword));

            return decl.WithModifiers(modifiers);
        }

        public static bool IsAssigment(SyntaxKind kind)
        {
            switch (kind)
            {
                case SyntaxKind.SimpleAssignmentExpression:
                case SyntaxKind.AddAssignmentExpression:
                case SyntaxKind.AndAssignmentExpression:
                case SyntaxKind.DivideAssignmentExpression:
                case SyntaxKind.ExclusiveOrAssignmentExpression:
                case SyntaxKind.LeftShiftAssignmentExpression:
                case SyntaxKind.ModuloAssignmentExpression:
                case SyntaxKind.MultiplyAssignmentExpression:
                case SyntaxKind.OrAssignmentExpression:
                case SyntaxKind.RightShiftAssignmentExpression:
                case SyntaxKind.SubtractAssignmentExpression:
                    return true;
            }

            return false;
        }

        public SyntaxNode MarkAsOurs(SyntaxNode node)
        {
            return node.WithAdditionalAnnotations(_mark);
        }

        public bool MarkedAsOurs(SyntaxNode node)
        {
            return node.HasAnnotation(_mark);
        }

        public static SyntaxNode GetSyntax(ISymbol symbol)
        {
            var syntaxRef = symbol.DeclaringSyntaxReferences.FirstOrDefault(); //td: handle multiple locations, not sure of cases
            return syntaxRef != null ? syntaxRef.GetSyntax() : null;
        }

        public static SyntaxNode GetSyntax(ISymbol symbol, out IEnumerable<SyntaxNode> multiples)
        {
            SyntaxNode result = GetSyntax(symbol);

            multiples = null;
            if (symbol.DeclaringSyntaxReferences.Count() > 1)
                multiples = symbol.DeclaringSyntaxReferences.Select<SyntaxReference, SyntaxNode>( reference => reference.GetSyntax());

            return result;
        }
    }

    public class ManagedParser<TLinker> : ManagedBase
    {
        public TLinker Linker { get; set; }

        public SyntaxNode Link(SyntaxNode node, Func<SyntaxNode, SemanticModel, SyntaxNode> linker)
        {
            var result = _ctx.AddLinker(node, (ctx, linkNode, newNode, model) =>
            {
                return linker(newNode, model);
            });

            return result;
        }
    }

    public class ManagedLinker : ManagedBase
    {
        public ISymbol GetSemantics(SemanticModel model, SyntaxNode node)
        {
            SymbolInfo info = model.GetSymbolInfo(node);
            if (info.Symbol != null)
                return info.Symbol;

            if (info.CandidateSymbols.Count() == 1)
            {
                switch (info.CandidateReason)
                {
                    case CandidateReason.Ambiguous:
                    case CandidateReason.LateBound:
                    case CandidateReason.OverloadResolutionFailure:
                        return info.CandidateSymbols[0]; //give peace a chance
                }
            }

            return null;
        }
    }
}
