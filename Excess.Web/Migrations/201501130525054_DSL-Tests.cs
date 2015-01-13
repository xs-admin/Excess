namespace Excess.Web.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DSLTests : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.DSLTests",
                c => new
                    {
                        ID = c.Guid(nullable: false),
                        ProjectID = c.Int(nullable: false),
                        Caption = c.String(),
                        Contents = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.DSLTests");
        }
    }
}
