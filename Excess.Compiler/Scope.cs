using Excess.Compiler;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using System.Diagnostics;

namespace Excess.Compiler
{
    public class Scope : DynamicObject
    {
        Scope _parent;
        public Scope(Scope parent)
        {
            _parent = parent;
        }


        public ICompilerService<TToken, TNode, TModel> GetService<TToken, TNode, TModel>()
        {
            var result = get<ICompilerService<TToken, TNode, TModel>>();
            if (result == null && _parent != null)
                result = _parent.GetService<TToken, TNode, TModel>();

            return result;
        }

        public IDocument<TToken, TNode, TModel> GetDocument<TToken, TNode, TModel>()
        {
            var result = get<IDocument<TToken, TNode, TModel>>();
            if (result == null && _parent != null)
                result = _parent.GetDocument<TToken, TNode, TModel>();

            return result;
        }

        private class UniqueIdHolder
        {
            public int Value { get; set; }
        }

        public void AddUniqueId(string seed)
        {
            var key = "__uid_" + seed;
            var result = get<UniqueIdHolder>(key);
            if (result != null)
                throw new ArgumentException("seed");

            set(key, new UniqueIdHolder { Value = 0 });
        }

        public string GetUniqueId(string seed)
        {
            var key = "__uid_" + seed;
            var result = get<UniqueIdHolder>(key);
            if (result == null)
            {
                result = new UniqueIdHolder { Value = 1 };
                set(key, result);
            }

            return seed + result.Value++;
        }

        public Scope CreateScope<TToken, TNode, TModel>(TNode node)
        {
            var service = GetService<TToken, TNode, TModel>();
            if (service == null)
                return null;

            int id = service.GetExcessId(node);
            if (id < 0)
                return null;

            IDictionary<int, Scope> repository = GetScopeRepository();
            Debug.Assert(repository != null);

            Scope result;
            if (!repository.TryGetValue(id, out result))
            {
                result = new Scope(this); //td: find parent scope
                repository[id] = result;
            }

            return result;
        }

        public Scope parent()
        {
            return _parent;
        }

        public Scope GetScope<TToken, TNode, TModel>(TNode node)
        {
            var service = GetService<TToken, TNode, TModel>();
            if (service == null)
                return null;

            int id = service.GetExcessId(node);
            if (id < 0)
                return null;

            IDictionary<int, Scope> repository = GetScopeRepository();
            Debug.Assert(repository != null);

            Scope result;
            if (repository.TryGetValue(id, out result))
                return result;

            return null;
        }

        private IDictionary<int, Scope> GetScopeRepository()
        {
            var result = get<IDictionary<int, Scope>>();
            if (result == null && _parent != null)
                result = _parent.GetScopeRepository();

            return result;
        }

        public ICollection<string> GetKeywords()
        {
            return (ICollection<string>)get("keywords");
        }

        public ICollection<string> AddKeywords(params string[] values)
        {
            var keywords = get<ICollection<string>>("keywords");
            if (keywords == null)
            {
                keywords = new List<string>();
                set("keywords", keywords);
            }

            foreach(var value in values)
                keywords.Add(value);

            return keywords;
        }

        //DynamicObject
        public dynamic context()
        {
            return this;
        }

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
            return get<T>(thash);
        }

        public T get<T>(string id) where T : class
        {
            object result;
            if (_values.TryGetValue(id, out result))
                return (T)result;

            return default(T);
        }

        public object get(string id)
        {
            object result;
            if (_values.TryGetValue(id, out result))
                return result;

            return null;
        }

        public T find<T>() where T : class
        {
            string thash = typeof(T).GetHashCode().ToString();
            return find<T>(thash);
        }

        public T find<T>(string id) where T : class
        {
            object result;
            if (_values.TryGetValue(id, out result))
                return (T)result;

            return _parent != null? _parent.find<T>(id) : default(T);
        }

        public object find(string id)
        {
            object result;
            if (_values.TryGetValue(id, out result))
                return result;

            return _parent != null ? _parent.find(id) : null;
        }

        public void set(string id, object value)
        {
            _values[id] = value;
        }

        public void set<T>(T t) where T : class
        {
            string id = typeof(T).GetHashCode().ToString();
            _values[id] = t;
        }

        public bool has(string id)
        {
            return _values.ContainsKey(id);
        }
    }
}
