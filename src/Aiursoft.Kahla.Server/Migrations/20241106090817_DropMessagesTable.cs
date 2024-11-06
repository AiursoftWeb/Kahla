using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aiursoft.Kahla.Server.Migrations
{
    /// <inheritdoc />
    public partial class DropMessagesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Messages");

            migrationBuilder.DropColumn(
                name: "ReadTimeStamp",
                table: "UserThreadRelations");

            migrationBuilder.AddColumn<int>(
                name: "ReadMessageIndex",
                table: "UserThreadRelations",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReadMessageIndex",
                table: "UserThreadRelations");

            migrationBuilder.AddColumn<DateTime>(
                name: "ReadTimeStamp",
                table: "UserThreadRelations",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateTable(
                name: "Messages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Content = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MessageId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    SendTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    SenderId = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ThreadId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Messages", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
