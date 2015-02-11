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

using CSharp = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

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

        private static Injector _extension = new DelegateInjector(compiler =>
        {
            compiler
                .Lexical()
                    .normalize()
                        .statements(MoveToApply);
        });

        static private CompilationUnitSyntax ExtensionClass = CSharp.ParseCompilationUnit(@"
            internal partial class Extension
            {
                public static Apply(ICompiler<SyntaxToken, SyntaxNode, SemanticModel> compiler)
                {
                    var lexical = compiler.Lexical();
                    var sintaxis = compiler.Sintaxis();
                    var semantics = compiler.Semantics();
                    var environment = compiler.Environment();

                }
            }");

        private static SyntaxNode MoveToApply(SyntaxNode root, IEnumerable<SyntaxNode> statements, Scope scope)
        {
            var applyCode = ExtensionClass
                .DescendantNodes()
                .OfType<BlockSyntax>()
                .First();

            var result = applyCode;
            foreach (var st in statements)
                result = result.AddStatements((StatementSyntax)st);

            return ExtensionClass.ReplaceNode(applyCode, result);
        }

        private static Injector _transform = new DelegateInjector(compiler =>
        {
            compiler
                .Lexical()
                    .normalize()
                        .members(MoveToClass);
        });

        static private CompilationUnitSyntax TransformClass = CSharp.ParseCompilationUnit(@"
            internal partial class Extension
            {
            }");


        private static SyntaxNode MoveToClass(SyntaxNode root, IEnumerable<SyntaxNode> members, Scope scope)
        {
            var @class = TransformClass
                .DescendantNodes()
                .OfType<ClassDeclarationSyntax>()
                .First();

            var result = @class;
            foreach (var member in members)
                result = result.AddMembers((MemberDeclarationSyntax)member);

            return TransformClass.ReplaceNode(@class, @class.AddMembers());
        }

        protected override ICompilerInjector<SyntaxToken, SyntaxNode, SemanticModel> getInjector(string file)
        {
            var xs = base.getInjector(file);
            if (file == "extension")
                return new CompositeInjector(new[] { _references, _extension, xs });

            if (file == "transform")
                return new CompositeInjector(new[] { _references, _transform, xs });

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
            return "extension";
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
