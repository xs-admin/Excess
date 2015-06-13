using Excess.Compiler;
using Excess.Compiler.Core;
using Concurrent.Compiler;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Excess.RuntimeProject
{
    using CSharp = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
    using Injector = ICompilerInjector<SyntaxToken, SyntaxNode, SemanticModel>;
    using DelegateInjector = DelegateInjector<SyntaxToken, SyntaxNode, SemanticModel>;
    using CompositeInjector = CompositeInjector<SyntaxToken, SyntaxNode, SemanticModel>;
    using Excess.Entensions.XS;

    class ConcurrentRuntime : ConsoleRuntime
    {
        public ConcurrentRuntime(IPersistentStorage storage) : base(storage) { }

        private static Injector _concurrent = new DelegateInjector(compiler => ConcurrentXS.Apply(compiler));

        private static Injector _main = new DelegateInjector(compiler =>
        {
            compiler
                .Lexical()
                    .normalize()
                    .with(statements: MoveToRun);

            compiler
                .Environment()
                    .dependency<System.Linq.Expressions.Expression>("System.Linq");
        });

        static CompilationUnitSyntax _app = CSharp.ParseCompilationUnit(@"
            public class application
            {
                Node _node;
                public void main()
                {
                    _node = new Node(3);
                    _node.StopOnEmpty();

                    run();

                    var tokenSource = new CancellationTokenSource();
                    try
                    {
                        CancellationToken token = tokenSource.Token;
                        Task.Delay(30000, token)
                            .ContinueWith((t) => _node.stop());

                        _node.waitForCompletion();
                        tokenSource.Cancel();
                    }
                    catch (OperationCanceledException)
                    {
                    }    
                }

                private void run()
                {
                }

                private void start(IConcurrentObject obj, params object[] args)
                {
                    _node.run(obj as ConcurrentObject, args);
                }

                private T start<T>(IConcurrentObject obj, params object[] args)
                {
                    return _node.run<T>(obj as ConcurrentObject, args);
                }

                private T spawn<T>() where T : IConcurrentObject, new ()
                {
                    return _node.spawn<T>();
                }

                private IEnumerable<T> spawn<T>(int count)  where T : IConcurrentObject, new ()
                {
                    return _node.spawnMany<T>(count);
                }
            }");

        private static SyntaxNode MoveToRun(SyntaxNode root, IEnumerable<SyntaxNode> statements, Scope scope)
        {
            return _app
                .ReplaceNodes(_app
                    .DescendantNodes()
                    .OfType<MethodDeclarationSyntax>()
                    .Where(m => m.Identifier.ToString() == "run"),
                    (on, nn) => nn.WithBody(CSharp.Block(
                        statements.Select(sn => (StatementSyntax)sn))));
        }

        protected override ICompilerInjector<SyntaxToken, SyntaxNode, SemanticModel> getInjector(string file)
        {
            var xs = XSLang.Create();
            if (file == "application")
                return new CompositeInjector(new[] { _main, _concurrent, xs });

            return new CompositeInjector(new[] { _concurrent, xs });
        }
    }
}
