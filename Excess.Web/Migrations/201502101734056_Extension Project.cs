namespace Excess.Web.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ExtensionProject : DbMigration
    {
        public override void Up()
        {
            DropTable("dbo.DSLProjects");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.DSLProjects",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        ProjectID = c.Int(nullable: false),
                        Name = c.String(),
                        ParserKind = c.String(),
                        LinkerKind = c.String(),
                        ExtendsNamespaces = c.Boolean(nullable: false),
                        ExtendsTypes = c.Boolean(nullable: false),
                        ExtendsMembers = c.Boolean(nullable: false),
                        ExtendsCode = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
        }
    }
}
