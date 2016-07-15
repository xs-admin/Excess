using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Excess.Compiler;
using Excess.Compiler.Roslyn;
using Excess.Compiler.Attributes;
using Settings.Model;

namespace Settings
{
    using ExcessCompiler = ICompiler<SyntaxToken, SyntaxNode, SemanticModel>;
    using CSharp = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
    using System.Configuration;
    [Extension("settings")]
    public class SettingExtension
    {
        public static void Apply(ExcessCompiler compiler, Scope scope = null)
        {
            scope?.AddKeywords("settings");

            compiler.Lexical()
                .indented<SettingsModel, SettingsRoot>("settings", ExtensionKind.Type, null)
                    .match<SettingsRoot, HeaderModel>(ParseHeader,
                        children: child => child
                            .match<SettingsModel, HeaderModel, AssignmentExpressionSyntax>(
                                then: (header, assignment) => header.Values.Add(assignment)))

                    .then()
                        .transform<SettingsRoot>(LinkSettings);
        }

        private static HeaderModel ParseHeader(string value, SettingsRoot root, Scope scope)
        {
            var trimmed = value.Trim();
            if (trimmed.StartsWith("[") && trimmed.EndsWith("]"))
            {
                var sectionName = trimmed.Substring(1, trimmed.Length - 2);
                var result = new HeaderModel { Name = sectionName };
                root.Headers.Add(result);
                return result;
            }

            //td: error
            return null;
        }

        //templates
        static Template SettingClass = Template.Parse(@"
            [AutoInit]            
            class _0
            {
                public static void __init()
                {
        
                }
            }");

        static Template AddSetting = Template.ParseStatement("ConfigurationManager.AppSettings[__0] = (__1).ToString();");
        private static SyntaxNode LinkSettings(SettingsRoot root, Func<SettingsModel, Scope, SyntaxNode> parse, Scope scope)
        {
            var calls = new List<StatementSyntax>();
            foreach (var header in root.Headers)
            {
                foreach (var value in header.Values)
                {
                    if (value.Left.GetType() != typeof(IdentifierNameSyntax))
                    {
                        //td: error
                        continue;
                    }

                    var valueId = value.Left.ToString();
                    calls.Add(AddSetting.Get<StatementSyntax>(
                        RoslynCompiler.Quoted(valueId),
                        value.Right));
                }
            }

            var result = SettingClass.Get<ClassDeclarationSyntax>("__settings");
            return result
                .ReplaceNodes(result
                    .DescendantNodes()
                    .OfType<MethodDeclarationSyntax>(),
                    (on, nn) => nn
                        .AddBodyStatements(calls.ToArray()));
        }

    }
}
