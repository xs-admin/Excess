using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.IO;
using System.Threading;
using Excess.Compiler;
using Excess.Compiler.Core;

namespace Excess.RuntimeProject
{
    using Injector = ICompilerInjector<SyntaxToken, SyntaxNode, SemanticModel>;
    using DelegateInjector = DelegateInjector<SyntaxToken, SyntaxNode, SemanticModel>;
    using CompositeInjector = CompositeInjector<SyntaxToken, SyntaxNode, SemanticModel>;

    class ExtensionRuntime : BaseRuntime, IExtensionRuntime
    {
        private static Injector _references = new DelegateInjector(compiler =>
        {
            var assemblyPath = Path.GetDirectoryName(typeof(object).Assembly.Location);

            compiler
                .Environment()
                    .dependency<Injector>("Excess.Compiler.Roslyn")
                    .dependency("System.Linq")
                    .dependency<SyntaxNode>("Microsoft.CodeAnalysis")
                    .dependency<CSharpSyntaxNode>(new[] {
                        "Microsoft.CodeAnalysis.CSharp",
                        "Microsoft.CodeAnalysis.CSharp.Syntax"})
                    .dependency(string.Empty, path: Path.Combine(assemblyPath, "System.Runtime.dll"))
                    .dependency(string.Empty, path: Path.Combine(assemblyPath, "System.Threading.Tasks.dll"));
        });

        protected override ICompilerInjector<SyntaxToken, SyntaxNode, SemanticModel> getInjector(string file)
        {
            var xs = base.getInjector(file);
            return new CompositeInjector(new[] { _references, xs });
        }

        public string debugExtension(string text)
        {
            //DllFactory factory = new DllFactory();

            //string error;
            //factory.AddReference(_assembly, out error);
            //if (error != null)
            //    return error;

            //SyntaxTree tree = ExcessContext.Compile(text, factory, out _ctx);
            //if (_ctx.NeedsLinking())
            //{
            //    Compilation compilation = CSharpCompilation.Create("debugDSL", syntaxTrees: new[] { tree });

            //    compilation = ExcessContext.Link(_ctx, compilation);
            //    tree = compilation.SyntaxTrees.First();
            //}

            //notifyInternalErrors();
            //return tree.GetRoot().NormalizeWhitespace().ToString();
            throw new NotImplementedException(); //td:
        }

        public override string defaultFile()
        {
            return "language";
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
                    keywords = " ",//td: + _dslName
                }
            };
        }
    }
}
