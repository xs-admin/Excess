using System.Text;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Excess.Concurrent.Runtime;

namespace ChameneoRedux
{
    public enum Color
    {
        blue,
        red,
        yellow,
    }

    [Concurrent(id = "0ac84506-1178-47f7-b45c-f9d3b17a8826")]
    public class Chameneo : ConcurrentObject
    {
        public Color Colour
        {
            get;
            private set;
        }

        public int Meetings
        {
            get;
            private set;
        }

        public int MeetingsWithSelf
        {
            get;
            private set;
        }

        public Broker MeetingPlace
        {
            get;
            private set;
        }

        public Chameneo(Broker meetingPlace, Color color)
        {
            MeetingPlace = meetingPlace;
            Colour = color;
            Meetings = 0;
            MeetingsWithSelf = 0;
        }

        protected override void __started()
        {
            var __enum = __concurrentmain(default (CancellationToken), null, null);
            __enter(() => __advance(__enum.GetEnumerator()), null);
        }

        [Concurrent]
        public void meet(Chameneo other, Color color)
        {
            meet(other, color, default (CancellationToken), null, null);
        }

        private IEnumerable<Expression> __concurrentmain(CancellationToken __cancellation, Action<object> __success, Action<Exception> __failure)
        {
            for (;;)
            {
                MeetingPlace.request(this);
                {
                    var __expr1_var = new __expr1{Start = (___expr) =>
                    {
                        var __expr = (__expr1)___expr;
                        __listen("meet", () =>
                        {
                            __expr.__op1(true, null, null);
                        }

                        );
                        __expr.__op1(null, false, null);
                    }

                    , End = (__expr) =>
                    {
                        __enter(() => __advance(__expr.Continuator), __failure);
                    }
                    };
                    yield return __expr1_var;
                    if (__expr1_var.Failure != null)
                        throw __expr1_var.Failure;
                }
            }

            {
                __dispatch("main");
                if (__success != null)
                    __success(null);
                yield break;
            }
        }

        private IEnumerable<Expression> __concurrentmeet(Chameneo other, Color color, CancellationToken __cancellation, Action<object> __success, Action<Exception> __failure)
        {
            Colour = ColorUtils.Compliment(Colour, color);
            Meetings++;
            if (other == this)
                MeetingsWithSelf++;
            {
                __dispatch("meet");
                if (__success != null)
                    __success(null);
                yield break;
            }
        }

        public Task<object> meet(Chameneo other, Color color, CancellationToken cancellation)
        {
            var completion = new TaskCompletionSource<object>();
            Action<object> __success = (__res) => completion.SetResult((object)__res);
            Action<Exception> __failure = (__ex) => completion.SetException(__ex);
            var __cancellation = cancellation;
            __enter(() => __advance(__concurrentmeet(other, color, __cancellation, __success, __failure).GetEnumerator()), __failure);
            return completion.Task;
        }

        public void meet(Chameneo other, Color color, CancellationToken cancellation, Action<object> success, Action<Exception> failure)
        {
            var __success = success;
            var __failure = failure;
            var __cancellation = cancellation;
            __enter(() => __advance(__concurrentmeet(other, color, __cancellation, __success, __failure).GetEnumerator()), failure);
        }

        private class __expr1 : Expression
        {
            public void __op1(bool ? v1, bool ? v2, Exception __ex)
            {
                if (!tryUpdate(v1, v2, ref __op1_Left, ref __op1_Right, __ex))
                    return;
                if (v1.HasValue)
                {
                    if (__op1_Left.Value)
                        __complete(true, null);
                    else if (__op1_Right.HasValue)
                        __complete(false, __ex);
                }
                else
                {
                    if (__op1_Right.Value)
                        __complete(true, null);
                    else if (__op1_Left.HasValue)
                        __complete(false, __ex);
                }
            }

            private bool ? __op1_Left;
            private bool ? __op1_Right;
        }

        public readonly Guid __ID = Guid.NewGuid();
    }

    [Concurrent(id = "88f4f156-33c6-4f7e-baac-dc2afa4b4232")]
    public class Broker : ConcurrentObject
    {
        int _meetings = 0;
        public Broker(int meetings)
        {
            _meetings = meetings;
        }

