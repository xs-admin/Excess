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
        public List<Tuple<ExpressionSyntax, ExpressionSyntax>> CondValue { get; private set; }
        public ExpressionSyntax Otherwise { get; internal set; }
    }
}
