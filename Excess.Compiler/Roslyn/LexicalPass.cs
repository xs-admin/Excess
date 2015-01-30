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
        //td: refactor
        //    public LexicalPass(string text) :
        //        base(text)
        //    {
        //    }

        //    internal SyntaxNode Root { get; private set; }
        //    internal string NewText { get; private set; }

        //    protected override string passId()
        //    {
        //        return "lexical-pass";
        //    }

        //    protected override CompilerStage passStage()
        //    {
        //        return CompilerStage.Lexical;
        //    }

        //    protected override ICompilerPass continuation(IEventBus events, Scope scope, string transformed, IEnumerable<PendingExtension<SyntaxToken, SyntaxNode>> extensions, IDictionary<SourceSpan, ISyntaxTransform<SyntaxNode>> transforms)
        //    {
        //        NewText = transformed;
        //        Root = CSharp.ParseCompilationUnit(transformed);

        //        Root = processPending(Root, extensions, transforms);

        //        return new SyntacticalPass(Root);
        //    }

        //    private SyntaxNode processPending(SyntaxNode root, IEnumerable<PendingExtension<SyntaxToken, SyntaxNode>> extensions, IDictionary<SourceSpan, ISyntaxTransform<SyntaxNode>> transforms)
        //    {
        //        var marks    = new Dictionary<SyntaxNode, string>();
        //        var handlers = new Dictionary<string, Func<Scope, SyntaxNode>>();

        //        foreach (var extension in extensions)
        //        {
        //            SyntaxNode extNode = root.FindNode(new TextSpan(extension.Span.Start, extension.Span.Length));
        //            switch (extension.Extension.Kind)
        //            {
        //                case ExtensionKind.Code: extNode = extNode.FirstAncestorOrSelf<ExpressionStatementSyntax>(); break;
        //                case ExtensionKind.Member: extNode = extNode.FirstAncestorOrSelf<MemberDeclarationSyntax>(); break;
        //                case ExtensionKind.Type: extNode = extNode.FirstAncestorOrSelf<MemberDeclarationSyntax>(); break;
        //            }

        //            string extMark = RoslynCompiler.uniqueId();

        //            marks[extNode]    = extMark;
        //            handlers[extMark] = TransformExtension(extension);
        //        }

        //        foreach (var transform in transforms)
        //        {
        //            var transformNode = root.FindNode(new TextSpan(transform.Key.Start, transform.Key.Length));
        //            transformNode = transform.Value.mapTransform(transformNode);

        //            string xformMark = RoslynCompiler.uniqueId();
        //            marks[transformNode] = xformMark;
        //            handlers[xformMark] = ApplyTransform(transform.Value);
        //        }

        //        if (marks.Count > 0)
        //        {
        //            root = root.ReplaceNodes(marks.Keys, (oldNode, newNode) => RoslynCompiler.MarkNode(newNode, marks[oldNode]));

        //            root = root.ReplaceNodes(root.GetAnnotatedNodes(RoslynCompiler.NodeIdAnnotation), (oldNode, newNode) =>
        //            {
        //                var mark    = RoslynCompiler.NodeMark(oldNode);
        //                var handler = handlers[mark];

        //                RoslynSyntacticalMatchResult result = new RoslynSyntacticalMatchResult(_scope, _events, newNode);
        //                return handler(result);
        //            });
        //        }

        //        return root;
        //    }

        //    private Func<Scope, SyntaxNode> ApplyTransform(ISyntaxTransform<SyntaxNode> transformer)
        //    {
        //        return result => transformer.transform(result);
        //    }

        //    private Func<Scope, SyntaxNode> TransformExtension(PendingExtension<SyntaxToken, SyntaxNode> extension)
        //    {
        //        return result => extension.Handler(result, extension.Extension);
        //    }

        //    protected override IEnumerable<SyntaxToken> parseTokens(string text)
        //    {
        //        //td: ! compiler service
        //        return CSharp.ParseTokens(text);
        //    }

        //    protected override string tokenToString(SyntaxToken token, out string lexicalId)
        //    {
        //        lexicalId = RoslynCompiler.GetLexicalId(token);
        //        return token.ToFullString();
        //    }
        }
    }
