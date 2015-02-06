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
            return RoslynCompiler.isLexicalIdentifier(token.CSharpKind());
        }


        public SyntaxNode Parse(string text)
        {
            return CSharp.ParseSyntaxTree(text).GetRoot();
        }

        public IEnumerable<SyntaxToken> ParseTokens(string text)
        {
            return CSharp.ParseTokens(text)
                .Where(token => token.CSharpKind() != SyntaxKind.EndOfFileToken);
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
    }

    public class RoslynCompiler : CompilerBase<SyntaxToken, SyntaxNode, SemanticModel>
    {
        public RoslynCompiler(Scope scope = null) : 
            base(new RoslynLexicalAnalysis(), 
                 new RoslynSyntaxAnalysis(),
                 new RoslynSemanticAnalysis(),
                 scope)
        {
            _scope.set<ICompilerService<SyntaxToken, SyntaxNode, SemanticModel>>(new CompilerService());
        }

        protected override IDocument<SyntaxToken, SyntaxNode, SemanticModel> createDocument()
        {
            var result = new RoslynDocument(_scope);
            _scope.set<IDocument<SyntaxToken, SyntaxNode, SemanticModel>>(result);

            applyLexical(result);
            return result;
        }

        private void applyLexical(RoslynDocument document)
        {
            var handler = _lexical as IDocumentHandler<SyntaxToken, SyntaxNode, SemanticModel>;
            Debug.Assert(handler != null);

            handler.apply(document);
        }

        //out of interface methods, used for testing
        public ExpressionSyntax CompileExpression(string expr)
        {
            var   document = new RoslynDocument(_scope, expr);
            var   handler  = _lexical as IDocumentHandler<SyntaxToken, SyntaxNode, SemanticModel>;
            _scope.set<IDocument<SyntaxToken, SyntaxNode, SemanticModel>>(document);

            handler.apply(document);
            document.applyChanges(CompilerStage.Lexical);

            return CSharp.ParseExpression(document.LexicalText);
        }

        public SyntaxNode ApplyLexicalPass(string text, out string newText)
        {
            var document = new RoslynDocument(_scope, text);
            var handler = _lexical as IDocumentHandler<SyntaxToken, SyntaxNode, SemanticModel>;
            _scope.set<IDocument<SyntaxToken, SyntaxNode, SemanticModel>>(document);

            handler.apply(document);
            document.applyChanges(CompilerStage.Lexical);

            newText = document.LexicalText;
            return document.Root;
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
            var lHandler = _lexical as IDocumentHandler<SyntaxToken, SyntaxNode, SemanticModel>;
            var sHandler = _sintaxis as IDocumentHandler<SyntaxToken, SyntaxNode, SemanticModel>;

            lHandler.apply(document);
            sHandler.apply(document);

            document.applyChanges(CompilerStage.Syntactical);

            result = document.Root.NormalizeWhitespace().ToFullString();
            return document.Root.SyntaxTree;
        }

        public SyntaxTree ApplySyntacticalPass(string text)
        {
            string useless;
            return ApplySyntacticalPass(text, out useless);
        }

        public SyntaxTree ApplySemanticalPass(string text, out string result)
        {
            var document  = new RoslynDocument(_scope, text); 
            var lHandler  = _lexical as IDocumentHandler<SyntaxToken, SyntaxNode, SemanticModel>;
            var sHandler  = _sintaxis as IDocumentHandler<SyntaxToken, SyntaxNode, SemanticModel>;
            var ssHandler = _semantics as IDocumentHandler<SyntaxToken, SyntaxNode, SemanticModel>;

            lHandler.apply(document);
            sHandler.apply(document);
            ssHandler.apply(document);

            document.applyChanges(CompilerStage.Syntactical);
            var tree = document.Root.SyntaxTree;

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
                tree = document.Root.SyntaxTree;
                compilation = compilation.ReplaceSyntaxTree(oldTree, tree);
            } 

            result = document.Root.NormalizeWhitespace().ToFullString();
            return document.Root.SyntaxTree;
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

        //declarations
        public static TypeSyntax @void    = CSharp.PredefinedType(CSharp.Token(SyntaxKind.VoidKeyword));
        public static TypeSyntax @object  = CSharp.PredefinedType(CSharp.Token(SyntaxKind.ObjectKeyword));
        public static TypeSyntax @double  = CSharp.PredefinedType(CSharp.Token(SyntaxKind.DoubleKeyword));
        public static TypeSyntax @int     = CSharp.PredefinedType(CSharp.Token(SyntaxKind.IntKeyword));
        public static TypeSyntax @string  = CSharp.PredefinedType(CSharp.Token(SyntaxKind.StringKeyword));
        public static TypeSyntax @boolean = CSharp.PredefinedType(CSharp.Token(SyntaxKind.BoolKeyword));
        public static TypeSyntax @dynamic = CSharp.ParseTypeName("dynamic");

        //modifiers
        public static SyntaxTokenList @public  = CSharp.TokenList(CSharp.Token(SyntaxKind.PublicKeyword));
        public static SyntaxTokenList @private = CSharp.TokenList(CSharp.Token(SyntaxKind.PrivateKeyword));

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
                .WithAdditionalAnnotations(
                    new SyntaxAnnotation(NodeIdAnnotation + id),
                    new SyntaxAnnotation(NodeIdAnnotation, id));
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
                if (token.CSharpKind() != SyntaxKind.EndOfFileToken)
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
            return isLexicalIdentifier(token.CSharpKind());
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
            string membertId = NodeMark(member);
            return (node, scope) =>
            {
                var clazz = (ClassDeclarationSyntax)node;
                return clazz.RemoveNode(FindNode(clazz.Members, membertId), SyntaxRemoveOptions.KeepTrailingTrivia);
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
                switch (modifier.CSharpKind())
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

        public static Func<SyntaxNode, Scope, SyntaxNode> ReplaceNode(SyntaxNode newNode)
        {
            return (node, scope) => newNode;
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
            switch (value.CSharpKind())
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
    }
}
