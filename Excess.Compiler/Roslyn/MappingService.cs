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
            if (token.HasLeadingTrivia && token.LeadingTrivia.Any(trivia => trivia.IsKind(SyntaxKind.EndOfLineTrivia)))
                _line++;

            var idx = _originalTokens.Count;
            _originalTokens[idx] = _line;

            if (token.HasTrailingTrivia && token.TrailingTrivia.Any(trivia => trivia.IsKind(SyntaxKind.EndOfLineTrivia)))
                _line++;

            return token.WithAdditionalAnnotations(new SyntaxAnnotation("xs", idx.ToString()));
        }

        public string RenderMapping(SyntaxNode node, string fileName)
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
                        if (original > 1)
                        {
                            if (lineChanged != lineNumber)
                                AppendCurrent(result, line, true); //more than one original line on a generated line

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
    }
}
