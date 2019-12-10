using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EFCore.TimeTravelerTests.Migrations
{
    public partial class Initialize : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Apple",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    FruitStatus = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Apple", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Worm",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    AppleId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Worm", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Worm_Apple_AppleId",
                        column: x => x.AppleId,
                        principalTable: "Apple",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Worm_AppleId",
                table: "Worm",
                column: "AppleId");

            migrationBuilder.Sql(
                @"ALTER TABLE dbo.Apple
ADD StartTime DATETIME2 GENERATED ALWAYS AS ROW START
   HIDDEN NOT NULL, 
 EndTime   DATETIME2 GENERATED ALWAYS AS ROW END
   HIDDEN  NOT NULL ,
 PERIOD FOR SYSTEM_TIME (StartTime, EndTime);

ALTER TABLE dbo.Apple
SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE=dbo.AppleHistory))");

            migrationBuilder.Sql(
                @"ALTER TABLE dbo.Worm
ADD StartTime DATETIME2 GENERATED ALWAYS AS ROW START
   HIDDEN NOT NULL, 
 EndTime   DATETIME2 GENERATED ALWAYS AS ROW END
   HIDDEN  NOT NULL ,
 PERIOD FOR SYSTEM_TIME (StartTime, EndTime);

ALTER TABLE dbo.Worm
SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE=dbo.WormHistory))");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Worm");

            migrationBuilder.DropTable(
                name: "Apple");
        }
    }
}
