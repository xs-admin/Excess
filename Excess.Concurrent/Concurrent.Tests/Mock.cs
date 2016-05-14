using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Excess.Concurrent.Runtime;
using Excess.Compiler;
using Excess.Compiler.Core;
using Excess.Compiler.Roslyn;
using Excess.Extensions.Concurrent;

namespace Concurrent.Tests
{
    using ExcessCompilation = Excess.Compiler.Roslyn.Compilation;
    using FactoryMap = Dictionary<string, Func<IConcurrentApp, object[], IConcurrentObject>>;
    using ConcurrentAttribute = Excess.Concurrent.Runtime.Concurrent;

    public static class Mock
    {
        public static SyntaxTree Compile(string code,
            out string text,
            bool withInterface = false,
            bool withRemote = false)
        {
            var config = MockInjector(new Options
            {
                GenerateInterface = withInterface,
                GenerateRemote = withRemote,
            });

            var compiler = new RoslynCompiler();
            config.apply(compiler);

            var tree = compiler.ApplySemanticalPass(code);
            text = tree.GetRoot().NormalizeWhitespace().ToFullString();
            return tree;
        }

        public static TestConcurrentApp Build(string code,
            out IEnumerable<Diagnostic> errors,
            bool withInterface = false,
            bool withRemote = false)
        {
            errors = null;

            var config = MockInjector(new Options
            {
                GenerateInterface = withInterface,
                GenerateRemote = withRemote,
            });
            var compilation = new ExcessCompilation();

            compilation.addDocument("concurrent-test", code, config);

            Assembly assembly = compilation.build();
            if (assembly == null)
            {
                errors = compilation.errors();

                //debug
                StringBuilder errorLines = new StringBuilder();
                foreach (var error in errors)
                {
                    errorLines.AppendLine(error.ToString());
                }

                var errorString = errorLines.ToString();
                return null;
            }

            var types = new FactoryMap();
            var result = new TestConcurrentApp(types);

            foreach (var type in assembly.GetTypes())
            {
                var attributes = type.CustomAttributes;
                if (!attributes.Any(attr => attr.AttributeType == typeof(ConcurrentAttribute)))
                    continue;

                var typeName = type.ToString();
                if (attributes.Any(attr => attr.AttributeType == typeof(ConcurrentSingleton)))
                {
                    result.AddSingleton(typeName, (IConcurrentObject)Activator.CreateInstance(type));
                    continue;
                }

                var useParameterLess = type.GetConstructors().Length == 0;
                if (!useParameterLess)
                    useParameterLess = type.GetConstructor(new Type[] { }) != null;

                types[typeName] = (app, args) =>
                {
                    if (useParameterLess)
                        return (IConcurrentObject)Activator.CreateInstance(type);

                    var ctor = type.GetConstructor(args
                        .Select(arg => arg.GetType())
                        .ToArray());

                    if (ctor != null)
                        return (IConcurrentObject)ctor.Invoke(args);

                    throw new InvalidOperationException("unable to find a constructor");
                };
            }

            return result;
        }

        private static ICompilerInjector<SyntaxToken, SyntaxNode, SemanticModel> MockInjector(Options options)
        {
            return new CompositeInjector<SyntaxToken, SyntaxNode, SemanticModel>(new[]
            {
                new DelegateInjector<SyntaxToken, SyntaxNode, SemanticModel>(compiler => compiler
                    .Environment()
                        .dependency(new[]
                        {
                            "System.Threading",
                            "System.Threading.Tasks",
                            "System.Diagnostics",
                        })
                        .dependency<ConcurrentObject>(new string[]
                        {
                            "Excess.Concurrent.Runtime"
                        })),

                new DelegateInjector<SyntaxToken, SyntaxNode, SemanticModel>(compiler =>
                    ConcurrentExtension.Apply((RoslynCompiler)compiler, options))
            });
        }

        private static void RunStep(IConcurrentObject @object, string methodName, Func<string> moveNext, Action<Exception> failure)
        {
            var method = @object.GetType().GetMethod(methodName, new[]
            {
                typeof(CancellationToken),
                typeof(Action<object>),
                typeof(Action<Exception>),
            });

            Action<object> success = (res) =>
            {
                try
                {
                    var next = moveNext();
                    if (next != null)
                        RunStep(@object, next, moveNext, failure);
                }
                catch (Exception ex)
                {
                    failure(ex);
                }
            };

            method.Invoke(@object, new object[] { default(CancellationToken), success, failure });
        }

        private static Exception RunSteps(IConcurrentObject @object, string[] steps)
        {
            int stepCount = steps.Length;
            int stepIdx = 1;

            Func<string> nextStep = () =>
            {
                if (stepIdx < stepCount)
                    return steps[stepIdx++];
                return null;
            };

            var ex = null as Exception;
            RunStep(@object, steps[0], nextStep, (__ex) => { ex = __ex; });
            while (ex == null && stepIdx < stepCount)
            {
                Thread.Sleep(100);
            }

            return ex;
        }

        public static void Succeeds(IConcurrentObject @object, params string[] steps)
        {
            var result = RunSteps(@object, steps);
            if (result != null)
                throw new InvalidOperationException("Expecting success");
        }

        public static void AssertFails(IConcurrentObject @object, params string[] steps)
        {
            var result = RunSteps(@object, steps);
            if (result == null)
                throw new InvalidOperationException("Expecting failure");
        }

        public static void Send(IConcurrentObject @object, string name, params object[] args)
        {
            Send(@object, name, true, args);
        }

        public static void SendAsync(IConcurrentObject @object, string name, params object[] args)
        {
            Send(@object, name, false, args);
        }

        private static void Send(IConcurrentObject @object, string name, bool wait, object[] args)
        {
            var method = @object
                .GetType()
                .GetMethods()
                .Where(m => m.Name == name
                         && m
                            .ReturnType
                            .ToString()
                            .Contains("Task"))
                .SingleOrDefault();

            if (method != null)
            {
                args = args.Union(new object[] { default(CancellationToken) }).ToArray();
                var tsk = method.Invoke(@object, args);

                if (wait)
                    ((Task)tsk).Wait();
            }
            else
            {
                var property = @object
                    .GetType()
                    .GetProperty(name);

                Debug.Assert(property != null && property.CanWrite);
                property.SetValue(@object, args[0]);
            }
        }
    }
}
