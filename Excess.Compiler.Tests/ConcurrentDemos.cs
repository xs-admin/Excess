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
                node.Spawn("chopstick"))
                .ToArray();

            var phCount = names.Length;
            for (int i = 0; i < phCount; i++)
            {
                var left = chopsticks[i];
                var right = i == phCount - 1 ? chopsticks[0] : chopsticks[i + 1];

                node.Spawn("philosopher", names[i], left, right);
            }

            Thread.Sleep(45000);
            node.Stop();
            node.waitForCompletion();

            Assert.AreEqual(150, console.items().Length);
        }


        [TestMethod]
        public void Barbers()
        {
            var errors = null as IEnumerable<Diagnostic>;
            var node = TestRuntime
                .Concurrent
                .Build(@"
                concurrent class barbershop
                {
                    barber[] _barbers;
                    bool[] _busy;
                    
                    public barbershop(barber barber1, barber barber2)
                    {
                        _barbers = new [] {barber1, barber2}; 
                        _busy    = new [] {false, false}; 
                    }

                    public void visit(int client)
                    {
                        console.write($""Client: {client}, Barber1 {barber_status(0)},  Barber2: {barber_status(1)}"");
                        if (_busy[0] && _busy[1])
                            await visit.enqueue();

                        for(int i = 0; i < 2; i++)
                        {
                            if (!_busy[i]) 
                            {
                                await shave_client(client, i);
                                break;
                            }
                        }

                        visit.dequeue();
                    }

                    private void shave_client(int client, int which)
                    {
                        var barber = _barbers[which];
                        double tip = rand(5, 10);
                        
                        _busy[which] = true;

                        barber.shave(client)
                            >> barber.tip(client, tip);

                        _busy[which] = false;
                    }

                    private string barber_status(int which)
                    {
                        return _busy[which]
                            ? ""working""
                            : ""available"";
                    }
                }

                concurrent class barber
                {
                    int _index;
                    void main(int index)
                    {
                        _index = index;

                        while(true)
                            shave >> tip;
                    }

                    public void shave(int client)
                    {
                        await seconds(rand(1, 2));
                    }

                    double _tip = 0;
                    public void tip(int client, double amount)
                    {
                        _tip += amount;
                        console.write($""Barber {_index}: {client} tipped {amount:C2}, for a total of {_tip:C2}"");
                    }
                }", out errors, threads: 1);

            //must not have compilation errors
            Assert.IsNull(errors);

            var barber1 = node.Spawn("barber", 0);
            var barber2 = node.Spawn("barber", 1);
            var shop = node.Spawn("barbershop", barber1, barber2);

            var rand = new Random();
            var clients = 30;
            for (int i = 1; i <= clients; i++)
            {
                Thread.Sleep((int)(3000 * rand.NextDouble()));
                TestRuntime
                    .Concurrent
                    .SendAsync(shop, "visit", i);
            }

            Thread.Sleep(3000); //wait for last one
            node.Stop();
            node.waitForCompletion();

            var output = console.items();
            Assert.AreEqual(output.Length, 60);
        }

        [TestMethod]
        public void DebugPrint()
        {
            RoslynCompiler compiler = new RoslynCompiler();
            Extensions.Concurrent.Extension.Apply(compiler);

            SyntaxTree tree = null;
            string text = null;

            tree = compiler.ApplySemanticalPass(@"
                concurrent class Chameneo
                {
                    public enum Color
                    {
                        blue,
                        red,    
                        yellow,    
                    }

                    public Color Colour {get; private set;}
                    public int Meetings {get; private set;}
                    public int MeetingsWithSelf {get; private set;}
                    public Broker MeetingPlace {get; private set;}

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
                        console.write($""{Colour}, {Meetings}, {MeetingsWithSelf}"");
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

                concurrent class Broker
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
                                Node.Stop();
                        }
                        else
                            _first = creature;
                    }
                }", out text);

            Assert.IsNotNull(text);
        }

    }
}
