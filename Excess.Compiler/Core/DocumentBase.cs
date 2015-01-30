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
        protected Scope _scope;    
        protected ICompilerService<TToken, TNode, TModel> _compiler;
        public BaseDocument(Scope scope)
        {
            _scope    = scope;
            _compiler = _scope.GetService<TToken, TNode, TModel>();
        }


        protected string _text;
        public string Text { get { return _text; } set { update(value); } }

        protected void update(string text)
        {
            Debug.Assert(_text == null); //td: merge, or reset
            _text = text;
        }

        List<Func<IEnumerable<TToken>, Scope, IEnumerable<TToken>>> _lexical = new List<Func<IEnumerable<TToken>, Scope, IEnumerable<TToken>>>();

        public void change(Func<IEnumerable<TToken>, Scope, IEnumerable<TToken>> transform, string kind)
        {
            if (kind != null)
                throw new NotImplementedException();

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

        public IEnumerable<TToken> change(IEnumerable<TToken> tokens, Func<TNode, Scope, TNode> transform, string kind)
        {
            int id;
            var result = _compiler.MarkTokens(tokens, out id);
            _lexicalChanges.Add(new Change
            {
                ID = id,
                Kind = kind,
                Transform = transform
            });

            return result;
        }

        List<Func<TNode, Scope, TNode>> _syntactical = new List<Func<TNode, Scope, TNode>>();
        Dictionary<string, Func<TNode, Scope, TNode>> _syntacticalPass = new Dictionary<string, Func<TNode, Scope, TNode>>();

        public void change(Func<TNode, Scope, TNode> transform, string kind)
        {
            if (kind != null)
                _syntacticalPass[kind] = transform;
            else
                _syntactical.Add(transform);
        }

        List<Change> _syntacticalChanges = new List<Change>();
        public TNode change(TNode node, Func<TNode, Scope, TNode> transform, string kind)
        {
            int nodeId;
            TNode result = _compiler.MarkNode(node, out nodeId);

            _syntacticalChanges.Add(new Change
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

        public bool hasErrors()
        {
            throw new NotImplementedException();
        }

        protected TNode _root;
        private void applyLexical()
        {
            if (_root != null)
                return; //already applied

            LexicalPass<TToken, TNode, TModel> pass = new LexicalPass<TToken, TNode, TModel>(_scope, _lexical);
            Dictionary<string, SourceSpan> annotations  = new Dictionary<string, SourceSpan>();
            string resultText;
            _root = pass.Parse(_text, annotations, out resultText);

            notifyResultText(resultText);

            if (annotations.Any())
                _root = processAnnotations(_root, annotations);

            //allow extensions writers to perform one last rewrite before the syntactical 
            _root = applyNodeChanges(_root, "lexical-extension", CompilerStage.Lexical);
        }

        protected virtual void notifyResultText(string resultText)
        {
        }

        private TNode applyNodeChanges(TNode root, CompilerStage stage)
        {
            return applyNodeChanges(root, null, stage);
        }

        private TNode applyNodeChanges(TNode root, string kind, CompilerStage stage)
        {
            List<Change> changeList = null;
            switch (stage)
            {
                case CompilerStage.Lexical:     changeList = _lexicalChanges; break;
                case CompilerStage.Syntactical: changeList = _syntacticalChanges; break;
                default: throw new NotImplementedException();
            }


            IEnumerable<Change> changes;
            if (kind != null)
                changes = poll(changeList, kind);
            else
            {
                changes = new List<Change>(changeList);
                changeList.Clear(); //take all for the stage
            }

            if (changes == null || !changes.Any())
                return root;

            var transformers = new Dictionary<int, Func<TNode, Scope, TNode>>();
            foreach (var change in changes)
            {
                Debug.Assert(change.Transform != null);
                transformers[change.ID] = change.Transform;
            }

            return transform(root, transformers);
        }

        protected IEnumerable<Change> poll(List<Change> changes, string kind)
        {
            for (int i = changes.Count - 1; i >= 0; i--)
            {
                var change = changes[i];
                if (change.Kind == kind)
                {
                    yield return change;
                    changes.RemoveAt(i);
                }
            }
        }

        protected abstract TNode transform(TNode root, Dictionary<int, Func<TNode, Scope, TNode>> transformers);
        protected abstract TNode processAnnotations(TNode node, Dictionary<string, SourceSpan> annotations);


        private void applySyntactical()
        {
            Debug.Assert(_root != null);
            _root = pass("syntactical-extensions", _root, _scope);

            if (_syntactical.Any())
                _root = syntacticalTransform(_root, _scope, _syntactical);

            _root = applyNodeChanges(_root, CompilerStage.Syntactical);
        }

        protected abstract TNode syntacticalTransform(TNode node, Scope scope, IEnumerable<Func<TNode, Scope, TNode>> transformers);

        protected TNode pass(string kind, TNode node, Scope scope)
        {
            Func<TNode, Scope, TNode> transform;
            if (_syntacticalPass.TryGetValue(kind, out transform))
                return transform(node, scope);

            return node;
        }

    }
}
