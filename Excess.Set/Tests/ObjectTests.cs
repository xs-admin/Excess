﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Excess.Compiler.Mock;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;
using System.Linq;
using Compiler;

namespace Tests
{
    [TestClass]
    public class ObjectTests
    {
        [TestMethod]
        public void Object_Usage()
        {
            var tree = ExcessMock.Link(@"
                namespace SomeNamespace
                {
                    object SomeObject
                    {
                        int SomeInt;
                        string SomeString;
                    }
                }", (compiler) => ObjectExtension.Apply(compiler));

            var root = tree
                ?.GetRoot()
                ?.NormalizeWhitespace();

            //a class must have been created
            var @class = root
                .DescendantNodes()
                .OfType<ClassDeclarationSyntax>()
                .Single();

            //named with the "Functions" convention
            Assert.AreEqual(@class.Identifier.ToString(), "SomeObject");

            //must have added properties
            Assert.AreEqual(2, root
                .DescendantNodes()
                .OfType<PropertyDeclarationSyntax>()
                .Count());
        }
    }
}