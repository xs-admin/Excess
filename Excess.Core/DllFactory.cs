using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Excess.Core
{
    class DllFactory : IDSLFactory
    {
        public bool AddReference(string name, string dll, out string error)
        {
            error = null;

            try
            {
                Assembly assembly = Assembly.LoadFrom(dll);
                Type type = assembly.GetType("DSLPlugin");

                object result = type.InvokeMember("Create", BindingFlags.InvokeMethod | BindingFlags.Static, null, null, null);

                factories_[name] = (IDSLFactory)result;
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
            IDSLFactory result;
            if (factories_.TryGetValue(name, out result))
                return result.create(name);

            return null;
        }

        public IEnumerable<string> supported()
        {
            foreach (IDSLFactory factory in factories_.Values)
            {
                foreach (string dsl in factory.supported())
                    yield return dsl;
            }
        }

        private Dictionary<string, IDSLFactory> factories_ = new Dictionary<string, IDSLFactory>();
    }
}
