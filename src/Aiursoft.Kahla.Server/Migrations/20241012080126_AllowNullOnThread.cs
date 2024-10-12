using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aiursoft.Kahla.Server.Migrations
{
    /// <inheritdoc />
    public partial class AllowNullOnThread : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChatThreads_UserThreadRelations_OwnerRelationId",
                table: "ChatThreads");

            migrationBuilder.AlterColumn<int>(
                name: "OwnerRelationId",
                table: "ChatThreads",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_ChatThreads_UserThreadRelations_OwnerRelationId",
                table: "ChatThreads",
                column: "OwnerRelationId",
                principalTable: "UserThreadRelations",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChatThreads_UserThreadRelations_OwnerRelationId",
                table: "ChatThreads");

            migrationBuilder.AlterColumn<int>(
                name: "OwnerRelationId",
                table: "ChatThreads",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ChatThreads_UserThreadRelations_OwnerRelationId",
                table: "ChatThreads",
                column: "OwnerRelationId",
                principalTable: "UserThreadRelations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
