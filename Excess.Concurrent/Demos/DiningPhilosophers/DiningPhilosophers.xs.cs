using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Excess.Concurrent.Runtime;

namespace DiningPhilosophers
{
    [Concurrent(id = "0b0cb53d-54d9-4014-b69e-c337dad28649")]
    [ConcurrentSingleton(id: "5d1dcbd1-a861-4392-b8e4-0a5f4d300951")]
    public class __app : ConcurrentObject
    {
        protected override void __started()
        {
            var __enum = __concurrentmain(default (CancellationToken), null, null);
            __enter(() => __advance(__enum.GetEnumerator()), null);
        }

        private IEnumerable<Expression> __concurrentmain(CancellationToken __cancellation, Action<object> __success, Action<Exception> __failure)
        {
            var names = new[]{"Kant", "Archimedes", "Nietzche", "Plato", "Spinoza", };
            var meals = 0;
            if (Arguments.Length != 1 || !int.TryParse(Arguments[0], out meals))
                meals = 3;
            //create chopsticks
            var chopsticks = names.Select(n => spawn<chopstick>()).ToArray();
            //create philosophers
            var phCount = names.Length;
            var stopper = new StopCount(phCount);
            for (int i = 0; i < phCount; i++)
            {
                var left = chopsticks[i];
                var right = i == phCount - 1 ? chopsticks[0] : chopsticks[i + 1];
                spawn<philosopher>(names[i], left, right, meals, stopper);
            }

            {
                __dispatch("main");
                if (__success != null)
                    __success(null);
                yield break;
            }
        }

        private static __app __singleton;
        public static void Start(IConcurrentApp app)
        {
            __singleton = app.Spawn<__app>();
        }

