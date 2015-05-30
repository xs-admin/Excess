using Microsoft.CodeAnalysis;
using RazorEngine;
using RazorEngine.Templating;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess.Compiler.Razor
{
    using CSharp = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

    internal enum TemplateKind
    {
        Expression,
        Statement,
        Code,
        Text
    }

    public class RazorTemplate
    {
        public static RazorTemplate ParseExpression(string text)
        {
            return new RazorTemplate(TemplateKind.Expression, ParseRazor(text));
        }

        public static RazorTemplate ParseExpression<T>(string text)
        {
            return new RazorTemplate(TemplateKind.Expression, ParseRazor(text, typeof(T)));
        }

        public static RazorTemplate ParseStatement(string text)
        {
            return new RazorTemplate(TemplateKind.Statement, ParseRazor(text));
        }

        public static RazorTemplate ParseStatement<T>(string text)
        {
            return new RazorTemplate(TemplateKind.Statement, ParseRazor(text, typeof(T)));
        }

        public static RazorTemplate ParseStatements(string text)
        {
            return new RazorTemplate(TemplateKind.Statement, ParseRazor("{" + text + "}"));
        }

        public static RazorTemplate ParseStatements<T>(string text)
        {
            return new RazorTemplate(TemplateKind.Statement, ParseRazor("{" + text + "}", typeof(T)));
        }

        public static RazorTemplate Parse(string text)
        {
            return new RazorTemplate(TemplateKind.Code, ParseRazor(text));
        }

        public static RazorTemplate Parse<T>(string text)
        {
            return new RazorTemplate(TemplateKind.Code, ParseRazor(text, typeof(T)));
        }

        public static RazorTemplate ParseText(string text)
        {
            return new RazorTemplate(TemplateKind.Text, ParseRazor(text));
        }

        public static RazorTemplate ParseText<T>(string text)
        {
            return new RazorTemplate(TemplateKind.Text, ParseRazor(text, typeof(T)));
        }

        private static string ParseRazor(string text, Type modelType = null)
        {
            var result = Guid.NewGuid().ToString();
            Engine.Razor.Compile(text, result, modelType);
            return result;
        }

        TemplateKind _kind;
        string _key;
        internal RazorTemplate(TemplateKind kind, string key)
        {
            _kind = kind;
            _key = key;
        }

        public string Render(object model)
        {
            return Engine.Razor.Run(_key, null, (object)model);
        }

        public SyntaxNode Get(object model)
        {
            if (_kind == TemplateKind.Text)
                throw new InvalidOperationException("asking syntax nodes from text");

            var text = Render(model);
            switch (_kind)
            {
                case TemplateKind.Code: return CSharp.ParseCompilationUnit(text);
                case TemplateKind.Expression: return CSharp.ParseExpression(text);
                case TemplateKind.Statement: return CSharp.ParseStatement(text);
            }

            throw new NotImplementedException();
        }

        public T Get<T>(object model) where T : SyntaxNode
        {
            var result = Get(model);
            return result
                .DescendantNodesAndSelf()
                .OfType<T>()
                .First();
        }
    }
}
