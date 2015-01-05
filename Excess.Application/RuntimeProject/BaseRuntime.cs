using Excess.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Excess.RuntimeProject
{
    internal abstract class BaseRuntime : IRuntimeProject
    {
        public BaseRuntime(IDSLFactory factory)
        {
            _ctx = new ExcessContext(factory);
        }

        public bool busy()
        {
            return _busy;
        }

        public void compile()
        {
            if (_busy)
                throw new InvalidOperationException();

            _busy = true;
            try
            {
                doCompile();
            }
            finally
            {
                notify(NotificationKind.Finished, "Finished Compiling");
                _busy = false;
            }
        }

        public void run()
        {
            if (_busy)
                throw new InvalidOperationException();

            _busy = true;
            try
            {
                if (!doCompile())
                    notifyErrors();
                else
                {
                    Assembly assembly = loadAssembly();
                    doRun(assembly);
                }
            }
            finally
            {
                notify(NotificationKind.Finished, "Finished Executing");
                _busy = false;
            }
        }

        private void notifyErrors()
        {
            foreach (var diag in _compilation.GetDiagnostics()
                                    .Where(d => d.Severity == DiagnosticSeverity.Error))
            {
                notify(NotificationKind.Error, diag.GetMessage());
            }
        }

        public void add(string file, int fileId, string contents)
        {
            if (_files.ContainsKey(file))
                throw new InvalidOperationException();

            var rFile = new RuntimeFile
            {
                FileName = file,
                FileId = fileId,
                Contents = contents,
            };

            _files[file] = rFile;
            _dirty.Add(rFile);
        }

        public void modify(string file, string contents)
        {
            RuntimeFile rFile = _files[file];
            rFile.Contents = contents;

            if (!_dirty.Contains(rFile))
                _dirty.Add(rFile);
        }

        public IEnumerable<Notification> notifications()
        {
            List<Notification> result = new List<Notification>();
            lock (_notificationLock)
            {
                int toRemove = 0;
                foreach (var not in _notifications)
                {
                    result.Add(not);
                    toRemove++;

                    if (not.Kind == NotificationKind.Finished)
                        break;
                }

                if (toRemove > 0)
                    _notifications.RemoveRange(0, toRemove);
            }

            return result;
        }

        public abstract string defaultFile();

        public string fileContents(string file)
        {
            RuntimeFile rFile;
            if (_files.TryGetValue(file, out rFile))
                return rFile.Contents;

            return null;
        }

        public int fileId(string file)
        {
            RuntimeFile rFile;
            if (_files.TryGetValue(file, out rFile))
                return rFile.FileId;

            return -1;
        }

        protected class RuntimeFile
        {
            public string FileName { get; set; }
            public int FileId { get; set; }
            public SyntaxTree Tree { get; set; }
            public string Contents { get; set; }
        }

        protected bool          _busy = false;
        protected ExcessContext _ctx;

        protected virtual bool doCompile()
        {
            updateCompilation();

            if (!_dirty.Any())
            {
                notify(NotificationKind.System, "Compilation up to date");
                return true;
            }

            //Compile
            foreach (var dirty in _dirty)
            {
                var newNode = _ctx.Compile(dirty.Tree.GetRoot());
                var newTree = newNode.SyntaxTree;

                _compilation = _compilation.ReplaceSyntaxTree(dirty.Tree, newTree);
                dirty.Tree = newTree;
            }

            //Link
            foreach (var dirty in _dirty)
            {
                var linker     = new Linker(_ctx, _compilation);
                var resultTree = linker.link(dirty.Tree.GetRoot(), out _compilation).SyntaxTree;

                dirty.Tree = resultTree;
            }

            _dirty.Clear();

            return !_compilation.GetDiagnostics()
                        .Where(d => d.Severity == DiagnosticSeverity.Error)
                        .Any();
        }

        protected abstract void doRun(Assembly asm);
        
        //File Management
        protected Dictionary<string, RuntimeFile> _files = new Dictionary<string, RuntimeFile>();
        protected List<RuntimeFile>               _dirty = new List<RuntimeFile>();

        //Compilation Management
        protected Compilation _compilation;
        protected abstract IEnumerable<MetadataReference> references();
        protected virtual void prepareContext(string fileName)
        {
        }

        private void updateCompilation()
        {
            if (_busy)
                throw new InvalidOperationException();

            if (_compilation == null)
            {
                var defaultReferences = new[]  {
                    MetadataReference.CreateFromAssembly(typeof(object).Assembly),
                    MetadataReference.CreateFromAssembly(typeof(Enumerable).Assembly),
                };

                IEnumerable<MetadataReference> projReferences = references();
                if (projReferences == null)
                    projReferences = defaultReferences;
                else
                    projReferences = projReferences.Union(defaultReferences);

                _compilation = CSharpCompilation.Create("runtime",
                    syntaxTrees: new[] { consoleTree },
                    references:  projReferences,
                    options:     new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
            }

            foreach (var dirty in _dirty)
            {
                prepareContext(dirty.FileName);
                var newTree = ExcessContext.Compile(_ctx, dirty.Contents);

                if (dirty.Tree == null)
                    _compilation = _compilation.AddSyntaxTrees(new[] { newTree });
                else
                    _compilation = _compilation.ReplaceSyntaxTree(dirty.Tree, newTree);

                dirty.Tree = newTree;
            }
        }

        //Notifications
        private List<Notification> _notifications = new List<Notification>();
        private object             _notificationLock;

        protected void notify(NotificationKind kind, string message)
        {
            lock(_notificationLock)
            {
                _notifications.Add(new Notification { Kind = kind, Message = message });
            }
        }

        //Assembly management
        private Assembly loadAssembly()
        {
            using (var stream = new MemoryStream())
            {
                var result = _compilation.Emit(stream);
                if (!result.Success)
                    return null;

                var assembly = Assembly.Load(stream.GetBuffer());
                setupConsole(assembly);

                return assembly;
            }
        }

        //Console
        static protected SyntaxTree consoleTree = SyntaxFactory.ParseSyntaxTree(
            @"public class console
            {
                static private Action<string> _notify;
                static internal void setup(Action<string> notify)
                {
                    _notify = notify;
                }

                static public void write(string message)
                {
                    _notify.notify(message);
                }
            }");

        private void setupConsole(Assembly assembly)
        {
            Type console = assembly.GetType("console");
            var  method = console.GetMethod("setup", BindingFlags.Static);
            method.Invoke(null, new [] { (Action<string>)consoleNotify });
        }

        private void consoleNotify(string message)
        {
            notify(NotificationKind.Application, message);
        }
    }
}
