using Microsoft.EntityFrameworkCore.Migrations;

namespace Kahla.Server.Migrations
{
    public partial class AddConversationIdToFileRecord : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ConversationId",
                table: "FileRecords",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_FileRecords_ConversationId",
                table: "FileRecords",
                column: "ConversationId");

            migrationBuilder.AddForeignKey(
                name: "FK_FileRecords_Conversations_ConversationId",
                table: "FileRecords",
                column: "ConversationId",
                principalTable: "Conversations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FileRecords_Conversations_ConversationId",
                table: "FileRecords");

            migrationBuilder.DropIndex(
                name: "IX_FileRecords_ConversationId",
                table: "FileRecords");

            migrationBuilder.DropColumn(
                name: "ConversationId",
                table: "FileRecords");
        }
    }
}
