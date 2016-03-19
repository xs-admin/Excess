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
                    if (_2.Value) __3;
                    else          __4;
                }
            }");

        public static ExpressionSyntax FailureParameter = CSharp.ParseExpression("__ex");
        public static ExpressionSyntax ExpressionParameter = CSharp.ParseExpression("__expr");

        public static Template ExpressionClass = Template.Parse(@"
            private class _0 : Expression
            {
            }");

        public static Template ExpressionInstantiation = Template.ParseStatement(@"
            yield return new _0
            {
                Start = __1,
                End = (__expr) => 
                {
                    __enter(() => __advance(__expr.Continuator), __failure);
                }
            };");

        public static Template StartCallback = Template.ParseExpression("__expr._1(__2, __3, __4)");
        public static Template StartExpression = Template.ParseExpression("__marker__(__0, __1, __2)");
        public static Template StartCallbackLambda = Template.ParseExpression(@"
            (___expr) => 
            {
                var __expr = (_0)___expr;
            }");

        public static Template StartCallbackEnter = Template.ParseStatement(@"
            __enter(() =>
            {
            }, __failure);");

        public static Template ExpressionVariable = Template.ParseStatement("var _0 = __1;");

        public static Template ExpressionReturn = Template.ParseStatement(@"
            {
                __dispatch(__1);
                if (__success != null)
                    __success(__0); 
                yield break;
            }");

        public static Template ConcurrentMethod = Template.Parse(@"
            private IEnumerable<Expression> _0(CancellationToken __cancellation, Action<object> __success, Action<Exception> __failure)
            {
            }");

        public static Template OperatorStartField = Template.Parse("public Action<_1> _0;");
        public static Template ExpressionCompleteCall = Template.ParseExpression("__complete(__0, __1)");

        public static Template SignalListener = Template.ParseStatement("__listen(__0, __1);");

        public static Template EmptySignalMethod = Template.Parse(@"
            private IEnumerable<Expression> _0(CancellationToken __cancellation, __Action<object> __success, Action<Exception> __failure)
            {
                if (__2 && !__awaiting(__1))
                    throw new InvalidOperationException(__1 + "" can not be executed in this state"");
                
                __dispatch(__1);

                if (__success != null)
                    __success(null);

                yield break;
            }");

        public static Template TaskPublicMethod = Template.Parse(@"
            public Task<__1> _0(CancellationToken cancellation)
            {
                var completion = new TaskCompletionSource<__1>();
                Action<object> __success = (__res) => completion.SetResult((__1)__res);
                Action<Exception> __failure = (__ex) => completion.SetException(__ex);
                var __cancellation = cancellation;
                __enter(() => __advance(__2), __failure);
                return completion.Task;
            }");

        public static Template TaskCallbackMethod = Template.Parse(@"
            public void _0(CancellationToken cancellation, Action<object> success, Action<Exception> failure)
            {
                var __success = success;
                var __failure = failure;
                var __cancellation = cancellation;
                __enter(() => __advance(__1), failure);
            }");

        public static Template InternalCall = Template.ParseExpression("_0(__cancellation, __success, __failure).GetEnumerator()");

        public static Template ExpressionAssigment = Template.ParseStatement("__expr._0 = (__1)__res;");
        public static Template AssigmentAfterExpression = Template.ParseStatement("_0 = _1._0;");

        public static Template ExpressionProperty = Template.ParseExpression("__expr._0");

        public static Template ExpressionFailedCheck = Template.ParseStatement(@"
            if (_0.Failure != null)
                throw _0.Failure;");

        public static Template AssignmentField = Template.Parse("public __1 _0;");

        public static Template TryVariable = Template.ParseStatement("var _0 = false;");
        public static Template SetTryVariable = Template.ParseStatement("_0 = true;");
        public static Template DefaultValue = Template.ParseExpression("default(__0)");
        public static Template Negation = Template.ParseExpression("!_0");

        public static Template AwaitExpr = Template.ParseStatement("__0 || false;");

        public static Template StartObject = Template.Parse(@"
            protected override void __start(object[] args)
            {
                var __enum = __0;
                __advance(__enum.GetEnumerator());
            }");

        public static Template ConcurrentMain = Template.ParseExpression("__concurrentmain()");

        public static Template StartObjectArgument = Template.ParseExpression("(__0)args[__1]");

        public static Template Seconds = Template.ParseStatement(@"
            Task.Delay((int)((__0)*1000))
                .ContinueWith(__task =>
                {
                    __enter(() => __1, (__ex) => __2);
                });");

        public static Template SuccessFunction = Template.ParseExpression("(__res) => __0");
        public static Template FailureFunction = Template.ParseExpression("(__ex) => __0");

        public static Template Advance = Template.ParseExpression("__advance((__0).GetEnumerator())");

        public static Template Enqueue = Template.ParseStatement(@"
            _0.Enqueue(() => __1);");

        public static Template DequeueAsynch = Template.ParseStatement(@"
            if (_0.Any())
                __enter(() => _0.Dequeue()(), null);");

        public static Template DequeueSynch = Template.ParseStatement(@"
            if (_0.Any())
                _0.Dequeue()();");

        public static Template SignalQueueMember = Template.Parse(@"
            private Queue<Action> _0 = new Queue<Action>();");

        public static Template SynchVoidMethod = Template.ParseStatement("__0;");
        public static Template SynchReturnMethod = Template.ParseStatement("return __0.Result;");

        public static ExpressionSyntax NullCancelationToken = CSharp.ParseExpression("default(CancellationToken)");
        public static ExpressionSyntax CancelationArgument = CSharp.ParseExpression("__cancellation");
        public static StatementSyntax PrivateSignal = CSharp.ParseStatement(
            "throw new InvalidOperationException(\"Cannot call private signals directly\");");

        public static AttributeArgumentListSyntax GuidAttributeArgument(bool optional, Guid? value = null)
        {
            Guid guid = value.HasValue
                ? value.Value
                : Guid.NewGuid();

            var colon = optional
                ? ":"
                : "=";

            return CSharp.ParseAttributeArgumentList(
                $"(Id {colon} \"{guid}\")");
        }

        public static Template RemoteMethod = Template.Parse(@"
            public static _1 CreateRemote(IIdentityServer server)
            {
                var result = new _0();
                result.Identity = server;
                return result;
            }");

        public static Template RemoteInternalMethod = Template.ParseStatements(@"
            _server.dispatch(_id, __0, JObject.FromObject(__1), response => 
            {
                var json = JObject.Parse(response);
                __success(__2);
            });

            yield break;");

        public static Template RemoteResult = Template.ParseExpression("(__0)json.__res");

        public static Template Interface = Template.Parse(@"
            public interface _0
            {
            }");
    }
}
