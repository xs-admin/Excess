using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess.Compiler
{
    public class Class1
    {
        static Class1()
        {
            int xx = 0;
            xx++;
        }

        //    public void init(ICompiler compiler)
        //    {
        //        compiler
        //            .Lexical()
        //                .match()
        //                    .any('(', '=', ',')
        //                    .token("function")
        //                    .enclosed('(', ')')
        //                    .mark("theEnd")
        //                    .token('{')
        //                    .then(compiler.Lexical()
        //                        .delete("function")
        //                        .insert("=>", after: "theEnd"))
        //                .match()
        //                    .any('(', '=', ',')
        //                    .mark("startArray")
        //                    .enclosed('[', ']', start: "openBracket", end: "closeBracket")
        //                    .then(compiler.Lexical()
        //                        .insert("new []", after: "startArray")
        //                        .replace("openBracket",  '{')
        //                        .replace("closeBracket", '}'));

        //        compiler
        //            .Syntaxis()
        //                .looseStatements(myStatementCompleter)
        //                .looseMembers(myMemberCompleter)
        //                .looseTypes(myTypeCompleter)

        //                .match<ClassDeclarationSyntax>(classDecl => classDecl.name == "myClass")
        //                    .then(myTransformation)
        //                .match<IdentifierNameSyntax>(identifier => identifier.ToString() == "myIdent")
        //                    .then(compiler.Syntaxis()
        //                        .replace(identifier => Syntax.Identifier("myOtherIdent")))
        //                .matchCodeDSL("myDSL")
        //                    .then(myTransformation);

        //        compiler
        //            .Semantics()
        //                .matchError("CS2001")
        //                    .then(myErrorFixing);
        //    }
        //}
    }
}