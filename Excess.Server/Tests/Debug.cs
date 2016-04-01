using Excess.Concurrent.Runtime;
using Excess.Concurrent.Runtime.Core;
using Middleware;
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

    [Concurrent(id = "c2b308a7-50e7-42ae-bcc3-8adaab507793")]
    [ConcurrentSingleton(id: "12e48bfc-bb85-4e80-b836-21e91a44506a")]
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

    [Concurrent(id = "c22a4f27-4efc-4de5-bd32-daf3aeb02b0d")]
    public class GoodbyeService : ConcurrentObject
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
    }

    namespace Servers
    {
        [ServerConfiguration]
        public class Default
        {
            public void Deploy()
            {
            }

            public void Start()
            {
                Startup.HttpServer.Start(url: "http://localhost:1080", identityUrl: "tcp://localhost:1079", threads: 8);
            }

            public int StartNodes(IList<Type> managedTypes, IDictionary<Guid, IConcurrentObject> managedInstances)
            {
                node1(managedTypes, managedInstances);
                node2(managedTypes, managedInstances);
                return 2;
            }

            public void node1(IList<Type> managedTypes, IDictionary<Guid, IConcurrentObject> managedInstances)
            {
                var hostedTypes = new Type[] { typeof(HelloService) };
                if (managedTypes != null)
                {
                    foreach (var hostedType in hostedTypes)
                        managedTypes.Add(hostedType);
                }

                Startup.NetMQNode.Start(localServer: "tcp://localhost:1081", remoteServer: "tcp://localhost:1079", threads: 8, classes: hostedTypes);
            }

            public void node2(IList<Type> managedTypes, IDictionary<Guid, IConcurrentObject> managedInstances)
            {
                var hostedTypes = new Type[] { typeof(GoodbyeService) };
                if (managedTypes != null)
                {
                    foreach (var hostedType in hostedTypes)
                        managedTypes.Add(hostedType);
                }

                Startup.NetMQNode.Start(localServer: "tcp://localhost:1082", remoteServer: "tcp://localhost:1079", threads: 8, classes: hostedTypes);
            }
        }
    }
}