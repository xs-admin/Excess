using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess.Compiler.Roslyn
{
    public class RoslynEnvironment : ICompilerEnvironment
    {
        private Scope _root;
        public RoslynEnvironment(Scope root)
        {
            _root = root;
        }


        private List<MetadataReference> _references = new List<MetadataReference>();
        private List<string> _modules = new List<string>();

        public ICompilerEnvironment dependency<T>(string module)
        {
            return dependency<T>(
                string.IsNullOrEmpty(module)? 
                    null :  
                    new[] { module });
        }

        dynamic _path;
        public void setPath(dynamic path)
        {
            _path = path;
        }

        private void addModules(IEnumerable<string> modules)
        {
            foreach (var module in modules)
            {
                if (!_modules.Contains(module))
                    _modules.Add(module);
            }
        }

        public ICompilerEnvironment dependency<T>(IEnumerable<string> modules)
        {
            MetadataReference reference = MetadataReference.CreateFromAssembly(typeof(T).Assembly);
            Debug.Assert(reference != null);

            _references.Add(reference);

            if (modules != null)
                addModules(modules);

            return this;
        }

        public ICompilerEnvironment dependency(string module, string path = null)
        {
            return dependency(
                string.IsNullOrEmpty(module)? 
                    null : 
                    new[] { module }, 
                path);
        }

        public ICompilerEnvironment dependency(IEnumerable<string> modules, string path = null)
        {
            if (path != null)
            {
                MetadataReference reference = MetadataReference.CreateFromFile(path);
                Debug.Assert(reference != null);

                _references.Add(reference);
            }

            if (modules != null)
                addModules(modules);

            return this;
        }

        List<string> _keywords = new List<string>();
        public ICompilerEnvironment keyword(string word)
        {
            _keywords.Add(word);
            return this;
        }

        public ICompilerEnvironment global<T>() where T : class, new()
        {
            _root.set<T>( new T() );
            return this;
        }

        public IEnumerable<string> modules()
        {
            return _modules;
        }

        public IEnumerable<string> keywords()
        {
            return _keywords;
        }

        public dynamic path()
        {
            return _path;
        }

        internal IEnumerable<MetadataReference> GetReferences()
        {
            return _references;
        }
    }
}
