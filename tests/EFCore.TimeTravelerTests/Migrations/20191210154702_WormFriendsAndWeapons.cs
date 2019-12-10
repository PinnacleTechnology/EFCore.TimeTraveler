using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EFCore.TimeTravelerTests.Migrations
{
    public partial class WormFriendsAndWeapons : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WormFriendship",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WormAId = table.Column<Guid>(nullable: false),
                    WormBId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WormFriendship", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WormFriendship_Worm_WormAId",
                        column: x => x.WormAId,
                        principalTable: "Worm",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WormFriendship_Worm_WormBId",
                        column: x => x.WormBId,
                        principalTable: "Worm",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WormWeapon",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    WormId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WormWeapon", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WormWeapon_Worm_WormId",
                        column: x => x.WormId,
                        principalTable: "Worm",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WormFriendship_WormAId",
                table: "WormFriendship",
                column: "WormAId");

            migrationBuilder.CreateIndex(
                name: "IX_WormFriendship_WormBId",
                table: "WormFriendship",
                column: "WormBId");

            migrationBuilder.CreateIndex(
                name: "IX_WormWeapon_WormId",
                table: "WormWeapon",
                column: "WormId");

            migrationBuilder.Sql(
                @"ALTER TABLE dbo.WormFriendship
ADD StartTime DATETIME2 GENERATED ALWAYS AS ROW START
   HIDDEN NOT NULL, 
 EndTime   DATETIME2 GENERATED ALWAYS AS ROW END
   HIDDEN  NOT NULL ,
 PERIOD FOR SYSTEM_TIME (StartTime, EndTime);

ALTER TABLE dbo.WormFriendship
SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE=dbo.WormFriendshipHistory))");

            migrationBuilder.Sql(
                @"ALTER TABLE dbo.WormWeapon
ADD StartTime DATETIME2 GENERATED ALWAYS AS ROW START
   HIDDEN NOT NULL, 
 EndTime   DATETIME2 GENERATED ALWAYS AS ROW END
   HIDDEN  NOT NULL ,
 PERIOD FOR SYSTEM_TIME (StartTime, EndTime);

ALTER TABLE dbo.WormWeapon
SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE=dbo.WormWeaponHistory))");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WormFriendship");

            migrationBuilder.DropTable(
                name: "WormWeapon");
        }
    }
}
