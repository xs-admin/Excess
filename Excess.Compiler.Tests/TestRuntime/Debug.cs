using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess.Compiler.Tests.TestRuntime
{
    class barbershop : ConcurrentObject
    {
        barber[] _barbers;
        bool[] _busy;
        barbershop()
        {
            _barbers = new[] { spawn<barber>(0), spawn<barber>(1) };
            _busy = new[] { false, false };
        }

        private IEnumerable<Expression> __concurrentvisit(int client, Action<object> __success, Action<Exception> __failure)
        {
            console.write("entered client: " + client);
            if (_busy[0] && _busy[1])
            {
                {
                    var __expr1_var = new __expr1
                    {
                        Start = (___expr) =>
                        {
                            var __expr = (__expr1)___expr;
                            __queuevisit.Enqueue(() => __expr.__op1(true, null, null));
                            __expr.__op1(null, false, null);
                        }

                    ,
                        End = (__expr) =>
                        {
                            __enter(() => __advance(__expr.Continuator), __failure);
                        }
                    };
                    yield return __expr1_var;
                    if (__expr1_var.Failure != null)
                        throw __expr1_var.Failure;
                }
            }

            for (int i = 0; i < 2; i++)
            {
                if (!_busy[i])
                {
                    {
                        var __expr2_var = new __expr2
                        {
                            Start = (___expr) =>
                            {
                                var __expr = (__expr2)___expr;
                                __advance((__concurrentshave_client(client, i, (__res) =>
                                {
                                    __expr.__op2(true, null, null);
                                }

                                , (__ex) =>
                                {
                                    __expr.__op2(false, null, __ex);
                                }

                                )).GetEnumerator());
                                __expr.__op2(null, false, null);
                            }

                        ,
                            End = (__expr) =>
                            {
                                __enter(() => __advance(__expr.Continuator), __failure);
                            }
                        };
                        yield return __expr2_var;
                        if (__expr2_var.Failure != null)
                            throw __expr2_var.Failure;
                    }

                    break;
                }
            }

            if (__queuevisit.Any())
                __queuevisit.Dequeue()();
            {
                __dispatch("visit");
                if (__success != null)
                    __success(null);
                yield break;
            }
        }

        public Task<object> visit(int client, bool async)
        {
            if (!async)
                throw new InvalidOperationException("use async: true");
            var completion = new TaskCompletionSource<object>();
            Action<object> __success = (__res) => completion.SetResult((object)__res);
            Action<Exception> __failure = (__ex) => completion.SetException(__ex);
            __enter(() => __advance(__concurrentvisit(client, __success, __failure).GetEnumerator()), __failure);
            return completion.Task;
        }

        public void visit(int client, Action<object> success = null, Action<Exception> failure = null)
        {
            var __success = success;
            var __failure = failure;
            __enter(() => __advance(__concurrentvisit(client, __success, __failure).GetEnumerator()), failure);
        }

        private IEnumerable<Expression> __concurrentshave_client(int client, int which, Action<object> __success, Action<Exception> __failure)
        {
            _busy[which] = true;
            var barber = _barbers[which];
            double tip = rand(5, 10);
            {
                var __try1 = false;
                try
                {
                }
                finally
                {
                    _busy[which] = false;
                }

                if (!__try1)
                {
                    var __expr3_var = new __expr3
                    {
                        Start = (___expr) =>
                        {
                            var __expr = (__expr3)___expr;
                            barber.shave(client, (__res) => __expr.__op3(true, null, null), (__ex) => __expr.__op3(false, null, __ex));
                        }

                    ,
                        End = (__expr) =>
                        {
                            __enter(() => __advance(__expr.Continuator), __failure);
                        }

                    ,
                        __start1 = (___expr) =>
                        {
                            var __expr = (__expr3)___expr;
                            __enter(() =>
                            {
                                barber.tip(client, tip, (__res) => __expr.__op3(null, true, null), (__ex) => __expr.__op3(null, false, __ex));
                            }

                            , __failure);
                        }
                    };
                    yield return __expr3_var;
                }
            }

            {
                __dispatch("shave_client");
                if (__success != null)
                    __success(null);
                yield break;
            }
        }

        private class __expr1 : Expression
        {
            public void __op1(bool? v1, bool? v2, Exception __ex)
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

            private bool? __op1_Left;
            private bool? __op1_Right;
        }

        private class __expr2 : Expression
        {
            public void __op2(bool? v1, bool? v2, Exception __ex)
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

            private bool? __op2_Left;
            private bool? __op2_Right;
        }

        private class __expr3 : Expression
        {
            public void __op3(bool? v1, bool? v2, Exception __ex)
            {
                if (!tryUpdate(v1, v2, ref __op3_Left, ref __op3_Right, __ex))
                    return;
                if (v1.HasValue)
                {
                    if (__op3_Left.Value)
                        __start1(this);
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

            private bool? __op3_Left;
            private bool? __op3_Right;
            public Action<__expr3> __start1;
        }

        Queue<Action> __queuevisit = new Queue<Action>();
    }

    class barber : ConcurrentObject
    {
        int _index;
        double _tip = 0;
        private IEnumerable<Expression> __concurrentmain(int index, Action<object> __success, Action<Exception> __failure)
        {
            _index = index;
            while (true)
            {
                var __expr4_var = new __expr4
                {
                    Start = (___expr) =>
                    {
                        var __expr = (__expr4)___expr;
                        __listen("shave", () =>
                        {
                            __expr.__op4(true, null, null);
                        }

                        );
                    }

                ,
                    End = (__expr) =>
                    {
                        __enter(() => __advance(__expr.Continuator), __failure);
                    }

                ,
                    __start2 = (___expr) =>
                    {
                        var __expr = (__expr4)___expr;
                        __enter(() =>
                        {
                            __listen("tip", () =>
                            {
                                __expr.__op4(null, true, null);
                            }

                            );
                        }

                        , __failure);
                    }
                };
                yield return __expr4_var;
                if (__expr4_var.Failure != null)
                    throw __expr4_var.Failure;
            }

            {
                __dispatch("main");
                if (__success != null)
                    __success(null);
                yield break;
            }
        }

        protected override void __start(params object[] args)
        {
            var __enum = __concurrentmain((int)args[0], null, null);
            __advance(__enum.GetEnumerator());
        }

        private IEnumerable<Expression> __concurrentshave(int client, Action<object> __success, Action<Exception> __failure)
        {
            {
                var __expr5_var = new __expr5
                {
                    Start = (___expr) =>
                    {
                        var __expr = (__expr5)___expr;
                        Task.Delay((int)((rand(1, 2)) * 1000)).ContinueWith(__task =>
                        {
                            __enter(() => __expr.__op5(true, null, null), (__ex) => __expr.__op5(false, null, __ex));
                        }

                        );
                        __expr.__op5(null, false, null);
                    }

                ,
                    End = (__expr) =>
                    {
                        __enter(() => __advance(__expr.Continuator), __failure);
                    }
                };
                yield return __expr5_var;
                if (__expr5_var.Failure != null)
                    throw __expr5_var.Failure;
            }

            {
                __dispatch("shave");
                if (__success != null)
                    __success(null);
                yield break;
            }
        }

        public Task<object> shave(int client, bool async)
        {
            if (!async)
                throw new InvalidOperationException("use async: true");
            var completion = new TaskCompletionSource<object>();
            Action<object> __success = (__res) => completion.SetResult((object)__res);
            Action<Exception> __failure = (__ex) => completion.SetException(__ex);
            __enter(() => __advance(__concurrentshave(client, __success, __failure).GetEnumerator()), __failure);
            return completion.Task;
        }

        public void shave(int client, Action<object> success = null, Action<Exception> failure = null)
        {
            var __success = success;
            var __failure = failure;
            __enter(() => __advance(__concurrentshave(client, __success, __failure).GetEnumerator()), failure);
        }

        private IEnumerable<Expression> __concurrenttip(int client, double amount, Action<object> __success, Action<Exception> __failure)
        {
            _tip += amount;
            console.write($"Barber {_index}: {client} tipped {amount:  C2}, for a total of {_tip: C2}");
            {
                __dispatch("tip");
                if (__success != null)
                    __success(null);
                yield break;
            }
        }

        public Task<object> tip(int client, double amount, bool async)
        {
            if (!async)
                throw new InvalidOperationException("use async: true");
            var completion = new TaskCompletionSource<object>();
            Action<object> __success = (__res) => completion.SetResult((object)__res);
            Action<Exception> __failure = (__ex) => completion.SetException(__ex);
            __enter(() => __advance(__concurrenttip(client, amount, __success, __failure).GetEnumerator()), __failure);
            return completion.Task;
        }

        public void tip(int client, double amount, Action<object> success = null, Action<Exception> failure = null)
        {
            var __success = success;
            var __failure = failure;
            __enter(() => __advance(__concurrenttip(client, amount, __success, __failure).GetEnumerator()), failure);
        }

        private class __expr4 : Expression
        {
            public void __op4(bool? v1, bool? v2, Exception __ex)
            {
                if (!tryUpdate(v1, v2, ref __op4_Left, ref __op4_Right, __ex))
                    return;
                if (v1.HasValue)
                {
                    if (__op4_Left.Value)
                        __start2(this);
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

            private bool? __op4_Left;
            private bool? __op4_Right;
            public Action<__expr4> __start2;
        }

        private class __expr5 : Expression
        {
            public void __op5(bool? v1, bool? v2, Exception __ex)
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

            private bool? __op5_Left;
            private bool? __op5_Right;
        }
    }
}
