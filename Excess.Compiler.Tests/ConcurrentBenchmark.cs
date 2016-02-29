using Excess.Compiler.Tests.TestRuntime;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;

namespace Excess.Compiler.Tests
{
    [TestClass]
    public class ConcurrentBenchmark
    {
        [TestMethod]
        public void ThreadRing()
        {
            var errors = null as IEnumerable<Diagnostic>;
            var node = TestRuntime
                .Concurrent
                .Build(@"
                concurrent class ring_item
                {
                    int _idx;
                    public ring_item(int idx)
                    {
                        _idx = idx;
                    }
                    
                    public ring_item Next {get; set;}

                    static int ITERATIONS = 50*1000*1000;
                    public void token(int value)
                    {
                        if (value >= ITERATIONS)
                        {
                            console.write(_idx);
                            Node.Stop();
                        }
                        else
                            Next.token(value + 1);
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
                node.WaitForCompletion();
            }
            sw.Stop();

            TimeSpan rt = TimeSpan.FromTicks(sw.ElapsedTicks);
            var ts = rt.TotalSeconds.ToString();
            Assert.IsNotNull(ts);
        }

        [TestMethod]
        public void ChameneosRedux()
        {
            IEnumerable<Diagnostic> errors;

            var node = TestRuntime
                .Concurrent
                .Build(@"
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
                }", out errors, threads: 6);

            //must not have compilation errors
            Assert.IsNull(errors);

            //run for a couple different sets
            const int blue = 0;
            const int red = 1;
            const int yellow = 2;
            const int iterations = 6 * 1000 * 1000;

            Action<int, int[]> run = (meeetings, colors) =>
            {
                var broker = node.Spawn("Broker", meeetings);
                for (int i = 0; i < colors.Length; i++)
                    node.Spawn("Chameneo", broker, colors[i]);
            };

            Stopwatch sw = new Stopwatch();
            sw.Start();
            {
                node.StopCount(2);

                run(iterations, new[] { blue, red, yellow });
                //node.WaitForCompletion();
                //node.Restart();
                run(iterations, new[] { blue, red, yellow, red, yellow, blue, red, yellow, red, blue });

                node.WaitForCompletion();
            }
            sw.Stop();

            TimeSpan rt = TimeSpan.FromTicks(sw.ElapsedTicks);
            var ts = rt.TotalSeconds.ToString();
            Assert.IsNotNull(ts);
        }
    }
}
