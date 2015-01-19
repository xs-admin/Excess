using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess.Compiler.Core
{
    internal class PassManager
    {
        Dictionary<string, List<CompilerEvent>> _events = new Dictionary<string, List<CompilerEvent>>();

        public void scheduleEvent(string pass, CompilerEvent ev)
        {
            List<CompilerEvent> events;
            if (!_events.TryGetValue(pass, out events))
                events = _events[pass] = new List<CompilerEvent>();
            
            events.Add(ev);
        }

        public IEnumerable<CompilerEvent> passEvents(string pass)
        {
            return _events[pass];
        }
    }
    
    public abstract class BasePass : ICompilerPass
    {
        protected static string        PassId;
        protected static CompilerStage PassStage = CompilerStage.Started;

        public BasePass()
        {
        }

        public string Id
        {
            get
            {
                return PassId;
            }
        }

        public CompilerStage Stage
        {
            get
            {
                return PassStage;
            }
        }

        public abstract ICompilerPass Compile(EventBus events, Scope scope);
    }
}
