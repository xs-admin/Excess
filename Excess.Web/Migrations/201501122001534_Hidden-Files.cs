namespace Excess.Web.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class HiddenFiles : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ProjectFiles", "isHidden", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ProjectFiles", "isHidden");
        }
    }
}
