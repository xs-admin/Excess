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
    using Compiler;
    using CSharp = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
    using Roslyn = RoslynCompiler;

    internal class Class
    {
        public Class(string name, Scope scope)
        {
            Scope = scope;
            Signals = new Dictionary<int, Signal>();
            Name = name;
        }

        public string Name { get; set; }
        public Signal Main { get; set; }
        public IDictionary<int, Signal> Signals { get; private set; }
        public Scope Scope { get; private set; }

        public IEnumerable<Signal> AllSignals()
        {
            return (new[] { Main })
                .Union(Signals.Values);
        }

        List<MemberDeclarationSyntax> _add = new List<MemberDeclarationSyntax>();
        public void AddMember(MemberDeclarationSyntax member)
        {
            _add.Add(member);
        }

        public bool IsSignal(MethodDeclarationSyntax node)
        {
            var name = node.Identifier.ToString();
            var pattern = "__concurrent";
            return _signals
                .Any(s => pattern + s.Key == name);
        }

        List<MemberDeclarationSyntax> _remove = new List<MemberDeclarationSyntax>();
        public void RemoveMember(MemberDeclarationSyntax member)
        {
            _remove.Add(member);
        }

        Dictionary<SyntaxNode, SyntaxNode> _replace = new Dictionary<SyntaxNode, SyntaxNode>();
        public void Replace(SyntaxNode oldNode, SyntaxNode newNode)
        {
            _replace[oldNode] = newNode;
        }

        Dictionary<string, int> _signals = new Dictionary<string, int>();
        public Signal AddSignal(string name, bool isPublic)
        {
            if (_signals.ContainsKey(name))
                throw new InvalidOperationException("duplicate concurrent signal");

            var id = _signals.Count;
            _signals[name] = id;

            var signal = new Signal(id, name, isPublic);
            Signals[id] = signal;
            return signal;
        }

        public Signal AddSignal(string name, TypeSyntax returnType, bool isPublic)
        {
            var signal = AddSignal(name, isPublic);
            signal.ReturnType = returnType;
            return signal;
        }

        public Signal AddSignal()
        {
            var signal = AddSignal(Roslyn.uniqueId(), false);
            signal.Internal = true;
            return signal;
        }

        public Signal GetSignal(string name)
        {
            var result = 0;
            if (_signals.TryGetValue(name, out result))
                return Signals[result];
            return null;
        }

        List<TypeDeclarationSyntax> _types = new List<TypeDeclarationSyntax>();
        public void AddType(TypeDeclarationSyntax type)
        {
            _types.Add(type);
        }

        static TypeSyntax inheritType = CSharp.ParseTypeName("Runtime.Object");
        public ClassDeclarationSyntax Update(ClassDeclarationSyntax @class)
        {
            _replace = RoslynCompiler.Track(@class.SyntaxTree, _replace);

            var result = @class
                .ReplaceNodes(_replace.Keys, (on, nn) => _replace[on])
                .RemoveNodes(_remove, SyntaxRemoveOptions.KeepNoTrivia)
                .AddBaseListTypes(CSharp.SimpleBaseType(inheritType))
                .AddMembers(_add
                    .Union(_types)
                    .Select(add => (MemberDeclarationSyntax)RoslynCompiler.TrackNode(add))
                    .ToArray());

            _remove.Clear();
            _add.Clear();
            _types.Clear();
            _replace.Clear();
            return result;
        }
    }
}
