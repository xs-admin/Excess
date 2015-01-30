using Excess.Compiler;
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


        public ICompilerService<TToken, TNode> GetService<TToken, TNode>()
        {
            return get<ICompilerService<TToken, TNode>>();
        }

        public IDocument<TToken, TNode, TModel> GetDocument<TToken, TNode, TModel>()
        {
            return get<IDocument<TToken, TNode, TModel>>();
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

        public object get(string id)
        {
            return _values[id];
        }

        internal void set(string id, object value)
        {
            _values[id] = value;
        }
    }
}
