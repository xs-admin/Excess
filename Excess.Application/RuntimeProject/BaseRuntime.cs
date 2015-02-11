using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Compilation = Excess.Compiler.Roslyn.Compilation;
using Excess.Compiler;
using Excess.Compiler.XS;

namespace Excess.RuntimeProject
{
    internal abstract class BaseRuntime : IRuntimeProject
    {
        public BaseRuntime()
        {
            _compilation.addSyntaxTree(consoleTree);
            _compilation.addSyntaxTree(randomTree);
        }

        public bool busy()
        {
            return _busy;
        }

        protected Compilation _compilation = new Compilation();

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
                bool succeded = doCompile();
                if (succeded)
                {
                    Assembly assembly = _compilation.build();
                    succeded = assembly != null;
                    if (succeded)
                    {
                        setupConsole(assembly);
                        doRun(assembly, out client);
                    }
                }

                if (!succeded)
                    notifyErrors();
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
            notifyErrors(_compilation.errors());
        }

        public void add(string file, int fileId, string contents)
        {
            if (_files.ContainsKey(file))
                throw new InvalidOperationException();

            _files[file] = fileId;
            _compilation.addDocument(file, contents, getInjector(file));

            _dirty = true;
        }

        protected virtual ICompilerInjector<SyntaxToken, SyntaxNode, SemanticModel> getInjector(string file)
        {
            return XSModule.Create();
        }

        public void modify(string file, string contents)
        {
            if (!_files.ContainsKey(file))
                throw new InvalidOperationException();

            _compilation.updateDocument(file, contents);

            _dirty = true;
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
            if (_files.ContainsKey(file))
                return _compilation.documentText(file);

            return null;
        }

        public int fileId(string file)
        {
            int result;
            if (_files.TryGetValue(file, out result))
                return result;

            return -1;
        }

        protected bool _busy  = false;
        protected bool _dirty = false;

        protected virtual bool doCompile()
        {
            if (!_dirty)
            {
                notify(NotificationKind.System, "Compilation up to date");
                return true;
            }

            var result = _compilation.compile();
            if (_dirty)
                _dirty = false;

            return result;
        }

        protected abstract void doRun(Assembly asm, out dynamic clientData);
        
        protected Dictionary<string, int> _files            = new Dictionary<string, int>();
        private List<Notification>        _notifications    = new List<Notification>();
        private object                    _notificationLock = new object();

        protected void notify(NotificationKind kind, string message)
        {
            lock(_notificationLock)
            {
                _notifications.Add(new Notification { Kind = kind, Message = message });
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
