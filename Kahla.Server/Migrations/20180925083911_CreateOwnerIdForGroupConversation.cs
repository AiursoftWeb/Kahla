using Microsoft.EntityFrameworkCore.Migrations;

namespace Kahla.Server.Migrations
{
    public partial class CreateOwnerIdForGroupConversation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OwnerId",
                table: "Conversations",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_OwnerId",
                table: "Conversations",
                column: "OwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Conversations_AspNetUsers_OwnerId",
                table: "Conversations",
                column: "OwnerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Conversations_AspNetUsers_OwnerId",
                table: "Conversations");

            migrationBuilder.DropIndex(
                name: "IX_Conversations_OwnerId",
                table: "Conversations");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "Conversations");
        }
    }
}
