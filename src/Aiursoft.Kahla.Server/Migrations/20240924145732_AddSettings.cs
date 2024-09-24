using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aiursoft.Kahla.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PreferedLanguage",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "MarkEmailPublic",
                table: "AspNetUsers",
                newName: "ThemeId");

            migrationBuilder.AddColumn<bool>(
                name: "EnableEnterToSendMessage",
                table: "AspNetUsers",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "EnableHideMyOnlineStatus",
                table: "AspNetUsers",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EnableEnterToSendMessage",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "EnableHideMyOnlineStatus",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "ThemeId",
                table: "AspNetUsers",
                newName: "MarkEmailPublic");

            migrationBuilder.AddColumn<string>(
                name: "PreferedLanguage",
                table: "AspNetUsers",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }
    }
}
