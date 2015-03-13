using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.IO;
using System.Threading;
using Excess.Compiler;
using Excess.Compiler.Core;

using CSharp = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Excess.RuntimeProject
{
    using Injector = ICompilerInjector<SyntaxToken, SyntaxNode, SemanticModel>;
    using DelegateInjector = DelegateInjector<SyntaxToken, SyntaxNode, SemanticModel>;
    using CompositeInjector = CompositeInjector<SyntaxToken, SyntaxNode, SemanticModel>;
    using Excess.Compiler.Roslyn;
    using System.Diagnostics;
    using Antlr4.Runtime;
    using System.CodeDom;

    class ExtensionRuntime : BaseRuntime, IExtensionRuntime
    {
        public ExtensionRuntime(IPersistentStorage storage) : base(storage) { }

        static ExtensionRuntime()
        {
            _tools[".g4"] = new AntlrTool();
        }

        private static Injector _references = new DelegateInjector(compiler =>
        {
            var assemblyPath = Path.GetDirectoryName(typeof(object).Assembly.Location);

            compiler
                .Environment()
                    .dependency<Injector>(new [] {
                        "Excess.Compiler",
                        "Excess.Compiler.Core",
                        "Excess.Compiler.Roslyn"
                    })
                    .dependency<System.Linq.Expressions.Expression>("System.Linq")
                    .dependency<SyntaxNode>("Microsoft.CodeAnalysis")
                    .dependency<CSharpSyntaxNode>(new[] {
                        "Microsoft.CodeAnalysis.CSharp",
                        "Microsoft.CodeAnalysis.CSharp.Syntax"})
                    .dependency(string.Empty, path: Path.Combine(assemblyPath, "System.Runtime.dll"))
                    .dependency(string.Empty, path: Path.Combine(assemblyPath, "System.Threading.Tasks.dll"))
                    .dependency<ParserRuleContext>("Antlr4.Runtime")
                    .dependency<CodeCompileUnit>("System.CodeDom");
        });

        private static Injector _extension = new DelegateInjector(compiler =>
        {
            compiler
                .Lexical()
                    .normalize()
                        .statements(MoveToApply);
        });

        static private CompilationUnitSyntax ExtensionClass = CSharp.ParseCompilationUnit(@"
            internal partial class Extension
            {
                public static void Apply(ICompiler<SyntaxToken, SyntaxNode, SemanticModel> compiler)
                {
                    var lexical = compiler.Lexical();
                    var syntax = compiler.Syntax();
                    var semantics = compiler.Semantics();
                    var environment = compiler.Environment();

                }
            }");

        private static SyntaxNode MoveToApply(SyntaxNode root, IEnumerable<SyntaxNode> statements, Scope scope)
        {
            return ExtensionClass
                .ReplaceNodes(ExtensionClass
                    .DescendantNodes()
                    .OfType<BlockSyntax>(),
                (on, nn) =>
                {
                    return nn.AddStatements(statements
                        .OfType<StatementSyntax>()
                        .ToArray());
                });
        }

        private static Injector _transform = new DelegateInjector(compiler =>
        {
            compiler
                .Lexical()
                    .normalize()
                        .members(MoveToClass);
        });

        static private CompilationUnitSyntax TransformClass = CSharp.ParseCompilationUnit(@"
            using CSharp = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
            internal partial class Extension
            {
            }");


        private static SyntaxNode MoveToClass(SyntaxNode root, IEnumerable<SyntaxNode> members, Scope scope)
        {
            return TransformClass.
                ReplaceNodes(TransformClass
                    .DescendantNodes()
                    .OfType<ClassDeclarationSyntax>(),
                    (on, nn) => nn.WithMembers(CSharp
                        .List(members
                        .Select(member => (MemberDeclarationSyntax)member))));
        }

        protected override ICompilerInjector<SyntaxToken, SyntaxNode, SemanticModel> getInjector(string file)
        {
            var xs = base.getInjector(file);
            if (file == "extension")
                return new CompositeInjector(new[] { _references, _extension, xs });

            if (file == "transform")
                return new CompositeInjector(new[] { _references, _transform, xs });

            return new CompositeInjector(new[] { _references, xs });
        }

        public string debugExtension(string text)
        {
            Debug.Assert(_compiler != null);

            string rText;
            var tree = _compiler.ApplySemanticalPass(text, out rText);

            return tree.GetRoot().NormalizeWhitespace().ToString();
        }

        static string[] _expressionTypes = {
            "ExpressionContext",
            "AssignmentExpressionContext",
            "UnaryExpressionContext",
            "LogicalAndExpressionContext",
            "AdditiveExpressionContext",
            "ConditionalExpressionContext",
            "LogicalOrExpressionContext",
            "InclusiveOrExpressionContext",
            "ExclusiveOrExpressionContext",
            "AndExpressionContext",
            "EqualityExpressionContext",
            "ConstantContext",
            "CastExpressionContext",
            "RelationalExpressionContext",
            "ShiftExpressionContext",
            "MultiplicativeExpressionContext",
            "MemberAccessContext",
            "ParenthesizedContext",
            "MemberPointerAccessContext",
            "SimpleExpressionContext",
            "IndexContext",
            "CallContext",
            "PostfixIncrementContext",
            "PostfixDecrementContext",
            "IdentifierContext",
            "StringLiteralContext",
            "ArgumentExpressionListContext", 
            "UnaryOperatorContext", 
            "AssignmentOperatorContext",
            "ConstantExpressionContext",
        };

        static string NodeTransformFunction = @"
private static SyntaxNode {0}({1} value, Func<ParserRuleContext, Scope, SyntaxNode> compile, Scope scope)
{{
    throw new NotImplementedException();
}}
";

        static string TransformFunction = @"
private static SyntaxNode Transform(SyntaxNode oldNode, SyntaxNode newNode, Scope scope, LexicalExtension<SyntaxToken> extension)
{
    return newNode;
}
";

        static string LexicalHeader = @"
lexical
    .grammar<{0}Grammar, ParserRuleContext>(""{0}"", ExtensionKind.Code)
";

        public bool generateGrammar(out string extension, out string transform)
        {
            extension = null;
            transform = null;

            var grammar = _compilation.getFileByExtension(".g4");
            if (grammar == null)
                return false;

            var grammarName = Path.GetFileNameWithoutExtension(grammar);
            var listenerName = grammarName + "BaseListener.cs";
            var listener = _compilation.getCSharpFile(listenerName);

            if (listener == null)
                return false;

            var methods = listener
                .GetRoot()
                .DescendantNodes()
                .OfType<MethodDeclarationSyntax>()
                .Where(method => method
                    .ParameterList
                    .Parameters
                    .Count == 1);

            var types = methods
                .Select(method => method
                    .ParameterList
                    .Parameters[0]
                    .Type
                    .ToString())
                .GroupBy(x => x)
                .Select(y => y.First()); 

            var parserName = grammarName + "Parser";

            StringBuilder extensionText = new StringBuilder();
            StringBuilder transformText = new StringBuilder();

            extensionText.AppendFormat(LexicalHeader, grammarName);

            //track default extenions
            StringBuilder expressionExtensionText = new StringBuilder();
            StringBuilder expressionTransformText = new StringBuilder();

            int expressionCount = 0;
            foreach (var type in types)
            {
                if (!type.Contains(parserName))
                    continue;

                var ext = extensionText;
                var xform = transformText;
                var context = type.Split('.')[1];
                if (_expressionTypes.Contains(context))
                {
                    ext = expressionExtensionText;
                    xform = expressionTransformText;

                    expressionCount++;
                }

                ext.AppendFormat("\t\t.transform<{0}>({1})\n", type, context);
                xform.AppendFormat(NodeTransformFunction, context, type);
            }

            if (expressionCount == _expressionTypes.Length)
                extensionText.AppendFormat("\t\t.transform<{0}.ExpressionContext>(AntlrExpression.Parse)\n", parserName);
            else
            {
                extensionText.Append(expressionExtensionText);
                transformText.Append(expressionTransformText);
            }

            extensionText.Append("\t\t.then(Transform);");
            transformText.Append(TransformFunction);

            extension = extensionText.ToString();
            transform = transformText.ToString();
            return true;
        }

        public override string defaultFile()
        {
            return "extension";
        }

        Assembly _assembly;

        private string keywordString(IEnumerable<string> keywords)
        {
            StringBuilder result = new StringBuilder();
            foreach (var k in keywords)
            {
                result.Append(" ");
                result.Append(k);
            }

            return result.Length > 0? result.ToString() : " ";
        }

        RoslynCompiler _compiler;
        protected override void doRun(Assembly asm, out dynamic client)
        {
            if (_assembly != asm)
            {
                _assembly = asm;

                Type type = _assembly.GetType("ExtensionPlugin");
                Injector result = (Injector)type.InvokeMember("Create", BindingFlags.Public | BindingFlags.InvokeMethod | BindingFlags.Static, null, null, null);
                if (result == null)
                    throw new InvalidOperationException("Corrupted extension");

                _compiler = new RoslynCompiler();
                result.apply(_compiler);
            }

            client = new {
                debuggerDlg = "/App/Main/dialogs/dslDebugger.html",
                debuggerCtrl = "dslDebuggerCtrl",
                debuggerData = new
                {
                    keywords = keywordString(_compiler.Environment().keywords())
                }
            };
        }

        public override IEnumerable<TreeNodeAction> fileActions(string file)
        {
            if (file == "extension")
                return new[] { new TreeNodeAction { id = "add-extension-item", icon = "fa-plus-circle" } };

            if (Path.GetExtension(file) == ".g4")
                return new[] { new TreeNodeAction { id = "generate-grammar", icon = "fa-flash" } };

            return base.fileActions(file);
        }
    }
}
