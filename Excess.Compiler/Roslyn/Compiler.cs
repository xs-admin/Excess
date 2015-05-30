using Excess.Compiler.Core;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Excess.Compiler.Roslyn
{
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Text;
    using CSharp = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

    public class CompilerService : ICompilerService<SyntaxToken, SyntaxNode, SemanticModel>
    {
        public string TokenToString(SyntaxToken token, out string xsId)
        {
            xsId = RoslynCompiler.TokenMark(token);
            return token.ToFullString();
        }

        public string TokenToString(SyntaxToken token, out int xsId)
        {
            string xsStr = RoslynCompiler.TokenMark(token);
            xsId = xsStr == null ? -1 : int.Parse(xsStr);
            return token.ToFullString();
        }

        public string TokenToString(SyntaxToken token)
        {
            return token.ToFullString();
        }

        public SyntaxToken MarkToken(SyntaxToken token, out int xsId)
        {
            string strId = RoslynCompiler.uniqueId();
                    xsId = int.Parse(strId);
            return RoslynCompiler.MarkToken(token, strId);
        }

        public SyntaxToken MarkToken(SyntaxToken token)
        {
            string strId = RoslynCompiler.uniqueId();
            return RoslynCompiler.MarkToken(token, strId);
        }

        public SyntaxToken InitToken(SyntaxToken token, int xsId)
        {
            return RoslynCompiler.MarkToken(token, xsId.ToString());
        }

        public IEnumerable<SyntaxToken> MarkTokens(IEnumerable<SyntaxToken> tokens, out int xsId)
        {
            string strId = RoslynCompiler.uniqueId();
                    xsId = int.Parse(strId);

            return tokens.Select(token => RoslynCompiler.MarkToken(token, strId));
        }

        public SyntaxNode MarkNode(SyntaxNode node, out int xsId)
        {
            string strId = GetExcessStringId(node);
            if (strId != null)
            {
                xsId = int.Parse(strId);
                return node;
            }

            strId = RoslynCompiler.uniqueId();
            xsId  = int.Parse(strId);
            return RoslynCompiler.MarkNode(node, strId); 
        }

        public SyntaxNode MarkNode(SyntaxNode node)
        {
            return RoslynCompiler.MarkNode(node, RoslynCompiler.uniqueId()); //td: scope ids
        }

        public SyntaxNode MarkTree(SyntaxNode node)
        {
            //td: optimize?
            return node.ReplaceNodes(node.DescendantNodes(), (oldNode, newNode) =>
            {
                return MarkNode(newNode);
            });
        }

        public int GetExcessId(SyntaxToken token)
        {
            string xsStr = RoslynCompiler.TokenMark(token);
            return xsStr == null ? -1 : int.Parse(xsStr);
        }

        public int GetExcessId(SyntaxNode node)
        {
            string xsStr = RoslynCompiler.NodeMark(node);
            return xsStr == null ? -1 : int.Parse(xsStr);
        }

        public string GetExcessStringId(SyntaxNode node)
        {
            return RoslynCompiler.NodeMark(node);
        }

        public bool isIdentifier(SyntaxToken token)
        {
            return RoslynCompiler.isLexicalIdentifier(token.Kind());
        }


        public SyntaxNode Parse(string text)
        {
            return CSharp.ParseSyntaxTree(text).GetRoot();
        }

        public IEnumerable<SyntaxToken> ParseTokens(string text)
        {
            return CSharp.ParseTokens(text)
                .Where(token => token.Kind() != SyntaxKind.EndOfFileToken);
        }

        public IEnumerable<SyntaxNode> Find(SyntaxNode node, IEnumerable<string> xsIds)
        {
            //td: optimize
            foreach (var id in xsIds)
            {
                var result = FindNode(node, id);
                if (result != null)
                    yield return result;
            }
        }

        public SyntaxNode Find(SyntaxNode node, SourceSpan span)
        {
            return node.FindNode(new TextSpan(span.Start, span.Length));
        }

        public SyntaxNode FindNode(SyntaxNode node, string xsId)
        {
            return node.GetAnnotatedNodes(RoslynCompiler.NodeIdAnnotation + xsId).FirstOrDefault();
        }

        public int GetOffset(SyntaxToken token)
        {
            return token.SpanStart;
        }
    }

    public class RoslynCompiler : CompilerBase<SyntaxToken, SyntaxNode, SemanticModel>
    {
        public RoslynCompiler(ICompilerEnvironment environment, Scope scope = null) : 
            base(new RoslynLexicalAnalysis(), 
                 new RoslynSyntaxAnalysis(),
                 new RoslynSemanticAnalysis(),
                 environment,
                 new InstanceAnalisysBase<SyntaxToken, SyntaxNode, SemanticModel>(),
                 scope)
        {
            _scope.set<ICompilerService<SyntaxToken, SyntaxNode, SemanticModel>>(new CompilerService());
            _scope.set<ICompilerEnvironment>(environment);
        }

        public RoslynCompiler(Scope scope) : this(new RoslynEnvironment(scope, null), scope)
        {
        }

        public RoslynCompiler() : this(new Scope(null))
        {
        }

        protected override IDocument<SyntaxToken, SyntaxNode, SemanticModel> createDocument()
        {
            var result = new RoslynDocument(_scope);
            _scope.set<IDocument<SyntaxToken, SyntaxNode, SemanticModel>>(result);

            applyLexical(result);
            return result;
        }

        public static ExpressionSyntax Constant(object value)
        {
            return SyntaxFactory.ParseExpression(value.ToString());
        }

        private void applyLexical(RoslynDocument document)
        {
            var handler = _lexical as IDocumentInjector<SyntaxToken, SyntaxNode, SemanticModel>;
            Debug.Assert(handler != null);

            handler.apply(document);
        }

        //out of interface methods, used for testing
        public RoslynDocument CreateDocument(string text)
        {
            return new RoslynDocument(_scope, text);
        }

        public ExpressionSyntax CompileExpression(string expr)
        {
            var   document = new RoslynDocument(_scope, expr);
            var   handler  = _lexical as IDocumentInjector<SyntaxToken, SyntaxNode, SemanticModel>;
            _scope.set<IDocument<SyntaxToken, SyntaxNode, SemanticModel>>(document);

            handler.apply(document);
            document.applyChanges(CompilerStage.Lexical);

            return CSharp.ParseExpression(document.LexicalText);
        }

        public SyntaxNode ApplyLexicalPass(string text, out string newText)
        {
            var document = new RoslynDocument(_scope, text);
            var handler = _lexical as IDocumentInjector<SyntaxToken, SyntaxNode, SemanticModel>;
            _scope.set<IDocument<SyntaxToken, SyntaxNode, SemanticModel>>(document);

            handler.apply(document);
            document.applyChanges(CompilerStage.Lexical);

            newText = document.LexicalText;
            return document.SyntaxRoot;
        }

        public string ApplyLexicalPass(string text)
        {
            string result;
            ApplyLexicalPass(text, out result);
            return result;
        }

        public SyntaxTree ApplySyntacticalPass(string text, out string result)
        {
            var document = new RoslynDocument(_scope, text); //we actually dont touch our own state during these calls
            var lHandler = _lexical as IDocumentInjector<SyntaxToken, SyntaxNode, SemanticModel>;
            var sHandler = _syntax as IDocumentInjector<SyntaxToken, SyntaxNode, SemanticModel>;

            lHandler.apply(document);
            sHandler.apply(document);

            document.applyChanges(CompilerStage.Syntactical);

            result = document.SyntaxRoot.NormalizeWhitespace().ToFullString();
            return document.SyntaxRoot.SyntaxTree;
        }

        public SyntaxTree ApplySyntacticalPass(string text)
        {
            string useless;
            return ApplySyntacticalPass(text, out useless);
        }

        public SyntaxTree ApplySemanticalPass(string text)
        {
            string useless;
            return ApplySemanticalPass(text, out useless);
        }

        public SyntaxTree ApplySemanticalPass(string text, out string result)
        {
            var document = new RoslynDocument(_scope, text);
            return ApplySemanticalPass(document, out result);
        }

        public SyntaxTree ApplySemanticalPass(RoslynDocument document, out string result)
        {
            var lHandler  = _lexical as IDocumentInjector<SyntaxToken, SyntaxNode, SemanticModel>;
            var sHandler  = _syntax as IDocumentInjector<SyntaxToken, SyntaxNode, SemanticModel>;
            var ssHandler = _semantics as IDocumentInjector<SyntaxToken, SyntaxNode, SemanticModel>;

            lHandler.apply(document);
            sHandler.apply(document);
            ssHandler.apply(document);

            document.applyChanges(CompilerStage.Syntactical);
            var tree = document.SyntaxRoot.SyntaxTree;

            var compilation = CSharpCompilation.Create("semantical-pass",
                syntaxTrees: new[] { tree },
                references: new[]
                {
                    MetadataReference.CreateFromAssembly(typeof(object).Assembly),
                    MetadataReference.CreateFromAssembly(typeof(Enumerable).Assembly),
                    MetadataReference.CreateFromAssembly(typeof(Dictionary<int, int>).Assembly),
                },
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            while (true)
            {
                document.Model = compilation.GetSemanticModel(tree);
                if (document.applyChanges(CompilerStage.Semantical))
                    break;

                var oldTree = tree;
                tree = document.SyntaxRoot.SyntaxTree;
                compilation = compilation.ReplaceSyntaxTree(oldTree, tree);
            } 

            result = document.SyntaxRoot.NormalizeWhitespace().ToFullString();
            return document.SyntaxRoot.SyntaxTree;
        }

        public SyntaxTree CompileInstance(RoslynInstanceDocument document, out string result)
        {
            var iHandler = _instance as IDocumentInjector<SyntaxToken, SyntaxNode, SemanticModel>;
            iHandler.apply(document);

            return ApplySemanticalPass(document, out result);
        }


        public static Func<SyntaxNode, Scope, SyntaxNode> AddMember(MemberDeclarationSyntax member)
        {
            return (node, scope) =>
            {
                Debug.Assert(node is TypeDeclarationSyntax);

                if (node is ClassDeclarationSyntax)
                    return (node as ClassDeclarationSyntax).AddMembers(member);

                Debug.Assert(false); //td: case
                return node;                        
            };
        }

        public static Func<SyntaxNode, Scope, SyntaxNode> AddStatement(StatementSyntax statement, SyntaxNode before = null, SyntaxNode after = null)
        {
            return (node, scope) =>
            {
                var block = node as BlockSyntax;
                Debug.Assert(block != null);
                    
                IEnumerable<StatementSyntax> newStatements = InsertStatement(block, statement, before, after);
                return block
                    .WithStatements(CSharp.List(
                        newStatements));
            };
        }

        private static IEnumerable<StatementSyntax> InsertStatement(BlockSyntax block, StatementSyntax statement, SyntaxNode before, SyntaxNode after)
        {
            Debug.Assert(before != null || after != null);

            string target = before != null ?
                RoslynCompiler.NodeMark(before) :
                RoslynCompiler.NodeMark(after);

            Debug.Assert(target != null);
                                            
            foreach (var st in block.Statements)
            {
                string id = RoslynCompiler.NodeMark(st);
                if (id == target)
                {
                    if (before != null)
                    {
                        yield return statement;
                        yield return st;
                    }
                    else
                    {
                        yield return st;
                        yield return statement;
                    }
                }
                else
                    yield return st;
            }
        }

        //declarations
        public static TypeSyntax @void      = CSharp.PredefinedType(CSharp.Token(SyntaxKind.VoidKeyword));
        public static TypeSyntax @object    = CSharp.PredefinedType(CSharp.Token(SyntaxKind.ObjectKeyword));
        public static TypeSyntax @double    = CSharp.PredefinedType(CSharp.Token(SyntaxKind.DoubleKeyword));
        public static TypeSyntax @int       = CSharp.PredefinedType(CSharp.Token(SyntaxKind.IntKeyword));
        public static TypeSyntax @string    = CSharp.PredefinedType(CSharp.Token(SyntaxKind.StringKeyword));
        public static TypeSyntax @boolean   = CSharp.PredefinedType(CSharp.Token(SyntaxKind.BoolKeyword));
        public static TypeSyntax @dynamic   = CSharp.ParseTypeName("dynamic");
        public static TypeSyntax @exception = CSharp.ParseTypeName("Exception");
        

        //modifiers
        public static SyntaxTokenList @public  = CSharp.TokenList(CSharp.Token(SyntaxKind.PublicKeyword));

        public static SyntaxTokenList @private = CSharp.TokenList(CSharp.Token(SyntaxKind.PrivateKeyword));
        public static SyntaxTokenList @static  = CSharp.TokenList(CSharp.Token(SyntaxKind.StaticKeyword));
        public static SyntaxToken     @out     = CSharp.Token(SyntaxKind.OutKeyword);

        //constants
        public static ExpressionSyntax @null = CSharp.ParseExpression("null");
        public static ExpressionSyntax @true = CSharp.ParseExpression("true");
        public static ExpressionSyntax @false = CSharp.ParseExpression("false");

        //tokens
        public static SyntaxToken semicolon = CSharp.ParseToken(";");
        

        //node marking
        static private int _seed = 0;
        public static string uniqueId()
        {
            return (++_seed).ToString();
        }

        public static string NodeIdAnnotation = "xs-node";

        public static SyntaxNode MarkNode(SyntaxNode node, string id)
        {
            return node
                .WithoutAnnotations(NodeIdAnnotation)
                .WithoutAnnotations(NodeIdAnnotation + id)
                .WithAdditionalAnnotations(
                    new SyntaxAnnotation(NodeIdAnnotation + id),
                    new SyntaxAnnotation(NodeIdAnnotation, id));
        }

        public static SyntaxNode TrackNode(SyntaxNode node)
        {
            var id = NodeMark(node);
            if (id != null)
                return node;

            return MarkNode(node, uniqueId());
        }

        public static string NodeMark(SyntaxNode node)
        {
            var annotation = node
                .GetAnnotations(NodeIdAnnotation)
                    .FirstOrDefault();

            if (annotation != null)
                return annotation.Data;

            return null;
        }

        public static SyntaxToken MarkToken(SyntaxToken token, string id)
        {
            return token
                .WithoutAnnotations(NodeIdAnnotation)
                .WithAdditionalAnnotations(new SyntaxAnnotation(NodeIdAnnotation, id));
        }

        public static string TokenMark(SyntaxToken token)
        {
            var annotation = token.GetAnnotations(NodeIdAnnotation).FirstOrDefault();
            if (annotation != null)
                return annotation.Data;

            return null;
        }

        public static SyntaxToken MarkToken(SyntaxToken token, string mark, object value)
        {
            var result = value == null ? new SyntaxAnnotation(mark) :
                                         new SyntaxAnnotation(mark, value.ToString());

            return token
                .WithoutAnnotations(mark)
                .WithAdditionalAnnotations(result);
        }

        public static IEnumerable<SyntaxToken> ParseTokens(string text)
        {
            var tokens = CSharp.ParseTokens(text);
            foreach (var token in tokens)
            {
                if (token.Kind() != SyntaxKind.EndOfFileToken)
                    yield return token;
            }
        }

        public static string TokensToString(IEnumerable<SyntaxToken> tokens)
        {
            StringBuilder result = new StringBuilder();
            foreach (var token in tokens)
                result.Append(token.ToFullString());

            return result.ToString();
        }

        public static ParameterListSyntax ParseParameterList(IEnumerable<SyntaxToken> parameters)
        {
            string parameterString = TokensToString(parameters); //td: mapping
            return CSharp.ParseParameterList(parameterString);
        }
        public static bool isLexicalIdentifier(SyntaxToken token)
        {
            return isLexicalIdentifier(token.Kind());
        }

        public static bool isLexicalIdentifier(SyntaxKind kind)
        {
            switch (kind)
            {
                case SyntaxKind.IdentifierToken:
                case SyntaxKind.BoolKeyword:
                case SyntaxKind.ByteKeyword:
                case SyntaxKind.SByteKeyword:
                case SyntaxKind.ShortKeyword:
                case SyntaxKind.UShortKeyword:
                case SyntaxKind.IntKeyword:
                case SyntaxKind.UIntKeyword:
                case SyntaxKind.LongKeyword:
                case SyntaxKind.ULongKeyword:
                case SyntaxKind.DoubleKeyword:
                case SyntaxKind.FloatKeyword:
                case SyntaxKind.DecimalKeyword:
                case SyntaxKind.StringKeyword:
                case SyntaxKind.CharKeyword:
                case SyntaxKind.VoidKeyword:
                case SyntaxKind.ObjectKeyword:
                case SyntaxKind.NullKeyword:
                    return true;
            }

            return false;
        }

        static public Func<SyntaxNode, Scope, SyntaxNode> RemoveStatement(SyntaxNode statement)
        {
            string statementId = NodeMark(statement);
            return (node, scope) =>
            {
                BlockSyntax code = (BlockSyntax)node;
                return code.RemoveNode(FindNode(code.Statements, statementId), SyntaxRemoveOptions.KeepTrailingTrivia);
            };
        }

        static public Func<SyntaxNode, Scope, SyntaxNode> RemoveMember(SyntaxNode member)
        {
            string memberId = NodeMark(member);
            return (node, scope) =>
            {
                var clazz = (ClassDeclarationSyntax)node;
                clazz = clazz.RemoveNode(FindNode(clazz.Members, memberId), SyntaxRemoveOptions.KeepTrailingTrivia);
                return clazz;
            };
        }

        static public SyntaxNode FindNode<T>(SyntaxList<T> nodes, string id) where T : SyntaxNode
        {
            foreach (var node in nodes)
            {
                if (NodeMark(node) == id)
                    return node;
            }

            return null;
        }

        static public SyntaxNode FindNode(SyntaxNode root, string id)
        {
            return root
                .GetAnnotatedNodes(NodeIdAnnotation + id)
                .FirstOrDefault();
        }

        static public SyntaxNode FindNode(SyntaxNode root, SyntaxNode tracked)
        {
            string id = NodeMark(tracked);
            if (id == null)
                return null;

            return FindNode(root, id);
        }

        static public SyntaxNode FindNode<T>(SyntaxList<T> nodes, SyntaxNode tracked) where T : SyntaxNode
        {
            string id = NodeMark(tracked);
            if (id == null)
                return null;

            return FindNode<T>(nodes, id);
        }

        static public StatementSyntax NextStatement(BlockSyntax code, StatementSyntax statement)
        {
            var found = false;
            foreach (var stment in code.Statements)
            {
                if (found)
                    return stment;

                found = stment == statement;
            }

            return null;
        }

        public static bool HasVisibilityModifier(SyntaxTokenList modifiers)
        {
            foreach(var modifier in modifiers)
            {
                switch (modifier.Kind())
                {
                    case SyntaxKind.PublicKeyword:
                    case SyntaxKind.PrivateKeyword:
                    case SyntaxKind.ProtectedKeyword:
                    case SyntaxKind.InternalKeyword:
                        return true;
                }
            }

            return false;
        }

        static public Func<SyntaxNode, Scope, SyntaxNode> AddInitializers(StatementSyntax initializer)
        {
            return (node, scope) =>
            {
                ClassDeclarationSyntax decl = (ClassDeclarationSyntax)node;

                var found = false;
                var result = decl.ReplaceNodes(decl
                    .DescendantNodes()
                    .OfType<ConstructorDeclarationSyntax>(), (oldConstuctor, newConstuctor) =>
                    {
                        found = true;
                        return newConstuctor.WithBody(SyntaxFactory.Block().
                                                WithStatements(SyntaxFactory.List(
                                                    newConstuctor.Body.Statements.Union(
                                                    new[] { initializer} ))));
                    });

                if (!found)
                {
                    result = result.WithMembers(SyntaxFactory.List(
                                        result.Members.Union(
                                        new MemberDeclarationSyntax[]{
                                            SyntaxFactory.ConstructorDeclaration(decl.Identifier.ToString()).
                                                WithBody(SyntaxFactory.Block().
                                                    WithStatements(SyntaxFactory.List(new[] {
                                                        initializer })))})));
                }

                return result;
            };
        }

        public static Func<SyntaxNode, Scope, SyntaxNode> ExplodeBlock(SyntaxNode newNode)
        {
            return (node, scope) =>
            {
                var block = node as BlockSyntax;
                return block
                    .WithStatements(CSharp.List(
                        ExplodeStatements(block.Statements, newNode)));
            };
        }

        private static IEnumerable<StatementSyntax> ExplodeStatements(SyntaxList<StatementSyntax> statements, SyntaxNode newNode)
        {
            var block = (BlockSyntax)newNode;
            var nodeID = RoslynCompiler.NodeMark(newNode);
            foreach (var statement in statements)
            {
                var innerID = RoslynCompiler.NodeMark(statement);
                if (innerID == nodeID)
                {
                    foreach (var inner in block.Statements)
                        yield return inner;
                }
                else
                    yield return statement;
            }
        }

        public static Func<SyntaxNode, Scope, SyntaxNode> ReplaceNode(SyntaxNode newNode)
        {
            return (node, scope) => RoslynCompiler.ReplaceExcessId(newNode, node);
        }

        public static SyntaxNode ReplaceExcessId(SyntaxNode node, SyntaxNode before)
        {
            string oldId = NodeMark(before);
            Debug.Assert(oldId != null);

            return MarkNode(node, oldId); 
        }

        public static SyntaxNode UpdateExcessId(SyntaxNode node, SyntaxNode before)
        {
            string oldId = NodeMark(before);
            Debug.Assert(oldId != null);

            string newId = NodeMark(node);
            if (newId == null)
                return MarkNode(node, oldId); //mark substitutions

            return node;
        }

        public static TypeSyntax ConstantType(ExpressionSyntax value)
        {
            switch (value.Kind())
            {
                case SyntaxKind.NumericLiteralExpression:
                { 
                    var valueStr = value.ToString();
                    
                    int val;
                    double dval;
                    if (int.TryParse(valueStr, out val))
                        return @int;
                    else if (double.TryParse(valueStr, out dval))
                        return @double;

                    break;
                }

                case SyntaxKind.StringLiteralExpression: 
                    return @string;
                
                case SyntaxKind.TrueLiteralExpression:
                case SyntaxKind.FalseLiteralExpression:
                    return @boolean;
            }

            return null;
        }

        public static TypeSyntax GetReturnType(BlockSyntax code, SemanticModel model)
        {
            ControlFlowAnalysis cfa = model.AnalyzeControlFlow(code);
            if (!cfa.ReturnStatements.Any())
                return @void;

            ITypeSymbol rt = null;
            foreach (var rs in cfa.ReturnStatements)
            {
                ReturnStatementSyntax rss = (ReturnStatementSyntax)rs;
                ITypeSymbol type = model.GetSpeculativeTypeInfo(rss.Expression.SpanStart, rss.Expression, SpeculativeBindingOption.BindAsExpression).Type;

                if (type == null)
                    continue;

                if (type.TypeKind == TypeKind.Error)
                {
                    rt = null;
                    break;
                }

                if (rt == null)
                    rt = type;
                else if (rt != type)
                {
                    rt = null;
                    break;
                }
            }

            if (rt == null)
                return @dynamic;

            return CSharp.ParseTypeName(rt.Name);
        }

        //td: !!! refactor the marking
        public static SyntaxNode UnMark(SyntaxNode node)
        {
            return node.ReplaceNodes(node.DescendantNodesAndSelf(), (oldNode, newNode) =>
            {
                var id = RoslynCompiler.NodeMark(oldNode);
                return newNode
                    .WithoutAnnotations(RoslynCompiler.NodeIdAnnotation)
                    .WithoutAnnotations(RoslynCompiler.NodeIdAnnotation + id);
            });
        }

        public static SyntaxNode Mark(SyntaxNode node)
        {
            return node.ReplaceNodes(node.DescendantNodesAndSelf(), (oldNode, newNode) =>
            {
                return MarkNode(newNode, uniqueId());
            });
        }

        public static bool IsVisible(MemberDeclarationSyntax member)
        {
            var modifiers = default(SyntaxTokenList);
            if (member is MethodDeclarationSyntax)
                modifiers = (member as MethodDeclarationSyntax).Modifiers;
            else if (member is PropertyDeclarationSyntax)
                modifiers = (member as PropertyDeclarationSyntax).Modifiers;
            else if (member is FieldDeclarationSyntax)
                modifiers = (member as FieldDeclarationSyntax).Modifiers;
            else
                throw new NotImplementedException();

            return modifiers
                .Where(modifier => modifier.Kind() == SyntaxKind.PublicKeyword
                                || modifier.Kind() == SyntaxKind.InternalKeyword)
                .Any();
        }

        public static ExpressionSyntax Quoted(string value)
        {
            return CSharp.ParseExpression('"' + value + '"');
        }

        public static SyntaxNode ReplaceAssignment(SyntaxNode node, SyntaxNode newNode, out bool isAssignment)
        {
            isAssignment = false;
            if (node is LocalDeclarationStatementSyntax)
            {
                isAssignment = true;

                var decl = node as LocalDeclarationStatementSyntax;
                return decl
                    .WithDeclaration(decl.Declaration
                    .WithVariables(CSharp.SeparatedList(new[] {
                            decl.Declaration.Variables[0]
                            .WithInitializer(decl.Declaration.Variables[0].Initializer
                            .WithValue((ExpressionSyntax)newNode))})));
            }

            if (node is BinaryExpressionSyntax)
            {
                var expr = node as BinaryExpressionSyntax;
                if (expr.Kind() == SyntaxKind.SimpleAssignmentExpression)
                {
                    isAssignment = true;
                    return expr.WithRight((ExpressionSyntax)newNode);
                }
            }

            return newNode;
        }

        //symbols
        public static ITypeSymbol SymbolType(SemanticModel model, SyntaxNode node)
        {
            if (node is CastExpressionSyntax)
                return SymbolType(model
                    .GetSymbolInfo((node as CastExpressionSyntax)
                        .Type)
                    .Symbol);

            return SymbolType(model.GetSymbolInfo(node).Symbol);
        }

        public static TypeSyntax SymbolTypeSyntax(SemanticModel model, SyntaxNode node)
        {
            ITypeSymbol type = SymbolType(model, node);
            if (type == null)
                return null;

            return CSharp.ParseTypeName(type.Name);
        }

        public static TypeSyntax SymbolTypeSyntax(ISymbol symbol)
        {
            ITypeSymbol type = SymbolType(symbol);
            if (type == null)
                return null;

            return CSharp.ParseTypeName(type.Name);
        }

        public static ITypeSymbol SymbolType(ISymbol symbol)
        {
            if (symbol == null)
                return null;

            switch (symbol.Kind)
            {
                case SymbolKind.Local: return ((ILocalSymbol)symbol).Type;
                case SymbolKind.Field: return ((IFieldSymbol)symbol).Type;
                case SymbolKind.Property: return ((IPropertySymbol)symbol).Type;
                case SymbolKind.Method: return ((IMethodSymbol)symbol).ReturnType;
                case SymbolKind.Parameter: return ((IParameterSymbol)symbol).Type;
                case SymbolKind.NamedType: return (ITypeSymbol)symbol;
                default:
                    {
                        return null;
                    }
            }
        }

        public static Dictionary<SyntaxNode, SyntaxNode> Track(SyntaxTree syntaxTree, Dictionary<SyntaxNode, SyntaxNode> nodes)
        {
            var result = new Dictionary<SyntaxNode, SyntaxNode>();
            foreach (var node in nodes)
            {
                if (node.Key.SyntaxTree == syntaxTree)
                    result[node.Key] = node.Value;
                else
                {
                    var nn = Track(syntaxTree, node.Key);
                    if (nn != null)
                        result[nn] = node.Value;
                }
            }

            return result;
        }

        public static SyntaxNode Track(SyntaxTree syntaxTree, SyntaxNode node)
        {
            var mark = NodeMark(node);
            if (mark == null)
                return null;

            return FindNode(syntaxTree.GetRoot(), mark);
        }
    }

}
