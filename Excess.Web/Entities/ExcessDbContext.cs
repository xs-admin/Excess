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
        public System.Data.Entity.DbSet<Project> Projects { get; set; }
        public System.Data.Entity.DbSet<ProjectFile> ProjectFiles { get; set; }
        public System.Data.Entity.DbSet<DSLProject> DSLProjects { get; set; }
        public System.Data.Entity.DbSet<DSLTest> DSLTests { get; set; }
    }
}