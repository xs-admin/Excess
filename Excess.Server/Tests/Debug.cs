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

    [Concurrent(id = "0e189d19-7d7f-4c0b-8189-f40d9db579f0")]
    [ConcurrentSingleton(id: "a57a767a-ea2a-413f-be74-146eeb6aa453")]
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

        public static IHelloService CreateRemote(Action<string, Action<string>> dispatch)
        {
            var result = new __remoteHelloService();
            result.Dispatch = dispatch;
            return result;
        }

        public class __remoteHelloService : ConcurrentObject, IHelloService
        {
            int _times = 0;
            [Concurrent]
            public HelloModel Hello(string who)
            {
                return Hello(who, default(CancellationToken)).Result;
            }

            private IEnumerable<Expression> __concurrentHello(string who, CancellationToken __cancellation, Action<object> __success, Action<Exception> __failure)
            {
                _server.dispatch(_id, "__concurrentHello", JObject.FromObject(new
                {
                    who = who,
                    __cancellation = __cancellation,
                    __success = __success,
                    __failure = __failure
                }

                ), response =>
                {
                    var json = JObject.Parse(response);
                    __success((HelloModel)json.__res);
                }

                );
                yield break;
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

            public Action<string, Action<string>> Dispatch
            {
                get;
                set;
            }
        }
    }

    [Concurrent(id = "9b975cc9-e498-42e0-a888-1f46f09772cb")]
    public class GoodbyeService : ConcurrentObject, IGoodbyeService
    {
        public string Name = "GoodbyeService";
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

        public static IGoodbyeService CreateRemote(Action<string, Action<string>> dispatch)
        {
            var result = new __remoteGoodbyeService();
            result.Dispatch = dispatch;
            return result;
        }

        public class __remoteGoodbyeService : ConcurrentObject, IGoodbyeService
        {
            public string Name = "GoodbyeService";
            [Concurrent]
            public string Goodbye(string what)
            {
                return Goodbye(what, default(CancellationToken)).Result;
            }

            private IEnumerable<Expression> __concurrentGoodbye(string what, CancellationToken __cancellation, Action<object> __success, Action<Exception> __failure)
            {
                _server.dispatch(_id, "__concurrentGoodbye", JObject.FromObject(new
                {
                    what = what,
                    __cancellation = __cancellation,
                    __success = __success,
                    __failure = __failure
                }

                ), response =>
                {
                    var json = JObject.Parse(response);
                    __success((string)json.__res);
                }

                );
                yield break;
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

            public Action<string, Action<string>> Dispatch
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
                instantiator = instantiator ?? new ReferenceInstantiator(this.GetType().Assembly, hostedTypes: null, remoteTypes: new Type[] { typeof(HelloService), typeof(GoodbyeService) });
                Startup.HttpServer.Start(url: "http://localhost:1080", identityUrl: "tcp://localhost:1079", threads: 8, classes: instantiator.GetConcurrentClasses(), instances: instantiator.GetConcurrentInstances());
            }

            public void StartNodes(IInstantiator instantiator)
            {
                node1(instantiator);
                node2(instantiator);
            }

            public void node1(IInstantiator instantiator)
            {
                instantiator = instantiator ?? new ReferenceInstantiator(this.GetType().Assembly, hostedTypes: new Type[] { typeof(HelloService) }, remoteTypes: null);
                Startup.NetMQNode.Start(url: "tcp://localhost:1081", identityUrl: "tcp://localhost:1079", threads: 8, classes: instantiator.GetConcurrentClasses(), instances: instantiator.GetConcurrentInstances());
            }

            public void node2(IInstantiator instantiator)
            {
                instantiator = instantiator ?? new ReferenceInstantiator(this.GetType().Assembly, hostedTypes: new Type[] { typeof(GoodbyeService) }, remoteTypes: null);
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