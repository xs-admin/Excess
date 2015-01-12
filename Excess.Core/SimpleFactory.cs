using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess.Core
{
    public class SimpleFactory : IDSLFactory
    {
        private Dictionary<string, Func<IDSLHandler>> items_ = new Dictionary<string, Func<IDSLHandler>>();
        public SimpleFactory Add<T>(string name) where T : new()
        {
            items_.Add(name, () => (IDSLHandler)new T());
            return this;
        }

        private List<IDSLFactory> _factories = new List<IDSLFactory>();
        public SimpleFactory AddFactory(IDSLFactory factory)
        {
            _factories.Add(factory);
            return this;
        }

        public IDSLHandler create(string name)
        {
            Func<IDSLHandler> handler = null;
            if (items_.TryGetValue(name, out handler))
                return handler();

            foreach (var factory in _factories)
            {
                var result = factory.create(name);
                if (result != null)
                    return result;
            }
            return null;
        }

        public IEnumerable<string> supported()
        {
            foreach (var item in items_.Keys)
                yield return item;

            foreach (var factory in _factories)
            {
                foreach (var fs in factory.supported())
                    yield return fs;
            }
        }
    }
}
