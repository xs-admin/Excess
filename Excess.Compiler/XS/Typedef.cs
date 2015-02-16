using Excess.Compiler.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess.Compiler.XS
{
    using CSharp = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
    using ExcessCompiler = ICompiler<SyntaxToken, SyntaxNode, SemanticModel>;

    class TypeDef
    {
        static public void Apply(ExcessCompiler compiler)
        {
            var lexical   = compiler.Lexical();
            var semantics = compiler.Semantics();

            lexical
                .match()
                    .token("typedef")
                    .identifier(named: "id")
                    .token("=")
                    .until(';', named: "definition")
                    .then(TypedefAssignment)
                .match() 
                    .token("typedef", named: "keyword")
                    .until(';', named: "definition")
                    .then(compiler.Lexical().transform()
                        .remove("keyword")
                        .then(Typedef));

            semantics
                .error("CS0246", FixMissingType);
        }

        private static IEnumerable<SyntaxToken> TypedefAssignment(IEnumerable<SyntaxToken> tokens, Scope scope)
        {
            dynamic context = scope;

            SyntaxToken identifier = context.id;

            bool found = false;
            foreach (var token in tokens)
            {
                if (found)
                {
                    if (token.CSharpKind() == SyntaxKind.SemicolonToken)
                        break;

                    yield return token;
                }
                else
                    found = token.CSharpKind() == SyntaxKind.EqualsToken;
            }

            var document = scope.GetDocument<SyntaxToken, SyntaxNode, SemanticModel>();

            yield return identifier.WithLeadingTrivia(CSharp.ParseLeadingTrivia(" "));
            yield return document.change(CSharp.Token(SyntaxKind.SemicolonToken), Typedef);
        }

        private static SyntaxNode Typedef(SyntaxNode node, Scope scope)
        {
            var field = node
                .AncestorsAndSelf()
                .OfType<FieldDeclarationSyntax>()
                .FirstOrDefault();

            if (field == null)
            {
                scope.AddError("xs01", "malformed typedef", node);
                //td: error, malformed typedef
                return node;
            }


            if (field.Declaration.Variables.Count != 1)
            {
                scope.AddError("xs01", "malformed typedef", node);
                return node;
            }

            var variable = field
                .Declaration
                .Variables[0];

            Debug.Assert(variable.Initializer == null || variable.Initializer.IsMissing);

            var type = RoslynCompiler.UnMark(field.Declaration.Type);
            var identifier = variable.Identifier;

            var parentScope = scope.CreateScope<SyntaxToken, SyntaxNode, SemanticModel>(field.Parent);
            Debug.Assert(parentScope != null);

            parentScope.set("__tdef" + identifier.ToString(), type);

            //schedule deletion
            var document = scope.GetDocument<SyntaxToken, SyntaxNode, SemanticModel>();
            document.change(field.Parent, RoslynCompiler.RemoveMember(field));

            //return intact
            return node;
        }

        private static void FixMissingType(SyntaxNode node, Scope scope)
        {
            var type = node
                .Ancestors()
                .OfType<TypeDeclarationSyntax>()
                .FirstOrDefault();

            if (type != null)
            {
                var typeScope = scope.GetScope<SyntaxToken, SyntaxNode, SemanticModel>(type);
                if (typeScope != null)
                {
                    SyntaxNode realType = typeScope.get<SyntaxNode>("__tdef" + node.ToString());
                    if (realType != null)
                    {
                        realType = RoslynCompiler.Mark(realType); //make sure not to duplicate nodes

                        var document = scope.GetDocument<SyntaxToken, SyntaxNode, SemanticModel>();
                        document.change(node, RoslynCompiler.ReplaceNode(realType));
                    }
                }
            }
        }
    }
}