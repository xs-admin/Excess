using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess.Compiler
{

    public interface ISyntacticalMatch
    {
        INestedSyntacticalMatch children(Func<SyntaxNode, bool> handler);
        INestedSyntacticalMatch children<T>(Func<T, bool> handler = null) where T : SyntaxNode;
        INestedSyntacticalMatch children();
        INestedSyntacticalMatch descendants(Func<SyntaxNode, bool> handler);
        INestedSyntacticalMatch descendants<T>(Func<T, bool> handler = null) where T : SyntaxNode;
        INestedSyntacticalMatch descendants();
        ISyntaxAnalysis then(Func<SyntaxNode, SyntaxNode> handler);
        ISyntaxAnalysis then(ISyntaxTransform transform);
    }

    public interface INestedSyntacticalMatch :  ISyntacticalMatch
    {
        ISyntacticalMatch then(Func<SyntaxNode, SyntaxNode> handler, bool continueToParent);
        ISyntacticalMatch then(ISyntaxTransform transform, bool continueToParent);
    }

    public interface ISyntacticalMatchResult
    {
        void matchNodes(IEnumerable<SyntaxNode> nodes, ISyntacticalMatch match);
    }

    public interface ISyntaxTransform
    {
        ISyntaxTransform insert();
        ISyntaxTransform replace();
        ISyntaxTransform remove();
    }

    public interface ISyntaxAnalysis
    {
        ISyntaxAnalysis looseStatements(Func<IEnumerable<StatementSyntax>, SyntaxNode> handler);
        ISyntaxAnalysis looseMembers(Func<IEnumerable<MemberDeclarationSyntax>, SyntaxNode> handler);
        ISyntaxAnalysis looseTypes(Func<IEnumerable<TypeDeclarationSyntax>, SyntaxNode> handler);

        ISyntacticalMatch match<T>(Func<T, bool> handler);
        ISyntacticalMatch match();
        ISyntacticalMatch matchCodeDSL(string dsl);
        ISyntacticalMatch matchTypeDSL(string dsl);
        ISyntacticalMatch matchMemberDSL(string dsl);
        ISyntacticalMatch matchNamespaceDSL(string dsl);

        IEnumerable<ISyntacticalMatch> matches();

        ISyntaxTransform transform();

    }
}
