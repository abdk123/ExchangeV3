namespace BWR.Infrastructure.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Create_sp_EmptyDB_Porc : DbMigration
    {
        public override void Up()
        {
            CreateStoredProcedure("sp_EmptyDB", "EXEC sp_MSForEachTable 'ALTER TABLE ? NOCHECK CONSTRAINT ALL' ; EXEC sp_MSForEachTable 'DELETE FROM ?'; EXEC sp_MSForEachTable 'ALTER TABLE ? WITH CHECK CHECK CONSTRAINT ALL';");
        }
        
        public override void Down()
        {
        }
    }
}
