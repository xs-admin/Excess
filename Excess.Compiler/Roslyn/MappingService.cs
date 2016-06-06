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
    using CSharp = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

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

        private void appendTrivia(SyntaxTriviaList triviaList, StringBuilder file, StringBuilder line, ref int lineNumber)
        {
            var lineChanged = false;
            var tmpLine = default(StringBuilder);
            foreach (var trivia in triviaList)
            {
                var endOfLine = trivia.IsKind(SyntaxKind.EndOfLineTrivia);

                if (lineChanged)
                {
                    if (endOfLine)
                    {
                        file.AppendLine(tmpLine.ToString());
                        tmpLine.Clear();
                    }
                    else
                        tmpLine.Append(trivia.ToString());

                    continue;
                }

                if (endOfLine)
                {
                    //insert our current line
                    lineChanged = true;
                    tmpLine = new StringBuilder();

                    if (lineNumber > 0)
                    {
                        file.AppendLine($"#line {lineNumber}");
                        lineNumber = -1;
                    }
                    else if (lineNumber < 0)
                    {
                        file.AppendLine($"#line hidden");
                        lineNumber = 0;
                    }

                    file.AppendLine(line.ToString());
                    line.Clear();
                }
                else line.Append(trivia.ToString());
            }

            if (tmpLine != null)
                line.Append(tmpLine);
        }

        private bool isOriginalToken(SyntaxToken token, out int originalLine)
        {
            var annotation = token
                .GetAnnotations("xs")
                .FirstOrDefault();

            var id = -1;
            originalLine = 0;
            if (annotation != null
                && int.TryParse(annotation.Data, out id))
                return _originalTokens.TryGetValue(id, out originalLine);
            return false;
        }

        public string RenderMapping(SyntaxNode node, string fileName)
        {
            //make sure we're normalized
            node = node.NormalizeWhitespace();

            var file = new StringBuilder();
            var line = new StringBuilder();
            var lineNumber = -1;
            foreach (var token in node.DescendantTokens())
            {
                if (token.HasLeadingTrivia)
                    appendTrivia(token.LeadingTrivia, file, line, ref lineNumber);

                var originalLine = 0;
                if (isOriginalToken(token, out originalLine) && originalLine != lineNumber)
                {
                    if (lineNumber > 0)
                    {
                        //2 originals on a line
                        appendTrivia(CSharp.TriviaList(CSharp.SyntaxTrivia(
                            SyntaxKind.EndOfLineTrivia, string.Empty)),
                            file, line, ref lineNumber);
                    }

                    lineNumber = originalLine;
                }

                //append the rest
                line.Append(token.ToString());

                if (token.HasTrailingTrivia)
                    appendTrivia(token.TrailingTrivia, file, line, ref lineNumber);
            }

            file.Append(line);
            return file.ToString();
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
