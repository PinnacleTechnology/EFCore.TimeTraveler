using Microsoft.EntityFrameworkCore.Migrations;

namespace EFCore.TimeTravelerTests.Migrations
{
    public partial class TemporalApples : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddAsTemporalTable(nameof(Apple));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
