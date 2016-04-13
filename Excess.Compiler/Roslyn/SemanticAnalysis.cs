using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess.Compiler.Roslyn
{
    public class RoslynSemanticAnalysis : ISemanticAnalysis<SyntaxToken, SyntaxNode, SemanticModel>,
                                          IDocumentInjector<SyntaxToken, SyntaxNode, SemanticModel>
    {
        class ErrorHandler
        {
            public string ErrorId { get; set; }
            public Action<SyntaxNode, SemanticModel, Scope> Handler { get; set; }
        }

        List<ErrorHandler> _errors = new List<ErrorHandler>();
        public ISemanticAnalysis<SyntaxToken, SyntaxNode, SemanticModel> error(string error, Action<SyntaxNode, SemanticModel, Scope> handler)
        {
            _errors.Add(new ErrorHandler { ErrorId = error, Handler = handler });
            return this;
        }

        public ISemanticAnalysis<SyntaxToken, SyntaxNode, SemanticModel> error(string error, Action<SyntaxNode, Scope> handler)
        {
            _errors.Add(new ErrorHandler { ErrorId = error, Handler = (node, model, scope) => handler(node, scope) });
            return this;
        }

        public void apply(IDocument<SyntaxToken, SyntaxNode, SemanticModel> document)
        {
            document.change(HandleErrors);
        }

        private SyntaxNode HandleErrors(SyntaxNode root, SemanticModel model, Scope scope)
        {
            var errors = model.GetDiagnostics();
            foreach (var error in errors)
            {
                string id = error.Id;
                foreach (var handler in _errors)
                {
                    if (handler.ErrorId == id)
                    {
                        try
                        {
                            var node = root.FindNode(error.Location.SourceSpan);
                            handler.Handler(node, model, scope);
                        }
                        catch
                        {
                        }
                    }
                }
            }

            return root; //unmodified
        }
    }
}
