using System;

namespace Excess.Server.Middleware
{
    [AttributeUsage(AttributeTargets.Class)]
    public class Service : Attribute
    {
        public string id;

        public Service(string id)
        {
            this.id = id;
        }
    }
}
