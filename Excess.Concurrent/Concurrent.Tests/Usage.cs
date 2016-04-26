using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Excess.Compiler.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using Excess.Extensions.Concurrent;
using System.Collections.Generic;

namespace Concurrent.Tests
{
    [TestClass]
    public class Usage
    {
        [TestMethod]
        public void BasicOperators()
        {
            RoslynCompiler compiler = new RoslynCompiler();
            Excess.Extensions.Concurrent.Extension.Apply(compiler);

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
                .Count(method =>
                    new[] {
                      "__concurrentmain",
                      "__concurrentA",
                      "__concurrentB",
                      "__concurrentC",
                      "__concurrentF",
                      "__concurrentG",}
                    .Contains(method
                        .Identifier
                        .ToString())) == 6); //must have created concurrent methods

            Assert.IsFalse(tree.GetRoot()
                .DescendantNodes()
                .OfType<MethodDeclarationSyntax>()
                .Any(method => method
                        .Identifier
                        .ToString() == "__concurrentD")); //but not for D
        }

        [TestMethod]
        public void BasicAssigment()
        {
            RoslynCompiler compiler = new RoslynCompiler();
            Extension.Apply(compiler);

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
                .OfType<ClassDeclarationSyntax>()
                .Where(@class => @class.Identifier.ToString() == "__expr1")
                .Single()
                .Members
                .OfType<FieldDeclarationSyntax>()
                .Count(field => new[] { "B", "E" }
                    .Contains(field
                        .Declaration
                        .Variables[0]
                        .Identifier.ToString())) == 2); //must have added fields to the expression object

            Assert.IsTrue(tree.GetRoot()
                .DescendantNodes()
                .OfType<AssignmentExpressionSyntax>()
                .Count(assignment => new[] { "B", "E" }
                    .Contains(assignment
                        .Left
                        .ToString())) == 2); //must have added assignments from fields to the expression object
        }

        [TestMethod]
        public void BasicTryCatch()
        {
            RoslynCompiler compiler = new RoslynCompiler();
            Extension.Apply(compiler);

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
                .OfType<TryStatementSyntax>()
                .Count() == 2); //must have added a a try statement
        }

        [TestMethod]
        public void BasicProtection()
        {
            RoslynCompiler compiler = new RoslynCompiler();
            Extension.Apply(compiler);

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
                            coin >> (choc | toffee);
                        }
                    }
                }", out text);

            Assert.IsTrue(tree.GetRoot()
                .DescendantNodes()
                .OfType<ThrowStatementSyntax>()
                .SelectMany(thrw => thrw
                        .DescendantNodes()
                        .OfType<LiteralExpressionSyntax>())
                .Select(s => s.ToString())
                .Count(s => new[] { "\"choc\"", "\"toffee\"" }
                    .Contains(s)) == 2); //must have added checks for choc and toffee
        }

        [TestMethod]
        public void BasicAwait()
        {
            RoslynCompiler compiler = new RoslynCompiler();
            Extension.Apply(compiler);

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
                .OfType<InvocationExpressionSyntax>()
                .Where(invocation => invocation
                    .Expression
                    .ToString() == "__listen")
                .Count(invocation => new[] { "\"A\"", "\"B\"" }
                    .Contains(invocation
                        .ArgumentList
                        .Arguments[0]
                        .Expression.ToString())) == 2); //must have listened to both signals
        }

        [TestMethod]
        public void BasicProtectionRuntime()
        {
            var errors = null as IEnumerable<Diagnostic>;
            var node = Mock.Build(@"
                concurrent class VendingMachine 
                { 
                    public    void coin();
                    protected void choc();
                    protected void toffee();

                    void main() 
                    {
                        for (;;)
                        {
                            coin >> (choc | toffee);
                        }
                    }
                }", out errors);

            //must not have compilation errors
            Assert.IsNull(errors);

            var vm = node.Spawn("VendingMachine");

            Mock.AssertFails(vm, "choc");
            Mock.AssertFails(vm, "toffee");

            Mock.Succeeds(vm, "coin", "choc");
            Mock.Succeeds(vm, "coin", "toffee");
        }

        [TestMethod]
        public void BasicSingleton()
        {
            var errors = null as IEnumerable<Diagnostic>;
            var app = Mock
                .Build(@"
                    concurrent object VendingMachine 
                    { 
                        public    void coin();
                        protected void choc();
                        protected void toffee();

                        void main() 
                        {
                            for (;;)
                            {
                                coin >> (choc | toffee);
                            }
                        }
                    }", out errors);

            //must not have compilation errors
            Assert.IsNull(errors);
            bool throws = false;
            try
            {
                app.Spawn("VendingMachine");
            }
            catch
            {
                throws = true;
            }

            Assert.IsTrue(throws);

            var vm = app.GetSingleton("VendingMachine");
            Assert.IsNotNull(vm);

            Mock.AssertFails(vm, "choc");
            Mock.AssertFails(vm, "toffee");

            Mock.Succeeds(vm, "coin", "choc");
            Mock.Succeeds(vm, "coin", "toffee");
        }

        [TestMethod]

        public void DebugPrint()
        {
            var text = null as string;
            var tree = Mock.Compile(@"
                using xs.concurrent;

                namespace DiningPhilosophers
                {
	                concurrent app
	                {
		                void main()
		                {
			                var names = new []
			                {
				                ""Kant"",
                                ""Archimedes"",
                                ""Nietzche"",
                                ""Plato"",
                                ""Spinoza"",

                            };

                            //create chopsticks
                            var chopsticks = names
                                .Select(n => spawn<chopstick>())
                                .ToArray();

                            //create philosophers
                            var phCount = names.Length;
			                for (int i = 0; i<phCount; i++)
			                {
				                var left = chopsticks[i];
                                var right = i == phCount - 1
                                    ? chopsticks[0]
                                    : chopsticks[i + 1];

                                spawn<philosopher>(names[i], left, right);
                            }
                        }
	                }

                    concurrent class philosopher
                    {
                        string _name;
                        chopstick _left;
                        chopstick _right;
                        int _meals;

                        public philosopher(string name, chopstick left, chopstick right, int meals)
                        {
                            _name = name;
                            _left = left;
                            _right = right;
                            _meals = meals;
                        }
                        
                        void main()
                        {
                            for (int i = 0; i < _meals; i++)
                            {
                                await think();
                            }
                        }

                        void think()
                        {
                            Console.WriteLine(_name + "" is thinking"");
                            seconds(rand(1.0, 2.0))
                                >> hungry();
                        }

                        void hungry()
                        {
                            Console.WriteLine(_name + "" is hungry"");
                            (_left.acquire(this) & _right.acquire(this))
                                >> eat();
                        }

                        void eat()
                        {
                            Console.WriteLine(_name + "" is eating"");
                            await seconds(rand(1.0, 2.0));

                            _left.release(this);
                            _right.release(this);
                        }
                    }

                    concurrent class chopstick
                    {
                        philosopher _owner;

                        public void acquire(philosopher owner)
                        {
                            if (_owner != null)
                            {
                                await release;
                            }

                            _owner = owner;
                        }

                        public void release(philosopher owner)
                        {
                            if (_owner != owner)
                                throw new InvalidOperationException();

                            _owner = null;
                        }
                    }
                }", out text, false, false); 

            Assert.IsNotNull(text);
        }
    }
}
