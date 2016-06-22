using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Antlr4.Runtime;
using Excess.Compiler.Roslyn;

namespace Excess.Compiler.Antlr4
{
    //interop between antlr and excess, derive
    public abstract class AntlrGrammar : IGrammar<SyntaxToken, SyntaxNode, ParserRuleContext>
    {
        public ParserRuleContext Parse(IEnumerable<SyntaxToken> tokens, Scope scope, int offset)
        {
            var text = RoslynCompiler.TokensToString(tokens); //td: token matching
            AntlrInputStream stream = new AntlrInputStream(text);
            ITokenSource lexer = GetLexer(stream);
            ITokenStream tokenStream = new CommonTokenStream(lexer);
            Parser parser = GetParser(tokenStream);
            parser.AddErrorListener(new AntlrErrors<IToken>(scope, offset));

            var result = GetRoot(parser);
            if (parser.NumberOfSyntaxErrors > 0)
                return null;

            return result;
        }

        protected abstract ParserRuleContext GetRoot(Parser parser);
        protected abstract Parser GetParser(ITokenStream tokenStream);
        protected abstract ITokenSource GetLexer(AntlrInputStream stream);
    }
}
