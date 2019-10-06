using Microsoft.EntityFrameworkCore.Migrations;

namespace Kahla.Server.Migrations
{
    public partial class DefaultEnterToSend : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("update AspNetUsers set EnableEnterToSendMessage = 1");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
