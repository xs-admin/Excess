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
    struct HelloModel
    {
        public string Greeting;
        public int Times;
        public GoodbyeService Goodbye;
    }

    [Concurrent(Id = new Guid("e9a414f1-e073-482b-b171-0127ce672592"))]
    [ConcurrentSingleton(Id = new Guid("aaffba6f-c953-4951-9f10-5077e23cd695"))]
    class HelloService : ConcurrentObject
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

        public static CreateRemote(IIdentityServer server)
        {
            return new __remoteHelloService(server);
        }

        class __remoteHelloService : ConcurrentObject
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
        }
    }

    [Concurrent(Id = new Guid("fc7534f5-403d-45a4-a6cd-2f1a11bb6313"))]
    class GoodbyeService : ConcurrentObject
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

        public static CreateRemote(IIdentityServer server)
        {
            return new __remoteGoodbyeService(server);
        }

        class __remoteGoodbyeService : ConcurrentObject
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
                instantiator = instantiator ?? new AssemblyInstantiator(this.GetType().Assembly);
                Startup.HttpServer.Start(url: "http://localhost:1080", identityUrl: "tcp://localhost:1079", threads: 8, classes: instantiator.GetConcurrentClasses(), instances: instantiator.GetConcurrentInstances(except: new Type[] { typeof(HelloService), typeof(GoodbyeService) }));
            }

            public void StartNodes(IInstantiator instantiator)
            {
                instantiator = instantiator ?? new AssemblyInstantiator(this.GetType().Assembly);
                node1(instantiator);
                node2(instantiator);
            }

            public void node1(IInstantiator instantiator)
            {
                instantiator = instantiator ?? new AssemblyInstantiator(this.GetType().Assembly);
                Startup.NetMQNode.Start(url: "tcp://localhost:1081", identityUrl: "tcp://localhost:1079", threads: 8, classes: instantiator.GetConcurrentClasses(), instances: instantiator.GetConcurrentInstances(only: new Type[] { typeof(HelloService) }));
            }

            public void node2(IInstantiator instantiator)
            {
                instantiator = instantiator ?? new AssemblyInstantiator(this.GetType().Assembly);
                Startup.NetMQNode.Start(url: "tcp://localhost:1082", identityUrl: "tcp://localhost:1079", threads: 8, classes: instantiator.GetConcurrentClasses(), instances: instantiator.GetConcurrentInstances(only: new Type[] { typeof(GoodbyeService) }));
            }
        }
    }
}