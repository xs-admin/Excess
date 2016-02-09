using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Excess.Compiler;
using Excess.Compiler.Roslyn;
using Microsoft.CodeAnalysis.CSharp;
using System.Diagnostics;

namespace Excess.Extensions.Concurrent
{
    using CSharp = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
    using Roslyn = RoslynCompiler;
    using ExcessCompiler = ICompiler<SyntaxToken, SyntaxNode, SemanticModel>;
    using Model;

    public class Extension
    {
        public static void Apply(ExcessCompiler compiler)
        {
            var lexical = compiler.Lexical();

            lexical
                .match()
                    .token("concurrent", named: "keyword")
                    .token("class", named: "ref")
                    .then(lexical.transform()
                        .remove("keyword")
                        .then(Compile))
                .match()
                    .token("concurrent", named: "keyword")
                    .token("object", named: "ref")
                    .then(lexical.transform()
                        .remove("keyword")
                        .replace("ref", "class")
                        .then(CompileObject))
                .match()
                    .token("->")
                    .token("(", named: "ref")
                    .then(lexical.transform()
                        .insert("__seq", before: "ref"));

            //var environment = compiler.Environment();
            //environment
            //    .keyword("concurrent")
            //    .global<ConcurrentNode>()
            //    .dependency<IConcurrentObject>(new[] {
            //        "Concurrent.Runtime",
            //        "Concurrent.THC",
            //        "System.Threading",
            //        "System.Threading.Tasks",
            //    })
            //    .dependency<THC.ConcurrentObject>(new[] {
            //        "Concurrent.THC",
            //    });
        }

        private static SyntaxNode Compile(SyntaxNode node, Scope scope)
        {
            var document = scope.GetDocument();

            Debug.Assert(node is ClassDeclarationSyntax);
            var @class = node as ClassDeclarationSyntax;
            var className = @class.Identifier.ToString();

            var myScope = new Scope(scope);

            var ctx = new ClassModel(className, myScope);
            myScope.set<ClassModel>(ctx);

            foreach (var member in @class.Members)
            {
                var isPublic = Roslyn.IsVisible(member);
                if (member is PropertyDeclarationSyntax)
                {
                    if (compileProperty(member as PropertyDeclarationSyntax, ctx, scope))
                        ctx.RemoveMember(member);
                }
                else if (member is MethodDeclarationSyntax)
                {
                    if (compileMethod(member as MethodDeclarationSyntax, ctx, scope))
                        ctx.RemoveMember(member);
                }
                else if (member is ConstructorDeclarationSyntax)
                    scope.AddError("concurrent01", "concurrent classes are not allowed to have constructors", node);
                else if (isPublic)
                    scope.AddError("concurrent02", "concurrent classes only allow public properties and methods", node);
            }

            if (ctx.Main == null)
            {
                compileMethod(Templates.EmptyMain.Get<MethodDeclarationSyntax>(), ctx, scope);
            }

            @class = ctx.Update(@class);
            return document.change(@class, Link(ctx), null);
        }

        private static Func<SyntaxNode, SyntaxNode, SemanticModel, Scope, SyntaxNode> Link(ClassModel ctx)
        {
            return (oldNode, newNode, model, scope) =>
            {
                Debug.Assert(newNode is ClassDeclarationSyntax);
                var @class = (ClassDeclarationSyntax)newNode;

                throw new NotImplementedException();
            };
        }

