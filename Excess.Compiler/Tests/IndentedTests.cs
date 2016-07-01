using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Excess.Compiler;
using Microsoft.CodeAnalysis;

namespace Tests
{
    using System.Collections.Generic;
    using Excess.Compiler.Mock;
    using Excess.Compiler.Roslyn;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Compiler = ICompiler<SyntaxToken, SyntaxNode, SemanticModel>;
    using CSharp = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

    class TestNode
    {
    };

    class TestRootNode : TestNode
    {
    };

    class SiblingNode : TestNode
    {
    };

    static class Indented_Usage_Extension
    {
        public static void Apply(Compiler compiler)
        {
            compiler.Lexical()
                .indented<TestNode>("someExtension", ExtensionKind.Code)
                    .match<TestRootNode>(MatchHeader)
                        .children(child => child
                            .match<TestRootNode, SiblingNode>(MatchAssignment));
        }

        private static SyntaxNode CheckEmpty(SyntaxNode node, SyntaxNode useless, Scope arg3)
        {
            return node is LiteralExpressionSyntax
                ? null
                : node;
        }

        private static Template SomeFunctionCall = Template.ParseStatement("SomeFunctionCall(__0, __1, __2);");
        private static SyntaxNode BuildChildren(SyntaxNode node, IEnumerable<SyntaxNode> children)
        {
            var header = (LiteralExpressionSyntax)node;
            return CSharp.Block(children
                .Select(n => (AssignmentExpressionSyntax)n)
                .Select(assign => SomeFunctionCall.Get<StatementSyntax>(
                    header,
                    assign.Left,
                    assign.Right))
                .ToArray());
        }

        private static SiblingNode MatchAssignment(string text, TestRootNode parent, Scope scope)
        {
            return null;
        }

        private static TestRootNode MatchHeader(string text, Scope scope)
        {
            return null; 
        }
    }

    [TestClass]
    public class IndentedTests
    {
        [TestMethod]
        public void Indented_Usage()
        {
            var tree = ExcessMock.Compile(@"
                class TestClass
                {
                    void TestMethod()
                    {
                        var TestVar = ""World"";        
                        someExtension()
                        {
                            [Header1]
                                Value1 = 10
                                Value2 = ""SomeValue""
                            [Header1]
                                Value3 = ""Hello "" + TestVar
                        }
                    }
                }", (compiler) => Indented_Usage_Extension.Apply(compiler));

            Assert.IsNotNull(tree);
        }
    }
}
