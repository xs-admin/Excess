using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Excess.Compiler;

namespace Excess.Server.Compiler
{
    public interface IServerConfiguration
    {
        void AddClientInterface(SyntaxTree document, string contents);
        void AddFunctionalContainer(string name, string body);
        string GetClientInterface();
        string GetServicePath();
    }

    public class ServerConfiguration : IServerConfiguration
    {
        ICompilerEnvironment _environment;
        public ServerConfiguration(ICompilerEnvironment environment)
        {
            _environment = environment;
        }

        StringBuilder _jsObject = new StringBuilder();
        public void AddClientInterface(SyntaxTree document, string contents)
        {
            _jsObject.AppendLine(contents);
        }

        public string GetClientInterface()
        {
            var services = _jsObject.ToString();
            if (_functions.Any())
            {
                var functionals = new StringBuilder();
                foreach (var function in _functions)
                {
                    functionals.AppendLine(Templates.jsService
                        .Render(new
                        {
                            Name = function.Key,
                            Body = function.Value.ToString(),
                            ID = Guid.NewGuid() //lol
                    }));
                }

                return services + functionals.ToString();
            }

            return services;
        }

        public string GetServicePath()
        {
            return _environment?.setting("servicePath") as string;
        }

        Dictionary<string, StringBuilder> _functions = new Dictionary<string, StringBuilder>();
        public void AddFunctionalContainer(string name, string body)
        {
            var builder = default(StringBuilder);
            if (!_functions.TryGetValue(name, out builder))
            {
                builder = new StringBuilder();
                _functions[name] = builder;
            }

            builder.AppendLine(body);
        }
    }

    public static class ScopeExtensions
    {
        public static IServerConfiguration GetServerConfiguration(this Scope scope)
        {
            var result = scope.get<IServerConfiguration>();
            if (result == null)
            {
                result = new ServerConfiguration(scope.get<ICompilerEnvironment>());
                scope.set(result);
            }

            return result;
        }
    }
}
