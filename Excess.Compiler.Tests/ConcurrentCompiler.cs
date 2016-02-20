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
                    
                    private string C()
                    {
                        if (2 > 1)
                            return ""SomeValue"";

                        F & G;

                        if (1 > 2)
                            return ""SomeValue"";
                        return ""SomeOtherValue"";
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

        [TestMethod]
        public void BasicAssigment()
        {
            RoslynCompiler compiler = new RoslynCompiler();
            Extensions.Concurrent.Extension.Apply(compiler);

            SyntaxTree tree = null;
            string text = null;

            tree = compiler.ApplySemanticalPass(@"
                concurrent class SomeClass 
                { 
                    int E;
                    void main() 
                    {
                        string B;
                        A | (B = C()) & (E = D(10));
                    }

                    public void A();
                    public void F();
                    public void G();
                    
                    private string C()
                    {
                        F & G;

                        return ""SomeValue"";
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

        [TestMethod]
        public void BasicTryCatch()
        {
            RoslynCompiler compiler = new RoslynCompiler();
            Extensions.Concurrent.Extension.Apply(compiler);

            SyntaxTree tree = null;
            string text = null;

            tree = compiler.ApplySemanticalPass(@"
                concurrent class SomeClass 
                { 
                    public void A();
                    public void B();

                    void main() 
                    {
                        try
                        {
                            int someValue = 10;
                            int someOtherValue = 11;

                            A | B;

                            someValue++;

                            B >> A;

                            someOtherValue++;
                        }
                        catch
                        {
                        }
                    }
                }", out text);

            Assert.IsTrue(tree.GetRoot()
                .DescendantNodes()
                .OfType<MethodDeclarationSyntax>()
                .First()
                .ReturnType
                .ToString() == "void"); //must have added a return type
        }

        [TestMethod]
        public void BasicProtection()
        {
            RoslynCompiler compiler = new RoslynCompiler();
            Extensions.Concurrent.Extension.Apply(compiler);

            SyntaxTree tree = null;
            string text = null;

            tree = compiler.ApplySemanticalPass(@"
                concurrent class VendingMachine 
                { 
                    public    void coin();
                    protected void choc();
                    protected void toffee();

                    void main() 
                    {
                        for (;;)
                        {
                            await coin;
                            coin >> (choc | toffee);
                        }
                    }
                }", out text);

            Assert.IsTrue(tree.GetRoot()
                .DescendantNodes()
                .OfType<MethodDeclarationSyntax>()
                .First()
                .ReturnType
                .ToString() == "void"); //must have added a return type
        }

        [TestMethod]
        public void BasicAwait()
        {
            RoslynCompiler compiler = new RoslynCompiler();
            Extensions.Concurrent.Extension.Apply(compiler);

            SyntaxTree tree = null;
            string text = null;

            tree = compiler.ApplySemanticalPass(@"
                concurrent class SomeClass
                { 
                    public void A();
                    public void B();

                    void main() 
                    {
                        await A;
                        int val = await C();
                        val++;
                    }

                    private int C()
                    {
                        await B;
                        return 10;
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
