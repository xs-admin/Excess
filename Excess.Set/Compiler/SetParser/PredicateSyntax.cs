using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.SetParser
{
    public class PredicateSyntax : ConstructorSyntax
    {
        public ExpressionSyntax Expression { get; set; }
    }
}
