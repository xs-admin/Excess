using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Excess.Compiler.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Excess.Entensions.XS;

namespace Excess.Compiler.Tests
{
    [TestClass]
    public class ExtUsage
    {
        [TestMethod]
        public void MatchExtension()
        {
            RoslynCompiler compiler = new RoslynCompiler();
            Match.Apply(compiler);

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
                .Where(expr => expr.OperatorToken.Kind() == SyntaxKind.BarBarToken)
                .Count() == 1); //must have added an or expression for multiple cases, 
                                //but not on the case containing the default statement
        }

        [TestMethod]
        public void AsynchUsage()
        {
            RoslynCompiler compiler = new RoslynCompiler();
            Asynch.Apply(compiler);

            SyntaxTree tree = null;
            string text = null;

            //event handler usage
            var AsynchText = @"
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

            tree = compiler.ApplySemanticalPass(AsynchText, out text);
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
            Excess.Entensions.XS.Contract.Apply(compiler);

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

        [TestMethod]
        public void RUsage()
        {
            RoslynCompiler compiler = new RoslynCompiler();
            Excess.Extensions.R.Extension.Apply(compiler);

            SyntaxTree tree = null;
            string text = null;

            //usage
            var Vectors = @"
                void main()
                {
                    R()
                    {
                        x <- c(10.4, 5.6, 3.1, 6.4, 21.7)
                        y <- c(x, 0, x)
                        z <- 2*x + y + 1

                        a <- x > 13
                        b <- x[!(is.na(x))]
                        c <- x[-(1:5)]
                    }
                }";

            tree = compiler.ApplySemanticalPass(Vectors, out text);
            Assert.IsTrue(tree.GetRoot()
                .DescendantNodes()
                .OfType<VariableDeclarationSyntax>()
                .Count() == 6); //must have created 5 variables (x, y, z, a, b)

            Assert.IsTrue(tree.GetRoot()
                .DescendantNodes()
                .OfType<BinaryExpressionSyntax>()
                .Count() == 0); //must have replaced all operators

            var Sequences = @"
                void main()
                {
                    R()
                    {
                        x <- 1:30
                        y <- 2*1:15
                        seq(-5, 5, by=.2) -> s3
                        s4 <- seq(length=51, from=-5, by=.2)
                        s5 <- rep(x, times=5)
                    }
                }";

            tree = compiler.ApplySemanticalPass(Sequences, out text);
            Assert.IsTrue(tree.GetRoot()
                .DescendantNodes()
                .OfType<InvocationExpressionSyntax>()
                .Where(invocation => invocation.Expression.ToString().Contains("RR"))
                .Count() == 6); //must have replaced all operators

            var Statements = @"
                void main()
                {
                    R()
                    {
                        x <- 1
                        y <- 2
                        z <- NA

                        if (x == 1) 
                            3 -> z
                                                    
                        if (y == 1) 
                        {
                            3 -> z
                        }
                        else
                        {
                            z1 <- 4
                            z <- z1 
                        }

                        while(z < 10)  c(a, z) -> a

                        for(i in z)
                        {
                            a <- c(a, i);
                        }

                        repeat
                        {
                            b <- a   
                            a <- c(b, b);
                            if (length(a) > 10) break;
                        }
                    }
                }";

            tree = compiler.ApplySemanticalPass(Statements, out text);
            Assert.IsTrue(tree.GetRoot()
                .DescendantNodes()
                .OfType<WhileStatementSyntax>()
                .Count() == 2); //must have replaced a while and a repeat

            Assert.IsTrue(tree.GetRoot()
                .DescendantNodes()
                .OfType<StatementSyntax>()
                .Where(ss => !(ss is ExpressionStatementSyntax || ss is LocalDeclarationStatementSyntax || ss is BlockSyntax))
                .Count() == 7); //3 if, 2 whiles, a foreach, a break
        }

        [TestMethod]
        public void JsonUsage()
        {
            RoslynCompiler compiler = new RoslynCompiler();
            Entensions.XS.Json.Apply(compiler);

            SyntaxTree tree = null;
            string text = null;

            //usage
            var Usage = @"
                void main()
                {
                    var expr = 20;
                    var foo = json()
                    {
                        x : 3,
                        y : [3, 4, 5],
                        z : {a : 10, b : 20},
                        w : 
                        [
                            {a : 100, b : 200, c: [expr, expr + 1, expr + 2]},
                            {a : 150, b : 250, c: [expr, expr - 1, expr - 2]}
                        ]
                    }
                }";

            tree = compiler.ApplySemanticalPass(Usage, out text);

            var anonymous = tree.GetRoot()
                .DescendantNodes()
                .OfType<AnonymousObjectCreationExpressionSyntax>()
                .First();

            Assert.IsNotNull(anonymous); //must have created an anonymous object 
            Assert.IsTrue(anonymous
                .Initializers
                .Count == 4); //4 members

            Assert.IsTrue(tree.GetRoot()
                .DescendantNodes()
                .OfType<ImplicitArrayCreationExpressionSyntax>()
                .Count() == 4); //4 arrays
        }

        [TestMethod]
        public void FlukeUsage()
        {
            RoslynCompiler compiler = new RoslynCompiler();
            Entensions.XS.Members.Apply(compiler);
            Extensions.Fluke.Extension.Apply(compiler);

            SyntaxTree tree = null;
            string text = null;

            //usage
            var Usage = @"
            public repository CompanyAddress
            {
                repository<AddressType> _address;
                constructor(repository<AddressType> address)
                {
                    _address = address;
                }

                private AddressType GetDefaultAddressType()
                {
                    //get default address type
                    return __address.ToList().FirstOrDefault();
                }
            }";

            tree = compiler.ApplySemanticalPass(Usage, out text);

            Assert.AreEqual(tree.GetRoot()
                .DescendantNodes()
                .OfType<ClassDeclarationSyntax>()
                .Count(), 1); //must have added a class

            Assert.AreEqual(tree.GetRoot()
                .DescendantNodes()
                .OfType<InterfaceDeclarationSyntax>()
                .Count(), 1); //must have added an interface
        }
    }
}