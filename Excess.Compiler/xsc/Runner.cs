using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Excess.Compiler.Roslyn;

namespace xsc
{
    using System.Reflection;
    using ExcessCompilation = Excess.Compiler.Roslyn.Compilation;

    public class Runner
    {
        public IEnumerable<string> Files { get; set; }
        public string SolutionFile { get; set; }
        public IDictionary<string, Action<RoslynCompiler>> Extensions { get; set; }
        public IDictionary<string, string> Flavors { get; set; }

        public Runner()
        {
            Flavors = new Dictionary<string, string>();
        }

        public IEnumerable<string> directoryFiles(string directory = null)
        {
            if (directory == null)
                directory = Environment.CurrentDirectory;

            var validExtensions = new[] { ".cs", ".xs" };
            return Directory.EnumerateFiles(directory, "*", SearchOption.AllDirectories)
                .Where(file => validExtensions.Contains(Path.GetExtension(file))
                            && !file.Contains("Generated"));
        }

        public IDictionary<string, Action<RoslynCompiler>> directoryExtensions(string directory)
        {
            var items = Directory.EnumerateFiles(directory, "*", SearchOption.AllDirectories)
                .Where(file => Path.GetExtension(file) == ".dll"
                            && Path.GetFileName(file).StartsWith("Excess.Extensions"))
                .Select(dll => loadExtension(dll));

            var result = new Dictionary<string, Action<RoslynCompiler>>();
            foreach (var item in items)
            {
                if (item.Equals(default(KeyValuePair<string, Action<RoslynCompiler>>)))
                    continue;

                result[item.Key] = item.Value;
            }

            return result;
        }

        private KeyValuePair<string, Action<RoslynCompiler>> loadExtension(string dll)
        {
            var assembly = Assembly.LoadFile(dll);
            var extensionTypes = assembly
                .GetTypes()
                .Where(type => type.Name == "Extension");

            var id = Path
                .GetFileNameWithoutExtension(dll)
                .Substring("Excess.Extensions.".Length)
                .ToLower();

            var flavor = "Apply";
            if (Flavors.TryGetValue(id, out flavor))
                Flavors.Remove(id);

            var method = null as MethodInfo;
            foreach (var extensionType in extensionTypes)
            {
                method = extensionType
                    .GetMethods()
                    .Where(m =>
                    {
                        if (!m.IsStatic || !m.IsPublic)
                            return false;

                        var parameters = m.GetParameters();
                        return parameters.Length == 1
                            && parameters[0].ParameterType.Name == "RoslynCompiler";
                    }).SingleOrDefault();

                if (method != null && method.Name == flavor)
                    break;
            }

            if (method == null)
                return default(KeyValuePair<string, Action<RoslynCompiler>>);

            return new KeyValuePair<string, Action<RoslynCompiler>>(id,
                compiler => method.Invoke(null, new object[] { compiler }));
        }

        public void validateFlavors()
        {
            if (Flavors.Any())
                throw new InvalidProgramException($"invalid argument(s) {string.Join(", ", Flavors.Keys.ToArray())}");
        }

        public void buildSolution()
        {
            if (SolutionFile == null || !File.Exists(SolutionFile))
                throw new InvalidProgramException($"invalid solution file: {SolutionFile ?? "null"}");

            throw new NotImplementedException();
        }

        public void buildFiles()
        {
            if (Files == null)
                throw new InvalidProgramException("must specify which files to compile");

            var actualFiles = Files.ToArray();
            if (actualFiles.Length == 0)
                throw new InvalidProgramException($"must specify which files to compile");

            var compilation = new ExcessCompilation(injectors: Extensions);
            foreach (var file in actualFiles)
            {
                var ext = Path.GetExtension(file);
                switch (ext)
                {
                    case ".cs": compilation.addCSharpFile(file); break;
                    case ".xs": compilation.addDocument(file); break;

                    default: throw new InvalidOperationException($"invalid extension: {ext}");
                }
            }

            var errors = null as IEnumerable<Diagnostic>;
            var result = compilation.build(out errors);
            if (result != null)
            {
                throw new NotImplementedException();
            }
            else
            {
                foreach (var error in errors)
                    Console.Error.WriteLine(error.ToString());
            }
        }

    }
}
