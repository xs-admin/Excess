using Excess.Compiler;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R
{
    public static class RScope
    {
        public static void InitR(this Scope scope)
        {
            scope.set("rPreStatements", new List<StatementSyntax>());
            scope.set("rVariables", new List<string>());
        }

        public static List<StatementSyntax> PreStatements(this Scope scope)
        {
            var result = scope.find<List<StatementSyntax>>("rPreStatements");
            Debug.Assert(result != null);
            return result;
        }

        public static bool hasVariable(this Scope scope, string varName)
        {
            var result = scope.find<List<string>>("rVariables");
            if (result != null && result.Contains(varName))
                return true;

            var parent = scope.parent();
            return parent == null ? false : parent.hasVariable(varName);
        }

        public static void addVariable(this Scope scope, string varName)
        {
            var result = scope.find<List<string>>("rVariables");
            Debug.Assert(result != null && !result.Contains(varName));
            result.Add(varName);
        }
    }
}
