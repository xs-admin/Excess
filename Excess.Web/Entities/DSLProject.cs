using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Excess.Web.Entities
{
    public class DSLProject
    {
        public int ID        { get; set; }
        public int ProjectID { get; set; }

        public string Name { get; set; }
        public string ParserKind { get; set; }
        public string LinkerKind { get; set; }
        public bool ExtendsNamespaces { get; set; }
        public bool ExtendsTypes { get; set; }
        public bool ExtendsMembers { get; set; }
        public bool ExtendsCode { get; set; }
    }
}