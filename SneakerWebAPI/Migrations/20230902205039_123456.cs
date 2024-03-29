using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SneakerWebAPI.Migrations
{
    /// <inheritdoc />
    public partial class _123456 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Sneakers_UserID",
                table: "Sneakers",
                column: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK_Sneakers_users_UserID",
                table: "Sneakers",
                column: "UserID",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sneakers_users_UserID",
                table: "Sneakers");

            migrationBuilder.DropIndex(
                name: "IX_Sneakers_UserID",
                table: "Sneakers");
        }
    }
}
