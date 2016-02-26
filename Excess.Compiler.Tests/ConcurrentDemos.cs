using Excess.Compiler.Roslyn;
using Excess.Compiler.Tests.TestRuntime;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
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
                    static int Meals = 10;

                    void main(string name, chopstick left, chopstick right) 
	                {
                        _name  = name;
	                    _left  = left;
	                    _right = right;
                               
                        for(int i = 0; i < Meals; i++)
                        {
	                        await think();
                        }    
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
                }", out errors, threads: 1);

            //must not have compilation errors
            Assert.IsNull(errors);

            var names = new[]
            {
                "Kant",
                "Archimedes",
                "Nietzche",
                "Plato",
                "Spinoza",
            };

            var chopsticks = names.Select(n =>
                //node.Spawn("chopstick"))
                node.Spawn<chopstick>())
                .ToArray();

            var phCount = names.Length;
            for (int i = 0; i < phCount; i++)
            {
                var left = chopsticks[i];
                var right = i == phCount - 1 ? chopsticks[0] : chopsticks[i + 1];

                node.Spawn<philosopher>(names[i], left, right);
                //node.Spawn("philosopher", names[i], left, right);
            }

            Thread.Sleep(45000);

            var output = console.items();
            node.Stop();
        }


        [TestMethod]
        public void ThreadRing()
        {
            var errors = null as IEnumerable<Diagnostic>;
            var node = TestRuntime
                .Concurrent
                .Build(@"
                concurrent class ring_item
                {
                    ring_item _next;
                    int _idx;
                    public ring_item(int idx)
                    {
                        _idx = idx;
                    }
                    
                    public ring_item Next 
                    {
                        set {_next = value;}
                    }

                    static int ITERATIONS = 50*1000*1000;
                    public void token(int value)
                    {
                        if (value >= ITERATIONS)
                        {
                            console.write(_idx);
                            Node.Stop();
                        }
                        else
                            _next.token(value + 1);
                    }                    
                }", out errors, threads: 1);

            //must not have compilation errors
            Assert.IsNull(errors);

            const int ringCount = 503;

            //create the ring
            var items = new ConcurrentObject[ringCount];
            for (int i = 0; i < ringCount; i++)
                items[i] = node.Spawn("ring_item", i);

            //update connectivity
            for (int i = 0; i < ringCount; i++)
            {
                var curr = items[i];
                var next = i < ringCount - 1 ? items[i + 1] : items[0];
                TestRuntime.Concurrent.Send(curr, "Next", next);
            }

            Stopwatch sw = new Stopwatch();
            sw.Start();
            {
                //run it by sending the first token, it will go around 50M times
                TestRuntime.Concurrent.Send(items[0], "token", 0);
                node.waitForCompletion();
            }
            sw.Stop();

            TimeSpan rt = TimeSpan.FromTicks(sw.ElapsedTicks);
            var ts = rt.TotalSeconds.ToString();
            Assert.IsNotNull(ts);
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
                    static int Meals = 10;

                    void main(string name, chopstick left, chopstick right) 
	                {
                        _name  = name;
	                    _left  = left;
	                    _right = right;
                               
                        for(int i = 0; i < Meals; i++)
                        {
	                        await think();
                        }    
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
