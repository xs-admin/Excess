using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Excess.Core;
using System.Net;
using Excess.DSL;

namespace Excess
{
    public class DSLService : IDSLService
    {
        public IDSLFactory AppFactory { set { _appFactory = value; } }

        public IDSLFactory factory()
        {
            IDSLFactory result = new SimpleFactory()
                .Add<ASynchDSL>("asynch")
                .Add<SynchDSL>("synch")
                .AddFactory(PureFactory.Create());

            if (_appFactory != null)
                result = new CompoundFactory(result, _appFactory);
            
            return result;
        }

        private IDSLFactory _appFactory;
    }
}
