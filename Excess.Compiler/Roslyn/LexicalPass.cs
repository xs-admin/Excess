using Excess.Compiler.Core;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess.Compiler.Roslyn
{
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Text;
    using CSharp = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

    public class LexicalPass : BaseLexicalPass<SyntaxToken, SyntaxNode>
    {
        public LexicalPass(string text) :
            base(text)
        {
        }

        internal SyntaxNode Root { get; private set; }
        internal string NewText { get; private set; }

        protected override string passId()
        {
            return "lexical-pass";
        }

        protected override CompilerStage passStage()
        {
            return CompilerStage.Lexical;
        }

        protected override IEnumerable<SyntaxToken> markTokens(IEnumerable<SyntaxToken> tokens, out string id)
        {
            var uid = RoslynCompiler.uniqueId();
            id = uid;
            return tokens.Select(token  =>
            {
                return RoslynCompiler.SetLexicalId(token, uid);
            });
        }

        protected override ICompilerPass continuation(IEventBus events, Scope scope, string transformed, IEnumerable<PendingExtension<SyntaxToken, SyntaxNode>> extensions, IDictionary<SourceSpan, ISyntaxTransform<SyntaxNode>> transforms)
        {
            NewText = transformed;
            Root = CSharp.ParseCompilationUnit(transformed);

            Root = processPending(Root, extensions, transforms);

            return new SyntacticalPass(Root);
        }

        private SyntaxNode processPending(SyntaxNode root, IEnumerable<PendingExtension<SyntaxToken, SyntaxNode>> extensions, IDictionary<SourceSpan, ISyntaxTransform<SyntaxNode>> transforms)
        {
            var replace = new Dictionary<SyntaxNode, SyntaxNode>();
            foreach (var extension in extensions)
            {
                SyntaxNode extNode = Root.FindNode(new TextSpan(extension.Span.Start, extension.Span.Length));
                switch (extension.Extension.Kind)
                {
                    case ExtensionKind.Code: extNode = extNode.FirstAncestorOrSelf<ExpressionStatementSyntax>(); break;
                    case ExtensionKind.Member: extNode = extNode.FirstAncestorOrSelf<MemberDeclarationSyntax>(); break;
                    case ExtensionKind.Type: extNode = extNode.FirstAncestorOrSelf<MemberDeclarationSyntax>(); break;
                }

                RoslynSyntacticalMatchResult result = new RoslynSyntacticalMatchResult(_scope, _events, extNode);
                var resultNode = extension.Handler(result, extension.Extension);

                replace[extNode] = resultNode;
            }

            foreach (var transform in transforms)
            {
                SyntaxNode transformNode = Root.FindNode(new TextSpan(transform.Key.Start, transform.Key.Length));

                RoslynSyntacticalMatchResult result = new RoslynSyntacticalMatchResult(_scope, _events, transformNode);
                var resultNode = transform.Value.transform(result);

                replace[transformNode] = resultNode;
            }

            if (replace.Count > 0)
                return root.ReplaceNodes(replace.Keys, (oldNode, newNode) => replace[oldNode]);

            return root;
        }

        protected override IEnumerable<SyntaxToken> parseTokens(string text)
        {
            //td: ! compiler service
            return CSharp.ParseTokens(text);
        }

        protected override string tokenToString(SyntaxToken token, out string lexicalId)
        {
            lexicalId = RoslynCompiler.GetLexicalId(token);
            return token.ToFullString();
        }
    }
}
