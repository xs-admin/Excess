using Excess.Compiler.Roslyn;
using Excess.Concurrent.Compiler;
using xslang;
using metaprogramming.interfaces;

namespace metaprogramming.server.Roslyn
{
    public class Transpiler : ICodeTranspiler
    {
        RoslynCompiler _compiler = new RoslynCompiler();
        public Transpiler()
        {
            XSLanguage.Apply(_compiler);
            ConcurrentExtension.Apply(_compiler);
        }

        public string Transpile(string source)
        {
            string result;
            _compiler.ApplySemanticalPass(source, out result);
            return result;
        }
    }
}
