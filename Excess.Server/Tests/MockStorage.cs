using Excess.Compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    public class MockStorage : IPersistentStorage
    {
        Dictionary<string, int> _index = new Dictionary<string, int>();
        Dictionary<int, string> _files = new Dictionary<int, string>();
        public int addFile(string name, string contents, bool hidden)
        {
            var idx = _index.Count;
            _index[name] = idx;
            _files[idx] = contents;
            return idx;
        }

        public int cachedId(string name)
        {
            return _index[name];
        }

        public void cachedId(string name, int id)
        {
            _index[name] = id;
        }

        public string GetContents(string name)
        {
            return _files[_index[name]];
        }
    }
}
