using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Excess.Runtime;

namespace $safeprojectname$
{
    class Program
    {
        static void Main(string[] args)
        {
        var serverName = default(string);
        switch (args.Length)
        {
            case 0: break;
            case 1:
                serverName = args[0];
                break;
            default:
                Console.WriteLine("please provide one and only one server");
                return;
        }

        //init the app
        var global = Application.Load(new[] { typeof(Program).Assembly });

        //find the server
        var servers = global.get<IEnumerable<IServer>>();
        if (servers == null || !servers.Any())
        {
            Console.WriteLine("no servers are registered");
            return;
        }

        if (serverName == null)
            serverName = "Default";

        var serverInstance = servers
            .SingleOrDefault(server => server.Name == serverName);

        if (serverInstance == null)
        {
            Console.WriteLine($"no server is registered under the name {serverName}");
            return;
        }

        //run
        serverInstance.Run();
    }
}
}

