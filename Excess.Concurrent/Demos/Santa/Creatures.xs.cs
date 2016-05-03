using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Excess.Concurrent.Runtime;

namespace Santa
{
    [Concurrent(id = "70380619-c3a8-46d1-bca4-bc5cb279adad")]
    class Reindeer : ConcurrentObject
    {
        string _name;
        SantaClaus _santa;
        public Reindeer(string name, SantaClaus santa)
        {
            _name = name;
            _santa = santa;
        }

        protected override void __started()
        {
            var __enum = __concurrentmain(default (CancellationToken), null, null);
            __enter(() => __advance(__enum.GetEnumerator()), null);
        }

        [Concurrent]
        public void unharness()
        {
            unharness(default (CancellationToken), null, null);
        }

        private void vacation()
        {
            throw new InvalidOperationException("Cannot call private signals directly");
        }

        private IEnumerable<Expression> __concurrentmain(CancellationToken __cancellation, Action<object> __success, Action<Exception> __failure)
        {
            for (;;)
            {
                {
                    var __expr1_var = new __expr1{Start = (___expr) =>
                    {
                        var __expr = (__expr1)___expr;
                        __advance((__concurrentvacation(__cancellation, (__res) =>
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

            {
                __dispatch("main");
                if (__success != null)
                    __success(null);
                yield break;
            }
        }

        private IEnumerable<Expression> __concurrentunharness(CancellationToken __cancellation, Action<object> __success, Action<Exception> __failure)
        {
            Console.WriteLine(_name + ": job well done");
            {
                __dispatch("unharness");
                if (__success != null)
                    __success(null);
                yield break;
            }
        }

        public Task<object> unharness(CancellationToken cancellation)
        {
            var completion = new TaskCompletionSource<object>();
            Action<object> __success = (__res) => completion.SetResult((object)__res);
            Action<Exception> __failure = (__ex) => completion.SetException(__ex);
            var __cancellation = cancellation;
            __enter(() => __advance(__concurrentunharness(__cancellation, __success, __failure).GetEnumerator()), __failure);
            return completion.Task;
        }

        public void unharness(CancellationToken cancellation, Action<object> success, Action<Exception> failure)
        {
            var __success = success;
            var __failure = failure;
            var __cancellation = cancellation;
            __enter(() => __advance(__concurrentunharness(__cancellation, __success, __failure).GetEnumerator()), failure);
        }

        private IEnumerable<Expression> __concurrentvacation(CancellationToken __cancellation, Action<object> __success, Action<Exception> __failure)
        {
            var __expr2_var = new __expr2{Start = (___expr) =>
            {
                var __expr = (__expr2)___expr;
                Task.Delay((int)((rand(3, 7)) * 1000)).ContinueWith(__task =>
                {
                    __enter(() => __expr.__op3(true, null, null), (__ex) => __expr.__op3(false, null, __ex));
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
                    _santa.reindeer(this, __cancellation, (__res) => __expr.__op4(true, null, null), (__ex) => __expr.__op4(false, null, __ex));
                    __listen("unharness", () =>
                    {
                        __expr.__op4(null, true, null);
                    }

                    );
                }

                , __failure);
            }

            , __start2 = (___expr) =>
            {
                var __expr = (__expr2)___expr;
                __enter(() =>
                {
                    try
                    {
                        Console.WriteLine(_name + ": back from vacation");
                        __expr.__op3(null, true, null);
                    }
                    catch (Exception __ex)
                    {
                        __expr.__op3(null, false, __ex);
                    }
                }

                , __failure);
            }
            };
            yield return __expr2_var;
            if (__expr2_var.Failure != null)
                throw __expr2_var.Failure;
            {
                __dispatch("vacation");
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
            public void __op3(bool ? v1, bool ? v2, Exception __ex)
            {
                if (!tryUpdate(v1, v2, ref __op3_Left, ref __op3_Right, __ex))
                    return;
                if (v1.HasValue)
                {
                    if (__op3_Left.Value)
                        __start2(this);
                    else
                        __op2(false, null, __ex);
                }
                else
                {
                    if (__op3_Right.Value)
                        __op2(true, null, null);
                    else
                        __op2(false, null, __ex);
                }
            }

            private bool ? __op3_Left;
            private bool ? __op3_Right;
            public Action<__expr2> __start2;
            public void __op4(bool ? v1, bool ? v2, Exception __ex)
            {
                if (!tryUpdate(v1, v2, ref __op4_Left, ref __op4_Right, __ex))
                    return;
                if (v1.HasValue)
                {
                    if (!__op4_Left.Value)
                        __op2(null, false, __ex);
                    else if (__op4_Right.HasValue)
                        __op2(null, true, null);
                }
                else
                {
                    if (!__op4_Right.Value)
                        __op2(null, false, __ex);
                    else if (__op4_Left.HasValue)
                        __op2(null, true, null);
                }
            }

            private bool ? __op4_Left;
            private bool ? __op4_Right;
        }

        public readonly Guid __ID = Guid.NewGuid();
    }

    [Concurrent(id = "78f117ff-935e-4202-9b0c-d849dc618e76")]
    class Elf : ConcurrentObject
    {
        string _name;
        SantaClaus _santa;
        public Elf(string name, SantaClaus santa)
        {
            _name = name;
            _santa = santa;
        }

        protected override void __started()
        {
            var __enum = __concurrentmain(default (CancellationToken), null, null);
            __enter(() => __advance(__enum.GetEnumerator()), null);
        }

        [Concurrent]
        public void advice(bool given)
        {
            advice(given, default (CancellationToken), null, null);
        }

        private void work()
        {
            throw new InvalidOperationException("Cannot call private signals directly");
        }

        private IEnumerable<Expression> __concurrentmain(CancellationToken __cancellation, Action<object> __success, Action<Exception> __failure)
        {
            for (;;)
            {
                {
                    var __expr3_var = new __expr3{Start = (___expr) =>
                    {
                        var __expr = (__expr3)___expr;
                        __advance((__concurrentwork(__cancellation, (__res) =>
                        {
                            __expr.__op5(true, null, null);
                        }

                        , (__ex) =>
                        {
                            __expr.__op5(false, null, __ex);
                        }

                        )).GetEnumerator());
                        __expr.__op5(null, false, null);
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
            }

            {
                __dispatch("main");
                if (__success != null)
                    __success(null);
                yield break;
            }
        }

        private IEnumerable<Expression> __concurrentadvice(bool given, CancellationToken __cancellation, Action<object> __success, Action<Exception> __failure)
        {
            if (given)
                Console.WriteLine(_name + ": great advice, santa!");
            else
                Console.WriteLine(_name + ": Santa is busy, back to work");
            {
                __dispatch("advice");
                if (__success != null)
                    __success(null);
                yield break;
            }
        }

        public Task<object> advice(bool given, CancellationToken cancellation)
        {
            var completion = new TaskCompletionSource<object>();
            Action<object> __success = (__res) => completion.SetResult((object)__res);
            Action<Exception> __failure = (__ex) => completion.SetException(__ex);
            var __cancellation = cancellation;
            __enter(() => __advance(__concurrentadvice(given, __cancellation, __success, __failure).GetEnumerator()), __failure);
            return completion.Task;
        }

        public void advice(bool given, CancellationToken cancellation, Action<object> success, Action<Exception> failure)
        {
            var __success = success;
            var __failure = failure;
            var __cancellation = cancellation;
            __enter(() => __advance(__concurrentadvice(given, __cancellation, __success, __failure).GetEnumerator()), failure);
        }

        private IEnumerable<Expression> __concurrentwork(CancellationToken __cancellation, Action<object> __success, Action<Exception> __failure)
        {
            var __expr4_var = new __expr4{Start = (___expr) =>
            {
                var __expr = (__expr4)___expr;
                Task.Delay((int)((rand(1, 5)) * 1000)).ContinueWith(__task =>
                {
                    __enter(() => __expr.__op7(true, null, null), (__ex) => __expr.__op7(false, null, __ex));
                }

                );
            }

            , End = (__expr) =>
            {
                __enter(() => __advance(__expr.Continuator), __failure);
            }

            , __start3 = (___expr) =>
            {
                var __expr = (__expr4)___expr;
                __enter(() =>
                {
                    _santa.elf(this, __cancellation, (__res) => __expr.__op8(true, null, null), (__ex) => __expr.__op8(false, null, __ex));
                    __listen("advice", () =>
                    {
                        __expr.__op8(null, true, null);
                    }

                    );
                }

                , __failure);
            }

            , __start4 = (___expr) =>
            {
                var __expr = (__expr4)___expr;
                __enter(() =>
                {
                    try
                    {
                        Console.WriteLine(_name + ": off to see Santa");
                        __expr.__op7(null, true, null);
                    }
                    catch (Exception __ex)
                    {
                        __expr.__op7(null, false, __ex);
                    }
                }

                , __failure);
            }
            };
            yield return __expr4_var;
            if (__expr4_var.Failure != null)
                throw __expr4_var.Failure;
            {
                __dispatch("work");
                if (__success != null)
                    __success(null);
                yield break;
            }
        }

        private class __expr3 : Expression
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

        private class __expr4 : Expression
        {
            public void __op6(bool ? v1, bool ? v2, Exception __ex)
            {
                if (!tryUpdate(v1, v2, ref __op6_Left, ref __op6_Right, __ex))
                    return;
                if (v1.HasValue)
                {
                    if (__op6_Left.Value)
                        __start3(this);
                    else
                        __complete(false, __ex);
                }
                else
                {
                    if (__op6_Right.Value)
                        __complete(true, null);
                    else
                        __complete(false, __ex);
                }
            }

            private bool ? __op6_Left;
            private bool ? __op6_Right;
            public Action<__expr4> __start3;
            public void __op7(bool ? v1, bool ? v2, Exception __ex)
            {
                if (!tryUpdate(v1, v2, ref __op7_Left, ref __op7_Right, __ex))
                    return;
                if (v1.HasValue)
                {
                    if (__op7_Left.Value)
                        __start4(this);
                    else
                        __op6(false, null, __ex);
                }
                else
                {
                    if (__op7_Right.Value)
                        __op6(true, null, null);
                    else
                        __op6(false, null, __ex);
                }
            }

            private bool ? __op7_Left;
            private bool ? __op7_Right;
            public Action<__expr4> __start4;
            public void __op8(bool ? v1, bool ? v2, Exception __ex)
            {
                if (!tryUpdate(v1, v2, ref __op8_Left, ref __op8_Right, __ex))
                    return;
                if (v1.HasValue)
                {
                    if (!__op8_Left.Value)
                        __op6(null, false, __ex);
                    else if (__op8_Right.HasValue)
                        __op6(null, true, null);
                }
                else
                {
                    if (!__op8_Right.Value)
                        __op6(null, false, __ex);
                    else if (__op8_Left.HasValue)
                        __op6(null, true, null);
                }
            }

            private bool ? __op8_Left;
            private bool ? __op8_Right;
        }

        public readonly Guid __ID = Guid.NewGuid();
    }
}