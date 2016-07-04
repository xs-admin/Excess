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
            Statements = new List<ForModel>();
        }

        public List<HeaderModel> Headers { get; private set; }
        public List<ForModel> Statements { get; private set; }
    }

    public class HeaderModel : MockIdentGrammarModel
    {
        public HeaderModel()
        {
            Values = new List<AssignmentExpressionSyntax>();
            Contacts = new List<ContactModel>();
        }

        public string Name { get; set; }
        public List<AssignmentExpressionSyntax> Values { get; private set; }
        public List<ContactModel> Contacts { get; private set; }
    }

    public class HeaderValueModel : MockIdentGrammarModel
    {
    }

    public class ContactModel : MockIdentGrammarModel
    {
        public ContactModel()
        {
            OtherNumbers = new List<TelephoneModel>();
        }

        public string Name { get; set; }
        public string Telephone { get; set; }
        public List<TelephoneModel> OtherNumbers { get; private set; }
    }

    public class TelephoneModel : MockIdentGrammarModel
    {
        public int AreaCode { get; set; }
        public int FirstThree { get; set; }
        public int LastFour { get; set; }
    }

    public class ForModel : MockIdentGrammarModel
    {
        public ForModel()
        {
            Statements = new List<StatementSyntax>();
        }

        public string Iterator { get; set; }
        public string Iterable { get; set; }
        public List<StatementSyntax> Statements { get; private set; }
    }
}
