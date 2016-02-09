using Excess.Compiler.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess.Extensions.Concurrent.Model
{
    using CSharp = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
    using Roslyn = RoslynCompiler;

    internal class SignalModel
    {
        public SignalModel(int id, string name, bool @public)
        {
            Id = id;
            Name = name;
            Internal = false;
            ReturnType = RoslynCompiler.boolean;
            Public = @public;
        }

        public int Id { get; internal set; }
        public string Name { get; internal set; }
        public bool Internal { get; internal set; }
        public bool Public { get; internal set; }
        public TypeSyntax ReturnType { get; internal set; }
        public IEnumerable<StatementSyntax> GoStatements { get { return _go; } }

        List<StatementSyntax> _go = new List<StatementSyntax>();
        public void OnGo(StatementSyntax statement)
        {
            _go.Add(statement);
        }

        public void OnGo(IEnumerable<StatementSyntax> statements)
        {
            foreach (var statement in statements)
                _go.Add(statement);
        }
    }
}
