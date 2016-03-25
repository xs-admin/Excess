using System.Text;
using Microsoft.CodeAnalysis;
using Excess.Compiler;

namespace LanguageExtension
{
    public interface IServerConfiguration
    {
        void AddClientInterface(SyntaxTree document, string contents);
        string GetClientInterface();
    }

    public class ServerConfiguration : IServerConfiguration
    {
        StringBuilder _jsObject = new StringBuilder();
        public void AddClientInterface(SyntaxTree document, string contents)
        {
            _jsObject.AppendLine(contents);
        }

        public string GetClientInterface()
        {
            return _jsObject.ToString();
        }
    }

    public static class ScopeExtensions
    {
        public static IServerConfiguration GetServerConfiguration(this Scope scope)
        {
            var result = scope.get<IServerConfiguration>();
            if (result == null)
            {
                result = new ServerConfiguration();
                scope.set(result);
            }

            return result;
        }
    }
}
