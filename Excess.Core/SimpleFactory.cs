using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess.Core
{
    public class SimpleFactory : IDSLFactory
    {
        public SimpleFactory Add<T>(string name) where T : new()
        {
            items_.Add(name, () => (IDSLHandler)new T());
            return this;
        }

        public IDSLHandler create(string name)
        {
            Func<IDSLHandler> handler = null;
            if (items_.TryGetValue(name, out handler))
                return handler();

            return null;
        }

        public IEnumerable<string> supported()
        {
            return items_.Keys;
        }

        private Dictionary<string, Func<IDSLHandler>> items_ = new Dictionary<string, Func<IDSLHandler>>();
    }
}
