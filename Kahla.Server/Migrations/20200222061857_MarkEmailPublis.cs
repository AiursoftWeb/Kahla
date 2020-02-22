using Microsoft.EntityFrameworkCore.Migrations;

namespace Kahla.Server.Migrations
{
    public partial class MarkEmailPublis : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MakeEmailPublic",
                table: "AspNetUsers",
                newName: "MarkEmailPublic");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MarkEmailPublic",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<bool>(
                name: "MakeEmailPublic",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
