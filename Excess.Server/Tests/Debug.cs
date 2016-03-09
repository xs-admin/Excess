using Excess.Concurrent.Runtime;
using Middleware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SomeNS
{
    //public class SomeServer
    //{
    //    public void Deploy()
    //    {
    //    }

    //    public void Start(IInstantiator instantiator)
    //    {
    //        instantiator = instantiator ?? new AssemblyInstantiator(this.GetType().Assembly);

    //        Startup
    //            .HttpServer
    //            .Start("http://*.1080", 8, 
    //                instantiator.GetConcurrentClasses(), 
    //                instantiator.GetConcurrentInstances(
    //                    except: new Type[] 
    //                    {
    //                        typeof(SomeService)
    //                    }), 
    //                    nodes: new IConcurrentNode[] 
    //                    {
    //                    });
    //    }

    //    public void someNode(IInstantiator instantiator)
    //    {
    //        Startup
    //            .NetMQ_RequestResponse
    //            .Start("http://*.2080", 25, 
    //                instantiator.GetConcurrentClasses(), 
    //                instances: instantiator.GetConcurrentInstances(
    //                    only: new Type[] 
    //                    {
    //                        typeof(SomeService)
    //                    }));
    //    }
    //}
}