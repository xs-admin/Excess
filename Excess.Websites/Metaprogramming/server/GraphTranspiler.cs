using System;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Microsoft.CodeAnalysis;
using Excess.Compiler;
using Excess.Compiler.Roslyn;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace metaprogramming.server
{
    using Excess.Compiler.GraphInstances;
    using CSharp = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

    class GraphTranspiler : IGraphTranspiler
    {
        RoslynCompiler _compiler = new RoslynCompiler();
        public GraphTranspiler()
        {
            _compiler = CreateCompiler();
        }

        public string Process(string source)
        {
            var scope = new Scope(_compiler.Scope);
            var jobject = JObject.Parse(source);
            var doc = GraphLoader.FromWebNodes(jobject, scope, serializers : GraphModel.Serializers);

            var text = string.Empty;
            _compiler.CompileInstance(doc, out text);
            return text;
        }

        //compiler
        private RoslynCompiler CreateCompiler()
        {
            var compiler = new RoslynCompiler();

            var instance = compiler.Instance();
            instance.match<Parameter>()
                .output("value");
            instance.match<Return>()
                .input("value", SetupReturn);
            instance.match<Operator>()
                .input("left",  SetupOperatorInput(true))
                .input("right", SetupOperatorInput(false));
            instance.then(GenerateCode);

            return compiler;
        }

        private void SetupReturn(InstanceConnector connector, object source, object target, Scope scope)
        {
            var @return = (Return)target;
            @return.Value = source is Parameter
                ? (source as Parameter).Name
                : (source as Operator).Result;
        }

        private Action<InstanceConnector, object, object, Scope> SetupOperatorInput(bool left)
        {
            return (connector, source, target, scope) =>
            {
                var @operator = (Operator)target;
                var value = source is Parameter
                    ? (source as Parameter).Name
                    : (source as Operator).Result;

                if (left)
                    @operator.LeftOperand = value;
                else
                    @operator.RightOperand = value;
            };
        }

        //rendering
        public static ClassDeclarationSyntax FormulaClass = Template.Parse(@"
            class Formula
            {
                public double Evaluate()
                {
                }
            }").Get<ClassDeclarationSyntax>();

        public static TypeSyntax DoubleType = CSharp.ParseTypeName("double");

        private static SyntaxNode GenerateCode(IDictionary<string, Tuple<object, SyntaxNode>> instances, Scope scope)
        {
            var graph = scope.get<IExcessGraph>();
            var parameters = graph.Root
                .Where(node => node.Owner is Parameter)
                .Select(node => node.Owner as Parameter);

            var method = FormulaClass
                .DescendantNodes()
                .OfType<MethodDeclarationSyntax>()
                .Single();

            return FormulaClass.ReplaceNode(method, method
                .WithParameterList(CSharp.ParameterList(CSharp.SeparatedList(
                    parameters.Select(parameter => CSharp.Parameter(CSharp.ParseToken(parameter.Name))
                        .WithType(DoubleType)))))
                .WithBody(CSharp.Block(
                    graph.RenderNodes(graph.Root))));
        }
    }
}
