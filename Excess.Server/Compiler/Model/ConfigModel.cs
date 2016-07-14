using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess.Server.Compiler.Model
{
    public class ConfigModel : ServerModel
    {
        public ConfigModel()
        {
            Threads = 4;
        }

        public int Threads { get; set; }
        public string Identity { get; set; }
    }
}
