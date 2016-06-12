using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess.Runtime
{
    //very prototype
    public sealed class __Scope
    {
        IInstantiator _instantiator;
        __Scope _parent;
        Dictionary<string, object> _bindings = new Dictionary<string, object>();
        public __Scope(IInstantiator instantiator)
        {
            _instantiator = instantiator;
        }

        public __Scope(__Scope parent)
        {
            _parent = parent;
        }

        private string key<T>() => typeof(T).GetHashCode().ToString();

        public T get<T>() where T : class
        {
            return (T)get(key<T>()) ?? set<T>();
        }

        public object get(string key)
        { 
            object result;
            if (_bindings.TryGetValue(key, out result))
                return result;

            return _parent?.get(key);
        }

        public T set<T>()
        {
            var value = (T)_instantiator?.Create(typeof(T));
            if (value == null)
                throw new InvalidOperationException($"type not instantiable: {typeof(T)}");

            set(key<T>(), value);
            return value;
        }

        void set(string key, object value)
        {
            if (_bindings.ContainsKey(key))
                throw new InvalidOperationException($"duplicate binding: {value?.GetType().Name}");

            _bindings[key] = value;
        }
    }
}
