using Excess.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Excess.RuntimeProject
{
    class ConsoleRuntime : BaseRuntime
    {
        public ConsoleRuntime(IDSLFactory factory) :
            base(factory)
        {
        }

        public override string defaultFile()
        {
            return "application";
        }

        protected override IEnumerable<MetadataReference> compilationReferences()
        {
            return null;
        }

        protected override IEnumerable<SyntaxTree> compilationFiles()
        {
            return null;
        }

        private static IEnumerable<SyntaxNode> CompleteTree(ExcessContext.CompleteInfo info)
        {
            ExcessContext.CompleteInfo newInfo = info.Clone();
            newInfo.DefaultClass = "application";

            if (info.Statements != null && info.Statements.Any())
            {
                var mainMethod = SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName("void"), "main")
                                    .WithBody(SyntaxFactory.Block()
                                        .WithStatements(SyntaxFactory.List(info.Statements)));

                newInfo.Members = newInfo.Members.Union(new[] { mainMethod });
            }

            return info.Context.TriggerDefaultComplete(newInfo);
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

            var main = appType.GetMethod("main", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (main == null)
            {
                notify(NotificationKind.Error, "Main method missing");
                return;
            }

            var instance = FormatterServices.GetUninitializedObject(appType);// Activator.CreateInstance(appType);
            main.Invoke(instance, new object[] { });
        }
    }
}
