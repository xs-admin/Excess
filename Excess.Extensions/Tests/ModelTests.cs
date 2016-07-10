using Excess.Compiler.Mock;
using Excess.Extensions.Model;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    [TestClass]
    public class ModelTests
    {
        [TestMethod]
        public void Model_Usage()
        {
            var tree = ExcessMock.Compile(@"
            namespace SomeNS
            {
                model SomeModel
                {
                    int Property1;
                    string Property2 = ""SomeValue"";
                }
            }", builder: (compiler) => ModelExtension.Apply(compiler));

            Assert.IsNotNull(tree);

            //an struct should have been added
            var @struct = tree
                .GetRoot()
                .DescendantNodes()
                .OfType<StructDeclarationSyntax>()
                .Single();

            //with one constructor
            var constructor = @struct
                .DescendantNodes()
                .OfType<ConstructorDeclarationSyntax>()
                .Single();

            //must have 2 parameters and 2 assignments
            Assert.AreEqual(2, constructor.ParameterList.Parameters.Count);
            Assert.AreEqual(2, constructor
                .DescendantNodes()
                .OfType<AssignmentExpressionSyntax>()
                .Count());

            //must have 2 properties with private sets
            Assert.AreEqual(2, @struct
                .DescendantNodes()
                .OfType<PropertyDeclarationSyntax>()
                .Where(property => property
                    .AccessorList
                    .Accessors
                    .Any(accessor => 
                        accessor.Keyword.IsKind(SyntaxKind.SetKeyword)
                        && accessor
                            .Modifiers
                            .Any(modifier => modifier.IsKind(SyntaxKind.PrivateKeyword))))
                .Count());
        }
    }
}