        Chameneo _first = null;
        [Concurrent]
        public void request(Chameneo creature)
        {
            request(creature, default (CancellationToken), null, null);
        }

        void done()
        {
            __dispatch("done");
        }

        [Concurrent]
        public void Finished()
        {
            Finished(default (CancellationToken), null, null);
        }

        private IEnumerable<Expression> __concurrentrequest(Chameneo creature, CancellationToken __cancellation, Action<object> __success, Action<Exception> __failure)
        {
            if (_meetings == 0)
            {
                __dispatch("request");
                if (__success != null)
                    __success(null);
                yield break;
            }

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
                    done();
            }
            else
                _first = creature;
            {
                __dispatch("request");
                if (__success != null)
                    __success(null);
                yield break;
            }
        }

        public Task<object> request(Chameneo creature, CancellationToken cancellation)
        {
            var completion = new TaskCompletionSource<object>();
            Action<object> __success = (__res) => completion.SetResult((object)__res);
            Action<Exception> __failure = (__ex) => completion.SetException(__ex);
            var __cancellation = cancellation;
            __enter(() => __advance(__concurrentrequest(creature, __cancellation, __success, __failure).GetEnumerator()), __failure);
            return completion.Task;
        }

        public void request(Chameneo creature, CancellationToken cancellation, Action<object> success, Action<Exception> failure)
        {
            var __success = success;
            var __failure = failure;
            var __cancellation = cancellation;
            __enter(() => __advance(__concurrentrequest(creature, __cancellation, __success, __failure).GetEnumerator()), failure);
        }

        private IEnumerable<Expression> __concurrentdone(CancellationToken __cancellation, Action<object> __success, Action<Exception> __failure)
        {
            if (false && !__awaiting("done"))
                throw new InvalidOperationException("done" + " can not be executed in this state");
            __dispatch("done");
            if (__success != null)
                __success(null);
            yield break;
        }

        private IEnumerable<Expression> __concurrentFinished(CancellationToken __cancellation, Action<object> __success, Action<Exception> __failure)
        {
            {
                var __expr2_var = new __expr2{Start = (___expr) =>
                {
                    var __expr = (__expr2)___expr;
                    __listen("done", () =>
                    {
                        __expr.__op2(true, null, null);
                    }

                    );
                    __expr.__op2(null, false, null);
                }

                , End = (__expr) =>
                {
                    __enter(() => __advance(__expr.Continuator), __failure);
                }
                };
                yield return __expr2_var;
                if (__expr2_var.Failure != null)
                    throw __expr2_var.Failure;
            }

            {
                __dispatch("Finished");
                if (__success != null)
                    __success(null);
                yield break;
            }
        }

        public Task<object> Finished(CancellationToken cancellation)
        {
            var completion = new TaskCompletionSource<object>();
            Action<object> __success = (__res) => completion.SetResult((object)__res);
            Action<Exception> __failure = (__ex) => completion.SetException(__ex);
            var __cancellation = cancellation;
            __enter(() => __advance(__concurrentFinished(__cancellation, __success, __failure).GetEnumerator()), __failure);
            return completion.Task;
        }

        public void Finished(CancellationToken cancellation, Action<object> success, Action<Exception> failure)
        {
            var __success = success;
            var __failure = failure;
            var __cancellation = cancellation;
            __enter(() => __advance(__concurrentFinished(__cancellation, __success, __failure).GetEnumerator()), failure);
        }

        private class __expr2 : Expression
        {
            public void __op2(bool ? v1, bool ? v2, Exception __ex)
            {
                if (!tryUpdate(v1, v2, ref __op2_Left, ref __op2_Right, __ex))
                    return;
                if (v1.HasValue)
                {
                    if (__op2_Left.Value)
                        __complete(true, null);
                    else if (__op2_Right.HasValue)
                        __complete(false, __ex);
                }
                else
                {
                    if (__op2_Right.Value)
                        __complete(true, null);
                    else if (__op2_Left.HasValue)
                        __complete(false, __ex);
                }
            }

            private bool ? __op2_Left;
            private bool ? __op2_Right;
        }

