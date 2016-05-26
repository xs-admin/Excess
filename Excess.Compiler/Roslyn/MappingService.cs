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
            var inHiddenPragma = false;
            foreach (var token in node.DescendantTokens())
            {
                var annotation = token
                    .GetAnnotations("xs")
                    .FirstOrDefault();

                var id = -1;
                var original = -1;
                var lastLine = lineNumber;
                if (annotation != null
                    && int.TryParse(annotation.Data, out id)
                    && _originalTokens.TryGetValue(id, out original))
                {
                    lineNumber = original;
                }
                else
                {
                    lineNumber = 0;
                }

                lineNumber = addToken(result, token, lastLine, lineNumber, ref inHiddenPragma);
            }

            Debug.Assert(lineNumber > 0); //must have mappings
            return result.ToString();
        }

        private bool assureHiddenPragma(StringBuilder result, bool inHiddenPragma)
        {
            if (!inHiddenPragma)
            {
                result.AppendLine($"#line hidden{Environment.NewLine}");
                inHiddenPragma = true;
            }

            return inHiddenPragma;
        }

        private int addToken(StringBuilder result, SyntaxToken token, int oldLine, int newLine, ref bool inHiddenPragma)
        {
            var linePragma = $"#line {newLine}";
            //var hiddenPragma = $"#line hidden";
            var afterLineChange = string.Empty;
            var lineChanged = false;

            if (oldLine <= 0 && newLine <= 0) //not actively on an original line
            {
                inHiddenPragma = assureHiddenPragma(result, inHiddenPragma);
                result.Append(token.ToFullString());
                return 0;
            }
            else if (newLine <= 0) //non-original token while rendering original line
            {
                afterLineChange = addTrivia(result, token.LeadingTrivia);
                lineChanged = afterLineChange != null;
                if (lineChanged)
                {
                    result.Append(afterLineChange);
                    inHiddenPragma = assureHiddenPragma(result, inHiddenPragma);
                }

                restOfToken(result, token, ref lineChanged);

                if (lineChanged)
                    inHiddenPragma = assureHiddenPragma(result, inHiddenPragma);
                return lineChanged ? 0 : oldLine;
            }
            else if (oldLine <= 0) //just found an original line
            {
                inHiddenPragma = false;
                afterLineChange = addTrivia(result, token.LeadingTrivia, linePragma);
                if (afterLineChange != null)
                    result.Append(afterLineChange);

                restOfToken(result, token, ref lineChanged);
                return lineChanged ? 0 : newLine;
            }
            else //found original line while rendering another
            {
                inHiddenPragma = false;
                if (oldLine == newLine) //on the same line
                {
                    addTrivia(result, token.LeadingTrivia);
                    result.Append(token.ToString());
                    afterLineChange = addTrivia(result, token.TrailingTrivia);
                    if (afterLineChange != null)
                    {
                        result.Append(afterLineChange);
                        return 0;
                    }

                    return newLine;
                }
                else
                {
                    afterLineChange = addTrivia(result, token.LeadingTrivia, linePragma);
                    if (afterLineChange != null)
                        result.Append(afterLineChange);

                    result.Append(token.ToString());
                    afterLineChange = addTrivia(result, token.TrailingTrivia);
                    if (afterLineChange != null)
                    {
                        result.Append(afterLineChange);
                        return 0;
                    }

                    return newLine;
                }
            }
        }

        private void restOfToken(StringBuilder result, SyntaxToken token, ref bool lineChanged)
        {
            result.Append(token.ToString());
            if (token.HasTrailingTrivia)
            {
                foreach (var trivia in token.TrailingTrivia)
                {
                    if (trivia.IsKind(SyntaxKind.EndOfLineTrivia))
                        lineChanged = true;

                    result.Append(trivia.ToString());
                }
            }
        }

        private string addTrivia(StringBuilder result, SyntaxTriviaList triviaList, string pragma = null)
        {
            var line = new StringBuilder();
            var lineChanged = false;
            foreach (var trivia in triviaList)
            {
                if (trivia.IsKind(SyntaxKind.EndOfLineTrivia))
                {
                    lineChanged = true;
                    result.Append(line.ToString() + Environment.NewLine); line.Clear();
                }
                else 
                    line.Append(trivia.ToString());
            }

            if (lineChanged)
            {
                if (pragma != null)
                    result.Append(pragma + Environment.NewLine);
                return line.ToString();
            }

            if (pragma != null)
                result.Append(Environment.NewLine + pragma + Environment.NewLine);

            result.Append(line.ToString());
            return null;
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
