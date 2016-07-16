using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Excess.Compiler;
using Excess.Compiler.Roslyn;
using Excess.Compiler.Mock;

namespace Tests
{
    using Microsoft.CodeAnalysis.CSharp;
    using CSharp = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

    [TestClass]
    public class CompilerExtensionsTest
    {
        private IEnumerable<SyntaxToken> FreeFormExtensions__Transform(IEnumerable<SyntaxToken> tokens, Scope scope, LexicalExtension<SyntaxToken> extension)
        {
            //must have matched starting on the first token
            Assert.AreEqual("SomeExtension", tokens.First().ToString());

            //must have matched the keyword correctly
            Assert.AreEqual("SomeExtension", extension.Keyword.ToString());

            yield return CSharp.ParseToken("ReplacedByTransform")
                .WithAdditionalAnnotations(extension
                    .Keyword
                    .GetAnnotations()); //keep original info, it is important

            //add arguments
            if (extension.Arguments != null && extension.Arguments.Any())
            {
                yield return extension.Arguments.First();

                if (!extension.Identifier.IsKind(SyntaxKind.None))
                {
                    //add the identifier as the first parameters
                    yield return extension.Identifier;
                    yield return CSharp.Token(SyntaxKind.CommaToken);
                }

                //add the rest of the arguments, skipping types
                foreach (var token in extension.Arguments)
                {
                    if (token.IsKind(SyntaxKind.IdentifierToken))
                        yield return token;
                }

                //close parameters
                yield return extension.Arguments.Last();
            }
            else
            {
                yield return CSharp.Token(SyntaxKind.OpenParenToken);
                    if (!extension.Identifier.IsMissing)
                    {
                        //add the identifier as the first parameters
                        yield return extension.Identifier;
                    }
                yield return CSharp.Token(SyntaxKind.CloseParenToken);
            }

            //add a semicolon, so it compiles
            yield return CSharp.Token(SyntaxKind.SemicolonToken);

            //keep the body as is
            foreach (var token in extension.Body)
                yield return token;
        }

        private static void FreeFormExtensions_Validation(SyntaxTree tree, bool validateArguments)
        {
            //must have kept the method
            var method = tree
                .GetRoot()
                .DescendantNodes()
                .OfType<MethodDeclarationSyntax>()
                .Single();

            //must have replaced "SomeExtension" by "ReplacedByTransform"
            Assert.AreEqual(0, method
                .DescendantTokens()
                .Where(token => token.ToString() == "SomeExtension")
                .Count());

            Assert.AreEqual(1, method
                .DescendantTokens()
                .Where(token => token.ToString() == "ReplacedByTransform")
                .Count());

            //must have left 2 statements, one expression statement and one block
            Assert.AreEqual(2, method.Body.Statements.Count);

            var exprStatement = (ExpressionStatementSyntax)method.Body.Statements.First();
            var blockStatement = (BlockSyntax)method.Body.Statements.Last();

            //must have added a semicolon (to make it compile without errors)
            Assert.IsFalse(exprStatement.SemicolonToken.IsMissing);

            //must have kept the call inside the body
            Assert.IsNotNull(method
                .DescendantNodes()
                .OfType<InvocationExpressionSyntax>()
                .Where(invocation => invocation.Expression.ToString() == "SomeCall")
                .Single());

            if (validateArguments)
            {
                //must have added an argument
                var arguments = tree
                    .GetRoot()
                    .DescendantNodes()
                    .OfType<ArgumentListSyntax>()
                    .Where(args => args.Arguments.Count > 0)
                    .Single();

                //with only one argument, for SomeIdentifier
                var argument = arguments.Arguments.Single();
                Assert.AreEqual("SomeIdentifier", argument.ToString());
            }
        }

        [TestMethod]
        public void FreeFormExtensions_Keyword()
        {
            var tree = ExcessMock.Compile(@"
                class SomeClass
                {
                    void SomeMethod()
                    {
                        SomeExtension
                        {
                            SomeCall();                
                        }    
                    }
                }",
                (compiler) => compiler.Lexical().extension(
                    "SomeExtension", ExtensionKind.Code,
                    FreeFormExtensions__Transform));

            Assert.IsNotNull(tree);
            FreeFormExtensions_Validation(tree, false);
        }

        [TestMethod]
        public void FreeFormExtensions_KeywordIdentifier()
        {
            var tree = ExcessMock.Compile(@"
                class SomeClass
                {
                    void SomeMethod()
                    {
                        SomeExtension SomeIdentifier
                        {
                            SomeCall();                
                        }    
                    }
                }",
                (compiler) => compiler.Lexical()
                    .extension(
                        "SomeExtension", ExtensionKind.None,
                        FreeFormExtensions__Transform));

            Assert.IsNotNull(tree);
            FreeFormExtensions_Validation(tree, true);
        }

        [TestMethod]
        public void FreeFormExtensions_KeywordParameters()
        {
            var tree = ExcessMock.Compile(@"
                class SomeClass
                {
                    void SomeMethod()
                    {
                        SomeExtension(int SomeIdentifier) 
                        {
                            SomeCall();                
                        }    
                    }
                }",
                (compiler) => compiler.Lexical().extension(
                    "SomeExtension", ExtensionKind.Code,
                    FreeFormExtensions__Transform));

            Assert.IsNotNull(tree);
            FreeFormExtensions_Validation(tree, true);
        }

