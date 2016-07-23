using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess.Server.Compiler.Model
{
    public class SQLLocation : ServerModel
    {
        public string ConnectionId { get; set; }
        public string ConnectionString { get; set; }
    }
}
