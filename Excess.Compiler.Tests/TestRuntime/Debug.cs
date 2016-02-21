using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess.Compiler.Tests.TestRuntime
{
    class philosopher : ConcurrentObject
    {
        private string _name;
        private chopstick _left;
        private chopstick _right;
        private IEnumerable<Expression> __concurrentmain(string name, chopstick left, chopstick right, Action<object> __success, Action<Exception> __failure)
        {
            _name = name;
            _left = left;
            _right = right;
            var __expr1_var = new __expr1
            {
                Start = (___expr) =>
                {
                    var __expr = (__expr1)___expr;
                    __concurrentthink((__res) =>
                    {
                        __expr.__op1(true, null, null);
                    }

                    , (__ex) =>
                    {
                        __expr.__op1(false, null, __ex);
                    }

                    );
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
            {
                if (__success != null)
                    __success(null);
                yield break;
            }
        }

        protected override void __start(params object[] args)
        {
            var __enum = __concurrentmain((string)args[0], (chopstick)args[1], (chopstick)args[2], null, null);
            __advance(__enum.GetEnumerator());
        }

        private IEnumerable<Expression> __concurrentthink(Action<object> __success, Action<Exception> __failure)
        {
            console.write(_name + " is thinking");
            var __expr2_var = new __expr2
            {
                Start = (___expr) =>
                {
                    var __expr = (__expr2)___expr;
                    Task.Delay((int)((rand(1.0, 2.0)) * 1000)).ContinueWith(__task =>
                    {
                        __enter(() => __expr.__op2(true, null, null), (__ex) => __expr.__op2(false, null, __ex));
                    }

                    );
                }

            ,
                End = (__expr) =>
                {
                    __enter(() => __advance(__expr.Continuator), __failure);
                }

            ,
                __start1 = (___expr) =>
                {
                    var __expr = (__expr2)___expr;
                    __enter(() =>
                    {
                        __concurrenthungry((__res) =>
                        {
                            __expr.__op2(null, true, null);
                        }

                        , (__ex) =>
                        {
                            __expr.__op2(null, false, __ex);
                        }

                        );
                    }

                    , __failure);
                }
            };
            yield return __expr2_var;
            if (__expr2_var.Failure != null)
                throw __expr2_var.Failure;
            {
                if (__success != null)
                    __success(null);
                yield break;
            }
        }

        private IEnumerable<Expression> __concurrenthungry(Action<object> __success, Action<Exception> __failure)
        {
            console.write(_name + " is hungry");
            var __expr3_var = new __expr3
            {
                Start = (___expr) =>
                {
                    var __expr = (__expr3)___expr;
                    _left.acquire(this, (__res) => __expr.__op4(true, null, null), (__ex) => __expr.__op4(false, null, __ex));
                    _right.acquire(this, (__res) => __expr.__op4(null, true, null), (__ex) => __expr.__op4(null, false, __ex));
                }

            ,
                End = (__expr) =>
                {
                    __enter(() => __advance(__expr.Continuator), __failure);
                }

            ,
                __start2 = (___expr) =>
                {
                    var __expr = (__expr3)___expr;
                    __enter(() =>
                    {
                        __concurrenteat((__res) =>
                        {
                            __expr.__op3(null, true, null);
                        }

                        , (__ex) =>
                        {
                            __expr.__op3(null, false, __ex);
                        }

                        );
                    }

                    , __failure);
                }
            };
            yield return __expr3_var;
            if (__expr3_var.Failure != null)
                throw __expr3_var.Failure;
            {
                if (__success != null)
                    __success(null);
                yield break;
            }
        }

        private IEnumerable<Expression> __concurrenteat(Action<object> __success, Action<Exception> __failure)
        {
            console.write(_name + " is eating");
            var __expr4_var = new __expr4
            {
                Start = (___expr) =>
                {
                    var __expr = (__expr4)___expr;
                    Task.Delay((int)((rand(1.0, 2.0)) * 1000)).ContinueWith(__task =>
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
            yield return __expr4_var;
            if (__expr4_var.Failure != null)
                throw __expr4_var.Failure;
            _left.release(this);
            _right.release(this);
            {
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

            private bool? __op2_Left;
            private bool? __op2_Right;
            public Action<__expr2> __start1;
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

            private bool? __op3_Left;
            private bool? __op3_Right;
            public Action<__expr3> __start2;
            public void __op4(bool? v1, bool? v2, Exception __ex)
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

            private bool? __op4_Left;
            private bool? __op4_Right;
        }

        private class __expr4 : Expression
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

    class chopstick : ConcurrentObject
    {
        private object _owner;
        private IEnumerable<Expression> __concurrentacquire(object owner, Action<object> __success, Action<Exception> __failure)
        {
            if (_owner != null)
            {
                var __expr5_var = new __expr5
                {
                    Start = (___expr) =>
                    {
                        var __expr = (__expr5)___expr;
                        __listen("release", () =>
                        {
                            __expr.__op6(true, null, null);
                        }

                        );
                        __expr.__op6(null, false, null);
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

            _owner = owner;
            {
                if (__success != null)
                    __success(null);
                yield break;
            }
        }

        public Task<object> acquire(object owner)
        {
            var completion = new TaskCompletionSource<object>();
            Action<object> __success = (__res) => completion.SetResult((object)__res);
            Action<Exception> __failure = (__ex) => completion.SetException(__ex);
            __enter(() => __advance(__concurrentacquire(owner, __success, __failure).GetEnumerator()), __failure);
            return completion.Task;
        }

        public void acquire(object owner, Action<object> success, Action<Exception> failure)
        {
            var __success = success;
            var __failure = failure;
            __enter(() => __advance(__concurrentacquire(owner, __success, __failure).GetEnumerator()), failure);
        }

        private IEnumerable<Expression> __concurrentrelease(object owner, Action<object> __success, Action<Exception> __failure)
        {
            if (_owner != owner)
                throw new InvalidOperationException();
            _owner = null;
            {
                if (__success != null)
                    __success(null);
                yield break;
            }
        }

        public Task<object> release(object owner)
        {
            var completion = new TaskCompletionSource<object>();
            Action<object> __success = (__res) => completion.SetResult((object)__res);
            Action<Exception> __failure = (__ex) => completion.SetException(__ex);
            __enter(() => __advance(__concurrentrelease(owner, __success, __failure).GetEnumerator()), __failure);
            return completion.Task;
        }

        public void release(object owner, Action<object> success, Action<Exception> failure)
        {
            var __success = success;
            var __failure = failure;
            __enter(() => __advance(__concurrentrelease(owner, __success, __failure).GetEnumerator()), failure);
        }

        private class __expr5 : Expression
        {
            public void __op6(bool? v1, bool? v2, Exception __ex)
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

            private bool? __op6_Left;
            private bool? __op6_Right;
        }
    }
}
