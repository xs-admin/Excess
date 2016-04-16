using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.LanguageServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.ComponentModelHost;
using NuGet;
using NuGet.VisualStudio;
using Excess.Compiler.Roslyn;
using Excess.Entensions.XS;

namespace Excess.VS
{
    using CSharp = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

    internal static class XSKeywords            
    {
        public static string[] Values = new[]
        {
            "function",
            "method",
            "property",
            "on",
            "match",
            "constructor",
            "var",
        };
    }

    class ExcessLanguageService : LanguageService
    {
        VisualStudioWorkspace _workspace;
        public ExcessLanguageService(VisualStudioWorkspace workspace)
        {
            _workspace = workspace;
        }

        public override string Name
        {
            get
            {
                return "xs";
            }
        }

        class ProjectCache
        {
            VisualStudioWorkspace _workspace;
            string _path;
            public ProjectCache(VisualStudioWorkspace workspace, ProjectId projectId, IVsPackageInstallerServices nuget, IVsPackageInstallerEvents nugetEvents)
            {
                var project = workspace.CurrentSolution.GetProject(projectId);

                _workspace = workspace;
                _path = Path.GetDirectoryName(project.FilePath);

                registerPackages(nuget.GetInstalledPackages());
                nugetEvents.PackageInstalled += nugetEventsPackageInstalled;
            }

            private void registerPackages(IEnumerable<IVsPackageMetadata> packages)
            {
                foreach (var package in packages)
                    addExtension(package);
            }

            private void nugetEventsPackageInstalled(IVsPackageMetadata package)
            {
                addExtension(package);
            }

            private void addExtension(IVsPackageMetadata package)
            {
                //td: determine if the package is installed in this project
                if (!package.Id.StartsWith("Excess.Extensions."))
                    return;

                var name = package
                    .Id
                    .Substring("Excess.Extensions.".Length)
                    .ToLower();

                var extensionPath = Path.Combine(package.InstallPath, $"tools\\{package.Id}.dll");
                if (!File.Exists(extensionPath))
                    return;

                _extensions[name] = loadReference(extensionPath, name);
            }

            Dictionary<long, RoslynCompiler> _cache = new Dictionary<long, RoslynCompiler>();
            Dictionary<long, IEnumerable<string>> _keywordCache = new Dictionary<long, IEnumerable<string>>();
            Dictionary<DocumentId, long> _documentExtensions = new Dictionary<DocumentId, long>();
            Dictionary<string, Action<RoslynCompiler, IList<string>>> _extensions = new Dictionary<string, Action<RoslynCompiler, IList<string>>>();

            public RoslynCompiler GetCompiler(DocumentId documentId, ICollection<UsingDirectiveSyntax> extensions, out IEnumerable<string> keywords)
            {
                keywords = null;

                //get an unique id 
                var extensionNames = extensions
                    .Where(@using => isExtension(@using))
                    .Select(@using => @using.Name.ToString());

                long hashCode = 0;
                foreach (var extension in extensionNames)
                    hashCode += extension.GetHashCode();

                //test the cache for some combination of extensions
                var result = null as RoslynCompiler;
                if (_cache.TryGetValue(hashCode, out result))
                {
                    keywords = _keywordCache[hashCode];
                    return result;
                }

                result = new RoslynCompiler();
                XSLang.Apply(result);

                var keywordList = new List<string>();
                foreach (var extension in extensions.ToArray())
                {
                    if (isExtension(extension))
                    {
                        var extensionName = extension
                            .Name
                            .ToString()
                            .Substring("xs.".Length);

                        var extensionFunc = null as Action<RoslynCompiler, IList<string>>;
                        if (_extensions.TryGetValue(extensionName, out extensionFunc))
                        {
                            extensionFunc(result, keywordList);
                            continue;
                        }
                    }

                    extensions.Remove(extension);
                }

                keywords = keywordList;
                _cache[hashCode] = result;
                _keywordCache[hashCode] = keywords;
                return result;
            }

            public IEnumerable<string> Keywords(DocumentId documentId)
            {
                long iid;
                if (!_documentExtensions.TryGetValue(documentId, out iid))
                    return null;

                IEnumerable<string> result = XSKeywords.Values;
                IEnumerable<string> cache;
                if (_keywordCache.TryGetValue(iid, out cache))
                    result = result.Union(cache);

                return result;
            }

            private Action<RoslynCompiler, IList<string>> loadReference(string filePath, string name)
            {
                var assembly = Assembly.LoadFrom(filePath);

                var extensionType = assembly
                    .GetTypes()
                    .Where(type => type.Name == "Extension")
                    .SingleOrDefault();
                var flavorsType = assembly
                    .GetTypes()
                    .Where(type => type.Name == "Flavors")
                    .SingleOrDefault();

                var items = name.Split('.');
                var result = null as Action<RoslynCompiler, IList<string>>;
                switch (items.Length)
                {
                    case 1:
                        if (flavorsType != null)
                            result = loadExtension(flavorsType, "Default", extensionType);
                        
                         if (result == null && extensionType != null)
                            result = loadExtension(extensionType, "Apply", extensionType);
                        break;
                    case 2:
                        if (flavorsType != null)
                            result = loadExtension(flavorsType, items[1], extensionType);
                        break;
                    default:
                        throw new ArgumentException(name);
                }

                return result;
            }

