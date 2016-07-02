using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Excess.Compiler;
using Excess.Compiler.Roslyn;
using Excess.Compiler.Mock;

namespace Tests
{
    using CSharp = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
    using Compiler = ICompiler<SyntaxToken, SyntaxNode, SemanticModel>;

    class TestNode
    {
    };

    class TestRootNode : TestNode
    {
        public TestRootNode()
        {
            Headers = new List<HeaderNode>();
        }

        public List<HeaderNode> Headers { get; private set; }
    };

    class HeaderNode : TestNode
    {
        public HeaderNode()
        {
            Values = new List<AssignmentExpressionSyntax>();
        }

        public string Name { get; set; }
        public List<AssignmentExpressionSyntax> Values { get; private set; }
    };

    class ValueNode : TestNode
    {
    };

    static class Indented_Usage_Extension
    {
        public static void Apply(Compiler compiler)
        {
            compiler.Lexical()
                .indented<TestNode, TestRootNode>("someExtension", ExtensionKind.Code)
                    .match<TestRootNode, HeaderNode>(MatchHeader)
                        .children(child => child
                            .match<HeaderNode, ValueNode>(MatchValue))
                .then()
                    .transform<TestRootNode>(TransformHeaderNode);
        }

        private static HeaderNode MatchHeader(string text, TestRootNode parent, Scope scope)
        {
            var header = new HeaderNode { Name = text };
            parent.Headers.Add(header);
            return header;
        }

        private static ValueNode MatchValue(string text, HeaderNode parent, Scope scope)
        {
            var assignment = CSharp.ParseExpression(text) as AssignmentExpressionSyntax;
            if (assignment == null)
                return null; //td: error

            parent.Values.Add(assignment);
            return new ValueNode();
        }

        private static Template HeaderValue = Template.ParseStatement("SetHeaderValue(__0, __1, )__2);");
        private static SyntaxNode TransformHeaderNode(TestRootNode root, Func<TestNode, Scope, SyntaxNode> parse, Scope scope)
        {
            var statements = new List<StatementSyntax>();
            foreach (var header in root.Headers)
            {
                foreach (var value in header.Values)
                {
                    statements.Add(HeaderValue.Get<StatementSyntax>(
                        RoslynCompiler.Quoted(header.Name),
                        RoslynCompiler.Quoted(value.Left.ToString()),
                        value.Right));
                }
            }

            return CSharp.Block(statements.ToArray());
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
                            [Header2]
                                Value3 = ""Hello "" + TestVar
                        }
                    }
                }", (compiler) => Indented_Usage_Extension.Apply(compiler));

            Assert.IsNotNull(tree);

            //must have add 3 calls in the form SetHeaderValue("[Header]", "Value", Value);
            Assert.AreEqual(3, tree
                .GetRoot()
                .DescendantNodes()
                .OfType<InvocationExpressionSyntax>()
                .Where(invocation => invocation
                    .Expression
                    .ToString()
                    .StartsWith("SetHeaderValue"))
                .Count());
        }
    }
}
