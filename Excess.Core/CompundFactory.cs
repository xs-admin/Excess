using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess.Core
{
    public class CompoundFactory : IDSLFactory
    {
        public CompoundFactory(params IDSLFactory[] factories)
        {
            _factories.AddRange(factories);
        }

        public CompoundFactory add(IDSLFactory factory)
        {
            _factories.Add(factory);
            return this;
        }

        public IDSLHandler create(string name)
        {
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
            IEnumerable<string> result = null;
            foreach (var factory in _factories)
            {
                var fs = factory.supported();
                if (result == null)
                    result = fs;
                else
                    result = result.Union(fs);
            }

            return result;
        }

        private List<IDSLFactory> _factories = new List<IDSLFactory>();
    }
}
