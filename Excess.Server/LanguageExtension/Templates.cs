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
        Startup.HttpServer.Start(__0, __1,
            classes: instantiator.ConcurrentClasses(this.GetType().Assembly),
            instances: instantiator.ConcurrentInstances(this.GetType().Assembly, except: __2),
            nodes : __3);");

        public static Template NodeServer = Template.ParseStatement(@"
        Startup.__3.Start(__0, __1,
            classes: instantiator.ConcurrentClasses(this.GetType().Assembly),
            instances: instantiator.ConcurrentInstances(only: __2));");

        public static ExpressionSyntax DefaultThreads = CSharp.ParseExpression("8");
        public static ArrayCreationExpressionSyntax TypeArray = Template
            .ParseExpression("new Type[] {}")
            .Get<ArrayCreationExpressionSyntax>();

        public static ArrayCreationExpressionSyntax NodeArray = Template
            .ParseExpression("new IConcurrentNode[] {}")
            .Get<ArrayCreationExpressionSyntax>();
    }
}
