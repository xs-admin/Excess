using Excess.Compiler;
using Excess.Compiler.Roslyn;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xslang;

namespace Excess.Compiler.Mock
{
    using CSharp = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
    using Compiler = ICompiler<SyntaxToken, SyntaxNode, SemanticModel>;
    using Mapper = IMappingService<SyntaxToken, SyntaxNode>;
    using Microsoft.CodeAnalysis.CSharp;
    public static class ExcessMock
    {
        public static SyntaxTree CompileWithMapping(string code, Action<Compiler> builder = null)
        {
            return Compile(code, builder, new MappingService());
        }

        public static SyntaxTree Compile(string code, Action<Compiler> builder = null, Mapper mapper = null)
        {
            //build a compiler
            var compiler = new RoslynCompiler();
            if (builder == null)
                builder = (c) => XSLanguage.Apply(c);
            builder(compiler);

            //then a document
            var document = new RoslynDocument(compiler.Scope, code);

            //mapping
            document.Mapper = mapper;

            //do the compilation
            compiler.apply(document);
            document.applyChanges();

            if (mapper != null)
            {
                var translated = mapper.RenderMapping(document.SyntaxRoot, string.Empty);
                return CSharp.ParseCompilationUnit(translated).SyntaxTree;
            }

            return document.SyntaxRoot.SyntaxTree;
        }

        public static SyntaxTree Link(string code, Action<Compiler> builder = null, Mapper mapper = null)
        {
            //build a compiler
            var compiler = new RoslynCompiler();
            if (builder == null)
                builder = (c) => XSLanguage.Apply(c);
            builder(compiler);

            //then a document
            var document = new RoslynDocument(compiler.Scope, code);

            //mapping
            document.Mapper = mapper;

            //do the compilation
            compiler.apply(document);
            document.applyChanges(CompilerStage.Syntactical);

            var node = document.SyntaxRoot;
            if (mapper != null)
            {
                var translated = mapper.RenderMapping(node, string.Empty);
                node = CSharp.ParseCompilationUnit(translated);
            }

            var compilation = CSharpCompilation.Create("mock-assembly",
                syntaxTrees: new[] { node.SyntaxTree },
                references: new[]
                {
                    MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(Dictionary<int, int>).Assembly.Location),
                });

            document.SyntaxRoot = node;
            document.Model = compilation.GetSemanticModel(node.SyntaxTree);
            document.applyChanges(CompilerStage.Finished);
            return document.SyntaxRoot.SyntaxTree;
        }
    }
}
