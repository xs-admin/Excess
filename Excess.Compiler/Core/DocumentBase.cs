using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess.Compiler.Core
{
    public abstract class BaseDocument<TToken, TNode, TModel> : IDocument<TToken, TNode, TModel>
    {
        Scope _scope;    
        ICompilerService<TToken, TNode> _compiler;
        public BaseDocument(Scope scope)
        {
            _scope = scope;
            _compiler = _scope.GetService<TToken, TNode>();
        }


        List<Func<IEnumerable<TToken>, Scope, IEnumerable<TToken>>> _lexical = new List<Func<IEnumerable<TToken>, Scope, IEnumerable<TToken>>>();
        public void change(Func<IEnumerable<TToken>, Scope, IEnumerable<TToken>> transform)
        {
            _lexical.Add(transform);
        }

        protected class Change
        {
            public int ID { get; set; }
            public string Kind { get; set; }
            public Func<TNode, Scope, TNode> Transform { get; set; }
        }

        List<Change> _lexicalChanges = new List<Change>();
        public TToken change(TToken token, Func<TNode, Scope, TNode> transform, string kind)
        {
            int tokenId;
            TToken result = _compiler.MarkToken(token, out tokenId);

            _lexicalChanges.Add(new Change
            {
                ID = tokenId,
                Kind = kind, 
                Transform = transform
            });

            return result;
        }

        List<Func<TNode, Scope, TNode>> _syntactical = new List<Func<TNode, Scope, TNode>>();
        public void change(Func<TNode, Scope, TNode> transform)
        {
            _syntactical.Add(transform);
        }

        public TNode change(TNode node, Func<TNode, Scope, TNode> transform, string kind)
        {
            int nodeId;
            TNode result = _compiler.MarkNode(node, out nodeId);

            _lexicalChanges.Add(new Change
            {
                ID = nodeId,
                Kind = kind,
                Transform = transform
            });

            return result;
        }

        public void applyChanges()
        {
            applyChanges(CompilerStage.Finished);
        }

        public void applyChanges(CompilerStage stage)
        {
            if (stage >= CompilerStage.Lexical)
                applyLexical();

            if (stage >= CompilerStage.Syntactical)
                applySyntactical();

        }

        string _text;
        TNode  _root;
        private void applyLexical()
        {
            if (_root != null)
                return; //already applied

            BaseLexicalPass<TToken, TNode> pass = new BaseLexicalPass<TToken, TNode>(_scope, matchers);
            Dictionary<string, SourceSpan> annotations = new Dictionary<string, SourceSpan>();
            _root = pass.Parse(_text, annotations);
            _root = processAnnotations(_root, annotations);

            //allow extensions writers to perform one last rewrite before the syntactical 
            _root = applyChanges(_root, "lexical-extension", CompilerStage.Lexical);
        }

        private TNode applyChanges(TNode root, string kind, CompilerStage stage)
        {
            switch (stage)
            {
                case CompilerStage.Lexical:
                {
                    IEnumerable<Change> changes = poll(_lexicalChanges, kind);
                    if (_lexicalChanges == null || !_lexicalChanges.Any())
                        return root;

                    var transformers = new Dictionary<int, Func<TNode, Scope, TNode>>();
                    foreach (var change in changes)
                    {
                        Debug.Assert(change.Transform != null);
                        transformers[change.ID] = change.Transform;
                    }

                    return transform(root, transformers);
                }
                default: throw new NotImplementedException();
            }
        }

        protected abstract TNode transform(TNode root, Dictionary<int, Func<TNode, Scope, TNode>> transformers);
        protected abstract TNode processAnnotations(TNode node, Dictionary<string, SourceSpan> annotations);

        private void applySyntactical()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TToken> change(IEnumerable<TToken> tokens, Func<TNode, Scope, TNode> transform, string kind = null)
        {
            throw new NotImplementedException();
        }
    }
}
