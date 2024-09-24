using Microsoft.EntityFrameworkCore.Migrations;

namespace Kahla.Server.Migrations
{
    public partial class ListInSearchResult : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("Update conversations set ListInSearchResult='1'");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
