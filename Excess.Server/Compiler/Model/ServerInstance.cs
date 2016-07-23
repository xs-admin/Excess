using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess.Server.Compiler.Model
{
    public class ServerInstance : ServerModel
    {
        public ServerInstance()
        {
            Threads = 4;
            Nodes = new List<ServerInstance>();
            DeployStatements = new List<StatementSyntax>();
            HostedClasses = new List<string>();
            Host = new ServerLocation();
        }

        public string Id { get; set; }
        public ServerInstance Parent { get; set; }
        public ServerLocation Host { get; private set; }
        public SQLLocation  SQL { get; private set; }

        public string Identity { get; set; }
        public int Threads { get; set; }
        public List<ServerInstance> Nodes { get; private set; }
        public List<StatementSyntax> DeployStatements { get; private set; }
        public List<string> HostedClasses { get; private set; }

        public string Url
        {
            get { return Host.Url; }
            set { Host.Url = value; }
        }

        public int Port 
        {
            get { return Host.Port; }
            set { Host.Port = value; }
        }

        public void SetSqlLocation(SQLLocation sql)
        {
            if (SQL != null)
                throw new InvalidOperationException("already has sql");

            SQL = sql;
        }
    }
}
