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
        public static ExpressionSyntax SignalContinuation = CSharp.ParseExpression(@"
            (value) => {
                if (response != null)
                    response.success(value);
            }");

        public static Template EmptyPrivateMethod = Template.Parse(
          @"private void _0()
            {
                Debug.Assert(false); //td
            }");

        public static Template OperatorState = Template.Parse("private bool? _0;");
        public static Template OrOperatorEval = Template.Parse(@"
            private void _0(bool? v1, bool? v2)
            {
                Debug.Assert(!v1.HasValue || !v2.HasValue);
                if (v1.HasValue)
                {
                    Debug.Assert(!_1.HasValue);
                    _1 = v1;
                    if (v1.Value)
                        __3;
                    else if (_2.HasValue)
                        __4;
                }
                else
                {
                    Debug.Assert(v2.HasValue && !_2.HasValue);
                    _2 = v2;
                    if (v2.Value)
                        __3;
                    else if (_1.HasValue)
                        __4;
                }
            }");

        public static Template AndOperatorEval = Template.Parse(@"
            private void _0(bool? v1, bool? v2)
            {
                Debug.Assert(!v1.HasValue || !v2.HasValue);
                if (v1.HasValue)
                {
                    Debug.Assert(!_1.HasValue);
                    _1 = v1;
                    if (!v1.Value)
                        __4;
                    else if (_2.HasValue)
                        __3;
                }
                else
                {
                    Debug.Assert(v2.HasValue && !_2.HasValue);
                    _2 = v2;
                    if (!v2.Value)
                        __4;
                    else if (_1.HasValue)
                        __3;
                }
            }");

        public static Template ContinuationOperatorEval = Template.Parse(@"
            private void _0(bool? v1, bool? v2)
            {
                Debug.Assert(!v1.HasValue || !v2.HasValue);
                if (v1.HasValue)
                {
                    Debug.Assert(!_2.HasValue);
                    _1 = v1;
                    if (_1.Value)
                        _5();
                    else
                        __4;
                }
                else
                {
                    Debug.Assert(v2.HasValue && !_2.HasValue);
                    Debug.Assert(_1.HasValue && _1.HasValue);

                    _2 = v2;
                    if (_2.Value)
                        __3;
                    else
                        __4;
                }
            }");

        public static Template ExpressionClass = Template.Parse(@"
            private class _0
            {
            }");

        public static Template ExpressionInstantiation = Template.ParseStatement(@"
            return new _0
            {
                Start = __1,
                Continuation = (expr) => Advance(expr),
            };");

        public static Template StartCallback = Template.ParseExpression("_0(__1, __2)");
        public static Template StartExpression = Template.ParseExpression("__marker__(__0, __1)");

        public static Template ExpressionVariable = Template.ParseStatement("var _0 = __1;");

        public static Template MethodInvocation = Template.ParseStatement(@"
            try
            {
                __0;
                
                try {__1(null);} catch {}
            }
            catch(Exception __ex)
            {
                try {__2(__ex);} catch {}
            }");

        public static Template ExpressionReturn = Template.ParseStatement(@"
            {
                if (__success != null)
                    __success(__0);
                yield break;
            }");

        public static Template ConcurrentMethod = Template.Parse(@"
            private IEnumerable<Expression> _0(Func<object> __success, Func<object> __failure)
            {
            }");

        public static Template OperatorStartField = Template.Parse("private Func<object> _0;");
        public static Template ExpressionStartField = Template.Parse("public Action Start;");
        public static Template ExpressionContinuationField = Template.Parse("public Func<Expression> Continuation;");

        public static Template ExpressionComplete = Template.Parse(@"
            private void complete(bool success)
            {
                if (!success)
                {
                    //td: exception
                }

                
                Continuation(this);
            }");

        public static Template ExpressionCompleteCall = Template.ParseExpression("complete(__0)");
    }
}
