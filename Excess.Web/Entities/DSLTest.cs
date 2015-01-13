using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Excess.Web.Entities
{
    public class DSLTest
    {
        public Guid ID { get; set; }

        public int ProjectID { get; set; }
        public string Caption { get; set; }
        public string Contents { get; set; }
    }
}