using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.SetParser
{
    public class InductionConstructorSyntax : ConstructorSyntax
    {
        internal List<ExpressionSyntax> RuleList { get; private set; }
        public InductionConstructorSyntax()
        {
            RuleList = new List<ExpressionSyntax>();
        }

        public IEnumerable<ExpressionSyntax> Rules => RuleList;
    }
}