        public readonly Guid __ID = Guid.NewGuid();
        public static void Start(string[] args)
        {
            Arguments = args;
            var app = new ThreadedConcurrentApp(threadCount: 4, blockUntilNextEvent: true, priority: ThreadPriority.Normal);
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

    [Concurrent(id = "8f75b5fa-639c-4862-8912-0126480630c6")]
    class philosopher : ConcurrentObject
    {
        string _name;
        chopstick _left;
        chopstick _right;
        int _meals;
        StopCount _stop;
        public philosopher(string name, chopstick left, chopstick right, int meals, StopCount stop)
        {
            _name = name;
            _left = left;
            _right = right;
            _meals = meals;
            _stop = stop;
        }

        protected override void __started()
        {
            var __enum = __concurrentmain(default (CancellationToken), null, null);
            __enter(() => __advance(__enum.GetEnumerator()), null);
        }

        void think()
        {
            throw new InvalidOperationException("Cannot call private signals directly");
        }

        void hungry()
        {
            throw new InvalidOperationException("Cannot call private signals directly");
        }

        void eat()
        {
            throw new InvalidOperationException("Cannot call private signals directly");
        }

        private IEnumerable<Expression> __concurrentmain(CancellationToken __cancellation, Action<object> __success, Action<Exception> __failure)
        {
            for (int i = 0; i < _meals; i++)
            {
                {
                    var __expr1_var = new __expr1{Start = (___expr) =>
                    {
                        var __expr = (__expr1)___expr;
                        __advance((__concurrentthink(__cancellation, (__res) =>
                        {
                            __expr.__op1(true, null, null);
                        }

                        , (__ex) =>
                        {
                            __expr.__op1(false, null, __ex);
                        }

                        )).GetEnumerator());
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

            if (_stop.ShouldStop())
            {
                App.Stop();
            }

            {
                __dispatch("main");
                if (__success != null)
                    __success(null);
                yield break;
            }
        }

        private IEnumerable<Expression> __concurrentthink(CancellationToken __cancellation, Action<object> __success, Action<Exception> __failure)
        {
            Console.WriteLine(_name + " is thinking");
            var __expr2_var = new __expr2{Start = (___expr) =>
            {
                var __expr = (__expr2)___expr;
                Task.Delay((int)((rand(1.0, 2.0)) * 1000)).ContinueWith(__task =>
                {
                    __enter(() => __expr.__op2(true, null, null), (__ex) => __expr.__op2(false, null, __ex));
                }

                );
            }

            , End = (__expr) =>
            {
                __enter(() => __advance(__expr.Continuator), __failure);
            }

            , __start1 = (___expr) =>
            {
                var __expr = (__expr2)___expr;
                __enter(() =>
                {
                    __advance((__concurrenthungry(__cancellation, (__res) =>
                    {
                        __expr.__op2(null, true, null);
                    }

                    , (__ex) =>
                    {
                        __expr.__op2(null, false, __ex);
                    }

                    )).GetEnumerator());
                }

                , __failure);
            }
            };
            yield return __expr2_var;
            if (__expr2_var.Failure != null)
                throw __expr2_var.Failure;
            {
                __dispatch("think");
                if (__success != null)
                    __success(null);
                yield break;
            }
        }

        private IEnumerable<Expression> __concurrenthungry(CancellationToken __cancellation, Action<object> __success, Action<Exception> __failure)
        {
            Console.WriteLine(_name + " is hungry");
            var __expr3_var = new __expr3{Start = (___expr) =>
            {
                var __expr = (__expr3)___expr;
                _left.acquire(this, __cancellation, (__res) => __expr.__op4(true, null, null), (__ex) => __expr.__op4(false, null, __ex));
                _right.acquire(this, __cancellation, (__res) => __expr.__op4(null, true, null), (__ex) => __expr.__op4(null, false, __ex));
            }

            , End = (__expr) =>
            {
                __enter(() => __advance(__expr.Continuator), __failure);
            }

            , __start2 = (___expr) =>
            {
                var __expr = (__expr3)___expr;
                __enter(() =>
                {
                    __advance((__concurrenteat(__cancellation, (__res) =>
                    {
                        __expr.__op3(null, true, null);
                    }

                    , (__ex) =>
                    {
                        __expr.__op3(null, false, __ex);
                    }

                    )).GetEnumerator());
                }

                , __failure);
            }
            };
            yield return __expr3_var;
            if (__expr3_var.Failure != null)
                throw __expr3_var.Failure;
            {
                __dispatch("hungry");
                if (__success != null)
                    __success(null);
                yield break;
            }
        }

        private IEnumerable<Expression> __concurrenteat(CancellationToken __cancellation, Action<object> __success, Action<Exception> __failure)
        {
            Console.WriteLine(_name + " is eating");
            {
                var __expr4_var = new __expr4{Start = (___expr) =>
                {
                    var __expr = (__expr4)___expr;
                    Task.Delay((int)((rand(1.0, 2.0)) * 1000)).ContinueWith(__task =>
                    {
                        __enter(() => __expr.__op5(true, null, null), (__ex) => __expr.__op5(false, null, __ex));
                    }

                    );
                    __expr.__op5(null, false, null);
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

            _left.release(this);
            _right.release(this);
            {
                __dispatch("eat");
                if (__success != null)
                    __success(null);
                yield break;
            }
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

        private class __expr2 : Expression
        {
            public void __op2(bool ? v1, bool ? v2, Exception __ex)
            {
                if (!tryUpdate(v1, v2, ref __op2_Left, ref __op2_Right, __ex))
                    return;
                if (v1.HasValue)
                {
                    if (__op2_Left.Value)
                        __start1(this);
                    else
                        __complete(false, __ex);
                }
                else
                {
                    if (__op2_Right.Value)
                        __complete(true, null);
                    else
                        __complete(false, __ex);
                }
            }

            private bool ? __op2_Left;
            private bool ? __op2_Right;
            public Action<__expr2> __start1;
        }

        private class __expr3 : Expression
        {
            public void __op3(bool ? v1, bool ? v2, Exception __ex)
            {
                if (!tryUpdate(v1, v2, ref __op3_Left, ref __op3_Right, __ex))
                    return;
                if (v1.HasValue)
                {
                    if (__op3_Left.Value)
                        __start2(this);
                    else
                        __complete(false, __ex);
                }
                else
                {
                    if (__op3_Right.Value)
                        __complete(true, null);
                    else
                        __complete(false, __ex);
                }
            }

            private bool ? __op3_Left;
            private bool ? __op3_Right;
            public Action<__expr3> __start2;
            public void __op4(bool ? v1, bool ? v2, Exception __ex)
            {
                if (!tryUpdate(v1, v2, ref __op4_Left, ref __op4_Right, __ex))
                    return;
                if (v1.HasValue)
                {
                    if (!__op4_Left.Value)
                        __op3(false, null, __ex);
                    else if (__op4_Right.HasValue)
                        __op3(true, null, null);
                }
                else
                {
                    if (!__op4_Right.Value)
                        __op3(false, null, __ex);
                    else if (__op4_Left.HasValue)
                        __op3(true, null, null);
                }
            }

            private bool ? __op4_Left;
            private bool ? __op4_Right;
        }

        private class __expr4 : Expression
        {
            public void __op5(bool ? v1, bool ? v2, Exception __ex)
            {
                if (!tryUpdate(v1, v2, ref __op5_Left, ref __op5_Right, __ex))
                    return;
                if (v1.HasValue)
                {
                    if (__op5_Left.Value)
                        __complete(true, null);
                    else if (__op5_Right.HasValue)
                        __complete(false, __ex);
                }
                else
                {
                    if (__op5_Right.Value)
                        __complete(true, null);
                    else if (__op5_Left.HasValue)
                        __complete(false, __ex);
                }
            }

            private bool ? __op5_Left;
            private bool ? __op5_Right;
        }

        public readonly Guid __ID = Guid.NewGuid();
    }

    [Concurrent(id = "18e234d9-d4b3-49b6-80a5-19a21d9362be")]
    class chopstick : ConcurrentObject
    {
        philosopher _owner;
        [Concurrent]
        public void acquire(philosopher owner)
        {
            acquire(owner, default (CancellationToken), null, null);
        }

        [Concurrent]
        public void release(philosopher owner)
        {
            release(owner, default (CancellationToken), null, null);
        }

        private IEnumerable<Expression> __concurrentacquire(philosopher owner, CancellationToken __cancellation, Action<object> __success, Action<Exception> __failure)
        {
            if (_owner != null)
            {
                {
                    var __expr5_var = new __expr5{Start = (___expr) =>
                    {
                        var __expr = (__expr5)___expr;
                        __listen("release", () =>
                        {
                            __expr.__op6(true, null, null);
                        }

                        );
                        __expr.__op6(null, false, null);
                    }

                    , End = (__expr) =>
                    {
                        __enter(() => __advance(__expr.Continuator), __failure);
                    }
                    };
                    yield return __expr5_var;
                    if (__expr5_var.Failure != null)
                        throw __expr5_var.Failure;
                }
            }

            _owner = owner;
            {
                __dispatch("acquire");
                if (__success != null)
                    __success(null);
                yield break;
            }
        }

        public Task<object> acquire(philosopher owner, CancellationToken cancellation)
        {
            var completion = new TaskCompletionSource<object>();
            Action<object> __success = (__res) => completion.SetResult((object)__res);
            Action<Exception> __failure = (__ex) => completion.SetException(__ex);
            var __cancellation = cancellation;
            __enter(() => __advance(__concurrentacquire(owner, __cancellation, __success, __failure).GetEnumerator()), __failure);
            return completion.Task;
        }

        public void acquire(philosopher owner, CancellationToken cancellation, Action<object> success, Action<Exception> failure)
        {
            var __success = success;
            var __failure = failure;
            var __cancellation = cancellation;
            __enter(() => __advance(__concurrentacquire(owner, __cancellation, __success, __failure).GetEnumerator()), failure);
        }

        private IEnumerable<Expression> __concurrentrelease(philosopher owner, CancellationToken __cancellation, Action<object> __success, Action<Exception> __failure)
        {
            if (_owner != owner)
                throw new ArgumentException();
            _owner = null;
            {
                __dispatch("release");
                if (__success != null)
                    __success(null);
                yield break;
            }
        }

        public Task<object> release(philosopher owner, CancellationToken cancellation)
        {
            var completion = new TaskCompletionSource<object>();
            Action<object> __success = (__res) => completion.SetResult((object)__res);
            Action<Exception> __failure = (__ex) => completion.SetException(__ex);
            var __cancellation = cancellation;
            __enter(() => __advance(__concurrentrelease(owner, __cancellation, __success, __failure).GetEnumerator()), __failure);
            return completion.Task;
        }

        public void release(philosopher owner, CancellationToken cancellation, Action<object> success, Action<Exception> failure)
        {
            var __success = success;
            var __failure = failure;
            var __cancellation = cancellation;
            __enter(() => __advance(__concurrentrelease(owner, __cancellation, __success, __failure).GetEnumerator()), failure);
        }

        private class __expr5 : Expression
        {
            public void __op6(bool ? v1, bool ? v2, Exception __ex)
            {
                if (!tryUpdate(v1, v2, ref __op6_Left, ref __op6_Right, __ex))
                    return;
                if (v1.HasValue)
                {
                    if (__op6_Left.Value)
                        __complete(true, null);
                    else if (__op6_Right.HasValue)
                        __complete(false, __ex);
                }
                else
                {
                    if (__op6_Right.Value)
                        __complete(true, null);
                    else if (__op6_Left.HasValue)
                        __complete(false, __ex);
                }
            }

            private bool ? __op6_Left;
            private bool ? __op6_Right;
        }

        public readonly Guid __ID = Guid.NewGuid();
    }

    //temporary class to abort the app when all meals has been served
    //will dissapear soon.
    class StopCount
    {
        int _count;
        public StopCount(int count)
        {
            _count = count;
        }

        public bool ShouldStop() => --_count == 0;
    }

    class Program
    {
        static void Main(string[] args)
        {
            __app.Start(args);
            {
            }

            __app.Await();
        }
    }
}