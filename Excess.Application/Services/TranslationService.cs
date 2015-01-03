using Excess.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess
{
    public class TranslationService : ITranslationService
    {
        public TranslationService(IDSLService dsl)
        {
            _dsl = dsl;
        }

        public string translate(string text)
        {
            ExcessContext ctx;
            SyntaxTree tree = ExcessContext.Compile(text, _dsl.factory(), out ctx);

            if (ctx.NeedsLinking())
            {
                Compilation compilation = CSharpCompilation.Create("translation",
                            syntaxTrees: new[] { tree });

                compilation = ExcessContext.Link(ctx, compilation);
                tree = compilation.SyntaxTrees.First();
            }

            return tree.GetRoot().NormalizeWhitespace().ToString();
        }

        private IDSLService _dsl;
    }
}
