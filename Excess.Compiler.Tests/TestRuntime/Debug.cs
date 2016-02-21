using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess.Compiler.Tests.TestRuntime
{
    class VendingMachine : ConcurrentObject
    {
        private IEnumerable<Expression> __concurrentcoin(Action<object> __success, Action<Exception> __failure)
        {
            if (false && !__awaiting("coin"))
                throw new InvalidOperationException("coin" + " can not be executed in this state");
            __dispatch("coin");
            if (__success != null)
                __success(null);
            yield break;
        }

        public Task<object> coin()
        {
            var completion = new TaskCompletionSource<object>();
            Action<object> __success = (__res) => completion.SetResult((object)__res);
            Action<Exception> __failure = (__ex) => completion.SetException(__ex);
            __enter(() => __advance(__concurrentcoin(__success, __failure).GetEnumerator()), __failure);
            return completion.Task;
        }

        public void coin(Action<object> success, Action<Exception> failure)
        {
            var __success = success;
            var __failure = failure;
            __enter(() => __advance(__concurrentcoin(__success, __failure).GetEnumerator()), failure);
        }

        private IEnumerable<Expression> __concurrentchoc(Action<object> __success, Action<Exception> __failure)
        {
            if (true && !__awaiting("choc"))
                throw new InvalidOperationException("choc" + " can not be executed in this state");
            __dispatch("choc");
            if (__success != null)
                __success(null);
            yield break;
        }

        public Task<object> choc()
        {
            var completion = new TaskCompletionSource<object>();
            Action<object> __success = (__res) => completion.SetResult((object)__res);
            Action<Exception> __failure = (__ex) => completion.SetException(__ex);
            __enter(() => __advance(__concurrentchoc(__success, __failure).GetEnumerator()), __failure);
            return completion.Task;
        }

        public void choc(Action<object> success, Action<Exception> failure)
        {
            var __success = success;
            var __failure = failure;
            __enter(() => __advance(__concurrentchoc(__success, __failure).GetEnumerator()), failure);
        }

        private IEnumerable<Expression> __concurrenttoffee(Action<object> __success, Action<Exception> __failure)
        {
            if (true && !__awaiting("toffee"))
                throw new InvalidOperationException("toffee" + " can not be executed in this state");
            __dispatch("toffee");
            if (__success != null)
                __success(null);
            yield break;
        }

        public Task<object> toffee()
        {
            var completion = new TaskCompletionSource<object>();
            Action<object> __success = (__res) => completion.SetResult((object)__res);
            Action<Exception> __failure = (__ex) => completion.SetException(__ex);
            __enter(() => __advance(__concurrenttoffee(__success, __failure).GetEnumerator()), __failure);
            return completion.Task;
        }

        public void toffee(Action<object> success, Action<Exception> failure)
        {
            var __success = success;
            var __failure = failure;
            __enter(() => __advance(__concurrenttoffee(__success, __failure).GetEnumerator()), failure);
        }

        private IEnumerable<Expression> __concurrentmain(Action<object> __success, Action<Exception> __failure)
        {
            for (;;)
            {
                var __expr1_var = new __expr1
                {
                    Start = (___expr) =>
                    {
                        var __expr = (__expr1)___expr;
                        __listen("coin", () =>
                        {
                            __expr.__op1(true, null, null);
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
                        var __expr = (__expr1)___expr;
                        __enter(() =>
                        {
                            __listen("choc", () =>
                            {
                                __expr.__op2(true, null, null);
                            }

                            );
                            __listen("toffee", () =>
                            {
                                __expr.__op2(null, true, null);
                            }

                            );
                        }

                        , __failure);
                    }
                };
                yield return __expr1_var;
                if (__expr1_var.Failure != null)
                    throw __expr1_var.Failure;
            }

            {
                if (__success != null)
                    __success(null);

                yield break;
            }
        }

        protected override void __start(params object[] args)
        {
            var __enum = __concurrentmain(null, null);
            __advance(__enum.GetEnumerator());
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
                        __start1(this);
                    else
                        __complete(false, __ex);
                }
                else
                {
                    if (__op1_Right.Value)
                        __complete(true, null);
                    else
                        __complete(false, __ex);
                }
            }

            private bool? __op1_Left;
            private bool? __op1_Right;
            public Action<__expr1> __start1;
            public void __op2(bool? v1, bool? v2, Exception __ex)
            {
                if (!tryUpdate(v1, v2, ref __op2_Left, ref __op2_Right, __ex))
                    return;
                if (v1.HasValue)
                {
                    if (__op2_Left.Value)
                        __op1(null, true, null);
                    else if (__op2_Right.HasValue)
                        __op1(null, false, __ex);
                }
                else
                {
                    if (__op2_Right.Value)
                        __op1(null, true, null);
                    else if (__op2_Left.HasValue)
                        __op1(null, false, __ex);
                }
            }

            private bool? __op2_Left;
            private bool? __op2_Right;
        }
    }
}
