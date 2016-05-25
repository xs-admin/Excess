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

    public static class Mock
    {
        public static SyntaxTree CompileWithMapping(string code)
        {
            var compiler = new RoslynCompiler();
            var document = new RoslynDocument(compiler.Scope, code);
            var mapper = document.Mapper = new MappingService();

            compiler.apply(document);
            document.applyChanges();

            var result = mapper.RenderMapping(document.SyntaxRoot, string.Empty);
            return CSharp.ParseCompilationUnit(result).SyntaxTree;
        }

        public static SyntaxTree Compile(string code, Action<Compiler> builder)
        {
            //build a compiler
            var compiler = new RoslynCompiler();
            if (builder == null)
                builder = (c) => XSLanguage.Apply(c);
            builder(compiler);

            //then a document
            var document = new RoslynDocument(compiler.Scope, code);

            //do the compilation
            compiler.apply(document);
            document.applyChanges();
            return document.SyntaxRoot.SyntaxTree;
        }
    }
}
