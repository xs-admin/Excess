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
using Excess.Compiler;

namespace Excess.RuntimeProject
{
    using CSharp = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
    using Compilation = Excess.Compiler.Roslyn.Compilation;

    public class AntlrTool : ICompilationTool
    {
        public string displayName { get { return "Antlr"; } }
        public bool doNotCache { get { return false; } }
        public bool compile(string file, string contents, Scope scope, Dictionary<string, string> result)
        {
            var environment = scope.get<ICompilerEnvironment>();

            string path = environment.path().ToolPath;
            string grammar = Path.GetFileName(file);

            CompileGrammar(grammar, path, contents, result);
            if (result.Count != 4) //lol
                throw new InvalidDataException("Antlr failed compiling the grammar, we'll know more later");

            grammar = Path.GetFileNameWithoutExtension(grammar);

            var grammarFile = grammar + "Grammar.cs";
            result[grammarFile] = GenerateGrammarFile(grammar, result[grammar + "Parser.cs"]);

            return true;
        }

        static string grammarFile = @"
            using System;
            using System.Collections.Generic;
            using Excess.Compiler;
            using Excess.Compiler.Roslyn;
            using Microsoft.CodeAnalysis;
            using Microsoft.CodeAnalysis.CSharp;
            using Microsoft.CodeAnalysis.CSharp.Syntax;
            using Antlr4.Runtime;

            public class {0}Grammar : IGrammar<SyntaxToken, SyntaxNode, ParserRuleContext>
            {{
                public ParserRuleContext parse(IEnumerable<SyntaxToken> tokens, Scope scope, int offset)
                {{
                    var text = RoslynCompiler.TokensToString(tokens);
                    AntlrInputStream stream = new AntlrInputStream(text);
                    ITokenSource lexer = new {0}Lexer(stream);
                    ITokenStream tokenStream = new CommonTokenStream(lexer);
                    {0}Parser parser = new {0}Parser(tokenStream);

                    parser.AddErrorListener(new AntlrErrors<IToken>(scope, offset));
                    var result = parser.{1}();
                    if (parser.NumberOfSyntaxErrors > 0)
                        return null;

                    return result;
                }}
            }}";

        private string GenerateGrammarFile(string grammar, string parserFile)
        {
            var syntaxTree = CSharp.ParseSyntaxTree(parserFile);

            var ruleName = syntaxTree
                .GetRoot()
                .DescendantNodes()
                .OfType<FieldDeclarationSyntax>()
                .Where(field => field
                    .Declaration
                    .Variables[0]
                    .Identifier
                    .ToString() == "ruleNames")
                .First()
                    .DescendantNodes()
                    .OfType<InitializerExpressionSyntax>()
                    .First()
                        .Expressions[0]
                        .ToString();
                ;

            return string.Format(grammarFile, grammar, ruleName.Substring(1, ruleName.Length - 2));
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

        private void CompileGrammar(string grammar, string toolPath, string contents, Dictionary<string, string> result)
        {
            var exeFile = Path.Combine(toolPath, AntlrExe);
            var exePath = Path.GetDirectoryName(exeFile);
            Debug.Assert(File.Exists(exeFile));

            var tempDir = Path.Combine(exePath, Guid.NewGuid().ToString().Replace("-", ""));

            Directory.CreateDirectory(tempDir);
            var g4 = Path.Combine(tempDir, grammar);
            using (var writer = File.CreateText(g4))
            {
                writer.Write(contents);
                writer.Close();
            }

            File.Copy(Path.Combine(exePath, "expressions.g4"),
                      Path.Combine(tempDir, "expressions.g4"));

            ProcessStartInfo start = new ProcessStartInfo();
            start.Arguments = string.Format("-Dlanguage=CSharp -o \"{1}\" {0}", g4, tempDir);
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
                    if (Path.GetExtension(file) != ".cs")
                        continue;

                    var resultName = Path.GetFileName(file);
                    result[resultName] = File.ReadAllText(file);
                }

                Directory.Delete(tempDir, true);
            }
        }
    }
}
