using Microsoft.CodeAnalysis.CSharp.Syntax;
using Excess.Compiler.Roslyn;

namespace Excess.Server.Compiler
{
    using System;
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

        public static string jsConcurrentClass(dynamic Model)
        {
            var template = new ASP._Templates_jsConcurrentClass_template()
            {
                Model = Model
            };
            return template.TransformText();
        }

        public static string jsMethod(dynamic Model)
        {
            var template = new ASP._Templates_jsMethod_template()
            {
                Model = Model
            };

            return template.TransformText();
        }

        public static string jsProperty(dynamic Model)
        {
            var template = new ASP._Templates_jsProperty_template()
            {
                Model = Model
            };
            return template.TransformText();
        }

        public static Template NodeInvocation = Template.ParseStatement(@"
            _0(commonClasses);");

        public static StatementSyntax RemoteTypes = CSharp.ParseStatement(@"
            return new Type[] {};");

        public static string jsService(dynamic Model)
        {
            var template = new ASP._Templates_jsService_template()
            {
                Model = Model
            };
            return template.TransformText();
        }

        public static string jsServiceFile(dynamic Model)
        {
            var template = new ASP._Templates_jsServiceFile_template()
            {
                Model = Model
            };

            return template.TransformText();
        }

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