        [TestMethod]
        public void FreeFormExtensions_KeywordIdentifierParameters()
        {
            var tree = ExcessMock.Compile(@"
                class SomeClass
                {
                    void SomeMethod()
                    {
                        SomeExtension SomeIdentifier(int SomeParam) 
                        {
                            SomeCall();                
                        }    
                    }
                }",
                (compiler) => compiler.Lexical().extension(
                    "SomeExtension", ExtensionKind.None,
                    FreeFormExtensions__Transform));

            Assert.IsNotNull(tree);
            FreeFormExtensions_Validation(tree, false);

            //must have added 2 arguments
            var arguments = tree
                .GetRoot()
                .DescendantNodes()
                .OfType<ArgumentListSyntax>()
                .Where(args => args.Arguments.Count == 2)
                .Single();

            //with 2 arguments, one for SomeIdentifier one for SomeParam
            var SomeIdentifier = arguments.Arguments.First();
            Assert.AreEqual("SomeIdentifier", SomeIdentifier.ToString());

            var SomeParam = arguments.Arguments.Last();
            Assert.AreEqual("SomeParam", SomeParam.ToString());
        }

        //code extensions
        private static Template CodeExtensions_Template = Template.ParseStatement("ReplacedByTransform(_0, () => {});");
        private Func<BlockSyntax, SyntaxToken, ParameterListSyntax, Scope, SyntaxNode> CodeExtensions_Transform(
            bool expectsIdentifier = false,
            bool expectsParameters = false)
        {
            return (block, identifier, parameters, scope) =>
            {
                if (expectsIdentifier)
                    Assert.IsFalse(identifier.IsKind(SyntaxKind.None));

                if (expectsParameters)
                    Assert.IsNotNull(parameters);

                if (identifier.IsKind(SyntaxKind.None))
                    identifier = CSharp.ParseToken("SomeIdentifier");

                var result = CodeExtensions_Template
                    .Get<StatementSyntax>(identifier);

                if (parameters != null)
                    result = result
                        .ReplaceNodes(result
                            .DescendantNodes()
                            .OfType<ArgumentListSyntax>(),
                            (on, nn) => nn
                                .AddArguments(parameters
                                    .Parameters
                                    .Select(parameter => CSharp.Argument(CSharp.IdentifierName(
                                        parameter.Identifier.IsMissing
                                            ? parameter.Type.GetFirstToken()
                                            : parameter.Identifier)))
                                    .ToArray()));
                var lambda = result
                    .DescendantNodes()
                    .OfType<ParenthesizedLambdaExpressionSyntax>()
                    .Single();

                return result.ReplaceNode(lambda, lambda
                    .WithBody(block));
            };
        }

        private void CodeExtensions_Assertions(SyntaxTree tree, int expectedArguments = 2)
        {
            //must have kept the method
            var method = tree
                .GetRoot()
                .DescendantNodes()
                .OfType<MethodDeclarationSyntax>()
                .Single();

            //must have replaced the extension by an invocation ReplacedByTransform()
            var invocation = (method
                .Body
                .Statements
                .Single() as ExpressionStatementSyntax)
                    .Expression as InvocationExpressionSyntax;

            Assert.IsNotNull(invocation);
            Assert.AreEqual("ReplacedByTransform", invocation.Expression.ToString());
            Assert.AreEqual(expectedArguments, invocation
                .ArgumentList
                .Arguments
                .Count);

            //must have added a lambda function with the SomeCall(); statement
            Assert.AreEqual("SomeCall();", (invocation
                .DescendantNodes()
                .OfType<ParenthesizedLambdaExpressionSyntax>()
                .Single()
                    .Body as BlockSyntax)
                        .Statements
                        .Single()
                        .ToString());
        }

        [TestMethod]
        public void CodeExtensions_Keyword()
        {
            var tree = ExcessMock.Compile(@"
                class SomeClass
                {
                    void SomeMethod()
                    {
                        SomeExtension
                        {
                            SomeCall();                
                        }    
                    }
                }",
                (compiler) => compiler.extension(
                    "SomeExtension",
                    CodeExtensions_Transform(
                        expectsIdentifier : false,
                        expectsParameters : false)));

            Assert.IsNotNull(tree);
            CodeExtensions_Assertions(tree);
        }

        [TestMethod]
        public void CodeExtensions_KeywordIdentifier()
        {
            var tree = ExcessMock.Compile(@"
                class SomeClass
                {
                    void SomeMethod()
                    {
                        SomeExtension SomeIndentifier
                        {
                            SomeCall();                
                        }    
                    }
                }",
                (compiler) => compiler.extension(
                    "SomeExtension",
                    CodeExtensions_Transform(
                        expectsIdentifier: true,
                        expectsParameters: false)));

            Assert.IsNotNull(tree);
            CodeExtensions_Assertions(tree);
        }

        [TestMethod]
        public void CodeExtensions_KeywordParameters()
        {
            var tree = ExcessMock.Compile(@"
                class SomeClass
                {
                    void SomeMethod()
                    {
                        SomeExtension(int SomeParam, SomeArgument)
                        {
                            SomeCall();                
                        }    
                    }
                }",
                (compiler) => compiler.extension(
                    "SomeExtension",
                    CodeExtensions_Transform(
                        expectsIdentifier: false,
                        expectsParameters: true)));

            Assert.IsNotNull(tree);
            CodeExtensions_Assertions(tree,
                expectedArguments : 4);
        }

        [TestMethod]
        public void CodeExtensions_KeywordIdentifierParameters()
        {
            var tree = ExcessMock.Compile(@"
                class SomeClass
                {
                    void SomeMethod()
                    {
                        SomeExtension SomeIdentifier(int SomeParam, SomeArgument)
                        {
                            SomeCall();                
                        }    
                    }
                }",
                (compiler) => compiler.extension(
                    "SomeExtension",
                    CodeExtensions_Transform(
                        expectsIdentifier: true,
                        expectsParameters: true)));

            Assert.IsNotNull(tree);
            CodeExtensions_Assertions(tree,
                expectedArguments: 4);
        }
    }
}