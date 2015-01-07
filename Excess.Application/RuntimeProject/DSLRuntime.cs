using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Excess.Core;

namespace Excess.RuntimeProject
{
    class DSLRuntime : BaseRuntime
    {
        public DSLRuntime() : 
            base(new SimpleFactory())
        {
        }

        public override string defaultFile()
        {
            return "parser";
        }

        protected override void doRun(Assembly asm)
        {
            throw new NotImplementedException();
        }

        protected override IEnumerable<MetadataReference> compilationReferences()
        {
            return null;
        }

        protected override IEnumerable<SyntaxTree> compilationFiles()
        {
            return new[] { pluginTree, parserTree, linkerTree };
        }

        static protected SyntaxTree pluginTree = SyntaxFactory.ParseSyntaxTree(
            @"using System;
            public class DSLPlugin
            {
                public class MyDSL : ManagedDSLHandler
                {
                    public ManagedDSLHandler():
                        base(new Parser(), new Linker())
                    {
                    }
                }

                public static IDSLFactory Create()
                {
                    return new SimpleFactory().Add<MyDSL>(DSLName);
                }
            }");

        static protected SyntaxTree parserTree = SyntaxFactory.ParseSyntaxTree(
            @"using System;
            public class RoslynParser : IParser
            {
                public virtual SyntaxNode ParseNamespace(SyntaxNode node, SyntaxToken id, ParameterListSyntax args, BlockSyntax code);
                {
                    throw new NotImplementedException();
                }

                public virtual SyntaxNode ParseClass(SyntaxNode node, SyntaxToken id, ParameterListSyntax args);
                {
                    throw new NotImplementedException();
                }

                public virtual SyntaxNode ParseMethod(SyntaxNode node, SyntaxToken id, ParameterListSyntax args, BlockSyntax code);
                {
                    throw new NotImplementedException();
                }

                public virtual SyntaxNode ParseCodeHeader(SyntaxNode node);
                {
                    return null;
                }

                public virtual SyntaxNode ParseCode(SyntaxNode node, SyntaxToken id, ParameterListSyntax args, BlockSyntax code, bool expectsResult)
                {
                    throw new NotImplementedException();
                }
            }");

        static protected SyntaxTree linkerTree = SyntaxFactory.ParseSyntaxTree(
            @"using System;
            public class RoslynLinker : ILinker
            {
                public virtual SyntaxNode Link(SyntaxNode node, SemanticModel model)
                {
                    throw new NotImplementedException();
                }
            }");
    }
}
