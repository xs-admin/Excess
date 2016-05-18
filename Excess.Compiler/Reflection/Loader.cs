using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using Excess.Compiler.Attributes;

namespace Excess.Compiler.Reflection
{
    using LoaderProperties = Scope;

    public class Loader<TCompiler, TCompilationAnalysis>
    {
        public static Action<TCompiler, LoaderProperties> CreateFrom(Assembly assembly, out string extensionName, out Action<TCompilationAnalysis> compilation, string flavor = null, Func<string, string> flavorFunction = null)
        {
            return Create(assembly.ExportedTypes, out extensionName, out compilation, flavor, flavorFunction);
        }

        public static Action<TCompiler, LoaderProperties> Create(IEnumerable<Type> extensionTypes, out string extensionName, out Action<TCompilationAnalysis> compilation, string flavor = null, Func<string, string> flavorFunction = null)
        {
            compilation = null;
            var types = extensionTypes
                .Where(type => type
                    .CustomAttributes
                    .Any(attr => attr.AttributeType == typeof(Extension)));

            extensionName = null;
            if (!types.Any())
                return null;

            //calculate the name of the extension
            foreach (var type in types)
            {
                var name = type.CustomAttributes
                    .Single(attr => attr.AttributeType == typeof(Extension))
                    .ConstructorArguments
                    .Single()
                    .Value as string;

                if (extensionName == null)
                    extensionName = name;
                else if (name != null && extensionName != name)
                    throw new InvalidOperationException("multiple names in an extension");
            }

            if (extensionName == null)
                throw new InvalidOperationException("nameless extension");

            var flavors = types
                .SelectMany(type => type
                    .GetMethods()
                    .Where(mthd => mthd
                        .CustomAttributes
                        .Any(attr => attr.AttributeType == typeof(Flavor))));

            if (flavor == null && flavorFunction != null)
                flavor = flavorFunction(extensionName);

            var method = flavors
                .Where(mthd => mthd.Name == (flavor ?? "Default"))
                .FirstOrDefault();

            var compilationMethod = null as MethodInfo;
            if (method == null)
            {
                foreach (var type in types)
                {
                    method = type.GetMethods()
                        .FirstOrDefault(mthd => mthd.Name == "Apply");

                    if (method != null)
                    {
                        compilationMethod = type.GetMethods()
                            .FirstOrDefault(mthd => mthd.Name == "Compilation");

                        break;
                    }
                }
            }
            else compilationMethod = flavors
                .Where(mthd => mthd.Name == (flavor ?? "Default") + "Compilation")
                .FirstOrDefault();

            if (method == null)
                return null;

            if (compilationMethod != null)
                compilation = (analysys) => compilationMethod.Invoke(null, new object[] { analysys });

            return (compiler, props) =>
            {
                if (method.GetParameters().Length == 1)
                    method.Invoke(null, new object[] { compiler });
                else if (method.GetParameters().Length == 2)
                    method.Invoke(null, new object[] { compiler, props });
                else
                    throw new InvalidOperationException("invalid extension method");
            };
        }

        public static void Apply(IEnumerable<Type> types, TCompiler compiler, out string extensionName, out Action<TCompilationAnalysis> compilation, string flavor = null, LoaderProperties props = null)
        {
            Create(types, out extensionName, out compilation, flavor)
                ?.Invoke(compiler, props);
        }
    }
}
