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

        //member extensions
        private Func<MethodDeclarationSyntax, Scope, MemberDeclarationSyntax> MemberExtensions_Transform(
            bool expectsIdentifier,
            bool expectsParameters)
        {
            return (method, scope) =>
            {
                var hasIdentifier = method.Identifier.ToString().Equals("SomeIdentifier");
                if (expectsIdentifier)
                    Assert.IsTrue(hasIdentifier);

                if (expectsParameters)
                    Assert.AreNotEqual(0, method.ParameterList.Parameters.Count);

                if (!hasIdentifier)
                    method = method.WithIdentifier(CSharp
                        .ParseToken("SomeIdentifier"));

                return method
                    .WithReturnType(RoslynCompiler.@void);
            };
        }

        private void MemberExtensions_Assertions(SyntaxTree tree, int expectedArguments = 0)
        {
            //must have replaced the extension a method called SomeIdentifier
            var method = tree
                .GetRoot()
                .DescendantNodes()
                .OfType<MethodDeclarationSyntax>()
                .Single();

            Assert.IsNotNull(method);
            Assert.AreEqual("SomeIdentifier", method.Identifier.ToString());
            Assert.AreEqual("void", method.ReturnType.ToString());
            Assert.AreEqual(expectedArguments, method
                .ParameterList
                .Parameters
                .Count);

            //must have the SomeCall(); statement
            Assert.AreEqual("SomeCall();", method
                .DescendantNodes()
                .OfType<ExpressionStatementSyntax>()
                .Single()
                .ToString());
        }

        [TestMethod]
        public void MemberExtensions_Keyword()
        {
            var tree = ExcessMock.Compile(@"
                class SomeClass
                {
                    SomeExtension
                    {
                        SomeCall();                
                    }    
                }",
                (compiler) => compiler.extension(
                    "SomeExtension",
                    MemberExtensions_Transform(
                        expectsIdentifier: false,
                        expectsParameters: false)));

            Assert.IsNotNull(tree);
            MemberExtensions_Assertions(tree);
        }

        [TestMethod]
        public void MemberExtensions_KeywordIdentifiers()
        {
            var tree = ExcessMock.Compile(@"
                class SomeClass
                {
                    SomeExtension SomeIdentifier
                    {
                        SomeCall();                
                    }    
                }",
                (compiler) => compiler.extension(
                    "SomeExtension",
                    MemberExtensions_Transform(
                        expectsIdentifier: true,
                        expectsParameters: false)));

            Assert.IsNotNull(tree);
            MemberExtensions_Assertions(tree);
        }

        [TestMethod]
        public void MemberExtensions_KeywordParameters()
        {
            var tree = ExcessMock.Compile(@"
                class SomeClass
                {
                    SomeExtension(int SomeParam)
                    {
                        SomeCall();                
                    }    
                }",
                (compiler) => compiler.extension(
                    "SomeExtension",
                    MemberExtensions_Transform(
                        expectsIdentifier: false,
                        expectsParameters: true)));

            Assert.IsNotNull(tree);
            MemberExtensions_Assertions(tree, expectedArguments: 1);
        }

        [TestMethod]
        public void MemberExtensions_KeywordIdentifierParameters()
        {
            var tree = ExcessMock.Compile(@"
                class SomeClass
                {
                    SomeExtension SomeIdentifier(int SomeParam)
                    {
                        SomeCall();                
                    }    
                }",
                (compiler) => compiler.extension(
                    "SomeExtension",
                    MemberExtensions_Transform(
                        expectsIdentifier: true,
                        expectsParameters: true)));

            Assert.IsNotNull(tree);
            MemberExtensions_Assertions(tree, expectedArguments: 1);
        }

        //member type extensions
        private Func<ClassDeclarationSyntax, ParameterListSyntax, Scope, MemberDeclarationSyntax> MemberTypeExtensions_Transform(
            bool expectsIdentifier,
            bool expectsParameters)
        {
            return (@class, parameters, scope) =>
            {
                var hasIdentifier = @class.Identifier.ToString().Equals("SomeIdentifier");
                if (expectsIdentifier)
                    Assert.IsTrue(hasIdentifier);

                if (expectsParameters)
                    Assert.IsNotNull(parameters);

                if (!hasIdentifier)
                    @class = @class.WithIdentifier(CSharp
                        .ParseToken("SomeIdentifier"));

                if (parameters != null)
                    @class = @class.AddMembers(CSharp
                        .MethodDeclaration(RoslynCompiler.@void, "SomeParameters")
                            .AddParameterListParameters(parameters
                                .Parameters
                                .ToArray()));

                return @class;
            };
        }

        private void MemberTypeExtensions_Assertions(SyntaxTree tree, int expectedArguments = 0)
        {
            //must have replaced the extension a method called SomeIdentifier
            var @class = tree
                .GetRoot()
                .DescendantNodes()
                .OfType<ClassDeclarationSyntax>()
                .Where(c => c.Identifier.ToString() == "SomeIdentifier")
                .Single();

            if (expectedArguments > 0)
            {
                Assert.AreEqual(expectedArguments, @class
                    .DescendantNodes()
                    .OfType<MethodDeclarationSyntax>()
                    .Where(method => method.Identifier.ToString() == "SomeParameters")
                    .Single()
                        .ParameterList
                        .Parameters
                        .Count);
            }

            //must have the SomeMethod method
            Assert.AreEqual(1, @class
                .DescendantNodes()
                .OfType<MethodDeclarationSyntax>()
                .Where(method => method.Identifier.ToString() == "SomeMethod")
                .Count());
        }

        [TestMethod]
        public void MemberTypeExtensions_Keyword()
        {
            var tree = ExcessMock.Compile(@"
                class SomeClass
                {
                    SomeExtension
                    {
                        void SomeMethod()
                        {
                        }
                    }    
                }",
                (compiler) => compiler.extension(
                    "SomeExtension",
                    MemberTypeExtensions_Transform(
                        expectsIdentifier: false,
                        expectsParameters: false)));

            Assert.IsNotNull(tree);
            MemberTypeExtensions_Assertions(tree);
        }

        [TestMethod]
        public void MemberTypeExtensions_KeywordIdentifier()
        {
            var tree = ExcessMock.Compile(@"
                class SomeClass
                {
                    SomeExtension SomeIdentifier
                    {
                        void SomeMethod()
                        {
                        }
                    }    
                }",
                (compiler) => compiler.extension(
                    "SomeExtension",
                    MemberTypeExtensions_Transform(
                        expectsIdentifier: true,
                        expectsParameters: false)));

            Assert.IsNotNull(tree);
            MemberTypeExtensions_Assertions(tree);
        }

        [TestMethod]
        public void MemberTypeExtensions_KeywordParameters()
        {
            var tree = ExcessMock.Compile(@"
                class SomeClass
                {
                    SomeExtension(int SomeParameter)
                    {
                        void SomeMethod()
                        {
                        }
                    }    
                }",
                (compiler) => compiler.extension(
                    "SomeExtension",
                    MemberTypeExtensions_Transform(
                        expectsIdentifier: false,
                        expectsParameters: true)));

            Assert.IsNotNull(tree);
            MemberTypeExtensions_Assertions(tree, expectedArguments: 1);
        }

        [TestMethod]
        public void MemberTypeExtensions_KeywordIdentifierParameters()
        {
            var tree = ExcessMock.Compile(@"
                class SomeClass
                {
                    SomeExtension SomeIdentifier(int SomeParameter)
                    {
                        void SomeMethod()
                        {
                        }
                    }    
                }",
                (compiler) => compiler.extension(
                    "SomeExtension",
                    MemberTypeExtensions_Transform(
                        expectsIdentifier: false,
                        expectsParameters: true)));

            Assert.IsNotNull(tree);
            MemberTypeExtensions_Assertions(tree, expectedArguments: 1);
        }

        //type extensions
        private Func<ClassDeclarationSyntax, ParameterListSyntax, Scope, TypeDeclarationSyntax> TypeExtensions_Transform(
            bool expectsIdentifier,
            bool expectsParameters)
        {
            return (@class, parameters, scope) =>
            {
                var hasIdentifier = @class.Identifier.ToString().Equals("SomeIdentifier");
                if (expectsIdentifier)
                    Assert.IsTrue(hasIdentifier);

                if (expectsParameters)
                    Assert.IsNotNull(parameters);

                if (!hasIdentifier)
                    @class = @class.WithIdentifier(CSharp
                        .ParseToken("SomeIdentifier"));

                if (parameters != null)
                    @class = @class.AddMembers(CSharp
                        .MethodDeclaration(RoslynCompiler.@void, "SomeParameters")
                            .AddParameterListParameters(parameters
                                .Parameters
                                .ToArray()));

                return @class;
            };
        }

        private void TypeExtensions_Assertions(SyntaxTree tree, int expectedArguments = 0)
        {
            //must have replaced the extension a method called SomeIdentifier
            var @class = tree
                .GetRoot()
                .DescendantNodes()
                .OfType<ClassDeclarationSyntax>()
                .Where(c => c.Identifier.ToString() == "SomeIdentifier")
                .Single();

            if (expectedArguments > 0)
            {
                Assert.AreEqual(expectedArguments, @class
                    .DescendantNodes()
                    .OfType<MethodDeclarationSyntax>()
                    .Where(method => method.Identifier.ToString() == "SomeParameters")
                    .Single()
                        .ParameterList
                        .Parameters
                        .Count);
            }

            //must have the SomeMethod method
            Assert.AreEqual(1, @class
                .DescendantNodes()
                .OfType<MethodDeclarationSyntax>()
                .Where(method => method.Identifier.ToString() == "SomeMethod")
                .Count());
        }

        [TestMethod]
        public void TypeExtensions_Keyword()
        {
            var tree = ExcessMock.Compile(@"
                namespace SomeNamespace
                {
                    SomeExtension
                    {
                        void SomeMethod()
                        {
                        }
                    }
                }",
                (compiler) => compiler.extension(
                    "SomeExtension",
                    TypeExtensions_Transform(
                        expectsIdentifier: false,
                        expectsParameters: false)));

            Assert.IsNotNull(tree);
            TypeExtensions_Assertions(tree);
        }

        [TestMethod]
        public void TypeExtensions_KeywordIdentifier()
        {
            var tree = ExcessMock.Compile(@"
                SomeExtension SomeIdentifier
                {
                    void SomeMethod()
                    {
                    }
                }",
                (compiler) => compiler.extension(
                    "SomeExtension",
                    TypeExtensions_Transform(
                        expectsIdentifier: true,
                        expectsParameters: false)));

            Assert.IsNotNull(tree);
            TypeExtensions_Assertions(tree);
        }

        [TestMethod]
        public void TypeExtensions_KeywordParameters()
        {
            var tree = ExcessMock.Compile(@"
                SomeExtension(int SomeParameter)
                {
                    void SomeMethod()
                    {
                    }
                }",
                (compiler) => compiler.extension(
                    "SomeExtension",
                    TypeExtensions_Transform(
                        expectsIdentifier: false,
                        expectsParameters: true)));

            Assert.IsNotNull(tree);
            TypeExtensions_Assertions(tree, expectedArguments: 1);
        }

        [TestMethod]
        public void TypeExtensions_KeywordIdentifierParameters()
        {
            var tree = ExcessMock.Compile(@"
                SomeExtension SomeIdentifier(int SomeParameter)
                {
                    void SomeMethod()
                    {
                    }
                }",
                (compiler) => compiler.extension(
                    "SomeExtension",
                    TypeExtensions_Transform(
                        expectsIdentifier: true,
                        expectsParameters: true)));

            Assert.IsNotNull(tree);
            TypeExtensions_Assertions(tree, expectedArguments: 1);
        }

        //type code extensions
        private Func<ClassDeclarationSyntax, ParameterListSyntax, BlockSyntax, Scope, TypeDeclarationSyntax> TypeCodeExtensions_Transform(
            bool expectsIdentifier,
            bool expectsParameters)
        {
            return (@class, parameters, code, scope) =>
            {
                var hasIdentifier = @class.Identifier.ToString().Equals("SomeIdentifier");
                if (expectsIdentifier)
                    Assert.IsTrue(hasIdentifier);

                if (expectsParameters)
                    Assert.IsNotNull(parameters);

                if (!hasIdentifier)
                    @class = @class.WithIdentifier(CSharp
                        .ParseToken("SomeIdentifier"));

                if (parameters != null)
                    @class = @class.AddMembers(CSharp
                        .MethodDeclaration(RoslynCompiler.@void, "SomeParameters")
                            .AddParameterListParameters(parameters
                                .Parameters
                                .ToArray()));

                return @class
                    .AddMembers(CSharp
                        .MethodDeclaration(RoslynCompiler.@void,"SomeMethod")
                            .WithBody(code));
            };
        }

        private void TypeCodeExtensions_Assertions(SyntaxTree tree, int expectedArguments = 0)
        {
            //must have replaced the extension a method called SomeIdentifier
            var @class = tree
                .GetRoot()
                .DescendantNodes()
                .OfType<ClassDeclarationSyntax>()
                .Where(c => c.Identifier.ToString() == "SomeIdentifier")
                .Single();

            if (expectedArguments > 0)
            {
                Assert.AreEqual(expectedArguments, @class
                    .DescendantNodes()
                    .OfType<MethodDeclarationSyntax>()
                    .Where(m => m.Identifier.ToString() == "SomeParameters")
                    .Single()
                        .ParameterList
                        .Parameters
                        .Count);
            }

            //must have the SomeMethod method, containing SomeCall()
            var method = @class
                .DescendantNodes()
                .OfType<MethodDeclarationSyntax>()
                .Where(m => m.Identifier.ToString() == "SomeMethod")
                .Single();

            Assert.AreEqual(1, method
                .DescendantNodes()
                .OfType<InvocationExpressionSyntax>()
                .Count(expr => expr.ToString() == "SomeCall()"));
        }

        [TestMethod]
        public void TypeCodeExtensions_Keyword()
        {
            var tree = ExcessMock.Compile(@"
                SomeExtension
                {
                    SomeCall();
                }",
                (compiler) => compiler.extension(
                    "SomeExtension",
                    TypeCodeExtensions_Transform(
                        expectsIdentifier: false,
                        expectsParameters: false)));

            Assert.IsNotNull(tree);
            TypeCodeExtensions_Assertions(tree);
        }

        [TestMethod]
        public void TypeCodeExtensions_KeywordIdentifier()
        {
            var tree = ExcessMock.Compile(@"
                SomeExtension SomeIdentifier
                {
                    SomeCall();
                }",
                (compiler) => compiler.extension(
                    "SomeExtension",
                    TypeCodeExtensions_Transform(
                        expectsIdentifier: true,
                        expectsParameters: false)));

            Assert.IsNotNull(tree);
            TypeCodeExtensions_Assertions(tree);
        }

        [TestMethod]
        public void TypeCodeExtensions_KeywordParameters()
        {
            var tree = ExcessMock.Compile(@"
                SomeExtension(int SomeParam)
                {
                    SomeCall();
                }",
                (compiler) => compiler.extension(
                    "SomeExtension",
                    TypeCodeExtensions_Transform(
                        expectsIdentifier: false,
                        expectsParameters: true)));

            Assert.IsNotNull(tree);
            TypeCodeExtensions_Assertions(tree, expectedArguments: 1);
        }

        [TestMethod]
        public void TypeCodeExtensions_KeywordIdentifierParameters()
        {
            var tree = ExcessMock.Compile(@"
                SomeExtension SomeIdentifier(int SomeParam)
                {
                    SomeCall();
                }",
                (compiler) => compiler.extension(
                    "SomeExtension",
                    TypeCodeExtensions_Transform(
                        expectsIdentifier: true,
                        expectsParameters: true)));

            Assert.IsNotNull(tree);
            TypeCodeExtensions_Assertions(tree, expectedArguments: 1);
        }
    }
}