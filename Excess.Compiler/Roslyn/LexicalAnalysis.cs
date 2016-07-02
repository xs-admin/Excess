using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Excess.Compiler.Core;

namespace Excess.Compiler.Roslyn
{
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Text;
    using System.Diagnostics;
    using CSharp = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

    public class RoslynLexicalAnalysis : BaseLexicalAnalysis<SyntaxToken, SyntaxNode, SemanticModel>
    {
        public override IIndentationGrammarAnalysis<SyntaxToken, SyntaxNode, GNode> indented<GNode, GRoot>(string keyword, ExtensionKind kind) 
        {
            return new RoslynIndentationGrammarAnalysis<GNode, GRoot>(this, keyword, kind);
        }

        protected override SyntaxNode normalize(SyntaxNode root, Scope scope)
        {
            IEnumerable<SyntaxNode> rootStatements = null;
            IEnumerable<SyntaxNode> rootMembers = null;
            IEnumerable<SyntaxNode> rootTypes = null;

            var node = root;
            var normalized = false;
            if (_normalizeStatements != null)
                node = normalizeCode(root, out rootStatements, out normalized);

            if (!normalized && (_normalizeMembers != null || _normalizeTypes != null))
                node = normalizePass(root, out rootMembers, out rootTypes, out normalized);

            if (normalized)
            {
                if (_normalizeStatements != null &&  rootStatements != null && rootStatements.Any())
                    node = _normalizeStatements(node, rootStatements, scope);

                if (_normalizeMembers != null)
                {
                    if (node != root)
                    {
                        node = normalizePass(node, out rootMembers, out rootTypes, out normalized);
                        root = node;
                    }

                    if (rootMembers != null && rootMembers.Any())
                        node = _normalizeMembers(node, rootMembers, scope);
                }

                if (_normalizeTypes != null)
                {
                    if (node != root)
                        node = normalizePass(node, out rootMembers, out rootTypes, out normalized);

                    if (rootTypes != null && rootTypes.Any())
                        node = _normalizeTypes(node, rootTypes, scope);
                }
            }

            if (_normalizeThen != null)
                node = _normalizeThen(node, scope);

            return node;
        }

        private SyntaxNode normalizeCode(SyntaxNode root, out IEnumerable<SyntaxNode> rootStatements, out bool normalized)
        {
            normalized = true;
            rootStatements = null;

            //td: line numbers!
            var newTree = CSharp.ParseSyntaxTree("void m() {" + root.ToFullString() + "}", options: new CSharpParseOptions(kind: SourceCodeKind.Script));
            try
            {
                var block = newTree
                    .GetRoot()
                    .ChildNodes()
                    .OfType<MethodDeclarationSyntax>()
                    .Single()
                    .Body;

                if (block.ChildNodes().All(child => child is StatementSyntax))
                {
                    rootStatements = new List<StatementSyntax>(block.ChildNodes().OfType<StatementSyntax>());
                    return block;
                }
            }
            catch
            {
            }

            normalized = false;
            return root;
        }

        private SyntaxNode normalizePass(SyntaxNode root, out IEnumerable<SyntaxNode> rootMembers, out IEnumerable<SyntaxNode> rootTypes, out bool normalized)
        {
            rootMembers = null;
            rootTypes = null;
            normalized = false;

            var tree = root.SyntaxTree;
            var codeErrors = root.GetDiagnostics().Where(error => error.Id == "CS1022").
                                  OrderBy(error => error.Location.SourceSpan.Start).GetEnumerator();

            Diagnostic currError = null;
            int currErrorPos = 0;

            if (codeErrors != null && codeErrors.MoveNext())
                currError = codeErrors.Current;

            BlockSyntax statementBlock;

            List<StatementSyntax> statements = new List<StatementSyntax>();
            List<MemberDeclarationSyntax> members = new List<MemberDeclarationSyntax>();
            List<MemberDeclarationSyntax> types = new List<MemberDeclarationSyntax>();
            List<TextSpan> toRemove = new List<TextSpan>();
            foreach (var child in root.ChildNodes())
            {
                if (child is IncompleteMemberSyntax)
                    continue;

                if (child is FieldDeclarationSyntax)
                {
                    //case: code variable?
                    FieldDeclarationSyntax field = (FieldDeclarationSyntax)child;
                    if (!field.Modifiers.Any())
                    {
                        //td: !!! variable initialization
                        continue;
                    }
                }

                if (child is MethodDeclarationSyntax)
                {
                    //case: bad method?
                    MethodDeclarationSyntax method = (MethodDeclarationSyntax)child;
                    if (method.Body == null)
                        continue;
                }

                if (child is MemberDeclarationSyntax)
                {
                    bool foundError = false;
                    if (currError != null)
                    {
                        if (child.SpanStart > currError.Location.SourceSpan.Start)
                        {
                            var errorSpan = new TextSpan(currErrorPos, child.SpanStart - currErrorPos);
                            SourceText errorSource = tree.GetText().GetSubText(errorSpan);

                            statementBlock = (BlockSyntax)SyntaxFactory.ParseStatement("{" + errorSource + "}");
                            statements.AddRange(statementBlock.Statements);

                            toRemove.Add(errorSpan);

                            foundError = true;
                            currError = null;
                            while (codeErrors.MoveNext())
                            {
                                var nextError = codeErrors.Current;
                                if (nextError.Location.SourceSpan.Start > child.Span.End)
                                {
                                    currError = nextError;
                                    break;
                                }
                            }
                        }
                    }

                    currErrorPos = child.Span.End;
                    var toAdd = child as MemberDeclarationSyntax;

                    if (foundError)
                    {
                        toAdd = toAdd.ReplaceTrivia(child.GetLeadingTrivia(), (oldTrivia, newTrivia) =>
                        {
                            return CSharp.SyntaxTrivia(SyntaxKind.WhitespaceTrivia, string.Empty);
                        });
                    }

                    if (toAdd is TypeDeclarationSyntax || toAdd is EnumDeclarationSyntax)
                        types.Add(toAdd);
                    else if (!(toAdd is NamespaceDeclarationSyntax))
                        members.Add(toAdd);
                }
                else
                {
                    //any other top level construct indicates completeness
                    return root;
                }
            }

            if (currError != null)
            {
                var errorSpan = new TextSpan(currErrorPos, tree.GetRoot().FullSpan.End - currErrorPos);
                SourceText errorSource = tree.GetText().GetSubText(errorSpan);
                statementBlock = (BlockSyntax)SyntaxFactory.ParseStatement("{" + errorSource + "}");
                statements.AddRange(statementBlock.Statements);

                toRemove.Add(errorSpan);
            }

            normalized = statements.Any() || members.Any() || !types.Any();

            if (!normalized)
                return root; //nothing to se here

            rootMembers = members;
            rootTypes = types;

            if (!toRemove.Any())
                return root; //nothing to remove

            return root.RemoveNodes(
                root
                    .ChildNodes()
                    .Where(node => contained(node, toRemove)),
                SyntaxRemoveOptions.KeepEndOfLine);
        }

        private bool contained(SyntaxNode node, List<TextSpan> text)
        {
            foreach(var inner in text)
            {
                if (inner.Contains(node.FullSpan))
                    return true;
            }

            return false;
        }
    }
}
