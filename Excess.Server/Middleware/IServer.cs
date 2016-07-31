using Excess.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess.Server.Middleware
{
    public interface IServer
    {
        string Name { get; }

        void Run(__Scope scope);
        void Run(__Scope scope, Action<object> success, Action<Exception> failure);
        void Deploy();
    }
}
