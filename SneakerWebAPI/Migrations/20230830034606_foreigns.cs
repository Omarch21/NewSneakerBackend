using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SneakerWebAPI.Migrations
{
    /// <inheritdoc />
    public partial class foreigns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_SnkrPrices_SneakerId",
                table: "SnkrPrices",
                column: "SneakerId");

            migrationBuilder.AddForeignKey(
                name: "FK_SnkrPrices_Sneakers_SneakerId",
                table: "SnkrPrices",
                column: "SneakerId",
                principalTable: "Sneakers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SnkrPrices_Sneakers_SneakerId",
                table: "SnkrPrices");

            migrationBuilder.DropIndex(
                name: "IX_SnkrPrices_SneakerId",
                table: "SnkrPrices");
        }
    }
}
