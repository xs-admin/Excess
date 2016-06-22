using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Excess.Compiler.Roslyn;
using Microsoft.CodeAnalysis;
using Json;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Tests
{
    [TestClass]
    public class JsonTests
    {
        [TestMethod]
        public void Json_Usage()
        {
            RoslynCompiler compiler = new RoslynCompiler();
            JsonExtension.Apply(compiler);

            SyntaxTree tree = null;
            string text = null;

            //usage
            var Code = @"
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

            tree = compiler.ApplySemanticalPass(Code, out text);

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

    }
}
