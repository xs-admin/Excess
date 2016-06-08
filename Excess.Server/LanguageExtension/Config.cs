using System.Text;
using Microsoft.CodeAnalysis;
using Excess.Compiler;

namespace LanguageExtension
{
    public interface IServerConfiguration
    {
        void AddClientInterface(SyntaxTree document, string contents);
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
            return _jsObject.ToString();
        }

        public string GetServicePath()
        {
            return _environment?.setting("servicePath") as string;
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
