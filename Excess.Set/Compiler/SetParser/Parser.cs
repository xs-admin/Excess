using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.SetParser
{
    public class Parser
    {
        public static SetSyntax Parse(string text)
        {
            throw new NotImplementedException();
        }

        public static SetSyntax Parse(IEnumerable<SyntaxToken> tokens)
        {
            var tokenArray = tokens.ToArray();
            var index = 0;

            var variables = new List<VariableSyntax>();
            var consumed = 0;
            if (!parseVariables(tokenArray, index, out consumed, variables))
                return null; //td: errors

            index += consumed;
            var constructor = parseConstructor(tokenArray, index, out consumed);
            if (constructor == null)
                return null; //td: errors

            return new SetSyntax(variables.ToArray(), constructor);
        }

        private static bool parseVariables(SyntaxToken[] tokens, int index, out int consumed, List<VariableSyntax> result)
        {
            consumed = 0;
            var startIndex = index;
            for (;;)
            {
                if (index >= tokens.Length)
                    return false; //td: error, unexpected en of file

                var iconsumed = 0;
                var token = tokens[index];
                var finished = false;
                if (!parseVariableSeparator(token, out iconsumed, out finished))
                    return false;

                index += iconsumed;
                if (finished)
                    break;

                var vs = null as VariableSyntax;
                if (!parseVariable(tokens, index, out iconsumed, out vs))
                    return false;

                index += iconsumed;
                result.Add(vs);
            }

            consumed = index - startIndex;
            return true;
        }

        private static bool parseVariableSeparator(SyntaxToken token, out int consumed, out bool finished)
        {
            consumed = 0;
            finished = false;
            switch ((SyntaxKind)token.RawKind)
            {
                case SyntaxKind.BarToken:
                    consumed = 1;
                    finished = true;
                    break;
                case SyntaxKind.CommaToken:
                    consumed = 1;
                    break;
                case SyntaxKind.IdentifierToken:
                    break;
                default:
                    return false; //td: error
            }

            return true;
        }

        private static bool parseVariable(SyntaxToken[] tokens, int index, out int consumed, out VariableSyntax variable)
        {
            consumed = 0;
            variable = null;
            var token = tokens[index];
            if (token.RawKind != (int)SyntaxKind.IdentifierToken)
                return false; //td: error, variable name expected

            //variable: name x, or xi for indexing, x as name for alias 
            string varName;
            bool isIndexed;
            string indexName;
            if (!parseVariableName(token, out varName, out isIndexed, out indexName))
                return false;

            consumed++;

            int iconsumed;
            string alias;
            SyntaxToken aliasToken;
            if (!parseAlias(tokens, index + consumed, out alias, out aliasToken, out iconsumed))
                return false;

            consumed += iconsumed;

            //type
            SyntaxToken typeToken;
            string typeName;
            if (!parseType(tokens, index + consumed, out typeToken, out typeName, out iconsumed))
                return false;

            consumed += iconsumed;

            //we golden
            variable = new VariableSyntax(varName, token, typeName, typeToken, alias, aliasToken, isIndexed, indexName);
            return true;
        }

        private static bool parseVariableName(SyntaxToken token, out string varName, out bool isIndexed, out string indexName)
        {
            varName = null;
            isIndexed = false;
            indexName = null;
            varName = token.ToString();
            if (varName.Length > 2)
                return false; //td: error

            if (!varName.All(c => char.IsLower(c)))
                return false; //td: error

            isIndexed = varName.Length == 2; //td: !!! multiple indices
            indexName = isIndexed? varName[1].ToString() : string.Empty;

            return true;
        }

        //false means "there was an error parsing" where as true can be "there is no alias"
        //with consumed == 0
        private static bool parseAlias(SyntaxToken[] tokens, int index, out string alias, out SyntaxToken token, out int consumed)
        {
            consumed = 0;
            alias = string.Empty;
            token = default(SyntaxToken);
            if (!tokens[index].IsKind(SyntaxKind.AsKeyword))
                return true;

            if (index + 1 >= tokens.Length)
                return false; //td: error, eof

            token = tokens[index + 1];
            if (!token.IsKind(SyntaxKind.IdentifierToken))
                return false; //td: error, identifier expected

            consumed = 2;
            alias = token.ToString();
            return true;
        }

        private static bool parseType(SyntaxToken[] tokens, int index, out SyntaxToken token, out string name, out int consumed)
        {
            consumed = 0;
            name = string.Empty;
            token = tokens[index];
            if (!token.IsKind(SyntaxKind.AsKeyword) && !token.IsKind(SyntaxKind.ColonToken))
                return true;

            if (index + 1 >= tokens.Length)
                return false; //td: error, eof

            token = tokens[index + 1];
            if (!token.IsKind(SyntaxKind.IdentifierToken))
                return false; //td: error, identifier expected

            consumed = 2;
            name = token.ToString();
            return true;
        }

        private static ConstructorSyntax parseConstructor(SyntaxToken[] tokenArray, int index, out int consumed)
        {
            throw new NotImplementedException();
        }
    }
}
