using Excess.Compiler;
using Excess.Compiler.Roslyn;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xslang;

namespace Tests
{
    using CSharp = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
    using Compiler = ICompiler<SyntaxToken, SyntaxNode, SemanticModel>;
    using Mapper = IMappingService<SyntaxToken, SyntaxNode>;

    public static class Mock
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
    }
}
