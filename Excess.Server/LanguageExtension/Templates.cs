using Excess.Compiler.Roslyn;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace LanguageExtension
{
    using CSharp = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

    public static class Templates
    {
        public static Template ConfigClass = Template.Parse(@"
            public class _0
            {
                public void Deploy()
                {
                }

                public void Start(IInstantiator instantiator)
                {
                }
            }");

        public static Template NodeMethod = Template.Parse(@"
            public void _0(IInstantiator instantiator)
            {
            }");

        public static Template HttpServer = Template.ParseStatement(@"
            Startup.HttpServer.Start(
                url: __0, 
                threads: __1
                classes: instantiator.ConcurrentClasses(),
                instances: instantiator.ConcurrentInstances(except: __2));");

        public static Template NodeServer = Template.ParseStatement(@"
            Startup._0.Start(
                url: __1, 
                threads: __2
                classes: instantiator.ConcurrentClasses(),
                instances: instantiator.ConcurrentInstances(only: __3));");

        public static Template NodeConnection = Template.ParseExpression("new _0(__1)");
        

        public static ExpressionSyntax DefaultThreads = CSharp.ParseExpression("8");
        public static ArrayCreationExpressionSyntax TypeArray = Template
            .ParseExpression("new Type[] {}")
            .Get<ArrayCreationExpressionSyntax>();

        public static ArrayCreationExpressionSyntax NodeArray = Template
            .ParseExpression("new IConcurrentNode[] {}")
            .Get<ArrayCreationExpressionSyntax>();

        public static StatementSyntax CreateInstantiator = CSharp
            .ParseStatement("instantiator = instantiator ?? new AssemblyInstantiator(this.GetType().Assembly);");
    }
}
