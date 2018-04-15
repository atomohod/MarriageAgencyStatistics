namespace MarriageAgencyStatistics.DataAccess.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddUsersEmails : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.UserEmails",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Date = c.DateTime(nullable: false),
                        Emails = c.Binary(),
                        User_ID = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.User_ID)
                .Index(t => t.User_ID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.UserEmails", "User_ID", "dbo.Users");
            DropIndex("dbo.UserEmails", new[] { "User_ID" });
            DropTable("dbo.UserEmails");
        }
    }
}
