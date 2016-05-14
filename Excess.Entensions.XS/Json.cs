using Antlr4.Runtime;
using Excess.Compiler;
using Excess.Compiler.Roslyn;
using Excess.Extensions.XS.Grammars;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Excess.Entensions.XS
{
    using CSharp = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
    using ExcessCompiler = ICompiler<SyntaxToken, SyntaxNode, SemanticModel>;
    using Roslyn = RoslynCompiler;

    internal class JsonGrammar : IGrammar<SyntaxToken, SyntaxNode, ParserRuleContext>
    {
        public ParserRuleContext parse(IEnumerable<SyntaxToken> tokens, Scope scope, int offset)
        {
            var text = RoslynCompiler.TokensToString(tokens);
            AntlrInputStream stream = new AntlrInputStream(text);
            ITokenSource lexer = new JSONLexer(stream);
            ITokenStream tokenStream = new CommonTokenStream(lexer);
            JSONParser parser = new JSONParser(tokenStream);

            parser.AddErrorListener(new AntlrErrors<IToken>(scope, offset));
            var result = parser.json();
            if (parser.NumberOfSyntaxErrors > 0)
                return null;

            return result;
        }
    }

    public class Json
    {
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
