using Excess.Compiler.Mock;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SQL.Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tests.Mock;
using xslang;

namespace Tests
{
    [TestClass]
    public class DapperTests
    {
        [TestMethod]
        public void DapperQuery_Usage()
        {
            var tree = ExcessMock.Compile(@"
                function SomeFunction(int SomeInt, int SomeId)
                {
                    IEnumerable<SomeModel> result = sql
                    {
                        select * from SomeTable
                        where SomeColumn > @SomeInt
                    }

                    SomeModel anotherResult = sql
                    {
                        select * from SomeTable
                        where IdColumn > @SomeId
                    }
                }",
                builder: compiler =>
                {
                    Functions.Apply(compiler);
                    DapperExtension.Apply(compiler);
                });

            //must have a call to __connection1.Query
            var invocation = (tree.GetRoot()
                .DescendantNodes()
                .OfType<MethodDeclarationSyntax>()
                .Single()
                .Body
                    .Statements
                    .Skip(2) //connection declarations
                    .First() as LocalDeclarationStatementSyntax)
                        .Declaration
                        .Variables
                        .Single()
                        .Initializer
                            .Value as InvocationExpressionSyntax;

            Assert.IsNotNull(invocation);
            Assert.IsTrue(invocation is InvocationExpressionSyntax);
            Assert.IsTrue((invocation as InvocationExpressionSyntax)
                .Expression
                .ToString()
                .StartsWith("__connection1.Query"));

            //must have added a literal string @"...query..."
            var literal = tree.GetRoot()
                .DescendantNodes()
                .OfType<LiteralExpressionSyntax>()
                .First()
                .ToString();

            Assert.IsTrue(literal.Contains("@\""));
            Assert.IsTrue(literal.Contains("where SomeColumn > @SomeInt"));

            //must have added parameters as an anonymous object
            var parameters = tree.GetRoot()
                .DescendantNodes()
                .OfType<AnonymousObjectCreationExpressionSyntax>()
                .First();

            Assert.IsTrue(parameters
                .Initializers
                .Any(init => init
                    .NameEquals
                    .Name
                    .ToString() == "SomeInt"));

            //must have added a second call
            invocation = (tree.GetRoot()
                .DescendantNodes()
                .OfType<MethodDeclarationSyntax>()
                .Single()
                .Body
                    .Statements
                    .Last() as LocalDeclarationStatementSyntax)
                        .Declaration
                        .Variables
                        .Single()
                        .Initializer
                            .Value as InvocationExpressionSyntax;

            //containing a Single() call
            Assert.AreEqual(1, invocation
                    .DescendantTokens()
                    .Count(token => token.ToString() == "Single"));
        }

        [TestMethod]
        public void DapperCommand_Usage()
        {
            var tree = ExcessMock.Compile(@"
                class SomeClass
                {
                    void SomeMethod(int SomeInt)
                    {
                        sql
                        {
                            insert into SomeTable
                            values(@SomeInt)
                        }
                    }
                }", builder: (compiler) => DapperExtension.Apply(compiler));

            //must have a call to __connection.Execute
            var invocation = (tree.GetRoot()
                .DescendantNodes()
                .OfType<MethodDeclarationSyntax>()
                .Single()
                .Body
                    .Statements
                    .Single() as ExpressionStatementSyntax)
                        .Expression as InvocationExpressionSyntax;

            Assert.IsNotNull(invocation);
            Assert.IsTrue(invocation is InvocationExpressionSyntax);
            Assert.IsTrue((invocation as InvocationExpressionSyntax)
                .Expression
                .ToString()
                .StartsWith("__connection1.Execute"));

            //must have added a literal string @"...query..."
            var literal = tree.GetRoot()
                .DescendantNodes()
                .OfType<LiteralExpressionSyntax>()
                .Single()
                .ToString();

            Assert.IsTrue(literal.Contains("@\""));
            Assert.IsTrue(literal.Contains("values(@SomeInt)"));

            //must have added parameters as an anonymous object
            var parameters = tree.GetRoot()
                .DescendantNodes()
                .OfType<AnonymousObjectCreationExpressionSyntax>()
                .Single();

            Assert.IsTrue(parameters
                .Initializers
                .Any(init => init
                    .NameEquals
                    .Name
                    .ToString() == "SomeInt"));
        }

        [TestMethod]
        public void DapperRuntime_Usage()
        {
            var dapper = DapperMock.Compile(@"
                function Values()
                {
                    IEnumerable<int> result = sql()
                    {
                        select * from SomeTable
                    }

                    return result;
                }", 
                database: @"
                    CREATE TABLE SomeTable
                    (
                        SomeValue int
                    );

                    INSERT INTO SomeTable (SomeValue)
                    VALUES (1), (2), (3), (4), (5), (6)");

            var allValues = dapper.GetMany<int>("Values");
            Assert.AreEqual(6, allValues.Count());
        }

        [TestMethod]
        public void DapperParameters_Usage()
        {
            var tree = ExcessMock.Compile(@"
                class SomeClass
                {
                    public void SomeMethod(IDBConnection connection)
                    {
                        sql(connection)
                        {
			                CREATE TABLE SomeTable
			                (
				                Id varchar(36) not null
                            );
                        }
                    }
                }", builder: (compiler) => DapperExtension.Apply(compiler));

            Assert.IsNotNull(tree);
            Assert.IsFalse(tree.GetDiagnostics().Any());

            //must have added a typecast for parameter connection
            Assert.AreEqual(1, tree
                .GetRoot()
                .DescendantNodes()
                .OfType<CastExpressionSyntax>()
                .Count());
        }
    }
}