using System;
using System.Linq;

namespace Excess.Concurrent.Runtime
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class Concurrent : Attribute
    {
        public string id;
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class ConcurrentSingleton : Attribute
    {
        public string id;

        public ConcurrentSingleton(string id)
        {
            this.id = id;
        }
    }

    public static class TypeExtensions
    {
        public static bool IsConcurrent(this Type type)
        {
            var attribute = type
                .CustomAttributes
                .Where(attr => attr.AttributeType.Name == "Concurrent")
                .SingleOrDefault();

            return attribute != null && attribute.ConstructorArguments.Count == 1;
        }

        public static bool IsConcurrentSingleton(this Type type, out Guid id)
        {
            var attribute = type
                .CustomAttributes
                .Where(attr => attr.AttributeType.Name == "ConcurrentSingleton")
                .SingleOrDefault();

            if (attribute != null && attribute.ConstructorArguments.Count == 1)
            {
                id = Guid.Parse((string)attribute.ConstructorArguments[0].Value);
                return true;
            }

            id = Guid.Empty;
            return false;
        }
    }
}
