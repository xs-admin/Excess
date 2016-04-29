using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Excess.Concurrent.Runtime;

namespace Barbers
{
    [Concurrent(id = "5b2caa9d-a05f-427c-8b65-b0b3a5964ff4")]
    [ConcurrentSingleton(id: "97c6148a-94b1-4bef-b50a-c7058bd43903")]
    public class __app : ConcurrentObject
    {
        protected override void __started()
        {
            var __enum = __concurrentmain(default (CancellationToken), null, null);
            __enter(() => __advance(__enum.GetEnumerator()), null);
        }

        private IEnumerable<Expression> __concurrentmain(CancellationToken __cancellation, Action<object> __success, Action<Exception> __failure)
        {
            var clients = 0;
            if (Arguments.Length < 1 || !int.TryParse(Arguments[0], out clients))
            {
                clients = 30;
            }

            var chairs = 0;
            if (Arguments.Length < 2 || !int.TryParse(Arguments[1], out chairs))
            {
                chairs = 2;
            }

            var barber1 = spawn<Barber>(0);
            var barber2 = spawn<Barber>(1);
            var shop = spawn<Barbershop>(barber1, barber2, clients, chairs);
            for (int i = 1; i <= clients; i++)
            {
                {
                    var __expr1_var = new __expr1{Start = (___expr) =>
                    {
                        var __expr = (__expr1)___expr;
                        Task.Delay((int)((rand(0, 1)) * 1000)).ContinueWith(__task =>
                        {
                            __enter(() => __expr.__op1(true, null, null), (__ex) => __expr.__op1(false, null, __ex));
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

                shop.visit(i);
            }

            {
                __dispatch("main");
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

    [Concurrent(id = "72fd2b5d-a056-4a02-938b-ab186957c43a")]
    class Barbershop : ConcurrentObject
    {
        Barber[] _barbers;
        bool[] _busy;
        int _clients;
        int _chairs;
        public Barbershop(Barber barber1, Barber barber2, int clients, int chairs)
        {
            _barbers = new[]{barber1, barber2};
            _busy = new[]{false, false};
            _clients = clients;
            _chairs = chairs;
        }

        [Concurrent]
        public void visit(int client)
        {
            visit(client, default (CancellationToken), null, null);
        }

        private void shave_client(int client, int which)
        {
            throw new InvalidOperationException("Cannot call private signals directly");
        }

        private string barber_status(int which)
        {
            return _busy[which] ? "working" : "available";
        }

        private IEnumerable<Expression> __concurrentvisit(int client, CancellationToken __cancellation, Action<object> __success, Action<Exception> __failure)
        {
            if (_chairs == 0)
            {
                Console.WriteLine($"Client {client}: the place is full!");
            }
            else
            {
                Console.WriteLine($"Client: {client}, Barber1 {barber_status(0)},  Barber2: {barber_status(1)}");
                if (_busy[0] && _busy[1])
                {
                    _chairs--;
                    {
                        var __expr2_var = new __expr2{Start = (___expr) =>
                        {
                            var __expr = (__expr2)___expr;
                            __queuevisit.Enqueue(() => __expr.__op2(true, null, null));
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

                    _chairs++;
                }

                for (int i = 0; i < 2; i++)
                {
                    if (!_busy[i])
                    {
                        {
                            var __expr3_var = new __expr3{Start = (___expr) =>
                            {
                                var __expr = (__expr3)___expr;
                                __advance((__concurrentshave_client(client, i, __cancellation, (__res) =>
                                {
                                    __expr.__op3(true, null, null);
                                }

                                , (__ex) =>
                                {
                                    __expr.__op3(false, null, __ex);
                                }

                                )).GetEnumerator());
                                __expr.__op3(null, false, null);
                            }

                            , End = (__expr) =>
                            {
                                __enter(() => __advance(__expr.Continuator), __failure);
                            }
                            };
                            yield return __expr3_var;
                            if (__expr3_var.Failure != null)
                                throw __expr3_var.Failure;
                        }

                        break;
                    }
                }

                if (__queuevisit.Any())
                    __queuevisit.Dequeue()();
            }

            _clients--;
            if (_clients == 0)
            {
                App.Stop();
            }

            {
                __dispatch("visit");
                if (__success != null)
                    __success(null);
                yield break;
            }
        }

        public Task<object> visit(int client, CancellationToken cancellation)
        {
            var completion = new TaskCompletionSource<object>();
            Action<object> __success = (__res) => completion.SetResult((object)__res);
            Action<Exception> __failure = (__ex) => completion.SetException(__ex);
            var __cancellation = cancellation;
            __enter(() => __advance(__concurrentvisit(client, __cancellation, __success, __failure).GetEnumerator()), __failure);
            return completion.Task;
        }

        public void visit(int client, CancellationToken cancellation, Action<object> success, Action<Exception> failure)
        {
            var __success = success;
            var __failure = failure;
            var __cancellation = cancellation;
            __enter(() => __advance(__concurrentvisit(client, __cancellation, __success, __failure).GetEnumerator()), failure);
        }

        private IEnumerable<Expression> __concurrentshave_client(int client, int which, CancellationToken __cancellation, Action<object> __success, Action<Exception> __failure)
        {
            var barber = _barbers[which];
            double tip = rand(5, 10);
            _busy[which] = true;
            var __expr4_var = new __expr4{Start = (___expr) =>
            {
                var __expr = (__expr4)___expr;
                barber.shave(client, __cancellation, (__res) => __expr.__op4(true, null, null), (__ex) => __expr.__op4(false, null, __ex));
            }

            , End = (__expr) =>
            {
                __enter(() => __advance(__expr.Continuator), __failure);
            }

            , __start1 = (___expr) =>
            {
                var __expr = (__expr4)___expr;
                __enter(() =>
                {
                    barber.tip(client, tip, __cancellation, (__res) => __expr.__op4(null, true, null), (__ex) => __expr.__op4(null, false, __ex));
                }

                , __failure);
            }
            };
            yield return __expr4_var;
            if (__expr4_var.Failure != null)
                throw __expr4_var.Failure;
            _busy[which] = false;
            {
                __dispatch("shave_client");
                if (__success != null)
                    __success(null);
                yield break;
            }
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

        private class __expr3 : Expression
        {
            public void __op3(bool ? v1, bool ? v2, Exception __ex)
            {
                if (!tryUpdate(v1, v2, ref __op3_Left, ref __op3_Right, __ex))
                    return;
                if (v1.HasValue)
                {
                    if (__op3_Left.Value)
                        __complete(true, null);
                    else if (__op3_Right.HasValue)
                        __complete(false, __ex);
                }
                else
                {
                    if (__op3_Right.Value)
                        __complete(true, null);
                    else if (__op3_Left.HasValue)
                        __complete(false, __ex);
                }
            }

            private bool ? __op3_Left;
            private bool ? __op3_Right;
        }

        private class __expr4 : Expression
        {
            public void __op4(bool ? v1, bool ? v2, Exception __ex)
            {
                if (!tryUpdate(v1, v2, ref __op4_Left, ref __op4_Right, __ex))
                    return;
                if (v1.HasValue)
                {
                    if (__op4_Left.Value)
                        __start1(this);
                    else
                        __complete(false, __ex);
                }
                else
                {
                    if (__op4_Right.Value)
                        __complete(true, null);
                    else
                        __complete(false, __ex);
                }
            }

            private bool ? __op4_Left;
            private bool ? __op4_Right;
            public Action<__expr4> __start1;
        }

        public readonly Guid __ID = Guid.NewGuid();
        private Queue<Action> __queuevisit = new Queue<Action>();
    }

    [Concurrent(id = "a0c471ce-8b09-449e-87c7-1436774bb412")]
    class Barber : ConcurrentObject
    {
        int _index;
        public Barber(int index)
        {
            _index = index;
        }

        protected override void __started()
        {
            var __enum = __concurrentmain(default (CancellationToken), null, null);
            __enter(() => __advance(__enum.GetEnumerator()), null);
        }

        [Concurrent]
        public void shave(int client)
        {
            shave(client, default (CancellationToken), null, null);
        }

        double _tip = 0;
        [Concurrent]
        public void tip(int client, double amount)
        {
            tip(client, amount, default (CancellationToken), null, null);
        }

        private IEnumerable<Expression> __concurrentmain(CancellationToken __cancellation, Action<object> __success, Action<Exception> __failure)
        {
            for (;;)
            {
                var __expr5_var = new __expr5{Start = (___expr) =>
                {
                    var __expr = (__expr5)___expr;
                    __listen("shave", () =>
                    {
                        __expr.__op5(true, null, null);
                    }

                    );
                }

                , End = (__expr) =>
                {
                    __enter(() => __advance(__expr.Continuator), __failure);
                }

                , __start2 = (___expr) =>
                {
                    var __expr = (__expr5)___expr;
                    __enter(() =>
                    {
                        __listen("tip", () =>
                        {
                            __expr.__op5(null, true, null);
                        }

                        );
                    }

                    , __failure);
                }
                };
                yield return __expr5_var;
                if (__expr5_var.Failure != null)
                    throw __expr5_var.Failure;
            }

            {
                __dispatch("main");
                if (__success != null)
                    __success(null);
                yield break;
            }
        }

        private IEnumerable<Expression> __concurrentshave(int client, CancellationToken __cancellation, Action<object> __success, Action<Exception> __failure)
        {
            {
                var __expr6_var = new __expr6{Start = (___expr) =>
                {
                    var __expr = (__expr6)___expr;
                    Task.Delay((int)((rand(1, 2)) * 1000)).ContinueWith(__task =>
                    {
                        __enter(() => __expr.__op6(true, null, null), (__ex) => __expr.__op6(false, null, __ex));
                    }

                    );
                    __expr.__op6(null, false, null);
                }

                , End = (__expr) =>
                {
                    __enter(() => __advance(__expr.Continuator), __failure);
                }
                };
                yield return __expr6_var;
                if (__expr6_var.Failure != null)
                    throw __expr6_var.Failure;
            }

            {
                __dispatch("shave");
                if (__success != null)
                    __success(null);
                yield break;
            }
        }

        public Task<object> shave(int client, CancellationToken cancellation)
        {
            var completion = new TaskCompletionSource<object>();
            Action<object> __success = (__res) => completion.SetResult((object)__res);
            Action<Exception> __failure = (__ex) => completion.SetException(__ex);
            var __cancellation = cancellation;
            __enter(() => __advance(__concurrentshave(client, __cancellation, __success, __failure).GetEnumerator()), __failure);
            return completion.Task;
        }

        public void shave(int client, CancellationToken cancellation, Action<object> success, Action<Exception> failure)
        {
            var __success = success;
            var __failure = failure;
            var __cancellation = cancellation;
            __enter(() => __advance(__concurrentshave(client, __cancellation, __success, __failure).GetEnumerator()), failure);
        }

        private IEnumerable<Expression> __concurrenttip(int client, double amount, CancellationToken __cancellation, Action<object> __success, Action<Exception> __failure)
        {
            _tip += amount;
            Console.WriteLine($"Barber {_index}: {client} tipped {amount.ToString("C2")}, for a total of {_tip.ToString("C2")}");
            {
                __dispatch("tip");
                if (__success != null)
                    __success(null);
                yield break;
            }
        }

        public Task<object> tip(int client, double amount, CancellationToken cancellation)
        {
            var completion = new TaskCompletionSource<object>();
            Action<object> __success = (__res) => completion.SetResult((object)__res);
            Action<Exception> __failure = (__ex) => completion.SetException(__ex);
            var __cancellation = cancellation;
            __enter(() => __advance(__concurrenttip(client, amount, __cancellation, __success, __failure).GetEnumerator()), __failure);
            return completion.Task;
        }

        public void tip(int client, double amount, CancellationToken cancellation, Action<object> success, Action<Exception> failure)
        {
            var __success = success;
            var __failure = failure;
            var __cancellation = cancellation;
            __enter(() => __advance(__concurrenttip(client, amount, __cancellation, __success, __failure).GetEnumerator()), failure);
        }

        private class __expr5 : Expression
        {
            public void __op5(bool ? v1, bool ? v2, Exception __ex)
            {
                if (!tryUpdate(v1, v2, ref __op5_Left, ref __op5_Right, __ex))
                    return;
                if (v1.HasValue)
                {
                    if (__op5_Left.Value)
                        __start2(this);
                    else
                        __complete(false, __ex);
                }
                else
                {
                    if (__op5_Right.Value)
                        __complete(true, null);
                    else
                        __complete(false, __ex);
                }
            }

            private bool ? __op5_Left;
            private bool ? __op5_Right;
            public Action<__expr5> __start2;
        }

        private class __expr6 : Expression
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