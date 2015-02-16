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
    using Excess.Compiler.Roslyn;
    using System.Diagnostics;

    class ExtensionRuntime : BaseRuntime, IExtensionRuntime
    {
        private static Injector _references = new DelegateInjector(compiler =>
        {
            var assemblyPath = Path.GetDirectoryName(typeof(object).Assembly.Location);

            compiler
                .Environment()
                    .dependency<Injector>(new [] {
                        "Excess.Compiler",
                        "Excess.Compiler.Core",
                        "Excess.Compiler.Roslyn"
                    })
                    .dependency<System.Linq.Expressions.Expression>("System.Linq")
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

        //static private CompilationUnitSyntax ExtensionClass = CSharp.ParseCompilationUnit(@"
        //    internal partial class Extension
        //    {
        //        public static void Apply(ICompiler<SyntaxToken, SyntaxNode, SemanticModel> compiler)
        //        {
        //            var lexical = compiler.Lexical();
        //            var syntax = compiler.Syntax();
        //            var semantics = compiler.Semantics();
        //            var environment = compiler.Environment();

        //        }
        //    }");

        static private Template ExtensionClass = Template.Parse<BlockSyntax>(@"
            internal partial class Extension
            {
                public static void Apply(ICompiler<SyntaxToken, SyntaxNode, SemanticModel> compiler)
                {
                    var lexical = compiler.Lexical();
                    var syntax = compiler.Syntax();
                    var semantics = compiler.Semantics();
                    var environment = compiler.Environment();

                }
            }");

        private static SyntaxNode MoveToApply(SyntaxNode root, IEnumerable<SyntaxNode> statements, Scope scope)
        {
            var applyCode = ExtensionClass.Value<BlockSyntax>();

            var result = applyCode;
            foreach (var st in statements)
                result = result.AddStatements((StatementSyntax)st);

            return ExtensionClass.Get(result);
        }

        private static Injector _transform = new DelegateInjector(compiler =>
        {
            compiler
                .Lexical()
                    .normalize()
                        .members(MoveToClass);
        });

        static private Template TransformClass = Template.Parse<ClassDeclarationSyntax>(@"
            using CSharp = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
            internal partial class Extension
            {
            }");


        private static SyntaxNode MoveToClass(SyntaxNode root, IEnumerable<SyntaxNode> members, Scope scope)
        {
            var result = TransformClass
                .Value<ClassDeclarationSyntax>()
                .WithMembers(CSharp.List(members
                    .Select(member => (MemberDeclarationSyntax)member)));

            return TransformClass.Get(result);
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
            Debug.Assert(_compiler != null);

            string rText;
            var tree = _compiler.ApplySemanticalPass(text, out rText);

            return tree.GetRoot().NormalizeWhitespace().ToString();
        }

        public override string defaultFile()
        {
            return "extension";
        }

        Assembly _assembly;

        private string keywordString(IEnumerable<string> keywords)
        {
            StringBuilder result = new StringBuilder();
            foreach (var k in keywords)
            {
                result.Append(" ");
                result.Append(k);
            }

            return result.Length > 0? result.ToString() : " ";
        }

        RoslynCompiler _compiler;
        protected override void doRun(Assembly asm, out dynamic client)
        {
            if (_assembly != asm)
            {
                _assembly = asm;

                Type type = _assembly.GetType("ExtensionPlugin");
                Injector result = (Injector)type.InvokeMember("Create", BindingFlags.Public | BindingFlags.InvokeMethod | BindingFlags.Static, null, null, null);
                if (result == null)
                    throw new InvalidOperationException("Corrupted extension");

                _compiler = new RoslynCompiler();
                result.apply(_compiler);
            }

            client = new {
                debuggerDlg = "/App/Main/dialogs/dslDebugger.html",
                debuggerCtrl = "dslDebuggerCtrl",
                debuggerData = new
                {
                    keywords = keywordString(_compiler.Environment().keywords())
                }
            };
        }

        public override IEnumerable<TreeNodeAction> fileActions(string file)
        {
            if (file == "extension")
                return new[] { new TreeNodeAction { id = "add-extension-item", icon = "fa-plus-circle" } };

            return base.fileActions(file);
        }
    }
}
