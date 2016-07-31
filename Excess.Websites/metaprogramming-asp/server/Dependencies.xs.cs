#line 1 "C:\dev\Excess\Excess.Websites\metaprogramming-asp\server\Dependencies.xs"
#line 2
using demo_transpiler;
#line hidden
using System;
using System.Collections.Generic;
using System.Linq;
using Excess.Runtime;
using Ninject;

#line 4
namespace metaprogramming_asp.server
#line 5
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

        public static void __init(__Scope scope)
        {
            scope.set<IInstantiator>(new NinjectInstantiator());
        }

        public override void Load()
        {
            Bind<ITranspiler>().To<Transpiler>();
            Bind<IGraphTranspiler>().To<GraphTranspiler>();
        }
    }
}