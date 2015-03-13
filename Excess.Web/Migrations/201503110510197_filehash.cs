namespace Excess.Web.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class filehash : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.FileHashes",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        FileID = c.Int(nullable: false),
                        Hash = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.FileHashes");
        }
    }
}
