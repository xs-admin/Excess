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
        public List<ExpressionSyntax> Values { get; private set; }
    }
}
