namespace BWR.Infrastructure.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Add_Box_Action_Type : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.BoxAction", "BoxActionType", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.BoxAction", "BoxActionType");
        }
    }
}
