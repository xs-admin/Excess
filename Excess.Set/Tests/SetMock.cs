using Compiler.SetParser;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    using CSharp = SyntaxFactory;

    public class SetMock
    {
        public static SetSyntax Parse(string text)
        {
            var tokens = CSharp.ParseTokens(text);
            return Parser.Parse(tokens);
        }
    }
}
