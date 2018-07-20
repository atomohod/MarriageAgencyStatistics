namespace MarriageAgencyStatistics.DataAccess.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UserChatStatistics : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.UserChats",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Date = c.DateTime(nullable: false),
                        ChatInvatationsCount = c.Int(nullable: false),
                        User_ID = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.User_ID)
                .Index(t => t.User_ID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.UserChats", "User_ID", "dbo.Users");
            DropIndex("dbo.UserChats", new[] { "User_ID" });
            DropTable("dbo.UserChats");
        }
    }
}
