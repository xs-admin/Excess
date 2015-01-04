namespace Excess.Web.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ReseedingProjects : DbMigration
    {
        public override void Up()
        {
            Sql("DBCC CHECKIDENT (Projects, RESEED, 999)");
            Sql("DBCC CHECKIDENT (ProjectFiles, RESEED, 999)");
        }

        public override void Down()
        {
        }
    }
}
