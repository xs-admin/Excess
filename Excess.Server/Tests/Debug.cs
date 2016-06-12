using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Excess.Concurrent.Runtime;
using Excess;

namespace SomeNS
{
    using System.Threading;
    using System.Threading.Tasks;
    using Excess.Concurrent.Runtime;
    using Excess.Server.Middleware;

    public struct HelloModel
    {
        public string Greeting;
        public int Times;
        public GoodbyeService Goodbye;
    }

    [Concurrent(id = "f47351db-946c-41ee-bc12-46785e113774")]
    [ConcurrentSingleton(id: "aa6fe8a4-a0c9-4039-8f22-11bc2d73132f")]
    public class HelloService : ConcurrentObject
    {
        int _times = 0;
        [Concurrent]
        public HelloModel Hello(string who)
        {
            return Hello(who, default(CancellationToken)).Result;
        }

        private IEnumerable<Expression> __concurrentHello(string who, CancellationToken __cancellation, Action<object> __success, Action<Exception> __failure)
        {
            {
                __dispatch("Hello");
                if (__success != null)
                    __success(new HelloModel { Greeting = "greetings, " + who, Times = _times++, Goodbye = spawn<GoodbyeService>(who) });
                yield break;
            }
        }

        public Task<HelloModel> Hello(string who, CancellationToken cancellation)
        {
            var completion = new TaskCompletionSource<HelloModel>();
            Action<object> __success = (__res) => completion.SetResult((HelloModel)__res);
            Action<Exception> __failure = (__ex) => completion.SetException(__ex);
            var __cancellation = cancellation;
            __enter(() => __advance(__concurrentHello(who, __cancellation, __success, __failure).GetEnumerator()), __failure);
            return completion.Task;
        }

        public void Hello(string who, CancellationToken cancellation, Action<object> success, Action<Exception> failure)
        {
            var __success = success;
            var __failure = failure;
            var __cancellation = cancellation;
            __enter(() => __advance(__concurrentHello(who, __cancellation, __success, __failure).GetEnumerator()), failure);
        }

        public readonly Guid __ID = Guid.NewGuid();
    }

    [Concurrent(id = "d51c8426-2f61-4157-9b8a-436b1f84231e")]
    public class GoodbyeService : ConcurrentObject
    {
        string _who;
        public GoodbyeService(string who)
        {
            _who = who;
        }

        [Concurrent]
        public string Goodbye(string what)
        {
            return Goodbye(what, default(CancellationToken)).Result;
        }

        private IEnumerable<Expression> __concurrentGoodbye(string what, CancellationToken __cancellation, Action<object> __success, Action<Exception> __failure)
        {
            {
                __dispatch("Goodbye");
                if (__success != null)
                    __success($"Goodbye {what}, goodbye {_who}");
                yield break;
            }
        }

        public Task<string> Goodbye(string what, CancellationToken cancellation)
        {
            var completion = new TaskCompletionSource<string>();
            Action<object> __success = (__res) => completion.SetResult((string)__res);
            Action<Exception> __failure = (__ex) => completion.SetException(__ex);
            var __cancellation = cancellation;
            __enter(() => __advance(__concurrentGoodbye(what, __cancellation, __success, __failure).GetEnumerator()), __failure);
            return completion.Task;
        }

        public void Goodbye(string what, CancellationToken cancellation, Action<object> success, Action<Exception> failure)
        {
            var __success = success;
            var __failure = failure;
            var __cancellation = cancellation;
            __enter(() => __advance(__concurrentGoodbye(what, __cancellation, __success, __failure).GetEnumerator()), failure);
        }

        public readonly Guid __ID = Guid.NewGuid();
    }

    [Concurrent(id = "2696cf63-5aaf-4340-94af-f27f6fce6cb5")]
    [ConcurrentSingleton(id: "df2531ad-1cfe-407e-985c-20e929c0c8d9")]
    public class ProcessingService : ConcurrentObject
    {
        [Concurrent]
        public string Process(string what, GoodbyeService unGreeter)
        {
            return Process(what, unGreeter, default(CancellationToken)).Result;
        }

        private IEnumerable<Expression> __concurrentProcess(string what, GoodbyeService unGreeter, CancellationToken __cancellation, Action<object> __success, Action<Exception> __failure)
        {
            string goodbyeText;
            {
                var __expr1_var = new __expr1
                {
                    Start = (___expr) =>
                    {
                        var __expr = (__expr1)___expr;
                        unGreeter.Goodbye(what, __cancellation, (__res) =>
                        {
                            __expr.goodbyeText = (String)__res;
                            __expr.__op1(true, null, null);
                        }

                        , (__ex) => __expr.__op1(false, null, __ex));
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
                goodbyeText = __expr1_var.goodbyeText;
            }

            {
                __dispatch("Process");
                if (__success != null)
                    __success(what + " then " + goodbyeText);
                yield break;
            }
        }

        public Task<string> Process(string what, GoodbyeService unGreeter, CancellationToken cancellation)
        {
            var completion = new TaskCompletionSource<string>();
            Action<object> __success = (__res) => completion.SetResult((string)__res);
            Action<Exception> __failure = (__ex) => completion.SetException(__ex);
            var __cancellation = cancellation;
            __enter(() => __advance(__concurrentProcess(what, unGreeter, __cancellation, __success, __failure).GetEnumerator()), __failure);
            return completion.Task;
        }

        public void Process(string what, GoodbyeService unGreeter, CancellationToken cancellation, Action<object> success, Action<Exception> failure)
        {
            var __success = success;
            var __failure = failure;
            var __cancellation = cancellation;
            __enter(() => __advance(__concurrentProcess(what, unGreeter, __cancellation, __success, __failure).GetEnumerator()), failure);
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
            public String goodbyeText;
        }

        public readonly Guid __ID = Guid.NewGuid();
    }

    namespace Servers
    {
        [ServerConfiguration]
        public class Default
        {
            public static void Deploy()
            {
            }

            public static void Start()
            {
                HttpServer.Start(url: "http://localhost:1080", identityUrl: "tcp://localhost:5000", threads: 8);
            }

            public static void StartNodes(IEnumerable<Type> commonClasses)
            {
                node1(commonClasses);
                node2(commonClasses);
            }

            public static int NodeCount()
            {
                return 2;
            }

            public static IEnumerable<Type> RemoteTypes()
            {
                return new Type[] { typeof(HelloService), typeof(ProcessingService) };
            }

            public static void node1(IEnumerable<Type> commonClasses)
            {
                var hostedTypes = commonClasses.Union(new Type[] { typeof(HelloService) });
                NetMQNode.Start(localServer: "tcp://localhost:1081", remoteServer: "tcp://localhost:5000", threads: 8, classes: hostedTypes);
            }

            public static void node2(IEnumerable<Type> commonClasses)
            {
                var hostedTypes = commonClasses.Union(new Type[] { typeof(ProcessingService) });
                NetMQNode.Start(localServer: "tcp://localhost:1082", remoteServer: "tcp://localhost:5000", threads: 8, classes: hostedTypes);
            }
        }
    }
}