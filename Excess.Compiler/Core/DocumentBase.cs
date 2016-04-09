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
            _scope    = new Scope(scope);
            _compiler = _scope.GetService<TToken, TNode, TModel>();

            //setup the node repository
            //td: per document?
            _scope.set<IDictionary<int, Scope>>(new Dictionary<int, Scope>());
        }


        public CompilerStage Stage { get; internal set; }
        public string Text { get { return _text; } set { update(value); } }
        public TNode SyntaxRoot { get { return getRoot(); } set { setRoot(value); } }
        public TModel Model { get; set; }
        public Scope Scope { get { return _scope; } }
        public IMappingService<TNode> Mapper { get; set; }

        protected abstract TNode getRoot();
        protected abstract void setRoot(TNode node);

        protected string _text;
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
            public Func<TNode, TNode, TModel, Scope, TNode> SemanticalTransform { get; set; }
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

        public TToken change(TToken token, Func<TNode, TNode, TModel, Scope, TNode> transform, string kind = null)
        {
            int tokenId;
            TToken result = _compiler.MarkToken(token, out tokenId);

            _lexicalChanges.Add(new Change
            {
                ID = tokenId,
                Kind = kind,
                SemanticalTransform = transform
            });

            return result;
        }

        public IEnumerable<TToken> change(IEnumerable<TToken> tokens, Func<TNode, TNode, TModel, Scope, TNode> transform, string kind = null)
        {
            int id;
            var result = _compiler.MarkTokens(tokens, out id);
            _lexicalChanges.Add(new Change
            {
                ID = id,
                Kind = kind,
                SemanticalTransform = transform
            });

            return result;
        }

        List<Func<TNode, Scope, TNode>> _syntactical = new List<Func<TNode, Scope, TNode>>();
        Dictionary<string, Func<TNode, Scope, TNode>> _syntacticalPass = new Dictionary<string, Func<TNode, Scope, TNode>>();

        public void change(Func<TNode, Scope, TNode> transform, string kind)
        {
            if (kind != null)
            {
                //known kinds
                switch (kind)
                {
                    case "normalize":
                    {
                        _lexicalChanges.Add(new Change
                        {
                            Kind = kind,
                            Transform = transform
                        });
                        break;
                    }
                    default:
                    {
                        _syntacticalPass[kind] = transform;
                        break;
                    }
                }
            }
            else
                _syntactical.Add(transform);
        }

        List<Change> _syntacticalChanges = new List<Change>();
        public TNode change(TNode node, Func<TNode, Scope, TNode> transform, string kind = null)
        {
            int nodeId;
            TNode result = _compiler.MarkNode(node, out nodeId);

            if (_stage <= CompilerStage.Syntactical)
            {
                _syntacticalChanges.Add(new Change
                {
                    ID = nodeId,
                    Kind = kind,
                    Transform = transform
                });
            }
            else
            {
                Debug.Assert(_stage == CompilerStage.Semantical);
                _semanticalChanges.Add(new Change
                {
                    ID = nodeId,
                    Kind = kind,
                    SemanticalTransform = (oldNode, newNode, model, scope) => transform(newNode, scope)
                });
            }

            return result;
        }

        List<Func<TNode, TModel, Scope, TNode>> _semantical = new List<Func<TNode, TModel, Scope, TNode>>();
        public void change(Func<TNode, TModel, Scope, TNode> transform, string kind = null)
        {
            Debug.Assert(kind == null); //td: 

            _semantical.Add(transform);
        }

        List<Change> _semanticalChanges = new List<Change>();
        public TNode change(TNode node, Func<TNode, TNode, TModel, Scope, TNode> transform, string kind)
        {
            int nodeId;
            TNode result = _compiler.MarkNode(node, out nodeId);

            _semanticalChanges.Add(new Change
            {
                ID = nodeId,
                Kind = kind,
                SemanticalTransform = transform
            });

            return result;
        }

        protected CompilerStage _stage = CompilerStage.Started;
        public bool applyChanges()
        {
            return applyChanges(CompilerStage.Finished);
        }

        int _semanticalTries = 0; 
        public bool applyChanges(CompilerStage stage)
        {
            if (stage < _stage)
                return true;

            var oldStage = _stage;
            _stage = stage;

            if (oldStage < CompilerStage.Lexical && stage >= CompilerStage.Lexical)
                applyLexical();

            if (oldStage < CompilerStage.Syntactical && stage >= CompilerStage.Syntactical)
            {
                applySyntactical();
                _semanticalTries = 0;
            }

            if (oldStage <= CompilerStage.Semantical && stage >= CompilerStage.Semantical)
            {
                bool result = applySemantical();
                if (result)
                    _semanticalTries = 0;
                else
                    _semanticalTries++;
                return result;
            }

            return true; //finished
        }

        public bool HasSemanticalChanges()
        {
            if (_semanticalTries > 5)
                return false; //shortcut

            if (_semanticalTries == 0 && _semantical.Count > 0)
                return true;

            return _semanticalChanges.Count > 0;
        }

        public abstract bool hasErrors();

        protected TNode _root;
        protected TNode _original;

        protected virtual void applyLexical()
        {
            //apply the lexical pass
            var tokens = _root == null
                ? _compiler.ParseTokens(_text)
                : _compiler.NodeTokens(_root);

            foreach (var lexical in _lexical)
                tokens = lexical(tokens, new Scope(_scope));

            //build modified text for syntactic parsing
            string resultText;
            Dictionary<string, SourceSpan> annotations = new Dictionary<string, SourceSpan>();
            _root = calculateNewText(tokens, annotations, out resultText);

            //allow extensions writers to perform one last rewrite before the syntactical 
            _root = applyNodeChanges(_root, "lexical-extension", CompilerStage.Lexical);

            //move any pending changes to the appropriate pass
            _syntacticalChanges.AddRange(_lexicalChanges
                .Where(change => change.Transform != null));

            _semanticalChanges.AddRange(_lexicalChanges
                .Where(change => change.SemanticalTransform != null));
        }

        private TNode calculateNewText(IEnumerable<TToken> tokens, Dictionary<string, SourceSpan> annotations, out string modifiedText)
        {
            //td: !! mapping info
            StringBuilder newText = new StringBuilder();
            string currId = null;
            foreach (var token in tokens)
            {
                string excessId;
                string toInsert = _compiler.TokenToString(token, out excessId);

                //store the actual position in the transformed stream of any tokens pending processing
                if (excessId != currId)
                {
                    if (excessId != null)
                        annotations[excessId] = new SourceSpan(newText.Length, toInsert.Length);

                    currId = excessId;
                }
                else if (excessId != null)
                {
                    //augment span
                    SourceSpan span = annotations[excessId];
                    span.Length += toInsert.Length;
                }
                else
                    currId = null;

                newText.Append(toInsert);
            }

            //parse
            modifiedText = newText.ToString();
            var root = _compiler.Parse(modifiedText);

            //assign ids to the nodes found
            root = _compiler.MarkTree(root);
            _original = root;

            processAnnotations(root, annotations);

            //allow for preprocessing of the original 
            notifyOriginal(modifiedText);

            //apply any scheduled normalization
            var normalizers = poll(_lexicalChanges, "normalize");
            foreach (var normalizer in normalizers)
            {
                root = normalizer.Transform(root, new Scope(_scope));
            }

            //allow for preprocessing
            _original = updateRoot(_original);
            root = updateRoot(root);
            return root;
        }

        private void processAnnotations(TNode root, Dictionary<string, SourceSpan> annotations)
        {
            foreach (var annotation in annotations)
            {
                TNode aNode = _compiler.Find(root, annotation.Value);
                Debug.Assert(aNode != null);

                //change the id of the change from token to node
                int annotationId = int.Parse(annotation.Key);
                foreach (var change in _lexicalChanges)
                {
                    if (change.ID == annotationId)
                    {
                        change.ID = _compiler.GetExcessId(aNode);
                        Debug.Assert(change.ID >= 0);
                    }
                }
            }
        }

        protected virtual void notifyOriginal(string newText)
        {
        }

        protected virtual TNode updateRoot(TNode root)
        {
            return root;
        }

        private TNode applyNodeChanges(TNode root, CompilerStage stage)
        {
            return applyNodeChanges(root, null, stage);
        }

        private void addTransformer(Dictionary<int, Func<TNode, TNode, TModel, Scope, TNode>> transformers, Change change)
        {
            var transform = change.SemanticalTransform;
            if (transform == null)
            {
                Debug.Assert(change.Transform != null);
                transform = (oldNode, newNode, model, scope) => change.Transform(oldNode, scope);
            }

            Func<TNode, TNode, TModel, Scope, TNode> existing;
            if (transformers.TryGetValue(change.ID, out existing))
            {
                var newTransform = transform;
                transform = (oldNode, newNode, model, scope) =>
                {
                    var result = existing(oldNode, newNode, model, scope);
                    return newTransform(oldNode, result, model, scope);
                };
            }

            transformers[change.ID] = transform;
        }

        private void addTransformer(Dictionary<int, Func<TNode, Scope, TNode>> transformers, Change change)
        {
            var transform = change.Transform;

            Func<TNode, Scope, TNode> existing;
            if (transformers.TryGetValue(change.ID, out existing))
            {
                var newTransform = transform;
                transform = (node, scope) =>
                {
                    var result = existing(node, scope);
                    return newTransform(result, scope);
                };
            }

            transformers[change.ID] = transform;
        }

        private TNode applyNodeChanges(TNode node, string kind, CompilerStage stage)
        {
            List<Change> changeList = null;
            switch (stage)
            {
                case CompilerStage.Lexical:     changeList = _lexicalChanges; break;
                case CompilerStage.Syntactical: changeList = _syntacticalChanges; break;
                case CompilerStage.Semantical:  changeList = _semanticalChanges; break;
                default: throw new NotImplementedException();
            }

            if (!changeList.Any())
                return node;

            IEnumerable<Change> changes;
            if (kind != null)
                changes = poll(changeList, kind);
            else
            {
                changes = new List<Change>(changeList);
                changeList.Clear(); //take all for the stage
            }

            if (changes == null || !changes.Any())
                return node;

            if (stage == CompilerStage.Semantical)
            {
                var transformers = new Dictionary<int, Func<TNode, TNode, TModel, Scope, TNode>>();
                foreach (var change in changes)
                {
                    addTransformer(transformers, change);
                    //if (change.SemanticalTransform != null)
                    //    transformers[change.ID] = change.SemanticalTransform; duplicates IDS
                    //else
                    //{
                    //    Debug.Assert(change.Transform != null);
                    //    transformers[change.ID] = (oldNode, newNode, model, scope) => change.Transform(oldNode, scope);
                    //}
                }

                return transform(node, transformers);
            }
            else
            {
                var transformers = new Dictionary<int, Func<TNode, Scope, TNode>>();
                foreach (var change in changes)
                {
                    if (change.Transform != null)
                        addTransformer(transformers, change);

                    //transformers[change.ID] = change.Transform;
                }

                return transform(node, transformers);
            }
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
        protected abstract TNode transform(TNode root, Dictionary<int, Func<TNode, TNode, TModel, Scope, TNode>> transformers);

        protected virtual void applySyntactical()
        {
            Debug.Assert(_root != null);
            _root = pass("syntactical-extensions", _root, _scope);

            if (_syntactical.Any())
                _root = syntacticalTransform(_root, _scope, _syntactical);

            do
            {
                _root = applyNodeChanges(_root, CompilerStage.Syntactical);
            } while (_syntacticalChanges.Any());

            //add modules before going into semantics
            ICompilerEnvironment environment = _scope.find<ICompilerEnvironment>();
            if (environment != null)
                _root = addModules(_root, environment.modules());
        }

        protected abstract TNode addModules(TNode root, IEnumerable<string> modules);
        protected abstract TNode syntacticalTransform(TNode node, Scope scope, IEnumerable<Func<TNode, Scope, TNode>> transformers);

        protected TNode pass(string kind, TNode node, Scope scope)
        {
            Func<TNode, Scope, TNode> transform;
            if (_syntacticalPass.TryGetValue(kind, out transform))
                return transform(node, scope);

            return node;
        }

        protected virtual bool applySemantical()
        {
            if (_semanticalTries > 5)
                return true; //shortcut

            var oldRoot = _root;

            _root = applyNodeChanges(_root, CompilerStage.Semantical);

            foreach (var semantical in _semantical)
            {
                _root = semantical(_root, Model, _scope);
            }

            return oldRoot.Equals(_root); 
        }
    }
}
