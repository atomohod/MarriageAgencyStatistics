namespace MarriageAgencyStatistics.DataAccess.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddSelectedUsers : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.SelectedUsers",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        User_ID = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.User_ID)
                .Index(t => t.User_ID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.SelectedUsers", "User_ID", "dbo.Users");
            DropIndex("dbo.SelectedUsers", new[] { "User_ID" });
            DropTable("dbo.SelectedUsers");
        }
    }
}
