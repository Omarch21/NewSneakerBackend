using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SneakerWebAPI.Migrations
{
    public partial class changetablename : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "SnkrPrices",  // Old table name
                newName: "SneakerPrices"  // New table name
            );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "SneakerPrices",  // New table name
                newName: "SnkrPrices"  // Revert to old table name
            );
        }
    }
}