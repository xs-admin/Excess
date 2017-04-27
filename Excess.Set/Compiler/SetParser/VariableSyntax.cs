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
            string indexName)
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
    }
}
