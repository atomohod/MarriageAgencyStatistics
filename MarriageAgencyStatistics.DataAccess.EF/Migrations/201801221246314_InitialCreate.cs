namespace MarriageAgencyStatistics.DataAccess.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Users",
                c => new
                    {
                        ID = c.String(nullable: false, maxLength: 128),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.UserOnlines",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        IsOnline = c.Boolean(nullable: false),
                        Online = c.Long(nullable: false),
                        User_ID = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.User_ID)
                .Index(t => t.User_ID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.UserOnlines", "User_ID", "dbo.Users");
            DropIndex("dbo.UserOnlines", new[] { "User_ID" });
            DropTable("dbo.UserOnlines");
            DropTable("dbo.Users");
        }
    }
}
