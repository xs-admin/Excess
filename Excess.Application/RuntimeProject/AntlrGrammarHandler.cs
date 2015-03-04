using Excess.Compiler.Roslyn;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Excess.RuntimeProject
{
    using CSharp = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
    using Compilation = Excess.Compiler.Roslyn.Compilation;

    public class AntlrGrammarHandler : IFileExtension
    {
        public void process(string fileName, int fileId, string contents, Compilation compilation)
        {
            string path = compilation.Environment.path().ToolPath;
            string grammar = Path.GetFileName(fileName);
            string file = grammar + "ExcessVisitor.antlr";

            if (!compilation.hasDocument(file))
            {
                Dictionary<string, string> files = CompileGrammar(grammar, path, contents);
                if (files.Count == 6)
                {
                    grammar = Path.GetFileNameWithoutExtension(grammar);
                    files["Excess"] = GenerateExcessFile(grammar, files[grammar + "BaseVisitor.cs"]);
                }
            }
        }

        static Template excessFile = Template.Parse(@"
            using Excess.Compiler.
            public class ExcessVisitor : _0
            {
                
                public ExcessVisitor()
                {
                }
            }");


        private string GenerateExcessFile(string grammar, string baseFile)
        {
            var syntaxTree = CSharp.ParseSyntaxTree(baseFile);

            var methods = syntaxTree
                .GetRoot()
                .DescendantNodes()
                .OfType<MethodDeclarationSyntax>()
                .Select(method => OverrideMethod(method));

            var result = excessFile.Get<ClassDeclarationSyntax>(grammar + "BaseVisitor.cs")
                .WithMembers(CSharp.List(
                    methods
                        .Select(method => method as MemberDeclarationSyntax)));

            return result
                .SyntaxTree
                .GetRoot()
                .NormalizeWhitespace()
                .ToString();
        }

        static Template contextCall = Template.ParseStatement("return _ctx.notify<__0>(context);");

        private MethodDeclarationSyntax OverrideMethod(MethodDeclarationSyntax method)
        {
            return method
                .WithModifiers(CSharp.TokenList(method
                        .Modifiers
                        .Where(modifier => modifier.CSharpKind() != SyntaxKind.VirtualKeyword)
                        .Union(new[] { CSharp.Token(SyntaxKind.OverrideKeyword) })
                        .ToArray()))
                .WithBody(CSharp.Block()
                    .WithStatements(CSharp.List(new[] {
                        contextCall.Get<StatementSyntax>(method
                            .ParameterList
                            .Parameters
                            .First()
                            .Type)
                    })));
        }

        private static string AntlrExe = "antlr-4.5-complete.exe";

        private Dictionary<string, string> CompileGrammar(string grammar, string toolPath, string contents)
        {
            var exeFile = Path.Combine(toolPath, AntlrExe);
            var exePath = Path.GetDirectoryName(exeFile);
            Debug.Assert(File.Exists(exeFile));

            var result = new Dictionary<string, string>();
            var tempDir = Path.Combine(exePath, Guid.NewGuid().ToString().Replace("-", ""));

            Directory.CreateDirectory(tempDir);
            var g4 = Path.Combine(tempDir, grammar);
            using (var writer = File.CreateText(g4))
            {
                writer.Write(contents);
                writer.Close();
            }

            ProcessStartInfo start = new ProcessStartInfo();
            start.Arguments = string.Format("-Dlanguage=CSharp -no-listener -visitor -o \"{1}\" {0}", g4, tempDir);
            start.FileName = exeFile;
            start.WindowStyle = ProcessWindowStyle.Hidden;
            start.CreateNoWindow = true;
            start.UseShellExecute = false;
            start.RedirectStandardOutput = true;

            // Run the external process & wait for it to finish
            using (Process proc = Process.Start(start))
            {
                //td: catch errors
                //StringBuilder error = new StringBuilder();
                //proc.ErrorDataReceived += (sender, e) =>
                //{
                //    if (e.Data != null)
                //        error.Append(e.Data);
                //};

                proc.WaitForExit();
                var files = Directory.GetFiles(tempDir);
                foreach (var file in files)
                {
                    if (Path.GetFileName(file).Equals(grammar))
                        continue;

                    var resultName = Path.GetFileName(file);
                    result[resultName] = File.ReadAllText(file);
                }

                Directory.Delete(tempDir, true);
            }

            return result;
        }
    }
}
