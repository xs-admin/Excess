using Excess.Compiler;
using Excess.Compiler.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess.Extensions.Fluke
{
    using CSharp = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
    using Roslyn = Excess.Compiler.Roslyn.RoslynCompiler;

    public class Extension
    {
        public static void Apply(ICompiler<SyntaxToken, SyntaxNode, SemanticModel> compiler)
        {
            var lexical = compiler.Lexical();

            //aca se hacen los cambios antes de llegar a Roslyn, o sea, aca no hay nada compilado
            lexical
                .match() //le dices que vas a buscar un patron
                    .token("repository", named: "keyword")  //algo que diga repository
                    .identifier()                           //seguido de un identificador
                    .token('{')                             //seguido de un {
                    .then(compiler.Lexical().transform()    //cuando lo encuentre
                        .replace("keyword", "class ")        //cambia repository por class    
                        .then(Repository));                 //y cuando se compile te llama a esta funcion con la clase
        }

        static Template repoInterface = Template.Parse(@"
            public interface _0
            {
            }");

        static Template repoClass = Template.Parse(@"
            public class _0 : EFGenericRepository<_1>, _2
            {
            }");
        
        private static SyntaxNode Repository(SyntaxNode node, Scope scope) 
        {
            //Hubo problemas con el templatecito bonito, 
            //Roslyn espera que tu reemplaces una clase con otra
            //Por tanto hay que escupir la clase nada mas y annadir 
            //la interfaz al scope

            var repository = node as ClassDeclarationSyntax; 

            if (repository == null)
                throw new InvalidOperationException();

            //Vamos a suponer que quieres cambiar los metodos para que sean UnitOfWork
            //solo un ejemplo.
            var typeName = repository.Identifier.ToString();
            var className = typeName + "Repository";
            var interfaceName = "I" + typeName + "Repository";

            var methods = repository
                .DescendantNodes()
                .OfType<MethodDeclarationSyntax>();

            var @interface = repoInterface.Get<InterfaceDeclarationSyntax>(interfaceName)
                .AddMembers(methods
                    .Select(method => CSharp.MethodDeclaration(method.ReturnType, method.Identifier)
                        .WithParameterList(method.ParameterList)
                        .WithSemicolonToken(Roslyn.semicolon))
                    .ToArray());

            scope.AddType(@interface);

            return repoClass.Get<ClassDeclarationSyntax>(className, typeName, interfaceName)
                .AddMembers(methods.ToArray());

        }
    }
}