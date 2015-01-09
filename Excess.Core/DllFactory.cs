using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Excess.Core
{
    public class DllFactory : IDSLFactory
    {
        public bool AddReference(string dll, out string error)
        {
            error = null;

            try
            {
                Assembly assembly = Assembly.LoadFrom(dll);
                Type type = assembly.GetType("DSLPlugin");
                object result = type.InvokeMember("Create", BindingFlags.InvokeMethod | BindingFlags.Static, null, null, null);
                factories_.Add((IDSLFactory)result);
            }
            catch (Exception e)
            {
                error = e.Message;
                return false;
            }

            return true;
        }

        public bool AddReference(Assembly assembly, out string error)
        {
            error = null;
            try
            {
                Type type = assembly.GetType("DSLPlugin");

                var Create = type.GetMethod("Create", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
                var result = Create.Invoke(null, new object[] { });

                factories_.Add((IDSLFactory)result);
            }
            catch (Exception e)
            {
                error = e.Message;
                return false;
            }

            return true;
        }

        public IDSLHandler create(string name)
        {
            foreach (var factory in factories_)
            {
                var result = factory.create(name);
                if (result != null)
                    return result;
            }

            return null;
        }

        public IEnumerable<string> supported()
        {
            foreach (IDSLFactory factory in factories_)
            {
                foreach (string dsl in factory.supported())
                    yield return dsl;
            }
        }

        private List<IDSLFactory> factories_ = new List<IDSLFactory>();
    }
}
