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
        public static Template EmptyMain = Template.Parse(@"
            void main()
            {
                continue;
            }");

        public static Template GetterProperty = Template.Parse(@"
            public Task<__0> _1()
            {
                return Node.send(this, _2);
            }");

        public static ExpressionSyntax MainContinuation = CSharp.ParseExpression(@"
            (value) => {
                if (response != null)
                    response.success(value);
                
                Node.finished(this);
            }");

        public static ExpressionSyntax MainContinuated = CSharp.ParseExpression(@"
            (value) => {
                if (response != null)
                    throw new InvalidOperationException(""infinite"");
            }");

        public static ExpressionSyntax SignalContinuation = CSharp.ParseExpression(@"
            (value) => {
                if (response != null)
                    response.success(value);
            }");

        public static Template EmptyPrivateMethod = Template.Parse(
          @"private void _0()
            {
                enqueue(__1, (Processor)null);
            }");

        public static Template PrivateSignal = Template.ParseStatement("{throw new InvalidOperationException(\"calling a private signal\");}");

        public static Template OperatorState = Template.Parse("private bool? _0;");
        public static Template OrOperatorEval = Template.Parse(@"
            private void _0(bool? v1, bool? v2)
            {
                Debug.Assert(!v1.HasValue || !v2.HasValue);
                if (v1.HasValue)
                {
                    Debug.Assert(!_1.HasValue);
                    _1 = v1;
                }
                else
                {
                    Debug.Assert(v2.HasValue && !_2.HasValue);
                    _2 = v2;
                }

                if (_1.HasValue && _2.HasValue)
                    complete(_1.Value || _2.Value, __3);
            }");

        public static Template AndOperatorEval = Template.Parse(@"
            private void _0(bool? v1, bool? v2)
            {
                Debug.Assert(!v1.HasValue || !v2.HasValue);
                if (v1.HasValue)
                {
                    Debug.Assert(!_1.HasValue);
                    _1 = v1;
                }
                else
                {
                    Debug.Assert(v2.HasValue && !_2.HasValue);
                    _2 = v2;
                }

                if (_1.HasValue && _2.HasValue)
                    complete(_1.Value && _2.Value, __3);
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
                        _4();
                    else
                        complete(false, _3)
                }
                else
                {
                    Debug.Assert(v2.HasValue && !_2.HasValue);
                    _2 = v2;
                    complete(_2.Value, _3);
                }
            }");

        public static Template ExpressionClass = Template.Parse(@"
            private class _0
            {
            }");

        public static Template ExpressionInstantiation = Template.ParseStatement(@"
            return new _0();");

        public static Template StartCallback  = Template.ParseExpression("_0(__1, __2)");
        public static Template StarExpression = Template.ParseExpression("__marker__(__0, __1)");

        public static Template ExpressionVariable = Template.ParseStatement("var _0 = __1;");
    }
}
