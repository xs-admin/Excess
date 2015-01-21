using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess.Compiler.Core
{
    public class BaseEventBus : IEventBus
    {
        List<CompilerEvent> _events = new List<CompilerEvent>();
        public IEnumerable<CompilerEvent> check(string pass)
        {
            return _events.Where(ev => ev.Pass == pass);
        }

        public IEnumerable<CompilerEvent> poll(string pass)
        {
            List<CompilerEvent> new_events = new List<CompilerEvent>();
            var result = _events.Select(ev =>
            {
                if (ev.Pass == pass)
                    return ev;

                new_events.Add(ev);
                return null;
            });

            _events = new_events;
            return result;
        }

        public void schedule(IEnumerable<CompilerEvent> evs)
        {
            _events.AddRange(evs);
        }

        public void schedule(CompilerEvent ev)
        {
            _events.Add(ev);
        }

        public void schedule(string pass, IEnumerable<CompilerEvent> evs)
        {
            _events.AddRange(evs.Select(ev =>
            {
                ev.Pass = pass;
                return ev;
            }));
        }

        public void schedule(string pass, CompilerEvent ev)
        {
            ev.Pass = pass;
        }
    }
}
