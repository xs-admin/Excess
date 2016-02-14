using Excess.Compiler.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace Excess.Compiler.Tests
{
    [TestClass]
    public class ConcurrentCompiler
    {
        [TestMethod]
        public void BasicOperators()
        {
            RoslynCompiler compiler = new RoslynCompiler();
            Extensions.Concurrent.Extension.Apply(compiler);

            SyntaxTree tree = null;
            string text = null;

            tree = compiler.ApplySemanticalPass(@"
                concurrent class SomeClass 
                { 
                    void main() 
                    {
                        A | (B & C()) >> D(10);
                    }

                    public void A();
                    public void B();
                    public void F();
                    public void G();
                    
                    private void C()
                    {
                        F & G;
                    }

                    private int D(int v)
                    {
                        return v + 1;
                    }
                }", out text);

            Assert.IsTrue(tree.GetRoot()
                .DescendantNodes()
                .OfType<MethodDeclarationSyntax>()
                .First()
                .ReturnType
                .ToString() == "void"); //must have added a return type
        }
    }
}
