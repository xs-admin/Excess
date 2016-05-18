using System;
using Excess.Compiler;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Excess.Compiler.Core;
using Excess.Compiler.Roslyn;

namespace Excess.VS
{
    class VSCompilation : ICompilation<SyntaxToken, SyntaxNode, SemanticModel>
    {
        Project _project;
        Scope _scope;
        public VSCompilation(Project project, Scope scope)
        {
            _project = project;
            _scope = scope;
        }

        public Project Project
        {
            get
            {
                return _project;
            }
        }


        public Scope Scope
        {
            get
            {
                return _scope;
            }
        }

        public void AddContent(string path, string contents)
        {
            _project = _project
                .AddAdditionalDocument(Path.GetFileName(path), contents, filePath: Path.GetDirectoryName(path))
                .Project;
        }

        public void AddDocument(string path, IDocument<SyntaxToken, SyntaxNode, SemanticModel> document)
        {
            throw new NotImplementedException();
        }

        public void AddDocument(string path, string contents)
        {
            throw new NotImplementedException();
        }

        public void AddNativeDocument(string path, string contents)
        {
            _project = _project
                .AddDocument(Path.GetFileName(path), contents, filePath: Path.GetDirectoryName(path))
                .Project;
        }

        public void AddNativeDocument(string path, SyntaxNode root)
        {
            throw new NotImplementedException();
        }

        public SemanticModel GetSemanticModel(SyntaxNode node)
        {
            var doc = _project.GetDocument(node.SyntaxTree);
            return doc?.GetSemanticModelAsync().Result;
        }

        public void ReplaceNode(SyntaxNode old, SyntaxNode @new)
        {
            throw new NotImplementedException();
        }

        public void PerformAnalysis(CompilationAnalysisBase<SyntaxToken, SyntaxNode, SemanticModel> analysis)
        {
            foreach (var doc in _project.Documents)
            {
                var root = doc.GetSyntaxRootAsync().Result;
                if (root != null)
                {
                    var visitor = new CompilationAnalysisVisitor(analysis, this, _scope);
                    visitor.Visit(root);
                }
            }

            analysis.Finish(this, _scope);
        }
    }
}
