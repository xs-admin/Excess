using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests.Mocks
{
    public class MockIdentGrammarModel
    {
    }

    public class RootModel : MockIdentGrammarModel
    {
        public RootModel()
        {
            Headers = new List<HeaderModel>();
        }

        public List<HeaderModel> Headers { get; private set; }
    }

    public class HeaderModel : MockIdentGrammarModel
    {
        public HeaderModel()
        {
            Values = new List<AssignmentExpressionSyntax>();
        }

        public string Name { get; set; }
        public List<AssignmentExpressionSyntax> Values { get; private set; }
    }

    public class HeaderValueModel : MockIdentGrammarModel
    {
    }

    public class RazorModel : MockIdentGrammarModel
    {
        public string Name { get; set; }
        public string Telephone { get; set; }
    }
}
