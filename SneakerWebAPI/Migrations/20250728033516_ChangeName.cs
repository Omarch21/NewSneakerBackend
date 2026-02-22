using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SneakerWebAPI.Migrations
{
    /// <inheritdoc />
    public partial class ChangeName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sneakers_users_UserID",
                table: "Sneakers");

            migrationBuilder.RenameColumn(
                name: "UserID",
                table: "Sneakers",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Sneakers_UserID",
                table: "Sneakers",
                newName: "IX_Sneakers_UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Sneakers_users_UserId",
                table: "Sneakers",
                column: "UserId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sneakers_users_UserId",
                table: "Sneakers");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Sneakers",
                newName: "UserID");

            migrationBuilder.RenameIndex(
                name: "IX_Sneakers_UserId",
                table: "Sneakers",
                newName: "IX_Sneakers_UserID");

            migrationBuilder.AddForeignKey(
                name: "FK_Sneakers_users_UserID",
                table: "Sneakers",
                column: "UserID",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
