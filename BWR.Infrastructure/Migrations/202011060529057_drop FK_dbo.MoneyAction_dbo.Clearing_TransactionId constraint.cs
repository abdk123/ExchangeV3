namespace BWR.Infrastructure.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class dropFK_dboMoneyAction_dboClearing_TransactionIdconstraint : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.MoneyAction", "FK_dbo.MoneyAction_dbo.Clearing_TransactionId");
        }
        
        public override void Down()
        {
        }
    }
}
