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
        public DSLRuntime(string projectName) :
            base(new SimpleFactory())
        {
            _projectName = projectName;

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

            ExcessContext ctx;
            SyntaxTree tree = ExcessContext.Compile(text, factory, out ctx);

            if (ctx.NeedsLinking())
            {
                Compilation compilation = CSharpCompilation.Create("debugDSL", syntaxTrees: new[] { tree });

                compilation = ExcessContext.Link(ctx, compilation);
                tree = compilation.SyntaxTrees.First();

                notifyErrors();
            }

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
            client = new { debuggerDlg = "/App/Main/dialogs/dslDebugger.html", debuggerCtrl = "dslDebuggerCtrl" };
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
            var root   = pluginTree.GetRoot();
            var plugin = root
                .ReplaceNode(root
                    .DescendantNodes()
                    .OfType<LiteralExpressionSyntax>()
                    .First(),
                    SyntaxFactory.LiteralExpression(
                        SyntaxKind.StringLiteralExpression,
                        SyntaxFactory.ParseToken( '"' + _projectName + '"')))
                .SyntaxTree;

            return new[] { plugin };
        }

        private string _projectName;

        static protected SyntaxTree pluginTree = SyntaxFactory.ParseSyntaxTree(
            @"using System;
            using Excess.Core;

            public class DSLPlugin
            {
                private static string DSLName = """";

                public static IDSLFactory Create()
                {
                    Parser.DSLName = DSLName;
                    Linker.DSLName = DSLName;

                    var parser = new Parser();
                    var linker = new Linker();

                    parser.Linker = linker;
                    return new ManagedDSLFactory(DSLName, parser, linker);
                }
            }");

    }
}
