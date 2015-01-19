using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess.Compiler.Core
{
    public class BaseSyntacticalMatch<TNode> : ISyntacticalMatch<TNode>
    {
        private ISyntaxAnalysis<TNode>   _syntax;
        private ISyntacticalMatch<TNode> _parent;

        public BaseSyntacticalMatch(ISyntaxAnalysis<TNode> syntax, ISyntacticalMatch<TNode> parent = null)
        {
            _syntax = syntax;
            _parent = parent;
        }


        private List<Func<TNode, ISyntacticalMatchResult<TNode>, bool>> _matchers = new List<Func<TNode, ISyntacticalMatchResult<TNode>, bool>>();
        public void addMatcher(Func<TNode, bool> matcher)
        {
            _matchers.Add((node, result) => matcher(node));
        }

        private static Func<TNode, ISyntacticalMatchResult<TNode>, bool> MatchChildren(ISyntacticalMatch<TNode> match)
        {
            return (node, result) =>
            {
                result.matchChildren(match);
                return true;
            };
        }

        private static Func<TNode, ISyntacticalMatchResult<TNode>, bool> MatchDescendants(ISyntacticalMatch<TNode> match)
        {
            return (node, result) =>
            {
                result.matchDescendants(match);
                return true;
            };
        }

        public ISyntacticalMatch<TNode> children()
        {
            BaseSyntacticalMatch<TNode> result = new BaseSyntacticalMatch<TNode>(_syntax, this);
            _matchers.Add(MatchChildren(result));

            return result;
        }

        public ISyntacticalMatch<TNode> children(Func<TNode, bool> handler)
        {
            BaseSyntacticalMatch<TNode> result = new BaseSyntacticalMatch<TNode>(_syntax, this);
            result.addMatcher(node => handler(node));
            _matchers.Add(MatchChildren(result));

            return result;
        }

        public ISyntacticalMatch<TNode> children<T>(Func<T, bool> handler) where T : TNode
        {
            BaseSyntacticalMatch<TNode> result = new BaseSyntacticalMatch<TNode>(_syntax, this);
            result.addMatcher(node => (node is T) && handler((T)node));
            _matchers.Add(MatchChildren(result));

            return result;
        }

        public ISyntacticalMatch<TNode> descendants()
        {
            BaseSyntacticalMatch<TNode> result = new BaseSyntacticalMatch<TNode>(_syntax, this);
            _matchers.Add(MatchDescendants(result));

            return result;
        }

        public ISyntacticalMatch<TNode> descendants(Func<TNode, bool> handler)
        {
            BaseSyntacticalMatch<TNode> result = new BaseSyntacticalMatch<TNode>(_syntax, this);
            result.addMatcher(node => handler(node));
            _matchers.Add(MatchDescendants(result));

            return result;
        }

        public ISyntacticalMatch<TNode> parent()
        {
            return _parent;
        }

        public ISyntacticalMatch<TNode> descendants<T>(Func<T, bool> handler) where T : TNode
        {
            BaseSyntacticalMatch<TNode> result = new BaseSyntacticalMatch<TNode>(_syntax, this);
            result.addMatcher(node => (node is T) && handler((T)node));
            _matchers.Add(MatchDescendants(result));

            return result;
        }

        public ISyntaxAnalysis<TNode> then(Func<TNode, TNode> handler)
        {
            throw new NotImplementedException();
        }

        public ISyntaxAnalysis<TNode> then(ISyntaxTransform transform)
        {
            throw new NotImplementedException();
        }
    }

    public class SyntaxAnalysisBase<TNode> : ISyntaxAnalysis<TNode>
    {
        private Func<IEnumerable<TNode>, TNode> _looseMembers;
        private Func<IEnumerable<TNode>, TNode> _looseStatements;
        private Func<IEnumerable<TNode>, TNode> _looseTypes;

        public ISyntaxAnalysis<TNode> looseMembers(Func<IEnumerable<TNode>, TNode> handler)
        {
            _looseMembers = handler;
            return this;
        }

        public ISyntaxAnalysis<TNode> looseStatements(Func<IEnumerable<TNode>, TNode> handler)
        {
            _looseStatements = handler;
            return this;
        }

        public ISyntaxAnalysis<TNode> looseTypes(Func<IEnumerable<TNode>, TNode> handler)
        {
            _looseTypes = handler;
            return this;
        }

        public ISyntacticalMatch<TNode> match()
        {
            throw new NotImplementedException();
        }

        public ISyntacticalMatch<TNode> match<T>(Func<T, bool> handler) where T : TNode
        {
            throw new NotImplementedException();
        }

        public ISyntacticalMatch<TNode> matchCodeDSL(string dsl)
        {
            throw new NotImplementedException();
        }

        public ISyntacticalMatch<TNode> matchTypeDSL(string dsl)
        {
            throw new NotImplementedException();
        }

        public ISyntacticalMatch<TNode> matchMemberDSL(string dsl)
        {
            throw new NotImplementedException();
        }

        public ISyntacticalMatch<TNode> matchNamespaceDSL(string dsl)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ISyntacticalMatch<TNode>> consume()
        {
            throw new NotImplementedException();
        }

        public ISyntaxTransform transform()
        {
            throw new NotImplementedException();
        }

        public TNode normalize(TNode node)
        {
            throw new NotImplementedException();
        }
    }
}
