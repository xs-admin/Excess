using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.SetParser
{
    public class PredicateConstructorSyntax : ConstructorSyntax
    {
        internal List<ExpressionSyntax> ExpressionList { get; }
        public PredicateConstructorSyntax()
        {
            ExpressionList = new List<ExpressionSyntax>();
        }

        public IEnumerable<ExpressionSyntax> Expressions { get { return ExpressionList; } }
    }
}
