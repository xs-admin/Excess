using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.SetParser
{
    public class VariableSyntax
    {
        public VariableSyntax(
            string name,
            SyntaxToken nameToken,
            string typeName,
            SyntaxToken typeToken,
            string alias,
            SyntaxToken aliasToken,
            bool isIndexed,
            string indexName,
            SyntaxToken indexType)
        {
            Name = name;
            NameToken = nameToken;
            TypeName = typeName;
            TypeToken = typeToken;
            Alias = alias;
            AliasToken = aliasToken;
            IsIndexed = isIndexed;
            IndexName = indexName;
        }

        public string Name { get; private set; }
        public SyntaxToken NameToken { get; private set; }
        public string TypeName { get; private set; }
        public SyntaxToken TypeToken { get; private set; }
        public string Alias { get; private set; }
        public SyntaxToken AliasToken { get; private set; }
        public bool IsIndexed { get; private set; }
        public string IndexName { get; private set; }
        public SyntaxToken IndexType { get; private set; }

        public VariableSyntax WithType(SyntaxToken token)
        {
            return new VariableSyntax(Name, NameToken, token.ToString(), token, Alias, AliasToken, IsIndexed, IndexName, IndexType);
        }

        public VariableSyntax WithIndexType(SyntaxToken token)
        {
            return new VariableSyntax(Name, NameToken, TypeName, TypeToken, Alias, AliasToken, IsIndexed, IndexName, token);
        }
    }
}
