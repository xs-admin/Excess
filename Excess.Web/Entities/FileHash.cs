using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Excess.Web.Entities
{
    public class FileHash
    {
        public int ID { get; set; }
        public int FileID { get; set; }
        public int Hash { get; set; }
    }
}