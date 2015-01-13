using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Excess.Core
{
    public class Compiler : CSharpSyntaxRewriter
    {
        public Compiler(ExcessContext ctx)
        {
            ctx_ = ctx;
        }

        public SyntaxTree compile(string code)
        {
            code = Regex.Replace(code, @"\bfunction\b\s*(\([^\)]*\))\s*{", "$1 => {");

            SyntaxTree tree = CSharpSyntaxTree.ParseText(code);

            tree = AnalyzeSyntax(tree, code);

            var result = Visit(tree.GetRoot());
            return result.SyntaxTree;
        }

        private SyntaxTree AnalyzeSyntax(SyntaxTree tree, string code)
        {
            var root = tree.GetRoot();
            var codeErrors = root.GetDiagnostics().Where(error => error.Id == "CS1022").
                                  OrderBy(error => error.Location.SourceSpan.Start).GetEnumerator();

            Diagnostic currError    = null;
            int        currErrorPos = 0;

            if (codeErrors != null && codeErrors.MoveNext())
                currError = codeErrors.Current;

            List<StatementSyntax>         statements = new List<StatementSyntax>();
            List<MemberDeclarationSyntax> members    = new List<MemberDeclarationSyntax>();
            List<MemberDeclarationSyntax> types     = new List<MemberDeclarationSyntax>();
            foreach (var child in root.ChildNodes())
            {
                if (child is IncompleteMemberSyntax)
                    continue;

                if (child is FieldDeclarationSyntax)
                {
                    //case: code variable?
                    FieldDeclarationSyntax field = (FieldDeclarationSyntax)child;
                    //td: !!! variable initialization
                    continue;
                }

                if (child is MethodDeclarationSyntax)
                {
                    //case: bad method?
                    MethodDeclarationSyntax method = (MethodDeclarationSyntax)child;
                    if (method.Body == null)
                        continue;
                }

                if (child is MemberDeclarationSyntax)
                {
                    bool foundError = false;
                    if (currError != null)
                    {
                        if (child.SpanStart > currError.Location.SourceSpan.Start)
                        {
                            SourceText errorSource = tree.GetText().GetSubText(new TextSpan(currErrorPos, child.SpanStart - currErrorPos));
                            parseStatements(errorSource.ToString(), currErrorPos, statements);

                            foundError = true;
                            currError = null;
                            while (codeErrors.MoveNext())
                            {
                                var nextError = codeErrors.Current;
                                if (nextError.Location.SourceSpan.Start > child.Span.End)
                                {
                                    currError = nextError;
                                    break;
                                }
                            }
                        }
                    }

                    currErrorPos = child.Span.End;
                    var toAdd    = child as MemberDeclarationSyntax;
                    
                    if (foundError)
                    {
                        toAdd = toAdd.ReplaceTrivia(child.GetLeadingTrivia(), (oldTrivia, newTrivia) =>
                        {
                            return SyntaxFactory.SyntaxTrivia(SyntaxKind.WhitespaceTrivia, string.Empty);
                        });
                    }

                    if (toAdd is TypeDeclarationSyntax || toAdd is EnumDeclarationSyntax)
                        types.Add(toAdd);
                    else if (!(toAdd is NamespaceDeclarationSyntax))
                        members.Add(toAdd);
                }
                else
                {
                    //any other top level construct indicates completeness
                    return tree;
                }
            }

            if (currError != null)
            {
                SourceText errorSource = tree.GetText().GetSubText(new TextSpan(currErrorPos, tree.GetRoot().FullSpan.End - currErrorPos));
                parseStatements(errorSource.ToString(), currErrorPos, statements);
            }

            bool hasCode    = statements.Count > 0;
            bool hasMembers = members.Count > 0;
            if (!hasCode && !hasMembers)
            {
                return tree; //nothing to se here
            }

            var complete = ctx_.Complete(statements, members, types);
            return complete != null? complete : tree;

            //var container = SyntaxFactory.ClassDeclaration("application");
            //if (hasCode)
            //{
            //    hasMembers = true;
            //    members.Add(SyntaxFactory.MethodDeclaration(Void, "main").
            //                              WithBody(SyntaxFactory.Block().
            //                                WithStatements(SyntaxFactory.List(statements))));
            //}

            //if (hasMembers)
            //    container = container.WithMembers(SyntaxFactory.List(members));

            //if (types.Count > 0)
            //{
            //    types.Insert(0, container);
            //    return SyntaxFactory.CompilationUnit().
            //                         WithMembers(SyntaxFactory.List(types)).SyntaxTree;

            //}

            //return container.SyntaxTree;
        }

        private void parseStatements(string source, int offset, List<StatementSyntax> statements)
        {
            BlockSyntax allStatements = (BlockSyntax)SyntaxFactory.ParseStatement("{" + source + "}");
            statements.AddRange(allStatements.Statements);
        }

        public override SyntaxNode Visit(SyntaxNode node)
        {
            if (pending_ != null)
            {
                ITreeLookAhead curr = pending_;
                pending_ = null;

                LookAheadAction action;
                SyntaxNode newNode = curr.rewrite(this, node, out action);
                switch (action)
                {
                    case LookAheadAction.SUCCEDED: return newNode;
                    case LookAheadAction.CONTINUE:
                    {
                        pending_ = curr;
                        return newNode;
                    }
                }
            }

            return base.Visit(node);
        }

        public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            ClassDeclarationSyntax result = (ClassDeclarationSyntax)base.VisitClassDeclaration(node);
            if (members_.Any())
            {
                result = result.AddMembers(SyntaxFactory.List(members_).ToArray());
                members_.Clear();
            }

            return result;
        }

        public override SyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            //handle language constructs
            string typeName = node.ReturnType.ToString();
            switch (typeName)
            {
                case "on":       return rewriteEventHandler(node);
                case "function": return rewriteFunction(node, false);
                case "method":   return rewriteFunction(node, true);
                case "typedef":  return rewriteTypedef(node);
                case "":
                {
                    switch (node.Identifier.ToString())
                    {
                        case "constructor": return rewriteConstructor(node);
                    }
                    break;
                }
            }

            //handle dsls
            IDSLHandler dsl = null;
            DSLSurroundings ds = node.Parent is CompilationUnitSyntax ? DSLSurroundings.Global : DSLSurroundings.TypeBody;
            string id = null;
            if (!node.ReturnType.IsMissing)
            {
                dsl = ctx_.CreateDSL(typeName);
                id = node.Identifier.ToString();
            }
            else
                dsl = ctx_.CreateDSL(node.Identifier.ToString());

            if (dsl != null)
            {
                DSLContext dctx = new DSLContext { MainNode = node, Surroundings = ds, Id = id, ExtraMembers = members_ };
                return dsl.compile(ctx_, dctx);
            }

            return node.WithBody((BlockSyntax)base.Visit(node.Body));
        }

        public override SyntaxNode VisitFieldDeclaration(FieldDeclarationSyntax node)
        {
            node = node.WithDeclaration((VariableDeclarationSyntax)VisitVariableDeclaration(node.Declaration));
            if (node.Declaration.Variables.Count == 1)
            {
                string typeName = node.Declaration.Type.ToString();
                switch (typeName)
                {
                    case "typedef":
                    {
                        Debug.Assert(pending_ == null);
                        pending_ = new ResolveTypedef(node, ctx_);
                        return null;
                    }

                    case "property":
                    {
                        var variable    = node.Declaration.Variables[0];
                        var initializer = variable.Initializer;
                        if (initializer != null)
                        {
                            var propType = Compiler.ConstantType(initializer.Value);
                            var result   = Compiler.Property(propType, variable.Identifier);
                            return result;
                        }
                        return node;
                    }
                }

                string variableName = node.Declaration.Variables[0].Identifier.ToString();
                switch (variableName)
                {
                    case "function":
                    case "method":
                    {
                        //when functions are declared with types the parser generates 
                        //a subsquent erroneous method declaration 
                        Debug.Assert(pending_ == null);
                        pending_ = new ResolveTypedFunction(node, ctx_, variableName == "method");
                        return null;
                    }
                    case "property":
                    {
                        //when functions are declared with types the parser generates 
                        //a subsquent erroneous method declaration 
                        Debug.Assert(pending_ == null);

                        var result = Compiler.Property(node.Declaration.Type);
                        pending_ = new ResolveProperty(result);
                        return null;
                    }
                }

            }
            return node;
        }

        public static TypeSyntax ConstantType(ExpressionSyntax value)
        {
            var valueStr = value.ToString();
            switch (value.CSharpKind())
            {
                case SyntaxKind.NumericLiteralExpression:
                { 
                    int val;
                    double dval;
                    if (int.TryParse(valueStr, out val))
                        return Compiler.Int;
                    else if (double.TryParse(valueStr, out dval))
                        return Compiler.Double;

                    break;
                }

                case SyntaxKind.StringLiteralExpression: 
                    return Compiler.String;
                
                case SyntaxKind.TrueLiteralExpression:
                case SyntaxKind.FalseLiteralExpression:
                    return Compiler.Boolean;
            }

            return SyntaxFactory.ParseTypeName(valueStr);
        }

        public override SyntaxNode VisitEventDeclaration(EventDeclarationSyntax node)
        {
            if (!node.Identifier.IsMissing)
                return node;

            string event_name    = node.Type.ToString();
            string delegate_name = event_name + "_delegate";

            Debug.Assert(pending_ == null);
            pending_ = new ResolveEventArguments(delegate_name, ctx_, members_, node.Modifiers);

            return SyntaxFactory.EventDeclaration(SyntaxFactory.IdentifierName(delegate_name), SyntaxFactory.Identifier(event_name)).
                                    WithModifiers(node.Modifiers).
                                    WithAccessorList(SyntaxFactory.AccessorList(SyntaxFactory.List( new AccessorDeclarationSyntax[] 
                                    {
                                        SyntaxFactory.AccessorDeclaration(SyntaxKind.AddAccessorDeclaration, 
                                            SyntaxFactory.Block(SyntaxFactory.List(new StatementSyntax[] 
                                            {
                                                Compiler.EventAccessor(event_name, true)
                                            }))),

                                        SyntaxFactory.AccessorDeclaration(SyntaxKind.RemoveAccessorDeclaration, 
                                            SyntaxFactory.Block(SyntaxFactory.List(new StatementSyntax[] 
                                            {
                                                Compiler.EventAccessor(event_name, false)
                                            }))),
                                    })));
        }

        public override SyntaxNode VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            return rewriteInvocation(node, false);
        }

        public override SyntaxNode VisitEqualsValueClause(EqualsValueClauseSyntax node)
        {
            if (node.Value is InvocationExpressionSyntax)
            {
                //only support simple cases of dsl invocation
                return SyntaxFactory.EqualsValueClause(node.EqualsToken, (ExpressionSyntax)rewriteInvocation((InvocationExpressionSyntax)node.Value, true));
            }
            else if (node.Value is ElementAccessExpressionSyntax)
            {
                ElementAccessExpressionSyntax array = (ElementAccessExpressionSyntax)node.Value;
                var result = VisitElementAccessExpression(array);
                if (result == null)
                    return null;

                return SyntaxFactory.EqualsValueClause(node.EqualsToken, (ExpressionSyntax)result);
            }

            return node;
        }

        public override SyntaxNode VisitElementAccessExpression(ElementAccessExpressionSyntax node)
        {
            bool isAssignment = node.Expression.IsMissing;
            bool isParam      = node.Expression is InvocationExpressionSyntax;

            if (!isAssignment && !isParam)
                return node;

            List<ExpressionSyntax> casting = new List<ExpressionSyntax>();
            foreach (ArgumentSyntax arg in node.ArgumentList.Arguments)
            {
                casting.Add(arg.Expression);
            }

            ArrayCreationExpressionSyntax array = SyntaxFactory.ArrayCreationExpression(
                                                        SyntaxFactory.ArrayType(SyntaxFactory.IdentifierName("object[]")),
                                                        SyntaxFactory.InitializerExpression(SyntaxKind.ArrayInitializerExpression,
                                                            SyntaxFactory.SeparatedList(casting)));

            if (isAssignment)
            {
                return ctx_.AddLinker(array, linkArray);
            }
            else if (isParam)
            {
                //first parameter is an array
                var invocation    = (InvocationExpressionSyntax)node.Expression;
                var newInvocation = SyntaxFactory.InvocationExpression(invocation.Expression);
                var result = ctx_.AddLinker(newInvocation, (ctx, linkNode, newNode, model) =>
                {
                    var args = (ArgumentListSyntax)ctx.GetLinkData(linkNode);
                    var inv  = (InvocationExpressionSyntax)newNode;
                    return inv.WithArgumentList(args);
                });

                Debug.Assert(pending_ == null);
                pending_ = new ResolveArrayArgument(ctx_, result, array);

                return result;
            }

            return node;
        }

        private SyntaxNode linkArray(ExcessContext ctx, SyntaxNode linkNode, SyntaxNode newNode, SemanticModel model)
        {
            ArrayCreationExpressionSyntax ace = newNode as ArrayCreationExpressionSyntax;
            ArgumentSyntax arg = null;
            bool asParam = newNode is ArgumentSyntax;
            if (asParam)
            {
                arg = (ArgumentSyntax)newNode;
                ace = (ArrayCreationExpressionSyntax)arg.Expression;
            }

            ITypeSymbol arrayType = null;
            foreach (var expr in ace.Initializer.Expressions)
            {
                ITypeSymbol type = model.GetSpeculativeTypeInfo(expr.SpanStart, expr, SpeculativeBindingOption.BindAsExpression).Type;

                if (arrayType == null)
                    arrayType = type;
                else if (type != arrayType)
                {
                    if (isSuperClass(type, arrayType))
                        arrayType = type; //downcast
                    else if (!isSuperClass(arrayType, type))
                    {
                        //td: error
                        return newNode; //unable to refine
                    }
                }
            }

            if (arrayType == null)
                return newNode;

            ace = ace.WithType(SyntaxFactory.ArrayType(SyntaxFactory.IdentifierName(arrayType.Name + "[]")));
            if (asParam)
                return arg.WithExpression(ace);

            return ace;
        }

        public override SyntaxNode VisitArgumentList(ArgumentListSyntax node)
        {
            List<ArgumentSyntax> casting = new List<ArgumentSyntax>();
            foreach (ArgumentSyntax arg in node.Arguments)
            {
                casting.Add(SyntaxFactory.Argument((ExpressionSyntax)Visit(arg.Expression)));
            }

            return SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(casting));
        }

        public override SyntaxNode VisitIncompleteMember(IncompleteMemberSyntax node)
        {
            string typename = node.Type.ToString();
            IDSLHandler dsl = ctx_.CreateDSL(typename);
            if (dsl != null)
            {
                pending_ = new ResolveDSLClass(dsl, ctx_, members_);
                return null;
            }

            return node;
        }

        public override SyntaxNode VisitExpressionStatement(ExpressionStatementSyntax node)
        {
            SyntaxNode result = Visit(node.Expression);
            if (result == null)
                return null;

            return result is ExpressionSyntax ? node.WithExpression((ExpressionSyntax)result) : result;
        }

        public override SyntaxNode VisitLocalDeclarationStatement(LocalDeclarationStatementSyntax node)
        {
            if (node.Declaration.Variables.Count == 1)
            {
                IDSLHandler dsl = null;
                dsl = ctx_.CreateDSL(node.Declaration.Type.ToString());
                if (dsl != null)
                {
                    if (node.Declaration.Variables.Count != 1)
                    {
                        //td: error
                        return node;
                    }

                    var variable = node.Declaration.Variables[0];
                    DSLContext dctx = new DSLContext { MainNode = variable, Surroundings = DSLSurroundings.Code, Assign = false, ExtraMembers = members_ };
                    Debug.Assert(pending_ == null);
                    pending_ = new ResolveDSLCode(dsl, ctx_, dctx);
                    return dsl.compile(ctx_, dctx);
                }
            }

            return base.VisitLocalDeclarationStatement(node);
        }

        //utils
        private SyntaxNode rewriteTypedef(MethodDeclarationSyntax node)
        {
            Debug.Assert(node.Body == null); //td: error
            if (node.SemicolonToken.IsMissing)
            {
                //td: error, lets make our like esasier
                return node;
            }

            int firstChar = node.Identifier.SpanStart;
            int lastChar  = node.SemicolonToken.SpanStart + node.SemicolonToken.Span.Length;
            TextSpan span = new TextSpan(firstChar, lastChar - firstChar);
            var typedefText = node.SyntaxTree.GetText().GetSubText(span).ToString();

            SyntaxNode compiled = SyntaxFactory.ParseStatement(typedefText);
            if (compiled is LocalDeclarationStatementSyntax)
            {
                LocalDeclarationStatementSyntax varDecl = (LocalDeclarationStatementSyntax)compiled;
                if (varDecl .Declaration.Variables.Count == 1)
                {
                    string typeName = varDecl.Declaration.Variables.First().Identifier.ToString();
                    ClassDeclarationSyntax parent = (ClassDeclarationSyntax)node.Parent;

                    var resultType = ctx_.CompileType(varDecl.Declaration.Type, parent);
                    ctx_.AddTypeInfo(parent.Identifier.ToString(), "typedefs", new Typedef(typeName, (TypeSyntax)resultType));
                    
                    //td: !!!
                    //ExcessTypeInfo type = ctx_.GetTarget<ExcessTypeInfo>();
                    //Debug.Assert(type != null && varDecl.Declaration.Type != null);
                    //type.RegisterType(typeName, varDecl.Declaration.Type);
                    return null;
                }
            }
            else
            {
                //td: error
            }

            return node;
        }

        private SyntaxNode rewriteFunction(MethodDeclarationSyntax node, bool asPublic)
        {
            //since no return type has been spacified we need to know if it returns something
            bool            returns   = node.DescendantNodes().OfType<ReturnStatementSyntax>().Any();
            TypeSyntax      rtype     = returns ? SyntaxFactory.IdentifierName("object") : SyntaxFactory.IdentifierName("void");

            List<SyntaxToken> modifiers = new List<SyntaxToken>();
            bool found = false;
            foreach (var mod in node.Modifiers)
            {
                switch(mod.CSharpKind())
                {
                    case SyntaxKind.PublicKeyword:
                    case SyntaxKind.PrivateKeyword:
                    case SyntaxKind.ProtectedKeyword:
                    case SyntaxKind.InternalKeyword:
                    {
                        found = true;
                        break;
                    }
                }

                modifiers.Add(mod);
            }

            if (!found)
            {
                if (asPublic)
                    modifiers.AddRange(Compiler.Public);
                else
                    modifiers.AddRange(Compiler.Private);
            }

            SyntaxNode result = SyntaxFactory.MethodDeclaration(rtype, node.Identifier)
                                    .WithModifiers(SyntaxFactory.TokenList(modifiers))
                                    .WithParameterList(rewriteParamList(node.ParameterList))
                                    .WithBody(rewriteBody(node.Body))
                                    .WithAdditionalAnnotations( new SyntaxAnnotation("ExcessFunction"));

            return returns? ctx_.AddLinker(result, (ctx, linkNode, newNode, model) =>
            {
                MethodDeclarationSyntax mthod = (MethodDeclarationSyntax)linkNode;
                ControlFlowAnalysis     cfa   = model.AnalyzeControlFlow(mthod.Body);

                ITypeSymbol rt     = null;
                foreach (var rs in cfa.ReturnStatements)
                {
                    ReturnStatementSyntax rss  = (ReturnStatementSyntax)rs;
                    ITypeSymbol           type = model.GetSpeculativeTypeInfo(rss.Expression.SpanStart, rss.Expression, SpeculativeBindingOption.BindAsExpression).Type;
                    
                    if (type == null)
                        continue;

                    if (type.TypeKind == TypeKind.Error)
                        return newNode;

                    if (rt == null)
                        rt = type;
                    else if (rt != type)
                        return newNode;
                }

                if (rt == null)
                    return newNode;

                MethodDeclarationSyntax res = (MethodDeclarationSyntax)newNode;
                return res.WithReturnType(SyntaxFactory.ParseTypeName(rt.Name));
            }) : result;
        }

        private SyntaxNode rewriteEventHandler(MethodDeclarationSyntax node)
        {
            var code = (BlockSyntax)VisitBlock(node.Body);
            var args = node.ParameterList.ReplaceNodes(node.ParameterList.Parameters, (oldParam, newParam) =>
            {
                if (oldParam.Identifier.IsMissing)
                    return newParam.WithType(Void).
                                    WithIdentifier(SyntaxFactory.Identifier(newParam.Type.ToString()));

                return newParam;
            });

            var result = node.WithReturnType(Void).
                              WithParameterList(args).  
                              WithModifiers(Private).
                              WithBody(code);

            return ctx_.AddLinker(result, (ctx, linkNode, newNode, model) =>
            {
                MethodDeclarationSyntax mthd   = (MethodDeclarationSyntax)linkNode;
                MethodDeclarationSyntax output = (MethodDeclarationSyntax)newNode;
                
                ParameterListSyntax methdArgs = mthd.ParameterList;

                string methodName = "on_" + mthd.Identifier.ToString();
                
                ISymbol     self = model.GetDeclaredSymbol(mthd);
                ITypeSymbol type = (ITypeSymbol)self.ContainingSymbol;

                string evName   = mthd.Identifier.ToString();
                string typeName = type.Name;
                bool   found    = false;
                while (type != null && !found)
                {
                    foreach (var ev in type.GetMembers().OfType<IEventSymbol>())
                    {
                        if (matchEventName(ev.Name, evName))
                        {
                            //arguments
                            foreach (var syntax in ev.Type.DeclaringSyntaxReferences)
                            {
                                var refNode = (DelegateDeclarationSyntax)syntax.GetSyntax();

                                int  pCount = methdArgs.Parameters.Count;
                                int  idx    = 0;
                                bool match  = true;
                                args = refNode.ParameterList.ReplaceNodes(refNode.ParameterList.Parameters, (oldArg, newArg) => 
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
                                                    return newArg.WithIdentifier(SyntaxFactory.Identifier(arg.Type.ToString()));

                                                return arg;
                                            }
                                        }
                                    }

                                    return newArg;
                                });
                            }

                            //register event initialization
                            ctx.AddTypeInfo(typeName, "initializer", EventInitializer(ev.Name, methodName));
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

                return output.WithIdentifier(SyntaxFactory.Identifier(methodName)).
                              WithParameterList(args);
            });
        }

        private bool matchEventName(string ev1, string ev2)
        {
            return ev1.ToLower().IndexOf(ev2.ToLower()) == 0; //td: 
        }

        private ParameterListSyntax rewriteParamList(ParameterListSyntax node)
        {
            ExcessParamListRewriter rw = new ExcessParamListRewriter(ctx_);
            return (ParameterListSyntax)rw.Visit(node);
        }

        private BlockSyntax rewriteBody(BlockSyntax node)
        {
            return (BlockSyntax)Visit(node);
        }

        private SyntaxNode rewriteInvocation(InvocationExpressionSyntax node, bool assign)
        {
            IDSLHandler dsl = ctx_.CreateDSL(node.Expression.ToString());
            if (dsl != null)
            {
                DSLContext dctx = new DSLContext { MainNode = node, Surroundings = DSLSurroundings.Code, Assign = assign, ExtraMembers = members_ };

                Debug.Assert(pending_ == null);
                pending_ = new ResolveDSLCode(dsl, ctx_, dctx);
                return dsl.compile(ctx_, dctx);
            }
            else if (node.ArgumentList.GetDiagnostics().Any())
            {
                return SyntaxFactory.InvocationExpression(node.Expression, (ArgumentListSyntax)Visit(node.ArgumentList));
            }

            return node;
        }

        private SyntaxNode rewriteConstructor(MethodDeclarationSyntax node)
        {
            string name = "__xs_constructor";
            
            ClassDeclarationSyntax parent = node.Parent as ClassDeclarationSyntax;
            if (parent != null)
            {
                name = parent.Identifier.ToString();
            }

            var modifiers = node.Modifiers.Any() ? node.Modifiers : Public;
            return SyntaxFactory.ConstructorDeclaration(name).
                                    WithModifiers(modifiers).
                                    WithParameterList(node.ParameterList).
                                    WithBody(node.Body);
        }

        private bool isSuperClass(ITypeSymbol type1, ITypeSymbol type2)
        {
            var parent = type2.BaseType;
            while (parent != null)
            {
                if (parent.Name == type1.Name)
                    return true;

                parent = parent.BaseType;
            }

            return false;
        }

        private ExcessContext ctx_;
        private ITreeLookAhead pending_;
        private List<MemberDeclarationSyntax> members_ = new List<MemberDeclarationSyntax>();


        //helpers
        static internal string LinkerAnnotationId = "ExcessLink";

        static internal SyntaxAnnotation LinkerAnnotation(string data)
        {
            return new SyntaxAnnotation(LinkerAnnotationId, data);
        }

        //Types
        public static TypeSyntax Void    = SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword));
        public static TypeSyntax Object  = SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.ObjectKeyword));
        public static TypeSyntax Double  = SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.DoubleKeyword));
        public static TypeSyntax Int     = SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.IntKeyword));
        public static TypeSyntax String  = SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.StringKeyword));
        public static TypeSyntax Boolean = SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.BoolKeyword));

        //Modifiers
        public static SyntaxTokenList Public = SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword));
        public static SyntaxTokenList Private = SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PrivateKeyword));

        //Artifacts
        private static StatementSyntax _EventInitializer = SyntaxFactory.ParseStatement("__1 += __2;");
        public  static StatementSyntax EventInitializer(string addMethod, string implementor)
        {
            return _EventInitializer.ReplaceNodes(_EventInitializer.DescendantNodes().OfType<IdentifierNameSyntax>(), (oldNode, newNode) =>
            {
                var id = oldNode.Identifier.ToString() == "__1" ? addMethod : implementor;
                return newNode.WithIdentifier(SyntaxFactory.Identifier(id));
            });
        }

        private static StatementSyntax _EventAdd    = SyntaxFactory.ParseStatement("__1 += value;");
        private static StatementSyntax _EventRemove = SyntaxFactory.ParseStatement("__1 -= value;");
        public static StatementSyntax EventAccessor(string eventName, bool addAccessor)
        {
            var node = addAccessor ? _EventAdd : _EventRemove;
            return node.ReplaceNodes(node.DescendantNodes().OfType<IdentifierNameSyntax>(), (oldNode, newNode) =>
            {
                if (oldNode.Identifier.ToString() == "__1")
                {
                    return newNode.WithIdentifier(SyntaxFactory.Identifier(eventName));
                }
                
                return newNode;
            });
        }

        private static PropertyDeclarationSyntax _Property = SyntaxFactory.ParseCompilationUnit("public __1 __2 {get; set;}").
                                                                    DescendantNodes().OfType<PropertyDeclarationSyntax>().First();

        private static PropertyDeclarationSyntax Property(TypeSyntax typeSyntax)
        {
            return _Property.ReplaceNode(_Property.Type, typeSyntax);
        }

        private static PropertyDeclarationSyntax Property(TypeSyntax typeSyntax, SyntaxToken identifier)
        {
            return _Property.WithIdentifier(identifier).
                             WithType(typeSyntax);
        }

        private static SyntaxTree _AugmentSyntax = SyntaxFactory.ParseSyntaxTree(
        @"public class __1
        {
            public void main()
            {
            }
        }");

        public static SyntaxTree AugmentCode(string className, string code)
        {
            StatementSyntax statement = SyntaxFactory.ParseStatement("{" + code + "}");
            if (statement is BlockSyntax)
            {
                var root = _AugmentSyntax.GetRoot();
                var classNode = root.DescendantNodes().OfType<ClassDeclarationSyntax>().Single();

                root = root.ReplaceToken(classNode.Identifier, SyntaxFactory.ParseToken(className));
                
                var blockNode = root.DescendantNodes().OfType<BlockSyntax>().Single();
                return root.ReplaceNode(blockNode, (BlockSyntax)statement).
                       SyntaxTree;
            }
            else
            {
                //td: error
            }

            return null;
        }

        private SyntaxTree AugmentClass(string className, IEnumerable<MemberDeclarationSyntax> members)
        {
            return SyntaxFactory.ClassDeclaration(className).
                                    WithMembers(SyntaxFactory.List(members)).
                   SyntaxTree;
        }
    }

    //td: !!! move this from here
    public class ExcessParamListRewriter : CSharpSyntaxRewriter
    {
        public ExcessParamListRewriter(ExcessContext ctx)
        {
            ctx_ = ctx;
        }

        public override SyntaxNode VisitParameter(ParameterSyntax node)
        {
            if (node.Identifier.IsMissing)
            {
                SyntaxNode result = SyntaxFactory.Parameter(SyntaxFactory.ParseToken(node.Type.ToString())).
                                        WithType(SyntaxFactory.IdentifierName("object"));

                return ctx_.AddLinker(result, (ctx, linkNode, newNode, model) =>
                {
                    MethodDeclarationSyntax container = linkNode.Ancestors().OfType<MethodDeclarationSyntax>().First();
                    ParameterSyntax         parameter = (ParameterSyntax)newNode;
                    var                     cs        = model.GetDeclaredSymbol(container);
                    ISymbol                 ps        = model.GetDeclaredSymbol(linkNode);

                    if (cs.DeclaredAccessibility == Accessibility.Private)
                    {
                        int pidx = 0;
                        foreach(var param in container.ParameterList.Parameters)
                        {
                            if (param == linkNode)
                                break;

                            pidx++;
                        }

                        var invocations = container.Parent.DescendantNodes().OfType<InvocationExpressionSyntax>();

                        ITypeSymbol type = null;
                        foreach (var invocation in invocations)
                        {
                            SymbolInfo si  = model.GetSymbolInfo(invocation.Expression);
                            ISymbol    sym = null;
                            if (si.Symbol == null)
                                sym = si.CandidateReason == CandidateReason.OverloadResolutionFailure ? si.CandidateSymbols[0] : null;
                            else
                                sym = si.Symbol;

                            if (sym != cs)
                                continue;

                            if (pidx < invocation.ArgumentList.Arguments.Count)
                            {
                                ITypeSymbol st = model.GetSpeculativeTypeInfo(invocation.ArgumentList.SpanStart, invocation.ArgumentList.Arguments[pidx].Expression, SpeculativeBindingOption.BindAsExpression).ConvertedType;
                                if (st != null)
                                {
                                    if (type == null)
                                        type = st;
                                    else if (type != st)
                                    {
                                        //td: !!! make this an util
                                        return linkNode;
                                    }
                                }
                            }
                        };

                        if (type != null)
                            return parameter.WithType(SyntaxFactory.IdentifierName(type.Name));
                    }
                    else
                    {
                        //td: error, only privates
                    }

                    return newNode;
                });
            }

            return node;
        }

        private ExcessContext ctx_;
    }

    public class ExcessAugmenter : CSharpSyntaxVisitor
    {
        public enum Result
        {
            Empty,
            Code,
            Class,
            Error

        }

        public static Result classify(SyntaxNode node, out IEnumerable<MemberDeclarationSyntax> members)
        {
            ExcessAugmenter worker = new ExcessAugmenter(node);
            worker.Visit(node);

            return worker.getResult(out members);
        }

        ExcessAugmenter(SyntaxNode node)
        {
            node_ = node;
        }

        public override void Visit(SyntaxNode node)
        {
            List<MemberDeclarationSyntax> members = new List<MemberDeclarationSyntax>();
            var newRoot = node.ReplaceNodes(node.ChildNodes().OfType<MemberDeclarationSyntax>(), (oldNode, newNode) =>
            {
                members.Add(newNode);
                return null;
            });

            if (node is CompilationUnitSyntax)
            {
                base.Visit(node);
                return;
            }

            all_++;
            if (node is MemberDeclarationSyntax)
            {
                members_.Add((MemberDeclarationSyntax)node);
            }
        }

        private SyntaxNode                    node_;
        private int                           all_ = 0;
        private List<MemberDeclarationSyntax> members_ = new List<MemberDeclarationSyntax>();
        
        private Result getResult(out IEnumerable<MemberDeclarationSyntax> members)
        {
            members = members_;

            if (all_ == 0)
            {
                bool codeFound = node_.GetDiagnostics().
                                       Where(error => 
                                       {
                                           return error.Id == "CS1022";
                                       }).Any();
                
                if (codeFound)
                    return Result.Code;

                return Result.Empty;
            }

            if (members_.Count == 0)
                return Result.Empty;

            if (all_ == members_.Count)
                return Result.Class;

            return Result.Error;
        }
    }
}
