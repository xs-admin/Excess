using Excess.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Excess.Project
{
    class ConsoleRuntime : BaseRuntime
    {
        public ConsoleRuntime(IDSLFactory factory, Dictionary<string, string> files) :
            base(factory, files)
        {
        }

        protected override IEnumerable<MetadataReference> references()
        {
            return null;
        }

        private static IEnumerable<SyntaxNode> CompleteTree(ExcessContext.CompleteInfo info)
        {
            if (info.Statements != null)
            {
                yield return SyntaxFactory.ClassDeclaration("application")
                                .WithMembers(SyntaxFactory.List(new MemberDeclarationSyntax[] {
                                        SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName("void"), "main")
                                            .WithBody(SyntaxFactory.Block()
                                                .WithStatements(SyntaxFactory.List(info.Statements)))
                                }));
            }
        }

        protected override void prepareContext(string fileName)
        {
            if (fileName == "application")
            {
                _ctx.SetOnComplete(CompleteTree);
            }
        }

        protected override void doRun(Assembly asm)
        {
            Type appType = asm.GetType("application");
            if (appType == null)
            {
                notify(NotificationKind.Error, "Application class missing");
                return;
            }

            var main = appType.GetMethod("main", BindingFlags.Instance);
            if (main == null)
            {
                notify(NotificationKind.Error, "Main method missing");
                return;
            }

            var instance = asm.CreateInstance("application");
            main.Invoke(instance, new object[] { });
        }
    }
}
