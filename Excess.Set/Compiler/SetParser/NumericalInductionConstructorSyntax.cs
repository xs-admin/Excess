using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.SetParser
{
    public class NumericalInductionConstructorSyntax : ConstructorSyntax
    {
        public NumericalInductionConstructorSyntax()
        {
            ValueList = new List<ExpressionSyntax>();
        }

        internal List<ExpressionSyntax> ValueList { get; private set; }
        public IEnumerable<ExpressionSyntax> Values => Values;
    }
}
