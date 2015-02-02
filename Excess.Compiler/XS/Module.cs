using Excess.Compiler.Roslyn;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Diagnostics;
using CSharp = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Excess.Compiler.XS
{
    public class XSModule
    {
        static public void Apply(RoslynCompiler compiler)
        {
            Functions .Apply(compiler);
            Members   .Apply(compiler);
            Events    .Apply(compiler);
            TypeDef   .Apply(compiler);
         }
    }
}
