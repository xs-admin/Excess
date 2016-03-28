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
                namespace ChameneoRedux
                {
                    public enum Color
                    {
                        blue,
                        red,    
                        yellow,    
                    }

                    public concurrent class Chameneo
                    {
                        public Color Colour {get; private set;}
                        public int Meetings {get; private set;}
                        public int MeetingsWithSelf {get; private set;}
                        public Broker MeetingPlace {get; private set;}

                        public Chameneo(Broker meetingPlace, int color)
                        : this(meetingPlace, (Color)color)
                        {
                        }

                        public Chameneo(Broker meetingPlace, Color color)
                        {
                            MeetingPlace = meetingPlace;
                            Colour = color;
                            Meetings = 0;
                            MeetingsWithSelf = 0;
                        }
    
                        void main() 
	                    {
                            for(;;)
                            {
                                MeetingPlace.request(this);
                                await meet;
                            }
	                    }
	                
                        public void meet(Chameneo other, Color color)
                        {
                            Colour = compliment(Colour, color);
                            Meetings++;
                            if (other == this)
                                MeetingsWithSelf++;
                        }                    

                        public void print()
                        {
                            Console.WriteLine($""{ Colour}, { Meetings}, { MeetingsWithSelf}"");
                        }

                        private static Color compliment(Color c1, Color c2)
                        {
                            switch (c1)
                            {
                                case Color.blue:
                                    switch (c2)
                                    {
                                        case Color.blue: return Color.blue;
                                        case Color.red: return Color.yellow;
                                        case Color.yellow: return Color.red;
                                        default: break;
                                    }
                                    break;
                                case Color.red:
                                    switch (c2)
                                    {
                                        case Color.blue: return Color.yellow;
                                        case Color.red: return Color.red;
                                        case Color.yellow: return Color.blue;
                                        default: break;
                                    }
                                    break;
                                case Color.yellow:
                                    switch (c2)
                                    {
                                        case Color.blue: return Color.red;
                                        case Color.red: return Color.blue;
                                        case Color.yellow: return Color.yellow;
                                        default: break;
                                    }
                                    break;
                            }
                            throw new Exception();
                        }

                    }

                    public concurrent class Broker
                    {
                        int _meetings = 0;
                        public Broker(int meetings)
                        {
                            _meetings = meetings;
                        }

                        Chameneo _first = null;
                        public void request(Chameneo creature)
                        {
                            if (_first != null)
                            {
                                //perform meeting
                                var firstColor = _first.Colour;
                                _first.meet(creature, creature.Colour);
                                creature.meet(_first, firstColor);

                                //prepare for next
                                _first = null;
                                _meetings--;
                                if (_meetings == 0)
                                    App.Stop();
                            }
                            else
                                _first = creature;
                        }
                    }
                }", out text, true, true);

            Assert.IsNotNull(text);
        }
    }
}
