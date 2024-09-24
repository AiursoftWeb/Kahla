using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aiursoft.Kahla.Server.Migrations
{
    /// <inheritdoc />
    public partial class RenameDeviceOwner : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Devices_AspNetUsers_UserId",
                table: "Devices");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Devices",
                newName: "OwnerId");

            migrationBuilder.RenameIndex(
                name: "IX_Devices_UserId",
                table: "Devices",
                newName: "IX_Devices_OwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Devices_AspNetUsers_OwnerId",
                table: "Devices",
                column: "OwnerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Devices_AspNetUsers_OwnerId",
                table: "Devices");

            migrationBuilder.RenameColumn(
                name: "OwnerId",
                table: "Devices",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Devices_OwnerId",
                table: "Devices",
                newName: "IX_Devices_UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Devices_AspNetUsers_UserId",
                table: "Devices",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
