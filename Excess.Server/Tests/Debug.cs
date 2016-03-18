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

    [Concurrent]
    [ConcurrentSingleton]
    class HelloService : ConcurrentObject
    {
        int _times = 0;
        [Concurrent]
        public HelloModel Hello(string who)
        {
            return Hello(who, default(CancellationToken)).Result;
        }

        private static readonly Guid __classID = new Guid("03b8b19b-08cd-489c-b80a-5dc60e3c8021");
        protected readonly Guid __ID = Guid.NewGuid();
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
    }

    [Concurrent]
    class GoodbyeService : ConcurrentObject
    {
        public string Name = "GoodbyeService";
        [Concurrent]
        public string Goodbye(string what)
        {
            return Goodbye(what, default(CancellationToken)).Result;
        }

        private static readonly Guid __classID = new Guid("bfec94b2-1a13-4d01-8177-7387c47b4dfd");
        protected readonly Guid __ID = Guid.NewGuid();
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