using Microsoft.CodeAnalysis.CSharp.Syntax;
using Excess.Compiler.Roslyn;

namespace NInjector
{
    public static class Templates
    {
        public static Template Bind = Template.ParseStatement("Bind<__0>().To<__1>();");
        public static ClassDeclarationSyntax Module = Template.Parse(@"
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
                }
            }").Get<ClassDeclarationSyntax>();
    }
}
