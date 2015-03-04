using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using Excess.Compiler;
using Excess.Compiler.Roslyn;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess.Extensions.R
{
    public class RGrammar : IGrammar<SyntaxToken, SyntaxNode, ParserRuleContext>
    {
        public ParserRuleContext parse(IEnumerable<SyntaxToken> tokens, Scope scope)
        {
            var text = RoslynCompiler.TokensToString(tokens);
            AntlrInputStream stream = new AntlrInputStream(text);
            ITokenSource lexer = new RLexer(stream);
            ITokenStream tokenStream = new CommonTokenStream(lexer);
            RParser parser = new RParser(tokenStream);
            return parser.prog();
        }
    }
}
