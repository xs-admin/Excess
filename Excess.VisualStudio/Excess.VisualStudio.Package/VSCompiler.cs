﻿using Excess.Compiler;
using Excess.Compiler.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using xslang;

namespace Excess.VisualStudio.VSPackage
{
    using CSharp = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
    using CompilerFunc = Action<RoslynCompiler, Scope>;
    using CompilerExtensions = Dictionary<string, Action<RoslynCompiler, Scope>>;

    public class VSCompiler
    {
        public static Task<RoslynDocument> Parse(string text, Scope scope)
        {
            return Task.Run(() => 
            {
                var result = CreateExcessDocument(text, scope);
                result.applyChanges(CompilerStage.Syntactical);
                return result;
            });
        }

        private static RoslynDocument CreateExcessDocument(string text, Scope scope)
        {
            //td: we need the using list in order to deduct the extensions
            //however, we don't need to parse the whole document.
            //We must optimize this (maybe a custom using parser?)
            var compilationUnit = CSharp.ParseCompilationUnit(text);
            var extensions = new List<UsingDirectiveSyntax>(compilationUnit.Usings);
            var keywords = null as IEnumerable<string>;
            var compiler = CreateCompiler(extensions, out keywords, scope);

            //build a new document
            var docScope = new Scope(scope);
            var result = new RoslynDocument(docScope, text);
            result.Mapper = new MappingService();
            compiler.apply(result);
            return result;
        }

        private static bool isExtension(UsingDirectiveSyntax @using) => @using.Name.ToString().StartsWith("xs.");

        public static RoslynCompiler CreateCompiler(ICollection<UsingDirectiveSyntax> extensions, out IEnumerable<string> keywords, Scope scope)
        {
            keywords = null;
            var result = (RoslynCompiler)XSLanguage.CreateCompiler();
            var compilerExtensions = scope.get<CompilerExtensions>();
            var keywordList = new List<string>();
            var props = new Scope(scope);
            props.set("keywords", keywordList);

            foreach (var extension in extensions.ToArray())
            {
                if (isExtension(extension))
                {
                    var extensionName = extension
                        .Name
                        .ToString()
                        .Substring("xs.".Length);

                    var compilerFunc = null as CompilerFunc;
                    if (compilerExtensions.TryGetValue(extensionName, out compilerFunc))
                    {
                        try
                        {
                            compilerFunc(result, props);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex.Message);
                        }
                        continue;
                    }
                }

                extensions.Remove(extension);
            }

            keywords = keywordList;
            return result;
        }
    }
}
