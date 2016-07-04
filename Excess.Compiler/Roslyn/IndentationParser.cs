using Excess.Compiler.Core;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using System.Diagnostics;

namespace Excess.Compiler.Roslyn
{
    public class IndentationParser
    {
        public static IndentationNode Parse(IEnumerable<SyntaxToken> tokens, int step)
        {
            var lines = readLines(tokens);
            var rootIndent = getIndentation(lines[0], step);
            var root = new IndentationNode(null, rootIndent - 1);
            var current = root;
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line) || isComment(line))
                    continue;

                var indent = getIndentation(line, step);
                current = newNode(current, indent);
                current.SetValue(line.TrimStart());
            }

            return root;
        }

        private static bool isComment(string line)
        {
            var count = 0;
            foreach (var ch in line)
            {
                if (char.IsWhiteSpace(ch))
                {
                    if (count == 0)
                        continue;

                    return false;
                }

                if (ch == '/')
                {
                    count++;
                    if (count == 2)
                        return true;
                }
                else
                {
                    return false;
                }
            }

            return false;
        }

        private static int getIndentation(string value, int step)
        {
            int result = 0;
            foreach (var ch in value)
            {
                if (ch == '\t')
                    result += step;
                else if (char.IsWhiteSpace(ch))
                    result++;
                else
                    break;
            }

            if (result % step != 0)
                throw new InvalidOperationException("bad indentation"); //td: error

            return result / step;
        }

        private static string[] readLines(IEnumerable<SyntaxToken> tokens)
        {
            var result = new List<string>();
            var line = new StringBuilder();
            foreach (var token in tokens)
            {
                if (token.HasLeadingTrivia)
                    addTrivia(token.LeadingTrivia, line, result);

                line.Append(token.ToString());

                if (token.HasTrailingTrivia)
                    addTrivia(token.TrailingTrivia, line, result);
            }

            return result.ToArray();
        }

        private static void addTrivia(SyntaxTriviaList triviaList, StringBuilder line, List<string> result)
        {
            foreach (var trivia in triviaList)
            {
                if (trivia.IsKind(SyntaxKind.EndOfLineTrivia))
                {
                    result.Add(line.ToString());
                    line.Clear();
                }
                else
                    line.Append(trivia.ToString());
            }
        }

        private static IndentationNode newNode(IndentationNode node, int depth)
        {
            if (node == null)
                return new IndentationNode(null, depth);

            var diff = depth - node.Depth;
            if (diff > 0)
            {
                if (diff > 1)
                    throw new ArgumentException("indentation");
            }
            else if (diff < 0)
            {
                while (node != null && node.Depth >= depth)
                    node = node.Parent;

                if (node == null)
                    throw new ArgumentException("indentation");
            }
            else
            {
                node = node.Parent;
            }

            
            if (depth - node.Depth != 1)
                throw new ArgumentException("indentation");

            var result = new IndentationNode(node, depth);
            node.AddChild(result);
            return result;
        }
    }
}
