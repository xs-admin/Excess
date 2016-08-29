using Microsoft.CodeAnalysis.CSharp.Syntax;
using Excess.Compiler.Roslyn;
using Excess.Compiler.Razor;

namespace Excess.Server.Compiler
{
    using CSharp = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

    public static class Templates
    {
        public static Template ServerInstance = Template.Parse(@"
            public class _0 : IServer
            {
                public string Name => __1;

                public void Run(__Scope __scope)
                {
                }

                public void Run(__Scope __scope, Action<object> success, Action<Exception> failure)
                {
                }

                public void Deploy()
                {
                    throw new NotImplementedException();
                }
            }");

        public static Template StartHttpServer = Template.ParseStatement(@"
            HttpServer.Start(
                url: __0, 
                scope: __scope,
                identityUrl: __1,
                staticFiles: __2,
                threads: __3, 
                except: __4,
                nodes: __5,
                assemblies: new [] {typeof(_6).Assembly},
                filters: __7);");

        public static Template StartNetMQServer = Template.ParseStatement(@"
            NetMQNode.Start(
                __0,
                __1,
                threads: __2, 
                classes: __3,
                assemblies: new [] {typeof(_4).Assembly});");

        public static Template StringArray = Template.ParseExpression("new string[] {}");
        public static Template CallStartNode = Template.ParseStatement("_0.Start();");

        public static RazorTemplate jsConcurrentClass = RazorTemplate.Parse(@"
            @Model.Name = function (__ID)
            {
                @Model.Body

                this.__ID = __ID;
            }");

        public static RazorTemplate jsMethod = RazorTemplate.Parse(@"
            this.@Model.MethodName = function @Model.Arguments
            {
                var deferred = $q.defer();

                $http.post(@Model.Path, {
                    @Model.Data
                }).then(function(__response) {
                    deferred.resolve(__response.data.__res);
                }, function(ex){
                    deferred.reject(ex);
                });

                return deferred.promise;
            }");

        public static RazorTemplate jsProperty = RazorTemplate.Parse(@"
            this.@Model.Name = __init.@Model.Name;");

        public static Template NodeInvocation = Template.ParseStatement(@"
            _0(commonClasses);");

        public static StatementSyntax RemoteTypes = CSharp.ParseStatement(@"
            return new Type[] {};");

        public static RazorTemplate jsService = RazorTemplate.Parse(@"
            xsServices.service('@Model.Name', ['$http', '$q', function($http, $q)
            {
                @Model.Body

                this.__ID = '@Model.ID';
            }])");

        public static RazorTemplate servicesFile = RazorTemplate.Parse(@"
            var xsServices = angular.module('xs.Services', []);
            @Model.Members");

        public static Template SqlFilter = Template.ParseExpression(@"
                prev =>
                {
                    var connectionString = __0;
                    return (data, request, scope) => 
                    {
                        using (var connection = new __1(connectionString))
                        {
                            scope.set<IDbConnection>(connection);
                            return prev(data, request, scope);
                        }
                    };
                }
            }");

        public static ExpressionSyntax UserFilter = CSharp.ParseExpression(@"
            prev => (data, request, scope) => 
            {
                if (request.User != null)
                    scope.set<IPrincipal>(request.User);
                return prev(data, request, scope);
            }");

        public static Template SqlConnectionStringFromConfig = Template.ParseExpression(
            "ConfigurationManager.ConnectionStrings[__0].ConnectionString");

        public static ArrayCreationExpressionSyntax EmptyArray = Template.ParseExpression(
            "new Func<Func<string, IOwinRequest, __Scope, object>,  Func<string, IOwinRequest, __Scope, object >>[] {}")
            .Get<ArrayCreationExpressionSyntax>();
    }
}
