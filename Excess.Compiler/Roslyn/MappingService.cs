using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess.Compiler.Roslyn
{
    public class MappingService : IMappingService<SyntaxNode>
    {
        public SyntaxNode LexicalTree     { get; set; }
        public SyntaxNode SyntacticalTree { get; set; }
        public SyntaxNode SemanticalTree  { get; set; }

        struct Change
        {
            public int Original;
            public int Modified;
            public bool isChange;
        }

        List<Change> _changes = new List<Change>();
        int _currOriginal = 0;
        public void LexicalChange(SourceSpan oldSpan, int newLength)
        {
            _changes.Add(new Change
            {
                Original = oldSpan.Start - _currOriginal,
                Modified = oldSpan.Start - _currOriginal,
                isChange = false
            });

            _changes.Add(new Change { Original = oldSpan.Length, Modified = newLength, isChange = true });
        }

        Dictionary<SyntaxNode, SyntaxNode> _map = new Dictionary<SyntaxNode, SyntaxNode>();
        public void SemanticalChange(SyntaxNode oldNode, SyntaxNode newNode)
        {
            _map[oldNode] = newNode;

            var oldNodes = oldNode.DescendantNodes().GetEnumerator();
            var newNodes = newNode.DescendantNodes().GetEnumerator();

            while (true)
            {
                if (!oldNodes.MoveNext())
                    break;

                if (!newNodes.MoveNext())
                    break;

                _map[oldNodes.Current] = newNodes.Current;
            }
        }

        public SyntaxNode SemanticalMap(SyntaxNode node)
        {
            return _map[node];
        }

        public SyntaxNode SemanticalMap(SourceSpan src)
        {
            if (LexicalTree == null)
                throw new InvalidOperationException("LexicalTree");

            if (SyntacticalTree == null)
                throw new InvalidOperationException("SyntacticalTree");

            TextSpan lexSpan = mapSource(src);
            if (lexSpan == null)
                return null;

            SyntaxNode node = LexicalTree.FindNode(lexSpan, getInnermostNodeForTie : true);
            if (node == null)
                return null;

            node = mapLexical(node);
            if (node == null)
                return null;

            if (SemanticalTree != null)
                return _map[node];

            return node;
        }

        private SyntaxNode mapLexical(SyntaxNode node)
        {
            throw new NotImplementedException();
        }

        private TextSpan mapSource(SourceSpan src)
        {
            throw new NotImplementedException();
        }

        public SourceSpan SourceMap(SyntaxNode node)
        {
            throw new NotImplementedException();
        }
    }
}
