using Excess.Concurrent.Runtime;
using Middleware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SomeNS
{
    [Concurrent]
    class HelloService : ConcurrentObject
    {
        public string Hello(string what)
        {
            return Hello(what, default(CancellationToken)).Result;
        }

        private IEnumerable<Expression> __concurrentHello(string what, CancellationToken __cancellation, Action<object> __success, Action<Exception> __failure)
        {
            {
                __dispatch("Hello");
                if (__success != null)
                    __success("Hello " + what);
                yield break;
            }
        }

        public Task<string> Hello(string what, CancellationToken cancellation)
        {
            var completion = new TaskCompletionSource<string>();
            Action<object> __success = (__res) => completion.SetResult((string)__res);
            Action<Exception> __failure = (__ex) => completion.SetException(__ex);
            var __cancellation = cancellation;
            __enter(() => __advance(__concurrentHello(what, __cancellation, __success, __failure).GetEnumerator()), __failure);
            return completion.Task;
        }

        public void Hello(string what, CancellationToken cancellation, Action<object> success, Action<Exception> failure)
        {
            var __success = success;
            var __failure = failure;
            var __cancellation = cancellation;
            __enter(() => __advance(__concurrentHello(what, __cancellation, __success, __failure).GetEnumerator()), failure);
        }
    }

    [Concurrent]
    class GoodbyeService : ConcurrentObject
    {
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
    }

    namespace Servers
    {
        public class Default
        {
            public void Deploy()
            {
            }

            public void Start(IInstantiator instantiator)
            {
                instantiator = instantiator ?? new AssemblyInstantiator(this.GetType().Assembly);
                Startup.HttpServer.Start(url: "http://localhost:1080", threads: 8, classes: instantiator.GetConcurrentClasses(), instances: instantiator.GetConcurrentInstances(except: new Type[] { typeof(HelloService), typeof(GoodbyeService) }));
            }

            private bool __ConfigClass__ = true;
            public void node1(IInstantiator instantiator)
            {
                instantiator = instantiator ?? new AssemblyInstantiator(this.GetType().Assembly);
                Startup.NetMQ_RequestResponseServer.Start(url: "http://localhost:1081", threads: 8, classes: instantiator.GetConcurrentClasses(), instances: instantiator.GetConcurrentInstances(only: new Type[] { typeof(HelloService) }));
            }

            public void node2(IInstantiator instantiator)
            {
                instantiator = instantiator ?? new AssemblyInstantiator(this.GetType().Assembly);
                Startup.NetMQ_RequestResponseServer.Start(url: "http://localhost:1082", threads: 8, classes: instantiator.GetConcurrentClasses(), instances: instantiator.GetConcurrentInstances(only: new Type[] { typeof(GoodbyeService) }));
            }
        }
    }
}