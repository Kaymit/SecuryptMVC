namespace SecuryptMVC.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class test : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.EncryptedItems",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        IsPrivate = c.Boolean(nullable: false),
                        Name = c.String(nullable: false),
                        PublicKey = c.String(nullable: false),
                        StorageLocation = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.EncryptedItems");
        }
    }
}
