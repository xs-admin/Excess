namespace Excess.Web.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddingProjects : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ProjectFiles",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Contents = c.String(),
                        OwnerProject = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.Projects",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        ProjectType = c.String(),
                        IsSample = c.Boolean(nullable: false),
                        UserID = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Projects");
            DropTable("dbo.ProjectFiles");
        }
    }
}
