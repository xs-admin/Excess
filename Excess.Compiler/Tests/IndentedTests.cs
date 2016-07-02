using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.CodeAnalysis;
using Excess.Compiler;
using Excess.Compiler.Mock;

namespace Tests
{
    using Compiler = ICompiler<SyntaxToken, SyntaxNode, SemanticModel>;

    class TestNode
    {
    };

    class TestRootNode : TestNode
    {
        public TestRootNode()
        {
            Siblings = new List<SiblingNode>();
        }

        public List<SiblingNode> Siblings { get; private set; }
    };

    class HeaderNode : TestNode
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
                    .match<TestRootNode, HeaderNode>(MatchHeader)
                        .children(child => child
                            .match<TestRootNode, SiblingNode>(MatchSibling));
        }

        private static HeaderNode MatchHeader(string text, TestRootNode parent, Scope scope)
        {
            return new HeaderNode();
        }

        private static SiblingNode MatchSibling(string text, TestRootNode parent, Scope scope)
        {
            return new SiblingNode();
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
