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

namespace Excess.RuntimeProject
{
    class DSLRuntime : BaseRuntime, IDSLRuntime
    {
        public DSLRuntime(string projectName) :
            base(new SimpleFactory())
        {
            _projectName = projectName;

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
                Compilation compilation = CSharpCompilation.Create("debugDSL",
                            syntaxTrees: new[] { tree },
                            references: new[]  {
                                MetadataReference.CreateFromAssembly(typeof(object).Assembly),
                                MetadataReference.CreateFromAssembly(typeof(Enumerable).Assembly),
                                MetadataReference.CreateFromAssembly(typeof(Dictionary<int, int>).Assembly),
                            },
                            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

                compilation = ExcessContext.Link(ctx, compilation);
                tree = compilation.SyntaxTrees.First();
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
            return new[]  {
                MetadataReference.CreateFromAssembly(typeof(IDSLFactory).Assembly),
                MetadataReference.CreateFromAssembly(typeof(SyntaxNode).Assembly),
                MetadataReference.CreateFromAssembly(typeof(ParameterListSyntax).Assembly),
            };
        }

        protected override IEnumerable<SyntaxTree> compilationFiles()
        {
            var dslName = pluginTree.GetRoot()
                .DescendantNodes()
                .OfType<IdentifierNameSyntax>()
                .Where(identifier => identifier.ToString() == "DSLName")
                .First();

            var plugin = pluginTree.GetRoot()
                .ReplaceNode(dslName,
                             SyntaxFactory.ParseExpression('"' + _projectName + '"'))
                .SyntaxTree;

            return new[] { plugin };
        }

        private string _projectName;

        static protected SyntaxTree pluginTree = SyntaxFactory.ParseSyntaxTree(
            @"using System;
            using Excess.Core;

            public class MyDSL : ManagedDSLHandler
            {
                public MyDSL():
                    base(new Parser(), new Linker())
                {
                }
            }

            public class DSLPlugin
            {
                public static IDSLFactory Create()
                {
                    return new SimpleFactory().Add<MyDSL>(DSLName);
                }

            }");

        //static protected SyntaxTree parserTree = SyntaxFactory.ParseSyntaxTree(
        //    @"using System;
        //    using Microsoft.CodeAnalysis;
        //    using Microsoft.CodeAnalysis.CSharp;
        //    using Microsoft.CodeAnalysis.CSharp.Syntax;
        //    using Excess.Core;

        //    namespace PPP
        //    {
        //    public class RoslynParser : BaseParser
        //    {
        //        public override SyntaxNode ParseNamespace(SyntaxNode node, SyntaxToken id, ParameterListSyntax args, BlockSyntax code)
        //        {
        //            throw new InvalidOperationException(""This dsl does not support namespaces"");
        //        }

        //        public override SyntaxNode ParseClass(SyntaxNode node, SyntaxToken id, ParameterListSyntax args)
        //        {
        //            throw new InvalidOperationException(""This dsl does not support types"");
        //        }

        //        public override SyntaxNode ParseMethod(SyntaxNode node, SyntaxToken id, ParameterListSyntax args, BlockSyntax code)
        //        {
        //            throw new InvalidOperationException(""This dsl does not support members"");
        //        }
        //        public override SyntaxNode ParseCodeHeader(SyntaxNode node)
        //        {
        //            throw new InvalidOperationException(""This dsl does not support code"");
        //        }

        //        public override SyntaxNode ParseCode(SyntaxNode node, SyntaxToken id, ParameterListSyntax args, BlockSyntax code, bool expectsResult)
        //        {
        //            throw new InvalidOperationException(""This dsl does not support code"");
        //        }
        //    }}");

        //    static protected SyntaxTree linkerTree = SyntaxFactory.ParseSyntaxTree(
        //        @"using System;
        //        using Microsoft.CodeAnalysis;
        //        using Microsoft.CodeAnalysis.CSharp;
        //        using Microsoft.CodeAnalysis.CSharp.Syntax;
        //        using Excess.Core;

        //        public class RoslynLinker : ILinker
        //        {
        //            public RoslynLinker()
        //            {
        //            }

        //            public virtual SyntaxNode Link(SyntaxNode node, SemanticModel model)
        //            {
        //                throw new NotImplementedException();
        //            }
        //        }");
    }
}
