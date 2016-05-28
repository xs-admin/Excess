#line 1 "C:\dev\Excess\Excess.Websites\metaprogramming\server\Dependencies.xs"
using System;
#line hidden

using System.Collections.Generic;
using System.Linq;
using Excess.Runtime;
using Ninject;


#line 3
namespace metaprogramming.server

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
            Bind<
#line 7
ITranspiler>().To<Transpiler>();
#line hidden

        }
    }

#line 9
}