        public readonly Guid __ID = Guid.NewGuid();
    }

    [Concurrent(id = "2e18eb22-afff-4425-93d6-64a09531fcee")]
    [ConcurrentSingleton(id: "288db633-5611-4660-8f93-243ce69a3d4f")]
    public class __app : ConcurrentObject
    {
        protected override void __started()
        {
            var __enum = __concurrentmain(default (CancellationToken), null, null);
            __enter(() => __advance(__enum.GetEnumerator()), null);
        }

        IEnumerable<Chameneo> Run(int meetings, Color[] colors)
        {
            throw new InvalidOperationException("Cannot call private signals directly");
        }

        void PrintColors()
        {
            printCompliment(Color.blue, Color.blue);
            printCompliment(Color.blue, Color.red);
            printCompliment(Color.blue, Color.yellow);
            printCompliment(Color.red, Color.blue);
            printCompliment(Color.red, Color.red);
            printCompliment(Color.red, Color.yellow);
            printCompliment(Color.yellow, Color.blue);
            printCompliment(Color.yellow, Color.red);
            printCompliment(Color.yellow, Color.yellow);
        }

        void PrintRun(Color[] colors, IEnumerable<Chameneo> creatures)
        {
            for (int i = 0; i < colors.Length; i++)
                Console.Write(" " + colors[i]);
            Console.WriteLine();
            var total = 0;
            foreach (var creature in creatures)
            {
                Console.WriteLine($"{creature.Meetings} {printNumber(creature.MeetingsWithSelf)}");
                total += creature.Meetings;
            }

            Console.WriteLine(printNumber(total));
        }

        void printCompliment(Color c1, Color c2)
        {
            Console.WriteLine(c1 + " + " + c2 + " -> " + ColorUtils.Compliment(c1, c2));
        }

        string[] NUMBERS = {"zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine"};
        string printNumber(int n)
        {
            StringBuilder sb = new StringBuilder();
            String nStr = n.ToString();
            for (int i = 0; i < nStr.Length; i++)
            {
                sb.Append(" ");
                sb.Append(NUMBERS[(int)Char.GetNumericValue(nStr[i])]);
            }

            return sb.ToString();
        }

        private IEnumerable<Expression> __concurrentmain(CancellationToken __cancellation, Action<object> __success, Action<Exception> __failure)
        {
            var meetings = 0;
            if (Arguments.Length != 1 || !int.TryParse(Arguments[0], out meetings))
            {
                meetings = 600;
            }

            var firstRunColors = new[]{Color.blue, Color.red, Color.yellow};
            var secondRunColors = new[]{Color.blue, Color.red, Color.yellow, Color.red, Color.yellow, Color.blue, Color.red, Color.yellow, Color.red, Color.blue};
            //run and await 
            IEnumerable<Chameneo> firstRun, secondRun;
            var __expr3_var = new __expr3{Start = (___expr) =>
            {
                var __expr = (__expr3)___expr;
                __advance((__concurrentRun(meetings, firstRunColors, __cancellation, (__res) =>
                {
                    __expr.firstRun = (System.Collections.Generic.IEnumerable<ChameneoRedux.Chameneo>)__res;
                    __expr.__op3(true, null, null);
                }

                , (__ex) =>
                {
                    __expr.__op3(false, null, __ex);
                }

                )).GetEnumerator());
                __advance((__concurrentRun(meetings, secondRunColors, __cancellation, (__res) =>
                {
                    __expr.secondRun = (System.Collections.Generic.IEnumerable<ChameneoRedux.Chameneo>)__res;
                    __expr.__op3(null, true, null);
                }

                , (__ex) =>
                {
                    __expr.__op3(null, false, __ex);
                }

                )).GetEnumerator());
            }

            , End = (__expr) =>
            {
                __enter(() => __advance(__expr.Continuator), __failure);
            }
            };
            yield return __expr3_var;
            if (__expr3_var.Failure != null)
                throw __expr3_var.Failure;
            firstRun = __expr3_var.firstRun;
            secondRun = __expr3_var.secondRun;
            //print results
            PrintColors();
            Console.WriteLine();
            PrintRun(firstRunColors, firstRun);
            Console.WriteLine();
            PrintRun(secondRunColors, secondRun);
            App.Stop();
            {
                __dispatch("main");
                if (__success != null)
                    __success(null);
                yield break;
            }
        }

        private IEnumerable<Expression> __concurrentRun(int meetings, Color[] colors, CancellationToken __cancellation, Action<object> __success, Action<Exception> __failure)
        {
            var id = 0;
            var broker = App.Spawn(new Broker(meetings));
            var result = colors.Select(color => App.Spawn(new Chameneo(broker, color))).ToArray();
            {
                var __expr4_var = new __expr4{Start = (___expr) =>
                {
                    var __expr = (__expr4)___expr;
                    broker.Finished(__cancellation, (__res) => __expr.__op4(true, null, null), (__ex) => __expr.__op4(false, null, __ex));
                    __expr.__op4(null, false, null);
                }

                , End = (__expr) =>
                {
                    __enter(() => __advance(__expr.Continuator), __failure);
                }
                };
                yield return __expr4_var;
                if (__expr4_var.Failure != null)
                    throw __expr4_var.Failure;
            }

            {
                __dispatch("Run");
                if (__success != null)
                    __success(result);
                yield break;
            }
        }

        private class __expr3 : Expression
        {
            public void __op3(bool ? v1, bool ? v2, Exception __ex)
            {
                if (!tryUpdate(v1, v2, ref __op3_Left, ref __op3_Right, __ex))
                    return;
                if (v1.HasValue)
                {
                    if (!__op3_Left.Value)
                        __complete(false, __ex);
                    else if (__op3_Right.HasValue)
                        __complete(true, null);
                }
                else
                {
                    if (!__op3_Right.Value)
                        __complete(false, __ex);
                    else if (__op3_Left.HasValue)
                        __complete(true, null);
                }
            }

            private bool ? __op3_Left;
            private bool ? __op3_Right;
            public System.Collections.Generic.IEnumerable<ChameneoRedux.Chameneo> firstRun;
            public System.Collections.Generic.IEnumerable<ChameneoRedux.Chameneo> secondRun;
        }

        private class __expr4 : Expression
        {
            public void __op4(bool ? v1, bool ? v2, Exception __ex)
            {
                if (!tryUpdate(v1, v2, ref __op4_Left, ref __op4_Right, __ex))
                {
                    Console.WriteLine("FAILED UPDATING");
                    return;
                }

                if (v1.HasValue)
                {
                    if (__op4_Left.Value)
                    {
                        Console.WriteLine("Complete 1");
                        __complete(true, null);
                    }
                    else if (__op4_Right.HasValue)
                    {
                        Console.WriteLine("Fail 1");
                        __complete(false, __ex);
                    }
                }
                else
                {
                    if (__op4_Right.Value)
                    {
                        Console.WriteLine("Complete 2");
                        __complete(true, null);
                    }
                    else if (__op4_Left.HasValue)
                    {
                        Console.WriteLine("Fail 2");
                        __complete(false, __ex);
                    }
                }
            }

            private bool ? __op4_Left;
            private bool ? __op4_Right;
        }

        public readonly Guid __ID = Guid.NewGuid();
        public static void Start(string[] args)
        {
            Arguments = args;
            var app = new ThreadedConcurrentApp(threadCount: 4, blockUntilNextEvent: false, priority: ThreadPriority.Highest);
            app.Start();
            app.Spawn(new __app());
            Await = () => app.AwaitCompletion();
            Stop = () => app.Stop();
        }

        public static string[] Arguments
        {
            get;
            set;
        }

        public static Action Stop
        {
            get;
            private set;
        }

        public static Action Await
        {
            get;
            private set;
        }
    }

    public class ColorUtils
    {
        public static Color Compliment(Color c1, Color c2)
        {
            switch (c1)
            {
                case Color.blue:
                    switch (c2)
                    {
                        case Color.blue:
                            return Color.blue;
                        case Color.red:
                            return Color.yellow;
                        case Color.yellow:
                            return Color.red;
                        default:
                            break;
                    }

                    break;
                case Color.red:
                    switch (c2)
                    {
                        case Color.blue:
                            return Color.yellow;
                        case Color.red:
                            return Color.red;
                        case Color.yellow:
                            return Color.blue;
                        default:
                            break;
                    }

                    break;
                case Color.yellow:
                    switch (c2)
                    {
                        case Color.blue:
                            return Color.red;
                        case Color.red:
                            return Color.blue;
                        case Color.yellow:
                            return Color.yellow;
                        default:
                            break;
                    }

                    break;
            }

            throw new Exception();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            __app.Start(args);
            __app.Await();
        }
    }
}