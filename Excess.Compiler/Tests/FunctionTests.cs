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
                    function SomeFunction(): double
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

            //with its type calculated to int
            Assert.IsTrue(method
                .ReturnType.ToString().ToLower()
                .StartsWith("int"));
        }

        [TestMethod]
        public void NamespaceFunction_Usage_WithType()
        {
            var tree = ExcessMock.Link(@"
                namespace SomeNamespace
                {
                    fn SomeFunction(): double
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

            //with its type calculated to int
            Assert.IsTrue(method
                .ReturnType.ToString().ToLower()
                .StartsWith("double"));
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
        }
    }
}
