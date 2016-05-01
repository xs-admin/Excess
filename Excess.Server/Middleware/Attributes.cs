using System;

namespace Middleware
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ServerConfiguration : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class Service : Attribute
    {
    }
}
