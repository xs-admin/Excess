using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess.Compiler.Core
{
    public abstract class BasePass : ICompilerPass
    {
        public BasePass()
        {
        }

        public string Id
        {
            get
            {
                return passId();
            }
        }


        public CompilerStage Stage
        {
            get
            {
                return passStage();
            }
        }

        public abstract ICompilerPass Compile(IEventBus events, Scope scope);

        protected abstract string passId();
        protected abstract CompilerStage passStage();
    }
}
