using Microsoft.EntityFrameworkCore.Migrations;

namespace Kahla.Server.Migrations
{
    public partial class AddMessageLiveTimeForConversation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaxLiveSeconds",
                table: "Conversations",
                nullable: false,
                defaultValue: int.MaxValue);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxLiveSeconds",
                table: "Conversations");
        }
    }
}
