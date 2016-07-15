using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Settings.Model
{
    public class HeaderModel : SettingsModel
    {
        public HeaderModel()
        {
            Values = new List<AssignmentExpressionSyntax>();
        }

        public string Name { get; set; }
        public List<AssignmentExpressionSyntax> Values { get; private set; }
    }
}
