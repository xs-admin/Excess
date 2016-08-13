namespace metaprogramming.interfaces
{
    public interface ICodeTranspiler
    {
        string Transpile(string code);
    }

    public interface IGraphTranspiler
    {
        string Transpile(string code);
    }
}
