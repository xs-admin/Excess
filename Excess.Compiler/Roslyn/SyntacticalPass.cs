using Excess.Compiler.Core;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess.Compiler.Roslyn
{
    public class SyntacticalPass : BasePass
    {
        SyntaxNode _root;
        IEnumerable<PendingExtension<SyntaxToken, SyntaxNode>> _extensions;
        public SyntacticalPass(SyntaxNode root, IEnumerable<PendingExtension<SyntaxToken, SyntaxNode>> extensions)
        {
            _root = root;
            _extensions = extensions;
        }

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

            if (_extensions.Any())
                processExtensions();

            var myEvents     = events.poll(passId());
            var matchEvents  = myEvents.OfType<SyntacticalMatchEvent<SyntaxNode>>();
            var customEvents = myEvents.OfType<SyntacticalNodeEvent<SyntaxNode>> ();

            var matchers = GetMatchers(matchEvents);
            var handlers = GetHandlers(customEvents);

            SyntaxRewriter pass = new SyntaxRewriter(_events, _scope, matchers, handlers);
            _root = pass.Visit(_root);

            if (events.check("syntactical-pass").Any())
                return this;

            throw new NotImplementedException();
        }

        private void processExtensions()
        {
            Dictionary<SyntaxNode, PendingExtension<SyntaxToken, SyntaxNode>> extensions = new Dictionary<SyntaxNode, PendingExtension<SyntaxToken, SyntaxNode>>();
            foreach (var ext in _extensions)
                extensions[ext.Node] = ext;

            _root = _root.ReplaceNodes(extensions.Keys, (oldNode, newNode) =>
            {
                var extension = extensions[oldNode];

                SyntacticalMatchResult result = new SyntacticalMatchResult(_scope, _events, newNode);
                return extension.Handler(result, extension.Extension);
            });
        }

        private Dictionary<int, Func<SyntaxNode, SyntaxNode>> GetHandlers(IEnumerable<SyntacticalNodeEvent<SyntaxNode>> events)
        {
            var result = new Dictionary<int, Func<SyntaxNode, SyntaxNode>>();
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
