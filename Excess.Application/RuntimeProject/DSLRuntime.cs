using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Excess.Core;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.IO;
using System.Threading;

namespace Excess.RuntimeProject
{
    class DSLRuntime : BaseRuntime, IDSLRuntime
    {
        private string _dslName;

        public DSLRuntime(dynamic config) :
            base(new SimpleFactory())
        {
            _dslName = config != null? config.Name : string.Empty;

            _ctx.AddUsing("System.Linq");
            _ctx.AddUsing("Microsoft.CodeAnalysis");
            _ctx.AddUsing("Microsoft.CodeAnalysis.CSharp");
            _ctx.AddUsing("Microsoft.CodeAnalysis.CSharp.Syntax");
            _ctx.AddUsing("Excess.Core");
        }

        public string debugDSL(string text)
        {
            DllFactory factory = new DllFactory();

            string error;
            factory.AddReference(_assembly, out error);
            if (error != null)
                return error;

            SyntaxTree tree = ExcessContext.Compile(text, factory, out _ctx);
            if (_ctx.NeedsLinking())
            {
                Compilation compilation = CSharpCompilation.Create("debugDSL", syntaxTrees: new[] { tree });

                compilation = ExcessContext.Link(_ctx, compilation);
                tree = compilation.SyntaxTrees.First();
            }

            notifyInternalErrors();
            return tree.GetRoot().NormalizeWhitespace().ToString();
        }

        public override string defaultFile()
        {
            return "parser";
        }

        Assembly _assembly;

        protected override void doRun(Assembly asm, out dynamic client)
        {
            _assembly = asm;
            client = new {
                debuggerDlg  = "/App/Main/dialogs/dslDebugger.html",
                debuggerCtrl = "dslDebuggerCtrl",
                debuggerData = new
                {
                    keywords = " " + _dslName
                }
            };
        }

        protected override IEnumerable<MetadataReference> compilationReferences()
        {
            var assemblyPath = Path.GetDirectoryName(typeof(object).Assembly.Location);
            return new[]  {
                MetadataReference.CreateFromAssembly(typeof(IDSLFactory).Assembly),
                MetadataReference.CreateFromAssembly(typeof(SyntaxNode).Assembly),
                MetadataReference.CreateFromAssembly(typeof(ParameterListSyntax).Assembly),
                MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Runtime.dll")),
                MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Threading.Tasks.dll")),
            };
        }

        protected override IEnumerable<SyntaxTree> compilationFiles()
        {
            return null;
        }
    }
}
