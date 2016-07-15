using Microsoft.VisualStudio.TestTools.UnitTesting;
using Excess.Compiler.Mock;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Settings;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Tests
{
    [TestClass]
    public class SettingsTests
    {
        [TestMethod]
        public void Settings_Usage()
        {
            var tree = ExcessMock.Compile(@"
            namespace SomeNS
            {
                settings
                {
                    [SomeSection]
                        SomeIntValue = 12
                        SomeStringValue = ""Hello""
                    [SomeOtherSection]
                        SomeOtherValue = ""World""
                }
            }", builder: (compiler) => SettingExtension.Apply(compiler));

            Assert.IsNotNull(tree);

            //must have created a class
            var @class = tree
                .GetRoot()
                .DescendantNodes()
                .OfType<ClassDeclarationSyntax>()
                .Single();

            //with an "__init" method
            var method = @class
                .DescendantNodes()
                .OfType<MethodDeclarationSyntax>()
                .Single();
            Assert.AreEqual("__init", method.Identifier.ToString());

            //with 3 statements
            Assert.AreEqual(3, method.Body.Statements.Count);

            //all containing a call to ConfigurationManager
            Assert.AreEqual(3, method
                .Body
                .DescendantNodes()
                .OfType<MemberAccessExpressionSyntax>()
                .Where(inv => inv.Expression.ToString() == "ConfigurationManager")
                .Count());
        }
    }
}