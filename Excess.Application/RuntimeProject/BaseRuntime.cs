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
                if (!doCompile())
                    notifyErrors();
            }
            finally
            {
                notify(NotificationKind.Finished, "Finished Compiling");
                _busy = false;
            }
        }

        public void run(out dynamic client)
        {
            client = null;
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
                    doRun(assembly, out client);
                }
            }
            finally
            {
                notify(NotificationKind.Finished, "Finished Executing");
                _busy = false;
            }
        }

        protected void notifyErrors(IEnumerable<Diagnostic> errors)
        {
            foreach (var diag in errors)
            {
                notify(NotificationKind.Error, diag.ToString());
            }
        }

        protected void notifyErrors()
        {
            notifyErrors(GetErrors());
        }

        protected void notifyInternalErrors()
        {
            notifyErrors(_ctx.GetErrors());
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
                notify(NotificationKind.System, dirty.FileName + "...");

                prepareContext(dirty.FileName);
                var newTree = ExcessContext.Compile(_ctx, dirty.Contents);

                if (dirty.Tree == null)
                    _compilation = _compilation.AddSyntaxTrees(new[] { newTree });
                else
                    _compilation = _compilation.ReplaceSyntaxTree(dirty.Tree, newTree);

                dirty.Tree = newTree;
            }

            Dictionary<SyntaxTree, SyntaxTree> track = new Dictionary<SyntaxTree, SyntaxTree>();
            _compilation = ExcessContext.Link(_ctx, _compilation, track);

            foreach (var file in _files)
            {
                SyntaxTree tree;
                if (track.TryGetValue(file.Value.Tree, out tree))
                    file.Value.Tree = tree;
            }

            _dirty.Clear();

            return !GetErrors().Any();
        }

        private IEnumerable<Diagnostic> GetErrors()
        {
            return _ctx
                .GetErrors()
                .Union(_compilation
                    .GetDiagnostics()
                    .Where(d => d.Severity == DiagnosticSeverity.Error));
        }

        protected abstract void doRun(Assembly asm, out dynamic clientData);
        
        //File Management
        protected Dictionary<string, RuntimeFile> _files = new Dictionary<string, RuntimeFile>();
        protected List<RuntimeFile>               _dirty = new List<RuntimeFile>();

        //Compilation Management
        protected Compilation _compilation;
        protected abstract IEnumerable<MetadataReference> compilationReferences();
        protected abstract IEnumerable<SyntaxTree> compilationFiles();
        protected virtual void prepareContext(string fileName)
        {
        }

        private void updateCompilation()
        {
            if (_compilation == null)
            {
                var defaultReferences = new[]  {
                    MetadataReference.CreateFromAssembly(typeof(object).Assembly),
                    MetadataReference.CreateFromAssembly(typeof(Enumerable).Assembly),
                };

                IEnumerable<MetadataReference> projReferences = compilationReferences();
                if (projReferences == null)
                    projReferences = defaultReferences;
                else
                    projReferences = projReferences.Union(defaultReferences);

                var defaultFiles = new[] {
                    consoleTree,
                    randomTree
                };

                IEnumerable<SyntaxTree> projFiles = compilationFiles();
                if (projFiles == null)
                    projFiles = defaultFiles;
                else
                    projFiles = projFiles.Union(defaultFiles);

                _compilation = CSharpCompilation.Create("runtime" + Guid.NewGuid().ToString().Replace("-", "") + ".dll",
                    syntaxTrees: projFiles,
                    references:  projReferences,
                    options:     new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
            }
        }

        //Notifications
        private List<Notification> _notifications    = new List<Notification>();
        private object             _notificationLock = new object();

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
            @"using System;
            public class console
            {
                static private Action<string> _notify;
                static public void setup(Action<string> notify)
                {
                    _notify = notify;
                }

                static public void write(string message)
                {
                    _notify(message);
                }
            }");

        static protected SyntaxTree randomTree = SyntaxFactory.ParseSyntaxTree(
            @"using System;
            public class random
            {
                static private Random _random = new Random();

                static public int Int()
                {
                    return _random.Next();
                }

                static public int Int(int range)
                {
                    return _random.Next(range);
                }

                static public double Double()
                {
                    return _random.NextDouble();
                }
            }");

        private void setupConsole(Assembly assembly)
        {
            Type console = assembly.GetType("console");
            var  method = console.GetMethod("setup");
            method.Invoke(null, new [] { (Action<string>)consoleNotify });
        }

        private void consoleNotify(string message)
        {
            notify(NotificationKind.Application, message);
        }
    }
}
