using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess.Compiler
{
    public class Scope : DynamicObject
    {
        public Scope()
        {
        }

        //DynamicObject
        Dictionary<string, object> _values = new Dictionary<string, object>();
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            _values.TryGetValue(binder.Name, out result);
            return true;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            _values[binder.Name] = value;
            return true;
        }

        public T get<T>() where T : class
        {
            string thash = typeof(T).GetHashCode().ToString();
            return _values[thash] as T;
        }

        public T get<T>(string id) where T : class
        {
            return _values[id] as T;
        }

        internal void set(string id, object value)
        {
            _values[id] = value;
        }
    }
}
