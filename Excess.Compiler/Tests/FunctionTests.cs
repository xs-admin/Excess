using Excess.Compiler.Mock;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xslang;

namespace Tests
{
    [TestClass]
    public class FunctionTests
    {
        [TestMethod]
        public void NamespaceFunction_Usage()
        {
            var tree = ExcessMock.Link(@"
                namespace SomeNamespace
                {
                    function SomeFunction()
                    {
                        return 10;
                    }
                }", (compiler) => Functions.Apply(compiler));

            var root = tree
                ?.GetRoot()
                ?.NormalizeWhitespace();

            //a class must have been created
            var @class = root
                .DescendantNodes()
                .OfType<ClassDeclarationSyntax>()
                .Single();

            //named with the "Functions" convention
            Assert.AreEqual(@class.Identifier.ToString(), "Functions");

            //the method must remain
            var method = root
                .DescendantNodes()
                .OfType<MethodDeclarationSyntax>()
                .Single();

            //inside the class
            Assert.AreEqual(method.Parent, @class);

            //with its type calculated to int (22 or 64)
            Assert.IsTrue(method.ReturnType.ToString().StartsWith("Int"));
        }

        [TestMethod]
        public void NamespaceFunction_SeveralFunctions_ShouldBeInvokedWithContext()
        {
            var tree = ExcessMock.Link(@"
                namespace SomeNamespace
                {
                    function Function1()
                    {
                        Function2(10);
                    }

                    function Function2(int value)
                    {
                        Console.WriteLine(value);
                    }
                }",
                (compiler) => Functions.Apply(compiler));

            var root = tree
                ?.GetRoot()
                ?.NormalizeWhitespace();

            //two partial classes must have been created
            Assert.AreEqual(root
                .DescendantNodes()
                .OfType<ClassDeclarationSyntax>()
                .Count(), 2);

            //the call from Function1 to Function2 must be made with an internal scope 
            Assert.AreEqual(root
                .DescendantNodes()
                .OfType<InvocationExpressionSyntax>()
                .Where(invocation => invocation.Expression.ToString() == "Function2")
                .Single()
                    .ArgumentList
                    .Arguments[1].ToString(), "__scope");
        }

        [TestMethod]
        public void NamespaceFunction_Injected_Requirements()
        {
            var tree = ExcessMock.Link(@"
                namespace SomeNamespace
                {
                    interface SomeInterface
                    {
                        void SomeMethod();
                    }

                    function SomeFunction()
                    {
                        inject
                        {
                            SomeInterface someInterface;
                        }

                        someInterface.SomeMethod();
                    }
                }", 
                (compiler) =>
                {
                    Functions.Apply(compiler);
                    DependencyInjection.Apply(compiler);
                });

            var root = tree
                ?.GetRoot()
                ?.NormalizeWhitespace();

            //the injection must transform in a variable assignment
            //essentially asking the context to resolve the requested instance
            Assert.IsTrue(root
                .DescendantNodes()
                .OfType<LocalDeclarationStatementSyntax>()
                .Single()
                    .Declaration
                    .Variables
                    .Single()
                        .Initializer
                        .Value.ToString().StartsWith("__scope"));
        }

        [TestMethod]
        public void NamespaceFunction_ScopeKeyword_ShouldCreateANewContext()
        {
            var tree = ExcessMock.Link(@"
                namespace SomeNamespace
                {
                    function Function1()
                    {
                        scope
                        {
                            var InjectedValue = ""Hello"";
                            Function2();
                        }
                    }

                    function Function2()
                    {
                        inject
                        {
                            string InjectedValue;
                        }

                        Console.WriteLine(InjectedValue);
                    }
                }",
                (compiler) =>
                {
                    Functions.Apply(compiler);
                    DependencyInjection.Apply(compiler);
                });

            var root = tree
                ?.GetRoot()
                ?.NormalizeWhitespace();

            //the injected value must be read from the context
            Assert.IsTrue(root
                .DescendantNodes()
                .OfType<StatementSyntax>()
                .Where(statement => statement.ToString() == "string InjectedValue = __scope.get<string>(\"InjectedValue\");")
                .Any());

            //as well as be saved to a different context
            Assert.IsTrue(root
                .DescendantNodes()
                .OfType<StatementSyntax>()
                .Where(statement => statement.ToString() == "__newScope.set(\"InjectedValue\", InjectedValue);")
                .Any());
        }

        [TestMethod]
        public void NamespaceFunction_WithoutModifiers_ShouldBePublic()
        {
            var tree = ExcessMock.Link(@"
                namespace SomeNamespace
                {
                    function Function1()
                    {
                    }
                }",
                (compiler) => Functions.Apply(compiler));

            var root = tree
                ?.GetRoot()
                ?.NormalizeWhitespace();

            //should have created a public method
            var method = root
                .DescendantNodes()
                .OfType<MethodDeclarationSyntax>()
                .Single();

            Assert.IsTrue(method
                .Modifiers
                .Where(modifier => modifier.IsKind(SyntaxKind.PublicKeyword))
                .Any());
        }

        [TestMethod]
        public void Debug()
        {
            var tree = ExcessMock.Compile(@"
                using xs.server;
                using xs.concurrent;

                using demo_transpiler;

                namespace Home
                { 
	                function Transpile(string text)
	                {
		                return _transpiler.Process(text);       
	                }

	                public function TranspileGraph(string text)
	                {
		                return _graphTranspiler.Process(text);      
	                } 
                }", (compiler) => DependencyInjection.Apply(compiler));

            //this is a placeholder to do some debuggin'
            Assert.AreNotEqual(null, tree);
        }
    }
}
