using Microsoft.EntityFrameworkCore.Migrations;

namespace Kahla.Server.Migrations
{
    public partial class AddPropertForConditionalEmail : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "MakeEmailPublic",
                table: "AspNetUsers",
                nullable: false,
                defaultValue: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MakeEmailPublic",
                table: "AspNetUsers");
        }
    }
}
