using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Excess.Compiler.Core
{
    using SyntacticalMatchFunction     = Func<SyntaxNode, ISyntacticalMatchResult, bool>;
    using SyntacticalMatchNodeFunction = Func<SyntaxNode, bool>;

    public class BaseSyntacticalMatch : ISyntacticalMatch
    {
        private ISyntaxAnalysis  _syntax;
        public BaseSyntacticalMatch(ISyntaxAnalysis syntax)
        {
            _syntax = syntax;
        }


        private List<SyntacticalMatchFunction> _matchers = new List<SyntacticalMatchFunction>();
        public void addMatcher(Func<SyntaxNode, bool> matcher)
        {
            _matchers.Add((node, result) => matcher(node));
        }

        private static SyntacticalMatchFunction MatchChildren(ISyntacticalMatch match)
        {
            return (node, result) =>
            {
                result.matchNodes(node.ChildNodes(), match);
                return true;
            };
        }

        private static SyntacticalMatchFunction MatchDescendants(ISyntacticalMatch match)
        {
            return (node, result) =>
            {
                result.matchNodes(node.DescendantNodes(), match);
                return true;
            };
        }

        public INestedSyntacticalMatch children()
        {
            BaseNestedSyntacticalMatch result = new BaseNestedSyntacticalMatch(this, _syntax);
            _matchers.Add(MatchChildren(result));

            return result;
        }

        public INestedSyntacticalMatch children(Func<SyntaxNode, bool> handler)
        {
            BaseNestedSyntacticalMatch result = new BaseNestedSyntacticalMatch(this, _syntax);
            result.addMatcher(node => handler(node));
            _matchers.Add(MatchChildren(result));

            return result;
        }

        public INestedSyntacticalMatch children<T>(Func<T, bool> handler) where T : SyntaxNode
        {
            BaseNestedSyntacticalMatch result = new BaseNestedSyntacticalMatch(this, _syntax);
            result.addMatcher(node => (node is T) && handler((T)node));
            _matchers.Add(MatchChildren(result));

            return result;
        }

        public INestedSyntacticalMatch descendants()
        {
            BaseNestedSyntacticalMatch result = new BaseNestedSyntacticalMatch(this, _syntax);
            _matchers.Add(MatchDescendants(result));

            return result;
        }

        public INestedSyntacticalMatch descendants(Func<SyntaxNode, bool> handler)
        {
            BaseNestedSyntacticalMatch result = new BaseNestedSyntacticalMatch(this, _syntax);
            result.addMatcher(node => handler(node));
            _matchers.Add(MatchDescendants(result));

            return result;
        }

        public INestedSyntacticalMatch descendants<T>(Func<T, bool> handler) where T : SyntaxNode
        {
            BaseNestedSyntacticalMatch result = new BaseNestedSyntacticalMatch(this, _syntax);
            result.addMatcher(node => (node is T) && handler((T)node));
            _matchers.Add(MatchDescendants(result));

            return result;
        }

        public ISyntaxAnalysis then(Func<SyntaxNode, SyntaxNode> handler)
        {
            throw new NotImplementedException();
        }

        public ISyntaxAnalysis then(ISyntaxTransform transform)
        {
            throw new NotImplementedException();
        }
    }

    public class BaseNestedSyntacticalMatch: BaseSyntacticalMatch, INestedSyntacticalMatch
    {
        ISyntacticalMatch _parent;
        public BaseNestedSyntacticalMatch(ISyntacticalMatch parent, ISyntaxAnalysis syntax) :
            base(syntax)
        {
            _parent = parent;
        }

        public ISyntacticalMatch then(Func<SyntaxNode, SyntaxNode> handler, bool stayOnParent)
        {
            return _parent;
        }

        public ISyntacticalMatch then(ISyntaxTransform transform, bool continueToParent)
        {
            throw new NotImplementedException();
        }
    }

    public class SyntaxAnalysisBase : ISyntaxAnalysis
    {
        private Func<IEnumerable<MemberDeclarationSyntax>, SyntaxNode> _looseMembers;
        private Func<IEnumerable<StatementSyntax>, SyntaxNode> _looseStatements;
        private Func<IEnumerable<TypeDeclarationSyntax>, SyntaxNode>  _looseTypes;

        public ISyntaxAnalysis looseMembers(Func<IEnumerable<MemberDeclarationSyntax>, SyntaxNode> handler)
        {
            _looseMembers = handler;
            return this;
        }

        public ISyntaxAnalysis looseStatements(Func<IEnumerable<StatementSyntax>, SyntaxNode> handler)
        {
            _looseStatements = handler;
            return this;
        }

        public ISyntaxAnalysis looseTypes(Func<IEnumerable<TypeDeclarationSyntax>, SyntaxNode> handler)
        {
            _looseTypes = handler;
            return this;
        }

        public ISyntacticalMatch match()
        {
            throw new NotImplementedException();
        }

        public ISyntacticalMatch match<T>(Func<T, bool> handler)
        {
            throw new NotImplementedException();
        }

        public ISyntacticalMatch matchCodeDSL(string dsl)
        {
            throw new NotImplementedException();
        }

        public ISyntacticalMatch matchTypeDSL(string dsl)
        {
            throw new NotImplementedException();
        }

        public ISyntacticalMatch matchMemberDSL(string dsl)
        {
            throw new NotImplementedException();
        }

        public ISyntacticalMatch matchNamespaceDSL(string dsl)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ISyntacticalMatch> matches()
        {
            throw new NotImplementedException();
        }

        public ISyntaxTransform transform()
        {
            throw new NotImplementedException();
        }
    }
}
