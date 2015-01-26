using Excess.Compiler.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess.Compiler.Roslyn
{
    public class SyntacticalPass : BasePass
    {
        SyntaxNode _root;
        public SyntacticalPass(SyntaxNode root)
        {
            _root = root;
        }

        public SyntaxTree Tree { get { return _root.SyntaxTree; } }
        protected override string passId()
        {
            return "syntactical-pass";
        }

        protected override CompilerStage passStage()
        {
            return CompilerStage.Syntactical;
        }

        //td: !!! move this to base pass
        IEventBus _events;
        Scope     _scope; 
        public override ICompilerPass Compile(IEventBus events, Scope scope)
        {
            _events = events;
            _scope  = scope;

            var myEvents        = events.poll(passId());
            var extensionEvents = myEvents.OfType<SyntacticExtensionEvent<SyntaxNode>>();
            var matchEvents     = myEvents.OfType<SyntacticalMatchEvent<SyntaxNode>>();
            var customEvents    = myEvents.OfType<SyntacticalNodeEvent<SyntaxNode>> ();

            _root = processExtensions(_root, extensionEvents);

            var matchers = GetMatchers(matchEvents);
            var handlers = GetHandlers(customEvents);

            SyntaxRewriter pass = new SyntaxRewriter(_events, _scope, matchers, handlers);
            _root = pass.Visit(_root);

            if (events.check("syntactical-pass").Any())
                return this;

            return null; //td: !!!
        }

        private SyntaxNode processExtensions(SyntaxNode node, IEnumerable<SyntacticExtensionEvent<SyntaxNode>> events)
        {
            ExtensionRewriter rewriter = new ExtensionRewriter(events, _events);
            return rewriter.Visit(node);
        }

        private SyntaxNode extensionNode(PendingExtension<SyntaxToken, SyntaxNode> extension)
        {
            SyntaxNode result = extension.Node;
            switch (extension.Extension.Kind)
            {
                case ExtensionKind.Code:
                    result = extension.Node
                        .AncestorsAndSelf()
                    
                        .OfType<ExpressionStatementSyntax>()
                        .FirstOrDefault();

                    if (result == null)
                    {
                        //td: error, malformed code extension
                    }
                    break;

                case ExtensionKind.Member:
                    result = extension.Node
                        .AncestorsAndSelf()
                        .OfType<MemberDeclarationSyntax>()
                        .FirstOrDefault();

                    if (result == null)
                    {
                        //td: error, malformed member extension
                    }
                    break;

                case ExtensionKind.Type:
                    result = extension.Node
                        .AncestorsAndSelf()
                        .OfType<TypeDeclarationSyntax>()
                        .FirstOrDefault();

                    if (result == null)
                    {
                        //td: error, malformed type extension
                    }
                    break;
            }

            return result;
        }

        private Dictionary<string, Func<SyntaxNode, SyntaxNode>> GetHandlers(IEnumerable<SyntacticalNodeEvent<SyntaxNode>> events)
        {
            var result = new Dictionary<string, Func<SyntaxNode, SyntaxNode>>();
            foreach(var ev in events)
                result[ev.Node] = ev.Handler;

            return result;
        }

        private IEnumerable<ISyntacticalMatch<SyntaxNode>> GetMatchers(IEnumerable<SyntacticalMatchEvent<SyntaxNode>> events)
        {
            foreach (var ev in events)
            {
                foreach (var matcher in ev.Matchers)
                    yield return matcher;
            }
        }
    }
}
