using System;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Excess.Compiler.Roslyn
{
    public class MappingService : IMappingService<SyntaxToken, SyntaxNode>
    {
        Dictionary<SyntaxNode, SyntaxNode> _map = new Dictionary<SyntaxNode, SyntaxNode>();
        public void Map(SyntaxNode oldNode, SyntaxNode newNode)
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

        public SyntaxNode NodeAt(SyntaxNode node)
        {
            SyntaxNode result;
            if (_map.TryGetValue(node, out result))
                return result;

            return node;
        }

        public SyntaxNode NodeAt(SyntaxNode root, int line, int column)
        {
            var lineString = line.ToString();
            var lineDirective = root
                .DescendantTrivia()
                .OfType<LineDirectiveTriviaSyntax>()
                .Where(ld => ld.Line.Text == lineString)
                .FirstOrDefault();

            if (lineDirective == null)
                return null;

            return lineDirective.Parent;
        }

        public SyntaxToken TokenAt(SyntaxNode root, int line, int column)
        {
            var lineString = line.ToString();
            var lineDirective = root
                .DescendantTrivia()
                .OfType<LineDirectiveTriviaSyntax>()
                .Where(ld => ld.Line.Text == lineString)
                .FirstOrDefault();

            if (lineDirective == null || lineDirective.Parent == null)
                return default(SyntaxToken);

            return lineDirective
                .GetLastToken()
                .GetNextToken();
        }

        Dictionary<int, int> _originalTokens = new Dictionary<int, int>(); //id -> line
        int _line = 1;
        public SyntaxToken Map(SyntaxToken token)
        {
            if (token.HasLeadingTrivia)
            {
                foreach(var trivia in token.LeadingTrivia.Where(trivia => trivia.IsKind(SyntaxKind.EndOfLineTrivia)))
                    _line++;
            }

            var idx = _originalTokens.Count;
            _originalTokens[idx] = _line;

            if (token.HasTrailingTrivia)
            {
                foreach (var trivia in token.TrailingTrivia.Where(trivia => trivia.IsKind(SyntaxKind.EndOfLineTrivia)))
                    _line++;
            }

            return token.WithAdditionalAnnotations(new SyntaxAnnotation("xs", idx.ToString()));
        }

        public string RenderMapping(SyntaxNode node, string fileName)
        {
            var result = new StringBuilder($"#line 1 \"{fileName}\"{Environment.NewLine}");
            var lineNumber = 1;
            foreach (var token in node.DescendantTokens())
            {
                var annotation = token
                    .GetAnnotations("xs")
                    .FirstOrDefault();

                var id = -1;
                var original = -1;
                var lastLine = 0;
                if (annotation != null
                    && int.TryParse(annotation.Data, out id)
                    && _originalTokens.TryGetValue(id, out original))
                {
                    lastLine = lineNumber;
                    lineNumber = original;
                }

                lineNumber = addToken(result, token, lastLine, lineNumber);
            }

            Debug.Assert(lineNumber > 0); //must have mappings
            return result.ToString();
        }

        private int addToken(StringBuilder result, SyntaxToken token, int oldLine, int newLine)
        {
            if (oldLine <= 0 && newLine <= 0) //not actively on an original line
            {
                result.Append(token.ToFullString());
                return 0;
            }
            else if (newLine <= 0) //non-original token while rendering original line
            {
                var pragma = $"#hidden";
                var lineChanged = addTrivia(result, token.LeadingTrivia, firstLineChange: pragma);
                result.Append(token.ToString());
                lineChanged |= addTrivia(result, token.TrailingTrivia,
                    lastLineChange: lineChanged ? null : pragma);

                return lineChanged ? 0 : oldLine;
            }
            else if (oldLine <= 0) //just found an original line
            {
                var pragma = $"#line {newLine}";
                addTrivia(result, token.LeadingTrivia, lastLineChange: pragma);
                result.Append(token.ToString());
                var lineChanged = addTrivia(result, token.TrailingTrivia, firstLineChange: "#hidden");
                return lineChanged ? 0 : newLine;
            }
            else //found original line while rendering another
            {
                if (oldLine == newLine) //on the same line
                {
                    addTrivia(result, token.LeadingTrivia);
                    result.Append(token.ToString());
                    var lineChanged = addTrivia(result, token.TrailingTrivia, firstLineChange: "#hidden");
                    return lineChanged ? 0 : newLine;
                }
                else
                {
                    addTrivia(result, token.LeadingTrivia);
                    result.Append(token.ToString());
                    var lineChanged = addTrivia(result, token.TrailingTrivia, firstLineChange: "#hidden");
                    return lineChanged ? 0 : newLine;
                }
            }
        }

        private bool addTrivia(StringBuilder result, SyntaxTriviaList triviaList, string firstLineChange = null, string lastLineChange = null)
        {
            var first = true;
            var line = new StringBuilder();
            var lineChanged = false;
            foreach (var trivia in triviaList)
            {
                if (trivia.IsKind(SyntaxKind.EndOfLineTrivia))
                {
                    lineChanged = true;
                    result.Append(line.ToString() + Environment.NewLine); line.Clear();
                    if (first && firstLineChange != null)
                        result.Append(firstLineChange + Environment.NewLine);
                }
                else 
                    line.Append(trivia.ToString());

                first = false;
            }

            if (lastLineChange != null)
            {
                if (lineChanged)
                    result.Append(lastLineChange + Environment.NewLine);
                else
                    result.Append(Environment.NewLine + lastLineChange + Environment.NewLine);
            }

            result.Append(line.ToString());
            return lineChanged;
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
    }
}
