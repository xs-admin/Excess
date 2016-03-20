using Newtonsoft.Json.Linq;
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
        T Instantiate<T>(params object[] args) where T : ConcurrentObject, new();
        ConcurrentObject Instantiate(string type, object[] args);

        IEnumerable<Type> GetConcurrentClasses();
        IEnumerable<KeyValuePair<Guid, ConcurrentObject>> GetConcurrentInstances();
    }

    public abstract class BaseInstantiator : IInstantiator
    {
        protected Type[] _hostedTypes;
        protected Type[] _remoteTypes;
        public BaseInstantiator(IEnumerable<Type> hostedTypes, IEnumerable<Type> remoteTypes)
        {
            _hostedTypes = hostedTypes != null
                ? hostedTypes.ToArray()
                : new Type[] { };

            _remoteTypes = hostedTypes != null
                ? hostedTypes.ToArray()
                : new Type[] { };
        }

        public T Instantiate<T>(params object[] args) where T : ConcurrentObject, new ()
        {
            var type = typeof(T);
            if (_hostedTypes.Contains(type))
                return new T();

            if (_remoteTypes.Contains(type))
                return (T)createRemote(type);

            var allClasses = GetConcurrentClasses();
            if (allClasses.Contains(type))
                return new T();

            return null;
        }

        public virtual ConcurrentObject Instantiate(string type, object[] args)
        {
            throw new NotImplementedException();
        }

        protected Dictionary<Guid, ConcurrentObject> _instances;
        public IEnumerable<KeyValuePair<Guid, ConcurrentObject>> GetConcurrentInstances()
        {
            if (_instances == null)
            {
                lock (this)
                {
                    if (_instances == null)
                    {
                        _instances = new Dictionary<Guid, ConcurrentObject>();
                        foreach (var type in GetConcurrentClasses())
                        {
                            if (!_hostedTypes.Contains(type))
                                continue;

                            Guid id;
                            if (isConcurrentSingleton(type, out id))
                            {
                                var @object = (ConcurrentObject)Activator.CreateInstance(type);
                                _instances[id] = @object;
                            }
                        }
                    }
                }
            }

            return _instances;
        }

        public abstract IEnumerable<Type> GetConcurrentClasses();
        protected abstract ConcurrentObject createRemote(Type type); 

        protected bool isConcurrent(Type type)
        {
            return type
                .CustomAttributes
                .Any(attr => attr.AttributeType.Name == "Concurrent");
        }

        protected bool isConcurrentSingleton(Type type, out Guid id)
        {
            id = Guid.Empty;

            var attribute = type
                .CustomAttributes
                .Where(attr => attr.AttributeType.Name == "ConcurrentSingleton")
                .SingleOrDefault();

            if (attribute == null || attribute.ConstructorArguments.Count != 1)
                return false;

            id = Guid.Parse((string)attribute.ConstructorArguments[0].Value);
            return true;
        }
    }

    public abstract class AssemblyInstantiator : BaseInstantiator
    {
        List<Type> _types;
        public AssemblyInstantiator(Assembly assembly, IEnumerable<Type> hostedTypes, IEnumerable<Type> remoteTypes)
            : base(hostedTypes, remoteTypes)
        {
            _types = new List<Type>();
            foreach (var type in assembly.GetTypes())
            {
                if (isConcurrent(type))
                    _types.Add(type);
            }
        }

        public override IEnumerable<Type> GetConcurrentClasses()
        {
            return _types;
        }
    }

    public class ReferenceInstantiator : AssemblyInstantiator
    {
        public ReferenceInstantiator(Assembly assembly, 
            IEnumerable<Type> hostedTypes, 
            IEnumerable<Type> remoteTypes,
            Action<Guid, string, string, Action<string>> dispatch) : base(assembly, hostedTypes, remoteTypes)
        {
            Dispatch = dispatch;
        }

        public Action<Guid, string, string, Action<string>> Dispatch { get; set; }

        protected override ConcurrentObject createRemote(Type type)
        {
            var method = type.GetMethod("CreateRemote", BindingFlags.Static);
            if (method == null)
                throw new InvalidCastException();

            Debug.Assert(Dispatch != null);
            return (ConcurrentObject)method.Invoke(null, new object[] {
                Dispatch,
                (Func<dynamic, string>)Serialize,
                (Func<string, dynamic>)Deserialize,
            });
        }

        private static string Serialize(dynamic obj)
        {
            return JObject
                .FromObject(obj)
                .ToString();
        }

        private static dynamic Deserialize(string text)
        {
            return JObject.Parse(text);
        }
    }
}
