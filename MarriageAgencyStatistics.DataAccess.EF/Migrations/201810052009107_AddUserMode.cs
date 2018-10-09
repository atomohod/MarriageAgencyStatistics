namespace MarriageAgencyStatistics.DataAccess.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddUserMode : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Users", "UserMode", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Users", "UserMode");
        }
    }
}
