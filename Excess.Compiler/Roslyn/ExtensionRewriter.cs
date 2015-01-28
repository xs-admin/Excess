using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Excess.Compiler.Roslyn
{
    using System.Diagnostics;
    using CSharp = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

    //public class ExtensionRewriter : CSharpSyntaxVisitor
    //{
    //    IEventBus _events;

    //    Dictionary<string, Func<ISyntacticalMatchResult<SyntaxNode>, SyntacticalExtension<SyntaxNode>, IEnumerable<SyntaxNode>>> _codeExtensions = new Dictionary<string, Func<ISyntacticalMatchResult<SyntaxNode>, SyntacticalExtension<SyntaxNode>, IEnumerable<SyntaxNode>>>();
    //    Dictionary<string, Func<ISyntacticalMatchResult<SyntaxNode>, SyntacticalExtension<SyntaxNode>, IEnumerable<SyntaxNode>>> _memberExtensions = new Dictionary<string, Func<ISyntacticalMatchResult<SyntaxNode>, SyntacticalExtension<SyntaxNode>, IEnumerable<SyntaxNode>>>();
    //    Dictionary<string, Func<ISyntacticalMatchResult<SyntaxNode>, SyntacticalExtension<SyntaxNode>, IEnumerable<SyntaxNode>>> _typeExtensions = new Dictionary<string, Func<ISyntacticalMatchResult<SyntaxNode>, SyntacticalExtension<SyntaxNode>, IEnumerable<SyntaxNode>>>();

    //    Dictionary<SyntaxNode, Dictionary<SyntaxNode, Func<ISyntacticalMatchResult<SyntaxNode>, IEnumerable<SyntaxNode>>>> _transform = new Dictionary<SyntaxNode, Dictionary<SyntaxNode, Func<ISyntacticalMatchResult<SyntaxNode>, IEnumerable<SyntaxNode>>>>();

    //    public ExtensionRewriter(IEnumerable<SyntacticExtensionEvent<SyntaxNode>> extensions, IEventBus events)
    //    {
    //        _events = events;

    //        foreach (var ev in extensions)
    //        {
    //            switch (ev.Kind)
    //            {
    //                case ExtensionKind.Code:   _codeExtensions  [ev.Keyword] = ev.Handler; break;
    //                case ExtensionKind.Member: _memberExtensions[ev.Keyword] = ev.Handler; break;
    //                case ExtensionKind.Type:   _typeExtensions  [ev.Keyword] = ev.Handler; break;
    //                default: throw new NotImplementedException(); 
    //            }
    //        }
    //    }

    //    public SyntaxTree rewrite(SyntaxTree tree)
    //    {
    //        var node = tree.GetRoot();
    //        Visit(node);

    //        return node.ReplaceNodes(_transform.Keys, (oldNode, newNode) =>
    //        {
    //            var children = _transform[oldNode];

    //            Debug.Assert(oldNode.ChildNodes().Count() == newNode.ChildNodes().Count()); //no structural changes
    //            var newChildren = oldNode.ChildNodes().GetEnumerator();

    //            List<SyntaxNode> resultChildren = new List<SyntaxNode>();
    //            foreach (var child in oldNode.ChildNodes())
    //            {
    //                var handler  = children[child];
    //                var newChild = newChildren.Current; newChildren.MoveNext();

    //                if (handler != null)
    //                {
    //                    var transformed = handler(result);
    //                }
    //                else
    //                    resultChildren.Add(newChild); ;
    //            }

    //            return newNode.WithC
    //        });
    //    }

    //    public override void VisitBlock(BlockSyntax node)
    //    {
    //        if (!_codeExtensions.Any())
    //            return; //bail on code is no extension is registered

    //        base.VisitBlock(node);
    //    }

    //    public override void VisitMethodDeclaration(MethodDeclarationSyntax method)
    //    {
    //        Debug.Assert(RoslynCompiler.GetSyntacticalExtensionId(method) == null);

    //        SyntacticalExtension<SyntaxNode> extension;
    //        method = processMethodExtension(method, out extension);
    //        if (extension != null)
    //        {
    //            if (extension.Kind == ExtensionKind.Member)
    //            {
    //                addChild(method.Parent, method, extension);
    //            }
    //            else
    //            {
    //                //td: error, incorrect extension (i.e. a code extension being used inside a type)
    //            }
    //        }

    //        base.VisitMethodDeclaration(method);
    //    }

    //    private MethodDeclarationSyntax processMethodExtension(MethodDeclarationSyntax method, out SyntacticalExtension<SyntaxNode> extension)
    //    {
    //        extension = null;

    //        string extName = null;
    //        string extIdentifier = null;
    //        if (!method.ReturnType.IsMissing)
    //        {
    //            extName = method.ReturnType.ToString(); 
    //            extIdentifier = method.Identifier.ToString();
    //        }
    //        else
    //        {
    //            extName = method.Identifier.ToString();
    //        }

    //        if (!_memberExtensions.ContainsKey(extName))
    //            return method;

    //        extension = new SyntacticalExtension<SyntaxNode>
    //        {
    //            Kind = ExtensionKind.Member,
    //            Keyword = extName,
    //            Identifier = extIdentifier,
    //            Arguments = method.ParameterList,
    //            Body = method.Body,
    //            Handler = _memberExtensions[extName]
    //        };

    //        string id;
    //        return (MethodDeclarationSyntax)RoslynCompiler.SetSyntacticalExtensionId(method, out id);
    //    }

    //    public override void VisitInvocationExpression(InvocationExpressionSyntax call)
    //    {
    //        Debug.Assert(RoslynCompiler.GetSyntacticalExtensionId(call) == null);

    //        SyntacticalExtension<SyntaxNode> extension = processCodeExtension(call);
    //        if (extension != null && extension.Kind != ExtensionKind.Code)
    //        {
    //            //td: error, incorrect extension (i.e. a type extension being used inside code)
    //        }

    //        base.VisitInvocationExpression(call);
    //    }

    //    private SyntacticalExtension<SyntaxNode> processCodeExtension(InvocationExpressionSyntax call)
    //    {
    //        if (!(call.Expression is SimpleNameSyntax))
    //            return null; //extensions are simple identifiers

    //        var extName = call.Expression.ToString();
    //        if (!_codeExtensions.ContainsKey(extName))
    //            return null; //not an extension

    //        StatementSyntax statement = null;
    //        BlockSyntax     code      = null;

    //        var parent = call.Parent;
    //        while (parent != null)
    //        {
    //            if (parent is StatementSyntax)
    //                statement = parent as StatementSyntax;

    //            if (parent is BlockSyntax)
    //            {
    //                code = parent as BlockSyntax;
    //                break;
    //            }

    //            parent = parent.Parent;
    //        }

    //        if (code == null || statement == null)
    //            return null; //td: not sure

    //        if (hasSemicolon(statement))
    //        {
    //            //td: syntaxis error
    //            return null; 
    //        }

    //        var extCode = NextStatement<BlockSyntax>(code, statement);
    //        if (extCode == null)
    //        {
    //            //td: syntaxis error
    //            return null;
    //        }

    //        removeNode(extCode);

    //        var extension = new SyntacticalExtension<SyntaxNode>
    //        {
    //            Kind = ExtensionKind.Member,
    //            Keyword = extName,
    //            Identifier = null,
    //            Arguments = call.ArgumentList,
    //            Body = extCode,
    //            Handler = _codeExtensions[extName]
    //        };

    //        addChild(code, statement, extension);
    //        return extension;
    //    }

    //    public override void VisitIncompleteMember(IncompleteMemberSyntax member)
    //    {
    //        Debug.Assert(RoslynCompiler.GetSyntacticalExtensionId(member) == null);

    //        SyntacticalExtension<SyntaxNode> extension;
    //        member = processTypeExtension(member, out extension);
    //        if (extension != null && extension.Kind != ExtensionKind.Type)
    //        {
    //            //td: error, incorrect extension
    //        }

    //        base.VisitIncompleteMember(member);
    //    }

    //    private IncompleteMemberSyntax processTypeExtension(IncompleteMemberSyntax member, out SyntacticalExtension<SyntaxNode> extension)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}
}
