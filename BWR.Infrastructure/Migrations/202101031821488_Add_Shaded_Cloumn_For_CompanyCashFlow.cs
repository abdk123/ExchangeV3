namespace BWR.Infrastructure.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Add_Shaded_Cloumn_For_CompanyCashFlow : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.CompanyCashFlow", "Shaded", c => c.Boolean(true));
        }
        
        public override void Down()
        {
            DropColumn("dbo.CompanyCashFlow", "Shaded");
        }
    }
}
