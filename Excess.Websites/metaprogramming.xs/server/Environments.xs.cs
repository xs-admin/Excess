using System;
using System.Collections.Generic;
using System.Linq;
using Excess.Runtime;
using System.Configuration;
using System.Security.Principal;
using Microsoft.Owin;
using Excess.Server.Middleware;

namespace metaprogramming
{
    public class Development : IServer
    {
        public string Name => "Development";
        public void Run(__Scope __scope)
        {
            HttpServer.Start(url: "http://ec2-52-43-215-170.us-west-2.compute.amazonaws.com", scope: __scope, identityUrl: "", staticFiles: @"../../client/app", threads: 4, except: new string[]{}, nodes: 0, assemblies: new[]{typeof (Development).Assembly}, filters: new Func<Func<string, IOwinRequest, __Scope, object>, Func<string, IOwinRequest, __Scope, object>>[]{prev => (data, request, scope) =>
            {
                scope.set<IPrincipal>(request.User);
                return prev(data, request, scope);
            }
            });
        }

        public void Run(__Scope __scope, Action<object> success, Action<Exception> failure)
        {
            HttpServer.Start(url: "http://ec2-52-43-215-170.us-west-2.compute.amazonaws.com", scope: __scope, identityUrl: "", staticFiles: @"../../client/app", threads: 4, except: new string[]{}, nodes: 0, assemblies: new[]{typeof (Development).Assembly}, filters: new Func<Func<string, IOwinRequest, __Scope, object>, Func<string, IOwinRequest, __Scope, object>>[]{prev => (data, request, scope) =>
            {
                scope.set<IPrincipal>(request.User);
                return prev(data, request, scope);
            }
            });
        }

        public void Deploy()
        {
            throw new NotImplementedException();
        }
    }
}