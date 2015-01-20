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
        static SyntacticalPass()
        {
            PassId = "syntactical-pass";
            PassStage = CompilerStage.Syntactical;
        }

        SyntaxNode _root;
        public SyntacticalPass(SyntaxNode root)
        {
            _root = root;
        }

        public override ICompilerPass Compile(IEventBus events, Scope scope)
        {
            var myEvents     = events.poll(PassId);
            var matchEvents  = myEvents.OfType<SyntacticalMatchEvent<SyntaxNode>>();
            var customEvents = myEvents.OfType<SyntacticalNodeEvent<SyntaxNode>> ();

            var matchers = GetMatchers(matchEvents);
            var handlers = GetHandlers(customEvents);

            SyntaxRewriter pass = new SyntaxRewriter(matchers, handlers);
            _root = pass.Visit(_root);

            if (events.check("syntactical-pass").Any())
                return this;

            throw new NotImplementedException();
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
