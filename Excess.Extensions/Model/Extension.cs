using System.Linq;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Excess.Compiler;
using Excess.Compiler.Roslyn;
using Excess.Compiler.Attributes;

namespace Excess.Extensions.Model
{
    using ExcessCompiler = ICompiler<SyntaxToken, SyntaxNode, SemanticModel>;
    using CSharp = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
    using Microsoft.CodeAnalysis.CSharp;
    [Extension("model")]
    public class ModelExtension
    {
        public static void Apply(ExcessCompiler compiler, Scope scope = null)
        {
            scope?.AddKeywords("model");
            compiler.extension("model", ParseModel);
        }

        private static Template ModelProperty = Template.Parse("public __0 T { get; private set; }");

        private static TypeDeclarationSyntax ParseModel(ClassDeclarationSyntax @class, ParameterListSyntax parameters, Scope scope)
        {
            var init = new List<ParameterSyntax>();
            var props = new List<PropertyDeclarationSyntax>();
            foreach (var member in @class.Members)
            {
                var field = ParseField(member);
                if (field == null)
                    continue; //error has already been registered

                var type = field.Declaration.Type;
                var variable = field.Declaration
                    .Variables
                    .Single();


                init.Add(CSharp.Parameter(variable.Identifier)
                    .WithType(type)
                    .WithDefault(CSharp.EqualsValueClause(
                        variable.Initializer != null
                            ? variable.Initializer.Value
                            : CSharp.DefaultExpression(type))));

                props.Add(ModelProperty.Get<PropertyDeclarationSyntax>(type)
                    .WithIdentifier(variable.Identifier));
            }

            if (!RoslynCompiler.HasVisibilityModifier(@class.Modifiers))
                @class = @class.AddModifiers(CSharp.Token(SyntaxKind.PublicKeyword));

            return @class
                .WithMembers(CSharp.List<MemberDeclarationSyntax>(
                    props.Union(new[] {
                    GenerateConstructor(@class, init)})));
        }

        private static Template ModelAssign = Template.ParseStatement(
            "this._0 = _0;");

        private static MemberDeclarationSyntax GenerateConstructor(ClassDeclarationSyntax @class, IEnumerable<ParameterSyntax> parameters)
        {
            return CSharp.ConstructorDeclaration(@class.Identifier)
                .WithModifiers(RoslynCompiler.@public)
                .AddParameterListParameters(parameters.ToArray())
                .AddBodyStatements(parameters
                    .Select(param => 
                        ModelAssign.Get<StatementSyntax>(param.Identifier))
                    .ToArray());
        }

        private static FieldDeclarationSyntax ParseField(MemberDeclarationSyntax member)
        {
            var result = member as FieldDeclarationSyntax;
            if (result == null)
            {
                //td: error
                return null;
            }

            if (result.Modifiers.Any())
            {
                //td: error, no modifiers
                return null;
            }

            return result;
        }
    }
}
