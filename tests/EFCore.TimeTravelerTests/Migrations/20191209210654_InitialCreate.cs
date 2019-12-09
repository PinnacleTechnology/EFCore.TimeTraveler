using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EFCore.TimeTravelerTests.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Apples",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    FruitStatus = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Apples", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Apples");
        }
    }
}
