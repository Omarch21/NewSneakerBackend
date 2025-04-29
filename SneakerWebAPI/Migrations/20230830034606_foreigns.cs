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
                name: "IX_SneakerPrices_SneakerId",
                table: "SneakerPrices",
                column: "SneakerId");

            migrationBuilder.AddForeignKey(
                name: "FK_SneakerPrices_Sneakers_SneakerId",
                table: "SneakerPrices",
                column: "SneakerId",
                principalTable: "Sneakers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SneakerPrices_Sneakers_SneakerId",
                table: "SneakerPrices");

            migrationBuilder.DropIndex(
                name: "IX_SneakerPrices_SneakerId",
                table: "SneakerPrices");
        }
    }
}
