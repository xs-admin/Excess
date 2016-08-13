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

    [AttributeUsage(AttributeTargets.Method)]
    public class route : Attribute
    {
        public string id;

        public route(string id)
        {
            this.id = id;
        }
    }

}
