using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Diagnostics;
using Excess.Core;
using Excess.RuntimeProject;

namespace Excess.DSL
{
    public class PureFactory
    {
        public static string DSLName = "pure";

        public static IDSLFactory Create()
        {
            PureParser.DSLName = DSLName;
            PureLinker.DSLName = DSLName;

            var parser = new PureParser();
            var linker = new PureLinker();

            parser.Linker = linker;
            return new ManagedDSLFactory(DSLName, parser, linker);
        }
    }

    public class PureParser : ManagedParser<PureLinker>
    {

        public SyntaxNode ParseClass(SyntaxNode node, string id, ParameterListSyntax args)
        {
            IEnumerable<SyntaxNode> assigments = node.DescendantNodes()
                                                     .OfType<ExpressionStatementSyntax>()
                                                     .Where(statement => IsAssigment(statement.Expression.CSharpKind()))
                                                     .Select(assignment => assignment.Expression);

            IEnumerable<SyntaxNode> calls = node.DescendantNodes()
                                                .OfType<InvocationExpressionSyntax>();

            var result = node.ReplaceNodes(assigments.Union(calls), (oldNode, newNode) =>
            {
                if (newNode is InvocationExpressionSyntax)
                    return Link(newNode, Linker.CheckCall);

                return Link(newNode, Linker.CheckAssignment);
            });

            return MarkAsOurs(result);
        }
    }

    public class PureLinker : ManagedLinker
    {
        public SyntaxNode CheckCall(SyntaxNode node, SemanticModel model)
        {
            InvocationExpressionSyntax call   = (InvocationExpressionSyntax)node;
            ISymbol                    caller = GetSemantics(model,  call.Expression);

            bool isPure = false;
            if (caller == null)
                Error(call, "cannot resolve '" + call.Expression.ToString() + "'");
            else
            {
                ITypeSymbol callee = caller.ContainingType;
                if (callee == null)
                    Error(call, "cannot resolve '" + call.Expression.ToString() + "'");
                else
                {
                    //code in a pure class should not modify foreign state.
                    //first we trust static classes
                    isPure = callee.IsStatic;
                    if (!isPure)
                    {
                        //then pures 
                        ClassDeclarationSyntax calleeSyntax = GetSyntax(callee) as ClassDeclarationSyntax;
                        if (calleeSyntax != null)
                            isPure = MarkedAsOurs(calleeSyntax);
                    }

                    if (!isPure)
                    {
                        //finally check for static methods
                        isPure = caller.IsStatic;
                    }

                    if (/* still */!isPure)
                        Error(call, "'" + call.Expression.ToString() + "' cannot not be guaranteed pure");
                }
            }

            return node;
        }

        public SyntaxNode CheckAssignment(SyntaxNode node, SemanticModel model)
        {
            BinaryExpressionSyntax assigment = (BinaryExpressionSyntax)node;
            ISymbol                assigner  = model.GetSymbolInfo(assigment.Left).Symbol;

            if (assigner == null)
                Error(assigment, "cannot resolve '" + assigment.ToString() + "'");
            else
            {
                //Allowed assigments only to modify internal state
                var ourClass = node.FirstAncestorOrSelf<ClassDeclarationSyntax>(); Debug.Assert(ourClass != null);
                var ourType  = model.GetSymbolInfo(ourClass).Symbol;

                if (assigner.ContainingType == ourType)
                    Error(assigment, "'" + assigment.ToString() + "' is an impure construct");
            }

            return node;
        }

        public SyntaxNode Link(SyntaxNode node, SemanticModel model)
        {
            return node;
        }
    }
}