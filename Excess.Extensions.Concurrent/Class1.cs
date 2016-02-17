using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Concurrent;

namespace Concurrent
{
    class SomeClass : Runtime.Object
    {
        private int D(int v)
        {
            return v + 1;
        }

        private IEnumerable<Runtime.Expression> __concurrentmain(Action<object> __success, Action<Exception> __failure)
        {
            var __expr1_var = new __expr1
            {
                Start = (___expr) =>
                {
                    var __expr = (__expr1)___expr;
                    __run(() =>
                    {
                        __listen("A", () =>
                        {
                            __expr.__op1(true, null, null);
                        }

                        );
                        __listen("B", () =>
                        {
                            __expr.__op3(true, null, null);
                        }

                        );
                        __concurrentC((__res) =>
                        {
                            __expr.__op3(null, true, null);
                        }

                        , (__ex) =>
                        {
                            __expr.__op3(null, false, __ex);
                        }

                        );
                    }

                    , null, __failure);
                }

            ,
                End = (__expr) =>
                {
                    __run(() => __advance(__expr.Continuator), __success, __failure);
                }

            ,
                __start1 = (___expr) =>
                {
                    var __expr = (__expr1)___expr;
                    __run(() =>
                    {
                        try
                        {
                            D(10);
                            try
                            {
                                __expr.__op2(null, true, null);
                            }
                            catch
                            {
                            }
                        }
                        catch (Exception __ex)
                        {
                            try
                            {
                                __expr.__op2(null, false, __ex);
                            }
                            catch
                            {
                            }
                        }
                    }

                    , null, __failure);
                }
            };
            yield return __expr1_var;
            if (__expr1_var.Failure != null)
                throw __expr1_var.Failure;
            {
                if (__success != null)
                    try
                    {
                        __success(null);
                    }
                    catch
                    {
                    }

                yield break;
            }
        }

        private IEnumerable<Runtime.Expression> __concurrentA(Action<object> __success, Action<Exception> __failure)
        {
            try
            {
                __dispatch("A");
            }
            catch (Exception __ex)
            {
                if (__failure != null)
                    try
                    {
                        __failure(__ex);
                    }
                    catch
                    {
                    }

                yield break;
            }

            if (__success != null)
                try
                {
                    __success(null);
                }
                catch
                {
                }

            yield break;
        }

        public Task<object> A()
        {
            var completion = new TaskCompletionSource<object>();
            Action<object> __success = (__res) => completion.SetResult((object)__res);
            Action<Exception> __failure = (__ex) => completion.SetException(__ex);
            __run(() => __advance(__concurrentA(__success, __failure).GetEnumerator()), null, null);
            return completion.Task;
        }

        public void A(Action<object> success = null, Action<Exception> failure = null)
        {
            var __success = success;
            var __failure = failure;
            __run(() => __advance(__concurrentA(__success, __failure).GetEnumerator()), null, null);
        }

        private IEnumerable<Runtime.Expression> __concurrentB(Action<object> __success, Action<Exception> __failure)
        {
            try
            {
                __dispatch("B");
            }
            catch (Exception __ex)
            {
                if (__failure != null)
                    try
                    {
                        __failure(__ex);
                    }
                    catch
                    {
                    }

                yield break;
            }

            if (__success != null)
                try
                {
                    __success(null);
                }
                catch
                {
                }

            yield break;
        }

        public Task<object> B()
        {
            var completion = new TaskCompletionSource<object>();
            Action<object> __success = (__res) => completion.SetResult((object)__res);
            Action<Exception> __failure = (__ex) => completion.SetException(__ex);
            __run(() => __advance(__concurrentB(__success, __failure).GetEnumerator()), null, null);
            return completion.Task;
        }

        public void B(Action<object> success = null, Action<Exception> failure = null)
        {
            var __success = success;
            var __failure = failure;
            __run(() => __advance(__concurrentB(__success, __failure).GetEnumerator()), null, null);
        }

        private IEnumerable<Runtime.Expression> __concurrentF(Action<object> __success, Action<Exception> __failure)
        {
            try
            {
                __dispatch("F");
            }
            catch (Exception __ex)
            {
                if (__failure != null)
                    try
                    {
                        __failure(__ex);
                    }
                    catch
                    {
                    }

                yield break;
            }

            if (__success != null)
                try
                {
                    __success(null);
                }
                catch
                {
                }

            yield break;
        }

        public Task<object> F()
        {
            var completion = new TaskCompletionSource<object>();
            Action<object> __success = (__res) => completion.SetResult((object)__res);
            Action<Exception> __failure = (__ex) => completion.SetException(__ex);
            __run(() => __advance(__concurrentF(__success, __failure).GetEnumerator()), null, null);
            return completion.Task;
        }

