using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Excess.Compiler.Roslyn
{
    using System.Diagnostics;
    using CSharp = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

    public class ExtensionRewriter : CSharpSyntaxRewriter
    {
        IEventBus _events;

        Dictionary<string, Func<ISyntacticalMatchResult<SyntaxNode>, SyntacticalExtension<SyntaxNode>, IEnumerable<SyntaxNode>>> _codeExtensions = new Dictionary<string, Func<ISyntacticalMatchResult<SyntaxNode>, SyntacticalExtension<SyntaxNode>, IEnumerable<SyntaxNode>>>();
        Dictionary<string, Func<ISyntacticalMatchResult<SyntaxNode>, SyntacticalExtension<SyntaxNode>, IEnumerable<SyntaxNode>>> _memberExtensions = new Dictionary<string, Func<ISyntacticalMatchResult<SyntaxNode>, SyntacticalExtension<SyntaxNode>, IEnumerable<SyntaxNode>>>();
        Dictionary<string, Func<ISyntacticalMatchResult<SyntaxNode>, SyntacticalExtension<SyntaxNode>, IEnumerable<SyntaxNode>>> _typeExtensions = new Dictionary<string, Func<ISyntacticalMatchResult<SyntaxNode>, SyntacticalExtension<SyntaxNode>, IEnumerable<SyntaxNode>>>();

        public ExtensionRewriter(IEnumerable<SyntacticExtensionEvent<SyntaxNode>> extensions, IEventBus events)
        {
            _events = events;

            foreach (var ev in extensions)
            {
                switch (ev.Kind)
                {
                    case ExtensionKind.Code: _codeExtensions[ev.Keyword] = ev.Handler; break;
                    case ExtensionKind.Member: _memberExtensions[ev.Keyword] = ev.Handler; break;
                    case ExtensionKind.Type: _typeExtensions[ev.Keyword] = ev.Handler; break;
                    default: throw new NotImplementedException(); 
                }
            }
        }

        Dictionary<string, IEnumerable<MemberDeclarationSyntax>> _globalMembers = new Dictionary<string, IEnumerable<MemberDeclarationSyntax>>();
        Dictionary<string, IEnumerable<UsingDirectiveSyntax>>    _usings = new Dictionary<string, IEnumerable<UsingDirectiveSyntax>>();

        public override SyntaxNode VisitCompilationUnit(CompilationUnitSyntax node)
        {
            var result = (CompilationUnitSyntax)base.Visit(node);
            if (_globalMembers.Any())
                result = result
                    .WithMembers(CSharp.List(
                        MergeMembers(result.Members, _globalMembers)));

            if (_usings.Any())
                result = result
                    .WithUsings(CSharp.List(
                        MergeUsings(result.Usings, _usings)));

            return result;
        }

        private IEnumerable<UsingDirectiveSyntax> MergeUsings(SyntaxList<UsingDirectiveSyntax> usings, Dictionary<string, IEnumerable<UsingDirectiveSyntax>> transform)
        {
            foreach (var directive in usings)
            {
                var usingId = RoslynCompiler.GetSyntacticalExtensionId(directive);

                IEnumerable<UsingDirectiveSyntax> toReplace;
                if (usingId != null && transform.TryGetValue(usingId, out toReplace))
                {
                    foreach (var uNode in toReplace)
                        yield return uNode;
                }
                else
                    yield return directive;
            }
        }

        private IEnumerable<MemberDeclarationSyntax> MergeMembers(SyntaxList<MemberDeclarationSyntax> members, Dictionary<string, IEnumerable<MemberDeclarationSyntax>> transform)
        {
            foreach (var member in members)
            {
                var memberId = RoslynCompiler.GetSyntacticalExtensionId(member);

                IEnumerable<MemberDeclarationSyntax> toReplace;
                if (memberId != null && transform.TryGetValue(memberId, out toReplace))
                {
                    foreach (var rNode in toReplace)
                        yield return rNode;
                }
                else
                    yield return member;
            }
        }

        public override SyntaxNode VisitNamespaceDeclaration(NamespaceDeclarationSyntax node)
        {
            var result = (NamespaceDeclarationSyntax)base.Visit(node);
            if (_globalMembers.Any())
            {
                result = result
                    .WithMembers(CSharp.List(
                        MergeMembers(result.Members, _globalMembers)));

                _globalMembers.Clear();
            }

            return result;
        }

        Stack<Dictionary<string, IEnumerable<MemberDeclarationSyntax>>> _typeMembers = new Stack<Dictionary<string, IEnumerable<MemberDeclarationSyntax>>>();

        public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            pushType();

            ClassDeclarationSyntax result = (ClassDeclarationSyntax)base.VisitClassDeclaration(node);

            result = popType(result);
            return result;
        }

        private ClassDeclarationSyntax popType(ClassDeclarationSyntax result)
        {
            var top = _typeMembers.Pop();
            if (top.Count > 0)
            {
                result = result
                    .WithMembers(CSharp.List(
                        MergeMembers(result.Members, top)));
            }

            return result;
        }

        private void pushType()
        {
            _typeMembers.Push(new Dictionary<string, IEnumerable<MemberDeclarationSyntax>>());
        }

        Stack<Dictionary<string, IEnumerable<StatementSyntax>>> _codeStatements = new Stack<Dictionary<string, IEnumerable<StatementSyntax>>>();
        public override SyntaxNode VisitBlock(BlockSyntax node)
        {
            if (!_codeExtensions.Any())
                return node; //bail on code is no extension is registered

            pushCode();
            BlockSyntax result = (BlockSyntax)base.VisitBlock(node);
            result = popCode(result);

            return result;
        }

        private BlockSyntax popCode(BlockSyntax result)
        {
            var top = _typeMembers.Pop();
            if (top.Count > 0)
            {
                result = result
                    .WithStatements(CSharp.List(
                        MergeStatements(result.Statements, top)));
            }

            return result;
        }

        private IEnumerable<SyntaxNode> MergeStatements(IEnumerable<StatementSyntax> statements, Dictionary<string, IEnumerable<MemberDeclarationSyntax>> transform)
        {
            foreach (var statement in statements)
            {
                var memberId = RoslynCompiler.GetSyntacticalExtensionId(statement);

                IEnumerable<MemberDeclarationSyntax> toReplace;
                if (memberId != null && transform.TryGetValue(memberId, out toReplace))
                {
                    foreach (var rNode in toReplace)
                        yield return rNode;
                }
                else
                    yield return statement;
            }
        }

        private void pushCode()
        {
            _codeStatements.Push(new Dictionary<string, IEnumerable<StatementSyntax>>());
        }

        public override SyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax method)
        {
            Debug.Assert(RoslynCompiler.GetSyntacticalExtensionId(method) == null);

            SyntacticalExtension<SyntaxNode> extension;
            method = processMethodExtension(method, out extension);
            if (extension == null)
                return base.Visit(method); //not an extension

            bool isNamespaceItem = method.Parent is CompilationUnitSyntax
                                || method.Parent is NamespaceDeclarationSyntax;

            if (extension.Kind != ExtensionKind.Member)
            {
                //td: error, incorrect extension (i.e. a code extension being used inside a type)
                return null;
            }

            var results = TransformExtension(method, extension);
            return processResults(method, ExtensionKind.Member, results);
        }

        private MethodDeclarationSyntax processMethodExtension(MethodDeclarationSyntax method, out SyntacticalExtension<SyntaxNode> extension)
        {
            throw new NotImplementedException();
        }

        private SyntaxNode processResults(SyntaxNode node, ExtensionKind kind,  IEnumerable<SyntaxNode> results)
        {
            if (results == null || !results.Any())
                return null;

            if (results.Take(2).Count() == 1)
                return results.First();

            string id;
            node = RoslynCompiler.SetSyntacticalExtensionId(node, out id);

            switch (kind)
            {
                case ExtensionKind.Code: _codeStatements.Peek()[id] = results.OfType<StatementSyntax>(); break;
                case ExtensionKind.Member: _typeMembers.Peek()[id] = results.OfType<MemberDeclarationSyntax>(); break;
                case ExtensionKind.Type: _globalMembers[id] = results.OfType<MemberDeclarationSyntax>(); break;
                default: throw new NotImplementedException();
            }

            return node;
        }

        private IEnumerable<SyntaxNode> TransformExtension(SyntaxNode node, SyntacticalExtension<SyntaxNode> extension)
        {
            RoslynSyntacticalMatchResult result = new RoslynSyntacticalMatchResult(new Scope(), _events, node);
            return extension.Handler(result, extension);
        }

        public override SyntaxNode VisitInvocationExpression(InvocationExpressionSyntax call)
        {
            Debug.Assert(RoslynCompiler.GetSyntacticalExtensionId(call) == null);

            SyntacticalExtension<SyntaxNode> extension;
            call = processCodeExtension(call, out extension);
            if (extension == null)
                return base.Visit(call); //not an extension

            if (extension.Kind != ExtensionKind.Code)
            {
                //td: error, incorrect extension (i.e. a type extension being used inside code)
                return null;
            }

            var results = TransformExtension(call, extension);
            return processResults(call, ExtensionKind.Code, results);
        }


        public override SyntaxNode VisitIncompleteMember(IncompleteMemberSyntax member)
        {
            Debug.Assert(RoslynCompiler.GetSyntacticalExtensionId(member) == null);

            SyntacticalExtension<SyntaxNode> extension;
            member = processTypeExtension(member, out extension);
            if (extension == null)
                return member; //not an extension, just an error, leave untouched

            if (extension.Kind != ExtensionKind.Type)
            {
                //td: error, incorrect extension
                return null;
            }

            var results = TransformExtension(member, extension);
            return processResults(member, ExtensionKind.Type, results);
        }

        private InvocationExpressionSyntax processCodeExtension(InvocationExpressionSyntax call, out SyntacticalExtension<SyntaxNode> extension)
        {
            throw new NotImplementedException();
        }

        private IncompleteMemberSyntax processTypeExtension(IncompleteMemberSyntax member, out SyntacticalExtension<SyntaxNode> extension)
        {
            throw new NotImplementedException();
        }
    }
}
