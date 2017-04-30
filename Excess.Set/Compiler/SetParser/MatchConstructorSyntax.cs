using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.SetParser
{
    public class MatchConstructorSyntax : ConstructorSyntax
    {
        internal MatchConstructorSyntax()
        {
            MatchPairList = new List<Tuple<ExpressionSyntax, ExpressionSyntax>>();
        }

        internal MatchConstructorSyntax(ExpressionSyntax otherwiseValue)
        {
            MatchPairList = new List<Tuple<ExpressionSyntax, ExpressionSyntax>>();
            OtherwiseValue = otherwiseValue;
        }

        internal List<Tuple<ExpressionSyntax, ExpressionSyntax>> MatchPairList { get; private set; }
        public IEnumerable<Tuple<ExpressionSyntax, ExpressionSyntax>> MatchPairs => MatchPairList;

        internal ExpressionSyntax OtherwiseValue { get; set; }
        public ExpressionSyntax Otherwise => OtherwiseValue;
    }
}
