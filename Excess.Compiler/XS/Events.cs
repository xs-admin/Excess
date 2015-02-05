using Excess.Compiler.Roslyn;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using CSharp = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Excess.Compiler.XS
{
    class Events
    {
        static public void Apply(RoslynCompiler compiler)
        {
            var lexical = compiler.Lexical();
            var sintaxis = compiler.Sintaxis();

            lexical
                .match()
                    .token("event", named: "ev")
                    .identifier(named: "id")
                    .enclosed('(', ')', contents: "args")
                    .then(LexicalEventDeclaration);

            sintaxis
                .match<MethodDeclarationSyntax>(method => method.ReturnType.ToString() == "on")
                    .then(EventHandler);
        }

        private static SyntaxNode EventHandler(SyntaxNode node, Scope scope)
        {
            MethodDeclarationSyntax method = (MethodDeclarationSyntax)node;
            if (method.Modifiers.Any())
            {
                //td: error, no modifiers allowed 
                return node;
            }

            var args = method.ParameterList.ReplaceNodes(method.ParameterList.Parameters, (oldParam, newParam) =>
            {
                if (oldParam.Identifier.IsMissing)
                    return newParam
                        .WithType(RoslynCompiler.@void)
                        .WithIdentifier(CSharp.Identifier(newParam.Type.ToString()));

                return newParam;
            });

            var result = method
                .WithReturnType(RoslynCompiler.@void)
                .WithParameterList(args)
                .WithModifiers(RoslynCompiler.@private);

            var document = scope.GetDocument<SyntaxToken, SyntaxNode, SemanticModel>();
            return document.change(result, SemanticEventHandler);
        }

        private static SyntaxNode SemanticEventHandler(SyntaxNode node, SemanticModel model, Scope scope)
        {
            var mthd       = (MethodDeclarationSyntax)node;
            var methdArgs  = mthd.ParameterList;
            var methodName = "on_" + mthd.Identifier.ToString();

            ISymbol     self = model.GetDeclaredSymbol(mthd);
            ITypeSymbol type = (ITypeSymbol)self.ContainingSymbol;

            string evName = mthd.Identifier.ToString();
            string typeName = type.Name;
            bool found = false;
            while (type != null && !found)
            {
                foreach (var ev in type.GetMembers().OfType<IEventSymbol>())
                {
                    if (ev.Name.Equals(evName))
                    {
                        //arguments
                        foreach (var syntax in ev.Type.DeclaringSyntaxReferences)
                        {
                            var refNode = (DelegateDeclarationSyntax)syntax.GetSyntax();

                            int pCount = methdArgs.Parameters.Count;
                            int idx = 0;
                            bool match = true;
                            methdArgs = refNode.ParameterList.ReplaceNodes(refNode.ParameterList.Parameters, (oldArg, newArg) =>
                            {
                                if (match)
                                {
                                    if (idx >= pCount && match)
                                        return newArg;

                                    ParameterSyntax arg = methdArgs.Parameters[idx++];
                                    string argName = arg.Identifier.ToString();
                                    if (argName == oldArg.Identifier.ToString())
                                    {
                                        //coincident parameters, fix missing type or return same
                                        if (arg.Identifier.IsMissing || arg.Type.ToString() == "void")
                                            return newArg.WithIdentifier(arg.Identifier);

                                        return arg;
                                    }
                                    else
                                    {
                                        match = false;
                                        if (!refNode.ParameterList.Parameters.Any(p => p.Identifier.ToString().Equals(arg.Identifier.ToString())))
                                        {
                                            //name change?
                                            if (oldArg.Identifier.IsMissing)
                                                return newArg.WithIdentifier(CSharp.Identifier(arg.Type.ToString()));

                                            return arg;
                                        }
                                    }
                                }

                                return newArg;
                            });
                        }

                        //event initialization
                        var document = scope.GetDocument<SyntaxToken, SyntaxNode, SemanticModel>();
                        document.change(mthd.Parent, RoslynCompiler.AddInitializers(EventInitializer(ev.Name, methodName)));
                        found = true;
                        break;
                    }
                }

                type = type.BaseType;
            }

            if (!found)
            {
                //td: error, no such event
            }

            return mthd.WithIdentifier(CSharp.Identifier(methodName)).
                          WithParameterList(methdArgs);
        }

        private static StatementSyntax _EventInitializer = CSharp.ParseStatement("__1 += __2;");
        public static StatementSyntax EventInitializer(string addMethod, string implementor)
        {
            return _EventInitializer.ReplaceNodes(_EventInitializer.DescendantNodes().OfType<IdentifierNameSyntax>(), (oldNode, newNode) =>
            {
                var id = oldNode.Identifier.ToString() == "__1" ? addMethod : implementor;
                return newNode.WithIdentifier(CSharp.Identifier(id));
            });
        }

        private static IEnumerable<SyntaxToken> LexicalEventDeclaration(IEnumerable<SyntaxToken> tokens, Scope scope)
        {
            dynamic context = scope;

            SyntaxToken              keyword    = context.ev;
            SyntaxToken              identifier = context.id;
            IEnumerable<SyntaxToken> args       = context.args;

            var document = scope.GetDocument<SyntaxToken, SyntaxNode, SemanticModel>();

            yield return document.change(keyword, EventDeclaration(args));
            yield return CSharp.Identifier(" useless ");
            yield return identifier;
        }

        private static Func<SyntaxNode, Scope, SyntaxNode> EventDeclaration(IEnumerable<SyntaxToken> args)
        {
            return (node, scope) =>
            {
                EventFieldDeclarationSyntax @event  = (EventFieldDeclarationSyntax)node;
                ParameterListSyntax         @params = CSharp.ParseParameterList(RoslynCompiler.TokensToString(args));

                var variable = @event
                    .Declaration
                    .Variables[0];

                var delegateName = variable.Identifier.ToString() + "_delegate"; //td: unique ids
                var delegateDecl = CSharp.DelegateDeclaration(RoslynCompiler.@void, delegateName)
                    .WithParameterList(@params)
                    .WithModifiers(@event.Modifiers);

                //add the delegate
                var document = scope.GetDocument<SyntaxToken, SyntaxNode, SemanticModel>();
                document.change(@event.Parent, RoslynCompiler.AddMember(delegateDecl));

                return @event
                    .WithDeclaration(@event.Declaration
                        .WithType(CSharp.ParseTypeName(delegateName)));
            };
        }
    }
}
