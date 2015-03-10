using Excess.Compiler.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Excess.Compiler;
using Excess.Compiler.Core;

namespace Excess
{
    using Injector = ICompilerInjector<SyntaxToken, SyntaxNode, SemanticModel>;
    using CompositeInjector = CompositeInjector<SyntaxToken, SyntaxNode, SemanticModel>;
    using DelegateInjector = DelegateInjector<SyntaxToken, SyntaxNode, SemanticModel>;
    using Excess.Entensions.XS;

    public class TranslationService : ITranslationService
    {
        public string translate(string text)
        {
            if (_compiler == null)
                initCompiler();

            string rText;
            var tree = _compiler.ApplySemanticalPass(text, out rText);
            return tree.GetRoot().NormalizeWhitespace().ToString();
        }

        RoslynCompiler _compiler;
        private void initCompiler()
        {
            _compiler = new RoslynCompiler();
            Injector injector = new CompositeInjector(new[] { XSModule.Create(), demoExtensions() });
            injector.apply(_compiler);
        }

        private Injector demoExtensions()
        {
            return new DelegateInjector(compiler =>
            {
                Asynch.Apply(compiler);
                Match.Apply(compiler);
                Contract.Apply(compiler);
            });
        }
    }
}
