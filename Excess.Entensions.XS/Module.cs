using Excess.Compiler;
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
using Excess.Compiler.Core;

namespace Excess.Entensions.XS
{
    using CSharp = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
    using ExcessCompiler = ICompiler<SyntaxToken, SyntaxNode, SemanticModel>;
    using Injector = ICompilerInjector<SyntaxToken, SyntaxNode, SemanticModel>;
    using DelegateInjector = DelegateInjector<SyntaxToken, SyntaxNode, SemanticModel>;

    public class XSModule
    {
        public static void Apply(ExcessCompiler compiler)
        {
            Functions   .Apply(compiler);
            Members     .Apply(compiler);
            Events      .Apply(compiler);
            TypeDef     .Apply(compiler);
            Arrays      .Apply(compiler);  
        }

        public static Injector Create()
        {
            return new DelegateInjector(compiler => Apply(compiler));
        }
    }
}
