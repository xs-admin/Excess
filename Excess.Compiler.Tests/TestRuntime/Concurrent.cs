using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Excess.Extensions.Concurrent;

namespace Excess.Compiler.Tests.TestRuntime
{
    using Core;
    using Microsoft.CodeAnalysis;
    using System.Diagnostics;
    using System.Reflection;
    using Spawner = Func<object[], ConcurrentObject>;

    public class Concurrent
    {
        private static void RunStep(ConcurrentObject @object, string methodName, Func<string> moveNext, Action<Exception> failure)
        {
            var method = @object.GetType().GetMethod(methodName, new[] 
            {
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

            method.Invoke(@object, new object[] { success, failure });
        }

        private static Exception RunSteps(ConcurrentObject @object, string[] steps)
        {
            int stepCount = steps.Length;
            int stepIdx = 0;

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

        public static void Succeeds(ConcurrentObject @object, params string[] steps)
        {
            var result = RunSteps(@object, steps);
            if (result != null)
                throw new InvalidOperationException("Expecting success");
        }

        public static void Fails(ConcurrentObject @object, params string[] steps)
        {
            var result = RunSteps(@object, steps);
            if (result == null)
                throw new InvalidOperationException("Expecting failure");
        }

        public static Node Build(string text, out IEnumerable<Diagnostic> errors, int threads = 1)
        {
            errors = null;

            var compilation = new Roslyn.Compilation(null, null);
            var injector = new CompositeInjector<SyntaxToken, SyntaxNode, SemanticModel>(new[]
            {
                new DelegateInjector<SyntaxToken, SyntaxNode, SemanticModel>(compiler => compiler
                    .Environment()
                        .dependency<console>("Excess.Compiler.Tests.TestRuntime")
                        //.dependency<object>(new[] {
                        //    "System",
                        //    "System.Collections",
                        //    "System.Collections.Generic" })
                        .dependency(new[] {
                            "System.Threading",
                            "System.Threading.Tasks",
                            "System.Diagnostics",
                        })),

                new DelegateInjector<SyntaxToken, SyntaxNode, SemanticModel>(compiler =>
                    Extensions
                    .Concurrent.Extension
                        .Apply(compiler))
            });

            compilation.addDocument("concurrent-test", text, injector);

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

            var exportTypes = new Dictionary<string, Spawner>();
            foreach (var type in assembly.GetTypes())
            {
                if (type.BaseType != typeof(ConcurrentObject) ||
                    type
                        .GetMethods()
                        .Where(method => method.Name == "__singleton")
                        .Any())
                    continue;

                var useParameterLess = type.GetConstructors().Length == 0;
                if (!useParameterLess)
                    useParameterLess = type.GetConstructor(new Type[] { }) != null;

                var typeName = type.ToString();
                exportTypes[typeName] = (args) =>
                {
                    if (useParameterLess)
                        return (ConcurrentObject)Activator.CreateInstance(type);

                    var ctor = type.GetConstructor(args
                        .Select(arg => arg.GetType())
                        .ToArray());

                    if (ctor != null)
                        return (ConcurrentObject)ctor.Invoke(args);

                    throw new InvalidOperationException("unable to find a constructor");
                };
            }

            return new Node(threads, exportTypes);
        }

        public static void Send(ConcurrentObject @object, string name, params object[] args)
        {
            Send(@object, name, true, args);
        }

        public static void SendAsync(ConcurrentObject @object, string name, params object[] args)
        {
            Send(@object, name, false, args);
        }

        private static void Send(ConcurrentObject @object, string name, bool wait, object[] args)
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
                args = args.Union(new object[] { true }).ToArray();
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
