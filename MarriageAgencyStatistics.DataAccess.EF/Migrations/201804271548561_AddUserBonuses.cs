namespace MarriageAgencyStatistics.DataAccess.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddUserBonuses : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.UserBonuses",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Date = c.DateTime(nullable: false),
                        Bonuses = c.Binary(),
                        User_ID = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.User_ID)
                .Index(t => t.User_ID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.UserBonuses", "User_ID", "dbo.Users");
            DropIndex("dbo.UserBonuses", new[] { "User_ID" });
            DropTable("dbo.UserBonuses");
        }
    }
}
