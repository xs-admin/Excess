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

    internal class ClassModel
    {
        public ClassModel(string name, Scope scope)
        {
            Scope = scope;
            Signals = new Dictionary<int, SignalModel>();
            Name = name;
        }

        public string Name { get; set; }
        public SignalModel Main { get; set; }
        public IDictionary<int, SignalModel> Signals { get; private set; }
        public Scope Scope { get; private set; }

        public IEnumerable<SignalModel> AllSignals()
        {
            return (new[] { Main })
                .Union(Signals.Values);
        }

        List<MemberDeclarationSyntax> _add = new List<MemberDeclarationSyntax>();
        public void AddMember(MemberDeclarationSyntax member)
        {
            _add.Add(member);
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

        public bool AcceptPublicSignals { get; set; }

        Dictionary<string, int> _signals = new Dictionary<string, int>();
        public SignalModel AddSignal(string name, bool isPublic)
        {
            if (_signals.ContainsKey(name))
                throw new InvalidOperationException("duplicate concurrent signal");

            var id = _signals.Count;
            _signals[name] = id;

            var signal = new SignalModel(id, name, isPublic);
            Signals[id] = signal;
            return signal;
        }

        public SignalModel AddSignal(string name, TypeSyntax returnType, bool isPublic)
        {
            var signal = AddSignal(name, isPublic);
            signal.ReturnType = returnType;
            return signal;
        }

        public SignalModel AddSignal()
        {
            var signal = AddSignal(Roslyn.uniqueId(), false);
            signal.Internal = true;
            return signal;
        }

        public void AddType(TypeDeclarationSyntax type)
        {
            _add.Add(type);
        }

        static TypeSyntax inheritType = CSharp.ParseTypeName("ConcurrentObject");
        public ClassDeclarationSyntax Update(ClassDeclarationSyntax @class)
        {
            _replace = RoslynCompiler.Track(@class.SyntaxTree, _replace);

            var result = @class
                .ReplaceNodes(_replace.Keys, (on, nn) => _replace[on])
                .RemoveNodes(_remove, SyntaxRemoveOptions.KeepNoTrivia)
                .WithBaseList(CSharp.BaseList(CSharp.SeparatedList(new[] {(BaseTypeSyntax)CSharp
                        .SimpleBaseType(inheritType)})))
                .AddMembers(_add
                    .Select(add => (MemberDeclarationSyntax)RoslynCompiler.TrackNode(add))
                    .ToArray());

            _remove.Clear();
            _add.Clear();
            _replace.Clear();
            return result;
        }

    }
}
