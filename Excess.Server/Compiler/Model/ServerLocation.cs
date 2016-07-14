using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess.Server.Compiler.Model
{
    public class ServerLocation : ServerModel
    {
        public ServerLocation()
        {
            Port = -1;
        }

        public string Url { get; set; }
        public int Port { get; set; }

        public string Address {
            get
            {
                var result = Url ?? "http://localhost";
                if (Port > 0)
                {
                    //td: ought to be a better way
                    result += ":" + Port;
                }

                return result;
            }
        }

        public bool isLocal() => Url == null || Url.Contains("localhost");
    }
}
