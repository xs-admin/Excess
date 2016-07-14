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
    using System.Text.RegularExpressions;
    using Microsoft.CodeAnalysis.CSharp;
    static class MockIndentGrammar
    {
        public static void Apply(Compiler compiler)
        {
            compiler.Lexical()
                .indented<MockIdentGrammarModel, RootModel>("someExtension", ExtensionKind.Code, null)
                    .match<RootModel, HeaderModel>(MatchHeader,
                        children: child => child
                            .match<HeaderModel, HeaderValueModel>(MatchValue)
                            .match<MockIdentGrammarModel, HeaderModel, ContactModel>("Call {Name} at {Telephone}",
                                then: (header, contact) => header.Contacts.Add(contact),
                                children: contactChild => contactChild
                                    .match<MockIdentGrammarModel, ContactModel, TelephoneModel>(MatchTelephone,
                                        then: (contact, phone) => contact.OtherNumbers.Add(phone))))
                    .match<MockIdentGrammarModel, RootModel, ForModel>("for {Iterator} in {Iterable}",
                        then: (root, forloop) => root.Statements.Add(forloop),
                        children: forChild => forChild
                            .match<MockIdentGrammarModel, ForModel, StatementSyntax>(
                                (@for, statement) => @for.Statements.Add(statement)))
                    .then()
                        .transform<RootModel>(TransformHeaderModel);
        }

        private static Regex MatchTelephone = new Regex(@"\(?(?<AreaCode>\d{3})\)?-? *(?<FirstThree>\d{3})-? *-?(?<LastFour>\d{4})");

        private static HeaderModel MatchHeader(string text, RootModel parent, Scope scope)
        {
            if (text.StartsWith("[") && text.EndsWith("]"))
            {
                var header = new HeaderModel { Name = text };
                parent.Headers.Add(header);
                return header;
            }

            return null;
        }

        private static HeaderValueModel MatchValue(string text, HeaderModel parent, Scope scope)
        {
            var assignment = CSharp.ParseExpression(text) as AssignmentExpressionSyntax;
            if (assignment == null)
                return null; //td: error

            parent.Values.Add(assignment);
            return new HeaderValueModel();
        }

        //templates
        private static Template HeaderValue = Template.ParseStatement("SetHeaderValue(__0, __1, __2);");
        private static Template Contact = Template.ParseStatement("SetContact(__0, __1, __2);");
        private static Template OtherNUmber = Template.ParseStatement("AddContactNumber(__0, __1, __2, __3, __4);");

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

                foreach (var contact in header.Contacts)
                {
                    statements.Add(Contact.Get<StatementSyntax>(
                        RoslynCompiler.Quoted(header.Name),
                        RoslynCompiler.Quoted(contact.Name),
                        RoslynCompiler.Quoted(contact.Telephone)));

                    foreach (var phone in contact.OtherNumbers)
                    {
                        statements.Add(OtherNUmber.Get<StatementSyntax>(
                            RoslynCompiler.Quoted(header.Name),
                            RoslynCompiler.Quoted(contact.Name),
                            CSharp.LiteralExpression(
                                SyntaxKind.NumericLiteralExpression,
                                CSharp.Literal(phone.AreaCode)),
                            CSharp.LiteralExpression(
                                SyntaxKind.NumericLiteralExpression,
                                CSharp.Literal(phone.FirstThree)),
                            CSharp.LiteralExpression(
                                SyntaxKind.NumericLiteralExpression,
                                CSharp.Literal(phone.LastFour))));
                    }
                }
            }

            foreach (var forStatement in root.Statements)
            {
                statements.Add(CSharp
                    .ForEachStatement(
                        CSharp.ParseTypeName("var"),
                        forStatement.Iterator,
                        CSharp.ParseExpression(forStatement.Iterable),
                        CSharp.Block(forStatement
                            .Statements
                            .ToArray())));
            }

            return CSharp.Block(statements.ToArray());
        }
    }
}
