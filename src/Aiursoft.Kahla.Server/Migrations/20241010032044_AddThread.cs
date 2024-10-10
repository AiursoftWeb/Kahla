using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aiursoft.Kahla.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddThread : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ListInSearchResult",
                table: "AspNetUsers",
                newName: "AllowSearchByName");

            migrationBuilder.CreateTable(
                name: "ChatThreads",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    IconFilePath = table.Column<string>(type: "TEXT", maxLength: 512, nullable: false),
                    OwnerRelationId = table.Column<int>(type: "INTEGER", nullable: false),
                    AllowDirectJoinWithoutInvitation = table.Column<bool>(type: "INTEGER", nullable: false),
                    AllowSearchByName = table.Column<bool>(type: "INTEGER", nullable: false),
                    AllowMemberSoftInvitation = table.Column<bool>(type: "INTEGER", nullable: false),
                    AllowMembersSendMessages = table.Column<bool>(type: "INTEGER", nullable: false),
                    AllowMembersEnlistAllMembers = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatThreads", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserThreadRelations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    JoinTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Role = table.Column<int>(type: "INTEGER", nullable: false),
                    Muted = table.Column<bool>(type: "INTEGER", nullable: false),
                    Baned = table.Column<bool>(type: "INTEGER", nullable: false),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    ThreadId = table.Column<int>(type: "INTEGER", nullable: false),
                    ReadTimeStamp = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserThreadRelations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserThreadRelations_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserThreadRelations_ChatThreads_ThreadId",
                        column: x => x.ThreadId,
                        principalTable: "ChatThreads",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChatThreads_OwnerRelationId",
                table: "ChatThreads",
                column: "OwnerRelationId");

            migrationBuilder.CreateIndex(
                name: "IX_UserThreadRelations_ThreadId",
                table: "UserThreadRelations",
                column: "ThreadId");

            migrationBuilder.CreateIndex(
                name: "IX_UserThreadRelations_UserId",
                table: "UserThreadRelations",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ChatThreads_UserThreadRelations_OwnerRelationId",
                table: "ChatThreads",
                column: "OwnerRelationId",
                principalTable: "UserThreadRelations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChatThreads_UserThreadRelations_OwnerRelationId",
                table: "ChatThreads");

            migrationBuilder.DropTable(
                name: "UserThreadRelations");

            migrationBuilder.DropTable(
                name: "ChatThreads");

            migrationBuilder.RenameColumn(
                name: "AllowSearchByName",
                table: "AspNetUsers",
                newName: "ListInSearchResult");
        }
    }
}
