using Microsoft.EntityFrameworkCore.Migrations;

namespace Kahla.Server.Migrations
{
    public partial class RenameUserIdInDevice : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Devices_AspNetUsers_UserID",
                table: "Devices");

            migrationBuilder.RenameColumn(
                name: "UserID",
                table: "Devices",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Devices_UserID",
                table: "Devices",
                newName: "IX_Devices_UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Devices_AspNetUsers_UserId",
                table: "Devices",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Devices_AspNetUsers_UserId",
                table: "Devices");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Devices",
                newName: "UserID");

            migrationBuilder.RenameIndex(
                name: "IX_Devices_UserId",
                table: "Devices",
                newName: "IX_Devices_UserID");

            migrationBuilder.AddForeignKey(
                name: "FK_Devices_AspNetUsers_UserID",
                table: "Devices",
                column: "UserID",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
