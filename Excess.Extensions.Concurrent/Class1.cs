using Excess.Extensions.Concurrent.Runtime;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess.Extensions.Concurrent
{
    class SomeClass : ConcurrentObject
    {
        private class __expr0
        {
            public Action Start;
            public Func<Expression> Continuation;
            private void complete(bool success)
            {
                if (!success)
                {
                    //td: exception
                }

                Continuation(this);
            }

            private void __op0(bool? v1, bool? v2)
            {
                Debug.Assert(!v1.HasValue || !v2.HasValue);
                if (v1.HasValue)
                {
                    Debug.Assert(!__op0_Left.HasValue);
                    __op0_Left = v1;
                    if (v1.Value)
                        complete(true);
                    else if (__op0_Right.HasValue)
                        complete(false);
                }
                else
                {
                    Debug.Assert(v2.HasValue && !__op0_Right.HasValue);
                    __op0_Right = v2;
                    if (v2.Value)
                        complete(true);
                    else if (__op0_Left.HasValue)
                        complete(false);
                }
            }

            private bool? __op0_Left;
            private bool? __op0_Right;
            private void __op1(bool? v1, bool? v2)
            {
                Debug.Assert(!v1.HasValue || !v2.HasValue);
                if (v1.HasValue)
                {
                    Debug.Assert(!__op1_Right.HasValue);
                    __op1_Left = v1;
                    if (__op1_Left.Value)
                        __op2();
                    else
                        __op0(null, false);
                }
                else
                {
                    Debug.Assert(v2.HasValue && !__op1_Right.HasValue);
                    Debug.Assert(__op1_Left.HasValue && __op1_Left.HasValue);
                    __op1_Right = v2;
                    if (__op1_Right.Value)
                        __op0(null, true);
                    else
                        __op0(null, false);
                }
            }

            private bool? __op1_Left;
            private bool? __op1_Right;
            private Func<object> __op2;
            private void __op3(bool? v1, bool? v2)
            {
                Debug.Assert(!v1.HasValue || !v2.HasValue);
                if (v1.HasValue)
                {
                    Debug.Assert(!__op3_Left.HasValue);
                    __op3_Left = v1;
                    if (!v1.Value)
                        __op1(false, null);
                    else if (__op3_Right.HasValue)
                        __op1(true, null);
                }
                else
                {
                    Debug.Assert(v2.HasValue && !__op3_Right.HasValue);
                    __op3_Right = v2;
                    if (!v2.Value)
                        __op1(false, null);
                    else if (__op3_Left.HasValue)
                        __op1(true, null);
                }
            }

            private bool? __op3_Left;
            private bool? __op3_Right;
        }

        private IEnumerable<Expression> main_concurrent(Func<object> __success, Func<object> __failure)
        {
            return new __expr0
            {
                Start = () =>
                {
                    __marker__(A, __op0(result, null));
                    __marker__(B, __op3(result, null));
                    __marker__(C(), __op3(null, result));
                }

            ,
                Continuation = (expr) => Advance(expr),
                __op2 = () =>
                {
                    __marker__(D(10), __op1(null, result));
                }
            };
        }
    }
}
