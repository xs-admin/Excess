using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Excess.Compiler.Roslyn;
using xslang;
using Excess.Extensions.Concurrent;

namespace metaprogramming.server
{
    public class Transpiler : ITranspiler
    {
        RoslynCompiler _compiler = new RoslynCompiler();
        public Transpiler()
        {
            XSLanguage.Apply(_compiler);
            ConcurrentExtension.Apply(_compiler);
        }

        public string Process(string source)
        {
            string result;
            _compiler.ApplySemanticalPass(source, out result);
            return result;
        }
    }
}
