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

                namespace Santa
                {
	                concurrent object SantaClaus
	                {
		                List<Reindeer> _reindeer = new List<Reindeer>();
		                List<Elf> _elves = new List<Elf>();
		                bool _busy = false;
		                public void reindeer(Reindeer r)
		                {
			                _reindeer.Add(r);
			                if (readyToDeliver())
			                {
				                //in case we're meeting with elves
				                if (_busy)
					                cancelMeeting() >> meetingCanceled;

				                //christmas!
				                _busy = true;
				                Console.WriteLine(""Santa: Off to deliver toys!"");
				                await seconds(rand(5, 10));
				                Console.WriteLine(""Santa: Merry Christmas, enjoy the toys!"");

				                //is over 
				                foreach(var rd in _reindeer)
					                rd.unharness();

				                _reindeer.Clear();
				                _busy = false;
			                }
		                }

		                public void elf(Elf e)
		                {
			                if (_busy)
			                {
				                e.advice(false);
				                return;
			                }

			                _elves.Add(e);
			                if (_elves.Count == 3)
			                {
				                _busy = true;

				                Console.WriteLine(""Santa: hey guys, need help? "");
				                seconds(1, 2) | cancelMeeting;

				                var isDelivering = readyToDeliver();
				                if (isDelivering)
				                {
					                Console.WriteLine(""Santa: sorry fellows, got toys to deliver!"");
					                meetingCanceled();
				                }
				                else
				                {
					                Console.WriteLine(""Santa: Good meeting, little fellas!"");
					                _busy = false;
				                }

				                //adjourned
				                foreach(var elf in _elves)
					                elf.advice(!isDelivering);

				                _elves.Clear();
			                }    
		                }

		                private void cancelMeeting();
		                private void meetingCanceled();

		                private bool readyToDeliver()
		                {
			                return _reindeer.Count == 8;
		                }
	                }
                }", out text, false, false); 

            Assert.IsNotNull(text);
        }
    }
}
