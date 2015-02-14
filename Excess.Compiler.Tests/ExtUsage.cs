using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Excess.Compiler.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;

namespace Excess.Compiler.Tests
{
    [TestClass]
    public class ExtUsage
    {
        [TestMethod]
        public void Match()
        {
            RoslynCompiler compiler = new RoslynCompiler();
            Extensions.Match.Apply(compiler);

            SyntaxTree tree = null;
            string text = null;

            //event handler usage
            var SimpleUsage = @"
                class foo
                {
                    void bar()
                    {
                        match(x)
                        {
                            case 10: is_10();
                            case > 10: greater_than_10();
                            default: less_than_10();
                        }
                    }
                }";

            tree = compiler.ApplySemanticalPass(SimpleUsage, out text);
            Assert.IsTrue(tree.GetRoot()
                .DescendantNodes()
                .OfType<IfStatementSyntax>()
                .Count() == 2); //must have replaced the match with 2 ifs

            Assert.IsTrue(tree.GetRoot()
                .DescendantNodes()
                .OfType<ElseClauseSyntax>()
                .Count() == 2); //must have added an else for the default case

            var MultipleUsage = @"
                class foo
                {
                    void bar()
                    {
                        match(x)
                        {
                            case 10: 
                            case 20: is_10_or_20();
                            case > 10: 
                                greater_than_10();
                                greater_than_10();
                            
                            case < 10: 
                            default: less_than_10();
                        }
                    }
                }";

            tree = compiler.ApplySemanticalPass(MultipleUsage, out text);
            Assert.IsTrue(tree.GetRoot()
                .DescendantNodes()
                .OfType<IfStatementSyntax>()
                .First()
                    .DescendantNodes()
                    .OfType<BlockSyntax>()
                    .Count() == 1); //must have added a block for multiple stements

            Assert.IsTrue(tree.GetRoot()
                .DescendantNodes()
                .OfType<BinaryExpressionSyntax>()
                .Where(expr => expr.OperatorToken.CSharpKind() == SyntaxKind.BarBarToken)
                .Count() == 1); //must have added an or expression for multiple cases, 
                                //but not on the case containing the default statement
        }

        [TestMethod]
        public void Asynch()
        {
            RoslynCompiler compiler = new RoslynCompiler();
            Extensions.Asynch.Apply(compiler);

            SyntaxTree tree = null;
            string text = null;

            //event handler usage
            var Asynch = @"
                class foo
                {
                    void bar()
                    {
                        asynch()
                        {
                            foobar();
                        }
                    }
                }";

            tree = compiler.ApplySemanticalPass(Asynch, out text);
            Assert.IsTrue(tree.GetRoot()
                .DescendantNodes()
                .OfType<ParenthesizedLambdaExpressionSyntax>()
                .Count() == 1); //must have added a callback 

            Assert.IsTrue(tree.GetRoot()
                .DescendantNodes()
                .OfType<InvocationExpressionSyntax>()
                .Where(invocation => invocation.Expression.ToString() == "Task.Factory.StartNew")
                .Count() == 1); //must have added a task factory invocation

            Assert.IsTrue(tree.GetRoot()
                .DescendantNodes()
                .OfType<LocalDeclarationStatementSyntax>()
                .Count() == 1); //must have added a local variable for the asynch context

            var Synch = @"
                class foo
                {
                    void bar()
                    {
                        asynch()
                        {
                            foobar();
                            synch()
                            {
                                barfoo();
                            }
                        }
                    }
                }";

            tree = compiler.ApplySemanticalPass(Synch, out text);
            Assert.IsTrue(tree.GetRoot()
                .DescendantNodes()
                .OfType<ParenthesizedLambdaExpressionSyntax>()
                .Count() == 2); //must have added a callback for asynch and another for synch
        }

        [TestMethod]
        public void Contract()
        {
            RoslynCompiler compiler = new RoslynCompiler();
            Extensions.Contract.Apply(compiler);

            SyntaxTree tree = null;
            string text = null;

            //usage
            var Usage = @"
                class foo
                {
                    void bar(int x, object y)
                    {
                        contract()
                        {
                            x > 3;
                            y != null;
                        }
                    }
                }";

            tree = compiler.ApplySemanticalPass(Usage, out text);
            Assert.IsTrue(tree.GetRoot()
                .DescendantNodes()
                .OfType<IfStatementSyntax>()
                .Count() == 2); //must have added an if for each contract condition

            //errors
            var Errors = @"
                class foo
                {
                    void bar(int x, object y)
                    {
                        contract()
                        {
                            x > 3;
                            return 4; //contract01 - only expressions
                        }

                        var noContract = contract() //contract01 - not as expression
                        {
                            x > 3;
                            return 4; 
                        }
                    }
                }";

            var doc = compiler.CreateDocument(Errors);
            compiler.ApplySemanticalPass(doc, out text);
            Assert.IsTrue(doc
                .GetErrors()
                .Count() == 2); //must produce 2 errors
        }
    }
}