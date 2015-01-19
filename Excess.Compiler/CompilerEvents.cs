using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess.Compiler
{
    //opaque events tobe passed around during compilation
    public class CompilerEvent
    {
        public CompilerEvent(CompilerStage stage)
        {
            Stage = stage;
        }

        public CompilerStage Stage { get; }
    }

    public class EventBus
    {
        public IEnumerable<CompilerEvent> poll(CompilerStage stage)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<CompilerEvent> poll(string pass)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> poll<T>() where T : CompilerEvent
        {
            throw new NotImplementedException();
        }

        public void push(IEnumerable<CompilerEvent> events)
        {
            throw new NotImplementedException();
        }
    }

    //expected events
    public class LexicalMatchEvent<TToken> : CompilerEvent
    {
        public LexicalMatchEvent(IEnumerable<ILexicalMatch<TToken>> matchers) :
            base(CompilerStage.Lexical)
        {
            Matchers = matchers;
        }

        public IEnumerable<ILexicalMatch<TToken>> Matchers { get; set; }
    };

    public class SyntacticalMatchEvent<TNode> : CompilerEvent
    {
        public SyntacticalMatchEvent(IEnumerable<ISyntacticalMatch<TNode>> matchers) :
            base(CompilerStage.Syntactical)
        {
            Matchers = matchers;
        }

        public IEnumerable<ISyntacticalMatch<TNode>> Matchers { get; set; }
    };
}
