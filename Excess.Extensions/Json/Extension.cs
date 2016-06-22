using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;
using Newtonsoft.Json.Linq;
using Antlr4.Runtime;
using Excess.Compiler;
using Excess.Compiler.Roslyn;
using Excess.Compiler.Attributes;
using Json.Grammar;

namespace Json
{
    using Excess.Compiler.Antlr4;
    using Microsoft.CodeAnalysis.CSharp;
    using CSharp = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
    using ExcessCompiler = ICompiler<SyntaxToken, SyntaxNode, SemanticModel>;
    using Roslyn = RoslynCompiler;

    [Extension("json")]
    public class JsonExtension
    {
        //grammar interop
        class JsonGrammar : AntlrGrammar
        {
            protected override ITokenSource GetLexer(AntlrInputStream stream) => new JSONLexer(stream);
            protected override Parser GetParser(ITokenStream tokenStream) => new JSONParser(tokenStream);
            protected override ParserRuleContext GetRoot(Parser parser) => ((JSONParser)parser).json();
        }

        public static void Apply(ExcessCompiler compiler)
        {
            compiler.Environment()
                .dependency<JObject>("Newtonsoft.Json.Linq");

            compiler.Lexical()
                .grammar<JsonGrammar, ParserRuleContext>("json", ExtensionKind.Code)
                    .transform<JSONParser.ExpressionContext>(AntlrExpression.Parse)
                    .transform<JSONParser.JsonContext>(Main)
                    .transform<JSONParser.ObjectContext>(JObject)
                    .transform<JSONParser.ArrayContext>(JArray)
                    .transform<JSONParser.PairContext>(JPair)
                    .transform<JSONParser.ValueContext>(JValue)

                    .then(Transform);
            ;
        }

        static Template createJson = Template.ParseExpression("JObject.FromObject(__0)");
        private static SyntaxNode Transform(SyntaxNode oldNode, SyntaxNode newNode, Scope scope, LexicalExtension<SyntaxToken> extension)
        {
            Debug.Assert(newNode is AnonymousObjectCreationExpressionSyntax);
            var result = createJson.Get(newNode);

            var isAssignment = false;
            result = Roslyn.ReplaceAssignment(oldNode, result, out isAssignment);
            if (!isAssignment)
            {
                scope.AddError("json01", "json expects to be assigned", oldNode);
                return newNode;
            }

            return result;
        }

        private static SyntaxNode JValue(JSONParser.ValueContext value, Func<ParserRuleContext, Scope, SyntaxNode> continuation, Scope scope)
        {
            if (value.ChildCount == 0)
                return null; //empty value

            return continuation(value.GetRuleContext<ParserRuleContext>(0), scope);
        }

        private static SyntaxNode JPair(JSONParser.PairContext pair, Func<ParserRuleContext, Scope, SyntaxNode> continuation, Scope scope)
        {
            string identifier = null;
            var id = pair.Identifier();
            if (id != null)
                identifier = id.GetText();
            else
            {
                identifier = pair
                    .StringLiteral()
                    .GetText();

                identifier = identifier.Substring(1, identifier.Length - 2); //traditional jason
            }

            var expr = (ExpressionSyntax)continuation(pair.value(), scope);
            return CSharp
                .AnonymousObjectMemberDeclarator(CSharp
                    .NameEquals(CSharp
                        .IdentifierName(identifier)),
                    expr);
        }

        private static SyntaxNode JArray(JSONParser.ArrayContext array, Func<ParserRuleContext, Scope, SyntaxNode> continuation, Scope scope)
        {
            var values = new List<ExpressionSyntax>();
            foreach (var value in array.value())
            {
                Debug.Assert(value is JSONParser.ValueContext);

                var expr = (ExpressionSyntax)JValue(value, continuation, scope);
                if (expr != null) //empty ok
                    values.Add(expr);
            }

            return CSharp
                .ImplicitArrayCreationExpression(CSharp
                .InitializerExpression(SyntaxKind.ArrayInitializerExpression, CSharp.SeparatedList(
                    values)));
        }

        private static SyntaxNode JObject(JSONParser.ObjectContext @object, Func<ParserRuleContext, Scope, SyntaxNode> continuation, Scope scope)
        {
            return ParsePairs(@object.pair(), continuation, scope);
        }

        private static SyntaxNode ParsePairs(IEnumerable<JSONParser.PairContext> pairs, Func<ParserRuleContext, Scope, SyntaxNode> continuation, Scope scope)
        {
            var members = new List<AnonymousObjectMemberDeclaratorSyntax>();
            foreach (var pair in pairs)
            {
                members.Add((AnonymousObjectMemberDeclaratorSyntax)continuation(pair, scope));
            }

            return CSharp
                .AnonymousObjectCreationExpression(CSharp.SeparatedList(
                    members));
        }

        private static SyntaxNode Main(JSONParser.JsonContext json, Func<ParserRuleContext, Scope, SyntaxNode> continuation, Scope scope)
        {
            return ParsePairs(json.pair(), continuation, scope);
        }
    }
}
