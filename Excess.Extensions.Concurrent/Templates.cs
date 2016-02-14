using Excess.Compiler.Roslyn;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess.Extensions.Concurrent
{
    using CSharp = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

    internal static class Templates
    {
        public static Template OperatorState = Template.Parse("private bool? _0;");
        public static Template OrOperatorEval = Template.Parse(@"
            public void _0(bool? v1, bool? v2, Exception __ex)
            {
                if (!tryUpdate(v1, v2, ref _1, ref _2, __ex))
                    return;

                if (v1.HasValue)
                {
                    if (_1.Value)         __3;
                    else if (_2.HasValue) __4;
                }
                else
                {
                    if (_2.Value)         __3;
                    else if (_1.HasValue) __4;
                }
            }");

        public static Template AndOperatorEval = Template.Parse(@"
            public void _0(bool? v1, bool? v2, Exception __ex)
            {
                if (!tryUpdate(v1, v2, ref _1, ref _2, __ex))
                    return;

                if (v1.HasValue)
                {
                    if (!_1.Value)        __4;
                    else if (_2.HasValue) __3;
                }
                else
                {
                    if (!_2.Value)        __4;
                    else if (_1.HasValue) __3;
                }
            }");

        public static Template ContinuationOperatorEval = Template.Parse(@"
            public void _0(bool? v1, bool? v2, Exception __ex)
            {
                if (!tryUpdate(v1, v2, ref _1, ref _2, __ex))
                    return;

                if (v1.HasValue)
                {
                    if (_1.Value) _5(this);
                    else          __4;
                }
                else
                {
                    Debug.Assert(_1.HasValue && _1.Value);
                    if (_2.Value) __3;
                    else          __4;
                }
            }");

        public static ExpressionSyntax FailureParameter = CSharp.ParseExpression("__ex");
        public static ExpressionSyntax ExpressionParameter = CSharp.ParseExpression("__expr");

        public static Template ExpressionClass = Template.Parse(@"
            private class _0 : Runtime.Expression
            {
            }");

        public static Template ExpressionInstantiation = Template.ParseStatement(@"
            yield return new _0
            {
                Start = __1,
                Continuation = (__expr) => run(() => __advance(__expr), __success, __failure)
            };");

        public static Template StartCallback = Template.ParseExpression("__expr._1(__2, __3, __4)");
        public static Template StartExpression = Template.ParseExpression("__marker__(__0, __1, __2)");
        public static Template StartCallbackLambda = Template.ParseExpression(@"
            (___expr) => 
            {
                var __expr = (_0)___expr;
            }");

        public static Template ExpressionVariable = Template.ParseStatement("var _0 = __1;");

        public static Template MethodInvocation = Template.ParseStatement(@"
            try
            {
                __0;
                
                try {__1;} catch {}
            }
            catch(Exception __ex)
            {
                try {__2;} catch {}
            }");

        public static Template ExpressionReturn = Template.ParseStatement(@"
            {
                if (__success != null)
                    __success(__0);
                yield break;
            }");

        public static Template ConcurrentMethod = Template.Parse(@"
            private IEnumerable<Runtime.Expression> _0(Action<object> __success, Action<Exception> __failure)
            {
            }");

        public static Template OperatorStartField = Template.Parse("public Action<_1> _0;");
        public static Template ExpressionCompleteCall = Template.ParseExpression("__complete(__0, __1)");

        public static Template SignalListener = Template.ParseStatement("__listen(__0, __1);");

        public static Template EmptySignalMethod = Template.Parse(@"
            private IEnumerable<Runtime.Expression> _0(Action<object> __success, Action<Exception> __failure)
            {
                try
                {
                    __dispatch(__1);
                }
                catch(Exception __ex)
                {
                    if (__failure != null)
                        try {__failure(__ex);} catch {}
                }

                try {__success(null);} catch {}

                yield break;
            }");

    }
}
