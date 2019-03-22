using Microsoft.EntityFrameworkCore.Migrations;

namespace Kahla.Server.Migrations
{
    public partial class AddForeignKeyToDevice : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "UserID",
                table: "Devices",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Devices_UserID",
                table: "Devices",
                column: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK_Devices_AspNetUsers_UserID",
                table: "Devices",
                column: "UserID",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Devices_AspNetUsers_UserID",
                table: "Devices");

            migrationBuilder.DropIndex(
                name: "IX_Devices_UserID",
                table: "Devices");

            migrationBuilder.AlterColumn<string>(
                name: "UserID",
                table: "Devices",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);
        }
    }
}
