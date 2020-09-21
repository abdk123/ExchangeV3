namespace BWR.Infrastructure.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Add_Date_To_MoneyAction : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.MoneyAction", "Date", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.MoneyAction", "Date");
        }
    }
}
