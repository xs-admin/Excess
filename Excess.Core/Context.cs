using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess.Core
{
    public enum DSLSurroundings
    {
        Global,
        TypeBody,
        Code,
    }

    public class DSLContext
    {
        public SyntaxNode MainNode { get; set; }
        public ParameterListSyntax Args { get; set; }
        public BlockSyntax Code { get; set; }
        public DSLSurroundings Surroundings { get; set; }
        public string Id { get; set; }
        public bool Assign { get; set; }


        public List<MemberDeclarationSyntax> ExtraMembers { get; set; }
    }

    public interface IDSLFactory
    {
        IDSLHandler         create(string name);
        IEnumerable<string> supported();
    }

    public interface IDSLHandler
    {
        SyntaxNode compile(ExcessContext ctx, DSLContext dctx);
        SyntaxNode link(ExcessContext ctx, SyntaxNode node, SemanticModel model);
        SyntaxNode setCode(ExcessContext ctx, DSLContext dctx, BlockSyntax code);
    }

    public enum LookAheadAction
    {
        FAILED,
        SUCCEDED,
        CONTINUE,
    }

    public interface ITreeLookAhead
    {
        SyntaxNode rewrite(CSharpSyntaxRewriter transform, SyntaxNode node, out LookAheadAction action);
    }

    public class Typedef
    {
        public Typedef(string name, TypeSyntax type)
        { 
            Name = name;
            Type = type;
        }

        public string     Name { get; set; }
        public TypeSyntax Type { get; set; }
    }

    public class ExcessContext
    {
        public static SyntaxTree Compile(string code, IDSLFactory factory, out ExcessContext ctx, string file = null)
        {
            ctx = new ExcessContext(factory);
            return Compile(ctx, code, file);
        }

        public static SyntaxTree Compile(ExcessContext ctx, string code, string file = null)
        {
            Compiler compiler = new Compiler(ctx);

            ctx.FileName = file;
            ctx.Rewriter = compiler;

            SyntaxTree tree = compiler.compile(code);

            bool needsRecompile = true; //td: !!!
            if (needsRecompile)
            {
                var root = tree.GetRoot() as CompilationUnitSyntax;
                if (root == null)
                {
                    root = SyntaxFactory.CompilationUnit().
                                            WithMembers(SyntaxFactory.List(new MemberDeclarationSyntax[] { 
                                                (MemberDeclarationSyntax)tree.GetRoot()
                                            }));
                }

                var usings = ctx.GetUsings().Select<string, UsingDirectiveSyntax>(
                                                @using => SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName(
                                                                        @using)));

                root = root.WithUsings(SyntaxFactory.List(usings)).
                            NormalizeWhitespace();

                //root = root.WithUsings(SyntaxFactory.List(new UsingDirectiveSyntax[] 
                //        {
                //            SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName("System")),
                //            SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName("System.Collections")),
                //            SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName("System.Collections.Generic")),
                //        })).NormalizeWhitespace();

                var linkerNodes = root.GetAnnotatedNodes(Compiler.LinkerAnnotationId);
                string currVersion = root.ToFullString();
                
                root = SyntaxFactory.ParseCompilationUnit(currVersion);

                foreach (var linkNode in linkerNodes)
                {
                    SyntaxNode rn = root.FindNode(linkNode.Span);
                    if (rn != null)
                    {
                        var annotation = linkNode.GetAnnotations(Compiler.LinkerAnnotationId).First();
                        annotation = new SyntaxAnnotation(annotation.Kind, annotation.Data);

                        root = root.ReplaceNode(rn, rn.WithAdditionalAnnotations(annotation));
                    }
                }

                tree = root.SyntaxTree;
            }

            ctx.FileName = null;
            return tree;
        }

        public static Compilation Link(ExcessContext ctx, Compilation compilation, Dictionary<SyntaxTree, SyntaxTree> track = null)
        {
            Compilation result = compilation;
            foreach (var tree in compilation.SyntaxTrees)
            {
                SyntaxTree curr = ctx.fixErrors(tree, result, out result);
                
                var linker     = new Linker(ctx, result);
                var resultTree = linker.link(curr.GetRoot(), out result).SyntaxTree;
                if (track != null)
                {
                    track[tree] = resultTree;
                }
            }

            return result;
        }

        public static Compilation Link(ExcessContext ctx, Compilation compilation, IEnumerable<SyntaxTree> trees, Dictionary<SyntaxTree, SyntaxTree> track = null)
        {
            Compilation result = compilation;
            foreach (var tree in trees)
            {
                //add usings
                var root = tree.GetRoot() as CompilationUnitSyntax;
                var actualTree = root.SyntaxTree;
                SyntaxTree curr = ctx.fixErrors(actualTree, result, out result);

                Linker linker = new Linker(ctx, result);
                var resultTree = linker.link(curr.GetRoot(), out result).SyntaxTree;

                if (track != null)
                    track[tree] = resultTree;
            }

            return result;
        }

        public IEnumerable<Diagnostic> GetErrors(Compilation compilation, SemanticModel model, out Compilation result)
        {
            IEnumerable<Diagnostic> errors = null;
            result = compilation;
            try
            {
                errors = model.GetDiagnostics();
            }
            catch
            {
                //td: revise this on newer versions of Roslyn
                //old diagnostics or nodes seems to linger on after being replaced
                var tree = model.SyntaxTree;
                var root = model.SyntaxTree.GetRoot().NormalizeWhitespace();

                var linkerNodes = root.GetAnnotatedNodes(Compiler.LinkerAnnotationId);
                string currVersion = root.ToFullString();
                root = SyntaxFactory.ParseCompilationUnit(currVersion);

                foreach (var linkNode in linkerNodes)
                {
                    SyntaxNode rn = root.FindNode(linkNode.Span);
                    if (rn != null)
                    {
                        var annotation = linkNode.GetAnnotations(Compiler.LinkerAnnotationId).First();
                        root = root.ReplaceNode(rn, rn.WithAdditionalAnnotations(annotation));
                    }
                }

                var trees = new List<SyntaxTree>();
                foreach (var tt in result.SyntaxTrees)
                {
                    if (tt == tree)
                        trees.Add(root.SyntaxTree);
                    else
                        trees.Add(tt);
                }

                result = CSharpCompilation.Create(result.AssemblyName,
                            syntaxTrees: trees,
                            references: new[] {
                                MetadataReference.CreateFromAssembly(typeof(object).Assembly),
                                MetadataReference.CreateFromAssembly(typeof(Enumerable).Assembly),
                                MetadataReference.CreateFromAssembly(typeof(Dictionary<int, int>).Assembly),
                            },
                            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

                model = result.GetSemanticModel(root.SyntaxTree);
                errors = model.GetDiagnostics();
            }

            return errors;
        }

        public IEnumerable<Diagnostic> GetErrors(Compilation compilation, out Compilation result, Dictionary<SyntaxTree, SyntaxTree> track = null)
        {
            //td: !!! fix
            IEnumerable<Diagnostic> errors = null;
            result = compilation;
            try
            {
                errors = result.GetDiagnostics();
            }
            catch
            {
                //td: !!! recompile before first compilation, dont work
                var trees = new List<SyntaxTree>();
                foreach (var tree in compilation.SyntaxTrees)
                {
                    var root = tree.GetRoot().NormalizeWhitespace();

                    var linkerNodes = root.GetAnnotatedNodes(Compiler.LinkerAnnotationId);
                    string currVersion = root.ToFullString();
                    root = SyntaxFactory.ParseCompilationUnit(currVersion);

                    foreach (var linkNode in linkerNodes)
                    {
                        SyntaxNode rn = root.FindNode(linkNode.Span);
                        if (rn != null)
                        {
                            var annotation = linkNode.GetAnnotations(Compiler.LinkerAnnotationId).First();
                            root = root.ReplaceNode(rn, rn.WithAdditionalAnnotations(annotation));
                        }
                    }

                    if (track != null)
                        track[tree] = root.SyntaxTree;

                    trees.Add(root.SyntaxTree);
                }

                result = CSharpCompilation.Create(result.AssemblyName,
                            syntaxTrees: trees,
                            references: new[] {
                                MetadataReference.CreateFromAssembly(typeof(object).Assembly),
                                MetadataReference.CreateFromAssembly(typeof(Enumerable).Assembly),
                            },
                            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

                errors = result.GetDiagnostics();
            }

            return errors;
        }

        private SyntaxTree fixErrors(SyntaxTree tree, Compilation compilation, out Compilation result)
        {
            var model     = compilation.GetSemanticModel(tree);
            var root      = tree.GetRoot();
            var toReplace = new Dictionary<SyntaxNode, SyntaxNode>();

            var errors = model.GetDiagnostics();
                result = compilation; 

            foreach (var error in errors)
            {
                SyntaxNode oldNode;
                SyntaxNode fixedNode = handleError(root, out oldNode, error, model);
                if (oldNode == null || oldNode == fixedNode)
                    continue;

                toReplace[oldNode] = fixedNode;
            }

            if (toReplace.Count == 0)
                return tree;

            var resultNode = root.ReplaceNodes(toReplace.Keys, (o, n) => toReplace[o]);

            result = result.ReplaceSyntaxTree(tree, resultNode.SyntaxTree);
            return resultNode.SyntaxTree;
        }

        private SyntaxNode handleError(SyntaxNode node, out SyntaxNode oldNode, Diagnostic error, SemanticModel model)
        {
            oldNode = null;
            if (error.Severity != DiagnosticSeverity.Error)
                return null;

            oldNode = node.FindNode(error.Location.SourceSpan);
            if (oldNode == null || oldNode.ToString() == "void")
            {
                oldNode = null;
                return null;
            }

            //td: !!! error manager
            switch (error.Id)
            {
                case "CS0246":
                {
                    //type not found
                    string typeName = oldNode.ToString();

                    if (oldNode is GenericNameSyntax)
                    {
                        GenericNameSyntax syntax = (GenericNameSyntax)oldNode;
                        typeName = syntax.Identifier.ToString();
                    }

                    switch (typeName)
                    {
                        case "function": return FunctionType(oldNode);
                        default:
                        {
                            var currType = oldNode.Ancestors().OfType<ClassDeclarationSyntax>().FirstOrDefault(); 
                            if (currType == null)
                                break;

                            IEnumerable<Typedef> tdefs = GetTypeInfo<Typedef>(currType.Identifier.ToString(), "typedefs");

                            if (tdefs != null)
                            {
                                var TypeName = oldNode.ToString();
                                foreach (var typedef in tdefs)
                                {
                                    if (typedef.Name == TypeName)
                                        return typedef.Type;
                                }
                            }
                            break;
                        }
                    }

                    break;
                }

                case "CS0815":
                {
                    if (oldNode is VariableDeclaratorSyntax)
                    {
                        VariableDeclaratorSyntax            decl   = (VariableDeclaratorSyntax)oldNode;
                        ParenthesizedLambdaExpressionSyntax lambda = (ParenthesizedLambdaExpressionSyntax)decl.Initializer.Value;

                        var arguments     = lambda.ParameterList.Parameters;
                        var funcArguments = new List<TypeSyntax>();
                        foreach (var arg in arguments)
                        {
                            if (arg.Type == null || arg.Type.IsMissing)
                                return oldNode; //can't recover

                            funcArguments.Add(arg.Type);
                        }

                        var         returnStatements = lambda.Body.DescendantNodes().OfType<ReturnStatementSyntax>();
                        ITypeSymbol returnType       = null;
                        bool        returns          = false;
                        foreach (var rs in returnStatements)
                        {
                            returns |= rs.Expression != null;

                            ITypeSymbol type = model.GetSpeculativeTypeInfo(rs.Expression.SpanStart, rs.Expression, SpeculativeBindingOption.BindAsExpression).Type;
                            if (type == null)
                                return oldNode; //can't recover

                            if (returnType == null)
                                returnType = type;
                            else if (type != returnType)
                            {
                                returnType = null;
                                break;
                            }
                        }

                        TypeSyntax resultType;
                        if (!returns)
                        {
                            resultType = SyntaxFactory.GenericName("Action").
                                                            WithTypeArgumentList(SyntaxFactory.TypeArgumentList(SyntaxFactory.SeparatedList(
                                                                funcArguments)));
                        }
                        else
                        {
                            string returnTypeName = "object";
                            if (returnType != null)
                            {
                                returnTypeName = returnType.Name;
                            }

                            funcArguments.Add(SyntaxFactory.ParseTypeName(returnTypeName));
                            resultType = SyntaxFactory.GenericName("Func").
                                                        WithTypeArgumentList(SyntaxFactory.TypeArgumentList(SyntaxFactory.SeparatedList(
                                                            funcArguments)));        
                        }


                        oldNode = oldNode.Parent;
                        VariableDeclarationSyntax declaration = (VariableDeclarationSyntax)oldNode;

                        return declaration.WithType(resultType);
                    }
                    
                    break;
                }
            }

            return oldNode;
        }

        public ExcessContext(IDSLFactory factory)
        {
            _dslFactory = factory;
            _usings     = new List<string>();
            _usings.AddRange(new [] {"System", "System.Collections", "System.Collections.Generic"});
        }

        public Compiler Rewriter { get; set; }
        public string   FileName { get; set; }

        public SyntaxNode Compile(SyntaxNode node)
        {
            Debug.Assert(Rewriter != null);
            return Rewriter.Visit(node);
        }

        public IDSLHandler CreateDSL(string name)
        {
            return _dslFactory.create(name);
        }

        public string RegisterDSL(IDSLHandler dsl)
        {
            int hash = dsl.GetHashCode();
            Debug.Assert(!_dslInstances.ContainsKey(hash));
            _dslInstances[hash] = dsl;

            return hash.ToString();
        }

        public IDSLHandler GetDSL(string id)
        {
            int hash = Convert.ToInt32(id);
            Debug.Assert(_dslInstances.ContainsKey(hash));
            return _dslInstances[hash];
        }

        public object GetVariable(string name)
        {
            object var;
            if (vars_.TryGetValue(name, out var))
                return var;
            return null;
        }

        public void SetVariable(string name, object value)
        {
            vars_[name] = value;
        }

        public SyntaxNode AddLinker(SyntaxNode node, Func<ExcessContext, SyntaxNode, SyntaxNode, SemanticModel, SyntaxNode> linker)
        {
            string id = (node.GetHashCode() + linker.GetHashCode()).ToString();
            if (linkers_.ContainsKey(id))
                return node;

            linkers_[id] = linker;
            return node.WithAdditionalAnnotations(Compiler.LinkerAnnotation(id));
        }

        public SyntaxNode Link(SyntaxNode node, SyntaxNode newNode, string linker, SemanticModel model)
        {
            if (linkers_.ContainsKey(linker))
                return linkers_[linker](this, node, newNode, model);

            IDSLHandler dsl = GetDSL(linker);
            if (dsl != null)
                return dsl.link(this, node, model);

            return node;
        }

        public bool NeedsLinking()
        {
            return linkers_.Count > 0;
        }

        public void AddTypeInfo(string type, string field, object value)
        {
            Dictionary<string, List<object>> info;
            if (!types_.TryGetValue(type, out info))
            {
                info = new Dictionary<string, List<object>>();
                types_[type] = info;
            }

            List<object> data;
            if (!info.TryGetValue(field, out data))
            {
                data = new List<object>();
                info[field] = data;
            }

            data.Add(value);
        }

        public IEnumerable<T> GetTypeInfo<T>(string type, string field)
        {
            Dictionary<string, List<object>> info;
            if (types_.TryGetValue(type, out info))
            {
                List<object> data;
                if (info.TryGetValue(field, out data))
                {
                    return data.Select<object, T>( t => (T)t );
                }
            }

            return null;
        }

        public SyntaxNode ApplyLinkerInfo(SyntaxNode node, SemanticModel model, Compilation compilation, out Compilation resultCompilation)
        {
            var tree = node.SyntaxTree;
            node = node.ReplaceNodes(node.DescendantNodes().OfType<ClassDeclarationSyntax>(), (oldNode, newNode) =>
            {
                ClassDeclarationSyntax decl = (ClassDeclarationSyntax)oldNode;

                var init_statements = GetTypeInfo<StatementSyntax>(decl.Identifier.ToString(), "initializer"); //td:
                if (init_statements == null || !init_statements.Any())
                    return newNode;

                var found = false;
                var result = decl.ReplaceNodes(decl.DescendantNodes().OfType<ConstructorDeclarationSyntax>(), (oldConstuctor, newConstuctor) =>
                {
                    found = true;
                    return newConstuctor.WithBody(SyntaxFactory.Block().
                                            WithStatements(SyntaxFactory.List(
                                                newConstuctor.Body.Statements.Union(
                                                init_statements))));
                });

                if (!found)
                {
                    result = result.WithMembers(SyntaxFactory.List(
                                        result.Members.Union(
                                        new MemberDeclarationSyntax[]{
                                            SyntaxFactory.ConstructorDeclaration(oldNode.Identifier.ToString()).
                                                WithBody(SyntaxFactory.Block().
                                                    WithStatements(SyntaxFactory.List(init_statements)))
                                        })));
                }

                return result;
            });

            resultCompilation = compilation.ReplaceSyntaxTree(tree, node.SyntaxTree);
            return node;
        }

        private IDSLFactory _dslFactory;
        private Dictionary<int, IDSLHandler> _dslInstances = new Dictionary<int, IDSLHandler>();
        private Dictionary<string, object> vars_ = new Dictionary<string, object>();
        private Dictionary<string, Func<ExcessContext, SyntaxNode, SyntaxNode, SemanticModel, SyntaxNode>> linkers_ = new Dictionary<string, Func<ExcessContext, SyntaxNode, SyntaxNode, SemanticModel, SyntaxNode>>();
        private Dictionary<string, object> linkerData_ = new Dictionary<string,object>(); 
        private Dictionary<string, Dictionary<string, List<object>>> types_ = new Dictionary<string, Dictionary<string, List<object>>>();

        public SyntaxNode CompileType(SyntaxNode type, ClassDeclarationSyntax owner)
        {
            string               typeName = type.ToString();
            GenericNameSyntax    generic  = null;
            IEnumerable<Typedef> typedefs = null;

            if (type is GenericNameSyntax)
            {
                generic   = (GenericNameSyntax)type;
                typeName = generic.Identifier.ToString();
            }

            switch (typeName)
            {
                case "function": return FunctionType(type);
                default:
                {
                    if (owner == null)
                    {
                        var ancestor = type.Ancestors().OfType<ClassDeclarationSyntax>();
                        if (!ancestor.Any())
                            break;

                        owner = ancestor.First(); 
                    }

                    var TypeName = type.ToString();
                    Debug.Assert(owner != null);
                    typedefs = GetTypeInfo<Typedef>(owner.Identifier.ToString(), "typedefs");

                    //check type defs 
                    if (typedefs != null)
                    {
                        foreach (var typedef in typedefs)
                        {
                            if (typedef.Name == TypeName)
                                return typedef.Type;
                        }
                    }
                    break;
                }
            }

            if (generic != null && typedefs != null)
            {
                var genType = typedefs.FirstOrDefault( typedef => typedef.Name == generic.Identifier.ToString() );
                if (genType != null)
                {
                    var cast = (GenericNameSyntax)genType.Type;
                    generic  = cast.WithIdentifier(generic.Identifier);
                }

                bool found = false;
                var typeArgumentList = generic.TypeArgumentList.
                                                WithArguments(SyntaxFactory.SeparatedList(
                                                    generic.TypeArgumentList.Arguments.Select(arg =>
                                                    {
                                                        var argType = typedefs.FirstOrDefault(typedef => typedef.Name == arg.ToString());
                                                        if (argType != null)
                                                        {
                                                            found = true;
                                                            return argType.Type;
                                                        }

                                                        return arg;
                                                    })));

                if (found)
                    generic = generic.WithTypeArgumentList(typeArgumentList);

                type = generic;
            }

            return type;
        }

        public static SyntaxNode FunctionType(SyntaxNode func)
        {
            if (func is GenericNameSyntax)
            {
                GenericNameSyntax syntax = (GenericNameSyntax)func;
                TypeSyntax returnType = syntax.TypeArgumentList.Arguments.First();

                if (returnType == null) //empty list
                {
                    return syntax.WithIdentifier(SyntaxFactory.Identifier("Action"));
                }
                else
                {
                    List<TypeSyntax> typeList = null;
                    foreach (var type in syntax.TypeArgumentList.Arguments)
                    {
                        if (typeList == null)
                            typeList = new List<TypeSyntax>();
                        else
                            typeList.Add(type);
                    }

                    return returnType.ToString() == "void" ? syntax.WithIdentifier(SyntaxFactory.Identifier("Action")).
                                                                WithTypeArgumentList(SyntaxFactory.TypeArgumentList(SyntaxFactory.SeparatedList(typeList)))
                                                            : syntax.WithIdentifier(SyntaxFactory.Identifier("Func")).
                                                                WithTypeArgumentList(SyntaxFactory.TypeArgumentList(SyntaxFactory.SeparatedList(
                                                                    typeList.Union(new TypeSyntax[] { returnType }))));
                }
            }

            return SyntaxFactory.IdentifierName("Action"); 
        }


        private string getLinkId(SyntaxNode node)
        {
            var annotation = node.GetAnnotations(Compiler.LinkerAnnotationId).First();
            if (annotation == null)
                return null;

            return annotation.Data;
        }

        public void AddLinkData(SyntaxNode node, object data)
        {
            var linkId = getLinkId(node);
            if (linkId != null)
                linkerData_[linkId] = data;
        }

        public object GetLinkData(SyntaxNode node)
        {
            var linkId = getLinkId(node);
            if (linkId == null)
                return null;

            object result = null;
            linkerData_.TryGetValue(linkId, out result);
            return result;
        }

        public SyntaxTree Complete(IEnumerable<StatementSyntax> statements, IEnumerable<MemberDeclarationSyntax> members, IEnumerable<MemberDeclarationSyntax> types)
        {
            var info = new CompleteInfo
            {
                Context      = this,
                Statements   = statements,
                Members      = members,
                Types        = types, 
                DefaultClass = FileName,
            };

            IEnumerable<SyntaxNode> completed = null;
            if (_onComplete != null)
            {
                completed = _onComplete(info);
                _onComplete = null;
            }
            else if (_defaultComplete != null)
                completed = _defaultComplete(info);

            if (completed == null)
                completed = types;
            else
                completed = completed.Union(types);

            return SyntaxFactory.CompilationUnit().
                                 WithMembers(SyntaxFactory.List(completed)).SyntaxTree;
        }

        public class CompleteInfo
        {
            public CompleteInfo()
            {
                Additional = new List<SyntaxNode>();
            }

            public ExcessContext                        Context;
            public string                               DefaultClass;
            public IEnumerable<StatementSyntax>         Statements;
            public IEnumerable<MemberDeclarationSyntax> Members;
            public IEnumerable<MemberDeclarationSyntax> Types;
            public List<SyntaxNode>                     Additional;

            public CompleteInfo Clone()
            {
                return new CompleteInfo
                {
                    Context = Context,
                    DefaultClass = DefaultClass,
                    Statements = Statements,
                    Members = Members,
                    Types = Types,
                    Additional = new List<SyntaxNode>(Additional),
                };
            }
        }

        private Func<CompleteInfo, IEnumerable<SyntaxNode>> _onComplete;
        private Func<CompleteInfo, IEnumerable<SyntaxNode>> _defaultComplete = _defaultHandler;

        private static IEnumerable<SyntaxNode> _defaultMemberHandler(CompleteInfo complete)
        {
            foreach (var member in complete.Members)
            {
                var method = member as MethodDeclarationSyntax;
                if (method == null)
                {
                    yield return member;
                    continue;
                }

                var ctx = complete.Context;
                var dsl = null as IDSLHandler;
                var id  = string.Empty;
                if (!method.ReturnType.IsMissing)
                {
                    dsl = ctx.CreateDSL(method.ReturnType.ToString());
                    id = method.Identifier.ToString();
                }
                else
                    dsl = ctx.CreateDSL(method.Identifier.ToString());

                if (dsl != null)
                {
                    var additional = new List<MemberDeclarationSyntax>();
                    DSLContext dctx = new DSLContext { MainNode = method, Surroundings = DSLSurroundings.Global, Id = id, ExtraMembers = additional };
                    var dslResult = dsl.compile(ctx, dctx);

                    if (dslResult != null)
                        complete.Additional.Add(dslResult);

                    complete.Additional.AddRange(additional);
                }
                else
                    yield return method;
            }
        }

        private static IEnumerable<SyntaxNode> _defaultHandler(CompleteInfo complete)
        {
            var defaultClass = complete.DefaultClass;
            var replace      = null as MemberDeclarationSyntax;
            var result       = null as ClassDeclarationSyntax;
            
            if (defaultClass != null)
            {
                replace = complete.Types.FirstOrDefault(type =>
                {
                    ClassDeclarationSyntax clazz = type as ClassDeclarationSyntax;
                    return clazz != null && clazz.Identifier.ToString() == defaultClass;
                });

                var members = _defaultMemberHandler(complete);

                if (members.Any() || complete.Statements.Any())
                {
                    result = replace != null ? replace as ClassDeclarationSyntax
                                             : SyntaxFactory.ClassDeclaration(defaultClass);


                    foreach (var member in complete.Members)
                        result = result.AddMembers(member);

                    if (complete.Statements.Any())
                        result = result.AddMembers(SyntaxFactory.ConstructorDeclaration(defaultClass).
                                                                 WithBody(SyntaxFactory.Block().
                                                                    WithStatements(SyntaxFactory.List(complete.Statements))));

                    if (replace == null)
                        yield return result;
                }
            }
            else
            {
                //td: error
            }

            foreach (var type in complete.Types.Union(complete.Additional))
            {
                if (type == replace)
                    yield return result;

                yield return type;
            }
        }

        public void SetOnComplete(Func<CompleteInfo, IEnumerable<SyntaxNode>> handler)
        {
            Debug.Assert(_onComplete == null);
            _onComplete = handler;
        }

        public void SetDefaultComplete(Func<CompleteInfo, IEnumerable<SyntaxNode>> handler)
        {
            _defaultComplete = handler;
        }

        public IEnumerable<SyntaxNode> TriggerDefaultComplete(CompleteInfo info)
        {
            if (_defaultComplete != null)
                return _defaultComplete(info);

            return null;
        }

        private List<string> _usings;
        public IEnumerable<string> GetUsings()
        {
            return _usings;
        }

        public void AddUsing(string @using)
        {
            _usings.Add(@using);
        }

        public void AddUsings(IEnumerable<string> usings)
        {
            _usings.AddRange(usings);
        }
    }
}
