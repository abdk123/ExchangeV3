namespace BWR.Infrastructure.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Add_Shaded_Cloumn_For_ClientCahsFlow : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ClientCashFlow", "Shaded", c => c.Boolean(nullable:true));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ClientCashFlow", "Shaded");
        }
    }
}
