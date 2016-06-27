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

    static class Indented_Usage_Extension
    {
        public static void Apply(Compiler compiler)
        {
            compiler.Lexical()
                .indented("settings", ExtensionKind.Code)
                    .match(SettingHeader)
                        .children(
                            child => child.match<AssignmentExpressionSyntax>(SettingValue),
                            BuildSettings)
                    .after(CheckEmpty);
        }

        private static SyntaxNode CheckEmpty(SyntaxNode node, SyntaxNode useless, Scope arg3)
        {
            return node is LiteralExpressionSyntax
                ? null
                : node;
        }

        private static Template SettingsCall = Template.ParseStatement("SetSettings(__0, __1, __2);");
        private static SyntaxNode BuildSettings(SyntaxNode node, IEnumerable<SyntaxNode> children)
        {
            var settingHeader = (LiteralExpressionSyntax)node;
            return CSharp.Block(children
                .Select(n => (AssignmentExpressionSyntax)n)
                .Select(assign => SettingsCall.Get<StatementSyntax>(
                    settingHeader,
                    assign.Left,
                    assign.Right))
                .ToArray());
        }

        private static bool SettingValue(AssignmentExpressionSyntax arg)
        {
            return true;
        }

        private static SyntaxNode SettingHeader(string text)
        {
            if (text.StartsWith("[") && text.EndsWith("]"))
                return RoslynCompiler.Quoted(text.Substring(1, text.Length - 2));

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
                        settings()
                        {
                            [TestSetting1]
                                TestSettingValue1 = 10
                                TestSettingValue2 = ""SomeValue""
                            [TestSetting2]
                                TestSettingValue3 = ""Hello "" + TestVar
                        }
                    }
                }", (compiler) => Indented_Usage_Extension.Apply(compiler));
        }
    }
}
