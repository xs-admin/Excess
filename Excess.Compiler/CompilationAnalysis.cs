using System;

namespace Excess.Compiler
{
    public interface ICompilation<TToken, TNode, TModel>
    {
        Scope Scope { get; }
        TModel GetSemanticModel(TNode node);
        void AddContent(string path, string contents);
        void AddNativeDocument(string path, TNode root);
        void AddNativeDocument(string path, string contents);
        void AddDocument(string path, string contents);
        void AddDocument(string path, IDocument<TToken, TNode, TModel> document);
        void ReplaceNode(TNode old, TNode @new);
    }

    public interface ICompilationMatch<TToken, TNode, TModel>
    {
        ICompilationAnalysis<TToken, TNode, TModel> then(Action<TNode, ICompilation<TToken, TNode, TModel>, Scope> handler);

        bool matched(TNode node, ICompilation<TToken, TNode, TModel> compilation, Scope scope);
    }

    public interface ICompilationAnalysis<TToken, TNode, TModel>
    {
        ICompilationMatch<TToken, TNode, TModel> match<T>(Func<T, ICompilation<TToken, TNode, TModel>, Scope, bool> matcher) where T : TNode;
        ICompilationAnalysis<TToken, TNode, TModel> after(Action<ICompilation<TToken, TNode, TModel>, Scope> handler);
    }
}
