using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace Excess.Web.Entities
{
    public class ExcessDbContext : DbContext
    {
        public ExcessDbContext() : 
            base("name=Default")
        {
        }

        public System.Data.Entity.DbSet<TranslationSample> Samples { get; set; }
    }
}