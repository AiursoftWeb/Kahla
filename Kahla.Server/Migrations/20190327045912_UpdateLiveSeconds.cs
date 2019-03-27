using Microsoft.EntityFrameworkCore.Migrations;

namespace Kahla.Server.Migrations
{
    public partial class UpdateLiveSeconds : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("update Conversations set MaxLiveSeconds=1576800000 where 1 = 1");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
