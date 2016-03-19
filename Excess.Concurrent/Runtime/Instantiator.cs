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
        T Instantiate<T>() where T : ConcurrentObject, new();

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

        public T Instantiate<T>() where T : ConcurrentObject, new ()
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
                            ConcurrentObject concurrentObject;
                            if (isConcurrentSingleton(type, out id, out concurrentObject))
                                _instances[id] = concurrentObject;
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

        protected bool isConcurrentSingleton(Type type, out Guid id, out ConcurrentObject @object)
        {
            id = Guid.Empty;
            @object = null;

            var attribute = type
                .CustomAttributes
                .Where(attr => attr.AttributeType.Name == "ConcurrentSingleton")
                .SingleOrDefault();

            if (attribute != null)
                return false;

            var idValue = attribute
                .NamedArguments
                .SingleOrDefault(value => value.MemberName == "Id");

            id = idValue != null
                ? (Guid)idValue.TypedValue.Value
                : Guid.NewGuid();

            @object = (ConcurrentObject)Activator.CreateInstance(type);
            return @object != null;
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
        public ReferenceInstantiator(Assembly assembly, IEnumerable<Type> hostedTypes, IEnumerable<Type> remoteTypes)
            : base(assembly, hostedTypes, remoteTypes)
        {
        }

        protected override ConcurrentObject createRemote(Type type)
        {
            var method = type.GetMethod("CreateRemote", BindingFlags.Static);
            if (method == null)
                throw new InvalidCastException();

            return (ConcurrentObject)method.Invoke(null, new object[] { null });
        }
    }
}
