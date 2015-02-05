using Excess.Compiler.Roslyn;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess.Compiler.XS
{
    class Arrays
    {
        static public void Apply(RoslynCompiler compiler)
        {
            compiler.Lexical()
                .match()
                    .any(new[] { '(', '=', ',' }, named: "start", matchDocumentStart: true)
                    .enclosed('[', ']', start: "open", end: "close")
                    .then(compiler.Lexical().transform()
                        .insert("new []", after: "start")
                        .replace("open", "{")
                        .replace("close", "}"));
        }
    }
}