            private Action<RoslynCompiler, IList<string>> loadExtension(Type type, string methodName, Type extensionType)
            {
                var method = type.GetMethod(methodName);
                if (method == null)
                    return null;

                var keywordMethod = extensionType != null
                    ? extensionType.GetMethod("GetKeywords")
                    : null;

                return (compiler, keywords) =>
                {
                    method.Invoke(null, new object[] { compiler });

                    if (keywordMethod != null)
                    {
                        var extKeywords = (IEnumerable<string>)keywordMethod.Invoke(null, new object[] {});
                        keywords.AddRange(extKeywords);
                    }
                };
            }

            private bool isExtension(UsingDirectiveSyntax @using) => @using.Name.ToString().StartsWith("xs.");

        }

        Dictionary<ProjectId, ProjectCache> _projects = new Dictionary<ProjectId, ProjectCache>();
        public RoslynDocument CreateExcessDocument(string text, DocumentId document)
        {
            ProjectCache cache;
            if (!_projects.TryGetValue(document.ProjectId, out cache))
            {
                ensureNuget();

                cache = new ProjectCache(_workspace, document.ProjectId, _nuget, _nugetEvents);
                _projects[document.ProjectId] = cache;
            }

            //td: we need the using list in order to deduct the extensions
            //however, we don't need to parse the whole document.
            //We must optimize this (maybe a custom using parser?)
            var compilationUnit = CSharp.ParseCompilationUnit(text);
            var extensions = new List<UsingDirectiveSyntax>(compilationUnit.Usings);
            var keywords = null as IEnumerable<string>;
            var compiler = cache.GetCompiler(document, extensions, out keywords);

            //build a new document
            var result = new RoslynDocument(compiler.Scope, text);
            result.Mapper = new MappingService();
            compiler.apply(result);

            var scanner = null as Scanner;
            if (keywords != null && keywords.Any()  && _scannerCache.TryGetValue(document, out scanner))
                scanner.Keywords = XSKeywords.Values.Union(keywords);

            return result;
        }

        IVsPackageInstallerServices _nuget;
        IVsPackageInstallerEvents _nugetEvents;
        private void ensureNuget()
        {
            if (_nuget == null)
            {
                var componentModel = (IComponentModel)GetService(typeof(SComponentModel));
                _nuget = componentModel.GetService<IVsPackageInstallerServices>();
                _nugetEvents = componentModel.GetService<IVsPackageInstallerEvents>();
            }

            if (_nuget == null || _nugetEvents == null)
                throw new InvalidOperationException("nuget");
        }

        public override string GetFormatFilterList()
        {
            return "XS files (*.xs)\n*.xs\n";
        }

        private LanguagePreferences _preferences;
        public override LanguagePreferences GetLanguagePreferences()
        {
            if (_preferences == null)
                _preferences = new LanguagePreferences(Site, typeof(ExcessLanguageService).GUID, Name);

            return _preferences;
        }

        Dictionary<DocumentId, Scanner> _scannerCache = new Dictionary<DocumentId, Scanner>();
        public override IScanner GetScanner(IVsTextLines buffer)
        {
            var documentId = GetDocumentId(buffer);
            if (documentId == null)
                throw new ArgumentException("buffer");

            var result = null as Scanner;
            if (_scannerCache.TryGetValue(documentId, out result))
                return result;

            var scanner = new Scanner(XSKeywords.Values);
            _scannerCache[documentId] = scanner;
            return scanner;
        }

        public override AuthoringScope ParseSource(ParseRequest req)
        {
            return new ExcessAuthoringScope();
        }

        public override Source CreateSource(IVsTextLines buffer)
        {
            var scanner = GetScanner(buffer) as Scanner;
            Debug.Assert(scanner != null);
            Debug.Assert(scanner.Keywords == XSKeywords.Values);

            var documentId = GetDocumentId(buffer);
            scanner.Keywords = GetKeywords(documentId);
            return new ExcessSource(this, buffer, new Colorizer(this, buffer, scanner), _workspace, documentId);
        }

        private IEnumerable<string> GetKeywords(DocumentId documentId)
        {
            var project = null as ProjectCache;
            if (_projects.TryGetValue(documentId.ProjectId, out project))
                return project.Keywords(documentId);

            return XSKeywords.Values;
        }

        private DocumentId GetDocumentId(IVsTextLines buffer)
        {
            var filePath = FilePathUtilities.GetFilePath(buffer) + ".cs";
            var documents = _workspace
                .CurrentSolution
                .GetDocumentIdsWithFilePath(filePath);

            return documents.Single();
        }
    }
}
