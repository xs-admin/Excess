using Excess.Compiler.Roslyn;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace LanguageExtension
{
    using Excess.Compiler.Razor;
    using CSharp = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

    public static class Templates
    {
        public static Template ConfigClass = Template.Parse(@"
            [ServerConfiguration]    
            public class _0
            {
                public void Deploy()
                {
                }

                public void Start(IInstantiator instantiator)
                {
                }

                public void StartNodes(IInstantiator instantiator)
                {
                    instantiator = instantiator ?? new AssemblyInstantiator(this.GetType().Assembly);
                }
            }");

        public static Template NodeMethod = Template.Parse(@"
            public void _0(IInstantiator instantiator)
            {
            }");

        public static Template HttpServer = Template.ParseStatement(@"
            Startup.HttpServer.Start(
                url: __0, 
                identityUrl: __3,
                threads: __1,
                classes: instantiator.GetConcurrentClasses(),
                instances: instantiator.GetConcurrentInstances(except: __2));");

        public static Template NodeServer = Template.ParseStatement(@"
            Startup._0.Start(
                url: __1, 
                identityUrl: __4,
                threads: __2,
                classes: instantiator.GetConcurrentClasses(),
                instances: instantiator.GetConcurrentInstances(only: __3));");

        public static Template NodeConnection = Template.ParseExpression("new _0(__1)");
        

        public static ExpressionSyntax DefaultThreads = CSharp.ParseExpression("8");
        public static ArrayCreationExpressionSyntax TypeArray = Template
            .ParseExpression("new Type[] {}")
            .Get<ArrayCreationExpressionSyntax>();

        public static StatementSyntax CreateInstantiator = CSharp.ParseStatement(@"
            instantiator = instantiator ?? new AssemblyInstantiator(this.GetType().Assembly);");

        public static RazorTemplate jsConcurrentClass = RazorTemplate.Parse(@"
            function @Model.Name (__init)
            {
                @Model.Body

                this.__ID = __init.__ID;
            }");

        public static RazorTemplate jsMethod = RazorTemplate.Parse(@"
            this.@Model.Name = function @Model.Arguments
            {
                var deferred = $q.defer();

                $http.post('/' + __init.__ID + '/@Model.Name', 
                {
                    @Model.Data
                })
                .success(function(response)
                {
                    deferred.resolve(@Model.Response);
                })
                .failure(function(ex)
                {
                    deferred.reject(ex);
                });

                return deferred.promise;
            }");

        public static RazorTemplate jsProperty = RazorTemplate.Parse(@"
            this.@Model.Name = __init.@Model.Name;");

        public static Template NodeInvocation = Template.ParseStatement(@"
            _0(instantiator);");
        
    }
}
