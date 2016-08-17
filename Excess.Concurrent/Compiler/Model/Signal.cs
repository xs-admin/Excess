using Excess.Compiler.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess.Concurrent.Compiler.Model
{
    using CSharp = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
    using Roslyn = RoslynCompiler;

    internal class Signal
    {
        public Signal(int id, string name, bool @public, bool @static)
        {
            Id = id;
            Name = name;
            Internal = false;
            ReturnType = RoslynCompiler.boolean;
            Public = @public;
            Static = @static;
        }

        public int Id { get; internal set; }
        public string Name { get; internal set; }
        public bool Internal { get; internal set; }
        public bool Public { get; internal set; }
        public TypeSyntax ReturnType { get; internal set; }
        public bool Static { get; internal set; }
    }
}
