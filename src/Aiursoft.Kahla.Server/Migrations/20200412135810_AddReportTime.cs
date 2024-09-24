using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace Kahla.Server.Migrations
{
    public partial class AddReportTime : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ReportTime",
                table: "Reports",
                nullable: false,
                defaultValue: new DateTime(2000, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReportTime",
                table: "Reports");
        }
    }
}
