using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess.Compiler.Roslyn
{
    public class IndentationNode
    {
        public int Depth { get; private set; }
        public string Value { get; private set; }
        public IEnumerable<IndentationNode> Children { get; private set; } 
    }

    public class IndentationParser
    {
        public static IndentationNode Parse(IEnumerable<SyntaxToken> tokens)
        {
            throw new NotImplementedException();
        }
    }
}
