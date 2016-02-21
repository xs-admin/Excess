using Excess.Compiler.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess.Compiler.Tests
{
    [TestClass]
    public class ConcurrentDemos
    {
        [TestMethod]
        public void DiningPhilosophers()
        {
            var errors = null as IEnumerable<Diagnostic>;
            var node = TestRuntime
                .Concurrent
                .Build(@"
                concurrent class philosopher 
                {
                    [Forever]
                    void main(string name, chopstick left, chopstick right) 
	                {
                        _name  = name;
	                    _left  = left;
	                    _right = right;
                               
	                    await think();
	                }
	
	                void think()
	                {
                        console.write(_name + "" is thinking"");
                        seconds(rand(1.0, 2.0))
                            >> hungry();
	                }

	                void hungry()
	                {
                        console.write(_name + "" is hungry"");
	                    (_left.acquire(this) & _right.acquire(this)) 
                            >> eat();
	                }
	
	                void eat()
	                {
                        console.write(_name + "" is eating"");
                        await seconds(rand(1.0, 2.0));

                        _left.release(this); 
                        _right.release(this);
	                }
	                
                    private string _name;
                    private chopstick _left;
	                private chopstick _right;
                }

                concurrent class chopstick
                {
	                public void acquire(object owner)
                    {
                        if (_owner != null)
                            await release();
                        
                        _owner = owner;
                    }
	
	                public void release(object owner)
                    {
                        if (_owner != owner)
                            throw new InvalidOperationException();

                        _owner = null;
                    }

                    private object _owner;
                }", out errors, threads: 3);

            //must not have compilation errors
            Assert.IsNull(errors);

            var vm = node.Spawn("VendingMachine");

            TestRuntime.Concurrent.Fails(vm, "choc");
            TestRuntime.Concurrent.Fails(vm, "toffee");

            TestRuntime.Concurrent.Succeeds(vm, "coin", "choc");
            TestRuntime.Concurrent.Succeeds(vm, "coin", "toffee");

            node.Stop();
        }


        [TestMethod]
        public void DebugPrint()
        {
            RoslynCompiler compiler = new RoslynCompiler();
            Extensions.Concurrent.Extension.Apply(compiler);

            SyntaxTree tree = null;
            string text = null;

            tree = compiler.ApplySemanticalPass(@"
                concurrent class philosopher 
                {
                    [Forever]
                    void main(string name, chopstick left, chopstick right) 
	                {
                        _name  = name;
	                    _left  = left;
	                    _right = right;
                               
	                    await think();
	                }
	
	                void think()
	                {
                        console.write(_name + "" is thinking"");
                        seconds(rand(1.0, 2.0))
                            >> hungry();
	                }

	                void hungry()
	                {
                        console.write(_name + "" is hungry"");
	                    (_left.acquire(this) & _right.acquire(this)) 
                            >> eat();
	                }
	
	                void eat()
	                {
                        console.write(_name + "" is eating"");
                        await seconds(rand(1.0, 2.0));

                        _left.release(this); 
                        _right.release(this);
	                }
	                
                    private string _name;
                    private chopstick _left;
	                private chopstick _right;
                }

                concurrent class chopstick
                {
	                public void acquire(object owner)
                    {
                        if (_owner != null)
                        {
                            await release;
                        }
                        
                        _owner = owner;
                    }
	
	                public void release(object owner)
                    {
                        if (_owner != owner)
                            throw new InvalidOperationException();

                        _owner = null;
                    }

                    private object _owner;
                }", out text);

            Assert.IsNotNull(text);
        }
    }
}
