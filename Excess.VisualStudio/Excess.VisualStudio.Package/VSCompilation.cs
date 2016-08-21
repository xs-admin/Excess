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
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell.Settings;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace Excess.VisualStudio.VSPackage
{
    class VSCompilation : ICompilation<SyntaxToken, SyntaxNode, SemanticModel>
    {
        Project _project;
        Scope _scope;
        Dictionary<string, object> _settings;
        public VSCompilation(Project project, Scope scope)
        {
            _project = project;
            _scope = scope;

            _settings = new Dictionary<string, object>();
            _scope.set<ICompilerEnvironment>(new RoslynEnvironment(_scope, new SolutionStorage(project), _settings));
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

        public string GetContent(string path)
        {
            var file = _project.AdditionalDocuments
                .Where(doc => doc.FilePath.EndsWith(path))
                .FirstOrDefault();

            var result = default(SourceText);
            file?.TryGetText(out result);
            return result?.ToString();
        }

        public void AddContent(string path, string contents)
        {
            var contentFile = Path.Combine(
                Path.GetDirectoryName(_project.FilePath), 
                path);

            if (File.Exists(contentFile))
            {
                File.WriteAllText(contentFile, contents);
            }
            else
            {
                _project = _project
                    .AddAdditionalDocument(Path.GetFileName(path), contents, filePath: Path.GetDirectoryName(path))
                    .Project;
            }
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
            if (!_settings.Any())
            {
                var appConfig = Path.Combine(
                    Path.GetDirectoryName(_project.FilePath),
                    "xs.config");

                if (File.Exists(appConfig))
                {
                    var xmlDocument = XDocument.Load(appConfig);
                    var xsSettings = xmlDocument
                        .Element("configuration")
                        .Elements("setting");

                    foreach (var xsSetting in xsSettings)
                    {
                        var settingId = xsSetting.Attribute("id").Value;
                        _settings[settingId] = xsSetting.Attribute("value").Value;
                    }
                }
            }

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
