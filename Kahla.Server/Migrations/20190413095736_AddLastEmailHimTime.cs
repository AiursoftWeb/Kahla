using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace Kahla.Server.Migrations
{
    public partial class AddLastEmailHimTime : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastEmailHimTime",
                table: "AspNetUsers",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastEmailHimTime",
                table: "AspNetUsers");
        }
    }
}
