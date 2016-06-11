#line hidden
using System;
using System.Collections.Generic;
using System.Linq;
using Excess.Runtime;
using Ninject;

#line 3
namespace metaprogramming_asp.server
#line 4
{
#line hidden
    [AutoInit]
    public class NinjectExcessModule : Ninject.Modules.NinjectModule
    {
        private class NinjectInstantiator : IInstantiator
        {
            IKernel _kernel = new StandardKernel(new NinjectExcessModule());
            public object Create(Type type)
            {
                return _kernel.GetService(type);
            }
        }

        public static void __init()
        {
            Application.RegisterService<IInstantiator>(new NinjectInstantiator());
        }

        public override void Load()
        {
#line 7
            Bind<ITranspiler>().To<Transpiler>();
#line 8
            Bind<IGraphTranspiler>().To<GraphTranspiler>();
#line hidden
        }
    }
}