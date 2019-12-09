using Microsoft.EntityFrameworkCore.Migrations;

namespace EFCore.TimeTravelerTests.Migrations
{
    // FROM: https://github.com/cpoDesign/EFCore.TemporalSupport
    /// <summary>
    /// EFCore Temporal Support, based on: https://blog.bennymichielsen.be/2017/11/07/auditing-with-ef-core-and-sql-server-part-1/
    /// </summary>
    public static class EnableTemporalDataOnTable
    {
        public static void AddAsTemporalTable(this MigrationBuilder migrationBuilder, string tableName, string temporalSchema, string temporalTableName)
        {
            var schemaName = "dbo";
            migrationBuilder.Sql($@"
                    IF NOT EXISTS (SELECT * FROM sys.[tables] t INNER JOIN sys.schemas s ON s.schema_id = t.schema_id WHERE t.name = '{tableName}' AND temporal_type = 2 and s.name = '{schemaName}')
                    BEGIN
                        ALTER TABLE [{schemaName}].[{tableName}]   
                        ADD  ValidFrom datetime2 (0) GENERATED ALWAYS AS ROW START HIDDEN constraint DF_{tableName}_ValidFrom DEFAULT DATEADD(second, -1, SYSUTCDATETIME())  
                            , ValidTo datetime2 (0)  GENERATED ALWAYS AS ROW END HIDDEN constraint DF_{tableName}_ValidTo DEFAULT '9999.12.31 23:59:59.99'  
                            , PERIOD FOR SYSTEM_TIME (ValidFrom, ValidTo);   
 
                        ALTER TABLE [{schemaName}].[{tableName}]
                        SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [{temporalSchema}].[{temporalTableName}])); 
                    END
                ");
        }

        public static void AddAsTemporalTable(this MigrationBuilder migrationBuilder, string tableName, string temporalSchema = null)
        {
            var temporalTableName = $"{tableName}History";
            temporalSchema ??= "dbo";
            AddAsTemporalTable(migrationBuilder, tableName, temporalSchema, temporalTableName);
        }
    }
}
