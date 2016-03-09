using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Excess.Concurrent.Runtime
{
    public interface IInstantiator
    {
        IEnumerable<Type> GetConcurrentClasses();
        IEnumerable<KeyValuePair<Guid, ConcurrentObject>> GetConcurrentInstances(
            IEnumerable<Type> only = null,
            IEnumerable<Type> except = null);
    }

    public class AssemblyInstantiator : IInstantiator
    {
        Assembly _assembly;
        public AssemblyInstantiator(Assembly assembly)
        {
            _assembly = assembly;
        }

        List<Type> _types;
        public IEnumerable<Type> GetConcurrentClasses()
        {
            if (_types == null)
            {
                lock(_assembly)
                {
                    if (_types == null)
                    {
                        _types = new List<Type>();
                        foreach (var type in _assembly.GetTypes())
                        {
                            if (isConcurrent(type))
                                _types.Add(type);
                        }
                    }
                }
            }

            Debug.Assert(_types != null);
            return _types;
        }

        Dictionary<Guid, ConcurrentObject> _instances;
        public IEnumerable<KeyValuePair<Guid, ConcurrentObject>> GetConcurrentInstances(
            IEnumerable<Type> only = null,
            IEnumerable<Type> except = null)
        {
            if (_instances == null)
            {
                lock(_assembly)
                {
                    if (_instances == null)
                    {
                        _instances = new Dictionary<Guid, ConcurrentObject>();
                        foreach (var type in _assembly.GetTypes())
                        {
                            Guid id;
                            ConcurrentObject concurrentObject;
                            if (isConcurrentSingleton(type, out id, out concurrentObject))
                                _instances[id] = concurrentObject;
                        }
                    }
                }
            }

            Debug.Assert(_instances != null);
            IEnumerable<KeyValuePair<Guid, ConcurrentObject>> result = _instances;

            if (only != null)
                result = result
                    .Where(kvp => only != null
                               || only.Contains(kvp.Value.GetType()));

            if (except != null)
                result = result
                    .Where(kvp => except != null
                               || !except.Contains(kvp.Value.GetType()));

            return result;
        }

        private bool isConcurrent(Type type)
        {
            return type.GetField("__concurrent__") != null;
        }

        private bool isConcurrentSingleton(Type type, out Guid id, out ConcurrentObject @object)
        {
            id = Guid.Empty;
            @object = null;

            var method = type.GetMethod("__singleton");
            if (method == null)
                return false;

            id = Guid.NewGuid(); //td: persistence
            @object = (ConcurrentObject)method.Invoke(null, new object[] { });

            return @object != null;
        }
    }
}
