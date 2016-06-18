using Excess.Compiler.Roslyn;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace LanguageExtension
{
    using Excess.Compiler.Razor;
    using System;
    using CSharp = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

    public static class Templates
    {
        public static Template ConfigClass = Template.Parse(@"
            [ServerConfiguration]    
            public class _0
            {
                public static void Deploy()
                {
                }

                public static void Start()
                {
                }

                public static void StartNodes(IEnumerable<Type> commonClasses)
                {
                }

                public static int NodeCount()
                {
                }

                public static IEnumerable<Type> RemoteTypes()
                {
                }
            }");

        public static Template NodeMethod = Template.Parse(@"
            public static void _0(IEnumerable<Type> commonClasses)
            {
                var hostedTypes = commonClasses.Union(__1);
            }");

        public static Template HttpServer = Template.ParseStatement(@"
            HttpServer.Start(
                url: __0, 
                identityUrl: __1,
                threads: __2);");

        public static Template NodeServer = Template.ParseStatement(@"
            _0.Start(
                localServer: __1, 
                remoteServer: __2,
                threads: __3,
                classes: hostedTypes);");

        public static ExpressionSyntax DefaultThreads = CSharp.ParseExpression("8");
        public static ArrayCreationExpressionSyntax TypeArray = Template
            .ParseExpression("new Type[] {}")
            .Get<ArrayCreationExpressionSyntax>();

        public static RazorTemplate jsConcurrentClass = RazorTemplate.Parse(@"
            @Model.Name = function (__ID)
            {
                @Model.Body

                this.__ID = __ID;
            }");

        public static RazorTemplate jsMethod = RazorTemplate.Parse(@"
            this.@Model.Name = function @Model.Arguments
            {
                var deferred = $q.defer();

                $http.post(@Model.Path + '/@Model.Name', {
                    @Model.Data
                }).then(function(response) {
                    deferred.resolve(response);
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
    }
}
