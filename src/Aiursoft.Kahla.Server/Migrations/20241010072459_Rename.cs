using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aiursoft.Kahla.Server.Migrations
{
    /// <inheritdoc />
    public partial class Rename : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ContactRecord_AspNetUsers_CreatorId",
                table: "ContactRecord");

            migrationBuilder.DropForeignKey(
                name: "FK_ContactRecord_AspNetUsers_TargetId",
                table: "ContactRecord");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ContactRecord",
                table: "ContactRecord");

            migrationBuilder.RenameTable(
                name: "ContactRecord",
                newName: "ContactRecords");

            migrationBuilder.RenameColumn(
                name: "CreateTime",
                table: "ContactRecords",
                newName: "AddTime");

            migrationBuilder.RenameIndex(
                name: "IX_ContactRecord_TargetId",
                table: "ContactRecords",
                newName: "IX_ContactRecords_TargetId");

            migrationBuilder.RenameIndex(
                name: "IX_ContactRecord_CreatorId",
                table: "ContactRecords",
                newName: "IX_ContactRecords_CreatorId");

            migrationBuilder.AlterColumn<string>(
                name: "NickName",
                table: "AspNetUsers",
                type: "TEXT",
                maxLength: 40,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 40,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "AspNetUsers",
                type: "TEXT",
                maxLength: 256,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 256,
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ContactRecords",
                table: "ContactRecords",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ContactRecords_AspNetUsers_CreatorId",
                table: "ContactRecords",
                column: "CreatorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ContactRecords_AspNetUsers_TargetId",
                table: "ContactRecords",
                column: "TargetId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ContactRecords_AspNetUsers_CreatorId",
                table: "ContactRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_ContactRecords_AspNetUsers_TargetId",
                table: "ContactRecords");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ContactRecords",
                table: "ContactRecords");

            migrationBuilder.RenameTable(
                name: "ContactRecords",
                newName: "ContactRecord");

            migrationBuilder.RenameColumn(
                name: "AddTime",
                table: "ContactRecord",
                newName: "CreateTime");

            migrationBuilder.RenameIndex(
                name: "IX_ContactRecords_TargetId",
                table: "ContactRecord",
                newName: "IX_ContactRecord_TargetId");

            migrationBuilder.RenameIndex(
                name: "IX_ContactRecords_CreatorId",
                table: "ContactRecord",
                newName: "IX_ContactRecord_CreatorId");

            migrationBuilder.AlterColumn<string>(
                name: "NickName",
                table: "AspNetUsers",
                type: "TEXT",
                maxLength: 40,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 40);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "AspNetUsers",
                type: "TEXT",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 256);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ContactRecord",
                table: "ContactRecord",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ContactRecord_AspNetUsers_CreatorId",
                table: "ContactRecord",
                column: "CreatorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ContactRecord_AspNetUsers_TargetId",
                table: "ContactRecord",
                column: "TargetId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
