using Excess.Concurrent.Runtime;
using Middleware;
using Middleware.NetMQ;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SomeNS
{
    public struct HelloModel
    {
        public string Greeting;
        public int Times;
        public GoodbyeService Goodbye;
    }

    [Concurrent(id = "673c7447-4885-40ad-8f2c-43976bf6af01")]
    [ConcurrentSingleton(id: "2900902e-c73e-4d7b-acc6-bf2a8f4780e1")]
    public class HelloService : ConcurrentObject, IHelloService
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
                    __success(new HelloModel { Greeting = "greetings, " + who, Times = _times++, Goodbye = spawn<GoodbyeService>() });
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

        public static IHelloService CreateRemote(Action<Guid, string, string, Action<string>> dispatch, Func<object, string> serialize, Func<string, object> deserialize)
        {
            return new __remoteHelloService { Dispatch = dispatch, Serialize = serialize, Deserialize = deserialize };
        }

        public class __remoteHelloService : ConcurrentObject, IHelloService
        {
            [Concurrent]
            public HelloModel Hello(string who)
            {
                return Hello(who, default(CancellationToken)).Result;
            }

            private IEnumerable<Expression> __concurrentHello(string who, CancellationToken __cancellation, Action<object> __success, Action<Exception> __failure)
            {
                var __expr = new Expression();
                Dispatch(Id, "Hello", Serialize(new
                {
                    who = who,
                    __cancellation = __cancellation,
                    __success = __success,
                    __failure = __failure
                }

                ), __response =>
                {
                    var __res = Deserialize(__response);
                    if (__res is Exception)
                        __failure(__res as Exception);
                    else
                        __success((HelloModel)__res);
                }

                );
                yield return __expr;
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

            public Guid Id
            {
                get;
                set;
            }

            public Action<Guid, string, string, Action<string>> Dispatch
            {
                get;
                set;
            }

            public Func<object, string> Serialize
            {
                get;
                set;
            }

            public Func<string, object> Deserialize
            {
                get;
                set;
            }
        }
    }

    [Concurrent(id = "6d34b542-c5ab-465a-82fc-1bca4897705d")]
    public class GoodbyeService : ConcurrentObject, IGoodbyeService
    {
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
                    __success("Goodbye " + what);
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

        public static IGoodbyeService CreateRemote(Action<Guid, string, string, Action<string>> dispatch, Func<object, string> serialize, Func<string, object> deserialize)
        {
            return new __remoteGoodbyeService { Dispatch = dispatch, Serialize = serialize, Deserialize = deserialize };
        }

        public class __remoteGoodbyeService : ConcurrentObject, IGoodbyeService
        {
            [Concurrent]
            public string Goodbye(string what)
            {
                return Goodbye(what, default(CancellationToken)).Result;
            }

            private IEnumerable<Expression> __concurrentGoodbye(string what, CancellationToken __cancellation, Action<object> __success, Action<Exception> __failure)
            {
                var __expr = new Expression();
                Dispatch(Id, "Goodbye", Serialize(new
                {
                    what = what,
                    __cancellation = __cancellation,
                    __success = __success,
                    __failure = __failure
                }

                ), __response =>
                {
                    var __res = Deserialize(__response);
                    if (__res is Exception)
                        __failure(__res as Exception);
                    else
                        __success((string)__res);
                }

                );
                yield return __expr;
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

            public Guid Id
            {
                get;
                set;
            }

            public Action<Guid, string, string, Action<string>> Dispatch
            {
                get;
                set;
            }

            public Func<object, string> Serialize
            {
                get;
                set;
            }

            public Func<string, object> Deserialize
            {
                get;
                set;
            }
        }
    }

    namespace Servers
    {
        [ServerConfiguration]
        public class Default
        {
            public void Deploy()
            {
            }

            public void Start(IInstantiator instantiator)
            {
                instantiator = instantiator ?? new ReferenceInstantiator(this.GetType().Assembly, hostedTypes: null, remoteTypes: new Type[] { typeof(HelloService), typeof(GoodbyeService) }, dispatch: null);
                Startup.HttpServer.Start(url: "http://localhost:1080", identityUrl: "tcp://localhost:1079", threads: 8, classes: instantiator.GetConcurrentClasses(), instances: instantiator.GetConcurrentInstances());
            }

            public void StartNodes()
            {
                node1(null);
                node2(null);
            }

            public void node1(IInstantiator instantiator)
            {
                instantiator = instantiator ?? new ReferenceInstantiator(this.GetType().Assembly, hostedTypes: new Type[] { typeof(HelloService) }, remoteTypes: null, dispatch: null);
                Startup.NetMQNode.Start(url: "tcp://localhost:1081", identityUrl: "tcp://localhost:1079", threads: 8, classes: instantiator.GetConcurrentClasses(), instances: instantiator.GetConcurrentInstances());
            }

            public void node2(IInstantiator instantiator)
            {
                instantiator = instantiator ?? new ReferenceInstantiator(this.GetType().Assembly, hostedTypes: new Type[] { typeof(GoodbyeService) }, remoteTypes: null, dispatch: null);
                Startup.NetMQNode.Start(url: "tcp://localhost:1082", identityUrl: "tcp://localhost:1079", threads: 8, classes: instantiator.GetConcurrentClasses(), instances: instantiator.GetConcurrentInstances());
            }
        }
    }

    public interface IHelloService
    {
        HelloModel Hello(string who);
        Task<HelloModel> Hello(string who, CancellationToken cancellation);
        void Hello(string who, CancellationToken cancellation, Action<object> success, Action<Exception> failure);
    }

    public interface IGoodbyeService
    {
        string Goodbye(string what);
        Task<string> Goodbye(string what, CancellationToken cancellation);
        void Goodbye(string what, CancellationToken cancellation, Action<object> success, Action<Exception> failure);
    }
}