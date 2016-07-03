using Excess.Compiler;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Excess.Compiler.Roslyn;

namespace Tests.Mocks
{
    using CSharp = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
    using Compiler = ICompiler<SyntaxToken, SyntaxNode, SemanticModel>;

    static class MockIndentGrammar
    {
        public static void Apply(Compiler compiler)
        {
            compiler.Lexical()
                .indented<MockIdentGrammarModel, RootModel>("someExtension", ExtensionKind.Code)
                    .match<RootModel, HeaderModel>(MatchHeader, 
                        children: child => child
                            .match<HeaderModel, HeaderValueModel>(MatchValue)
                            .match<MockIdentGrammarModel, HeaderModel, RazorModel>("Call {Name} at {Telephone}"))
                .then()
                    .transform<RootModel>(TransformHeaderModel);
        }

        private static HeaderModel MatchHeader(string text, RootModel parent, Scope scope)
        {
            var header = new HeaderModel { Name = text };
            parent.Headers.Add(header);
            return header;
        }

        private static HeaderValueModel MatchValue(string text, HeaderModel parent, Scope scope)
        {
            var assignment = CSharp.ParseExpression(text) as AssignmentExpressionSyntax;
            if (assignment == null)
                return null; //td: error

            parent.Values.Add(assignment);
            return new HeaderValueModel();
        }

        private static Template HeaderValue = Template.ParseStatement("SetHeaderValue(__0, __1, )__2);");
        private static SyntaxNode TransformHeaderModel(RootModel root, Func<MockIdentGrammarModel, Scope, SyntaxNode> parse, Scope scope)
        {
            var statements = new List<StatementSyntax>();
            foreach (var header in root.Headers)
            {
                foreach (var value in header.Values)
                {
                    statements.Add(HeaderValue.Get<StatementSyntax>(
                        RoslynCompiler.Quoted(header.Name),
                        RoslynCompiler.Quoted(value.Left.ToString()),
                        value.Right));
                }
            }

            return CSharp.Block(statements.ToArray());
        }
    }
}
