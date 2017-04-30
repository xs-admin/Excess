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
        public List<ExpressionSyntax> Rules { get; private set; }
    }
}
