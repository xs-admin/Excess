using Excess.Compiler.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace xsc
{
    public class Parser
    {
        public static void Run(string[] args)
        {
            try
            {
                var toRun = parseArguments(args);
                var code = $@"
                    using System;
                    using Excess.Compiler.Roslyn;

                    namespace xsc
                    {{
                        public class myRunner : Runner
                        {{
                            public void run()
                            {{
                                Files = directoryFiles();
                                SolutionFile = null;
                                
                                {toRun}

                                validateFlavors();
                                if (SolutionFile != null)
                                    buildSolution();
                                else
                                    buildFiles();
                            }}
                        }}
                    }}";

                buildAndExecute(code);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"xsc failed with: \n {ex.Message}");
            }
        }

        private static void buildAndExecute(string code)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            if (errors(syntaxTree.GetDiagnostics()))
                return;

            //link 
            MetadataReference[] references = new MetadataReference[]
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Runner).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(RoslynCompiler).Assembly.Location)
            };

            string assemblyName = Path.GetRandomFileName();
            CSharpCompilation compilation = CSharpCompilation.Create(
                assemblyName,
                syntaxTrees: new[] { syntaxTree },
                references: references,
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            using (var ms = new MemoryStream())
            {
                EmitResult result = compilation.Emit(ms);

                if (!result.Success)
                {
                    if (!errors(result.Diagnostics))
                        throw new InvalidOperationException("unexpected errors");
                }
                else
                {
                    ms.Seek(0, SeekOrigin.Begin);
                    Assembly assembly = Assembly.Load(ms.ToArray());

                    Type type = assembly.GetType("xsc.myRunner");
                    object obj = Activator.CreateInstance(type);

                    //and execute
                    type.InvokeMember("run",
                        BindingFlags.Default | BindingFlags.InvokeMethod,
                        null,
                        obj,
                        new object[] { });
                }
            }
        }

        private static bool errors(IEnumerable<Diagnostic> diagnostics)
        {
            var failures = diagnostics
                .Where(diagnostic =>
                    diagnostic.IsWarningAsError 
                    || diagnostic.Severity == DiagnosticSeverity.Error);

            bool result = false;
            foreach (Diagnostic diagnostic in failures)
            {
                if (!result)
                {
                    result = true;
                    Console.Error.WriteLine($"xsc failed with:");
                }

                Console.Error.WriteLine($"{diagnostic.Id}: {diagnostic.GetMessage()}");
            }

            return result;
        }

        class ParseResult
        {
            public string Result { get; set; }
            public int Consumed { get; set; }
        }

        private static readonly Dictionary<string, Func<string[], int, ParseResult>> options = new Dictionary<string, Func<string[], int, ParseResult>>
        {
            { "-code",       (args, index) => loadFile(args, index) },
            { "-solution",   (args, index) => consumeOne(args, index, "SolutionFile = @\"{0}\";") },
            { "-file",       (args, index) => consumeOne(args, index, "Files = new [] {@\"{0}\"};") },
            { "-files",      (args, index) => consumeOne(args, index, "Files = directoryFiles(@\"{0}\");") },
            { "-extensions", (args, index) => consumeOne(args, index, "Extensions = directoryExtensions(@\"{0}\");") },
        };

        private static ParseResult consumeOne(string[] args, int index, string template)
        {
            return new ParseResult
            {
                Result = string.Format(template, args[index]),
                Consumed = 1
            };
        }

        private static ParseResult loadFile(string[] args, int index)
        {
            var value = string.Empty;
            using (var stream = new FileStream(args[index], FileMode.Open))
            using (var reader = new StreamReader(stream))
            {
                value = reader.ReadToEnd();
            }

            return new ParseResult
            {
                Result = value,
                Consumed = 1
            };
        }

        private static string parseArguments(string[] args)
        {
            if (!args.Any())
                return string.Empty;

            var result = new StringBuilder();
            var index = 0;
            while (index < args.Length)
            {
                var processor = null as Func<string[], int, ParseResult>;
                var arg = args[index++];
                if (options.TryGetValue(arg, out processor))
                {
                    var parseResult = processor(args, index);
                    result.AppendLine(parseResult.Result);
                    index += parseResult.Consumed;
                }
                else if (arg.StartsWith("-"))
                {
                    //unknown arguments will be assumed as extension flavors
                    var extension = arg.Substring(1).Trim();
                    var flavor = args[index++];
                    result.Insert(0, $"Flavors[\"{extension}\"] = \"{flavor}\";\n");
                }
                else if (index == 1)
                {
                    //failed on the first token, assume its all code
                    return string.Join("", args);
                }
                else
                    throw new ArgumentException($"invalid argument {args[index - 1]}");
            }

            return result.ToString();
        }
    }
}
