using Excess.Compiler.Roslyn;
using Excess.Extensions.Concurrent;
using xslang;

namespace demo_transpiler
{
    public interface ITranspiler
    {
        string Process(string source);
    }

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