        public void F(Action<object> success = null, Action<Exception> failure = null)
        {
            var __success = success;
            var __failure = failure;
            __run(() => __advance(__concurrentF(__success, __failure).GetEnumerator()), null, null);
        }

        private IEnumerable<Runtime.Expression> __concurrentG(Action<object> __success, Action<Exception> __failure)
        {
            try
            {
                __dispatch("G");
            }
            catch (Exception __ex)
            {
                if (__failure != null)
                    try
                    {
                        __failure(__ex);
                    }
                    catch
                    {
                    }

                yield break;
            }

            if (__success != null)
                try
                {
                    __success(null);
                }
                catch
                {
                }

            yield break;
        }

        public Task<object> G()
        {
            var completion = new TaskCompletionSource<object>();
            Action<object> __success = (__res) => completion.SetResult((object)__res);
            Action<Exception> __failure = (__ex) => completion.SetException(__ex);
            __run(() => __advance(__concurrentG(__success, __failure).GetEnumerator()), null, null);
            return completion.Task;
        }

        public void G(Action<object> success = null, Action<Exception> failure = null)
        {
            var __success = success;
            var __failure = failure;
            __run(() => __advance(__concurrentG(__success, __failure).GetEnumerator()), null, null);
        }

        private IEnumerable<Runtime.Expression> __concurrentC(Action<object> __success, Action<Exception> __failure)
        {
            if (2 > 1)
            {
                if (__success != null)
                    try
                    {
                        __success(null);
                    }
                    catch
                    {
                    }

                yield break;
            }

            var __expr2_var = new __expr2
            {
                Start = (___expr) =>
                {
                    var __expr = (__expr2)___expr;
                    __run(() =>
                    {
                        __listen("F", () =>
                        {
                            __expr.__op4(true, null, null);
                        }

                        );
                        __listen("G", () =>
                        {
                            __expr.__op4(null, true, null);
                        }

                        );
                    }

                    , null, __failure);
                }

            ,
                End = (__expr) =>
                {
                    __run(() => __advance(__expr.Continuator), __success, __failure);
                }
            };
            yield return __expr2_var;
            if (__expr2_var.Failure != null)
                throw __expr2_var.Failure;
            if (1 > 2)
            {
                if (__success != null)
                    try
                    {
                        __success(null);
                    }
                    catch
                    {
                    }

                yield break;
            }

            {
                if (__success != null)
                    try
                    {
                        __success(null);
                    }
                    catch
                    {
                    }

                yield break;
            }
        }

        private class __expr1 : Runtime.Expression
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
            public void __op2(bool? v1, bool? v2, Exception __ex)
            {
                if (!tryUpdate(v1, v2, ref __op2_Left, ref __op2_Right, __ex))
                    return;
                if (v1.HasValue)
                {
                    if (__op2_Left.Value)
                        __start1(this);
                    else
                        __op1(null, false, __ex);
                }
                else
                {
                    Debug.Assert(__op2_Left.HasValue && __op2_Left.Value);
                    if (__op2_Right.Value)
                        __op1(null, true, null);
                    else
                        __op1(null, false, __ex);
                }
            }

            private bool? __op2_Left;
            private bool? __op2_Right;
            public Action<__expr1> __start1;
            public void __op3(bool? v1, bool? v2, Exception __ex)
            {
                if (!tryUpdate(v1, v2, ref __op3_Left, ref __op3_Right, __ex))
                    return;
                if (v1.HasValue)
                {
                    if (!__op3_Left.Value)
                        __op2(false, null, __ex);
                    else if (__op3_Right.HasValue)
                        __op2(true, null, null);
                }
                else
                {
                    if (!__op3_Right.Value)
                        __op2(false, null, __ex);
                    else if (__op3_Left.HasValue)
                        __op2(true, null, null);
                }
            }

            private bool? __op3_Left;
            private bool? __op3_Right;
        }

        private class __expr2 : Runtime.Expression
        {
            public void __op4(bool? v1, bool? v2, Exception __ex)
            {
                if (!tryUpdate(v1, v2, ref __op4_Left, ref __op4_Right, __ex))
                    return;
                if (v1.HasValue)
                {
                    if (!__op4_Left.Value)
                        __complete(false, __ex);
                    else if (__op4_Right.HasValue)
                        __complete(true, null);
                }
                else
                {
                    if (!__op4_Right.Value)
                        __complete(false, __ex);
                    else if (__op4_Left.HasValue)
                        __complete(true, null);
                }
            }

            private bool? __op4_Left;
            private bool? __op4_Right;
        }
    }
}
