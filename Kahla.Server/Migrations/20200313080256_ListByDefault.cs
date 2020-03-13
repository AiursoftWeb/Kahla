using Microsoft.EntityFrameworkCore.Migrations;

namespace Kahla.Server.Migrations
{
    public partial class ListByDefault : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("update AspNetUsers set ListInSearchResult = 1");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
