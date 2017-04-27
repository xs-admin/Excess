using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.SetParser
{
    public class SetSyntax
    {
        public SetSyntax(VariableSyntax[] variables, ConstructorSyntax constructor)
        {
            Variables = variables;
            Constructor = constructor;
        }

        public VariableSyntax[] Variables { get; private set; }
        public ConstructorSyntax Constructor { get; private set; }
    }
}