        private static bool compileProperty(PropertyDeclarationSyntax property, ClassModel ctx, Scope scope)
        {
            if (!Roslyn.IsVisible(property))
                return false;

            var @get = null as AccessorDeclarationSyntax;
            var @set = null as AccessorDeclarationSyntax;
            foreach (var accesor in property.AccessorList.Accessors)
            {
                switch (accesor.Keyword.Kind())
                {
                    case SyntaxKind.GetKeyword:
                        @get = accesor;
                        break;
                    case SyntaxKind.SetKeyword:
                        @set = accesor;
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }

            if (@get != null || @set != null)
            {
                scope.AddError("concurrent00", "invalid concurrent property", property);
                return false;
            }

            ctx.RemoveMember(property);

            if (@get != null)
                addGetter(ctx, property, @get);

            if (@set != null)
                addSetter(ctx, property, @set);

            return true;
        }

        private static bool compileMethod(MethodDeclarationSyntax method, ClassModel ctx, Scope scope)
        {
            var name = method.Identifier.ToString();
            var isMain = name == "main";

            var isVisible = Roslyn.IsVisible(method);
            var hasReturnType = method.ReturnType.ToString() == "void";
            var returnType = hasReturnType ?
                Roslyn.boolean : method.ReturnType;

            var emptySignal = method.Body == null || method.Body.IsMissing;
            if (emptySignal)
            {
                if (method.ParameterList.Parameters.Count > 0)
                    scope.AddError("concurrent03", "empty signals cannot contain parameters", method);

                if (method.ReturnType.ToString() != "void")
                    scope.AddError("concurrent04", "empty signals cannot return values", method);

                method = method
                    .WithSemicolonToken(CSharp.MissingToken(SyntaxKind.SemicolonToken))
                    .WithBody(CSharp.Block());
            }

            var cc = parseConcurrentBlock(ctx, method.Body);
            if (cc != null)
                method = method.WithBody(cc);

            //remove attributes, until needed
            method = method.WithAttributeLists(CSharp.List<AttributeListSyntax>());

            if (isMain)
            {
                if (ctx.Main != null)
                {
                    scope.AddError("concurrent06", "multiple main methods", method);
                    return false;
                }

                ctx.Main = ctx.AddSignal(name, true);

                var statements = method.Body.Statements;
                var isContinued = (cc == null) && checkContinued(statements);
                if (isContinued)
                {
                    ctx.AcceptPublicSignals = true;

                    method = method
                        .WithBody(CSharp.Block(statements
                            .Take(statements.Count - 1)));
                }

                concurrentMethod(ctx, method);
                ctx.Main.OnGo(
                    incomingSignal(isContinued 
                        ? Templates.MainContinuated 
                        : Templates.MainContinuation));

                return true;
            }

            if (isVisible)
            {
                var signal = ctx.AddSignal(name, returnType, isVisible);
                createPublicSignals(ctx, method, signal);

                concurrentMethod(ctx, method);
                signal.OnGo(incomingSignal(Templates.SignalContinuation));
            }
            else if (cc != null)
            {
                concurrentMethod(ctx, method);
                ctx.AddMember(method
                    .WithBody(Templates.PrivateSignal.Get<BlockSyntax>()));
            }
            else if (emptySignal)
            {
                var signal = ctx.AddSignal(name, returnType, isVisible);
                ctx.AddMember(Templates.EmptyPrivateMethod.Get<MethodDeclarationSyntax>(name, signal.Id));
            }
            else
                return false;

            return true;
        }

        private static BlockSyntax parseConcurrentBlock(ClassModel ctx, BlockSyntax body)
        {
            throw new NotImplementedException("body rewriter");
        }

        private static void concurrentMethod(ClassModel ctx, MethodDeclarationSyntax method, bool forever = false)
        {
            throw new NotImplementedException("Method rewriter");
        }

        private static bool checkContinued(IEnumerable<StatementSyntax> statements)
        {
            var statement = statements.LastOrDefault();
            return (statement != null && statement is ContinueStatementSyntax);
        }

        private static void addGetter(ClassModel ctx, PropertyDeclarationSyntax property, AccessorDeclarationSyntax get)
        {
            throw new NotImplementedException();
        }

        private static void addSetter(ClassModel ctx, PropertyDeclarationSyntax property, AccessorDeclarationSyntax set)
        {
            throw new NotImplementedException();
        }

        private static IEnumerable<StatementSyntax> incomingSignal(ExpressionSyntax continuation)
        {
            throw new NotImplementedException();
        }

        private static void createPublicSignals(ClassModel ctx, MethodDeclarationSyntax method, SignalModel signal)
        {
            throw new NotImplementedException();
        }

        private static SyntaxNode CompileObject(SyntaxNode node, Scope scope)
        {
            throw new NotImplementedException();
        }
    }
}
