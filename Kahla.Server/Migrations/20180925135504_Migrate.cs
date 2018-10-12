using Microsoft.EntityFrameworkCore.Migrations;

namespace Kahla.Server.Migrations
{
    public partial class Migrate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GroupImage",
                table: "Conversations");

            migrationBuilder.DropColumn(
                name: "HeadImgUrl",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<int>(
                name: "GroupImageKey",
                table: "Conversations",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "HeadImgFileKey",
                table: "AspNetUsers",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GroupImageKey",
                table: "Conversations");

            migrationBuilder.DropColumn(
                name: "HeadImgFileKey",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<string>(
                name: "GroupImage",
                table: "Conversations",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HeadImgUrl",
                table: "AspNetUsers",
                nullable: true);
        }
    }
}
