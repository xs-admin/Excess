using System;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.Text;

namespace Excess.Compiler.Roslyn
{
    using Microsoft.CodeAnalysis.CSharp;
    using System.Diagnostics;
    using CSharp = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

    public class MappingService : IMappingService<SyntaxToken, SyntaxNode>
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
            SyntaxNode result;
            if (_map.TryGetValue(node, out result))
                return result;

            return node;
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

        Dictionary<int, int> _originalTokens = new Dictionary<int, int>(); //id -> line
        int _line = 1;
        public SyntaxToken Transform(SyntaxToken token)
        {
            if (token.HasLeadingTrivia && token.LeadingTrivia.Any(trivia => trivia.IsKind(SyntaxKind.EndOfLineTrivia)))
                _line++;

            var idx = _originalTokens.Count;
            _originalTokens[idx] = _line;

            if (token.HasTrailingTrivia && token.TrailingTrivia.Any(trivia => trivia.IsKind(SyntaxKind.EndOfLineTrivia)))
                _line++;

            return token.WithAdditionalAnnotations(new SyntaxAnnotation("xs", idx.ToString()));
        }

        public string MapLines(SyntaxNode node, string fileName)
        {
            var result = new StringBuilder($"#line 1 \"{fileName}\"{Environment.NewLine}");
            var line = new StringBuilder();
            var lineNumber = 0;
            var lineChanged = 0;
            foreach (var token in node.DescendantTokens())
            {
                var annotation = token
                    .GetAnnotations("xs")
                    .FirstOrDefault();

                var id = -1;
                var original = -1;
                if (annotation != null 
                    && int.TryParse(annotation.Data, out id)
                    && _originalTokens.TryGetValue(id, out original))
                {
                    if (original != lineNumber)
                    {
                        if (lineNumber > 1)
                        {
                            if (lineChanged != lineNumber)
                                result.AppendLine(); //more than one original line on a generated line

                            result.AppendLine($"#line {original}");
                            AppendCurrent(result, line, false);
                        }
                        else if (lineNumber > 0)
                            AppendCurrent(result, line, true);

                        lineNumber = original;
                    }
                }

                line.Append(token.ToFullString());
                if (token.HasTrailingTrivia && token.TrailingTrivia.Any(trivia => trivia.IsKind(SyntaxKind.EndOfLineTrivia)))
                {
                    AppendCurrent(result, line, false);
                    lineChanged = lineNumber;
                }
            }

            Debug.Assert(lineNumber > 0); //must have mappings
            AppendCurrent(result, line, false);
            return result.ToString();
        }

        private void AppendCurrent(StringBuilder result, StringBuilder line, bool asLine)
        {
            if (asLine)
                result.AppendLine(line.ToString());
            else
                result.Append(line.ToString());

            line.Clear();
        }

        public SyntaxNode AppyMappings(SyntaxNode root, Dictionary<int, string> mappings)
        {
            return root.ReplaceTokens(root.DescendantTokens(),
                (ot, nt) =>
                {
                    var annotation = default(string);
                    if (mappings.TryGetValue(ot.SpanStart, out annotation))
                        return nt.WithAdditionalAnnotations(new SyntaxAnnotation("xs", annotation));
                    return nt;
                });
                
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
